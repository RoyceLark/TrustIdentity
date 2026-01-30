using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Resources;

public class EditApiResourceModel : PageModel
{
    private readonly IApiResourceStore _resourceStore;

    public EditApiResourceModel(IApiResourceStore resourceStore)
    {
        _resourceStore = resourceStore;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool Enabled { get; set; }

        [Display(Name = "Allowed Scopes")]
        public string? Scopes { get; set; }

        [Display(Name = "User Claims")]
        public string? UserClaims { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var res = await _resourceStore.FindApiResourceAsync(id);
        if (res == null) return NotFound();

        Input = new InputModel
        {
            Name = res.Name,
            DisplayName = res.DisplayName ?? string.Empty,
            Description = res.Description,
            Enabled = res.Enabled,
            Scopes = res.Scopes != null ? string.Join(", ", res.Scopes) : null,
            UserClaims = res.UserClaims != null ? string.Join(", ", res.UserClaims) : null
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var res = await _resourceStore.FindApiResourceAsync(Input.Name);
        if (res == null) return NotFound();

        res.DisplayName = Input.DisplayName;
        res.Description = Input.Description;
        res.Enabled = Input.Enabled;
        res.Scopes = string.IsNullOrEmpty(Input.Scopes)
            ? new List<string>()
            : Input.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        res.UserClaims = string.IsNullOrEmpty(Input.UserClaims)
            ? new List<string>()
            : Input.UserClaims.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

        await _resourceStore.UpdateResourceAsync(res);

        return RedirectToPage("./Index");
    }
}
