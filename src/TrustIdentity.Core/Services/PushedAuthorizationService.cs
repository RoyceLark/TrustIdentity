using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for handling Pushed Authorization Requests (PAR) - RFC 9126
/// </summary>
public class PushedAuthorizationService : IPushedAuthorizationService
{
    private readonly ILogger<PushedAuthorizationService> _logger;
    private readonly Dictionary<string, PushedAuthorizationRequest> _requestStore = new();
    private readonly int _requestUriLifetime = 60; // 60 seconds as per RFC

    /// <summary>
    /// Initializes a new instance of PushedAuthorizationService
    /// </summary>
    public PushedAuthorizationService(ILogger<PushedAuthorizationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Stores a pushed authorization request and returns a request URI
    /// </summary>
    public async Task<PushedAuthorizationResponse> StorePushedRequestAsync(
        Dictionary<string, string> parameters,
        string clientId)
    {
        // Generate a unique request URI
        var requestUri = GenerateRequestUri();
        
        var request = new PushedAuthorizationRequest
        {
            RequestUri = requestUri,
            Parameters = parameters,
            ClientId = clientId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(_requestUriLifetime)
        };

        _requestStore[requestUri] = request;
        
        _logger.LogInformation("Stored PAR request {RequestUri} for client {ClientId}", requestUri, clientId);

        // Clean up expired requests
        await CleanupExpiredRequestsAsync();

        return new PushedAuthorizationResponse
        {
            RequestUri = requestUri,
            ExpiresIn = _requestUriLifetime
        };
    }

    /// <summary>
    /// Retrieves and consumes a pushed authorization request
    /// </summary>
    public Task<PushedAuthorizationRequest?> GetAndRemoveRequestAsync(string requestUri)
    {
        if (_requestStore.TryGetValue(requestUri, out var request))
        {
            if (request.ExpiresAt > DateTime.UtcNow)
            {
                _requestStore.Remove(requestUri);
                _logger.LogInformation("Retrieved and consumed PAR request {RequestUri}", requestUri);
                return Task.FromResult<PushedAuthorizationRequest?>(request);
            }
            else
            {
                _requestStore.Remove(requestUri);
                _logger.LogWarning("PAR request {RequestUri} has expired", requestUri);
                return Task.FromResult<PushedAuthorizationRequest?>(null);
            }
        }

        _logger.LogWarning("PAR request {RequestUri} not found", requestUri);
        return Task.FromResult<PushedAuthorizationRequest?>(null);
    }

    /// <summary>
    /// Generates a cryptographically secure request URI
    /// </summary>
    private string GenerateRequestUri()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        var requestId = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
        
        return $"urn:ietf:params:oauth:request_uri:{requestId}";
    }

    /// <summary>
    /// Removes expired requests from the store
    /// </summary>
    private Task CleanupExpiredRequestsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = new List<string>();

        foreach (var kvp in _requestStore)
        {
            if (kvp.Value.ExpiresAt <= now)
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            _requestStore.Remove(key);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {Count} expired PAR requests", expiredKeys.Count);
        }

        return Task.CompletedTask;
    }
}
