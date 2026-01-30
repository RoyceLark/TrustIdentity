using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Resources;

public class CreateApiResourceModel : PageModel
{
    private readonly IApiResourceStore _resourceStore;

    public CreateApiResourceModel(IApiResourceStore resourceStore)
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

        [Required]
        [StringLength(200)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool Enabled { get; set; } = true;

        [Display(Name = "Allowed Scopes")]
        public string? Scopes { get; set; }

        [Display(Name = "User Claims")]
        public string? UserClaims { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _resourceStore.FindApiResourceAsync(Input.Name);
        if (existing != null)
        {
            ModelState.AddModelError("Input.Name", "Resource name already exists.");
            return Page();
        }

        var resource = new ApiResource
        {
            Name = Input.Name,
            DisplayName = Input.DisplayName,
            Description = Input.Description,
            Enabled = Input.Enabled,
            Scopes = string.IsNullOrEmpty(Input.Scopes) 
                ? new List<string>() 
                : Input.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
            UserClaims = string.IsNullOrEmpty(Input.UserClaims) 
                ? new List<string>() 
                : Input.UserClaims.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList()
        };

        await _resourceStore.AddResourceAsync(resource);

        return RedirectToPage("./Index");
    }
}
