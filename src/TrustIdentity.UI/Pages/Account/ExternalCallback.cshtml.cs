using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.UI.Pages.Account;

public class ExternalCallbackModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ILogger<ExternalCallbackModel> _logger;

    public ExternalCallbackModel(IUserService userService, ILogger<ExternalCallbackModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl, error = $"Error from external provider: {remoteError}" });
        }

        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded)
        {
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var externalUser = result.Principal;
        var claims = externalUser.Claims.ToList();

        // 1. Identify the user (by email or unique provider ID)
        var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (email == null)
        {
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl, error = "Email claim not provided by external provider." });
        }

        var user = await _userService.FindByUsernameAsync(email);
        if (user == null)
        {
            // Auto-provision user or redirect to supplemental registration
            _logger.LogInformation("External user {Email} not found. Provisioning new account.", email);
            // ... Logic to create user ...
        }

        // Sign in locally
        var localClaims = (await _userService.GetClaimsAsync(user!)).ToList();
        var identity = new ClaimsIdentity(localClaims, "TrustIdentity");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("TrustIdentity", principal);
        await HttpContext.SignOutAsync("External");

        return LocalRedirect(returnUrl ?? "/");
    }
}
