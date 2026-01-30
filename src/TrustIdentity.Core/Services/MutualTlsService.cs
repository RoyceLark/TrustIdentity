using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Mutual TLS (mTLS) service - RFC 8705
/// </summary>
public class MutualTlsService : IMutualTlsService
{
    private readonly ILogger<MutualTlsService> _logger;

    /// <summary>
    /// Initializes a new instance of MutualTlsService
    /// </summary>
    public MutualTlsService(ILogger<MutualTlsService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates a client certificate
    /// </summary>
    public Task<MutualTlsValidationResult> ValidateClientCertificateAsync(X509Certificate2 certificate, Client client)
    {
        var result = new MutualTlsValidationResult();

        try
        {
            // Check if certificate is null
            if (certificate == null)
            {
                result.Error = "No client certificate provided";
                return Task.FromResult(result);
            }

            // Validate certificate is not expired
            var now = DateTime.UtcNow;
            if (certificate.NotBefore > now || certificate.NotAfter < now)
            {
                result.Error = "Client certificate is expired or not yet valid";
                _logger.LogWarning("Certificate validation failed: expired or not yet valid");
                return Task.FromResult(result);
            }

            // Validate certificate chain
            using var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

            var chainBuilt = chain.Build(certificate);
            if (!chainBuilt)
            {
                var errors = string.Join(", ", chain.ChainStatus.Select(s => s.StatusInformation));
                result.Error = $"Certificate chain validation failed: {errors}";
                _logger.LogWarning("Certificate chain validation failed: {Errors}", errors);
                return Task.FromResult(result);
            }

            // Check if client has certificate thumbprint configured
            // In a real implementation, you would check against client.AllowedCertificateThumbprints
            var thumbprint = GetCertificateThumbprintSync(certificate);
            
            result.IsValid = true;
            result.Thumbprint = thumbprint;
            result.Subject = certificate.Subject;

            _logger.LogInformation("Client certificate validated successfully for subject {Subject}", certificate.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating client certificate");
            result.Error = $"Validation error: {ex.Message}";
        }

        return Task.FromResult(result);
    }

    /// <summary>
    /// Extracts the certificate thumbprint for token binding
    /// </summary>
    public Task<string> GetCertificateThumbprintAsync(X509Certificate2 certificate)
    {
        var thumbprint = GetCertificateThumbprintSync(certificate);
        return Task.FromResult(thumbprint);
    }

    /// <summary>
    /// Creates a certificate-bound access token
    /// </summary>
    public Task<string> CreateCertificateBoundTokenAsync(string accessToken, string certificateThumbprint)
    {
        // In a real implementation, this would add the x5t#S256 claim to the access token
        // For now, we'll just return a marker that this token is certificate-bound
        var boundToken = $"{accessToken}:mtls:{certificateThumbprint}";
        
        _logger.LogInformation("Created certificate-bound token with thumbprint {Thumbprint}", certificateThumbprint);
        
        return Task.FromResult(boundToken);
    }

    /// <summary>
    /// Gets certificate thumbprint synchronously
    /// </summary>
    private string GetCertificateThumbprintSync(X509Certificate2 certificate)
    {
        // Generate SHA-256 thumbprint (x5t#S256)
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(certificate.RawData);
        return Base64UrlEncoder.Encode(hash);
    }
}
