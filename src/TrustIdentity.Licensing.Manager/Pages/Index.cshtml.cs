using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Licensing;

namespace TrustIdentity.Licensing.Manager.Pages;

public class IndexModel : PageModel
{
    private readonly ILicenseStore _store;

    public IndexModel(ILicenseStore store)
    {
        _store = store;
    }

    public IEnumerable<License> Licenses { get; set; } = new List<License>();

    public async Task OnGetAsync()
    {
        Licenses = await _store.GetAllLicensesAsync();
    }
}
