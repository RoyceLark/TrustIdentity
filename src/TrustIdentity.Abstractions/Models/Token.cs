namespace TrustIdentity.Abstractions.Models;

using System;
using System.Collections.Generic;
using System.Security.Claims;

/// <summary>
/// Represents a security token (Access token, ID token)
/// </summary>
public class Token
{
    /// <summary>The token issuer</summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>The primary audience</summary>
    public string Audience { get; set; } = string.Empty;
    /// <summary>When the token was issued</summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    /// <summary>When the token expires</summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>Subject ID of the user</summary>
    public string SubjectId { get; set; } = string.Empty;
    /// <summary>Client ID associated with the token</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>Scopes included in the token</summary>
    public List<string> Scopes { get; set; } = new();
    /// <summary>Claims included in the token</summary>
    public List<System.Security.Claims.Claim> Claims { get; set; } = new();
    /// <summary>The access token value</summary>
    public string? AccessTokenValue { get; set; }
    /// <summary>The refresh token value (if applicable)</summary>
    public string? RefreshTokenValue { get; set; }
  
    /// <summary>The list of audiences</summary>
    public List<string> Audiences { get; set; } = new();
    /// <summary>Creation time</summary>
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    /// <summary>Lifetime of the token in seconds</summary>
    public int Lifetime { get; set; }
    /// <summary>Type of the token</summary>
    public string Type { get; set; } = string.Empty;
   
    /// <summary>The token description</summary>
    public string? Description { get; set; }
    /// <summary>The access token type (e.g. Jwt, Reference)</summary>
    public string? AccessTokenType { get; set; }
    /// <summary>The principal subject of the token</summary>
    public ClaimsIdentity? Subject { get; set; }
    /// <summary>Confirmation method (e.g. cnf)</summary>
    public string? ConfirmationMethod { get; set; }
    /// <summary>DPoP JWK Thumbprint (jkt)</summary>
    public string? DPoPThumbprint { get; set; }
    /// <summary>Version of the token</summary>
    public string Version { get; set; } = "1.0";
    /// <summary>Additional properties</summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();


}