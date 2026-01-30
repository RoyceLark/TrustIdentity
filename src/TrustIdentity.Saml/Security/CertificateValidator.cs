using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace TrustIdentity.Saml.Security;

/// <summary>
/// Advanced X.509 Certificate Validation for SAML
/// </summary>
public class CertificateValidator
{
    private readonly ILogger<CertificateValidator> _logger;
    private readonly CertificateValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the CertificateValidator
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="options">Validation options</param>
    public CertificateValidator(
        ILogger<CertificateValidator> logger,
        CertificateValidationOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new CertificateValidationOptions();
    }

    /// <summary>
    /// Comprehensive certificate validation
    /// </summary>
    /// <param name="certificate">The certificate to validate</param>
    /// <returns>Validation result</returns>
    public CertificateValidationResult ValidateCertificate(X509Certificate2 certificate)
    {
        var result = new CertificateValidationResult { IsValid = true };

        try
        {
            // 1. Check if certificate has private key (for signing operations)
            if (_options.RequirePrivateKey && !certificate.HasPrivateKey)
            {
                result.IsValid = false;
                result.Errors.Add("Certificate does not contain a private key");
                return result;
            }

            // 2. Check expiration with clock skew tolerance
            if (!ValidateExpiration(certificate, out var expirationError))
            {
                result.IsValid = false;
                result.Errors.Add(expirationError);
                if (!_options.AllowExpiredCertificates)
                    return result;
            }

            // 3. Validate key usage
            if (_options.ValidateKeyUsage && !ValidateKeyUsage(certificate, out var keyUsageError))
            {
                result.IsValid = false;
                result.Errors.Add(keyUsageError);
            }

            // 4. Validate certificate chain
            if (_options.ValidateChain && !ValidateCertificateChain(certificate, out var chainErrors))
            {
                result.IsValid = false;
                result.Errors.AddRange(chainErrors);
            }

            // 5. Check certificate revocation (CRL/OCSP)
            if (_options.CheckRevocation && !CheckRevocationStatus(certificate, out var revocationError))
            {
                result.IsValid = false;
                result.Errors.Add(revocationError);
            }

            // 6. Validate basic constraints
            if (_options.ValidateBasicConstraints && !ValidateBasicConstraints(certificate, out var constraintError))
            {
                result.IsValid = false;
                result.Errors.Add(constraintError);
            }

            // 7. Validate subject alternative names (if specified)
            if (_options.RequiredSubjectAlternativeNames?.Any() == true)
            {
                if (!ValidateSubjectAlternativeNames(certificate, out var sanError))
                {
                    result.IsValid = false;
                    result.Errors.Add(sanError);
                }
            }

            if (result.IsValid)
            {
                _logger.LogInformation("Certificate validation successful for: {Subject}", certificate.Subject);
            }
            else
            {
                _logger.LogWarning("Certificate validation failed for: {Subject}. Errors: {Errors}", 
                    certificate.Subject, string.Join("; ", result.Errors));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during certificate validation");
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
            return result;
        }
    }

    private bool ValidateExpiration(X509Certificate2 certificate, out string error)
    {
        var now = DateTime.UtcNow;
        var notBefore = certificate.NotBefore.ToUniversalTime() - _options.ClockSkewTolerance;
        var notAfter = certificate.NotAfter.ToUniversalTime() + _options.ClockSkewTolerance;

        if (now < notBefore)
        {
            error = $"Certificate not yet valid. Valid from: {certificate.NotBefore}";
            return false;
        }

        if (now > notAfter)
        {
            error = $"Certificate expired on: {certificate.NotAfter}";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private bool ValidateKeyUsage(X509Certificate2 certificate, out string error)
    {
        foreach (var extension in certificate.Extensions)
        {
            if (extension is X509KeyUsageExtension keyUsage)
            {
                // For SAML signing, we need DigitalSignature
                if (!keyUsage.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature))
                {
                    error = "Certificate does not have DigitalSignature key usage";
                    return false;
                }
                
                error = string.Empty;
                return true;
            }
        }

        error = "Certificate does not contain KeyUsage extension";
        return false;
    }

    private bool ValidateCertificateChain(X509Certificate2 certificate, out List<string> errors)
    {
        errors = new List<string>();

        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Handled separately
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
        
        if (_options.TrustedRootCertificates?.Any() == true)
        {
            chain.ChainPolicy.ExtraStore.AddRange(_options.TrustedRootCertificates.ToArray());
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
        }

        var isValid = chain.Build(certificate);

        if (!isValid)
        {
            foreach (var status in chain.ChainStatus)
            {
                if (status.Status != X509ChainStatusFlags.NoError)
                {
                    errors.Add($"Chain validation error: {status.Status} - {status.StatusInformation}");
                }
            }
        }

        return isValid || errors.Count == 0;
    }

    private bool CheckRevocationStatus(X509Certificate2 certificate, out string error)
    {
        try
        {
            using var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            
            var isValid = chain.Build(certificate);

            foreach (var status in chain.ChainStatus)
            {
                if (status.Status == X509ChainStatusFlags.Revoked)
                {
                    error = "Certificate has been revoked";
                    return false;
                }
                
                if (status.Status == X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    _logger.LogWarning("Unable to determine revocation status for certificate: {Subject}", 
                        certificate.Subject);
                }
            }

            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Revocation check failed: {ex.Message}";
            _logger.LogWarning(ex, "Failed to check certificate revocation status");
            return !_options.RequireRevocationCheck;
        }
    }

    private bool ValidateBasicConstraints(X509Certificate2 certificate, out string error)
    {
        foreach (var extension in certificate.Extensions)
        {
            if (extension is X509BasicConstraintsExtension basicConstraints)
            {
                // For end-entity certificates, CA should be false
                if (basicConstraints.CertificateAuthority && !_options.AllowCertificateAuthority)
                {
                    error = "Certificate is marked as CA but should be end-entity";
                    return false;
                }
                
                error = string.Empty;
                return true;
            }
        }

        error = string.Empty;
        return true; // Extension is optional
    }

    private bool ValidateSubjectAlternativeNames(X509Certificate2 certificate, out string error)
    {
        foreach (var extension in certificate.Extensions)
        {
            if (extension.Oid?.Value == "2.5.29.17") // Subject Alternative Name OID
            {
                var sanExtension = new X509Extension(extension.Oid, extension.RawData, extension.Critical);
                var sanString = sanExtension.Format(false);
                
                foreach (var requiredName in _options.RequiredSubjectAlternativeNames!)
                {
                    if (!sanString.Contains(requiredName, StringComparison.OrdinalIgnoreCase))
                    {
                        error = $"Certificate SAN does not contain required name: {requiredName}";
                        return false;
                    }
                }
                
                error = string.Empty;
                return true;
            }
        }

        error = "Certificate does not contain Subject Alternative Name extension";
        return false;
    }
}

/// <summary>
/// Configuration options for certificate validation
/// </summary>
public class CertificateValidationOptions
{
    /// <summary>
    /// Require a private key
    /// </summary>
    public bool RequirePrivateKey { get; set; } = false;
    
    /// <summary>
    /// Validate KeyUsage extension
    /// </summary>
    public bool ValidateKeyUsage { get; set; } = true;
    
    /// <summary>
    /// Validate certificate chain of trust
    /// </summary>
    public bool ValidateChain { get; set; } = true;
    
    /// <summary>
    /// Check for revocation (CRL/OCSP)
    /// </summary>
    public bool CheckRevocation { get; set; } = true;
    
    /// <summary>
    /// Fail if revocation information is unavailable
    /// </summary>
    public bool RequireRevocationCheck { get; set; } = false; // Don't fail if CRL/OCSP unavailable
    
    /// <summary>
    /// Validate BasicConstraints extension
    /// </summary>
    public bool ValidateBasicConstraints { get; set; } = true;
    
    /// <summary>
    /// Allow expired certificates (e.g., for testing)
    /// </summary>
    public bool AllowExpiredCertificates { get; set; } = false;
    
    /// <summary>
    /// Allow CA certificates
    /// </summary>
    public bool AllowCertificateAuthority { get; set; } = false;
    
    /// <summary>
    /// Clock skew tolerance for expiration check
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// List of trusted root certificates
    /// </summary>
    public List<X509Certificate2>? TrustedRootCertificates { get; set; }
    
    /// <summary>
    /// Required names to appear in Subject Alternative Names
    /// </summary>
    public List<string>? RequiredSubjectAlternativeNames { get; set; }
}

/// <summary>
/// Result of certificate validation
/// </summary>
public class CertificateValidationResult
{
    /// <summary>
    /// Valid status
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// List of validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}