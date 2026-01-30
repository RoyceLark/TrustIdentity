using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using System.Text.Json;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for handling CIBA (Client Initiated Backchannel Authentication) flows.
/// </summary>
public class CibaService
{
    private readonly IPersistedGrantStore _grantStore;
    private readonly ILogger<CibaService> _logger;

    /// <summary>
    /// Initializes a new instance of the CibaService
    /// </summary>
    public CibaService(IPersistedGrantStore grantStore, ILogger<CibaService> logger)
    {
        _grantStore = grantStore;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new backchannel authentication request
    /// </summary>
    /// <param name="request">The request details</param>
    /// <returns>The authentication request ID (auth_req_id)</returns>
    public async Task<string> CreateRequestAsync(BackchannelAuthenticationRequest request)
    {
        var authReqId = Guid.NewGuid().ToString("N");
        request.Id = authReqId;

        var grant = new PersistedGrant
        {
            Key = authReqId,
            Type = "ciba",
            SubjectId = request.SubjectId,
            ClientId = request.ClientId,
            CreationTime = request.CreatedAt,
            Expiration = request.ExpiresAt,
            Data = JsonSerializer.Serialize(request)
        };

        await _grantStore.StoreAsync(grant);
        _logger.LogInformation("Created CIBA request {AuthReqId} for client {ClientId}", authReqId, request.ClientId);
        return authReqId;
    }

    /// <summary>
    /// Retrieves a backchannel authentication request by ID
    /// </summary>
    /// <param name="authReqId">The authentication request ID</param>
    /// <returns>The request details, or null if not found or expired</returns>
    public async Task<BackchannelAuthenticationRequest?> GetRequestAsync(string authReqId)
    {
        var grant = await _grantStore.GetAsync(authReqId);
        if (grant == null || grant.Type != "ciba") return null;

        if (grant.Expiration < DateTime.UtcNow)
        {
            await _grantStore.RemoveAsync(authReqId);
            return null;
        }

        return JsonSerializer.Deserialize<BackchannelAuthenticationRequest>(grant.Data);
    }

    /// <summary>
    /// Updates an existing backchannel authentication request
    /// </summary>
    /// <param name="request">The updated request details</param>
    public async Task UpdateRequestAsync(BackchannelAuthenticationRequest request)
    {
        var grant = await _grantStore.GetAsync(request.Id);
        if (grant == null || grant.Type != "ciba") return;

        grant.Data = JsonSerializer.Serialize(request);
        await _grantStore.StoreAsync(grant);
    }

    /// <summary>
    /// Removes a backchannel authentication request
    /// </summary>
    /// <param name="authReqId">The authentication request ID to remove</param>
    public async Task RemoveRequestAsync(string authReqId)
    {
        await _grantStore.RemoveAsync(authReqId);
    }
}
