using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.Admin.Pages.Clients;

public class IndexModel : PageModel
{
    private readonly IClientStore _clientStore;

    public IndexModel(IClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    public List<ClientViewModel> Clients { get; set; } = new();

    public class ClientViewModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public int AllowedScopesCount { get; set; }
        public DateTime Created { get; set; }
    }

    public async Task OnGetAsync()
    {
        var clients = await _clientStore.GetAllClientsAsync();
        
        Clients = clients.Select(c => new ClientViewModel
        {
            ClientId = c.ClientId,
            ClientName = c.ClientName ?? c.ClientId,
            Enabled = c.Enabled,
            AllowedScopesCount = c.AllowedScopes.Count,
            Created = c.Created
        }).ToList();
    }

    [BindProperty]
    public List<string> SelectedIds { get; set; } = new();

    public async Task<IActionResult> OnPostBulkActionAsync(string action)
    {
        if (SelectedIds == null || !SelectedIds.Any()) return RedirectToPage();

        foreach (var id in SelectedIds)
        {
            switch (action.ToLower())
            {
                case "delete":
                    await _clientStore.DeleteClientAsync(id);
                    break;
                case "enable":
                    var clientToEnable = await _clientStore.FindClientByIdAsync(id);
                    if (clientToEnable != null)
                    {
                        clientToEnable.Enabled = true;
                        await _clientStore.UpdateClientAsync(clientToEnable);
                    }
                    break;
                case "disable":
                    var clientToDisable = await _clientStore.FindClientByIdAsync(id);
                    if (clientToDisable != null)
                    {
                        clientToDisable.Enabled = false;
                        await _clientStore.UpdateClientAsync(clientToDisable);
                    }
                    break;
            }
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        await _clientStore.DeleteClientAsync(id);
        return RedirectToPage();
    }
}
