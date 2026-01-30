using System;
using System.Collections.Generic;

namespace TrustIdentity.Abstractions.Models;

/// <summary>
/// Represents a user session for server-side session management
/// </summary>
public class UserSession
{
    /// <summary>The session ID</summary>
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>The subject ID of the user</summary>
    public string SubjectId { get; set; } = string.Empty;
    
    /// <summary>The display name of the user</summary>
    public string? DisplayName { get; set; }
    
    /// <summary>The list of client IDs associated with this session</summary>
    public List<string> ClientIds { get; set; } = new();
    
    /// <summary>When the session was created</summary>
    public DateTime Created { get; set; }
    
    /// <summary>When the session expires</summary>
    public DateTime Expires { get; set; }
    
    /// <summary>When the session was last renewed</summary>
    public DateTime? Renewed { get; set; }
    
    /// <summary>Metadata associated with the session (e.g. User Agent, IP)</summary>
    public string? Data { get; set; }
}
