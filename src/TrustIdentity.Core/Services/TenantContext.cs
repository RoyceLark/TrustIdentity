namespace TrustIdentity.Core.Services;

using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;

/// <summary>
/// Default implementation of ITenantContext
/// </summary>
public class TenantContext : ITenantContext
{
    private Tenant? _currentTenant;

    /// <inheritdoc/>
    public Tenant? CurrentTenant => _currentTenant;

    /// <inheritdoc/>
    public string? TenantId => _currentTenant?.Id;

    /// <inheritdoc/>
    public bool HasTenant => _currentTenant != null;

    /// <inheritdoc/>
    public void SetTenant(Tenant? tenant)
    {
        _currentTenant = tenant;
    }
}
