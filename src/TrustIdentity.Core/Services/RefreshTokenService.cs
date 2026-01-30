using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using TrustIdentity.Abstractions.Configuration;
namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for managing refresh tokens
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly IPersistedGrantStore _grantStore;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly TrustIdentityOptions _options;

    /// <summary>
    /// Initializes a new instance of the RefreshTokenService
    /// </summary>
    /// <param name="grantStore">The persisted grant store</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="options">The TrustIdentity options</param>
    public RefreshTokenService(
        IPersistedGrantStore grantStore,
        ILogger<RefreshTokenService> logger,
        TrustIdentityOptions options)
    {
        _grantStore = grantStore;
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Creates a new refresh token
    /// </summary>
    /// <param name="request">The token creation request</param>
    /// <returns>A new refresh token instance</returns>
    public async Task<RefreshToken> CreateRefreshTokenAsync(TokenCreationRequest request)
    {
        var refreshToken = new RefreshToken
        {
            ClientId = request.ClientId,
            CreationTime = DateTime.UtcNow,
            Lifetime = _options.Authentication.RefreshTokenLifetime,
            AuthorizedScopes = request.ValidatedScopes.ToList()
        };

        if (request.Subject != null)
        {
            refreshToken.OriginalSubjectId = request.Subject.FindFirst("sub")?.Value ?? string.Empty;
            refreshToken.SessionId = request.SessionId;
        }

        return await Task.FromResult(refreshToken);
    }

    /// <summary>
    /// Updates a refresh token during a refresh request
    /// </summary>
    /// <param name="refreshToken">The existing refresh token</param>
    /// <param name="client">The client</param>
    /// <returns>The updated refresh token</returns>
    public async Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken refreshToken, Client client)
    {
        refreshToken.Updated = DateTime.UtcNow;
        
        // For one-time use tokens, create a new one
        var newToken = new RefreshToken
        {
            ClientId = refreshToken.ClientId,
            OriginalSubjectId = refreshToken.OriginalSubjectId,
            SessionId = refreshToken.SessionId,
            AuthorizedScopes = refreshToken.AuthorizedScopes,
            CreationTime = DateTime.UtcNow,
            Lifetime = refreshToken.Lifetime
        };
        
        return await Task.FromResult(newToken);
    }

    /// <summary>
    /// Stores the refresh token in the persisted store
    /// </summary>
    /// <param name="refreshToken">The refresh token to store</param>
    /// <returns>The handle for the stored token</returns>
    public async Task<string> StoreRefreshTokenAsync(RefreshToken refreshToken)
    {
        var handle = GenerateHandle();
        
        var grant = new PersistedGrant
        {
            Key = handle,
            Type = "refresh_token",
            SubjectId = refreshToken.OriginalSubjectId,
            SessionId = refreshToken.SessionId,
            ClientId = refreshToken.ClientId,
            CreationTime = refreshToken.CreationTime,
            Expiration = refreshToken.CreationTime.AddSeconds(refreshToken.Lifetime),
            Data = System.Text.Json.JsonSerializer.Serialize(refreshToken)
        };

        await _grantStore.StoreAsync(grant);
        
        _logger.LogDebug("Stored refresh token for client {ClientId}", refreshToken.ClientId);
        return handle;
    }

    /// <summary>
    /// Retrieves a refresh token by its handle
    /// </summary>
    /// <param name="handle">The handle</param>
    /// <returns>The refresh token if found and valid; otherwise null</returns>
    public async Task<RefreshToken?> GetRefreshTokenAsync(string handle)
    {
        var grant = await _grantStore.GetAsync(handle);
        
        if (grant == null)
        {
            _logger.LogWarning("Refresh token not found");
            return null;
        }

        if (grant.Expiration.HasValue && grant.Expiration < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expired");
            await _grantStore.RemoveAsync(handle);
            return null;
        }

        if (grant.ConsumedTime.HasValue)
        {
            _logger.LogWarning("Refresh token already consumed");
            return null;
        }

        var refreshToken = System.Text.Json.JsonSerializer.Deserialize<RefreshToken>(grant.Data);
        return refreshToken;
    }

    /// <summary>
    /// Marks a refresh token as consumed
    /// </summary>
    /// <param name="handle">The handle to consume</param>
    /// <returns>A task representing the operation</returns>
    public async Task ConsumeRefreshTokenAsync(string handle)
    {
        var grant = await _grantStore.GetAsync(handle);
        if (grant != null)
        {
            grant.ConsumedTime = DateTime.UtcNow;
            await _grantStore.StoreAsync(grant);
            _logger.LogDebug("Refresh token consumed");
        }
    }

    private string GenerateHandle()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}