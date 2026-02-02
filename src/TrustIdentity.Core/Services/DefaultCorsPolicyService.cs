using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Default implementation of ICorsPolicyService
/// </summary>
public class DefaultCorsPolicyService : ICorsPolicyService
{
    private readonly IClientStore _clientStore;
    private readonly ILogger<DefaultCorsPolicyService> _logger;

    /// <summary>
    /// Initializes a new instance of the DefaultCorsPolicyService
    /// </summary>
    public DefaultCorsPolicyService(IClientStore clientStore, ILogger<DefaultCorsPolicyService> logger)
    {
        _clientStore = clientStore;
        _logger = logger;
    }

    /// <summary>
    /// Determines if an origin is allowed
    /// </summary>
    public async Task<bool> IsOriginAllowedAsync(string origin)
    {
        _logger.LogDebug("Checking if origin {Origin} is allowed", origin);

        // Get all clients and check their allowed CORS origins
        var clients = await _clientStore.GetAllClientsAsync();
        
        foreach (var client in clients)
        {
            if (client.AllowedCorsOrigins?.Contains(origin) == true)
            {
                _logger.LogDebug("Origin {Origin} is allowed for client {ClientId}", origin, client.ClientId);
                return true;
            }
        }

        _logger.LogDebug("Origin {Origin} is not allowed", origin);
        return false;
    }
}
