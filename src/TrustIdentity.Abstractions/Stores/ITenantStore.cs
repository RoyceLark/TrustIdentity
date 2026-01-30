namespace TrustIdentity.Abstractions.Stores;

using System.Collections.Generic;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

/// <summary>
/// Store for managing tenants
/// </summary>
public interface ITenantStore
{
    /// <summary>
    /// Gets a tenant by ID
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>The tenant, or null if not found</returns>
    Task<Tenant?> GetByIdAsync(string tenantId);

    /// <summary>
    /// Gets a tenant by identifier (unique key)
    /// </summary>
    /// <param name="identifier">The tenant identifier</param>
    /// <returns>The tenant, or null if not found</returns>
    Task<Tenant?> GetByIdentifierAsync(string identifier);

    /// <summary>
    /// Gets a tenant by host/domain
    /// </summary>
    /// <param name="host">The host/domain</param>
    /// <returns>The tenant, or null if not found</returns>
    Task<Tenant?> GetByHostAsync(string host);

    /// <summary>
    /// Gets all tenants
    /// </summary>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to take</param>
    /// <returns>List of tenants</returns>
    Task<IEnumerable<Tenant>> GetAllAsync(int skip = 0, int take = 100);

    /// <summary>
    /// Creates a new tenant
    /// </summary>
    /// <param name="tenant">The tenant to create</param>
    /// <returns>The created tenant</returns>
    Task<Tenant> CreateAsync(Tenant tenant);

    /// <summary>
    /// Updates an existing tenant
    /// </summary>
    /// <param name="tenant">The tenant to update</param>
    /// <returns>The updated tenant</returns>
    Task<Tenant> UpdateAsync(Tenant tenant);

    /// <summary>
    /// Deletes a tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>True if deleted, false otherwise</returns>
    Task<bool> DeleteAsync(string tenantId);

    /// <summary>
    /// Gets the total count of tenants
    /// </summary>
    /// <returns>Total tenant count</returns>
    Task<int> GetCountAsync();
}
