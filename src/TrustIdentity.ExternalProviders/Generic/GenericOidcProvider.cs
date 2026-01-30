using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TrustIdentity.ExternalProviders.Generic;

/// <summary>
/// Generic OpenID Connect Provider
/// </summary>
public class GenericOidcProvider : IExternalAuthenticationProvider
{
    private readonly GenericOidcConfiguration _config;
    private readonly ILogger<GenericOidcProvider> _logger;
    private readonly HttpClient _httpClient;
    private OidcDiscoveryDocument? _discoveryDocument;

    /// <summary>
    /// Initializes a new instance of the GenericOidcProvider
    /// </summary>
    public GenericOidcProvider(
        GenericOidcConfiguration config,
        ILogger<GenericOidcProvider> logger,
        IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("GenericOidc");
    }

    /// <summary>
    /// The name of the provider
    /// </summary>
    public string ProviderName => _config.ProviderName;

    /// <summary>
    /// Ensure discovery document is loaded if needed
    /// </summary>
    private async Task EnsureConfigurationAsync()
    {
        if (_discoveryDocument != null) return;
        
        if (!string.IsNullOrEmpty(_config.MetadataAddress))
        {
            try 
            {
                var response = await _httpClient.GetAsync(_config.MetadataAddress);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _discoveryDocument = JsonSerializer.Deserialize<OidcDiscoveryDocument>(content);
                }
                else
                {
                    _logger.LogError("Failed to load OIDC discovery document from {Address}", _config.MetadataAddress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading OIDC discovery document");
            }
        }
        
        // Fallback or initialization of manual config
        _discoveryDocument ??= new OidcDiscoveryDocument
        {
            AuthorizationEndpoint = _config.AuthorizationEndpoint,
            TokenEndpoint = _config.TokenEndpoint,
            UserInfoEndpoint = _config.UserInfoEndpoint
        };
    }

    /// <inheritdoc/>
    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        // This is synchronous but we might need async for discovery. 
        // Best practice: Cache discovery doc at startup or assume config is ready.
        // For this implementation, we'll block on discovery if it's missing (not ideal but safe)
        // Or better, we assume AuthorizationEndpoint is set in Config or we can't proceed.
        
        // If discovery is needed, we should have loaded it. 
        // If this method must be sync, we can't use await.
        // We will assume _config has the endpoints populated or we fail.
        // But to be safe, let's make a blocking call if absolutely necessary, 
        // OR rely on the metadata already being loaded by a background service.
        
        // For simplicity: We'll require AuthorizationEndpoint to be set explicitly or via a sync check (bad).
        // Let's assume the user MUST provide the specific endpoints in config OR we rely on a background init.
        // However, I'll add a check:
        
        var authEndpoint = _config.AuthorizationEndpoint;
        if (string.IsNullOrEmpty(authEndpoint) && !string.IsNullOrEmpty(_config.MetadataAddress))
        {
             // Try to resolve via discovery (blocking)
             var task = EnsureConfigurationAsync();
             task.Wait();
             authEndpoint = _discoveryDocument?.AuthorizationEndpoint;
        }

        if (string.IsNullOrEmpty(authEndpoint))
        {
            throw new ExternalProviderException("Authorization Endpoint is missing and could not be discovered.");
        }

        var actualRedirectUri = redirectUri ?? _config.RedirectUri;
        
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _config.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = actualRedirectUri,
            ["scope"] = string.Join(" ", _config.Scopes),
            ["state"] = state,
            ["nonce"] = Guid.NewGuid().ToString()
        };

