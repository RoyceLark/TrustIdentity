using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
namespace TrustIdentity.ExternalProviders.Azure;

/// <summary>
/// Azure AD / Microsoft Entra ID Provider
/// </summary>
public class AzureADProvider : IExternalAuthenticationProvider
{
    private readonly AzureADConfiguration _config;
    private readonly ILogger<AzureADProvider> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the AzureADProvider
    /// </summary>
    /// <param name="config">The Azure AD configuration</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="httpClientFactory">The HTTP client factory</param>
    public AzureADProvider(
        AzureADConfiguration config,
        ILogger<AzureADProvider> logger,
        IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("AzureAD");
    }

    /// <summary>
    /// The name of the provider
    /// </summary>
    public string ProviderName => "AzureAD";

    /// <summary>
    /// Get authorization URL for Azure AD
    /// </summary>
    /// <param name="state">The state parameter for CSRF protection</param>
    /// <param name="redirectUri">Optional redirect URI to override the default</param>
    /// <returns>The Azure AD authorization URL</returns>
    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        try
        {
            var actualRedirectUri = redirectUri ?? _config.RedirectUri;
            var authUrl = $"https://login.microsoftonline.com/{_config.TenantId}/oauth2/v2.0/authorize";
            
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = _config.ClientId,
                ["response_type"] = "code",
                ["redirect_uri"] = actualRedirectUri,
                ["response_mode"] = "query",
                ["scope"] = string.Join(" ", _config.Scopes),
                ["state"] = state
            };

            var queryString = string.Join("&", parameters.Select(kvp => 
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            return $"{authUrl}?{queryString}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Azure AD authorization URL");
            throw new ExternalProviderException("Failed to generate authorization URL", ex);
        }
    }

    /// <summary>
    /// Exchange authorization code for access token
    /// </summary>
    /// <param name="code">The authorization code received from Azure AD</param>
    /// <param name="redirectUri">Optional redirect URI to override the default</param>
    /// <returns>A result object containing user info and tokens</returns>
    public async Task<ExternalAuthenticationResult> AuthenticateAsync(string code, string? redirectUri = null)
    {
        try
        {
            _logger.LogInformation("Authenticating with Azure AD using authorization code");

            var actualRedirectUri = redirectUri ?? _config.RedirectUri;

            // Use MSAL for token acquisition
            var app = ConfidentialClientApplicationBuilder
                .Create(_config.ClientId)
                .WithClientSecret(_config.ClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_config.TenantId}"))
                .Build();

            var result = await app.AcquireTokenByAuthorizationCode(_config.Scopes, code)
                .ExecuteAsync();

            // Get user info from token claims
            var claims = ExtractClaims(result.ClaimsPrincipal);

            // Get additional user info from Microsoft Graph if needed
            if (_config.RetrieveUserInfo)
            {
                var additionalClaims = await GetUserInfoFromGraphAsync(result.AccessToken);
                claims.AddRange(additionalClaims);
            }

            return new ExternalAuthenticationResult
            {
                Success = true,
                Provider = ProviderName,
                ProviderUserId = result.UniqueId,
                Email = result.ClaimsPrincipal.FindFirst("preferred_username")?.Value 
                    ?? result.ClaimsPrincipal.FindFirst("email")?.Value,
                DisplayName = result.ClaimsPrincipal.FindFirst("name")?.Value,
                AccessToken = result.AccessToken,
                RefreshToken = null, // MSAL handles refresh tokens internally
                TokenExpiration = result.ExpiresOn.UtcDateTime,
                Claims = claims
            };
        }
        catch (MsalException ex)
        {
            _logger.LogError(ex, "Azure AD authentication failed: {ErrorCode}", ex.ErrorCode);
            return new ExternalAuthenticationResult
            {
                Success = false,
                ErrorMessage = $"Azure AD authentication failed: {ex.Message}",
                ErrorCode = ex.ErrorCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Azure AD authentication");
            return new ExternalAuthenticationResult
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred during authentication"
            };
        }
    }

    /// <summary>
    /// Get user information from Microsoft Graph API
    /// </summary>
    private async Task<List<Claim>> GetUserInfoFromGraphAsync(string accessToken)
    {
        var claims = new List<Claim>();

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<JsonElement>(json);

                if (userInfo.TryGetProperty("givenName", out var givenName))
                    claims.Add(new Claim(ClaimTypes.GivenName, givenName.GetString()!));

                if (userInfo.TryGetProperty("surname", out var surname))
                    claims.Add(new Claim(ClaimTypes.Surname, surname.GetString()!));

                if (userInfo.TryGetProperty("jobTitle", out var jobTitle))
                    claims.Add(new Claim("job_title", jobTitle.GetString()!));

                if (userInfo.TryGetProperty("department", out var department))
                    claims.Add(new Claim("department", department.GetString()!));

                if (userInfo.TryGetProperty("officeLocation", out var office))
                    claims.Add(new Claim("office", office.GetString()!));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve additional user info from Microsoft Graph");
        }

        return claims;
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="refreshToken">The refresh token (unused as MSAL handles it internally)</param>
    /// <returns>A result object containing the new access token</returns>
    public async Task<ExternalAuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var app = ConfidentialClientApplicationBuilder
                .Create(_config.ClientId)
                .WithClientSecret(_config.ClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_config.TenantId}"))
                .Build();

#pragma warning disable CS0618 // Type or member is obsolete
            var accounts = await app.GetAccountsAsync();
#pragma warning restore CS0618 // Type or member is obsolete
            var result = await app.AcquireTokenSilent(_config.Scopes, accounts.FirstOrDefault())
                .ExecuteAsync();

            return new ExternalAuthenticationResult
            {
                Success = true,
                Provider = ProviderName,
                AccessToken = result.AccessToken,
                RefreshToken = null, // MSAL handles refresh tokens internally
                TokenExpiration = result.ExpiresOn.UtcDateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh Azure AD token");
            return new ExternalAuthenticationResult
            {
                Success = false,
                ErrorMessage = "Failed to refresh token"
            };
        }
    }

    private List<Claim> ExtractClaims(ClaimsPrincipal principal)
    {
        var claims = new List<Claim>();

        foreach (var claim in principal.Claims)
        {
            claims.Add(new Claim(claim.Type, claim.Value));
        }

        return claims;
    }
}

/// <summary>
/// Azure AD Configuration
/// </summary>
public class AzureADConfiguration
{
    /// <summary>The Azure tenant ID</summary>
    public string TenantId { get; set; } = string.Empty;
    /// <summary>The Azure client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The Azure client secret</summary>
    public string ClientSecret { get; set; } = string.Empty;
    /// <summary>The redirect URI for the application</summary>
    public string RedirectUri { get; set; } = string.Empty;
    /// <summary>The requested OAuth 2.0 scopes</summary>
    public List<string> Scopes { get; set; } = new() { "openid", "profile", "email", "User.Read" };
    /// <summary>Whether to retrieve additional user info from MS Graph</summary>
    public bool RetrieveUserInfo { get; set; } = true;
}