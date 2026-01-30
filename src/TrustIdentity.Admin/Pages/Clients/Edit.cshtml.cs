using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Clients;

public class EditModel : PageModel
{
    private readonly IClientStore _clientStore;

    public EditModel(IClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public string ClientId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Enabled")]
        public bool Enabled { get; set; }

        [Required]
        [Display(Name = "Grant Types")]
        public List<string> AllowedGrantTypes { get; set; } = new();

        [Display(Name = "Allow Offline Access")]
        public bool AllowOfflineAccess { get; set; }

        [Display(Name = "Require PKCE")]
        public bool RequirePkce { get; set; }

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

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var client = await _clientStore.FindClientByIdAsync(id);
        if (client == null) return NotFound();

        Input = new InputModel
        {
            ClientId = client.ClientId,
            ClientName = client.ClientName ?? string.Empty,
            Description = client.Description,
            Enabled = client.Enabled,
            AllowedGrantTypes = client.AllowedGrantTypes,
            AllowOfflineAccess = client.AllowOfflineAccess,
            RequirePkce = client.RequirePkce,
            LogoUri = client.LogoUri,
            PrimaryColor = client.PrimaryColor,
            SecondaryColor = client.SecondaryColor,
            CustomCss = client.CustomCss,
            ProtocolType = client.ProtocolType
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var client = await _clientStore.FindClientByIdAsync(Input.ClientId);
        if (client == null) return NotFound();

        client.ClientName = Input.ClientName;
        client.Description = Input.Description;
        client.Enabled = Input.Enabled;
        client.AllowedGrantTypes = Input.AllowedGrantTypes;
        client.AllowOfflineAccess = Input.AllowOfflineAccess;
        client.RequirePkce = Input.RequirePkce;
        client.LogoUri = Input.LogoUri;
        client.PrimaryColor = Input.PrimaryColor;
        client.SecondaryColor = Input.SecondaryColor;
        client.CustomCss = Input.CustomCss;
        client.ProtocolType = Input.ProtocolType;
        client.Updated = DateTime.UtcNow;

        await _clientStore.UpdateClientAsync(client);

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostRegenerateSecretAsync(string id)
    {
        var client = await _clientStore.FindClientByIdAsync(id);
        if (client == null) return NotFound();

        var newSecret = Guid.NewGuid().ToString("N");
        client.ClientSecrets = new List<Secret> { new ClientSecret { Value = newSecret } };
        await _clientStore.UpdateClientAsync(client);

        TempData["NewSecret"] = newSecret;
        return RedirectToPage("./Edit", new { id });
    }
}
