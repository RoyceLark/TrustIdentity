using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Implementation of token revocation service using distributed cache
/// </summary>
public class TokenRevocationService : ITokenRevocationService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<TokenRevocationService> _logger;
    private const string RevokedTokenPrefix = "revoked:token:";
    private const string RevokedUserPrefix = "revoked:user:";

    /// <summary>
    /// Initializes a new instance of the TokenRevocationService
    /// </summary>
    public TokenRevocationService(
        IDistributedCache cache,
        ILogger<TokenRevocationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Revokes a token by its JTI (JWT ID)
    /// </summary>
    public async Task<bool> RevokeTokenAsync(string jti, DateTime expiresAt)
    {
        try
        {
            var key = $"{RevokedTokenPrefix}{jti}";
            var value = JsonSerializer.Serialize(new RevokedToken
            {
                Jti = jti,
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            });

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiresAt
            };

            await _cache.SetStringAsync(key, value, options);
            _logger.LogWarning("Token revoked: {Jti}", jti);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke token: {Jti}", jti);
            return false;
        }
    }

    /// <summary>
    /// Checks if a token is revoked
    /// </summary>
    public async Task<bool> IsRevokedAsync(string jti)
    {
        try
        {
            var key = $"{RevokedTokenPrefix}{jti}";
            var value = await _cache.GetStringAsync(key);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check token revocation status: {Jti}", jti);
            // Fail secure: if we can't check, assume it's revoked
            return true;
        }
    }

    /// <summary>
    /// Revokes all tokens for a specific user
    /// </summary>
    public async Task<int> RevokeUserTokensAsync(string subjectId)
    {
        try
        {
            var key = $"{RevokedUserPrefix}{subjectId}";
            var value = JsonSerializer.Serialize(new RevokedUser
            {
                SubjectId = subjectId,
                RevokedAt = DateTime.UtcNow
            });

            // Store user revocation with 1 year expiration
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365)
            };

            await _cache.SetStringAsync(key, value, options);
            _logger.LogWarning("All tokens revoked for user: {SubjectId}", subjectId);
            return 1; // Return 1 to indicate success
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke user tokens: {SubjectId}", subjectId);
            return 0;
        }
    }

    /// <summary>
    /// Cleans up expired revoked tokens
    /// </summary>
    public Task<int> CleanupExpiredTokensAsync()
    {
        // Distributed cache automatically removes expired entries
        // This method is here for interface compatibility and future enhancements
        _logger.LogInformation("Token cleanup triggered (handled automatically by distributed cache)");
        return Task.FromResult(0);
    }

    private class RevokedToken
    {
        public string Jti { get; set; } = string.Empty;
        public DateTime RevokedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    private class RevokedUser
    {
        public string SubjectId { get; set; } = string.Empty;
        public DateTime RevokedAt { get; set; }
    }
}
