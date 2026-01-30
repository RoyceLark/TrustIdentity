namespace TrustIdentity.Abstractions.Services;

using TrustIdentity.Abstractions.Models;

/// <summary>
/// Provides access to the current tenant context
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant
    /// </summary>
    Tenant? CurrentTenant { get; }

    /// <summary>
    /// Gets the current tenant ID
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Sets the current tenant
    /// </summary>
    /// <param name="tenant">The tenant to set as current</param>
    void SetTenant(Tenant? tenant);

    /// <summary>
    /// Checks if a tenant is currently set
    /// </summary>
    bool HasTenant { get; }
}
