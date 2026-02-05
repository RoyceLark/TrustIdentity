using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using TrustIdentity.AspNetCore.Extensions;
using System;

namespace TrustIdentity.UI.Library.Extensions;

/// <summary>
/// Extension methods for registering TrustIdentity UI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds TrustIdentity UI services to the TrustIdentity builder.
    /// </summary>
    public static TrustIdentityBuilder AddTrustIdentityUI(this TrustIdentityBuilder builder, Action<TrustIdentityUIOptions>? configureOptions = null)
    {
        var options = new TrustIdentityUIOptions();
        configureOptions?.Invoke(options);

        // Register options
        builder.Services.AddSingleton(options);

        // Add Razor Pages and verify static assets
        builder.Services.AddRazorPages()
            .AddApplicationPart(typeof(ServiceCollectionExtensions).Assembly);

        return builder;
    }
}

/// <summary>
/// Configuration options for TrustIdentity UI.
/// </summary>
public class TrustIdentityUIOptions
{
    /// <summary>Application name displayed in the UI.</summary>
    public string ApplicationName { get; set; } = "TrustIdentity";
    
    /// <summary>URL for the application logo.</summary>
    public string LogoUrl { get; set; } = "/images/logo.png";
    
    /// <summary>Theme for the UI (light or dark).</summary>
    public string Theme { get; set; } = "light";
    
    /// <summary>Primary brand color.</summary>
    public string PrimaryColor { get; set; } = "#007bff";
    
    /// <summary>Enable user registration.</summary>
    public bool EnableRegistration { get; set; } = true;

    /// <summary>Enable forgot password functionality.</summary>
    public bool EnableForgotPassword { get; set; } = true;
}
