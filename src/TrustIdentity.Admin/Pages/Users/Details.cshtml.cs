using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Users;

public class DetailsModel : PageModel
{
    private readonly IUserStore _userStore;

    public DetailsModel(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public new User User { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _userStore.FindBySubjectIdAsync(id);
        if (user == null) return NotFound();

        User = user;
        return Page();
    }
}
