using TrustIdentity.Core.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for storing and retrieving reference tokens
/// </summary>
public class ReferenceTokenStore
{
    private readonly IPersistedGrantStore _grantStore;
    private readonly ILogger<ReferenceTokenStore> _logger;

    /// <summary>
    /// Initializes a new instance of the ReferenceTokenStore
    /// </summary>
    /// <param name="grantStore">The persisted grant store</param>
    /// <param name="logger">The logger instance</param>
    public ReferenceTokenStore(
        IPersistedGrantStore grantStore,
        ILogger<ReferenceTokenStore> logger)
    {
        _grantStore = grantStore;
        _logger = logger;
    }

    /// <summary>
    /// Stores a token as a reference token and returns a handle
    /// </summary>
    /// <param name="token">The token to store</param>
    /// <returns>The reference token handle</returns>
    public async Task<string> StoreReferenceTokenAsync(Abstractions.Models.Token token)
    {
        var handle = GenerateHandle();
        
        var grant = new PersistedGrant
        {
            Key = handle,
            Type = "reference_token",
            SubjectId = token.Subject?.FindFirst("sub")?.Value ?? string.Empty,
            ClientId = token.ClientId,
            Description = token.Description,
            CreationTime = token.CreationTime,
            Expiration = token.CreationTime.AddSeconds(token.Lifetime),
            Data = System.Text.Json.JsonSerializer.Serialize(token)
        };

        await _grantStore.StoreAsync(grant);
        
        _logger.LogDebug("Stored reference token for client {ClientId}", token.ClientId);
        return handle;
    }

    /// <summary>
    /// Retrieves a token by its reference handle
    /// </summary>
    /// <param name="handle">The reference handle</param>
    /// <returns>The token if found and valid; otherwise null</returns>
    public async Task<Abstractions.Models.Token?> GetReferenceTokenAsync(string handle)
    {
        var grant = await _grantStore.GetAsync(handle);
        
        if (grant == null)
        {
            _logger.LogWarning("Reference token not found");
            return null;
        }

        if (grant.Expiration.HasValue && grant.Expiration < DateTime.UtcNow)
        {
            _logger.LogWarning("Reference token expired");
            await _grantStore.RemoveAsync(handle);
            return null;
        }

        var token = System.Text.Json.JsonSerializer.Deserialize<Abstractions.Models.Token>(grant.Data);
        return token;
    }

    /// <summary>
    /// Removes a reference token from the store
    /// </summary>
    /// <param name="handle">The reference handle to remove</param>
    /// <returns>A task representing the operation</returns>
    public async Task RemoveReferenceTokenAsync(string handle)
    {
        await _grantStore.RemoveAsync(handle);
        _logger.LogDebug("Reference token removed");
    }

    private string GenerateHandle()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}