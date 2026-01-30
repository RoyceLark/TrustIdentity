using System.Collections.Generic;

namespace TrustIdentity.Abstractions.Models;

/// <summary>
/// Result of resource validation (RFC 8707)
/// </summary>
public class ResourceValidationResult
{
    /// <summary>Whether validation succeeded</summary>
    public bool Success { get; set; }
    
    /// <summary>Error code if validation failed</summary>
    public string? Error { get; set; }
    
    /// <summary>Error description if validation failed</summary>
    public string? ErrorDescription { get; set; }
    
    /// <summary>Validated resources</summary>
    public List<ApiResource> Resources { get; set; } = new();
    
    /// <summary>Parsed and validated scopes</summary>
    public List<string> ParsedScopes { get; set; } = new();
}
