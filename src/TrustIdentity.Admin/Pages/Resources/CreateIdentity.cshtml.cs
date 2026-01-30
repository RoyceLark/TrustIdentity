using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Resources;

public class CreateIdentityModel : PageModel
{
    private readonly IResourceStore _resourceStore;

    public CreateIdentityModel(IResourceStore resourceStore)
    {
        _resourceStore = resourceStore;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Display Name")]
        public string? DisplayName { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool Required { get; set; }
        
        [Display(Name = "Emphasize")]
        public bool Emphasize { get; set; }

        [Display(Name = "Show In Discovery Document")]
        public bool ShowInDiscoveryDocument { get; set; } = true;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _resourceStore.FindIdentityResourceAsync(Input.Name);
        if (existing != null)
        {
            ModelState.AddModelError("Input.Name", "Resource name already exists.");
            return Page();
        }

        var resource = new IdentityResource
        {
            Name = Input.Name,
            DisplayName = Input.DisplayName,
            Description = Input.Description,
            Required = Input.Required,
            Emphasize = Input.Emphasize,
            ShowInDiscoveryDocument = Input.ShowInDiscoveryDocument
        };

        await _resourceStore.AddResourceAsync(resource);

        return RedirectToPage("./Index");
    }
}
