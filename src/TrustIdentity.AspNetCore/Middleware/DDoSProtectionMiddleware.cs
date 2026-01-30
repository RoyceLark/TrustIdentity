using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TrustIdentity.AspNetCore.Middleware;

/// <summary>
/// DDoS protection middleware with advanced threat detection
/// </summary>
public class DDoSProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DDoSProtectionMiddleware> _logger;
    private readonly DDoSProtectionOptions _options;
    
    // Track suspicious activity
    private static readonly ConcurrentDictionary<string, SuspiciousActivity> _suspiciousClients = new();
    private static readonly ConcurrentDictionary<string, DateTime> _blockedClients = new();
    private static readonly Timer _cleanupTimer;
    
    static DDoSProtectionMiddleware()
    {
        // Cleanup every 2 minutes
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
    }

    /// <summary>
    /// Initializes a new instance of the DDoSProtectionMiddleware
    /// </summary>
    public DDoSProtectionMiddleware(
        RequestDelegate next,
        ILogger<DDoSProtectionMiddleware> logger,
        DDoSProtectionOptions options)
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

        var clientIp = GetClientIp(context);

        // Check if client is blocked
        if (_blockedClients.TryGetValue(clientIp, out var blockedUntil))
        {
            if (DateTime.UtcNow < blockedUntil)
            {
                _logger.LogWarning("Blocked DDoS attempt from {ClientIp}", clientIp);
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "access_denied",
                    error_description = "Your IP has been temporarily blocked due to suspicious activity."
                });
                return;
            }
            else
            {
                // Unblock if time expired
                _blockedClients.TryRemove(clientIp, out _);
            }
        }

        // Track request patterns
        var activity = _suspiciousClients.GetOrAdd(clientIp, _ => new SuspiciousActivity
        {
            FirstSeen = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow,
            RequestCount = 0,
            SuspicionScore = 0
        });

        bool shouldBlock = false;
        int suspicionScore = 0;
        
        lock (activity)
        {
            activity.LastSeen = DateTime.UtcNow;
            activity.RequestCount++;

            // Calculate request rate
            var timeSpan = activity.LastSeen - activity.FirstSeen;
            var requestsPerSecond = timeSpan.TotalSeconds > 0 
                ? activity.RequestCount / timeSpan.TotalSeconds 
                : activity.RequestCount;

            // Detect suspicious patterns
            if (requestsPerSecond > _options.MaxRequestsPerSecond)
            {
                activity.SuspicionScore += 10;
                _logger.LogWarning(
                    "High request rate detected from {ClientIp}: {Rate} req/s (Score: {Score})",
                    clientIp, requestsPerSecond, activity.SuspicionScore);
            }

            // Check request size
            if (context.Request.ContentLength > _options.MaxRequestSize)
            {
                activity.SuspicionScore += 5;
                _logger.LogWarning(
                    "Large request detected from {ClientIp}: {Size} bytes (Score: {Score})",
                    clientIp, context.Request.ContentLength, activity.SuspicionScore);
            }

            // Check for unusual patterns
            if (IsUnusualPattern(context, activity))
            {
                activity.SuspicionScore += 3;
            }

            // Check if should block
            if (activity.SuspicionScore >= _options.BlockThreshold)
            {
                shouldBlock = true;
                suspicionScore = activity.SuspicionScore;
            }

            // Decay suspicion score over time
            if (timeSpan.TotalMinutes > 1)
            {
                activity.SuspicionScore = Math.Max(0, activity.SuspicionScore - 1);
            }
        }

        // Handle blocking outside of lock
        if (shouldBlock)
        {
            var blockUntil = DateTime.UtcNow.Add(_options.BlockDuration);
            _blockedClients.TryAdd(clientIp, blockUntil);
            
            _logger.LogError(
                "Blocking client {ClientIp} until {BlockUntil} due to DDoS suspicion (Score: {Score})",
                clientIp, blockUntil, suspicionScore);

            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "access_denied",
                error_description = "Your IP has been blocked due to suspicious activity."
            });
            return;
        }

        await _next(context);
    }

    private string GetClientIp(HttpContext context)
    {
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        if (ip.Contains(','))
        {
            ip = ip.Split(',')[0].Trim();
        }

        return ip;
    }

    private bool IsUnusualPattern(HttpContext context, SuspiciousActivity activity)
    {
        // Check for rapid endpoint switching (bot behavior)
        var currentPath = context.Request.Path.Value ?? "/";
        if (activity.LastPath != null && activity.LastPath != currentPath)
        {
            activity.PathSwitchCount++;
            if (activity.PathSwitchCount > 10 && (DateTime.UtcNow - activity.FirstSeen).TotalSeconds < 10)
            {
                return true; // Rapid endpoint switching
            }
        }
        activity.LastPath = currentPath;

        // Check for missing common headers (bot behavior)
        if (!context.Request.Headers.ContainsKey("User-Agent") ||
            !context.Request.Headers.ContainsKey("Accept"))
        {
            return true;
        }

        // Check for suspicious user agents
        var userAgent = context.Request.Headers["User-Agent"].ToString().ToLower();
        if (string.IsNullOrEmpty(userAgent) ||
            userAgent.Contains("bot") ||
            userAgent.Contains("crawler") ||
            userAgent.Contains("spider") ||
            userAgent.Contains("scraper"))
        {
            // Allow legitimate bots (Google, Bing, etc.) but be suspicious of others
            if (!userAgent.Contains("googlebot") && !userAgent.Contains("bingbot"))
            {
                return true;
            }
        }

        return false;
    }

    private static void CleanupExpiredEntries(object? state)
    {
        var now = DateTime.UtcNow;
        
        // Remove old suspicious activity records
        var expiredActivity = _suspiciousClients
            .Where(kvp => now - kvp.Value.LastSeen > TimeSpan.FromHours(1))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredActivity)
        {
            _suspiciousClients.TryRemove(key, out _);
        }

        // Remove expired blocks
        var expiredBlocks = _blockedClients
            .Where(kvp => now > kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredBlocks)
        {
            _blockedClients.TryRemove(key, out _);
        }
    }
}

/// <summary>
/// DDoS protection options
/// </summary>
public class DDoSProtectionOptions
{
    /// <summary>
    /// Whether DDoS protection is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum requests per second before flagging as suspicious
    /// </summary>
    public double MaxRequestsPerSecond { get; set; } = 10.0;

    /// <summary>
    /// Maximum request size in bytes (10 MB default)
    /// </summary>
    public long MaxRequestSize { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Suspicion score threshold for blocking
    /// </summary>
    public int BlockThreshold { get; set; } = 20;

    /// <summary>
    /// Duration to block suspicious clients
    /// </summary>
    public TimeSpan BlockDuration { get; set; } = TimeSpan.FromMinutes(15);
}

/// <summary>
/// Tracks suspicious activity for a client
/// </summary>
internal class SuspiciousActivity
{
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public int RequestCount { get; set; }
    public int SuspicionScore { get; set; }
    public string? LastPath { get; set; }
    public int PathSwitchCount { get; set; }
}
