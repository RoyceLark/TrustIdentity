using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Linq;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Services;
using System.Security.Claims;
using System;

namespace TrustIdentity.UI.Pages.Manage;

[Authorize]
public class TwoFactorModel : PageModel
{
    private readonly IUserStore _userStore;
    private readonly TotpService _totpService;

    public TwoFactorModel(IUserStore userStore, TotpService totpService)
    {
        _userStore = userStore;
        _totpService = totpService;
    }

    public bool IsTwoFactorEnabled { get; set; }
    public string? SharedKey { get; set; }

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            var user = await _userStore.FindBySubjectIdAsync(userId);
            if (user != null)
            {
                IsTwoFactorEnabled = user.Claims.Any(c => c.Type == "amr" && c.Value == "mfa");
                
                // For enrollment, we'd generate a secret if not existing
                SharedKey = _totpService.GenerateSecret();
            }
        }
    }

    public async Task<IActionResult> OnPostEnableAsync(string code, string secret)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        if (_totpService.ValidateCode(secret, code))
        {
            var user = await _userStore.FindBySubjectIdAsync(userId);
            if (user != null)
            {
                // Save the secret and mark MFA as enabled
                user.Claims.Add(new Claim("mfa_secret", secret));
                user.Claims.Add(new Claim("amr", "mfa"));
                await _userStore.UpdateUserAsync(user);
                return RedirectToPage();
            }
        }

        ModelState.AddModelError(string.Empty, "Invalid verification code.");
        SharedKey = secret; // Keep secret for retry
        return Page();
    }

    public async Task<IActionResult> OnPostDisableAsync()
    {
        // Placeholder for disabling 2FA
        return RedirectToPage();
    }
}
