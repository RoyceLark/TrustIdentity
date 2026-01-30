using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Resources;

public class EditIdentityModel : PageModel
{
    private readonly IResourceStore _resourceStore;

    public EditIdentityModel(IResourceStore resourceStore)
    {
        _resourceStore = resourceStore;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Display Name")]
        public string? DisplayName { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool Required { get; set; }
        public bool Emphasize { get; set; }
        public bool ShowInDiscoveryDocument { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var res = await _resourceStore.FindIdentityResourceAsync(id);
        if (res == null) return NotFound();

        Input = new InputModel
        {
            Name = res.Name,
            DisplayName = res.DisplayName,
            Description = res.Description,
            Required = res.Required,
            Emphasize = res.Emphasize,
            ShowInDiscoveryDocument = res.ShowInDiscoveryDocument
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var res = await _resourceStore.FindIdentityResourceAsync(Input.Name);
        if (res == null) return NotFound();

        res.DisplayName = Input.DisplayName;
        res.Description = Input.Description;
        res.Required = Input.Required;
        res.Emphasize = Input.Emphasize;
        res.ShowInDiscoveryDocument = Input.ShowInDiscoveryDocument;

        await _resourceStore.UpdateResourceAsync(res);

        return RedirectToPage("./Index");
    }
}
