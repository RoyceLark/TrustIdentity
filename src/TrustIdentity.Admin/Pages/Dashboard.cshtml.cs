using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.Admin.Pages;

public class DashboardModel : PageModel
{
    private readonly IUserStore _userStore;
    private readonly IClientStore _clientStore;
    private readonly IAuditStore _auditStore;

    public DashboardModel(IUserStore userStore, IClientStore clientStore, IAuditStore auditStore)
    {
        _userStore = userStore;
        _clientStore = clientStore;
        _auditStore = auditStore;
    }

    public DashboardMetrics Metrics { get; set; } = new();

    public class DashboardMetrics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalClients { get; set; }
        public int TokensIssuedToday { get; set; }
        public int FailedLoginsToday { get; set; }
        public List<ChartData> LoginTrend { get; set; } = new();
        public List<FraudAlert> RecentAlerts { get; set; } = new();
    }

    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class FraudAlert
    {
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public double RiskScore { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        var (_, totalUsers) = await _userStore.GetAllUsersAsync(take: 1);
        var clients = await _clientStore.GetAllClientsAsync();
        var auditStats = await _auditStore.GetStatsAsync();
        var recentAudit = await _auditStore.GetRecentEventsAsync(20);

        Metrics = new DashboardMetrics
        {
            TotalUsers = totalUsers,
            ActiveUsers = totalUsers, // Simple assumption for now
            TotalClients = clients.Count(),
            TokensIssuedToday = auditStats.TokensIssuedToday,
            FailedLoginsToday = auditStats.FailedLoginsToday,
            LoginTrend = auditStats.LoginTrend.Select(t => new ChartData { Label = t.Label, Value = t.Value }).ToList(),
            RecentAlerts = recentAudit
                .Where(e => e.EventType == "UserLoginFailed" || (e.Data?.Contains("Risk") ?? false))
                .Select(e => new FraudAlert
                {
                    Timestamp = e.Timestamp,
                    Username = e.SubjectId ?? "Unknown",
                    IpAddress = e.IpAddress ?? "Unknown",
                    RiskScore = 0.5, // Default placeholder
                    Reason = e.Message
                }).ToList()
        };
    }
}
