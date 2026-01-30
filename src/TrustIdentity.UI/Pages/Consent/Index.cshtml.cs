using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TrustIdentity.UI.Pages.Consent;

public class IndexModel : PageModel
{
    [BindProperty]
    public ConsentInputModel Input { get; set; } = new();

    public string ClientName { get; set; } = string.Empty;
    public string ClientLogoUrl { get; set; } = string.Empty;
    public List<ScopeViewModel> Scopes { get; set; } = new();
    public string ReturnUrl { get; set; } = string.Empty;

    public class ConsentInputModel
    {
        public List<string> ScopesConsented { get; set; } = new();
        public bool RememberConsent { get; set; }
    }

    public class ScopeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool Checked { get; set; }
    }

    private readonly TrustIdentity.Core.Services.ConsentService _consentService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(TrustIdentity.Core.Services.ConsentService consentService, ILogger<IndexModel> logger)
    {
        _consentService = consentService;
        _logger = logger;
    }

    public async Task OnGetAsync(string returnUrl)
    {
        ReturnUrl = returnUrl;
        
        // Load data from returnUrl (contains OIDC request)
        var request = await _consentService.GetConsentRequestAsync(returnUrl);
        if (request != null)
        {
            ClientName = request.ClientName;
            ClientLogoUrl = request.ClientLogoUrl ?? string.Empty;
            Scopes = request.Scopes.Select(s => new ScopeViewModel
            {
                Name = s.Name,
                DisplayName = s.DisplayName,
                Description = s.Description,
                Required = s.Required,
                Checked = s.Required || s.Default
            }).ToList();
        }
    }

    public async Task<IActionResult> OnPostAsync(string button)
    {
        if (button == "deny")
        {
            await _consentService.DenyConsentAsync(ReturnUrl);
            
            if (IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }
            return RedirectToPage("/Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Grant consent
        var subjectId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (subjectId != null)
        {
            await _consentService.GrantConsentAsync(ReturnUrl, subjectId, Input.ScopesConsented, Input.RememberConsent);
        }
        
        if (IsLocalUrl(ReturnUrl))
        {
            return Redirect(ReturnUrl);
        }
        return RedirectToPage("/Index");
    }

    private bool IsLocalUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        
        // In a real OIDC server, we should also validate against valid Client RedirectUris
        return Url.IsLocalUrl(url) || url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\");
    }
}
