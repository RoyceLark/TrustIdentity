using System.Collections.Generic;

namespace TrustIdentity.Core.Models;

/// <summary>
/// Represents a request for user consent
/// </summary>
public class ConsentRequest
{
    /// <summary>The Client ID requesting consent</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The Display Name of the client</summary>
    public string ClientName { get; set; } = string.Empty;
    /// <summary>The logo URL of the client</summary>
    public string? ClientLogoUrl { get; set; }
    /// <summary>The list of scopes being requested</summary>
    public List<ConsentScope> Scopes { get; set; } = new();
}

/// <summary>
/// Represents a scope presented on the consent screen
/// </summary>
public class ConsentScope
{
    /// <summary>The scope identifier</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>The display name of the scope</summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>Description of what the scope allows</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Whether this scope is required</summary>
    public bool Required { get; set; }
    /// <summary>Whether this scope is selected by default</summary>
    public bool Default { get; set; }
}
