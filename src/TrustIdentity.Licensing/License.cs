using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrustIdentity.Licensing;

/// <summary>
/// Represents a software license for TrustIdentity
/// </summary>
public class License
{
    /// <summary>Unique identifier for the license</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>The actual serialized/signed key</summary>
    public string LicenseKey { get; set; } = string.Empty;
    /// <summary>Type of license (e.g., Standard, Enterprise)</summary>
    public string LicenseType { get; set; } = "Standard";
    /// <summary>Name of the customer</summary>
    public string CustomerName { get; set; } = string.Empty;
    /// <summary>Email of the customer</summary>
    public string CustomerEmail { get; set; } = string.Empty;
    /// <summary>Date when the license was created</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date when the license expires</summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>List of features enabled by this license</summary>
    public List<string> Features { get; set; } = new();
    /// <summary>Whether the license is currently active</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Store for managing license data
/// </summary>
public interface ILicenseStore
{
    /// <summary>Gets a license by ID</summary>
    Task<License?> GetLicenseAsync(Guid id);
    /// <summary>Gets a license by its key</summary>
    Task<License?> GetLicenseByKeyAsync(string key);
    /// <summary>Saves a license</summary>
    Task SaveLicenseAsync(License license);
    /// <summary>Gets all licenses</summary>
    Task<IEnumerable<License>> GetAllLicensesAsync();
}

/// <summary>
/// Service for generating new licenses
/// </summary>
public interface ILicenseGenerator
{
    /// <summary>Generates a signed license key</summary>
    string GenerateLicense(License licenseData, string privateKeyXml);
}

/// <summary>
/// Service for validating existing licenses
/// </summary>
public interface ILicenseValidator
{
    /// <summary>Validates a license key</summary>
    bool ValidateLicense(string licenseKey, string? publicKeyXml, out License? licenseData);
}
