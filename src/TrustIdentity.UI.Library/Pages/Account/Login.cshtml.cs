using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Stores;
using Microsoft.Extensions.Logging;
using TrustIdentity.Core.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace TrustIdentity.UI.Pages.Account;

public class LoginModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShowExternalProviders { get; set; } = true;

    public class InputModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    private readonly IUserService _userService;
    private readonly IFraudDetectionService _fraudService;
    private readonly IClientStore _clientStore;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly ILogger<LoginModel> _logger;

    public IEnumerable<AuthenticationScheme> ExternalProviders { get; set; } = new List<AuthenticationScheme>();

    public LoginModel(
        IUserService userService, 
        IFraudDetectionService fraudService, 
        IClientStore clientStore, 
        IAuthenticationSchemeProvider schemeProvider,
        ILogger<LoginModel> logger)
    {
        _userService = userService;
        _fraudService = fraudService;
        _clientStore = clientStore;
        _schemeProvider = schemeProvider;
        _logger = logger;
    }

    public async Task OnGetAsync(string? returnUrl = null, string? error = null)
    {
        if (returnUrl != null && !Url.IsLocalUrl(returnUrl))
        {
            _logger.LogWarning("Potential open redirect attempt with returnUrl: {ReturnUrl}", returnUrl);
            returnUrl = null;
        }

        ReturnUrl = returnUrl;
        ErrorMessage = error;

        var schemes = await _schemeProvider.GetAllSchemesAsync();
        ExternalProviders = schemes.Where(s => !string.IsNullOrEmpty(s.DisplayName));

        if (!string.IsNullOrEmpty(returnUrl))
        {
            var clientId = ExtractClientId(returnUrl);
            if (!string.IsNullOrEmpty(clientId))
            {
                var client = await _clientStore.FindClientByIdAsync(clientId);
                if (client != null)
                {
                    ViewData["PrimaryColor"] = client.PrimaryColor;
                    ViewData["SecondaryColor"] = client.SecondaryColor;
                    ViewData["CustomCss"] = client.CustomCss;
                    ViewData["LogoUri"] = client.LogoUri;
                }
            }
        }
    }

    private string? ExtractClientId(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url)) return null;
            
            // Only process relative URLs to avoid Uri constructor issues with absolute URLs
            var queryIndex = url.IndexOf('?');
            if (queryIndex == -1) return null;

            var queryString = url.Substring(queryIndex);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString);
            
            if (query.TryGetValue("client_id", out var clientId))
            {
                return clientId.ToString();
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userService.FindByUsernameAsync(Input.Username);
        if (user == null)
        {
            _logger.LogWarning("Invalid login attempt for non-existent user {Username}", Input.Username);
            ErrorMessage = "Invalid username or password";
            return Page();
        }

        // Check if locked
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            _logger.LogSecurityEvent("ACCOUNT_LOCKED", $"Login attempt for locked user {Input.Username}");
            ErrorMessage = $"Account is temporarily locked. Please try again after {user.LockoutEnd.Value.LocalDateTime}.";
            return Page();
        }

        // Validate credentials
        if (await _userService.ValidateCredentialsAsync(Input.Username, Input.Password))
        {
            if (user.IsActive)
            {
                // Reset failed attempts
                await _userService.ResetFailedAttemptsAsync(user.SubjectId);

                // AI Fraud Analysis
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var ua = Request.Headers["User-Agent"].ToString();
                var fraudScore = await _fraudService.AnalyzeLoginAttemptAsync(user.SubjectId, ip, ua);

                if (fraudScore > 0.8)
                {
                    _logger.LogSecurityEvent("FRAUD_DETECTED", $"High fraud score ({fraudScore:P}) for user {Input.Username}");
                    ErrorMessage = "Security check failed. This login attempt was flagged as suspicious. Please try from a known device or contact support.";
                    return Page();
                }

                // Check if 2FA is required
                var mfaSecret = user.Claims.FirstOrDefault(c => c.Type == "mfa_secret")?.Value;
                if (!string.IsNullOrEmpty(mfaSecret) || fraudScore > 0.5)
                {
                    // Redirect to 2FA page
                    // In a real system we'd use a temporary cookie or session to store the user ID
                    return RedirectToPage("/Account/TwoFactor", new { area = "", ReturnUrl, RememberMe = Input.RememberMe, SubjectId = user.SubjectId });
                }

                var claims = (await _userService.GetClaimsAsync(user)).ToList();
                
                var identity = new System.Security.Claims.ClaimsIdentity(claims, "TrustIdentity");
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("TrustIdentity", principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                });

                _logger.LogInformation("User {Username} logged in successfully.", Input.Username);

                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                return RedirectToPage("/Index");
            }
        }

        // Increment failed attempts
        await _userService.IncrementFailedAttemptsAsync(user.SubjectId);
        
        // Check for lockout threshold
        if (user.FailedLoginAttempts + 1 >= 5)
        {
            var lockoutDuration = DateTimeOffset.UtcNow.AddMinutes(30);
            await _userService.LockAccountAsync(user.SubjectId, lockoutDuration);
            _logger.LogSecurityEvent("ACCOUNT_LOCKED", $"Account locked for user {Input.Username} due to multiple failed attempts.");
            ErrorMessage = "Too many failed attempts. Your account has been temporarily locked for 30 minutes.";
        }
        else
        {
            _logger.LogWarning("Invalid login attempt for user {Username}. Attempt {Count}/5", Input.Username, user.FailedLoginAttempts + 1);
            ErrorMessage = "Invalid username or password";
        }
        
        return Page();
    }

    public IActionResult OnPostExternal(string provider)
    {
        // Challenge the external provider and redirect to callback
        var redirectUrl = Url.Page("./ExternalCallback", new { ReturnUrl });
        return Challenge(new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = redirectUrl
        }, provider);
    }
}
