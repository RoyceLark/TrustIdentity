using System.Collections.Generic;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for creating and validating tokens (Access tokens, ID tokens, Refresh tokens)
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates an access token for the specified client and user
    /// </summary>
    /// <param name="client">The client requesting the token</param>
    /// <param name="user">The user for whom the token is being created</param>
    /// <param name="scopes">The requested scopes</param>
    /// <param name="dpopJkt">Optional DPoP JWK thumbprint for token binding</param>
    /// <returns>A token object</returns>
    Task<Token> CreateAccessTokenAsync(Client client, User user, IEnumerable<string> scopes, string? dpopJkt = null);
    
    /// <summary>
    /// Creates a refresh token for the specified client and user
    /// </summary>
    /// <param name="client">The client requesting the token</param>
    /// <param name="user">The user</param>
    /// <returns>A token object representing the refresh token</returns>
    Task<Token> CreateRefreshTokenAsync(Client client, User user);
    
    /// <summary>
    /// Generates a JWT string from a token object
    /// </summary>
    /// <param name="token">The token object</param>
    /// <returns>A signed JWT string</returns>
    Task<string> GenerateJwtAsync(Token token);
    
    /// <summary>
    /// Validates a token string
    /// </summary>
    /// <param name="token">The token string</param>
    /// <returns>True if valid; otherwise false</returns>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Validates a token and returns its claims
    /// </summary>
    Task<TokenValidationResultDetailed> ValidateTokenDetailedAsync(string token);
}

/// <summary>
/// Result of token validation with details
/// </summary>
public class TokenValidationResultDetailed
{
    /// <summary>Whether the token is valid</summary>
    public bool IsValid { get; set; }
    /// <summary>Error message if invalid</summary>
    public string? Error { get; set; }
    /// <summary>The claims if valid</summary>
    public System.Security.Claims.ClaimsPrincipal? Principal { get; set; }
}