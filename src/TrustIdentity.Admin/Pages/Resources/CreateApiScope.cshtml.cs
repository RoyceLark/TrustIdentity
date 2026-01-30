using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Resources;

public class CreateApiScopeModel : PageModel
{
    private readonly IApiScopeStore _scopeStore;

    public CreateApiScopeModel(IApiScopeStore scopeStore)
    {
        _scopeStore = scopeStore;
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
        public bool Emphasize { get; set; }
        public bool ShowInDiscoveryDocument { get; set; } = true;

        [Display(Name = "User Claims")]
        public string? UserClaims { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _scopeStore.FindApiScopeAsync(Input.Name);
        if (existing != null)
        {
            ModelState.AddModelError("Input.Name", "Scope name already exists.");
            return Page();
        }

        var scope = new ApiScope
        {
            Name = Input.Name,
            DisplayName = Input.DisplayName,
            Description = Input.Description,
            Required = Input.Required,
            Emphasize = Input.Emphasize,
            ShowInDiscoveryDocument = Input.ShowInDiscoveryDocument,
            UserClaims = string.IsNullOrEmpty(Input.UserClaims) 
                ? new List<string>() 
                : Input.UserClaims.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList()
        };

        await _scopeStore.AddScopeAsync(scope);

        return RedirectToPage("./Index");
    }
}
