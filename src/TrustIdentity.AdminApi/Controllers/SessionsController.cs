using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.AdminApi.Controllers;

/// <summary>
/// Controller for managing User Sessions
/// </summary>
[Authorize(Policy = "AdminApiAccess")]
[ApiController]
[Route("api/v1/admin/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionStore _sessionStore;

    /// <summary>
    /// Initializes a new instance of the SessionsController
    /// </summary>
    public SessionsController(ISessionStore sessionStore)
    {
        _sessionStore = sessionStore;
    }

    /// <summary>
    /// Retrieves all active sessions for a specific user
    /// </summary>
    /// <param name="subjectId">The user subject ID</param>
    [HttpGet("user/{subjectId}")]
    public async Task<IActionResult> GetUserSessions(string subjectId)
    {
        var sessions = await _sessionStore.GetUserSessionsAsync(subjectId);
        return Ok(sessions);
    }

    /// <summary>
    /// Retrieves a specific user session by ID
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetById(string sessionId)
    {
        var session = await _sessionStore.GetAsync(sessionId);
        if (session == null) return NotFound();
        return Ok(session);
    }

    /// <summary>
    /// Terminates a specific user session
    /// </summary>
    /// <param name="sessionId">The session ID to terminate</param>
    [HttpDelete("{sessionId}")]
    public async Task<IActionResult> KillSession(string sessionId)
    {
        await _sessionStore.RemoveAsync(sessionId);
        return NoContent();
    }

    /// <summary>
    /// Terminates all active sessions for a specific user
    /// </summary>
    /// <param name="subjectId">The user subject ID</param>
    [HttpDelete("user/{subjectId}")]
    public async Task<IActionResult> KillAllUserSessions(string subjectId)
    {
        await _sessionStore.RemoveUserSessionsAsync(subjectId);
        return NoContent();
    }
}
