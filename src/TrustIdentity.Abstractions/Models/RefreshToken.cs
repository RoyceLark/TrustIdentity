using System;
using System.Collections.Generic;

namespace TrustIdentity.Abstractions.Models;

/// <summary>
/// Represents an OAuth 2.0 refresh token
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// ID of the client associated with the token
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Subject ID of the authenticated user
    /// </summary>
    public string OriginalSubjectId { get; set; } = string.Empty;
    
    /// <summary>
    /// Session ID associated with the token
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Time when the token was created
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Lifetime of the token in seconds
    /// </summary>
    public int Lifetime { get; set; } // in seconds
    
    /// <summary>
    /// Scopes authorized for this token
    /// </summary>
    public List<string> AuthorizedScopes { get; set; } = new();
    
    /// <summary>
    /// Time when the token was last updated
    /// </summary>
    public DateTime? Updated { get; set; }

    /// <summary>
    /// Check if the refresh token has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > CreationTime.AddSeconds(Lifetime);

    /// <summary>
    /// Compatibility property for SubjectId (maps to OriginalSubjectId)
    /// </summary>
    public string SubjectId 
    { 
        get => OriginalSubjectId; 
        set => OriginalSubjectId = value; 
    }
}