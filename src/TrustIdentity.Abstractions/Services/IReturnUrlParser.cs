using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Interface for handling return URL validation
/// </summary>
public interface IReturnUrlParser
{
    /// <summary>
    /// Parses a return URL
    /// </summary>
    /// <param name="returnUrl">The return URL</param>
    /// <returns></returns>
    Task<AuthorizationRequest?> ParseAsync(string returnUrl);

    /// <summary>
    /// Checks if a return URL is valid
    /// </summary>
    /// <param name="returnUrl">The return URL</param>
    /// <returns></returns>
    Task<bool> IsValidReturnUrlAsync(string returnUrl);
}

/// <summary>
/// Represents a parsed authorization request
/// </summary>
public class AuthorizationRequest
{
    /// <summary>
    /// Gets or sets the client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the redirect URI
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response type
    /// </summary>
    public string ResponseType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scope
    /// </summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nonce
    /// </summary>
    public string? Nonce { get; set; }
}
