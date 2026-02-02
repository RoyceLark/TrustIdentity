using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Default implementation of IEventService
/// </summary>
public class DefaultEventService : IEventService
{
    private readonly IEventSink _sink;
    private readonly ILogger<DefaultEventService> _logger;

    /// <summary>
    /// Initializes a new instance of the DefaultEventService
    /// </summary>
    public DefaultEventService(IEventSink sink, ILogger<DefaultEventService> logger)
    {
        _sink = sink;
        _logger = logger;
    }

    /// <summary>
    /// Raises an event
    /// </summary>
    public async Task RaiseAsync(Event evt)
    {
        if (evt == null)
        {
            _logger.LogWarning("Null event passed to RaiseAsync");
            return;
        }

        _logger.LogDebug("Raising event: {EventName} (ID: {EventId})", evt.Name, evt.Id);

        await _sink.PersistAsync(evt);
    }
}

/// <summary>
/// Default event sink that logs to the standard logging system
/// </summary>
public class DefaultEventSink : IEventSink
{
    private readonly ILogger<DefaultEventSink> _logger;

    /// <summary>
    /// Initializes a new instance of the DefaultEventSink
    /// </summary>
    public DefaultEventSink(ILogger<DefaultEventSink> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Persists an event
    /// </summary>
    public Task PersistAsync(Event evt)
    {
        var logLevel = evt.EventType switch
        {
            EventType.Success => LogLevel.Information,
            EventType.Failure => LogLevel.Warning,
            EventType.Error => LogLevel.Error,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, 
            "Event: {EventName} (ID: {EventId}, Category: {Category}, Type: {Type}) - {Message}",
            evt.Name, evt.Id, evt.Category, evt.EventType, evt.Message);

        return Task.CompletedTask;
    }
}
