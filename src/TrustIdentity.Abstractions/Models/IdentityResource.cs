namespace TrustIdentity.Abstractions.Models;

using System.Collections.Generic;
/// <summary>
/// Represents an Identity resource (e.g. openid, profile)
/// </summary>
public class IdentityResource
{
    /// <summary>The name of the identity resource</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>The display name</summary>
    public string? DisplayName { get; set; }
    /// <summary>The description</summary>
    public string? Description { get; set; }
    /// <summary>Whether the resource is enabled</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>Whether the resource is required</summary>
    public bool Required { get; set; }
    /// <summary>Whether to emphasize the resource on the consent screen</summary>
    public bool Emphasize { get; set; }
    /// <summary>Whether to show the resource in the discovery document</summary>
    public bool ShowInDiscoveryDocument { get; set; } = true;
    /// <summary>The user claims that should be included when this resource is requested</summary>
    public List<string> UserClaims { get; set; } = new();
}