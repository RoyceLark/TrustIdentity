using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Configuration;
using TrustIdentity.Core.Services;
namespace TrustIdentity.AspNetCore.Endpoints;

/// <summary>
/// Handlers for the OAuth 2.0 Token Endpoint
/// </summary>
public static class TokenEndpointHandlers
{
    /// <summary>
    /// Handles the token request asynchronously
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleTokenRequestAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("TokenEndpoint");
        var tokenService = context.RequestServices.GetRequiredService<ITokenService>();
        var validator = context.RequestServices.GetRequiredService<TrustIdentity.Core.Validation.TokenRequestValidator>();

        try
        {
            var form = await context.Request.ReadFormAsync();
            var (clientId, clientSecret) = await EndpointHelper.GetClientCredentialsAsync(context);
            
            var tokenRequest = new TrustIdentity.Core.Validation.TokenRequest
            {
                GrantType = form["grant_type"].ToString(),
                ClientId = clientId ?? form["client_id"].ToString(),
                ClientSecret = clientSecret ?? form["client_secret"].ToString(),
                Code = form["code"].ToString(),
                RedirectUri = form["redirect_uri"].ToString(),
                CodeVerifier = form["code_verifier"].ToString(),
                Username = form["username"].ToString(),
                Password = form["password"].ToString(),
                RefreshToken = form["refresh_token"].ToString(),
                Scope = form["scope"].ToString(),
                AuthReqId = form["auth_req_id"].ToString(),
                SubjectToken = form["subject_token"].ToString(),
                SubjectTokenType = form["subject_token_type"].ToString(),
                ActorToken = form["actor_token"].ToString(),
                ActorTokenType = form["actor_token_type"].ToString()
            };

            var validationResult = await validator.ValidateAsync(tokenRequest);
            if (validationResult.IsError)
            {
                await WriteErrorResponse(context, validationResult.Error!, validationResult.ErrorDescription ?? "Invalid request");
                return;
            }

            // DPoP Validation
            string? dpopThumbprint = null;
            if (context.Request.Headers.ContainsKey("DPoP"))
            {
                var dpopService = context.RequestServices.GetRequiredService<TrustIdentity.Abstractions.Services.IDPoPService>();
                var dpopProof = context.Request.Headers["DPoP"].ToString();
                var dpopResult = await dpopService.ValidateDPoPProofAsync(dpopProof, context.Request.Method, context.Request.Scheme + "://" + context.Request.Host + context.Request.Path);
                
                if (!dpopResult.IsValid)
                {
                    await WriteErrorResponse(context, "invalid_dpop_proof", dpopResult.Error ?? "Invalid DPoP proof");
                    return;
                }
                dpopThumbprint = dpopResult.Thumbprint;
            }

            var client = validationResult.Client!;
            var grantType = tokenRequest.GrantType;
            var scopes = tokenRequest.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            User? user = null;

            // Handle different grant types
            switch (grantType)
            {
                case "authorization_code":
                    user = await GetUserFromAuthorizationCodeAsync(context, tokenRequest.Code!);
                    if (user == null)
                    {
                        await WriteErrorResponse(context, "invalid_grant", "Invalid authorization code");
                        return;
                    }
                    break;

                case "password":
                    user = await AuthenticateUserAsync(context, tokenRequest.Username!, tokenRequest.Password!);
                    if (user == null)
                    {
                        await WriteErrorResponse(context, "invalid_grant", "Invalid username or password");
                        return;
                    }
                    break;

                case "client_credentials":
                    user = new User
                    {
                        SubjectId = client.ClientId,
                        Username = client.ClientName ?? client.ClientId,
                        Email = $"{client.ClientId}@client.local"
                    };
                    break;

                case "refresh_token":
                    user = await GetUserFromRefreshTokenAsync(context, tokenRequest.RefreshToken!);
                    if (user == null)
                    {
                        logger.LogWarning("Token request failed: Invalid refresh token for client_id={ClientId} from IP={IpAddress}", 
                            client.ClientId, context.Connection.RemoteIpAddress);
                        await WriteErrorResponse(context, "invalid_grant", "Invalid refresh token");
                        return;
                    }
                    break;
                case "urn:openid:params:grant-type:ciba":
                    user = await GetUserFromCibaRequestAsync(context, tokenRequest.AuthReqId!, client.ClientId);
                    if (user == null)
                    {
                        var cibaService = context.RequestServices.GetRequiredService<CibaService>();
                        var cibaReq = await cibaService.GetRequestAsync(tokenRequest.AuthReqId!);
                        if (cibaReq != null && cibaReq.IsApproved == null)
                        {
                            await WriteErrorResponse(context, "authorization_pending", "Authorization is pending");
                            return;
                        }
                        await WriteErrorResponse(context, "invalid_grant", "Invalid or expired CIBA request");
                        return;
                    }
                    break;

                case "urn:ietf:params:oauth:grant-type:token-exchange":
                    var tokenExchangeService = context.RequestServices.GetRequiredService<TrustIdentity.Abstractions.Services.ITokenExchangeService>();
                    var exchangeResult = await tokenExchangeService.ExchangeAsync(tokenRequest.SubjectToken!, tokenRequest.SubjectTokenType!, tokenRequest.ActorToken, tokenRequest.ActorTokenType);
                    
                    if (exchangeResult.IsError)
                    {
                        await WriteErrorResponse(context, exchangeResult.Error ?? "invalid_request", exchangeResult.ErrorDescription ?? "Token exchange failed");
                        return;
                    }
                    user = exchangeResult.User;
                    break;

                default:
                    await WriteErrorResponse(context, "unsupported_grant_type", 
                        $"Grant type '{grantType}' is not supported");
                    return;
            }

            if (user == null)
            {
                await WriteErrorResponse(context, "invalid_request", "User identification failed");
                return;
            }

            var token = await tokenService.CreateAccessTokenAsync(client, user, scopes);
            
            // Appending DPoP confirmation claim if present
            if (!string.IsNullOrEmpty(dpopThumbprint))
            {
                 // Create valid cnf claim structure
                 var cnfJson = $"{{\"jkt\":\"{dpopThumbprint}\"}}";
                 token.Claims.Add(new System.Security.Claims.Claim("cnf", cnfJson, "json"));
                 token.ConfirmationMethod = "dpop"; 
            }
            var accessTokenString = await tokenService.GenerateJwtAsync(token);

            // ==============================================================================
            // AI FRAUD DETECTION HOOK
            // ==============================================================================
            var fraudService = context.RequestServices.GetService<IFraudDetectionService>();
            var options = context.RequestServices.GetRequiredService<TrustIdentityOptions>();
            
            if (fraudService != null && options.EnableFraudDetection)
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                
                var riskScore = await fraudService.AnalyzeLoginAttemptAsync(user.SubjectId, ipAddress, userAgent);
                
                if (riskScore > 0.8)
                {
                    logger.LogWarning("HIGH RISK LOGIN DETECTED: User {SubjectId}, Score {Score}", user.SubjectId, riskScore);
                    
                    // Notify User
                    var emailSender = context.RequestServices.GetService<IEmailSender>();
                    if (emailSender != null && !string.IsNullOrEmpty(user.Email))
                    {
                        await emailSender.SendEmailAsync(
                            user.Email, 
                            "Security Alert: Suspicious Login Detected",
                            $"We detected a login attempt with a high risk score ({riskScore:P1}) from IP {ipAddress}. If this was not you, please contact support immediately."
                        );
                    }
                    
                    // Active Blocking
                    if (options.BlockHighRiskLogins)
                    {
                        logger.LogCritical("BLOCKING LOGIN: User {SubjectId} due to high fraud risk score {Score}", user.SubjectId, riskScore);
                        await WriteErrorResponse(context, "access_denied", "Your login attempt was blocked due to suspicious activity. Please contact security.");
                        return;
                    }
                }
            }
            // ==============================================================================

