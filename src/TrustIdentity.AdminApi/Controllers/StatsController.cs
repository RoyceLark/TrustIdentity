using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.AdminApi.Controllers;

/// <summary>
/// Controller for dashboard statistics
/// </summary>
[Authorize(Policy = "AdminApiAccess")]
[ApiController]
[Route("api/v1/admin/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IAuditStore _auditStore;
    private readonly IUserStore _userStore;
    private readonly IClientStore _clientStore;

    /// <summary>
    /// Initializes a new instance of the StatsController
    /// </summary>
    public StatsController(IAuditStore auditStore, IUserStore userStore, IClientStore clientStore)
    {
        _auditStore = auditStore;
        _userStore = userStore;
        _clientStore = clientStore;
    }

    /// <summary>
    /// Retrieves statistical data for the admin dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboardStats()
    {
        var auditStats = await _auditStore.GetStatsAsync();
        var (_, totalUsers) = await _userStore.GetAllUsersAsync(take: 1);
        var clients = await _clientStore.GetAllClientsAsync();

        return Ok(new
        {
            totalUsers,
            totalClients = clients.Count(),
            tokensIssuedToday = auditStats.TokensIssuedToday,
            failedLoginsToday = auditStats.FailedLoginsToday,
            loginTrend = auditStats.LoginTrend
        });
    }
}
