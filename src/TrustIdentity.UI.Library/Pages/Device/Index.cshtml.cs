using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Services;
using System.Threading.Tasks;

namespace TrustIdentity.UI.Pages.Device;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IDeviceFlowService _deviceFlowService;

    public IndexModel(IDeviceFlowService deviceFlowService)
    {
        _deviceFlowService = deviceFlowService;
    }

    [BindProperty]
    public string UserCode { get; set; } = string.Empty;

    public void OnGet(string? userCode = null)
    {
        UserCode = userCode ?? string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(UserCode))
        {
            ModelState.AddModelError("", "User code is required");
            return Page();
        }

        var result = await _deviceFlowService.ValidateUserCodeAsync(UserCode);
        if (!result.Success)
        {
            ModelState.AddModelError("", "Invalid user code");
            return Page();
        }

        // Redirect to consent page for this device flow request
        return RedirectToPage("/Consent/Index", new { returnUrl = result.ConsentReturnUrl });
    }
}
