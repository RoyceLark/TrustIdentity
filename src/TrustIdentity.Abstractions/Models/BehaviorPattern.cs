namespace TrustIdentity.Abstractions.Models;

using System;
using System.Collections.Generic;
/// <summary>
/// Represents a learned or observed behavior pattern for a user
/// </summary>
public class BehaviorPattern
{
    /// <summary>Unique ID of the pattern</summary>
    public string PatternId { get; set; } = string.Empty;
    /// <summary>The user ID</summary>
    public string UserId { get; set; } = string.Empty;
    /// <summary>The IP address where the pattern was observed</summary>
    public string IpAddress { get; set; } = string.Empty;
    /// <summary>The user agent observed</summary>
    public string UserAgent { get; set; } = string.Empty;
    /// <summary>Timestamp of the observation</summary>
    public DateTime Timestamp { get; set; }
    /// <summary>The action being performed</summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>The type of pattern (e.g. LoginTime, Location)</summary>
    public string PatternType { get; set; } = string.Empty;
    /// <summary>Number of failed attempts observed in this pattern</summary>
    public int FailedAttempts { get; set; }
    /// <summary>Number of location changes observed</summary>
    public int LocationChanges { get; set; }
    /// <summary>Number of device changes observed</summary>
    public int DeviceChanges { get; set; }
    /// <summary>Additional metadata related to the pattern</summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}