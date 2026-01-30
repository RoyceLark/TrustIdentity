using System;
using System.Collections.Generic;
using System.Security.Claims;
namespace TrustIdentity.WsFederation.Models;

/// <summary>
/// WS-Federation Sign-In Request
/// </summary>
public class WsFederationSignInRequest
{
    /// <summary>The action (wa)</summary>
    public string? Wa { get; set; } = WsFederationConstants.Actions.SignIn;
    /// <summary>The realm (wtrealm)</summary>
    public string? Wtrealm { get; set; }
    /// <summary>The reply URL (wreply)</summary>
    public string? Wreply { get; set; }
    /// <summary>The context (wctx)</summary>
    public string? Wctx { get; set; }
    /// <summary>The current time (wct)</summary>
    public string? Wct { get; set; }
    /// <summary>The home realm (whr)</summary>
    public string? Whr { get; set; }
    /// <summary>The request (wreq)</summary>
    public string? Wreq { get; set; }
    /// <summary>The freshness (wfresh)</summary>
    public string? Wfresh { get; set; }
    /// <summary>The authentication type (wauth)</summary>
    public string? Wauth { get; set; }
}

/// <summary>
/// WS-Federation Sign-In Response
/// </summary>
public class WsFederationSignInResponse
{
    /// <summary>The action (wa)</summary>
    public string? Wa { get; set; } = WsFederationConstants.Actions.SignIn;
    /// <summary>The result (wresult) containing the token</summary>
    public string? Wresult { get; set; }
    /// <summary>The context (wctx)</summary>
    public string? Wctx { get; set; }
}

/// <summary>
/// WS-Trust RequestSecurityToken (RST)
/// </summary>
public class RequestSecurityToken
{
    /// <summary>The request type</summary>
    public string RequestType { get; set; } = WsTrustConstants.RequestTypes.Issue;
    /// <summary>The token type requested</summary>
    public string? TokenType { get; set; }
    /// <summary>The realm applying to the request</summary>
    public string? AppliesTo { get; set; }
    /// <summary>The key type</summary>
    public string? KeyType { get; set; }
    /// <summary>The key size</summary>
    public int? KeySize { get; set; }
    /// <summary>The requested lifetime</summary>
    public DateTime? Lifetime { get; set; }
    /// <summary>The requested claims</summary>
    public string? Claims { get; set; }
}

/// <summary>
/// WS-Trust RequestSecurityTokenResponse (RSTR)
/// </summary>
public class RequestSecurityTokenResponse
{
    /// <summary>The request type</summary>
    public string RequestType { get; set; } = WsTrustConstants.RequestTypes.Issue;
    /// <summary>The token type</summary>
    public string? TokenType { get; set; }
    /// <summary>The issued security token</summary>
    public string? RequestedSecurityToken { get; set; }
    /// <summary>The token lifetime</summary>
    public DateTime? Lifetime { get; set; }
    /// <summary>The realm applying to the response</summary>
    public string? AppliesTo { get; set; }
    /// <summary>Attached reference</summary>
    public string? RequestedAttachedReference { get; set; }
    /// <summary>Unattached reference</summary>
    public string? RequestedUnattachedReference { get; set; }
}

/// <summary>
/// Security Token (SAML 1.1 or SAML 2.0)
/// </summary>
public class SecurityToken
{
    /// <summary>The unique identifier</summary>
    public string Id { get; set; } = $"_{Guid.NewGuid():N}";
    /// <summary>When the token was issued</summary>
    public DateTime IssueInstant { get; set; } = DateTime.UtcNow;
    /// <summary>When the token starts being valid</summary>
    public DateTime NotBefore { get; set; } = DateTime.UtcNow;
    /// <summary>When the token expires</summary>
    public DateTime NotOnOrAfter { get; set; } = DateTime.UtcNow.AddHours(1);
    /// <summary>The token issuer</summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>The token audience</summary>
    public string Audience { get; set; } = string.Empty;
    /// <summary>The token subject</summary>
    public string Subject { get; set; } = string.Empty;
    /// <summary>The token claims</summary>
    public List<Claim> Claims { get; set; } = new();
}

/// <summary>
/// WS-Federation Metadata
/// </summary>
public class WsFederationMetadata
{
    /// <summary>The entity ID</summary>
    public string EntityId { get; set; } = string.Empty;
    /// <summary>The passive requestor endpoints</summary>
    public List<string> PassiveRequestorEndpoints { get; set; } = new();
    /// <summary>The security token service endpoints</summary>
    public List<string> SecurityTokenServiceEndpoints { get; set; } = new();
    /// <summary>The key descriptors</summary>
    public List<WsFederationKeyDescriptor> KeyDescriptors { get; set; } = new();
    /// <summary>The claim types</summary>
    public List<WsFederationClaimType> ClaimTypes { get; set; } = new();
}

/// <summary>
/// WS-Federation Key Descriptor
/// </summary>
public class WsFederationKeyDescriptor
{
    /// <summary>The use of the key (e.g. signing)</summary>
    public string Use { get; set; } = "signing";
    /// <summary>The base64 encoded X.509 certificate</summary>
    public string X509Certificate { get; set; } = string.Empty;
}

/// <summary>
/// WS-Federation Claim Type
/// </summary>
public class WsFederationClaimType
{
    /// <summary>The claim type URI</summary>
    public string Uri { get; set; } = string.Empty;
    /// <summary>The display name</summary>
    public string? DisplayName { get; set; }
    /// <summary>The description</summary>
    public string? Description { get; set; }
}