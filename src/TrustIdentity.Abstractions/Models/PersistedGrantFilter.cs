namespace TrustIdentity.Abstractions.Models;

/// <summary>
/// Filter for searching persisted grants
/// </summary>
public class PersistedGrantFilter
{
    /// <summary>The client ID to filter by</summary>
    public string? ClientId { get; set; }
    /// <summary>The session ID to filter by</summary>
    public string? SessionId { get; set; }
    /// <summary>The subject ID to filter by</summary>
    public string? SubjectId { get; set; }
    /// <summary>The grant type to filter by (e.g. refresh_token)</summary>
    public string? Type { get; set; }
}