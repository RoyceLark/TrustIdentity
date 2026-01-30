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
/// Entity Framework implementation of IClientStore
/// </summary>
public class EntityFrameworkClientStore : IClientStore
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<EntityFrameworkClientStore> _logger;

    /// <summary>
    /// Initializes a new instance of the EntityFrameworkClientStore
    /// </summary>
    /// <param name="context">The configuration database context</param>
    /// <param name="logger">The logger instance</param>
    public EntityFrameworkClientStore(
        ConfigurationDbContext context,
        ILogger<EntityFrameworkClientStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Finds a client by its ID
    /// </summary>
    /// <param name="clientId">The client ID</param>
    /// <returns>The client if found; otherwise null</returns>
    public async Task<Client?> FindClientByIdAsync(string clientId)
    {
        return await _context.Clients
            .AsNoTracking()
            .Include(c => c.ClientSecrets)
            .Include(c => c.Claims)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);
    }

    /// <summary>
    /// Gets all registered clients
    /// </summary>
    /// <returns>A collection of all clients</returns>
    public async Task<IEnumerable<Client>> GetAllClientsAsync()
    {
        return await _context.Clients
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new client to the store
    /// </summary>
    /// <param name="client">The client to add</param>
    public async Task AddClientAsync(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing client in the store
    /// </summary>
    /// <param name="client">The client to update</param>
    public async Task UpdateClientAsync(Client client)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a client by its ID
    /// </summary>
    /// <param name="clientId">The client ID to delete</param>
    public async Task DeleteClientAsync(string clientId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client != null)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }
    }
}

/// <summary>
/// Entity Framework implementation of IResourceStore
/// </summary>
public class EntityFrameworkResourceStore : IResourceStore
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<EntityFrameworkResourceStore> _logger;

    /// <summary>
    /// Initializes a new instance of the EntityFrameworkResourceStore
    /// </summary>
    /// <param name="context">The configuration database context</param>
    /// <param name="logger">The logger instance</param>
    public EntityFrameworkResourceStore(
        ConfigurationDbContext context,
        ILogger<EntityFrameworkResourceStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Finds an identity resource by its name
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <returns>The identity resource if found; otherwise null</returns>
    public async Task<IdentityResource?> FindIdentityResourceAsync(string name)
    {
        return await _context.IdentityResources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    /// <summary>
    /// Finds identity resources by their scope names
    /// </summary>
    /// <param name="scopeNames">The list of scope names</param>
    /// <returns>A collection of matching identity resources</returns>
    public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(
        IEnumerable<string> scopeNames)
    {
        var names = scopeNames.ToHashSet();
        return await _context.IdentityResources
            .AsNoTracking()
            .Where(r => names.Contains(r.Name))
            .ToListAsync();
    }

    /// <summary>
    /// Gets all registered identity resources
    /// </summary>
    /// <returns>A collection of all identity resources</returns>
    public async Task<IEnumerable<IdentityResource>> GetAllResourcesAsync()
    {
        return await _context.IdentityResources
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new identity resource
    /// </summary>
    public async Task AddResourceAsync(IdentityResource resource)
    {
        _context.IdentityResources.Add(resource);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing identity resource
    /// </summary>
    public async Task UpdateResourceAsync(IdentityResource resource)
    {
        _context.IdentityResources.Update(resource);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes an identity resource by name
    /// </summary>
    public async Task DeleteResourceAsync(string name)
    {
        var resource = await _context.IdentityResources.FindAsync(name);
        if (resource != null)
        {
            _context.IdentityResources.Remove(resource);
            await _context.SaveChangesAsync();
        }
    }
}

/// <summary>
/// Entity Framework implementation of IPersistedGrantStore
/// </summary>
public class EntityFrameworkPersistedGrantStore : IPersistedGrantStore
{
    private readonly PersistedGrantDbContext _context;
    private readonly ILogger<EntityFrameworkPersistedGrantStore> _logger;

    /// <summary>
    /// Initializes a new instance of the EntityFrameworkPersistedGrantStore
    /// </summary>
    /// <param name="context">The persisted grant database context</param>
    /// <param name="logger">The logger instance</param>
    public EntityFrameworkPersistedGrantStore(
        PersistedGrantDbContext context,
        ILogger<EntityFrameworkPersistedGrantStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Stores a persisted grant
    /// </summary>
    /// <param name="grant">The grant to store</param>
    public async Task StoreAsync(PersistedGrant grant)
    {
        var existing = await _context.PersistedGrants.FindAsync(grant.Key);
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(grant);
        }
        else
        {
            _context.PersistedGrants.Add(grant);
        }
        
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets a persisted grant by its key
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>The persisted grant if found; otherwise null</returns>
    public async Task<PersistedGrant?> GetAsync(string key)
    {
        return await _context.PersistedGrants
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Key == key);
    }

    /// <summary>
    /// Gets all persisted grants for a subject
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <returns>A collection of persisted grants</returns>
    public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
    {
        return await _context.PersistedGrants
            .AsNoTracking()
            .Where(g => g.SubjectId == subjectId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all persisted grants matching a filter
    /// </summary>
    /// <param name="filter">The filter criteria</param>
    /// <returns>A collection of matching persisted grants</returns>
    public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
    {
        var query = _context.PersistedGrants.AsNoTracking();

        if (!string.IsNullOrEmpty(filter.SubjectId))
            query = query.Where(g => g.SubjectId == filter.SubjectId);

        if (!string.IsNullOrEmpty(filter.SessionId))
            query = query.Where(g => g.SessionId == filter.SessionId);

        if (!string.IsNullOrEmpty(filter.ClientId))
            query = query.Where(g => g.ClientId == filter.ClientId);

        if (!string.IsNullOrEmpty(filter.Type))
            query = query.Where(g => g.Type == filter.Type);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Removes a persisted grant by its key
    /// </summary>
    /// <param name="key">The key</param>
    public async Task RemoveAsync(string key)
    {
        var grant = await _context.PersistedGrants.FindAsync(key);
        if (grant != null)
        {
            _context.PersistedGrants.Remove(grant);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Removes all persisted grants for a subject and client
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <param name="clientId">The client ID</param>
    public async Task RemoveAllAsync(string subjectId, string clientId)
    {
        var grants = await _context.PersistedGrants
            .Where(g => g.SubjectId == subjectId && g.ClientId == clientId)
            .ToListAsync();
        
        _context.PersistedGrants.RemoveRange(grants);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes all persisted grants for a subject, client and type
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <param name="clientId">The client ID</param>
    /// <param name="type">The grant type</param>
    public async Task RemoveAllAsync(string subjectId, string clientId, string type)
    {
        var grants = await _context.PersistedGrants
            .Where(g => g.SubjectId == subjectId && g.ClientId == clientId && g.Type == type)
            .ToListAsync();
        
        _context.PersistedGrants.RemoveRange(grants);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes all persisted grants matching a filter
    /// </summary>
    /// <param name="filter">The filter criteria</param>
    public async Task RemoveAllAsync(PersistedGrantFilter filter)
    {
        var query = _context.PersistedGrants.AsQueryable();

        if (!string.IsNullOrEmpty(filter.SubjectId))
            query = query.Where(g => g.SubjectId == filter.SubjectId);

        if (!string.IsNullOrEmpty(filter.ClientId))
            query = query.Where(g => g.ClientId == filter.ClientId);

        if (!string.IsNullOrEmpty(filter.Type))
            query = query.Where(g => g.Type == filter.Type);

        _context.PersistedGrants.RemoveRange(query);
        await _context.SaveChangesAsync();
    }
}