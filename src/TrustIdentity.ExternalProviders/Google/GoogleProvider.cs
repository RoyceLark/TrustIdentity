using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
namespace TrustIdentity.ExternalProviders.Google;

/// <summary>
/// Google OAuth 2.0 Provider
/// </summary>
public class GoogleProvider : IExternalAuthenticationProvider
{
    private readonly GoogleConfiguration _config;
    private readonly ILogger<GoogleProvider> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the GoogleProvider
    /// </summary>
    /// <param name="config">The Google configuration</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="httpClientFactory">The HTTP client factory</param>
    public GoogleProvider(
        GoogleConfiguration config,
        ILogger<GoogleProvider> logger,
        IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Google");
    }

    /// <summary>The provider name</summary>
    public string ProviderName => "Google";

    /// <summary>
    /// Gets the authorization URL for Google
    /// </summary>
    /// <param name="state">The state parameter</param>
    /// <param name="redirectUri">The redirect URI</param>
    /// <returns>The authorization URL</returns>
    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        try
        {
            var actualRedirectUri = redirectUri ?? _config.RedirectUri;
            
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = _config.ClientId,
                ["redirect_uri"] = actualRedirectUri,
                ["response_type"] = "code",
                ["scope"] = string.Join(" ", _config.Scopes),
                ["state"] = state,
                ["access_type"] = "offline",
                ["prompt"] = "consent"
            };

            var queryString = string.Join("&", parameters.Select(kvp => 
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Google authorization URL");
            throw new ExternalProviderException("Failed to generate authorization URL", ex);
        }
    }

    /// <summary>
    /// Authenticates with Google using an authorization code
    /// </summary>
    /// <param name="code">The authorization code</param>
    /// <param name="redirectUri">The redirect URI</param>
    /// <returns>The authentication result</returns>
    public async Task<ExternalAuthenticationResult> AuthenticateAsync(string code, string? redirectUri = null)
    {
        try
        {
            _logger.LogInformation("Authenticating with Google using authorization code");

            var actualRedirectUri = redirectUri ?? _config.RedirectUri;

            // Exchange code for token
            var tokenRequest = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret,
                ["redirect_uri"] = actualRedirectUri,
                ["grant_type"] = "authorization_code"
            };

            var tokenResponse = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var error = await tokenResponse.Content.ReadAsStringAsync();
                _logger.LogError("Google token exchange failed: {Error}", error);
                return new ExternalAuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to exchange authorization code for token"
                };
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

            var accessToken = tokenData.GetProperty("access_token").GetString()!;
            var refreshToken = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var expiresIn = tokenData.GetProperty("expires_in").GetInt32();

            // Get user info
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var userInfoResponse = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            
            if (!userInfoResponse.IsSuccessStatusCode)
            {
                return new ExternalAuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve user information"
                };
            }

            var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoJson);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.GetProperty("id").GetString()!),
                new Claim(ClaimTypes.Email, userInfo.GetProperty("email").GetString()!),
                new Claim(ClaimTypes.Name, userInfo.GetProperty("name").GetString()!),
                new Claim("email_verified", userInfo.GetProperty("verified_email").GetBoolean().ToString())
            };

            if (userInfo.TryGetProperty("given_name", out var givenName))
                claims.Add(new Claim(ClaimTypes.GivenName, givenName.GetString()!));

            if (userInfo.TryGetProperty("family_name", out var familyName))
                claims.Add(new Claim(ClaimTypes.Surname, familyName.GetString()!));

            if (userInfo.TryGetProperty("picture", out var picture))
                claims.Add(new Claim("picture", picture.GetString()!));

            return new ExternalAuthenticationResult
            {
                Success = true,
                Provider = ProviderName,
                ProviderUserId = userInfo.GetProperty("id").GetString()!,
                Email = userInfo.GetProperty("email").GetString(),
                DisplayName = userInfo.GetProperty("name").GetString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn),
                Claims = claims
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google authentication failed");
            return new ExternalAuthenticationResult
            {
                Success = false,
                ErrorMessage = $"Google authentication failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Refreshes the Google access token
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>The authentication result with new tokens</returns>
    public async Task<ExternalAuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var tokenRequest = new Dictionary<string, string>
            {
                ["refresh_token"] = refreshToken,
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret,
                ["grant_type"] = "refresh_token"
            };

            var response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                return new ExternalAuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to refresh token"
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);

            return new ExternalAuthenticationResult
            {
                Success = true,
                Provider = ProviderName,
                AccessToken = data.GetProperty("access_token").GetString()!,
                TokenExpiration = DateTime.UtcNow.AddSeconds(data.GetProperty("expires_in").GetInt32())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh Google token");
            return new ExternalAuthenticationResult
            {
                Success = false,
                ErrorMessage = "Failed to refresh token"
            };
        }
    }
}

/// <summary>
/// Configuration for Google provider
/// </summary>
public class GoogleConfiguration
{
    /// <summary>The Google Client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The Google Client Secret</summary>
    public string ClientSecret { get; set; } = string.Empty;
    /// <summary>The redirect URI</summary>
    public string RedirectUri { get; set; } = string.Empty;
    /// <summary>The requested scopes</summary>
    public List<string> Scopes { get; set; } = new() 
    { 
        "openid", 
        "profile", 
        "email" 
    };
}