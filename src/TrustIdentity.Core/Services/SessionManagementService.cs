using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Configuration;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for managing user sessions and single sign-on (SSO)
/// </summary>
public class SessionManagementService
{
    private readonly ILogger<SessionManagementService> _logger;
    private readonly ISessionStore _sessionStore;
    private readonly TrustIdentityOptions _options;

    /// <summary>
    /// Initializes a new instance of the SessionManagementService
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="sessionStore">The session store</param>
    /// <param name="options">The TrustIdentity options</param>
    public SessionManagementService(
        ILogger<SessionManagementService> logger, 
        ISessionStore sessionStore,
        TrustIdentityOptions options)
    {
        _logger = logger;
        _sessionStore = sessionStore;
        _options = options;
    }

    /// <summary>
    /// Creates a new user session
    /// </summary>
    /// <param name="subjectId">The user subject ID</param>
    /// <param name="clientId">The client ID</param>
    /// <param name="displayName">The display name</param>
    /// <returns>The generated session ID</returns>
    public async Task<string> CreateSessionAsync(string subjectId, string clientId, string? displayName = null)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        
        var session = new UserSession
        {
            SessionId = sessionId,
            SubjectId = subjectId,
            DisplayName = displayName,
            ClientIds = new List<string> { clientId },
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddSeconds(_options.Authentication.CookieLifetime)
        };

        await _sessionStore.StoreAsync(session);
        
        _logger.LogDebug("Created session {SessionId} for user {SubjectId}", sessionId, subjectId);
        return sessionId;
    }

    /// <summary>
    /// Retrieves a user session by ID
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <returns>The user session if found and not expired; otherwise null</returns>
    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        var session = await _sessionStore.GetAsync(sessionId);
        
        if (session != null && session.Expires < DateTime.UtcNow)
        {
            await _sessionStore.RemoveAsync(sessionId);
            return null;
        }

        return session;
    }

    /// <summary>
    /// Adds a client to an existing session
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <param name="clientId">The client ID</param>
    /// <returns>A task representing the operation</returns>
    public async Task AddClientToSessionAsync(string sessionId, string clientId)
    {
        var session = await _sessionStore.GetAsync(sessionId);
        if (session != null)
        {
            if (!session.ClientIds.Contains(clientId))
            {
                session.ClientIds.Add(clientId);
                await _sessionStore.StoreAsync(session);
            }
        }
    }

    /// <summary>
    /// Removes a user session
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <returns>A task representing the operation</returns>
    public async Task RemoveSessionAsync(string sessionId)
    {
        await _sessionStore.RemoveAsync(sessionId);
        _logger.LogDebug("Removed session {SessionId}", sessionId);
    }

    /// <summary>
    /// Gets all active sessions for a user
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <returns>A collection of user sessions</returns>
    public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(string subjectId)
    {
        var sessions = await _sessionStore.GetUserSessionsAsync(subjectId);
        return sessions.Where(s => s.Expires > DateTime.UtcNow);
    }
}