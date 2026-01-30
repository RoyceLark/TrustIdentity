using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.Admin.Pages;

/// <summary>
/// Page model for viewing audit logs
/// </summary>
public class AuditLogsModel : PageModel
{
    private readonly IAuditStore _auditStore;

    /// <summary>
    /// Initializes a new instance of the AuditLogsModel
    /// </summary>
    public AuditLogsModel(IAuditStore auditStore)
    {
        _auditStore = auditStore;
    }

    /// <summary>The collection of recent audit events</summary>
    public List<AuditEvent> Events { get; set; } = new();

    /// <summary>Handles GET requests</summary>
    public async Task OnGetAsync()
    {
        Events = (await _auditStore.GetRecentEventsAsync(100)).ToList();
    }
}
