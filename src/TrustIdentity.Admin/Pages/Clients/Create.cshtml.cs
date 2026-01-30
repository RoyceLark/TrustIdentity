using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Clients;

public class CreateModel : PageModel
{
    private readonly IClientStore _clientStore;

    public CreateModel(IClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Client ID")]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Enabled")]
        public bool Enabled { get; set; } = true;

        [Required]
        [Display(Name = "Grant Types")]
        public List<string> AllowedGrantTypes { get; set; } = new() { "authorization_code" };

        [Display(Name = "Allow Offline Access")]
        public bool AllowOfflineAccess { get; set; } = true;

        [Display(Name = "Require PKCE")]
        public bool RequirePkce { get; set; } = true;

        [Display(Name = "Logo URI")]
        public string? LogoUri { get; set; }

        [Display(Name = "Primary Color")]
        public string? PrimaryColor { get; set; }

        [Display(Name = "Secondary Color")]
        public string? SecondaryColor { get; set; }

        [Display(Name = "Custom CSS")]
        public string? CustomCss { get; set; }

        [Display(Name = "Protocol Type")]
        public string ProtocolType { get; set; } = "oidc";
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _clientStore.FindClientByIdAsync(Input.ClientId);
        if (existing != null)
        {
            ModelState.AddModelError("Input.ClientId", "Client ID already exists.");
            return Page();
        }

        var client = new Client
        {
            ClientId = Input.ClientId,
            ClientName = Input.ClientName,
            Description = Input.Description,
            Enabled = Input.Enabled,
            AllowedGrantTypes = Input.AllowedGrantTypes,
            AllowOfflineAccess = Input.AllowOfflineAccess,
            RequirePkce = Input.RequirePkce,
            LogoUri = Input.LogoUri,
            PrimaryColor = Input.PrimaryColor,
            SecondaryColor = Input.SecondaryColor,
            CustomCss = Input.CustomCss,
            ProtocolType = Input.ProtocolType,
            Created = DateTime.UtcNow
        };

        await _clientStore.AddClientAsync(client);

        return RedirectToPage("./Index");
    }
}
