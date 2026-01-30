namespace TrustIdentity.Abstractions.Models;

using System.Collections.Generic;
/// <summary>
/// Represents an API scope
/// </summary>
public class ApiScope
{
    /// <summary>The name of the scope</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>The display name</summary>
    public string? DisplayName { get; set; }
    /// <summary>The description</summary>
    public string? Description { get; set; }
    /// <summary>Whether the scope is required</summary>
    public bool Required { get; set; }
    /// <summary>Whether to emphasize the scope on the consent screen</summary>
    public bool Emphasize { get; set; }
    /// <summary>Whether to show the scope in the discovery document</summary>
    public bool ShowInDiscoveryDocument { get; set; } = true;
    /// <summary>The user claims that should be included when this scope is requested</summary>
    public List<string> UserClaims { get; set; } = new();
}