using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using TrustIdentity.WsFederation.Services;
using TrustIdentity.WsFederation.Security;
using TrustIdentity.WsFederation.Endpoints;

namespace TrustIdentity.WsFederation.Extensions;

/// <summary>
/// Service collection extensions for WS-Federation
/// </summary>
public static class WsFederationServiceCollectionExtensions
{
    /// <summary>
    /// Add WS-Federation Identity Provider to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure WS-Federation options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWsFederationIdentityProvider(
        this IServiceCollection services,
        Action<WsFederationConfiguration> configureOptions)
    {
        var config = new WsFederationConfiguration();
        configureOptions(config);

        services.AddSingleton(config);
        services.AddSingleton<WsTrustSecurityTokenService>();
        services.AddSingleton<WsFederationIdentityProvider>();

        return services;
    }
}

/// <summary>
/// Application builder extensions for WS-Federation
/// </summary>
public static class WsFederationApplicationBuilderExtensions
{
    /// <summary>
    /// Map WS-Federation endpoints in the HTTP request pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseWsFederationIdentityProvider(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/wsfed", WsFederationSignInEndpoint.HandleAsync);
            endpoints.MapPost("/wsfed", WsFederationSignInEndpoint.HandleAsync);
            endpoints.MapGet("/FederationMetadata/2007-06/FederationMetadata.xml", 
                WsFederationMetadataEndpoint.HandleAsync);
        });

        return app;
    }
}