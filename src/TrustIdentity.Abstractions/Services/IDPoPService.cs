using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for DPoP (Demonstrating Proof-of-Possession) - RFC 9449
/// </summary>
public interface IDPoPService
{
    /// <summary>
    /// Validates a DPoP proof
    /// </summary>
    /// <param name="dpopProof">The DPoP proof JWT</param>
    /// <param name="httpMethod">The HTTP method</param>
    /// <param name="httpUri">The HTTP URI</param>
    /// <returns>Validation result</returns>
    Task<DPoPValidationResult> ValidateDPoPProofAsync(string dpopProof, string httpMethod, string httpUri);
    
    /// <summary>
    /// Generates a DPoP token thumbprint
    /// </summary>
    /// <param name="dpopProof">The DPoP proof JWT</param>
    /// <returns>JWK thumbprint</returns>
    Task<string> GenerateThumbprintAsync(string dpopProof);
    
    /// <summary>
    /// Creates a DPoP-bound access token
    /// </summary>
    /// <param name="accessToken">The access token</param>
    /// <param name="dpopThumbprint">The DPoP JWK thumbprint</param>
    /// <returns>DPoP-bound token</returns>
    Task<string> CreateDPoPBoundTokenAsync(string accessToken, string dpopThumbprint);
}

/// <summary>
/// Result of DPoP proof validation
/// </summary>
public class DPoPValidationResult
{
    /// <summary>Whether validation succeeded</summary>
    public bool IsValid { get; set; }
    
    /// <summary>Error description if validation failed</summary>
    public string? Error { get; set; }
    
    /// <summary>The JWK thumbprint</summary>
    public string? Thumbprint { get; set; }
    
    /// <summary>The public key from the proof</summary>
    public string? PublicKey { get; set; }
}
