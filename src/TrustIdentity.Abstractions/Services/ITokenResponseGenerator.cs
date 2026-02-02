using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Interface for customizing token responses
/// </summary>
public interface ITokenResponseGenerator
{
    /// <summary>
    /// Processes the response
    /// </summary>
    /// <param name="request">The validated token request</param>
    /// <returns></returns>
    Task<TokenResponse> ProcessAsync(ValidatedTokenRequest request);
}

/// <summary>
/// Token response
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Gets or sets the access token
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the identity token
    /// </summary>
    public string? IdentityToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the token type
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the expires in (seconds)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the scope
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Gets or sets custom parameters to include in the response
    /// </summary>
    public Dictionary<string, object> Custom { get; set; } = new Dictionary<string, object>();
}
