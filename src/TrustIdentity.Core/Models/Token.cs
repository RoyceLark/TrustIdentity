using System;
using System.Collections.Generic;
using System.Security.Claims;
namespace TrustIdentity.Core.Models;

/// <summary>
/// Represents a security token
/// </summary>
public class Token
{
    /// <summary>The token issuer</summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>The primary audience</summary>
    public string? Audience { get; set; }
    /// <summary>The list of audiences</summary>
    public List<string> Audiences { get; set; } = new();
    /// <summary>When the token was created</summary>
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    /// <summary>The token lifetime in seconds</summary>
    public int Lifetime { get; set; }
    /// <summary>The token type</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>The client ID associated with the token</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The token description</summary>
    public string? Description { get; set; }
    /// <summary>The access token type</summary>
    public string? AccessTokenType { get; set; }
    /// <summary>The user subject</summary>
    public ClaimsIdentity? Subject { get; set; }
    /// <summary>The token claims</summary>
    public List<Claim> Claims { get; set; } = new();
    /// <summary>The confirmation method</summary>
    public string? ConfirmationMethod { get; set; }
    /// <summary>The token version</summary>
    public string Version { get; set; } = "1.0";
    /// <summary>Additional properties for the token</summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
}

/// <summary>
/// Represents a refresh token
/// </summary>
public class RefreshToken : Token
{
    /// <summary>The original subject ID</summary>
    public string OriginalSubjectId { get; set; } = string.Empty;
    /// <summary>The session ID</summary>
    public string? SessionId { get; set; }
    /// <summary>The scopes authorized for this refresh token</summary>
    public List<string> AuthorizedScopes { get; set; } = new();
    /// <summary>When the token was consumed</summary>
    public DateTime? ConsumedTime { get; set; }
}

/// <summary>
/// Represents an authorization code
/// </summary>
public class AuthorizationCode
{
    /// <summary>The client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The subject ID</summary>
    public string? Subject { get; set; }
    /// <summary>The session ID</summary>
    public string? SessionId { get; set; }
    /// <summary>When the code was created</summary>
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    /// <summary>The lifetime in seconds</summary>
    public int Lifetime { get; set; }
    /// <summary>The redirect URI</summary>
    public string? RedirectUri { get; set; }
    /// <summary>The requested scopes</summary>
    public List<string> RequestedScopes { get; set; } = new();
    /// <summary>Whether it's an OpenID request</summary>
    public bool IsOpenId { get; set; }
    /// <summary>The nonce value</summary>
    public string? Nonce { get; set; }
    /// <summary>The PKCE code challenge</summary>
    public string? CodeChallenge { get; set; }
    /// <summary>The PKCE code challenge method</summary>
    public string? CodeChallengeMethod { get; set; }
    /// <summary>Additional properties for the authorization code</summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}
