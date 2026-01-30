using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Abstractions.Services
{
    /// <summary>
    /// Service for handling Dynamic Client Registration (RFC 7591)
    /// </summary>
    public interface IDynamicClientRegistrationService
    {
        /// <summary>
        /// Registers a new client dynamically
        /// </summary>
        /// <param name="request">The registration request</param>
        /// <returns>The registration response including the new client credentials</returns>
        Task<DynamicClientRegistrationResponse> RegisterClientAsync(DynamicClientRegistrationRequest request);
    }
}
