using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Storage.EntityFramework;

namespace TrustIdentity.Storage.Stores;

/// <summary>
/// Entity Framework implementation of session store
/// </summary>
public class EntityFrameworkSessionStore : ISessionStore
{
    private readonly PersistedGrantDbContext _context;

    /// <summary>
    /// Initializes a new instance of the EntityFrameworkSessionStore
    /// </summary>
    /// <param name="context">The database context</param>
    public EntityFrameworkSessionStore(PersistedGrantDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Stores a session
    /// </summary>
    /// <param name="session">The session to store</param>
    public async Task StoreAsync(UserSession session)
    {
        var existing = await _context.UserSessions.FindAsync(session.SessionId);
        if (existing == null)
        {
            _context.UserSessions.Add(session);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(session);
        }
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets a session by ID
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <returns>The user session or null</returns>
    public async Task<UserSession?> GetAsync(string sessionId)
    {
        return await _context.UserSessions.FindAsync(sessionId);
    }

    /// <summary>
    /// Gets all sessions for a user
    /// </summary>
    /// <param name="subjectId">The user identifier</param>
    /// <returns>A collection of user sessions</returns>
    public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(string subjectId)
    {
        return await _context.UserSessions
            .Where(s => s.SubjectId == subjectId)
            .OrderByDescending(s => s.Created)
            .ToListAsync();
    }

    /// <summary>
    /// Removes a session
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    public async Task RemoveAsync(string sessionId)
    {
        var session = await _context.UserSessions.FindAsync(sessionId);
        if (session != null)
        {
            _context.UserSessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Removes all sessions for a user
    /// </summary>
    /// <param name="subjectId">The user identifier</param>
    public async Task RemoveUserSessionsAsync(string subjectId)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.SubjectId == subjectId)
            .ToListAsync();
            
        if (sessions.Any())
        {
            _context.UserSessions.RemoveRange(sessions);
            await _context.SaveChangesAsync();
        }
    }
}