            logger.LogInformation("Token issued successfully: grant_type={GrantType}, client_id={ClientId}, subject={Subject}", 
                grantType, client.ClientId, user.SubjectId);

            var response = new
            {
                access_token = accessTokenString,
                token_type = "Bearer",
                expires_in = 3600,
                scope = string.Join(" ", scopes)
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing token request");
            await WriteErrorResponse(context, "server_error", "An error occurred processing the request");
        }
    }
    private static async Task<User?> GetUserFromAuthorizationCodeAsync(HttpContext context, string code)
    {
        // In a real implementation, you would:
        // 1. Validate the authorization code
        // 2. Retrieve the associated user from the authorization code store
        // 3. Mark the code as used (one-time use)
        
        if (string.IsNullOrEmpty(code))
            return null;

        // Try to get user store from DI
        var userStore = context.RequestServices.GetService<IUserStore>();
        if (userStore == null)
            return null;

        // For now, extract user ID from code (in production, use proper code validation)
        // This is a simplified example - implement proper authorization code validation
        try
        {
            // You should have an IAuthorizationCodeStore service that maps codes to users
            var authCodeStore = context.RequestServices.GetService<IAuthorizationCodeStore>();
            if (authCodeStore != null)
            {
                var authCode = await authCodeStore.GetAuthorizationCodeAsync(code);
                if (authCode != null && !authCode.IsExpired)
                {
                    var user = await userStore.FindBySubjectIdAsync(authCode.SubjectId);
                    await authCodeStore.RemoveAuthorizationCodeAsync(code); // One-time use
                    return user;
                }
                
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("TokenEndpoint");
                logger.LogWarning("Invalid or expired authorization code used: {Code}", code);
            }
        }
        catch (Exception ex)
        {
             var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("TokenEndpoint");
             logger.LogError(ex, "Error validating authorization code");
        }

        return null;
    }

    private static async Task<User?> GetUserFromCibaRequestAsync(HttpContext context, string authReqId, string clientId)
    {
        var cibaService = context.RequestServices.GetRequiredService<CibaService>();
        var userStore = context.RequestServices.GetRequiredService<IUserStore>();
        
        var request = await cibaService.GetRequestAsync(authReqId);
        if (request != null && request.ClientId == clientId && request.IsApproved == true)
        {
            var user = await userStore.FindBySubjectIdAsync(request.SubjectId);
            if (user != null)
            {
                // Consume the request
                await cibaService.RemoveRequestAsync(authReqId);
            }
            return user;
        }

        return null;
    }

    private static async Task<User?> AuthenticateUserAsync(HttpContext context, string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return null;

        // Get user store from DI
        var userStore = context.RequestServices.GetService<IUserStore>();
        if (userStore == null)
            return null;

        try
        {
            // Find user by username
            var user = await userStore.FindByUsernameAsync(username);
            if (user == null)
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("TokenEndpoint");
                logger.LogWarning("Authentication failed: User not found {Username} from IP={IpAddress}", 
                    username, context.Connection.RemoteIpAddress);
                return null;
            }

            // Validate password (you should use a proper password hasher)
            var passwordHasher = context.RequestServices.GetService<IPasswordHasher>();
            if (passwordHasher != null)
            {
                var isValid = passwordHasher.VerifyPassword(user, password);
                if (!isValid)
                {
                    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("TokenEndpoint");
                    logger.LogWarning("Authentication failed: Invalid password for {Username} from IP={IpAddress}", 
                        username, context.Connection.RemoteIpAddress);
                }
                return isValid ? user : null;
            }

            return null;
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("TokenEndpoint");
            logger.LogError(ex, "Error during user authentication");
            return null;
        }
    }

    private static async Task<User?> GetUserFromRefreshTokenAsync(HttpContext context, string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        try
        {
            // Get refresh token service from DI
            var tokenService = context.RequestServices.GetService<IRefreshTokenService>();
            if (tokenService == null)
                return null;

            // Validate and get refresh token
            var storedToken = await tokenService.GetRefreshTokenAsync(refreshToken);
            if (storedToken == null || storedToken.IsExpired)
                return null;

            // Get user from user store
            var userStore = context.RequestServices.GetService<IUserStore>();
            if (userStore == null)
                return null;

            return await userStore.FindBySubjectIdAsync(storedToken.SubjectId);
        }
        catch
        {
            return null;
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, string error, string description)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";
        
        var errorResponse = new
        {
            error = error,
            error_description = description
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}

/// <summary>
/// Handler for the OpenID Connect UserInfo Endpoint
/// </summary>
public static class UserInfoEndpoint
{
    /// <summary>
    /// Handles the userinfo request asynchronously
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UserInfoEndpoint");
        var tokenService = context.RequestServices.GetRequiredService<ITokenService>();
        var userStore = context.RequestServices.GetRequiredService<IUserStore>();
        
        try
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var token = authHeader.Substring("Bearer ".Length);
            var validationResult = await tokenService.ValidateTokenDetailedAsync(token);

            if (!validationResult.IsValid || validationResult.Principal == null)
            {
                logger.LogWarning("UserInfo request failed: Invalid token from IP={IpAddress}", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = 401;
                return;
            }

            var subjectId = validationResult.Principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(subjectId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var user = await userStore.FindBySubjectIdAsync(subjectId);
            if (user == null)
            {
                logger.LogWarning("UserInfo request failed: User {SubjectId} not found", subjectId);
                context.Response.StatusCode = 401;
                return;
            }

            var userInfo = new
            {
                sub = user.SubjectId,
                name = user.Username,
                email = user.Email,
                email_verified = true // In a real system, this would come from the user record
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(userInfo));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing userinfo request");
            context.Response.StatusCode = 500;
        }
    }
}

/// <summary>
/// Handler for the OAuth 2.0 Token Introspection Endpoint
/// </summary>
public static class IntrospectionEndpoint
{
    /// <summary>
    /// Handles the introspection request asynchronously
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("IntrospectionEndpoint");
        var tokenService = context.RequestServices.GetRequiredService<ITokenService>();
        var clientService = context.RequestServices.GetRequiredService<ClientService>();
        
        try
        {
            // 1. Authenticate Client
            var (clientId, clientSecret) = await EndpointHelper.GetClientCredentialsAsync(context);
            if (string.IsNullOrEmpty(clientId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var client = await clientService.FindClientByIdAsync(clientId);
            if (client == null || !await clientService.ValidateSecretAsync(client, clientSecret ?? ""))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var form = await context.Request.ReadFormAsync();
            var token = form["token"].ToString();

            if (string.IsNullOrEmpty(token))
            {
                await context.Response.WriteAsJsonAsync(new { active = false });
                return;
            }

            var validationResult = await tokenService.ValidateTokenDetailedAsync(token);

            if (!validationResult.IsValid || validationResult.Principal == null)
            {
                await context.Response.WriteAsJsonAsync(new { active = false });
                return;
            }

            var principal = validationResult.Principal;
            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var iat = principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
            var scopes = principal.FindAll("scope").Select(c => c.Value).ToList();

            var response = new
            {
                active = true,
                sub = sub,
                scope = string.Join(" ", scopes),
                iat = iat != null ? long.Parse(iat) : (long?)null,
                token_type = "Bearer",
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing introspection request");
            context.Response.StatusCode = 500;
        }
    }
}

/// <summary>
/// Handler for the OAuth 2.0 / OpenID Connect Authorization Endpoint
/// </summary>
public static class AuthorizationEndpoint
{
    /// <summary>
    /// Handles the authorization request asynchronously
    /// </summary>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("AuthorizationEndpoint");
        var validator = context.RequestServices.GetRequiredService<TrustIdentity.Core.Validation.AuthorizeRequestValidator>();
        var clientStore = context.RequestServices.GetRequiredService<IClientStore>();
        var authCodeService = context.RequestServices.GetRequiredService<IAuthorizationCodeService>();

        try
        {
            var query = context.Request.Query;
            var request = new TrustIdentity.Core.Validation.AuthorizeRequest
            {
                ClientId = query["client_id"].ToString(),
                RedirectUri = query["redirect_uri"].ToString(),
                ResponseType = query["response_type"].ToString(),
                Scope = query["scope"].ToString(),
                State = query["state"].ToString(),
                Nonce = query["nonce"].ToString(),
                CodeChallenge = query["code_challenge"].ToString(),
                CodeChallengeMethod = query["code_challenge_method"].ToString(),
                AcrValues = query["acr_values"].ToString()
            };

            var client = await clientStore.FindClientByIdAsync(request.ClientId);
            if (client == null)
            {
                logger.LogWarning("Authorization request for unknown client: {ClientId}", request.ClientId);
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("invalid_client");
                return;
            }

            var validationResult = validator.Validate(request, client);
            if (validationResult.IsError)
            {
                // Security: Only redirect if the redirect URI is valid for that client
                if (!string.IsNullOrEmpty(request.RedirectUri) && client.RedirectUris.Contains(request.RedirectUri))
                {
                    var errorUrl = $"{request.RedirectUri}?error={validationResult.Error}&error_description={System.Net.WebUtility.UrlEncode(validationResult.ErrorDescription)}";
                    if (!string.IsNullOrEmpty(request.State)) errorUrl += $"&state={request.State}";
                    context.Response.Redirect(errorUrl);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync($"Error: {validationResult.Error}. {validationResult.ErrorDescription}");
                }
                return;
            }

            // Check if user is authenticated
            // Note: In a production system, use context.AuthenticateAsync() or rely on middleware
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                var options = context.RequestServices.GetRequiredService<TrustIdentityOptions>();
                var loginUrl = options.UserInteraction.LoginUrl;
                
                // Redirect to login page
                var returnUrl = System.Net.WebUtility.UrlEncode(context.Request.Path + context.Request.QueryString);
                var loginRedirectUrl = loginUrl.Contains("?") ? $"{loginUrl}&returnUrl={returnUrl}" : $"{loginUrl}?returnUrl={returnUrl}";
                
                context.Response.Redirect(loginRedirectUrl);
                return;
            }

            // Check if consent is required
            var consentService = context.RequestServices.GetRequiredService<ConsentService>();
            var requestedScopes = request.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            
            if (await consentService.RequiresConsentAsync(client, context.User, requestedScopes))
            {
                var options = context.RequestServices.GetRequiredService<TrustIdentityOptions>();
                var consentUrl = options.UserInteraction.ConsentUrl;
                
                var returnUrl = System.Net.WebUtility.UrlEncode(context.Request.Path + context.Request.QueryString);
                var consentRedirectUrl = consentUrl.Contains("?") ? $"{consentUrl}&returnUrl={returnUrl}" : $"{consentUrl}?returnUrl={returnUrl}";
                
                context.Response.Redirect(consentRedirectUrl);
                return;
            }

            // Generate Authorization Code
            var subjectId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(subjectId))
            {
                logger.LogWarning("Authenticated user has no NameIdentifier claim. Cannot issue authorization code.");
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("unauthorized_client");
                return;
            }

            var authCode = new AuthorizationCode
            {
                SubjectId = subjectId,
                ClientId = request.ClientId,
                Scopes = request.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                RedirectUri = request.RedirectUri,
                Nonce = request.Nonce,
                CodeChallenge = request.CodeChallenge,
                CodeChallengeMethod = request.CodeChallengeMethod,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(context.RequestServices.GetRequiredService<TrustIdentityOptions>().Authentication.AuthorizationCodeLifetime)
            };

            var codeValue = await authCodeService.CreateAuthorizationCodeAsync(authCode);

            // Redirect back to client
            var redirectUrl = $"{request.RedirectUri}?code={codeValue}";
            if (!string.IsNullOrEmpty(request.State)) redirectUrl += $"&state={request.State}";
            
            context.Response.Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing authorization request");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }
}

/// <summary>
/// Handler for the OAuth 2.0 Token Revocation Endpoint
/// </summary>
public static class RevocationEndpoint
{
    /// <summary>
    /// Handles the revocation request asynchronously
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RevocationEndpoint");
        var grantStore = context.RequestServices.GetRequiredService<IPersistedGrantStore>();
        var clientService = context.RequestServices.GetRequiredService<ClientService>();
        
        try
        {
            // 1. Authenticate Client
            var (clientId, clientSecret) = await EndpointHelper.GetClientCredentialsAsync(context);
            if (string.IsNullOrEmpty(clientId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var client = await clientService.FindClientByIdAsync(clientId);
            if (client == null || !await clientService.ValidateSecretAsync(client, clientSecret ?? ""))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var form = await context.Request.ReadFormAsync();
            var token = form["token"].ToString();

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 400;
                return;
            }

            // 3. Verify Ownership and Revoke
            var grant = await grantStore.GetAsync(token);
            if (grant != null)
            {
                if (grant.ClientId != client.ClientId)
                {
                    logger.LogWarning("Client {ClientId} attempted to revoke token belonging to client {TokenClientId}", client.ClientId, grant.ClientId);
                    context.Response.StatusCode = 400;
                    return;
                }

                await grantStore.RemoveAsync(token);
            }

            logger.LogInformation("Token revoked successfully for client {ClientId}", client.ClientId);
            context.Response.StatusCode = 200;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing revocation request");
            context.Response.StatusCode = 500;
        }
    }
}

/// <summary>
/// Handler for the OIDC End Session (Logout) Endpoint
/// </summary>
public static class EndSessionEndpoint
{
    /// <summary>
    /// Handles the end session request asynchronously
    /// </summary>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("EndSessionEndpoint");
        var clientStore = context.RequestServices.GetRequiredService<IClientStore>();
        var options = context.RequestServices.GetRequiredService<TrustIdentityOptions>();
        
        try
        {
            var query = context.Request.Query;
            var postLogoutRedirectUri = query["post_logout_redirect_uri"].ToString();
            var state = query["state"].ToString();

            logger.LogInformation("End session request for IP: {IpAddress}", context.Connection.RemoteIpAddress);

            // 1. Perform proper logout from the authentication cookie
            await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(context, options.Authentication.CookieAuthenticationScheme);
            
            // 2. Security: Validate post_logout_redirect_uri
            if (!string.IsNullOrEmpty(postLogoutRedirectUri))
            {
                // In a production system, we should ideally validate this against the client that issued the id_token_hint.
                // For broad compatibility, we check if the URI is registered for ANY client.
                var allClients = await clientStore.GetAllClientsAsync();
                var isValidUri = allClients.Any(c => c.PostLogoutRedirectUris.Contains(postLogoutRedirectUri, StringComparer.OrdinalIgnoreCase));

                if (isValidUri)
                {
                    logger.LogInformation("Redirecting to validated post-logout URI: {Uri}", postLogoutRedirectUri);
                    var redirectUrl = postLogoutRedirectUri;
                    if (!string.IsNullOrEmpty(state))
                    {
                        redirectUrl += redirectUrl.Contains("?") ? $"&state={state}" : $"?state={state}";
                    }
                    context.Response.Redirect(redirectUrl);
                    return;
                }
                else
                {
                    logger.LogWarning("Invalid post_logout_redirect_uri blocked: {Uri}", postLogoutRedirectUri);
                }
            }

            // Fallback: Default logout success response
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("<html><body><h1>Logged out successfully</h1><p>You may now close this window.</p><a href='/'>Return to Home</a></body></html>");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing end session request");
            context.Response.StatusCode = 500;
        }
    }
}

/// <summary>
/// Handler for the OAuth 2.0 Device Authorization Endpoint
/// </summary>
public static class DeviceAuthorizationEndpoint
{
    /// <summary>
    /// Handles the device authorization request asynchronously
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("DeviceAuthorizationEndpoint");
        
        try
        {
            var deviceCode = Guid.NewGuid().ToString("N");
            var userCode = EndpointHelper.GenerateUserCode();

            // 1. Extract client credentials
            var (clientId, clientSecret) = await EndpointHelper.GetClientCredentialsAsync(context);
            
            // 2. Validate client if present (device flow usually requires client_id)
            if (!string.IsNullOrEmpty(clientId))
            {
                var clientService = context.RequestServices.GetRequiredService<ClientService>();
                var client = await clientService.FindClientByIdAsync(clientId);
                if (client == null || (client.RequireClientSecret && !await clientService.ValidateSecretAsync(client, clientSecret ?? "")))
                {
                    context.Response.StatusCode = 401;
                    return;
                }
            }

            var options = context.RequestServices.GetRequiredService<TrustIdentityOptions>();
            var issuer = options.IssuerUri.TrimEnd('/');

            var response = new
            {
                device_code = deviceCode,
                user_code = userCode,
                verification_uri = $"{issuer}/device",
                verification_uri_complete = $"{issuer}/device?user_code={userCode}",
                expires_in = options.DeviceFlow.DeviceCodeLifetime,
                interval = 5
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing device authorization request");
            context.Response.StatusCode = 500;
        }
    }

}

/// <summary>
/// Helper methods for endpoint handlers
/// </summary>
internal static class EndpointHelper
{
    /// <summary>
    /// Generates a random user code for device flow
    /// </summary>
    public static string GenerateUserCode()
    {
        return string.Format("{0:D4}-{1:D4}", 
            System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 10000), 
            System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 10000));
    }

    /// <summary>
    /// Extracts client credentials from Basic Auth header or Form POST body
    /// </summary>
    public static async Task<(string? clientId, string? clientSecret)> GetClientCredentialsAsync(HttpContext context)
    {
        // 1. Check Basic Authentication header
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var base64 = authHeader.Substring(6);
                var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                var parts = decoded.Split(':', 2);
                if (parts.Length == 2)
                {
                    return (parts[0], parts[1]);
                }
            }
            catch { /* Ignore invalid base64 */ }
        }

        // 2. Check Form POST body
        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();
            var clientId = form["client_id"].ToString();
            var clientSecret = form["client_secret"].ToString();

            if (!string.IsNullOrEmpty(clientId))
            {
                return (clientId, clientSecret);
            }
        }

        return (null, null);
    }
}


