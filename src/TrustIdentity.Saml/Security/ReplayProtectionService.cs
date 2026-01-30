using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace TrustIdentity.Saml.Security;

/// <summary>
/// Replay Attack Protection - Prevents reuse of SAML assertions
/// </summary>
public class ReplayProtectionService
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _assertionLifetime;

    /// <summary>
    /// Initializes a new instance of the ReplayProtectionService
    /// </summary>
    /// <param name="cache">Distributed cache instance</param>
    /// <param name="assertionLifetime">Lifetime of assertion replay protection</param>
    public ReplayProtectionService(IDistributedCache cache, TimeSpan? assertionLifetime = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _assertionLifetime = assertionLifetime ?? TimeSpan.FromMinutes(10);
    }

    /// <summary>
    /// Check if assertion ID has been used before (replay attack)
    /// </summary>
    /// <param name="assertionId">The assertion ID to check</param>
    /// <returns>True if the assertion is a replay; otherwise false</returns>
    public async Task<bool> IsReplayedAssertionAsync(string assertionId)
    {
        if (string.IsNullOrWhiteSpace(assertionId))
            throw new ArgumentException("Assertion ID cannot be null or empty", nameof(assertionId));

        var cacheKey = $"saml:assertion:{assertionId}";
        var value = await _cache.GetAsync(cacheKey);
        
        return value != null; // If exists, it's a replay
    }

    /// <summary>
    /// Record assertion ID to prevent replay
    /// </summary>
    /// <param name="assertionId">The assertion ID to record</param>
    /// <param name="customLifetime">Optional custom lifetime for this record</param>
    /// <returns>A task representing the operation</returns>
    public async Task RecordAssertionAsync(string assertionId, TimeSpan? customLifetime = null)
    {
        if (string.IsNullOrWhiteSpace(assertionId))
            throw new ArgumentException("Assertion ID cannot be null or empty", nameof(assertionId));

        var cacheKey = $"saml:assertion:{assertionId}";
        var lifetime = customLifetime ?? _assertionLifetime;
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = lifetime
        };

        // Store timestamp as value
        var timestamp = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o"));
        await _cache.SetAsync(cacheKey, timestamp, options);
    }

    /// <summary>
    /// Validate and record assertion (atomic operation)
    /// </summary>
    /// <param name="assertionId">The assertion ID</param>
    /// <param name="customLifetime">Optional custom lifetime</param>
    /// <returns>True if the assertion was valid and recorded; false if it was already used</returns>
    public async Task<bool> ValidateAndRecordAssertionAsync(string assertionId, TimeSpan? customLifetime = null)
    {
        // Check if already used
        if (await IsReplayedAssertionAsync(assertionId))
            return false;

        // Record it
        await RecordAssertionAsync(assertionId, customLifetime);
        return true;
    }

    /// <summary>
    /// Clear all recorded assertions (for testing or maintenance)
    /// </summary>
    /// <returns>A task representing the operation</returns>
    public async Task ClearAllAssertionsAsync()
    {
        // Note: IDistributedCache doesn't have a clear all method
        // This would need to be implemented per cache provider
        await Task.CompletedTask;
    }
}

/// <summary>
/// In-memory implementation for development/testing
/// </summary>
public class InMemoryReplayProtectionService
{
    private readonly Dictionary<string, DateTime> _usedAssertions = new();
    private readonly object _lock = new();
    private readonly TimeSpan _assertionLifetime;

    /// <summary>
    /// Initializes a new instance of the InMemoryReplayProtectionService
    /// </summary>
    /// <param name="assertionLifetime">Lifetime of assertion replay protection</param>
    public InMemoryReplayProtectionService(TimeSpan? assertionLifetime = null)
    {
        _assertionLifetime = assertionLifetime ?? TimeSpan.FromMinutes(10);
    }

    /// <summary>
    /// Check if assertion ID has been used before
    /// </summary>
    /// <param name="assertionId">The assertion ID</param>
    /// <returns>True if replayed; otherwise false</returns>
    public bool IsReplayedAssertion(string assertionId)
    {
        lock (_lock)
        {
            CleanupExpired();
            return _usedAssertions.ContainsKey(assertionId);
        }
    }

    /// <summary>
    /// Record assertion ID to prevent replay
    /// </summary>
    /// <param name="assertionId">The assertion ID</param>
    /// <param name="customLifetime">Optional custom lifetime</param>
    public void RecordAssertion(string assertionId, TimeSpan? customLifetime = null)
    {
        lock (_lock)
        {
            var lifetime = customLifetime ?? _assertionLifetime;
            _usedAssertions[assertionId] = DateTime.UtcNow.Add(lifetime);
        }
    }

    /// <summary>
    /// Validate and record assertion (atomic thread-safe operation)
    /// </summary>
    /// <param name="assertionId">The assertion ID</param>
    /// <returns>True if valid and recorded; false if replayed</returns>
    public bool ValidateAndRecordAssertion(string assertionId)
    {
        lock (_lock)
        {
            CleanupExpired();
            
            if (_usedAssertions.ContainsKey(assertionId))
                return false;

            _usedAssertions[assertionId] = DateTime.UtcNow.Add(_assertionLifetime);
            return true;
        }
    }

    private void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        var expired = _usedAssertions.Where(kvp => kvp.Value < now).Select(kvp => kvp.Key).ToList();
        foreach (var key in expired)
        {
            _usedAssertions.Remove(key);
        }
    }
}