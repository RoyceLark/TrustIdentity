using System.Collections.Generic;
using System.Security.Claims;
namespace TrustIdentity.Abstractions.Models;

/// <summary>
/// Request for creating a token
/// </summary>
public class TokenCreationRequest
{
    /// <summary>The client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The principal subject</summary>
    public ClaimsPrincipal? Subject { get; set; }
    /// <summary>Validated scopes for the token</summary>
    public IEnumerable<string> ValidatedScopes { get; set; } = new List<string>();
    /// <summary>The session ID</summary>
    public string? SessionId { get; set; }
}