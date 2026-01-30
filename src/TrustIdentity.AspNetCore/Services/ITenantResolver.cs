namespace TrustIdentity.AspNetCore.Services;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

/// <summary>
/// Resolves the current tenant from an HTTP request
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Resolves the tenant from the HTTP context
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <returns>The resolved tenant, or null if not found</returns>
    Task<Tenant?> ResolveAsync(HttpContext httpContext);
}

/// <summary>
/// Tenant resolution strategies
/// </summary>
public enum TenantResolutionStrategy
{
    /// <summary>
    /// Resolve tenant from the host/domain (e.g., tenant1.myapp.com)
    /// </summary>
    Host,

    /// <summary>
    /// Resolve tenant from HTTP header (e.g., X-Tenant-Id)
    /// </summary>
    Header,

    /// <summary>
    /// Resolve tenant from user claims (after authentication)
    /// </summary>
    Claim,

    /// <summary>
    /// Resolve tenant from route parameter (e.g., /tenants/{tenantId}/...)
    /// </summary>
    Route,

    /// <summary>
    /// Resolve tenant from query string (e.g., ?tenant=tenant1)
    /// </summary>
    QueryString,

    /// <summary>
    /// Resolve tenant from cookie (e.g., Ti-Tenant-Id)
    /// </summary>
    Cookie
}
