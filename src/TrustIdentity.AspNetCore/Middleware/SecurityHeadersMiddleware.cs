using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Configuration;

namespace TrustIdentity.AspNetCore.Middleware;

/// <summary>
/// Middleware for adding security headers including CSP, HSTS, and Frame Options
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TrustIdentityOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The TrustIdentity options.</param>
    public SecurityHeadersMiddleware(RequestDelegate next, TrustIdentityOptions options)
    {
        _next = next;
        _options = options;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the completion of the request processing.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // HTTPS Redirection and HSTS (Handled by ASP.NET Core middleware usually, but we can add headers)
        if (_options.RequireHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // CSP - Enhanced for production
        if (_options.Csp.Enabled)
        {
            var csp = "default-src 'self'; " +
                      "script-src 'self'; " +
                      "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " + 
                      "img-src 'self' data: https:; " +
                      "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
                      "connect-src 'self'; " +
                      "frame-ancestors 'none'; " +
                      "form-action 'self'; " + // Prevent form data being sent to malicious sites
                      "base-uri 'self'; " +
                      "object-src 'none';";
            
            context.Response.Headers["Content-Security-Policy"] = csp;
        }

        // Basic Security Headers
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        await _next(context);
    }
}
