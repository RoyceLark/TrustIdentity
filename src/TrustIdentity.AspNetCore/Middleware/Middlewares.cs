using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace TrustIdentity.AspNetCore.Middleware;

/// <summary>
/// Middleware to add security headers and logging
/// </summary>
public class TrustIdentityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TrustIdentityMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the TrustIdentityMiddleware
    /// </summary>
    /// <param name="next">The next request delegate</param>
    /// <param name="logger">The logger instance</param>
    public TrustIdentityMiddleware(RequestDelegate next, ILogger<TrustIdentityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogDebug("Processing request: {Path}", context.Request.Path);

        // Add security headers
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";

        await _next(context);
    }
}

/// <summary>
/// Middleware to handle Cross-Origin Resource Sharing (CORS)
/// </summary>
public class CorsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorsMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the CorsMiddleware
    /// </summary>
    /// <param name="next">The next request delegate</param>
    /// <param name="logger">The logger instance</param>
    public CorsMiddleware(RequestDelegate next, ILogger<CorsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers["Origin"].ToString();
        
        if (!string.IsNullOrEmpty(origin))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
        }

        if (context.Request.Method == "OPTIONS")
        {
            context.Response.StatusCode = 204;
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Middleware to enforce rate limiting
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;
    private readonly Dictionary<string, (int Count, DateTime Window)> _requestCounts = new();
    private readonly object _lock = new object();

    /// <summary>
    /// Initializes a new instance of the RateLimitMiddleware
    /// </summary>
    /// <param name="next">The next request delegate</param>
    /// <param name="logger">The logger instance</param>
    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;
        var limit = 100;
        var window = TimeSpan.FromMinutes(1);

        lock (_lock)
        {
            if (_requestCounts.TryGetValue(clientId, out var data))
            {
                if (now - data.Window < window)
                {
                    if (data.Count >= limit)
                    {
                        context.Response.StatusCode = 429;
                        context.Response.Headers["Retry-After"] = "60";
                        _logger.LogWarning("Rate limit exceeded for {ClientId}", clientId);
                        return;
                    }
                    _requestCounts[clientId] = (data.Count + 1, data.Window);
                }
                else
                {
                    _requestCounts[clientId] = (1, now);
                }
            }
            else
            {
                _requestCounts[clientId] = (1, now);
            }
        }

        await _next(context);
    }
}