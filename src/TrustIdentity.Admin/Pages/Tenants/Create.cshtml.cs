using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;
using System.ComponentModel.DataAnnotations;

namespace TrustIdentity.Admin.Pages.Tenants;

/// <summary>
/// Page model for creating a new tenant
/// </summary>
public class CreateModel : PageModel
{
    private readonly ITenantStore _tenantStore;

    public CreateModel(ITenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    [BindProperty]
    public TenantInputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var tenant = new Tenant
        {
            Name = Input.Name,
            Identifier = Input.Identifier,
            Host = Input.Host,
            IsActive = Input.IsActive,
            IssuerUri = Input.IssuerUri,
            MaxUsers = Input.MaxUsers,
            MaxClients = Input.MaxClients,
            SubscriptionTier = Input.SubscriptionTier,
            SubscriptionExpiresAt = Input.SubscriptionExpiresAt
        };

        await _tenantStore.CreateAsync(tenant);

        TempData["SuccessMessage"] = $"Tenant '{tenant.Name}' created successfully!";
        return RedirectToPage("Index");
    }

    public class TenantInputModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Tenant Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Identifier must contain only lowercase letters, numbers, and hyphens")]
        [Display(Name = "Identifier")]
        public string Identifier { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Host/Domain")]
        public string? Host { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        [Url]
        [Display(Name = "Issuer URI")]
        public string? IssuerUri { get; set; }

        [Display(Name = "Max Users")]
        [Range(0, int.MaxValue)]
        public int? MaxUsers { get; set; }

        [Display(Name = "Max Clients")]
        [Range(0, int.MaxValue)]
        public int? MaxClients { get; set; }

        [StringLength(50)]
        [Display(Name = "Subscription Tier")]
        public string? SubscriptionTier { get; set; }

        [Display(Name = "Subscription Expires")]
        [DataType(DataType.Date)]
        public DateTime? SubscriptionExpiresAt { get; set; }
    }
}
