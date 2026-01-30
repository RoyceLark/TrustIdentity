using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace TrustIdentity.Bff;

/// <summary>
/// Extension methods for configuring BFF (Backend-for-Frontend) pattern
/// </summary>
public static class BffExtensions
{
    /// <summary>
    /// Adds BFF services to the service collection
    /// </summary>
    public static IServiceCollection AddTrustIdentityBff(this IServiceCollection services, Action<BffOptions>? configure = null)
    {
        var options = new BffOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<IBffService, BffService>();
        
        // Add authentication
        services.AddAuthentication(options.AuthenticationScheme)
            .AddCookie(options.AuthenticationScheme, cookieOptions =>
            {
                cookieOptions.Cookie.Name = options.CookieName;
                cookieOptions.Cookie.SameSite = SameSiteMode.Strict;
                cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                cookieOptions.Cookie.HttpOnly = true;
                cookieOptions.ExpireTimeSpan = TimeSpan.FromHours(options.SessionDurationHours);
                cookieOptions.SlidingExpiration = true;
            });

        // Add anti-forgery
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-X-CSRF-TOKEN";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        return services;
    }

    /// <summary>
    /// Uses BFF middleware
    /// </summary>
    public static IApplicationBuilder UseTrustIdentityBff(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        
        return app;
    }
}

/// <summary>
/// BFF configuration options
/// </summary>
public class BffOptions
{
    /// <summary>Authentication scheme name</summary>
    public string AuthenticationScheme { get; set; } = "BffCookie";
    
    /// <summary>Cookie name</summary>
    public string CookieName { get; set; } = "__Host-bff";
    
    /// <summary>Session duration in hours</summary>
    public int SessionDurationHours { get; set; } = 8;
    
    /// <summary>Enable automatic token refresh</summary>
    public bool EnableAutomaticTokenRefresh { get; set; } = true;
    
    /// <summary>API proxy base path</summary>
    public string ApiProxyBasePath { get; set; } = "/api";
}

/// <summary>
/// BFF service interface
/// </summary>
public interface IBffService
{
    /// <summary>
    /// Stores tokens in session
    /// </summary>
    System.Threading.Tasks.Task StoreTokensAsync(HttpContext context, string accessToken, string? refreshToken = null);
    
    /// <summary>
    /// Retrieves access token from session
    /// </summary>
    System.Threading.Tasks.Task<string?> GetAccessTokenAsync(HttpContext context);
    
    /// <summary>
    /// Refreshes the access token if needed
    /// </summary>
    System.Threading.Tasks.Task<bool> RefreshTokenIfNeededAsync(HttpContext context);
}

/// <summary>
/// BFF service implementation
/// </summary>
public class BffService : IBffService
{
    private readonly BffOptions _options;

    /// <summary>
    /// Initializes a new instance of BffService
    /// </summary>
    public BffService(BffOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Stores tokens in session
    /// </summary>
    public System.Threading.Tasks.Task StoreTokensAsync(HttpContext context, string accessToken, string? refreshToken = null)
    {
        context.Session.SetString("access_token", accessToken);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            context.Session.SetString("refresh_token", refreshToken);
        }
        context.Session.SetString("token_stored_at", DateTimeOffset.UtcNow.ToString("O"));
        
        return System.Threading.Tasks.Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves access token from session
    /// </summary>
    public System.Threading.Tasks.Task<string?> GetAccessTokenAsync(HttpContext context)
    {
        var token = context.Session.GetString("access_token");
        return System.Threading.Tasks.Task.FromResult(token);
    }

    /// <summary>
    /// Refreshes the access token if needed
    /// </summary>
    public System.Threading.Tasks.Task<bool> RefreshTokenIfNeededAsync(HttpContext context)
    {
        // In a real implementation, this would check token expiration and refresh if needed
        // For now, just return true
        return System.Threading.Tasks.Task.FromResult(true);
    }
}
