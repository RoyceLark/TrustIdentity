using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Licensing;

namespace TrustIdentity.Licensing.Manager.Pages;

public class KeysModel : PageModel
{
    public string PrivateKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;

    public void OnGet()
    {
        (PrivateKey, PublicKey) = LicenseService.GenerateKeys();
    }
}
