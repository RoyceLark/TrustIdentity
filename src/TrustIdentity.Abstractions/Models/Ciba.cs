using System;
using System.Collections.Generic;

namespace TrustIdentity.Abstractions.Models;

/// <summary>
/// Represents a CIBA (Client Initiated Backchannel Authentication) request.
/// </summary>
public class BackchannelAuthenticationRequest
{
    /// <summary>The unique ID for the request (auth_req_id)</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>The client ID that initiated the request</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The subject ID of the user to be authenticated</summary>
    public string SubjectId { get; set; } = string.Empty;
    /// <summary>The list of requested scopes</summary>
    public List<string> Scopes { get; set; } = new();
    /// <summary>The binding message to be shown to the user</summary>
    public string? BindingMessage { get; set; }
    /// <summary>Whether the request has been approved</summary>
    public bool? IsApproved { get; set; }
    /// <summary>When the request was created</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>When the request expires</summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>Additional data stored as JSON</summary>
    public string? Data { get; set; }
}
