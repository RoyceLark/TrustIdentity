using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Interface for handling CORS policy
/// </summary>
public interface ICorsPolicyService
{
    /// <summary>
    /// Determines whether origin is allowed
    /// </summary>
    /// <param name="origin">The origin</param>
    /// <returns></returns>
    Task<bool> IsOriginAllowedAsync(string origin);
}
