using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for Mutual TLS (mTLS) client authentication - RFC 8705
/// </summary>
public interface IMutualTlsService
{
    /// <summary>
    /// Validates a client certificate
    /// </summary>
    /// <param name="certificate">The client certificate</param>
    /// <param name="client">The client configuration</param>
    /// <returns>Validation result</returns>
    Task<MutualTlsValidationResult> ValidateClientCertificateAsync(X509Certificate2 certificate, Client client);
    
    /// <summary>
    /// Extracts the certificate thumbprint for token binding
    /// </summary>
    /// <param name="certificate">The client certificate</param>
    /// <returns>Certificate thumbprint</returns>
    Task<string> GetCertificateThumbprintAsync(X509Certificate2 certificate);
    
    /// <summary>
    /// Creates a certificate-bound access token
    /// </summary>
    /// <param name="accessToken">The access token</param>
    /// <param name="certificateThumbprint">The certificate thumbprint</param>
    /// <returns>Certificate-bound token</returns>
    Task<string> CreateCertificateBoundTokenAsync(string accessToken, string certificateThumbprint);
}

/// <summary>
/// Result of mTLS validation
/// </summary>
public class MutualTlsValidationResult
{
    /// <summary>Whether validation succeeded</summary>
    public bool IsValid { get; set; }
    
    /// <summary>Error description if validation failed</summary>
    public string? Error { get; set; }
    
    /// <summary>The certificate thumbprint</summary>
    public string? Thumbprint { get; set; }
    
    /// <summary>The certificate subject</summary>
    public string? Subject { get; set; }
}
