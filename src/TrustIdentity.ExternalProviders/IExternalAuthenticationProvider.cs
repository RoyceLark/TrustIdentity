using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
namespace TrustIdentity.ExternalProviders;

/// <summary>
/// Interface for external authentication providers
/// </summary>
public interface IExternalAuthenticationProvider
{
    /// <summary>The unique provider name</summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the URL to redirect the user to for authentication
    /// </summary>
    /// <param name="state">The state parameter for CSRF protection</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <returns>The authorization URL</returns>
    string GetAuthorizationUrl(string state, string? redirectUri = null);
    
    /// <summary>
    /// Authenticates the user using the provided authorization code
    /// </summary>
    /// <param name="code">The authorization code</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <returns>A result object containing the user info and tokens</returns>
    Task<ExternalAuthenticationResult> AuthenticateAsync(string code, string? redirectUri = null);
    
    /// <summary>
    /// Refreshes the external access token
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>A result object with the new tokens</returns>
    Task<ExternalAuthenticationResult> RefreshTokenAsync(string refreshToken);
}

/// <summary>
/// Result of external authentication
/// </summary>
public class ExternalAuthenticationResult
{
    /// <summary>Whether successfully authenticated</summary>
    public bool Success { get; set; }
    /// <summary>The provider name</summary>
    public string? Provider { get; set; }
    /// <summary>The user ID from the provider</summary>
    public string? ProviderUserId { get; set; }
    /// <summary>The user's email</summary>
    public string? Email { get; set; }
    /// <summary>The user's display name</summary>
    public string? DisplayName { get; set; }
    /// <summary>The provider access token</summary>
    public string? AccessToken { get; set; }
    /// <summary>The provider refresh token</summary>
    public string? RefreshToken { get; set; }
    /// <summary>When the token expires</summary>
    public DateTime? TokenExpiration { get; set; }
    /// <summary>The user's claims from the provider</summary>
    public List<Claim> Claims { get; set; } = new();
    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }
    /// <summary>Error code if failed</summary>
    public string? ErrorCode { get; set; }
}

/// <summary>
/// External provider exception
/// </summary>
public class ExternalProviderException : Exception
{
    /// <summary>Initializes a new instance of the ExternalProviderException</summary>
    /// <param name="message">The message</param>
    public ExternalProviderException(string message) : base(message) { }
    /// <summary>Initializes a new instance of the ExternalProviderException with inner exception</summary>
    /// <param name="message">The message</param>
    /// <param name="inner">The inner exception</param>
    public ExternalProviderException(string message, Exception inner) : base(message, inner) { }
}