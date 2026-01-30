using System;
using System.Collections.Generic;

namespace TrustIdentity.Abstractions.Models;

/// <summary>
/// Represents user consent for a client application
/// </summary>
public class UserConsent
{
    /// <summary>The unique ID for the consent record</summary>
    public long Id { get; set; }
    
    /// <summary>The subject ID of the user</summary>
    public string SubjectId { get; set; } = string.Empty;
    
    /// <summary>The client ID the consent is for</summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>The consented scopes (JSON or comma-separated in DB)</summary>
    public List<string> Scopes { get; set; } = new();
    
    /// <summary>When the consent was created</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>When the consent expires</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Whether the user chose to remember this consent</summary>
    public bool RememberConsent { get; set; }
}
