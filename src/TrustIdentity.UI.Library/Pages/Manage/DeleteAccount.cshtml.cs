using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Stores;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace TrustIdentity.UI.Pages.Manage;

[Authorize]
public class DeleteAccountModel : PageModel
{
    private readonly IUserStore _userStore;

    public DeleteAccountModel(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public async Task<IActionResult> OnPostAsync(string password)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return NotFound();

        var user = await _userStore.FindBySubjectIdAsync(userId);
        if (user == null) return NotFound();

        if (await _userStore.ValidateCredentialsAsync(user.Username, password))
        {
            await _userStore.DeleteUserAsync(userId);
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Index");
        }

        ModelState.AddModelError(string.Empty, "Invalid password.");
        return Page();
    }
}