/// <summary>
/// Backchannel Authentication endpoint (CIBA - Client Initiated Backchannel Authentication)
/// </summary>
public static class BackchannelAuthenticationEndpoint
{
    /// <summary>
    /// Handles backchannel authentication requests
    /// </summary>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("BackchannelAuthenticationEndpoint");
        var clientStore = context.RequestServices.GetRequiredService<IClientStore>();
        var userStore = context.RequestServices.GetRequiredService<IUserStore>();
        var cibaService = context.RequestServices.GetRequiredService<CibaService>();
        var clientService = context.RequestServices.GetRequiredService<ClientService>();

        try
        {
            var (clientId, clientSecret) = await EndpointHelper.GetClientCredentialsAsync(context);
            if (string.IsNullOrEmpty(clientId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "invalid_client" });
                return;
            }

            var client = await clientStore.FindClientByIdAsync(clientId);
            if (client == null || !await clientService.ValidateSecretAsync(client, clientSecret ?? string.Empty))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "invalid_client" });
                return;
            }

            var form = await context.Request.ReadFormAsync();
            var scope = form["scope"].ToString();
            var loginHint = form["login_hint"].ToString();
            var bindingMessage = form["binding_message"].ToString();

            var user = await userStore.FindByUsernameAsync(loginHint);
            if (user == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { error = "unknown_user_id" });
                return;
            }

            var request = new BackchannelAuthenticationRequest
            {
                ClientId = clientId,
                SubjectId = user.SubjectId,
                Scopes = scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                BindingMessage = bindingMessage,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            var authReqId = await cibaService.CreateRequestAsync(request);

            var emailSender = context.RequestServices.GetService<IEmailSender>();
            var options = context.RequestServices.GetRequiredService<TrustIdentityOptions>();
            if (emailSender != null && !string.IsNullOrEmpty(user.Email))
            {
                var approveUrl = $"{options.IssuerUri}/Ciba?id={authReqId}&binding_message={Uri.EscapeDataString(bindingMessage)}";
                await emailSender.SendEmailAsync(user.Email, "Login Request", $"A login is requested. Click here: {approveUrl}");
            }

            await context.Response.WriteAsJsonAsync(new { auth_req_id = authReqId, expires_in = 300, interval = 5 });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CIBA endpoint error");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "server_error" });
        }
    }
}