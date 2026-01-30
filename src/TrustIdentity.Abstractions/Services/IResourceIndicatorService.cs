using System.Collections.Generic;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for handling resource indicators (RFC 8707)
/// </summary>
public interface IResourceIndicatorService
{
    /// <summary>
    /// Validates resource indicators in an authorization request
    /// </summary>
    /// <param name="requestedResources">List of resource indicators</param>
    /// <param name="requestedScopes">List of requested scopes</param>
    /// <returns>Validation result</returns>
    Task<ResourceValidationResult> ValidateResourcesAsync(
        IEnumerable<string> requestedResources,
        IEnumerable<string> requestedScopes);

    /// <summary>
    /// Gets the audience claims for the specified resources
    /// </summary>
    /// <param name="resources">List of resource indicators</param>
    /// <returns>List of audience values</returns>
    Task<List<string>> GetAudiencesAsync(IEnumerable<string> resources);
}
