namespace TrustIdentity.Abstractions.Models;

using System;
/// <summary>
/// Represents a user grant (e.g. consent, or authorization code)
/// </summary>
public class Grant
{
    /// <summary>The unique key for the grant</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>The grant type</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>The subject ID of the user</summary>
    public string SubjectId { get; set; } = string.Empty;
    /// <summary>The client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>Creation time</summary>
    public DateTime CreationTime { get; set; }
    /// <summary>Expiration time</summary>
    public DateTime? Expiration { get; set; }
    /// <summary>Serialized data associated with the grant</summary>
    public string? Data { get; set; }
}