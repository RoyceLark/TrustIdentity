using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TrustIdentity.Licensing;

/// <summary>
/// Options for configuring licensing in the application
/// </summary>
public class LicenseOptions
{
    /// <summary>The public key used to verify the license signature</summary>
    public string? PublicKey { get; set; }
    /// <summary>The license key for this installation</summary>
    public string? LicenseKey { get; set; }
}

/// <summary>
/// High-level service for validating the application license status
/// </summary>
public class LicenseValidationService
{
    private readonly ILicenseValidator _validator;
    private readonly LicenseOptions _options;
    private License? _cachedLicense;

    /// <summary>
    /// Initializes a new instance of the LicenseValidationService
    /// </summary>
    public LicenseValidationService(ILicenseValidator validator, IOptions<LicenseOptions> options)
    {
        _validator = validator;
        _options = options.Value;
    }

    /// <summary>
    /// Checks if the provided license is valid
    /// </summary>
    /// <param name="license">The decoded license information if valid</param>
    /// <returns>True if the license is valid and not expired</returns>
    public bool IsValid(out License? license)
    {
        if (_cachedLicense != null)
        {
            license = _cachedLicense;
            return true;
        }

        if (string.IsNullOrEmpty(_options.LicenseKey))
        {
            license = null;
            return false;
        }

        bool valid = _validator.ValidateLicense(_options.LicenseKey, _options.PublicKey, out var result);
        if (valid)
        {
            _cachedLicense = result;
        }
        
        license = result;
        return valid;
    }

    /// <summary>
    /// Checks if the current license enables a specific feature
    /// </summary>
    /// <param name="feature">The feature name to check</param>
    /// <returns>True if the feature is enabled</returns>
    public bool HasFeature(string feature)
    {
        if (IsValid(out var license) && license != null)
        {
            return license.Features.Contains(feature, StringComparer.OrdinalIgnoreCase);
        }
        return false;
    }
}

/// <summary>
/// Extensions for registering licensing services
/// </summary>
public static class LicensingServiceExtensions
{
    /// <summary>
    /// Adds TrustIdentity licensing services to the container
    /// </summary>
    public static IServiceCollection AddTrustLicensing(this IServiceCollection services, Action<LicenseOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<ILicenseValidator, LicenseService>();
        services.AddSingleton<LicenseValidationService>();
        return services;
    }
}
