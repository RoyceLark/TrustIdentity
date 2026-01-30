namespace TrustIdentity.Abstractions.Models;

using System;

/// <summary>
/// Represents a persisted grant (e.g. authorization code, refresh token, reference token)
/// </summary>
public class PersistedGrant
{
    /// <summary>
    /// The unique key of the grant
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of grant (e.g. authorization_code, refresh_token)
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// The subject ID of the user associated with the grant
    /// </summary>
    public string? SubjectId { get; set; }
    
    /// <summary>
    /// The session ID associated with the grant
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// The client ID associated with the grant
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// The time when the grant was created
    /// </summary>
    public DateTime CreationTime { get; set; }
    
    /// <summary>
    /// The time when the grant expires
    /// </summary>
    public DateTime? Expiration { get; set; }
    
    /// <summary>
    /// The time when the grant was consumed (if applicable)
    /// </summary>
    public DateTime? ConsumedTime { get; set; }
    
    /// <summary>
    /// Json Data associated with the grant
    /// </summary>
    public string Data { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the grant
    /// </summary>
    public string? Description { get; set; }
}