using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrustIdentity.UI.Pages.Grants;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IPersistedGrantStore _grantStore;
    private readonly IClientStore _clientStore;
    private readonly IResourceStore _resourceStore;

    public IndexModel(IPersistedGrantStore grantStore, IClientStore clientStore, IResourceStore resourceStore)
    {
        _grantStore = grantStore;
        _clientStore = clientStore;
        _resourceStore = resourceStore;
    }

    public List<GrantViewModel> Grants { get; set; } = new();

    public async Task OnGetAsync()
    {
        var subjectId = User.FindFirst("sub")?.Value;
        if (subjectId == null) return;

        var grants = await _grantStore.GetAllAsync(subjectId);
        
        foreach (var grant in grants.GroupBy(x => x.ClientId))
        {
            var client = await _clientStore.FindClientByIdAsync(grant.Key);
            if (client == null) continue;

            var model = new GrantViewModel
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName ?? client.ClientId,
                ClientLogoUrl = client.LogoUri,
                Created = grant.Min(x => x.CreationTime),
                Expires = grant.Max(x => x.Expiration),
                IdentityGrantNames = new List<string>(), // Simplified for now
                ApiGrantNames = new List<string>()
            };

            Grants.Add(model);
        }
    }

    public async Task<IActionResult> OnPostRevokeAsync(string clientId)
    {
        var subjectId = User.FindFirst("sub")?.Value;
        if (subjectId == null) return Page();

        await _grantStore.RemoveAllAsync(subjectId, clientId);
        return RedirectToPage();
    }

    public class GrantViewModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string? ClientLogoUrl { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime? Expires { get; set; }
        public List<string> IdentityGrantNames { get; set; } = new();
        public List<string> ApiGrantNames { get; set; } = new();
    }
}
