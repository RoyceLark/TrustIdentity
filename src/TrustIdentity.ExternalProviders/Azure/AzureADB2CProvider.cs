using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace TrustIdentity.ExternalProviders.Azure;

/// <summary>
/// Azure AD B2C Provider for consumer identity scenarios
/// </summary>
public class AzureADB2CProvider : IExternalAuthenticationProvider
{
    private readonly AzureADB2CConfiguration _config;
    private readonly ILogger<AzureADB2CProvider> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the AzureADB2CProvider
    /// </summary>
    /// <param name="config">The Azure AD B2C configuration</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="httpClientFactory">The HTTP client factory</param>
    public AzureADB2CProvider(
        AzureADB2CConfiguration config,
        ILogger<AzureADB2CProvider> logger,
        IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("AzureADB2C");
    }

    /// <summary>
    /// The name of the provider
    /// </summary>
    public string ProviderName => "AzureADB2C";

    /// <summary>
    /// Get authorization URL for Azure AD B2C
    /// </summary>
    /// <param name="state">The state parameter for CSRF protection</param>
    /// <param name="redirectUri">Optional redirect URI to override the default</param>
    /// <returns>The Azure AD B2C authorization URL</returns>
    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        try
        {
            var actualRedirectUri = redirectUri ?? _config.RedirectUri;
            
            // Azure AD B2C uses a different URL structure
            var authUrl = $"https://{_config.Instance}/{_config.Domain}/{_config.SignUpSignInPolicyId}/oauth2/v2.0/authorize";
            
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = _config.ClientId,
                ["response_type"] = "code",
                ["redirect_uri"] = actualRedirectUri,
                ["response_mode"] = "query",
                ["scope"] = string.Join(" ", _config.Scopes),
                ["state"] = state,
                ["nonce"] = Guid.NewGuid().ToString()
            };

            // Add optional parameters
            if (!string.IsNullOrEmpty(_config.DomainHint))
            {
                parameters["domain_hint"] = _config.DomainHint;
            }

            if (!string.IsNullOrEmpty(_config.LoginHint))
            {
                parameters["login_hint"] = _config.LoginHint;
            }

            var queryString = string.Join("&", parameters.Select(kvp => 
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            return $"{authUrl}?{queryString}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Azure AD B2C authorization URL");
            throw new ExternalProviderException("Failed to generate authorization URL", ex);
        }
    }

    /// <summary>
    /// Exchange authorization code for access token
    /// </summary>
    /// <param name="code">The authorization code received from Azure AD B2C</param>
    /// <param name="redirectUri">Optional redirect URI to override the default</param>
    /// <returns>A result object containing user info and tokens</returns>
    public async Task<ExternalAuthenticationResult> AuthenticateAsync(string code, string? redirectUri = null)
    {
        try
        {
            _logger.LogInformation("Authenticating with Azure AD B2C using authorization code");

            var actualRedirectUri = redirectUri ?? _config.RedirectUri;
            var tokenUrl = $"https://{_config.Instance}/{_config.Domain}/{_config.SignUpSignInPolicyId}/oauth2/v2.0/token";

            var tokenRequest = new Dictionary<string, string>
            {
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = actualRedirectUri,
                ["scope"] = string.Join(" ", _config.Scopes)
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await _httpClient.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure AD B2C token request failed: {Error}", errorContent);
                
                return new ExternalAuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to exchange authorization code for tokens",
                    ErrorCode = response.StatusCode.ToString()
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);

            var accessToken = tokenResponse.GetProperty("access_token").GetString()!;
            var idToken = tokenResponse.GetProperty("id_token").GetString();
            var refreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();

            // Parse ID token to get user claims
            var claims = ParseIdToken(idToken!);

            var email = claims.FirstOrDefault(c => c.Type == "emails" || c.Type == "email")?.Value;
            var name = claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var userId = claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "oid")?.Value;

            return new ExternalAuthenticationResult
            {
                Success = true,
                Provider = ProviderName,
                ProviderUserId = userId,
                Email = email,
                DisplayName = name,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn),
                Claims = claims
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Azure AD B2C authentication");
            return new ExternalAuthenticationResult
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred during authentication"
            };
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>A result object containing the new access token</returns>
    public async Task<ExternalAuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var tokenUrl = $"https://{_config.Instance}/{_config.Domain}/{_config.SignUpSignInPolicyId}/oauth2/v2.0/token";

            var tokenRequest = new Dictionary<string, string>
            {
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["scope"] = string.Join(" ", _config.Scopes)
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await _httpClient.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure AD B2C token refresh failed: {Error}", errorContent);
                
                return new ExternalAuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to refresh token"
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);

            var accessToken = tokenResponse.GetProperty("access_token").GetString()!;
            var newRefreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : refreshToken;
            var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();

            return new ExternalAuthenticationResult
            {
                Success = true,
                Provider = ProviderName,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                TokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh Azure AD B2C token");
            return new ExternalAuthenticationResult
            {
                Success = false,
                ErrorMessage = "Failed to refresh token"
            };
        }
    }

    /// <summary>
    /// Parse JWT ID token to extract claims
    /// </summary>
    private List<Claim> ParseIdToken(string idToken)
    {
        var claims = new List<Claim>();

        try
        {
            // JWT tokens have 3 parts separated by dots
            var parts = idToken.Split('.');
            if (parts.Length != 3)
            {
                _logger.LogWarning("Invalid ID token format");
                return claims;
            }

            // Decode the payload (second part)
            var payload = parts[1];
            
            // Add padding if necessary
            var paddingLength = 4 - (payload.Length % 4);
            if (paddingLength < 4)
            {
                payload += new string('=', paddingLength);
            }

            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson);

            // Extract standard claims
            foreach (var property in payloadData.EnumerateObject())
            {
                var value = property.Value.ValueKind == JsonValueKind.String 
                    ? property.Value.GetString() 
                    : property.Value.ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    claims.Add(new Claim(property.Name, value));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse ID token");
        }

        return claims;
    }
}

/// <summary>
/// Azure AD B2C Configuration
/// </summary>
public class AzureADB2CConfiguration
{
    /// <summary>The B2C instance (e.g., "myb2ctenant.b2clogin.com")</summary>
    public string Instance { get; set; } = string.Empty;
    
    /// <summary>The B2C domain (e.g., "myb2ctenant.onmicrosoft.com")</summary>
    public string Domain { get; set; } = string.Empty;
    
    /// <summary>The Azure client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>The Azure client secret</summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>The redirect URI for the application</summary>
    public string RedirectUri { get; set; } = string.Empty;
    
    /// <summary>The sign-up/sign-in policy ID (e.g., "B2C_1_signupsignin1")</summary>
    public string SignUpSignInPolicyId { get; set; } = "B2C_1_signupsignin1";
    
    /// <summary>The password reset policy ID</summary>
    public string? ResetPasswordPolicyId { get; set; }
    
    /// <summary>The edit profile policy ID</summary>
    public string? EditProfilePolicyId { get; set; }
    
    /// <summary>The requested OAuth 2.0 scopes</summary>
    public List<string> Scopes { get; set; } = new() { "openid", "profile", "email" };
    
    /// <summary>Optional domain hint for the login page</summary>
    public string? DomainHint { get; set; }
    
    /// <summary>Optional login hint (pre-fill email)</summary>
    public string? LoginHint { get; set; }
}
