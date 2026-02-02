using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private readonly IDistributedCache? _cache;
    
    // In-memory storage for rate limiting (fallback)
    private static readonly ConcurrentDictionary<string, ClientRateLimit> _memoryLimits = new();
    private static readonly Timer _cleanupTimer;
    
    static RateLimitingMiddleware()
    {
        // Cleanup expired entries every 5 minutes (for in-memory fallback)
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Initializes a new instance of the RateLimitingMiddleware
    /// </summary>
    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitingOptions options,
        IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _options = options;
        _cache = serviceProvider.GetService(typeof(IDistributedCache)) as IDistributedCache;
        
        if (_cache != null)
        {
            _logger.LogInformation("RateLimitingMiddleware initialized with Distributed Cache");
        }
        else
        {
            _logger.LogWarning("RateLimitingMiddleware initialized with In-Memory Cache (Not suitable for multi-server)");
        }
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
        
        // Use endpoint-specific limit
        var permitLimit = _options.GetEndpointLimit(endpoint);
        var window = _options.Window;

        bool isRateLimited = false;
        int remaining = 0;
        long resetTime = 0;
        
        if (_cache != null)
        {
            // Distributed Rate Limiting
            (isRateLimited, remaining, resetTime) = await CheckDistributedLimitAsync(clientId, endpoint, permitLimit, window);
        }
        else
        {
            // In-Memory Rate Limiting
            (isRateLimited, remaining, resetTime) = CheckMemoryLimit(clientId, endpoint, permitLimit, window);
        }

        // Add rate limit headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-RateLimit-Limit"] = permitLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
            context.Response.Headers["X-RateLimit-Reset"] = resetTime.ToString();
            return Task.CompletedTask;
        });

        if (isRateLimited)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId} on endpoint {Endpoint}",
                clientId, endpoint);

            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers["Retry-After"] = ((int)window.TotalSeconds).ToString();
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "rate_limit_exceeded",
                error_description = "Too many requests. Please try again later.",
                retry_after = (int)window.TotalSeconds
            });
            
            return;
        }

        await _next(context);
    }

    private async Task<(bool Limited, int Remaining, long Reset)> CheckDistributedLimitAsync(string clientId, string endpoint, int limit, TimeSpan window)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        // Fixed window strategy: Key includes the precise time window
        // e.g. "ratelimit:client:123:endpoint:token:1738000" (window ID)
        var windowId = now / (long)window.TotalSeconds;
        var key = $"ratelimit:{clientId}:{endpoint}:{windowId}";
        
        // Get current count
        var countBytes = await _cache!.GetAsync(key);
        int count = 0;
        
        if (countBytes != null)
        {
            count = BitConverter.ToInt32(countBytes, 0);
        }
        
        // Check limit
        if (count >= limit)
        {
            var reset = (windowId + 1) * (long)window.TotalSeconds;
            return (true, 0, reset);
        }
        
        // Increment (atomic in Redis usually, here we do best-effort read-modify-write)
        count++;
        await _cache.SetAsync(key, BitConverter.GetBytes(count), new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds((windowId + 1) * (long)window.TotalSeconds)
        });
        
        var resetTime = (windowId + 1) * (long)window.TotalSeconds;
        return (false, Math.Max(0, limit - count), resetTime);
    }

    private (bool Limited, int Remaining, long Reset) CheckMemoryLimit(string clientId, string endpoint, int limit, TimeSpan window)
    {
        var key = $"{clientId}:{endpoint}";
        var limitEntry = _memoryLimits.GetOrAdd(key, _ => new ClientRateLimit
        {
            WindowStart = DateTime.UtcNow,
            RequestCount = 0
        });

        lock (limitEntry)
        {
            // Reset window if expired
            if (DateTime.UtcNow - limitEntry.WindowStart > window)
            {
                limitEntry.WindowStart = DateTime.UtcNow;
                limitEntry.RequestCount = 0;
            }

            var resetTime = new DateTimeOffset(limitEntry.WindowStart.Add(window)).ToUnixTimeSeconds();

            // Check if limit exceeded
            if (limitEntry.RequestCount >= limit)
            {
                return (true, 0, resetTime);
            }
            
            // Increment
            limitEntry.RequestCount++;
            var remaining = Math.Max(0, limit - limitEntry.RequestCount);
            
            return (false, remaining, resetTime);
        }
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
        var expiredKeys = _memoryLimits
            .Where(kvp => now - kvp.Value.WindowStart > TimeSpan.FromHours(1))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _memoryLimits.TryRemove(key, out _);
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
    /// Maximum number of requests per window (default limit)
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Queue limit (0 = no queue)
    /// </summary>
    public int QueueLimit { get; set; } = 0;

    /// <summary>
    /// Endpoint-specific rate limits (overrides PermitLimit for specific endpoints)
    /// </summary>
    public Dictionary<string, int> EndpointLimits { get; set; } = new()
    {
        // Stricter limits for sensitive endpoints
        ["/connect/token"] = 10,        // Token endpoint: 10 req/min
        ["/connect/authorize"] = 20,    // Authorize endpoint: 20 req/min
        ["/connect/introspect"] = 30,   // Introspection: 30 req/min
        ["/connect/revoke"] = 20,       // Revocation: 20 req/min
        ["/saml"] = 20,                 // SAML: 20 req/min
        ["/wsfed"] = 20                 // WS-Federation: 20 req/min
    };

    /// <summary>
    /// Gets the rate limit for a specific endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <returns>The rate limit for the endpoint</returns>
    public int GetEndpointLimit(string endpoint)
    {
        return EndpointLimits.TryGetValue(endpoint, out var limit) ? limit : PermitLimit;
    }
}

/// <summary>
/// Client rate limit tracking
/// </summary>
internal class ClientRateLimit
{
    public DateTime WindowStart { get; set; }
    public int RequestCount { get; set; }
}
