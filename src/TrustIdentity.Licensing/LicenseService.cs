using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TrustIdentity.Licensing;

/// <summary>
/// Service implementation for license generation and validation
/// </summary>
public class LicenseService : ILicenseGenerator, ILicenseValidator
{
    /// <inheritdoc/>
    public string GenerateLicense(License licenseData, string privateKeyXml)
    {
        var payload = new
        {
            licenseData.Id,
            licenseData.LicenseType,
            licenseData.CustomerName,
            licenseData.CustomerEmail,
            licenseData.CreatedAt,
            licenseData.ExpiresAt,
            licenseData.Features
        };

        var json = JsonSerializer.Serialize(payload);
        var dataBytes = Encoding.UTF8.GetBytes(json);

        using var rsa = RSA.Create();
        // Ignoring platform compat warning for XML on non-windows as User is on Windows
        // In .NET Core, ToXmlString/FromXmlString works on Linux too usually (implemented via OpenSSL)
        rsa.FromXmlString(privateKeyXml);

        var signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return $"{Convert.ToBase64String(dataBytes)}.{Convert.ToBase64String(signatureBytes)}";
    }

    /// <inheritdoc/>
    public bool ValidateLicense(string licenseKey, string? publicKeyXml, out License? licenseData)
    {
        licenseData = null;
        if (string.IsNullOrWhiteSpace(licenseKey)) return false;

        try
        {
            var parts = licenseKey.Split('.');
            if (parts.Length != 2) return false;

            var dataBytes = Convert.FromBase64String(parts[0]);
            var signatureBytes = Convert.FromBase64String(parts[1]);

            using var rsa = RSA.Create();
            rsa.FromXmlString(string.IsNullOrWhiteSpace(publicKeyXml) ? KeyAuthority.DefaultPublicKeyXml : publicKeyXml);

            if (!rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            {
                // Signature invalid
                return false;
            }

            var json = Encoding.UTF8.GetString(dataBytes);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            licenseData = JsonSerializer.Deserialize<License>(json, options);

            if (licenseData == null) return false;
            
            // Check Expiry
            if (licenseData.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a new RSA key pair for license signing
    /// </summary>
    /// <returns>A tuple containing (PrivateKey, PublicKey) in XML format</returns>
    public static (string PrivateKey, string PublicKey) GenerateKeys()
    {
        using var rsa = RSA.Create(2048);
        return (rsa.ToXmlString(true), rsa.ToXmlString(false));
    }
}
