using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Licensing;

namespace TrustIdentity.Licensing.Manager.Pages;

public class CreateModel : PageModel
{
    private readonly ILicenseStore _store;
    private readonly ILicenseGenerator _generator;
    private readonly IConfiguration _configuration;

    public CreateModel(ILicenseStore store, ILicenseGenerator generator, IConfiguration configuration)
    {
        _store = store;
        _generator = generator;
        _configuration = configuration;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    public bool HasConfiguredKey { get; set; }

    public class InputModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string LicenseType { get; set; } = "Standard";
        public int ValidityDays { get; set; } = 365;
        public string Features { get; set; } = string.Empty;
        public string? PrivateKey { get; set; }
    }

    public void OnGet()
    {
        HasConfiguredKey = !string.IsNullOrEmpty(_configuration["Licensing:PrivateKey"]);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var privateKey = _configuration["Licensing:PrivateKey"];
        if (string.IsNullOrEmpty(privateKey))
        {
            privateKey = Input.PrivateKey;
        }

        if (string.IsNullOrEmpty(privateKey))
        {
            ModelState.AddModelError("Input.PrivateKey", "Private Key is required (configure it or paste it).");
            return Page();
        }

        var license = new License
        {
            CustomerName = Input.CustomerName,
            CustomerEmail = Input.CustomerEmail,
            LicenseType = Input.LicenseType,
            ExpiresAt = DateTime.UtcNow.AddDays(Input.ValidityDays),
            Features = Input.Features.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList()
        };

        try 
        {
            license.LicenseKey = _generator.GenerateLicense(license, privateKey);
            await _store.SaveLicenseAsync(license);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
             ModelState.AddModelError("", "Error generating license: " + ex.Message);
             return Page();
        }
    }
}
