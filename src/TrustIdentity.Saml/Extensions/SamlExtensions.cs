using System;
using Microsoft.Extensions.DependencyInjection;
using TrustIdentity.Saml.Services;
using TrustIdentity.Saml.Serialization;
using TrustIdentity.Saml.Security;
using Microsoft.AspNetCore.Builder;
using TrustIdentity.Saml.Endpoints;
using System.Security.Cryptography.X509Certificates;

namespace TrustIdentity.Saml.Extensions;

/// <summary>
/// Service collection extensions for SAML
/// </summary>
public static class SamlServiceCollectionExtensions
{
    /// <summary>
    /// Add SAML Identity Provider services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">The configuration action</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddSamlIdentityProvider(
        this IServiceCollection services,
        Action<SamlIdentityProviderConfig> configureOptions)
    {
        var config = new SamlIdentityProviderConfig();
        configureOptions(config);

        services.AddSingleton(config);
        services.AddSingleton<SamlSerializer>();
        services.AddSingleton<SamlSigningService>();
        services.AddSingleton<SamlIdentityProvider>();

        return services;
    }

    /// <summary>
    /// Add SAML Service Provider services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">The configuration action</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddSamlServiceProvider(
        this IServiceCollection services,
        Action<SamlServiceProviderConfig> configureOptions)
    {
        var config = new SamlServiceProviderConfig();
        configureOptions(config);

        services.AddSingleton(config);
        services.AddSingleton<SamlSerializer>();
        services.AddSingleton<SamlSigningService>();
        services.AddSingleton<SamlServiceProvider>();

        return services;
    }
}

/// <summary>
/// Application builder extensions for SAML
/// </summary>
public static class SamlApplicationBuilderExtensions
{
    /// <summary>
    /// Map SAML Identity Provider endpoints
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseSamlIdentityProvider(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapPost("/saml/sso", SamlSsoEndpoint.HandleAsync);
            endpoints.MapGet("/saml/sso", SamlSsoEndpoint.HandleAsync);
            endpoints.MapGet("/saml/metadata", SamlMetadataEndpoint.HandleIdpMetadataAsync);
        });

        return app;
    }

    /// <summary>
    /// Map SAML Service Provider endpoints
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseSamlServiceProvider(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapPost("/saml/acs", SamlAcsEndpoint.HandleAsync);
            endpoints.MapGet("/saml/metadata", SamlMetadataEndpoint.HandleSpMetadataAsync);
        });

        return app;
    }
}