using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Storage.EntityFramework;

namespace TrustIdentity.Storage.Stores;

/// <summary>
/// Entity Framework implementation of ITenantStore
/// </summary>
public class EntityFrameworkTenantStore : ITenantStore
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<EntityFrameworkTenantStore> _logger;

    /// <summary>
    /// Initializes a new instance of the EntityFrameworkTenantStore
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    public EntityFrameworkTenantStore(
        ConfigurationDbContext context,
        ILogger<EntityFrameworkTenantStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Tenant?> GetByIdAsync(string tenantId)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId);
    }

    /// <inheritdoc/>
    public async Task<Tenant?> GetByIdentifierAsync(string identifier)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Identifier == identifier && t.IsActive);
    }

    /// <inheritdoc/>
    public async Task<Tenant?> GetByHostAsync(string host)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Host == host && t.IsActive);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Tenant>> GetAllAsync(int skip = 0, int take = 100)
    {
        return await _context.Tenants
            .OrderBy(t => t.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        _logger.LogInformation("Creating tenant {TenantId} - {TenantName}", tenant.Id, tenant.Name);
        
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        
        return tenant;
    }

    /// <inheritdoc/>
    public async Task<Tenant> UpdateAsync(Tenant tenant)
    {
        _logger.LogInformation("Updating tenant {TenantId}", tenant.Id);
        
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
        
        return tenant;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string tenantId)
    {
        _logger.LogWarning("Deleting tenant {TenantId}", tenantId);
        
        var tenant = await GetByIdAsync(tenantId);
        if (tenant == null)
        {
            return false;
        }

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync()
    {
        return await _context.Tenants.CountAsync();
    }
}
