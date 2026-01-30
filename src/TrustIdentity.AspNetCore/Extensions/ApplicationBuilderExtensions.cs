using Microsoft.AspNetCore.Builder;
using TrustIdentity.AspNetCore.Endpoints;

namespace TrustIdentity.AspNetCore.Extensions;

/// <summary>
/// Extension methods for adding TrustIdentity to the HTTP request pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds TrustIdentity to the HTTP request pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseTrustIdentity(this IApplicationBuilder app)
    {
        app.UseMiddleware<TrustIdentity.AspNetCore.Middleware.SecurityHeadersMiddleware>();
        app.UseRouting();
        app.UseRateLimiter();
        
        app.UseEndpoints(endpoints =>
        {
            // OAuth & OpenID Connect endpoints
            TrustIdentityEndpoints.MapEndpoints(endpoints);
        });

        return app;
    }
}