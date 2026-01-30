using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.Core.Services;

/// <summary>
/// In-memory implementation of the client store
/// </summary>
public class InMemoryClientStore : IClientStore
{
    private readonly ConcurrentDictionary<string, Client> _clients;

    /// <summary>
    /// Initializes a new instance of the InMemoryClientStore
    /// </summary>
    /// <param name="clients">The initial set of clients</param>
    public InMemoryClientStore(IEnumerable<Client> clients)
    {
        _clients = new ConcurrentDictionary<string, Client>(
            clients.Select(c => new KeyValuePair<string, Client>(c.ClientId, c)));
    }

    /// <summary>
    /// Finds a client by its ID
    /// </summary>
    /// <param name="clientId">The client ID</param>
    /// <returns>The client if found; otherwise null</returns>
    public Task<Client?> FindClientByIdAsync(string clientId)
    {
        _clients.TryGetValue(clientId, out var client);
        return Task.FromResult(client);
    }

    /// <summary>
    /// Gets all clients in the store
    /// </summary>
    /// <returns>An enumerable of clients</returns>
    public Task<IEnumerable<Client>> GetAllClientsAsync()
    {
        return Task.FromResult<IEnumerable<Client>>(_clients.Values);
    }

    /// <summary>
    /// Adds a new client to the store
    /// </summary>
    /// <param name="client">The client to add</param>
    /// <returns>A task representing the operation</returns>
    public Task AddClientAsync(Client client)
    {
        _clients[client.ClientId] = client;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates an existing client in the store
    /// </summary>
    /// <param name="client">The client to update</param>
    /// <returns>A task representing the operation</returns>
    public Task UpdateClientAsync(Client client)
    {
        _clients[client.ClientId] = client;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a client from the store
    /// </summary>
    /// <param name="clientId">The client ID to delete</param>
    /// <returns>A task representing the operation</returns>
    public Task DeleteClientAsync(string clientId)
    {
        _clients.TryRemove(clientId, out _);
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory implementation of the identity resource store
/// </summary>
public class InMemoryIdentityResourceStore : IResourceStore
{
    private readonly ConcurrentDictionary<string, IdentityResource> _resources;

    /// <summary>
    /// Initializes a new instance of the InMemoryIdentityResourceStore
    /// </summary>
    /// <param name="resources">The initial set of resources</param>
    public InMemoryIdentityResourceStore(IEnumerable<IdentityResource> resources)
    {
        _resources = new ConcurrentDictionary<string, IdentityResource>(
            resources.Select(r => new KeyValuePair<string, IdentityResource>(r.Name, r)));
    }

    /// <summary>
    /// Finds identity resources by name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The identity resource if found; otherwise null</returns>
    public Task<IdentityResource?> FindIdentityResourceAsync(string name)
    {
        _resources.TryGetValue(name, out var resource);
        return Task.FromResult(resource);
    }

    /// <summary>
    /// Finds identity resources by scope names
    /// </summary>
    /// <param name="scopeNames">The scope names</param>
    /// <returns>A collection of identity resources</returns>
    public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
    {
        var resources = _resources.Values.Where(r => scopeNames.Contains(r.Name));
        return Task.FromResult(resources);
    }

    /// <summary>
    /// Gets all resources in the store
    /// </summary>
    /// <returns>An enumerable of identity resources</returns>
    public Task<IEnumerable<IdentityResource>> GetAllResourcesAsync()
    {
        return Task.FromResult<IEnumerable<IdentityResource>>(_resources.Values);
    }

    /// <inheritdoc/>
    public Task AddResourceAsync(IdentityResource resource)
    {
        _resources[resource.Name] = resource;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateResourceAsync(IdentityResource resource)
    {
        _resources[resource.Name] = resource;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteResourceAsync(string name)
    {
        _resources.TryRemove(name, out _);
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory implementation of the API scope store
/// </summary>
public class InMemoryApiScopeStore : IApiScopeStore
{
    private readonly ConcurrentDictionary<string, ApiScope> _scopes;

    /// <summary>
    /// Initializes a new instance of the InMemoryApiScopeStore
    /// </summary>
    /// <param name="scopes">The initial set of scopes</param>
    public InMemoryApiScopeStore(IEnumerable<ApiScope> scopes)
    {
        _scopes = new ConcurrentDictionary<string, ApiScope>(
            scopes.Select(s => new KeyValuePair<string, ApiScope>(s.Name, s)));
    }

    /// <summary>
    /// Finds an API scope by name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The API scope if found; otherwise null</returns>
    public Task<ApiScope?> FindApiScopeAsync(string name)
    {
        _scopes.TryGetValue(name, out var scope);
        return Task.FromResult(scope);
    }

    /// <summary>
    /// Finds API scopes by their names
    /// </summary>
    /// <param name="scopeNames">The names of the scopes</param>
    /// <returns>A collection of API scopes</returns>
    public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
    {
        var scopes = _scopes.Values.Where(s => scopeNames.Contains(s.Name));
        return Task.FromResult(scopes);
    }

    /// <summary>
    /// Gets all API scopes in the store
    /// </summary>
    /// <returns>An enumerable of API scopes</returns>
    public Task<IEnumerable<ApiScope>> GetAllScopesAsync()
    {
        return Task.FromResult<IEnumerable<ApiScope>>(_scopes.Values);
    }

    /// <inheritdoc/>
    public Task AddScopeAsync(ApiScope scope)
    {
        _scopes[scope.Name] = scope;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateScopeAsync(ApiScope scope)
    {
        _scopes[scope.Name] = scope;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteScopeAsync(string name)
    {
        _scopes.TryRemove(name, out _);
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory implementation of the API resource store
/// </summary>
public class InMemoryApiResourceStore : IApiResourceStore
{
    private readonly ConcurrentDictionary<string, ApiResource> _resources;

    /// <summary>
    /// Initializes a new instance of the InMemoryApiResourceStore
    /// </summary>
    /// <param name="resources">The initial set of resources</param>
    public InMemoryApiResourceStore(IEnumerable<ApiResource> resources)
    {
        _resources = new ConcurrentDictionary<string, ApiResource>(
            resources.Select(r => new KeyValuePair<string, ApiResource>(r.Name, r)));
    }

    /// <summary>
    /// Finds an API resource by name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The API resource if found; otherwise null</returns>
    public Task<ApiResource?> FindApiResourceAsync(string name)
    {
        _resources.TryGetValue(name, out var resource);
        return Task.FromResult(resource);
    }

    /// <summary>
    /// Finds API resources by scope names
    /// </summary>
    /// <param name="scopeNames">The scope names</param>
    /// <returns>A collection of API resources</returns>
    public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
    {
        var resources = _resources.Values.Where(r => 
            r.Scopes.Any(s => scopeNames.Contains(s)));
        return Task.FromResult(resources);
    }

    /// <summary>
    /// Gets all API resources in the store
    /// </summary>
    /// <returns>An enumerable of API resources</returns>
    public Task<IEnumerable<ApiResource>> GetAllResourcesAsync()
    {
        return Task.FromResult<IEnumerable<ApiResource>>(_resources.Values);
    }

    /// <inheritdoc/>
    public Task AddResourceAsync(ApiResource resource)
    {
        _resources[resource.Name] = resource;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateResourceAsync(ApiResource resource)
    {
        _resources[resource.Name] = resource;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteResourceAsync(string name)
    {
        _resources.TryRemove(name, out _);
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory implementation of the user store using TestUser objects
/// </summary>
public class InMemoryUserStore : IUserStore
{
    private readonly IPasswordHasher? _passwordHasher;
    private readonly ConcurrentDictionary<string, TestUser> _users;

    /// <summary>
    /// Initializes a new instance of the InMemoryUserStore
    /// </summary>
    /// <param name="users">The initial set of test users</param>
    /// <param name="passwordHasher">The password hasher</param>
    public InMemoryUserStore(IEnumerable<TestUser> users, IPasswordHasher? passwordHasher = null)
    {
        _passwordHasher = passwordHasher;
        _users = new ConcurrentDictionary<string, TestUser>(
            users.Select(u => new KeyValuePair<string, TestUser>(u.SubjectId, u)));
    }

    /// <summary>
    /// Finds a user by their subject ID
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <returns>The user if found; otherwise null</returns>
    public Task<User?> FindBySubjectIdAsync(string subjectId)
    {
        if (_users.TryGetValue(subjectId, out var testUser))
        {
            return Task.FromResult<User?>(new User
            {
                SubjectId = testUser.SubjectId,
                Username = testUser.Username,
                Email = testUser.Email,
                IsActive = testUser.IsActive,
                PasswordHash = testUser.Password,
                Claims = testUser.Claims
            });
        }
        return Task.FromResult<User?>(null);
    }

    /// <summary>
    /// Finds a user by their username
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>The user if found; otherwise null</returns>
    public Task<User?> FindByUsernameAsync(string username)
    {
        var testUser = _users.Values.FirstOrDefault(u => u.Username == username);
        if (testUser != null)
        {
            return Task.FromResult<User?>(new User
            {
                SubjectId = testUser.SubjectId,
                Username = testUser.Username,
                Email = testUser.Email,
                IsActive = testUser.IsActive,
                PasswordHash = testUser.Password,
                Claims = testUser.Claims
            });
        }
        return Task.FromResult<User?>(null);
    }

    /// <summary>
    /// Validates user credentials
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The password</param>
    /// <returns>True if credentials are valid; otherwise false</returns>
    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        var user = await FindByUsernameAsync(username);
        if (user == null) return false;

        if (_passwordHasher != null)
        {
            return _passwordHasher.VerifyPassword(user, password);
        }

        // Secure Default: Fail if no hasher is configured
        return false;
    }

    /// <inheritdoc/>
    public Task<(IEnumerable<User> Users, int TotalCount)> GetAllUsersAsync(string? search = null, int skip = 0, int take = 20)
    {
        var query = _users.Values.AsQueryable();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                                    (u.Email != null && u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var total = query.Count();
        var users = query.Skip(skip).Take(take).Select(u => new User
        {
            SubjectId = u.SubjectId,
            Username = u.Username,
            Email = u.Email,
            IsActive = u.IsActive,
            Claims = u.Claims
        });

        return Task.FromResult((users.AsEnumerable(), total));
    }

    /// <inheritdoc/>
    public Task AddUserAsync(User user, string password)
    {
        var testUser = new TestUser
        {
            SubjectId = user.SubjectId,
            Username = user.Username,
            Email = user.Email,
            Password = password,
            IsActive = user.IsActive,
            Claims = user.Claims
        };
        _users[user.SubjectId] = testUser;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateUserAsync(User user)
    {
        if (_users.TryGetValue(user.SubjectId, out var existing))
        {
            existing.Username = user.Username;
            existing.Email = user.Email;
            existing.IsActive = user.IsActive;
            existing.Claims = user.Claims;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteUserAsync(string subjectId)
    {
        _users.TryRemove(subjectId, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SetPasswordAsync(string subjectId, string password)
    {
        if (_users.TryGetValue(subjectId, out var existing))
        {
            existing.Password = password;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task IncrementFailedAttemptsAsync(string subjectId)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ResetFailedAttemptsAsync(string subjectId)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task LockAccountAsync(string subjectId, DateTimeOffset? lockoutEnd)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory implementation of the persisted grant store
/// </summary>
public class InMemoryPersistedGrantStore : IPersistedGrantStore
{
    private readonly ConcurrentDictionary<string, PersistedGrant> _repository = new();

    /// <summary>
    /// Stores the grant
    /// </summary>
    public Task StoreAsync(PersistedGrant grant)
    {
        _repository[grant.Key] = grant;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the grant
    /// </summary>
    public Task<PersistedGrant?> GetAsync(string key)
    {
        _repository.TryGetValue(key, out var grant);
        return Task.FromResult(grant);
    }

    /// <summary>
    /// Gets all grants for a specific subject
    /// </summary>
    public Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
    {
        var grants = _repository.Values.Where(x => x.SubjectId == subjectId);
        return Task.FromResult(grants);
    }

    /// <summary>
    /// Removes the grant by key
    /// </summary>
    public Task RemoveAsync(string key)
    {
        _repository.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes all grants for a subject and client
    /// </summary>
    public Task RemoveAllAsync(string subjectId, string clientId)
    {
        var keys = _repository.Values
            .Where(x => x.SubjectId == subjectId && x.ClientId == clientId)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keys)
        {
            _repository.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes all grants for a subject, client, and type
    /// </summary>
    public Task RemoveAllAsync(string subjectId, string clientId, string type)
    {
        var keys = _repository.Values
            .Where(x => x.SubjectId == subjectId && x.ClientId == clientId && x.Type == type)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keys)
        {
            _repository.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all grants matching a filter
    /// </summary>
    public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
    {
        var query = _repository.Values.AsQueryable();

        if (!string.IsNullOrEmpty(filter.SubjectId))
            query = query.Where(x => x.SubjectId == filter.SubjectId);
        
        if (!string.IsNullOrEmpty(filter.ClientId))
            query = query.Where(x => x.ClientId == filter.ClientId);

        if (!string.IsNullOrEmpty(filter.SessionId))
            query = query.Where(x => x.SessionId == filter.SessionId);

        if (!string.IsNullOrEmpty(filter.Type))
            query = query.Where(x => x.Type == filter.Type);

        return Task.FromResult<IEnumerable<PersistedGrant>>(query.ToList());
    }

    /// <summary>
    /// Removes all grants matching a filter
    /// </summary>
    public Task RemoveAllAsync(PersistedGrantFilter filter)
    {
        var query = _repository.Values.AsQueryable();

        if (!string.IsNullOrEmpty(filter.SubjectId))
            query = query.Where(x => x.SubjectId == filter.SubjectId);
        
        if (!string.IsNullOrEmpty(filter.ClientId))
            query = query.Where(x => x.ClientId == filter.ClientId);

        if (!string.IsNullOrEmpty(filter.SessionId))
            query = query.Where(x => x.SessionId == filter.SessionId);

        if (!string.IsNullOrEmpty(filter.Type))
            query = query.Where(x => x.Type == filter.Type);

        var keys = query.Select(x => x.Key).ToList();
        foreach (var key in keys)
        {
            _repository.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}