using Microsoft.Extensions.Logging;

namespace TrustIdentity.Core.Services;

using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// Service for raising and logging identity server events
/// </summary>
public class EventService
{
    private readonly ILogger<EventService> _logger;

    /// <summary>
    /// Initializes a new instance of the EventService
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public EventService(ILogger<EventService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Raises an identity server event
    /// </summary>
    /// <param name="evt">The event to raise</param>
    /// <returns>A task representing the operation</returns>
    public async Task RaiseAsync(IdentityServerEvent evt)
    {
        _logger.LogInformation("Event: {EventType} - {Message}", evt.EventType, evt.Message);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Base class for all IdentityServer events
/// </summary>
public abstract class IdentityServerEvent
{
    /// <summary>The type identifier for the event</summary>
    public string EventType { get; protected set; } = string.Empty;
    /// <summary>A human-readable message describing the event</summary>
    public string Message { get; protected set; } = string.Empty;
    /// <summary>When the event occurred</summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    /// <summary>The unique ID for the event type</summary>
    public int EventId { get; protected set; }
}

/// <summary>
/// Event raised when a token is successfully issued
/// </summary>
public class TokenIssuedSuccessEvent : IdentityServerEvent
{
    /// <summary>
    /// Initializes a new instance of the TokenIssuedSuccessEvent
    /// </summary>
    /// <param name="clientId">The client receiving the token</param>
    /// <param name="grantType">The grant type used</param>
    public TokenIssuedSuccessEvent(string clientId, string grantType)
    {
        EventType = "TokenIssued";
        EventId = 2000;
        Message = $"Token issued for client {clientId} using {grantType}";
    }
}

/// <summary>
/// Event raised when a token request fails
/// </summary>
public class TokenIssuedFailureEvent : IdentityServerEvent
{
    /// <summary>
    /// Initializes a new instance of the TokenIssuedFailureEvent
    /// </summary>
    /// <param name="clientId">The client</param>
    /// <param name="error">The error description</param>
    public TokenIssuedFailureEvent(string clientId, string error)
    {
        EventType = "TokenIssueFailed";
        EventId = 2001;
        Message = $"Token issue failed for client {clientId}: {error}";
    }
}

/// <summary>
/// Event raised when a user logs in successfully
/// </summary>
public class UserLoginSuccessEvent : IdentityServerEvent
{
    /// <summary>
    /// Initializes a new instance of the UserLoginSuccessEvent
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="subjectId">The subject ID</param>
    public UserLoginSuccessEvent(string username, string subjectId)
    {
        EventType = "UserLogin";
        EventId = 1000;
        Message = $"User {username} logged in successfully";
    }
}

/// <summary>
/// Event raised when a user login fails
/// </summary>
public class UserLoginFailureEvent : IdentityServerEvent
{
    /// <summary>
    /// Initializes a new instance of the UserLoginFailureEvent
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="error">The error description</param>
    public UserLoginFailureEvent(string username, string error)
    {
        EventType = "UserLoginFailed";
        EventId = 1001;
        Message = $"User login failed for {username}: {error}";
    }
}