using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.UI.Pages.Manage;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IAuditStore _auditStore;

    public IndexModel(IUserService userService, IAuditStore auditStore)
    {
        _userService = userService;
        _auditStore = auditStore;
    }

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int ExternalLoginsCount { get; set; }
    public List<AuditEvent> RecentActivity { get; set; } = new();

    public async Task OnGetAsync()
    {
        var subjectId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (subjectId != null)
        {
            var user = await _userService.FindBySubjectIdAsync(subjectId);
            if (user != null)
            {
                Username = user.Username;
                Email = user.Email ?? string.Empty;
                EmailConfirmed = true; // Logic could be added to User model
                TwoFactorEnabled = false; 
                
                // Load personal audit logs
                var allEvents = await _auditStore.GetRecentEventsAsync(50);
                RecentActivity = allEvents.Where(e => e.SubjectId == subjectId).Take(10).ToList();
            }
        }
    }
}
