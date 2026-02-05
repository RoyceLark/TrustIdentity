using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Core.Services;

namespace TrustIdentity.UI.Pages.Sessions;

[Authorize]
public class IndexModel : PageModel
{
    private readonly SessionManagementService _sessionService;

    public IndexModel(SessionManagementService sessionService)
    {
        _sessionService = sessionService;
    }

    public IEnumerable<UserSession> Sessions { get; set; } = Enumerable.Empty<UserSession>();

    public async Task OnGetAsync()
    {
        var subjectId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (subjectId != null)
        {
            Sessions = await _sessionService.GetUserSessionsAsync(subjectId);
        }
    }

    public async Task<IActionResult> OnPostRevokeAsync(string sessionId)
    {
        var subjectId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var session = await _sessionService.GetSessionAsync(sessionId);
        
        if (session != null && session.SubjectId == subjectId)
        {
            await _sessionService.RemoveSessionAsync(sessionId);
        }

        return RedirectToPage();
    }
}
