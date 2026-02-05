using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TrustIdentity.UI.Pages.Diagnostics;

[Authorize] // Should ideally be restricted to admins
public class IndexModel : PageModel
{
    public IEnumerable<Claim> Claims { get; set; } = new List<Claim>();
    public string? IdentityToken { get; set; }

    public async Task OnGetAsync()
    {
        Claims = User.Claims;
        
        // In a real app, we might retrieve the id_token from the authentication properties
        IdentityToken = await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.GetTokenAsync(HttpContext, "id_token");
    }
}
