using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Interface for customizing claims in tokens
/// </summary>
public interface IClaimsService
{
    /// <summary>
    /// Returns claims for an identity token
    /// </summary>
    /// <param name="subject">The subject</param>
    /// <param name="clientId">The client ID</param>
    /// <param name="scopes">The requested scopes</param>
    /// <param name="includeAllIdentityClaims">Whether to include all identity claims</param>
    /// <returns></returns>
    Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(
        ClaimsPrincipal subject,
        string clientId,
        IEnumerable<string> scopes,
        bool includeAllIdentityClaims);

    /// <summary>
    /// Returns claims for an access token
    /// </summary>
    /// <param name="subject">The subject</param>
    /// <param name="clientId">The client ID</param>
    /// <param name="scopes">The requested scopes</param>
    /// <returns></returns>
    Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(
        ClaimsPrincipal subject,
        string clientId,
        IEnumerable<string> scopes);
}
