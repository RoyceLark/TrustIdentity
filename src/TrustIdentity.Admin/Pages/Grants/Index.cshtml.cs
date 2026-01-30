using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Grants;

public class IndexModel : PageModel
{
    private readonly IPersistedGrantStore _grantStore;

    public IndexModel(IPersistedGrantStore grantStore)
    {
        _grantStore = grantStore;
    }

    [BindProperty(SupportsGet = true)]
    public string? SubjectId { get; set; }

    public List<PersistedGrant> Grants { get; set; } = new();

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrEmpty(SubjectId))
        {
            Grants = (await _grantStore.GetAllAsync(SubjectId)).ToList();
        }
    }

    public async Task<IActionResult> OnPostRevokeAsync(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            await _grantStore.RemoveAsync(key);
        }
        return RedirectToPage(new { subjectId = SubjectId });
    }
}
