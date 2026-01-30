using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Resources;

public class EditApiScopeModel : PageModel
{
    private readonly IApiScopeStore _scopeStore;

    public EditApiScopeModel(IApiScopeStore scopeStore)
    {
        _scopeStore = scopeStore;
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

        [Display(Name = "User Claims")]
        public string? UserClaims { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var scope = await _scopeStore.FindApiScopeAsync(id);
        if (scope == null) return NotFound();

        Input = new InputModel
        {
            Name = scope.Name,
            DisplayName = scope.DisplayName,
            Description = scope.Description,
            Required = scope.Required,
            Emphasize = scope.Emphasize,
            ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
            UserClaims = scope.UserClaims != null ? string.Join(", ", scope.UserClaims) : null
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var scope = await _scopeStore.FindApiScopeAsync(Input.Name);
        if (scope == null) return NotFound();

        scope.DisplayName = Input.DisplayName;
        scope.Description = Input.Description;
        scope.Required = Input.Required;
        scope.Emphasize = Input.Emphasize;
        scope.ShowInDiscoveryDocument = Input.ShowInDiscoveryDocument;
        scope.UserClaims = string.IsNullOrEmpty(Input.UserClaims)
            ? new List<string>()
            : Input.UserClaims.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

        await _scopeStore.UpdateScopeAsync(scope);

        return RedirectToPage("./Index");
    }
}
