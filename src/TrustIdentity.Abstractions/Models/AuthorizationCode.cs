using System;
using System.Collections.Generic;

namespace TrustIdentity.Abstractions.Models;

/// <summary>
/// Represents an OAuth 2.0 authorization code
/// </summary>
public class AuthorizationCode
{
    /// <summary>
    /// The authorization code string
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Subject ID of the user who granted authorization
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the client requesting authorization
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Redirect URI used in the request
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
    
    /// <summary>
    /// Time when the code was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Time when the code expires
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    
    /// <summary>
    /// Scopes requested in the authorization request
    /// </summary>
    public List<string> Scopes { get; set; } = new();
    
    /// <summary>
    /// Nonce from the request
    /// </summary>
    public string? Nonce { get; set; }
    
    /// <summary>
    /// PKCE code challenge
    /// </summary>
    public string? CodeChallenge { get; set; }
    
    /// <summary>
    /// PKCE code challenge method (e.g. S256)
    /// </summary>
    public string? CodeChallengeMethod { get; set; }
    
    /// <summary>
    /// Check if the authorization code has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}
