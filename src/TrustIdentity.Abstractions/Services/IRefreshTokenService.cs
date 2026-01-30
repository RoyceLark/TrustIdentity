using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for managing refresh tokens
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Creates a new refresh token based on the creation request
    /// </summary>
    /// <param name="request">The token creation request</param>
    /// <returns>A new refresh token object</returns>
    Task<RefreshToken> CreateRefreshTokenAsync(TokenCreationRequest request);
    
    /// <summary>
    /// Updates an existing refresh token (e.g. for sliding expiration)
    /// </summary>
    /// <param name="refreshToken">The existing refresh token</param>
    /// <param name="client">The client for whom the token is being updated</param>
    /// <returns>The updated refresh token object</returns>
    Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken refreshToken, Client client);
    
    /// <summary>
    /// Stores a refresh token and returns its handle/key
    /// </summary>
    /// <param name="refreshToken">The refresh token to store</param>
    /// <returns>The handle/key of the stored token</returns>
    Task<string> StoreRefreshTokenAsync(RefreshToken refreshToken);
    
    /// <summary>
    /// Retrieves a refresh token by its handle
    /// </summary>
    /// <param name="handle">The refresh token handle</param>
    /// <returns>The refresh token object or null if not found</returns>
    Task<RefreshToken?> GetRefreshTokenAsync(string handle);
    
    /// <summary>
    /// Marks a refresh token as consumed (for one-time use tokens)
    /// </summary>
    /// <param name="handle">The refresh token handle</param>
    /// <returns>A task representing the operation</returns>
    Task ConsumeRefreshTokenAsync(string handle);
}