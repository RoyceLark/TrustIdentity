using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Stores;
using System.Security.Claims;

namespace TrustIdentity.UI.Pages.Manage;

[Authorize]
public class ChangePasswordModel : PageModel
{
    private readonly IUserStore _userStore;

    public ChangePasswordModel(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public async Task<IActionResult> OnPostAsync(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "The new password and confirmation password do not match.");
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return NotFound();

        var user = await _userStore.FindBySubjectIdAsync(userId);
        if (user == null) return NotFound();

        if (await _userStore.ValidateCredentialsAsync(user.Username, currentPassword))
        {
            await _userStore.SetPasswordAsync(userId, newPassword);
            return RedirectToPage("./Index", new { Message = "PasswordChanged" });
        }

        ModelState.AddModelError(string.Empty, "Invalid current password.");
        return Page();
    }
}
