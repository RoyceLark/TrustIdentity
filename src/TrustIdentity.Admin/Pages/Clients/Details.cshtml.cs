using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Clients;

public class DetailsModel : PageModel
{
    private readonly IClientStore _clientStore;

    public DetailsModel(IClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    public Client Client { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var client = await _clientStore.FindClientByIdAsync(id);
        if (client == null) return NotFound();

        Client = client;
        return Page();
    }
}
