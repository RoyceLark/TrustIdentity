using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Interface for customizing the consent workflow
/// </summary>
public interface IConsentService
{
    /// <summary>
    /// Checks if consent is required
    /// </summary>
    /// <param name="subject">The subject</param>
    /// <param name="clientId">The client ID</param>
    /// <param name="scopes">The requested scopes</param>
    /// <returns></returns>
    Task<bool> RequiresConsentAsync(string subject, string clientId, IEnumerable<string> scopes);

    /// <summary>
    /// Updates consent
    /// </summary>
    /// <param name="subject">The subject</param>
    /// <param name="clientId">The client ID</param>
    /// <param name="scopes">The consented scopes</param>
    /// <returns></returns>
    Task UpdateConsentAsync(string subject, string clientId, IEnumerable<string> scopes);

    /// <summary>
    /// Gets previously granted consent
    /// </summary>
    /// <param name="subject">The subject</param>
    /// <param name="clientId">The client ID</param>
    /// <returns></returns>
    Task<IEnumerable<string>?> GetPreviousConsentAsync(string subject, string clientId);
}
