using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.AdminApi.Controllers;

/// <summary>
/// Controller for managing and viewing audit logs
/// </summary>
[Authorize(Policy = "AdminApiAccess")]
[ApiController]
[Route("api/v1/admin/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditStore _auditStore;

    /// <summary>
    /// Initializes a new instance of the AuditController
    /// </summary>
    public AuditController(IAuditStore auditStore)
    {
        _auditStore = auditStore;
    }

    /// <summary>
    /// Gets the most recent audit events
    /// </summary>
    /// <param name="count">Number of events to retrieve</param>
    /// <returns>A collection of audit events</returns>
    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 50)
    {
        var events = await _auditStore.GetRecentEventsAsync(count);
        return Ok(events);
    }
}