        var queryString = string.Join("&", parameters.Select(kvp => 
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{authEndpoint}?{queryString}";
    }

    /// <inheritdoc/>
    public async Task<ExternalAuthenticationResult> AuthenticateAsync(string code, string? redirectUri = null)
    {
        try
        {
            await EnsureConfigurationAsync();
            var tokenEndpoint = _discoveryDocument?.TokenEndpoint ?? _config.TokenEndpoint;
            
            if (string.IsNullOrEmpty(tokenEndpoint))
                throw new ExternalProviderException("Token Endpoint is missing.");

            var actualRedirectUri = redirectUri ?? _config.RedirectUri;

            var tokenRequest = new Dictionary<string, string>
            {
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = actualRedirectUri
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await _httpClient.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Generic OIDC token request failed: {Error}", errorContent);
                return new ExternalAuthenticationResult { Success = false, ErrorMessage = "Token exchange failed" };
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
            
            var accessToken = tokenResponse.GetProperty("access_token").GetString()!;
            var idToken = tokenResponse.TryGetProperty("id_token", out var idt) ? idt.GetString() : null;
            var refreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            
            // Extract claims
            List<Claim> claims = new List<Claim>();
            if (!string.IsNullOrEmpty(idToken))
            {
                claims.AddRange(ParseIdToken(idToken));
            }
            else
            {
                // Fetch from UserInfo endpoint if ID token is missing
                var userInfoEndpoint = _discoveryDocument?.UserInfoEndpoint ?? _config.UserInfoEndpoint;
                if (!string.IsNullOrEmpty(userInfoEndpoint))
                {
                    claims.AddRange(await GetUserInfoAsync(userInfoEndpoint, accessToken));
                }
            }

            var userId = claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = claims.FirstOrDefault(c => c.Type == "name" || c.Type == "given_name")?.Value;

            return new ExternalAuthenticationResult
            {
                Success = true,
                Provider = ProviderName,
                ProviderUserId = userId,
                Email = email,
                DisplayName = name,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Claims = claims
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Generic OIDC authentication");
            return new ExternalAuthenticationResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private async Task<IEnumerable<Claim>> GetUserInfoAsync(string endpoint, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            var claims = new List<Claim>();
            foreach(var prop in json.EnumerateObject())
            {
                 claims.Add(new Claim(prop.Name, prop.Value.ToString()));
            }
            return claims;
        }
        return Enumerable.Empty<Claim>();
    }

    /// <inheritdoc/>
    public async Task<ExternalAuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        // Similar implementation to AuthenticateAsync but with grant_type=refresh_token
        // Omitting for brevity in this initial pass, assuming standard OIDC behavior
         try
        {
            await EnsureConfigurationAsync();
            var tokenEndpoint = _discoveryDocument?.TokenEndpoint ?? _config.TokenEndpoint;

            var tokenRequest = new Dictionary<string, string>
            {
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await _httpClient.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                return new ExternalAuthenticationResult { Success = false, ErrorMessage = "Refresh failed" };
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
            var accessToken = tokenResponse.GetProperty("access_token").GetString()!;
            var newRefreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : refreshToken;

            return new ExternalAuthenticationResult 
            { 
                Success = true, 
                Provider = ProviderName,
                AccessToken = accessToken, 
                RefreshToken = newRefreshToken 
            };
        }
        catch(Exception ex)
        {
             _logger.LogError(ex, "Error refreshing token");
             return new ExternalAuthenticationResult { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    private List<Claim> ParseIdToken(string idToken)
    {
        var claims = new List<Claim>();
        try
        {
            var parts = idToken.Split('.');
            if (parts.Length != 3) return claims;
            
            var payload = parts[1];
             var paddingLength = 4 - (payload.Length % 4);
            if (paddingLength < 4) payload += new string('=', paddingLength);
            
            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson);
            
            foreach (var property in payloadData.EnumerateObject())
            {
                claims.Add(new Claim(property.Name, property.Value.ToString()));
            }
        }
        catch (Exception ex) {_logger.LogError(ex, "Failed to parse ID Token");}
        return claims;
    }

    private class OidcDiscoveryDocument
    {
        [System.Text.Json.Serialization.JsonPropertyName("authorization_endpoint")]
        public string? AuthorizationEndpoint { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("token_endpoint")]
        public string? TokenEndpoint { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("userinfo_endpoint")]
        public string? UserInfoEndpoint { get; set; }
    }
}

/// <summary>
/// Configuration options for Generic OIDC Provider
/// </summary>
public class GenericOidcConfiguration
{
    /// <summary>Name of the provider (e.g. "Google")</summary>
    public string ProviderName { get; set; } = "GenericOidc";
    
    /// <summary>Client ID from the provider</summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>Client Secret from the provider</summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>Redirect URI registered with the provider</summary>
    public string RedirectUri { get; set; } = string.Empty;
    
    /// <summary>Scopes to request</summary>
    public List<string> Scopes { get; set; } = new() { "openid", "profile", "email" };
    
    /// <summary>Explicit Authorization Endpoint (optional if MetadataAddress is set)</summary>
    public string? AuthorizationEndpoint { get; set; }
    
    /// <summary>Explicit Token Endpoint (optional if MetadataAddress is set)</summary>
    public string? TokenEndpoint { get; set; }
    
    /// <summary>Explicit UserInfo Endpoint (optional if MetadataAddress is set)</summary>
    public string? UserInfoEndpoint { get; set; }
    
    /// <summary>OIDC Discovery Metadata Address (e.g. .well-known/openid-configuration)</summary>
    public string? MetadataAddress { get; set; }
}
