using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TrustIdentity.UI.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(ILogger<LogoutModel> logger)
    {
        _logger = logger;
    }

    public string? LogoutId { get; set; }

    public void OnGet(string? logoutId)
    {
        LogoutId = logoutId;
    }

    public async Task<IActionResult> OnPostAsync(string? logoutId)
    {
        await HttpContext.SignOutAsync("TrustIdentity");
        _logger.LogInformation("User logged out.");

        // In a real OIDC system, we might need to handle PostLogoutRedirectUri from logoutId
        return RedirectToPage("/Account/LoggedOut", new { logoutId });
    }
}
