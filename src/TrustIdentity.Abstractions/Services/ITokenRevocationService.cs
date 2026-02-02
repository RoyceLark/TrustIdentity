using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for managing token revocation
/// </summary>
public interface ITokenRevocationService
{
    /// <summary>
    /// Revokes a token by its JTI (JWT ID)
    /// </summary>
    /// <param name="jti">The JWT ID to revoke</param>
    /// <param name="expiresAt">When the token expires (for cleanup)</param>
    /// <returns>True if revocation was successful</returns>
    Task<bool> RevokeTokenAsync(string jti, System.DateTime expiresAt);

    /// <summary>
    /// Checks if a token is revoked
    /// </summary>
    /// <param name="jti">The JWT ID to check</param>
    /// <returns>True if the token is revoked</returns>
    Task<bool> IsRevokedAsync(string jti);

    /// <summary>
    /// Revokes all tokens for a specific user
    /// </summary>
    /// <param name="subjectId">The user's subject ID</param>
    /// <returns>Number of tokens revoked</returns>
    Task<int> RevokeUserTokensAsync(string subjectId);

    /// <summary>
    /// Cleans up expired revoked tokens
    /// </summary>
    /// <returns>Number of tokens cleaned up</returns>
    Task<int> CleanupExpiredTokensAsync();
}
