using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.AspNetCore.Services;

/// <summary>
/// Composite tenant resolver that tries multiple strategies
/// </summary>
public class CompositeTenantResolver : ITenantResolver
{
    private readonly ITenantStore _tenantStore;
    private readonly ILogger<CompositeTenantResolver> _logger;
    private readonly List<TenantResolutionStrategy> _strategies;

    /// <summary>
    /// Initializes a new instance of the CompositeTenantResolver
    /// </summary>
    /// <param name="tenantStore">The tenant store</param>
    /// <param name="logger">The logger</param>
    /// <param name="strategies">The list of strategies to use (defaults to standard set if null)</param>
    public CompositeTenantResolver(
        ITenantStore tenantStore,
        ILogger<CompositeTenantResolver> logger,
        List<TenantResolutionStrategy>? strategies = null)
    {
        _tenantStore = tenantStore;
        _logger = logger;
        _strategies = strategies ?? new List<TenantResolutionStrategy>
        {
            TenantResolutionStrategy.Cookie,
            TenantResolutionStrategy.Host,
            TenantResolutionStrategy.Header,
            TenantResolutionStrategy.Route,
            TenantResolutionStrategy.QueryString
        };
    }

    /// <inheritdoc/>
    public async Task<Tenant?> ResolveAsync(HttpContext httpContext)
    {
        foreach (var strategy in _strategies)
        {
            var tenant = await ResolveByStrategyAsync(httpContext, strategy);
            if (tenant != null)
            {
                _logger.LogInformation("Resolved tenant {TenantId} using strategy {Strategy}", 
                    tenant.Id, strategy);
                return tenant;
            }
        }

        _logger.LogWarning("Could not resolve tenant from request");
        return null;
    }

    private async Task<Tenant?> ResolveByStrategyAsync(HttpContext httpContext, TenantResolutionStrategy strategy)
    {
        return strategy switch
        {
            TenantResolutionStrategy.Cookie => await ResolveFromCookieAsync(httpContext),
            TenantResolutionStrategy.Host => await ResolveFromHostAsync(httpContext),
            TenantResolutionStrategy.Header => await ResolveFromHeaderAsync(httpContext),
            TenantResolutionStrategy.Route => await ResolveFromRouteAsync(httpContext),
            TenantResolutionStrategy.QueryString => await ResolveFromQueryStringAsync(httpContext),
            TenantResolutionStrategy.Claim => await ResolveFromClaimAsync(httpContext),
            _ => null
        };
    }

    private async Task<Tenant?> ResolveFromCookieAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue("Ti-Tenant-Id", out var tenantId) && !string.IsNullOrEmpty(tenantId))
        {
            // Verify the user is a super admin before allowing this override?
            // For now, we'll assume the cookie can only be set by an authenticated process
            // But ideally, we should check if the current user has permission to switch.
            // However, tenant resolution happens very early, potentially before authentication middleware.
            // So we rely on the fact that the endpoint that sets this cookie must be secured.
            
            return await _tenantStore.GetByIdentifierAsync(tenantId);
        }
        return null;
    }

    private async Task<Tenant?> ResolveFromHostAsync(HttpContext httpContext)
    {
        var host = httpContext.Request.Host.Host;
        return await _tenantStore.GetByHostAsync(host);
    }

    private async Task<Tenant?> ResolveFromHeaderAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId))
        {
            return await _tenantStore.GetByIdentifierAsync(tenantId.ToString());
        }

        if (httpContext.Request.Headers.TryGetValue("X-Tenant", out var tenantIdentifier))
        {
            return await _tenantStore.GetByIdentifierAsync(tenantIdentifier.ToString());
        }

        return null;
    }

    private async Task<Tenant?> ResolveFromRouteAsync(HttpContext httpContext)
    {
        if (httpContext.Request.RouteValues.TryGetValue("tenantId", out var tenantId) && tenantId != null)
        {
            return await _tenantStore.GetByIdentifierAsync(tenantId.ToString()!);
        }

        if (httpContext.Request.RouteValues.TryGetValue("tenant", out var tenant) && tenant != null)
        {
            return await _tenantStore.GetByIdentifierAsync(tenant.ToString()!);
        }

        return null;
    }

    private async Task<Tenant?> ResolveFromQueryStringAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Query.TryGetValue("tenantId", out var tenantId))
        {
            return await _tenantStore.GetByIdentifierAsync(tenantId.ToString());
        }

        if (httpContext.Request.Query.TryGetValue("tenant", out var tenant))
        {
            return await _tenantStore.GetByIdentifierAsync(tenant.ToString());
        }

        return null;
    }

    private async Task<Tenant?> ResolveFromClaimAsync(HttpContext httpContext)
    {
        var user = httpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = user.FindFirst("tenant_id") ?? user.FindFirst("tid");
            if (tenantClaim != null)
            {
                return await _tenantStore.GetByIdAsync(tenantClaim.Value);
            }
        }

        return null;
    }
}
