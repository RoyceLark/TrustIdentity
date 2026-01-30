using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Licensing;

namespace TrustIdentity.Licensing.Manager.Pages;

public class ValidateModel : PageModel
{
    private readonly ILicenseValidator _validator;

    public ValidateModel(ILicenseValidator validator)
    {
        _validator = validator;
    }

    [BindProperty]
    public string LicenseKey { get; set; } = string.Empty;

    public License? ValidatedLicense { get; set; }
    public bool? IsValid { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    public void OnPost()
    {
        if (string.IsNullOrWhiteSpace(LicenseKey))
        {
            Error = "Please paste a license key.";
            IsValid = false;
            return;
        }

        // Now validating using ONLY the LicenseKey. 
        // The validator uses the built-in TrustIdentity Public Key automatically.
        IsValid = _validator.ValidateLicense(LicenseKey, null, out var license);
        ValidatedLicense = license;
        
        if (IsValid == false)
        {
            Error = "This license key is invalid, expired, or has been altered.";
        }
    }
}
