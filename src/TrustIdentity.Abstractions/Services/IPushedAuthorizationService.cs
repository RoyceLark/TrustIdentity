using System.Collections.Generic;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for handling Pushed Authorization Requests (PAR) - RFC 9126
/// </summary>
public interface IPushedAuthorizationService
{
    /// <summary>
    /// Stores a pushed authorization request and returns a request URI
    /// </summary>
    /// <param name="parameters">Authorization request parameters</param>
    /// <param name="clientId">Client identifier</param>
    /// <returns>Response containing request URI and expiration</returns>
    Task<PushedAuthorizationResponse> StorePushedRequestAsync(
        Dictionary<string, string> parameters,
        string clientId);

    /// <summary>
    /// Retrieves and consumes a pushed authorization request
    /// </summary>
    /// <param name="requestUri">The request URI</param>
    /// <returns>The stored request or null if not found/expired</returns>
    Task<PushedAuthorizationRequest?> GetAndRemoveRequestAsync(string requestUri);
}
