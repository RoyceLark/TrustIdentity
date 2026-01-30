using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Storage.EntityFramework;

namespace TrustIdentity.Storage.Stores;

/// <summary>
/// Entity Framework implementation of IApiScopeStore
/// </summary>
public class EntityFrameworkApiScopeStore : IApiScopeStore
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<EntityFrameworkApiScopeStore> _logger;
    /// <summary>
    /// EntityFrameworkApiScopeStore
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    public EntityFrameworkApiScopeStore(ConfigurationDbContext context, ILogger<EntityFrameworkApiScopeStore> logger)
    {
        _context = context;
        _logger = logger;
    }
    /// <summary>
    /// FindApiScopeAsync
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<ApiScope?> FindApiScopeAsync(string name)
    {
        return await _context.ApiScopes.AsNoTracking().FirstOrDefaultAsync(s => s.Name == name);
    }
    /// <summary>
    /// FindApiScopesByNameAsync
    /// </summary>
    /// <param name="scopeNames"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
    {
        var names = scopeNames.ToHashSet();
        return await _context.ApiScopes.AsNoTracking().Where(s => names.Contains(s.Name)).ToListAsync();
    }
    /// <summary>
    /// GetAllScopesAsync
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ApiScope>> GetAllScopesAsync()
    {
        return await _context.ApiScopes.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Adds a new API scope
    /// </summary>
    /// <param name="scope">The scope to add</param>
    public async Task AddScopeAsync(ApiScope scope)
    {
        _context.ApiScopes.Add(scope);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing API scope
    /// </summary>
    /// <param name="scope">The scope to update</param>
    public async Task UpdateScopeAsync(ApiScope scope)
    {
        _context.ApiScopes.Update(scope);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes an API scope
    /// </summary>
    /// <param name="name">The name of the scope to delete</param>
    public async Task DeleteScopeAsync(string name)
    {
        var scope = await _context.ApiScopes.FindAsync(name);
        if (scope != null)
        {
            _context.ApiScopes.Remove(scope);
            await _context.SaveChangesAsync();
        }
    }
}

/// <summary>
/// Entity Framework implementation of IApiResourceStore
/// </summary>
public class EntityFrameworkApiResourceStore : IApiResourceStore
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<EntityFrameworkApiResourceStore> _logger;
    /// <summary>
    /// EntityFrameworkApiResourceStore
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    public EntityFrameworkApiResourceStore(ConfigurationDbContext context, ILogger<EntityFrameworkApiResourceStore> logger)
    {
        _context = context;
        _logger = logger;
    }
    /// <summary>
    /// FindApiResourceAsync
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<ApiResource?> FindApiResourceAsync(string name)
    {
        // Note: For real production, ensure 'Scopes' navigation property is correctly configured in EF
        // Assuming ApiResource has a List<string> Scopes which might need a value converter or owned entity
        // However, based on the DbContext, it's just basic property mapping.
        // If ApiResource.Scopes is List<string>, EF Core 8+ supports primitive collections naturally.
        // If < 8, it needs conversion. Assuming net9.0 implies EF Core 9, so it should be fine.
        return await _context.ApiResources.AsNoTracking().FirstOrDefaultAsync(r => r.Name == name);
    }
    /// <summary>
    /// FindApiResourcesByScopeAsync
    /// </summary>
    /// <param name="scopeNames"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
    {
        var names = scopeNames.ToHashSet();
        var allResources = await _context.ApiResources.AsNoTracking().ToListAsync();
        return allResources.Where(r => r.Scopes.Any(s => names.Contains(s)));
    }
    /// <summary>
    /// GetAllResourcesAsync
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ApiResource>> GetAllResourcesAsync()
    {
        return await _context.ApiResources.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Adds a new API resource
    /// </summary>
    /// <param name="resource">The resource to add</param>
    public async Task AddResourceAsync(ApiResource resource)
    {
        _context.ApiResources.Add(resource);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing API resource
    /// </summary>
    /// <param name="resource">The resource to update</param>
    public async Task UpdateResourceAsync(ApiResource resource)
    {
        _context.ApiResources.Update(resource);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes an API resource
    /// </summary>
    /// <param name="name">The name of the resource to delete</param>
    public async Task DeleteResourceAsync(string name)
    {
        var resource = await _context.ApiResources.FindAsync(name);
        if (resource != null)
        {
            _context.ApiResources.Remove(resource);
            await _context.SaveChangesAsync();
        }
    }
}
