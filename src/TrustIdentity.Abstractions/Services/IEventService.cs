using System;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Interface for the event service
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Raises the specified event.
    /// </summary>
    /// <param name="evt">The event.</param>
    /// <returns></returns>
    Task RaiseAsync(Event evt);
}

/// <summary>
/// Interface for event sink (where events are persisted/logged)
/// </summary>
public interface IEventSink
{
    /// <summary>
    /// Persists the event
    /// </summary>
    /// <param name="evt">The event.</param>
    /// <returns></returns>
    Task PersistAsync(Event evt);
}

/// <summary>
/// Base class for events
/// </summary>
public class Event
{
    /// <summary>
    /// Gets or sets the event identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the event name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event category
    /// </summary>
    public EventCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the event type
    /// </summary>
    public EventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the activity ID
    /// </summary>
    public string? ActivityId { get; set; }

    /// <summary>
    /// Gets or sets the local IP address
    /// </summary>
    public string? LocalIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the remote IP address
    /// </summary>
    public string? RemoteIpAddress { get; set; }
}

/// <summary>
/// Event categories
/// </summary>
public enum EventCategory
{
    /// <summary>
    /// Authentication events
    /// </summary>
    Authentication,

    /// <summary>
    /// Token events
    /// </summary>
    Token,

    /// <summary>
    /// Grant events
    /// </summary>
    Grant,

    /// <summary>
    /// Error events
    /// </summary>
    Error,

    /// <summary>
    /// Device flow events
    /// </summary>
    Device,

    /// <summary>
    /// Information events
    /// </summary>
    Information
}

/// <summary>
/// Event types
/// </summary>
public enum EventType
{
    /// <summary>
    /// Success event
    /// </summary>
    Success,

    /// <summary>
    /// Failure event
    /// </summary>
    Failure,

    /// <summary>
    /// Error event
    /// </summary>
    Error,

    /// <summary>
    /// Information event
    /// </summary>
    Information
}
