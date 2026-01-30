using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.AspNetCore.Services;

namespace TrustIdentity.AspNetCore.Middleware;

/// <summary>
/// Middleware that resolves and sets the current tenant for each request
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the TenantResolutionMiddleware
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">The logger</param>
    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <param name="tenantResolver">The tenant resolver</param>
    /// <param name="tenantContext">The tenant context</param>
    public async Task InvokeAsync(
        HttpContext httpContext,
        ITenantResolver tenantResolver,
        ITenantContext tenantContext)
    {
        try
        {
            var tenant = await tenantResolver.ResolveAsync(httpContext);
            
            if (tenant != null)
            {
                tenantContext.SetTenant(tenant);
                _logger.LogDebug("Resolved tenant {TenantId} for request {Path}", 
                    tenant.Id, httpContext.Request.Path);
                
                // Add tenant ID to response headers for debugging
                httpContext.Response.Headers["X-Tenant-Id"] = tenant.Id;
            }
            else
            {
                _logger.LogDebug("No tenant resolved for request {Path}", httpContext.Request.Path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant for request {Path}", httpContext.Request.Path);
        }

        await _next(httpContext);
    }
}
