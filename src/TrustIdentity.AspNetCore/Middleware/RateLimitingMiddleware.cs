using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TrustIdentity.AspNetCore.Middleware;

/// <summary>
/// Advanced rate limiting middleware with DDoS protection
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitingOptions _options;
    
    // In-memory storage for rate limiting (use Redis in production for distributed scenarios)
    private static readonly ConcurrentDictionary<string, ClientRateLimit> _clientLimits = new();
    private static readonly Timer _cleanupTimer;
    
    static RateLimitingMiddleware()
    {
        // Cleanup expired entries every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Initializes a new instance of the RateLimitingMiddleware
    /// </summary>
    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitingOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var endpoint = GetEndpointIdentifier(context);
        var key = $"{clientId}:{endpoint}";

        var limit = _clientLimits.GetOrAdd(key, _ => new ClientRateLimit
        {
            WindowStart = DateTime.UtcNow,
            RequestCount = 0
        });

        bool isRateLimited = false;
        int remaining = 0;
        
        lock (limit)
        {
            // Reset window if expired
            if (DateTime.UtcNow - limit.WindowStart > _options.Window)
            {
                limit.WindowStart = DateTime.UtcNow;
                limit.RequestCount = 0;
            }

            // Check if limit exceeded
            if (limit.RequestCount >= _options.PermitLimit)
            {
                isRateLimited = true;
            }
            else
            {
                // Increment request count
                limit.RequestCount++;
                remaining = Math.Max(0, _options.PermitLimit - limit.RequestCount);
            }
            
            // Add rate limit headers
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-RateLimit-Limit"] = _options.PermitLimit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(limit.WindowStart.Add(_options.Window)).ToUnixTimeSeconds().ToString();
                return Task.CompletedTask;
            });
        }

        // Handle rate limit response outside of lock
        if (isRateLimited)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId} on endpoint {Endpoint}",
                clientId, endpoint);

            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers["Retry-After"] = ((int)_options.Window.TotalSeconds).ToString();
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "rate_limit_exceeded",
                error_description = "Too many requests. Please try again later.",
                retry_after = (int)_options.Window.TotalSeconds
            });
            
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get client IP from various headers (for proxy scenarios)
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        // Take only the first IP if multiple are present
        if (ip.Contains(','))
        {
            ip = ip.Split(',')[0].Trim();
        }

        return ip;
    }

    private string GetEndpointIdentifier(HttpContext context)
    {
        // Group by endpoint path
        var path = context.Request.Path.Value ?? "/";
        
        // Normalize common OAuth/OIDC endpoints
        if (path.StartsWith("/connect/token", StringComparison.OrdinalIgnoreCase))
            return "/connect/token";
        if (path.StartsWith("/connect/authorize", StringComparison.OrdinalIgnoreCase))
            return "/connect/authorize";
        if (path.StartsWith("/connect/userinfo", StringComparison.OrdinalIgnoreCase))
            return "/connect/userinfo";
        if (path.StartsWith("/connect/introspect", StringComparison.OrdinalIgnoreCase))
            return "/connect/introspect";
        if (path.StartsWith("/connect/revoke", StringComparison.OrdinalIgnoreCase))
            return "/connect/revoke";
        if (path.StartsWith("/saml", StringComparison.OrdinalIgnoreCase))
            return "/saml";
        if (path.StartsWith("/wsfed", StringComparison.OrdinalIgnoreCase))
            return "/wsfed";
        
        return path;
    }

    private static void CleanupExpiredEntries(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _clientLimits
            .Where(kvp => now - kvp.Value.WindowStart > TimeSpan.FromHours(1))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _clientLimits.TryRemove(key, out _);
        }
    }
}

/// <summary>
/// Rate limiting options
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Whether rate limiting is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Time window for rate limiting
    /// </summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Maximum number of requests per window
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Queue limit (0 = no queue)
    /// </summary>
    public int QueueLimit { get; set; } = 0;
}

/// <summary>
/// Client rate limit tracking
/// </summary>
internal class ClientRateLimit
{
    public DateTime WindowStart { get; set; }
    public int RequestCount { get; set; }
}
