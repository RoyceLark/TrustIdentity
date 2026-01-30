using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Storage.EntityFramework;

namespace TrustIdentity.Storage.Stores;

/// <summary>
/// Entity Framework implementation of audit store
/// </summary>
public class EntityFrameworkAuditStore : IAuditStore
{
    private readonly PersistedGrantDbContext _context;
    private readonly ILogger<EntityFrameworkAuditStore> _logger;

    /// <summary>
    /// Initializes a new instance of the EntityFrameworkAuditStore
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    public EntityFrameworkAuditStore(PersistedGrantDbContext context, ILogger<EntityFrameworkAuditStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Stores an audit event
    /// </summary>
    /// <param name="evt">The event to store</param>
    public async Task StoreAsync(AuditEvent evt)
    {
        _context.AuditEvents.Add(evt);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets recent audit events
    /// </summary>
    /// <param name="count">Number of events to retrieve</param>
    /// <returns>A collection of audit events</returns>
    public async Task<IEnumerable<AuditEvent>> GetRecentEventsAsync(int count = 50)
    {
        return await _context.AuditEvents
            .AsNoTracking()
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Gets audit statistics
    /// </summary>
    /// <returns>The audit statistics</returns>
    public async Task<AuditStats> GetStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        
        var tokensIssued = await _context.AuditEvents
            .CountAsync(e => e.Timestamp >= today && e.EventType == "TokenIssued");
            
        var failedLogins = await _context.AuditEvents
            .CountAsync(e => e.Timestamp >= today && e.EventType == "UserLoginFailed");

        var successLogins = await _context.AuditEvents
            .CountAsync(e => e.Timestamp >= today && e.EventType == "UserLogin");

        // Simple login trend for last 7 days
        var trend = new List<(string Label, int Value)>();
        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddDays(-i).Date;
            var nextDate = date.AddDays(1);
            var count = await _context.AuditEvents
                .CountAsync(e => e.Timestamp >= date && e.Timestamp < nextDate && e.EventType == "UserLogin");
            
            trend.Add((date.ToString("MMM dd"), count));
        }

        return new AuditStats
        {
            TokensIssuedToday = tokensIssued,
            FailedLoginsToday = failedLogins,
            SuccessLoginsToday = successLogins,
            LoginTrend = trend
        };
    }
}
