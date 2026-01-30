using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
namespace TrustIdentity.ExternalProviders.GitHub;

/// <summary>
/// GitHub External Authentication Provider
/// </summary>
public class GitHubProvider : IExternalAuthenticationProvider
{
    private readonly GitHubConfiguration _config;
    private readonly ILogger<GitHubProvider> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the GitHubProvider
    /// </summary>
    /// <param name="config">The GitHub configuration</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="httpClientFactory">The HTTP client factory</param>
    public GitHubProvider(
        GitHubConfiguration config,
        ILogger<GitHubProvider> logger,
        IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("GitHub");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TrustIdentity");
    }

    /// <summary>The provider name</summary>
    public string ProviderName => "GitHub";

    /// <summary>
    /// Gets the authorization URL for GitHub
    /// </summary>
    /// <param name="state">The state parameter</param>
    /// <param name="redirectUri">The redirect URI</param>
    /// <returns>The authorization URL</returns>
    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        var actualRedirectUri = redirectUri ?? _config.RedirectUri;
        
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _config.ClientId,
            ["redirect_uri"] = actualRedirectUri,
            ["state"] = state,
            ["scope"] = string.Join(" ", _config.Scopes)
        };

        var queryString = string.Join("&", parameters.Select(kvp => 
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"https://github.com/login/oauth/authorize?{queryString}";
    }

    /// <summary>
    /// Authenticates with GitHub using an authorization code
    /// </summary>
    /// <param name="code">The authorization code</param>
    /// <param name="redirectUri">The redirect URI</param>
    /// <returns>The authentication result</returns>
    public async Task<ExternalAuthenticationResult> AuthenticateAsync(string code, string? redirectUri = null)
    {
        try
        {
            // Exchange code for token
            var tokenRequest = new Dictionary<string, string>
            {
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret,
                ["code"] = code,
                ["redirect_uri"] = redirectUri ?? _config.RedirectUri
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var tokenResponse = await _httpClient.PostAsync(
                "https://github.com/login/oauth/access_token",
                new FormUrlEncodedContent(tokenRequest));

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
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var userInfoResponse = await _httpClient.GetAsync("https://api.github.com/user");
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

            // Get email if not in user info
            string? email = null;
            if (userInfo.TryGetProperty("email", out var emailProp) && !emailProp.ValueKind.Equals(JsonValueKind.Null))
            {
                email = emailProp.GetString();
            }
            else
            {
                // Fetch from emails endpoint
                var emailResponse = await _httpClient.GetAsync("https://api.github.com/user/emails");
                if (emailResponse.IsSuccessStatusCode)
                {
                    var emailJson = await emailResponse.Content.ReadAsStringAsync();
                    var emails = JsonSerializer.Deserialize<JsonElement>(emailJson);
                    foreach (var e in emails.EnumerateArray())
                    {
                        if (e.GetProperty("primary").GetBoolean())
                        {
                            email = e.GetProperty("email").GetString();
                            break;
                        }
                    }
                }
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.GetProperty("id").GetInt64().ToString()),
                new Claim(ClaimTypes.Name, userInfo.GetProperty("login").GetString()!)
            };

            if (email != null)
                claims.Add(new Claim(ClaimTypes.Email, email));

            if (userInfo.TryGetProperty("name", out var name) && !name.ValueKind.Equals(JsonValueKind.Null))
                claims.Add(new Claim("full_name", name.GetString()!));

            if (userInfo.TryGetProperty("company", out var company) && !company.ValueKind.Equals(JsonValueKind.Null))
                claims.Add(new Claim("company", company.GetString()!));

            return new ExternalAuthenticationResult
            {
                Success = true,
                Provider = ProviderName,
                ProviderUserId = userInfo.GetProperty("id").GetInt64().ToString(),
                Email = email,
                DisplayName = userInfo.GetProperty("login").GetString(),
                AccessToken = accessToken,
                Claims = claims
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub authentication failed");
            return new ExternalAuthenticationResult
            {
                Success = false,
                ErrorMessage = $"GitHub authentication failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Refreshes the GitHub access token (not supported for OAuth Apps)
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>A failed authentication result</returns>
    public Task<ExternalAuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        return Task.FromResult(new ExternalAuthenticationResult
        {
            Success = false,
            ErrorMessage = "GitHub does not support refresh tokens for OAuth Apps"
        });
    }
}

/// <summary>
/// Configuration for GitHub provider
/// </summary>
public class GitHubConfiguration
{
    /// <summary>The GitHub Client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The GitHub Client Secret</summary>
    public string ClientSecret { get; set; } = string.Empty;
    /// <summary>The redirect URI</summary>
    public string RedirectUri { get; set; } = string.Empty;
    /// <summary>The requested scopes</summary>
    public List<string> Scopes { get; set; } = new() { "user:email", "read:user" };
}