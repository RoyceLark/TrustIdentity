using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TrustIdentity.UI.Pages.Manage;

[Authorize]
public class ExternalLoginsModel : PageModel
{
    public List<AuthenticationScheme> CurrentLogins { get; set; } = new();
    public List<AuthenticationScheme> OtherLogins { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Placeholder: in a real app, we'd check against the user's stored external logins
        var schemes = await HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>().GetAllSchemesAsync();
        OtherLogins = schemes.Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
    }

    public async Task<IActionResult> OnPostRemoveLoginAsync(string provider, string key)
    {
        // Placeholder for removing link
        return RedirectToPage();
    }

    public IActionResult OnPostLinkLoginAsync(string provider)
    {
        // Logic to challenge external provider with the current user context
        var redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, provider);
    }
}
