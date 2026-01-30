using TrustIdentity.Abstractions.Stores;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for validating CORS (Cross-Origin Resource Sharing) policies
/// </summary>
public class CorsPolicyService
{
    private readonly IClientStore _clientStore;
    private readonly ILogger<CorsPolicyService> _logger;

    /// <summary>
    /// Initializes a new instance of the CorsPolicyService
    /// </summary>
    /// <param name="clientStore">The client store</param>
    /// <param name="logger">The logger instance</param>
    public CorsPolicyService(IClientStore clientStore, ILogger<CorsPolicyService> logger)
    {
        _clientStore = clientStore;
        _logger = logger;
    }

    /// <summary>
    /// Checks if an origin is allowed based on client configuration
    /// </summary>
    /// <param name="origin">The origin string to check</param>
    /// <returns>True if the origin is allowed; otherwise false</returns>
    public async Task<bool> IsOriginAllowedAsync(string origin)
    {
        var clients = await _clientStore.GetAllClientsAsync();
        
        foreach (var client in clients)
        {
            if (client.AllowedCorsOrigins.Contains(origin))
            {
                _logger.LogDebug("Origin {Origin} allowed for client {ClientId}", origin, client.ClientId);
                return true;
            }
        }
        
        _logger.LogWarning("Origin {Origin} not allowed", origin);
        return false;
    }
}