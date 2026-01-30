using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
namespace TrustIdentity.ExternalProviders.Facebook;

/// <summary>
/// Facebook External Authentication Provider
/// </summary>
public class FacebookProvider : IExternalAuthenticationProvider
{
    private readonly FacebookConfiguration _config;
    private readonly ILogger<FacebookProvider> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the FacebookProvider
    /// </summary>
    /// <param name="config">The Facebook configuration</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="httpClientFactory">The HTTP client factory</param>
    public FacebookProvider(
        FacebookConfiguration config,
        ILogger<FacebookProvider> logger,
        IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Facebook");
    }

    /// <summary>The provider name</summary>
    public string ProviderName => "Facebook";

    /// <summary>
    /// Gets the authorization URL for Facebook
    /// </summary>
    /// <param name="state">The state parameter</param>
    /// <param name="redirectUri">The redirect URI</param>
    /// <returns>The authorization URL</returns>
    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        var actualRedirectUri = redirectUri ?? _config.RedirectUri;
        
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _config.AppId,
            ["redirect_uri"] = actualRedirectUri,
            ["state"] = state,
            ["scope"] = string.Join(",", _config.Scopes)
        };

        var queryString = string.Join("&", parameters.Select(kvp => 
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"https://www.facebook.com/v18.0/dialog/oauth?{queryString}";
    }

    /// <summary>
    /// Authenticates with Facebook using an authorization code
    /// </summary>
    /// <param name="code">The authorization code</param>
    /// <param name="redirectUri">The redirect URI</param>
    /// <returns>The authentication result</returns>
    public async Task<ExternalAuthenticationResult> AuthenticateAsync(string code, string? redirectUri = null)
    {
        try
        {
            var actualRedirectUri = redirectUri ?? _config.RedirectUri;

            // Exchange code for token
            var tokenUrl = $"https://graph.facebook.com/v18.0/oauth/access_token?" +
                $"client_id={_config.AppId}&" +
                $"client_secret={_config.AppSecret}&" +
                $"code={code}&" +
                $"redirect_uri={Uri.EscapeDataString(actualRedirectUri)}";

            var tokenResponse = await _httpClient.GetAsync(tokenUrl);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return new ExternalAuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to exchange authorization code"
                };
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);
            var accessToken = tokenData.GetProperty("access_token").GetString()!;

            // Get user info
            var fields = "id,name,email,first_name,last_name,picture";
            var userInfoUrl = $"https://graph.facebook.com/v18.0/me?fields={fields}&access_token={accessToken}";
            
            var userInfoResponse = await _httpClient.GetAsync(userInfoUrl);
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
                new Claim(ClaimTypes.Name, userInfo.GetProperty("name").GetString()!)
            };

            if (userInfo.TryGetProperty("email", out var email))
                claims.Add(new Claim(ClaimTypes.Email, email.GetString()!));

            if (userInfo.TryGetProperty("first_name", out var firstName))
                claims.Add(new Claim(ClaimTypes.GivenName, firstName.GetString()!));

            if (userInfo.TryGetProperty("last_name", out var lastName))
                claims.Add(new Claim(ClaimTypes.Surname, lastName.GetString()!));

            return new ExternalAuthenticationResult
            {
                Success = true,
                Provider = ProviderName,
                ProviderUserId = userInfo.GetProperty("id").GetString()!,
                Email = userInfo.TryGetProperty("email", out var e) ? e.GetString() : null,
                DisplayName = userInfo.GetProperty("name").GetString(),
                AccessToken = accessToken,
                Claims = claims
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facebook authentication failed");
            return new ExternalAuthenticationResult
            {
                Success = false,
                ErrorMessage = $"Facebook authentication failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Refreshes the Facebook access token (not supported)
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>A failed authentication result</returns>
    public Task<ExternalAuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        // Facebook doesn't support refresh tokens in the same way
        return Task.FromResult(new ExternalAuthenticationResult
        {
            Success = false,
            ErrorMessage = "Facebook does not support refresh tokens"
        });
    }
}

/// <summary>
/// Configuration for Facebook provider
/// </summary>
public class FacebookConfiguration
{
    /// <summary>The Facebook App ID</summary>
    public string AppId { get; set; } = string.Empty;
    /// <summary>The Facebook App Secret</summary>
    public string AppSecret { get; set; } = string.Empty;
    /// <summary>The redirect URI</summary>
    public string RedirectUri { get; set; } = string.Empty;
    /// <summary>The requested scopes</summary>
    public List<string> Scopes { get; set; } = new() { "email", "public_profile" };
}