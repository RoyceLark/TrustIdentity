using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TrustIdentity.Core.Services;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using System.Security.Claims;

namespace TrustIdentity.UI.Pages.Ciba;

[Authorize]
public class IndexModel : PageModel
{
    private readonly CibaService _cibaService;
    private readonly IClientStore _clientStore;

    public IndexModel(CibaService cibaService, IClientStore clientStore)
    {
        _cibaService = cibaService;
        _clientStore = clientStore;
    }

    public string ClientName { get; set; } = "Third Party App";
    public string BindingMessage { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;

    public async Task OnGetAsync(string id, string binding_message)
    {
        RequestId = id;
        BindingMessage = binding_message;

        var request = await _cibaService.GetRequestAsync(id);
        if (request != null)
        {
            var client = await _clientStore.FindClientByIdAsync(request.ClientId);
            ClientName = client?.ClientName ?? request.ClientId;
        }
    }

    public async Task<IActionResult> OnPostAsync(string id, string button)
    {
        var request = await _cibaService.GetRequestAsync(id);
        if (request == null) return RedirectToPage("/Index");

        if (button == "deny")
        {
            request.IsApproved = false;
        }
        else
        {
            request.IsApproved = true;
        }

        await _cibaService.UpdateRequestAsync(request);
        return RedirectToPage("/Ciba/Success");
    }
}
