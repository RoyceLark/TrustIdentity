using System.Collections.Generic;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Interface for handling token creation
/// </summary>
public interface ITokenCreationService
{
    /// <summary>
    /// Creates a token
    /// </summary>
    /// <param name="token">The token descriptor</param>
    /// <returns></returns>
    Task<string> CreateTokenAsync(Token token);
}
