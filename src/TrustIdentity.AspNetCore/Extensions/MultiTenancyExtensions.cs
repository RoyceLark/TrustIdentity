using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.AspNetCore.Middleware;
using TrustIdentity.AspNetCore.Services;
using TrustIdentity.Core.Services;
using TrustIdentity.Storage.Stores;

namespace TrustIdentity.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring multi-tenancy
/// </summary>
public static class MultiTenancyExtensions
{
    /// <summary>
    /// Adds multi-tenancy support to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="strategies">Optional list of tenant resolution strategies to use</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMultiTenancy(
        this IServiceCollection services,
        List<TenantResolutionStrategy>? strategies = null)
    {
        // Register tenant context as scoped (per-request)
        services.AddScoped<ITenantContext, TenantContext>();

        // Register tenant store
        services.AddScoped<ITenantStore, EntityFrameworkTenantStore>();

        // Register tenant resolver
        services.AddScoped<ITenantResolver>(sp =>
        {
            var tenantStore = sp.GetRequiredService<ITenantStore>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CompositeTenantResolver>>();
            return new CompositeTenantResolver(tenantStore, logger, strategies);
        });

        return services;
    }

    /// <summary>
    /// Adds the tenant resolution middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
