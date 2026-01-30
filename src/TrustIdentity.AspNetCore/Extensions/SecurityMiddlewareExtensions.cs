using Microsoft.AspNetCore.Builder;
using TrustIdentity.AspNetCore.Middleware;

namespace TrustIdentity.AspNetCore.Extensions;

/// <summary>
/// Extension methods for adding rate limiting and DDoS protection
/// </summary>
public static class SecurityMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseTrustIdentityRateLimiting(
        this IApplicationBuilder app,
        RateLimitingOptions? options = null)
    {
        options ??= new RateLimitingOptions();
        return app.UseMiddleware<RateLimitingMiddleware>(options);
    }

    /// <summary>
    /// Adds DDoS protection middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseTrustIdentityDDoSProtection(
        this IApplicationBuilder app,
        DDoSProtectionOptions? options = null)
    {
        options ??= new DDoSProtectionOptions();
        return app.UseMiddleware<DDoSProtectionMiddleware>(options);
    }

    /// <summary>
    /// Adds both rate limiting and DDoS protection (recommended)
    /// </summary>
    public static IApplicationBuilder UseTrustIdentitySecurityProtection(
        this IApplicationBuilder app,
        RateLimitingOptions? rateLimitOptions = null,
        DDoSProtectionOptions? ddosOptions = null)
    {
        // DDoS protection first (blocks malicious traffic early)
        app.UseTrustIdentityDDoSProtection(ddosOptions);
        
        // Then rate limiting (controls legitimate traffic)
        app.UseTrustIdentityRateLimiting(rateLimitOptions);
        
        return app;
    }
}
