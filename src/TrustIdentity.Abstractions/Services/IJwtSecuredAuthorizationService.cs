using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for JWT Secured Authorization Request (JAR) - RFC 9101
/// </summary>
public interface IJwtSecuredAuthorizationService
{
    /// <summary>
    /// Validates a JWT authorization request
    /// </summary>
    /// <param name="requestJwt">The request JWT</param>
    /// <param name="clientId">The client ID</param>
    /// <returns>Validation result with extracted parameters</returns>
    Task<JarValidationResult> ValidateRequestAsync(string requestJwt, string clientId);
    
    /// <summary>
    /// Extracts parameters from a validated request JWT
    /// </summary>
    /// <param name="requestJwt">The request JWT</param>
    /// <returns>Dictionary of request parameters</returns>
    Task<Dictionary<string, string>> ExtractParametersAsync(string requestJwt);
}

/// <summary>
/// Result of JAR validation
/// </summary>
public class JarValidationResult
{
    /// <summary>Whether validation succeeded</summary>
    public bool IsValid { get; set; }
    
    /// <summary>Error description if validation failed</summary>
    public string? Error { get; set; }
    
    /// <summary>Extracted request parameters</summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
    
    /// <summary>The client ID from the request</summary>
    public string? ClientId { get; set; }
}
