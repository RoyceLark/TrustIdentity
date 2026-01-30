namespace TrustIdentity.Abstractions.Models;

using System.Collections.Generic;
/// <summary>
/// Represents an API resource
/// </summary>
public class ApiResource
{
    /// <summary>The name of the API</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>The display name</summary>
    public string? DisplayName { get; set; }
    /// <summary>The description</summary>
    public string? Description { get; set; }
    /// <summary>Whether the API is enabled</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>The scopes associated with the API</summary>
    public List<string> Scopes { get; set; } = new();
    /// <summary>The secrets for the API</summary>
    public List<Secret> ApiSecrets { get; set; } = new();
    /// <summary>The user claims that should be included in the token</summary>
    public List<string> UserClaims { get; set; } = new();
}