using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Core.Services;

namespace TrustIdentity.UI.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly AccountService _accountService;

    public ConfirmEmailModel(AccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<IActionResult> OnGetAsync(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var result = await _accountService.VerifyEmailAsync(code);
        return result ? Page() : RedirectToPage("/Error");
    }
}
