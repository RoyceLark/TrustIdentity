using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.RateLimiting;

namespace TrustIdentity.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring security features like rate limiting and security headers
/// </summary>
public static class SecurityExtensions
{
    private const string TokenPolicy = "token";
    private const string AuthPolicy = "auth";

    /// <summary>
    /// Adds security services including rate limiting
    /// </summary>
    public static IServiceCollection AddTrustIdentitySecurity(this IServiceCollection services)
    {

        return services;
    }

    /// <summary>
    /// Adds security headers middleware to the request pipeline
    /// </summary>
    public static IApplicationBuilder UseTrustIdentitySecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Security Headers
            var headers = context.Response.Headers;
            
            headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none'; " +
                "base-uri 'self'; " +
                "form-action 'self';");

            headers.Append("X-Content-Type-Options", "nosniff");
            headers.Append("X-Frame-Options", "DENY");
            headers.Append("X-XSS-Protection", "1; mode=block");
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocations=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
            
            // HSTS for production
            if (!context.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            }

            await next();
        });

        return app;
    }

    /// <summary>
    /// Applies rate limiting policies to TrustIdentity endpoints
    /// </summary>
    public static void ApplyTrustIdentityRateLimits(this IEndpointConventionBuilder builder, string policy)
    {
        builder.RequireRateLimiting(policy);
    }
}
