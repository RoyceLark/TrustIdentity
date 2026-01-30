using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace TrustIdentity.UI.Pages.Account;

public class LoggedOutModel : PageModel
{
    private readonly IClientStore _clientStore;

    public LoggedOutModel(IClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    public string? PostLogoutRedirectUri { get; set; }
    public string? ClientName { get; set; }

    public async Task OnGetAsync(string? logoutId)
    {
        // In a real implementation, you would retrieve the logout context using the logoutId.
        // For this implementation, we will simulate validation by checking if any client 
        // has a matching post-logout redirect URI.
        
        // SECURITY: We retrieve the URI from the context (simulated here via a query parameter or previously stored state)
        var uri = Request.Query["post_logout_redirect_uri"].ToString();
        
        if (!string.IsNullOrEmpty(uri))
        {
            var allClients = await _clientStore.GetAllClientsAsync();
            var client = allClients.FirstOrDefault(c => c.PostLogoutRedirectUris.Contains(uri, StringComparer.OrdinalIgnoreCase));
            
            if (client != null)
            {
                PostLogoutRedirectUri = uri;
                ClientName = client.ClientName ?? client.ClientId;
            }
        }
    }
}
