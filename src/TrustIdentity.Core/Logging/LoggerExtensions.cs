using Microsoft.Extensions.Logging;
using System;

namespace TrustIdentity.Core.Logging;

/// <summary>
/// Extension methods for ILogger to handle security-specific events
/// </summary>
public static class LoggerExtensions
{
    private static readonly EventId SecurityEvent = new EventId(1000, "SecurityEvent");

    /// <summary>
    /// Logs a security-related event with a specific type and message
    /// </summary>
    public static void LogSecurityEvent(this ILogger logger, string eventType, string message, params object[] args)
    {
        // Enrich the message with the event type
        var formattedMessage = $"[Security][{eventType}] {message}";
        
        // Use Warning level for security events by default, as they are often significant
        logger.LogWarning(SecurityEvent, formattedMessage, args);
        
        // In a real production system, this could also send data to a SIEM, 
        // a dedicated security log file, or an audit table via another service.
    }
}
