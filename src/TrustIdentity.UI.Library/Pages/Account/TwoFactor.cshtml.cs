using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Core.Services;
using System;

namespace TrustIdentity.UI.Pages.Account;

public class TwoFactorModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(7, MinimumLength = 6)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Text)]
        public string TwoFactorCode { get; set; } = string.Empty;

        public bool RememberMachine { get; set; }
    }

    private readonly IUserService _userService;
    private readonly TotpService _totpService;
    private readonly ILogger<TwoFactorModel> _logger;

    public TwoFactorModel(IUserService userService, TotpService totpService, ILogger<TwoFactorModel> logger)
    {
        _userService = userService;
        _totpService = totpService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string SubjectId { get; set; } = string.Empty;

    public void OnGet(bool rememberMe, string? returnUrl = null, string? subjectId = null)
    {
        if (returnUrl != null && !Url.IsLocalUrl(returnUrl))
        {
            _logger.LogWarning("Potential open redirect attempt in 2FA with returnUrl: {ReturnUrl}", returnUrl);
            returnUrl = null;
        }

        RememberMe = rememberMe;
        ReturnUrl = returnUrl;
        SubjectId = subjectId ?? string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userService.FindBySubjectIdAsync(SubjectId);
        if (user == null) return NotFound();

        var mfaSecret = user.Claims.FirstOrDefault(c => c.Type == "mfa_secret")?.Value;
        
        // If high risk but no secret set, we might allow a one-time code or email, 
        // but for now we assume secret exists if we're here
        if (string.IsNullOrEmpty(mfaSecret))
        {
            _logger.LogWarning("MFA required but no secret found for user {SubjectId}", SubjectId);
            return RedirectToPage("/Index");
        }

        if (_totpService.ValidateCode(mfaSecret, Input.TwoFactorCode))
        {
            var claims = (await _userService.GetClaimsAsync(user)).ToList();
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TrustIdentity");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("TrustIdentity", principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            });

            _logger.LogInformation("User {SubjectId} passed MFA.", SubjectId);
            return LocalRedirect(ReturnUrl ?? "/");
        }

        ModelState.AddModelError(string.Empty, "Invalid authentication code.");
        return Page();
    }
}
