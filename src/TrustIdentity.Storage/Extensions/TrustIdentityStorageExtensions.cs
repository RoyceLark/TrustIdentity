using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Storage.EntityFramework;
using TrustIdentity.Storage.Stores;

namespace TrustIdentity.Storage.Extensions;
/// <summary>
/// TrustIdentityStorageExtensions
/// </summary>
public static class TrustIdentityStorageExtensions
{
    /// <summary>
    /// AddTrustIdentityConfigurationStore
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionsAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddTrustIdentityConfigurationStore(
        this IServiceCollection services, 
        Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddDbContext<ConfigurationDbContext>(optionsAction);
        
        services.AddScoped<IClientStore, EntityFrameworkClientStore>();
        services.AddScoped<IResourceStore, EntityFrameworkResourceStore>();
        services.AddScoped<IApiScopeStore, EntityFrameworkApiScopeStore>();
        services.AddScoped<IApiResourceStore, EntityFrameworkApiResourceStore>();
        
        return services;
    }
    /// <summary>
    /// AddTrustIdentityOperationalStore
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionsAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddTrustIdentityOperationalStore(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddDbContext<PersistedGrantDbContext>(optionsAction);
        
        services.AddScoped<IPersistedGrantStore, EntityFrameworkPersistedGrantStore>();
        services.AddScoped<IAuditStore, EntityFrameworkAuditStore>();
        services.AddScoped<ISessionStore, EntityFrameworkSessionStore>();
        services.AddScoped<IConsentStore, EntityFrameworkConsentStore>();
        services.AddScoped<IDeviceFlowStore, EntityFrameworkDeviceFlowStore>();
        
        return services;
    }
    /// <summary>
    /// AddTrustIdentityUserStore
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionsAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddTrustIdentityUserStore(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddDbContext<TrustIdentityDbContext>(optionsAction);
        services.AddScoped<IUserStore, EntityFrameworkUserStore>();
        return services;
    }
}
