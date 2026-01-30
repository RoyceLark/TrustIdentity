namespace TrustIdentity.Abstractions.Models;

using System;
using System.Collections.Generic;
/// <summary>
/// Context for behavior analysis
/// </summary>
public class BehaviorAnalysisContext
{
    /// <summary>The user ID being analyzed</summary>
    public string UserId { get; set; } = string.Empty;
    /// <summary>The IP address of the request</summary>
    public string IpAddress { get; set; } = string.Empty;
    /// <summary>The user agent of the requester</summary>
    public string UserAgent { get; set; } = string.Empty;
    /// <summary>The timestamp of the event</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    /// <summary>The action being performed</summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>Additional metadata related to the context</summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}