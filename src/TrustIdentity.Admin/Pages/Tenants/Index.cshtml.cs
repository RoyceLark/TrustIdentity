using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Tenants;

/// <summary>
/// Page model for listing all tenants
/// </summary>
public class IndexModel : PageModel
{
    private readonly ITenantStore _tenantStore;

    public IndexModel(ITenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    public IEnumerable<Tenant> Tenants { get; set; } = new List<Tenant>();
    public int TotalCount { get; set; }
    public int PageSize { get; set; } = 20;
    public int CurrentPage { get; set; } = 1;

    public async Task OnGetAsync(int page = 1)
    {
        CurrentPage = page;
        var skip = (page - 1) * PageSize;
        
        Tenants = await _tenantStore.GetAllAsync(skip, PageSize);
        TotalCount = await _tenantStore.GetCountAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        await _tenantStore.DeleteAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSwitchAsync(string id)
    {
        var tenant = await _tenantStore.GetByIdAsync(id);
        if (tenant != null)
        {
            Response.Cookies.Append("Ti-Tenant-Id", tenant.Identifier, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Assuming HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(1)
            });
        }
        return RedirectToPage();
    }

    public IActionResult OnPostResetSwitch()
    {
        Response.Cookies.Delete("Ti-Tenant-Id");
        return RedirectToPage();
    }
}
