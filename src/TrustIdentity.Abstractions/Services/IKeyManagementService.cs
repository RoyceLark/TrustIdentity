using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for automatic key rotation and management
/// </summary>
public interface IKeyManagementService
{
    /// <summary>
    /// Gets the current active signing key
    /// </summary>
    Task<SigningKey> GetCurrentSigningKeyAsync();
    
    /// <summary>
    /// Gets all valid signing keys (for token validation)
    /// </summary>
    Task<IEnumerable<SigningKey>> GetAllValidKeysAsync();
    
    /// <summary>
    /// Rotates the signing key
    /// </summary>
    Task RotateKeyAsync();
    
    /// <summary>
    /// Gets the JWKS (JSON Web Key Set) for discovery
    /// </summary>
    Task<string> GetJwksAsync();
}

/// <summary>
/// Represents a signing key with metadata
/// </summary>
public class SigningKey
{
    /// <summary>Key identifier</summary>
    public string KeyId { get; set; } = string.Empty;
    
    /// <summary>The RSA key</summary>
    public RSA? RsaKey { get; set; }
    
    /// <summary>The certificate (if using X509)</summary>
    public X509Certificate2? Certificate { get; set; }
    
    /// <summary>When the key was created</summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>When the key becomes active</summary>
    public DateTime ActiveFrom { get; set; }
    
    /// <summary>When the key expires</summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>Whether this is the current active key</summary>
    public bool IsActive { get; set; }
    
    /// <summary>Algorithm (e.g., RS256)</summary>
    public string Algorithm { get; set; } = "RS256";
}
