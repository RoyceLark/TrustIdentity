using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
namespace TrustIdentity.Abstractions.Stores;

/// <summary>
/// Interface for retrieving client configuration
/// </summary>
public interface IClientStore
{
    /// <summary>
    /// Finds a client by its client ID
    /// </summary>
    /// <param name="clientId">The client ID</param>
    /// <returns>The client object or null if not found</returns>
    Task<Client?> FindClientByIdAsync(string clientId);
    
    /// <summary>
    /// Gets all registered clients
    /// </summary>
    /// <returns>A collection of all clients</returns>
    Task<IEnumerable<Client>> GetAllClientsAsync();
    
    /// <summary>
    /// Adds a new client
    /// </summary>
    /// <param name="client">The client to add</param>
    /// <returns>A task representing the operation</returns>
    Task AddClientAsync(Client client);
    
    /// <summary>
    /// Updates an existing client
    /// </summary>
    /// <param name="client">The client with updated values</param>
    /// <returns>A task representing the operation</returns>
    Task UpdateClientAsync(Client client);
    
    /// <summary>
    /// Deletes a client by its client ID
    /// </summary>
    /// <param name="clientId">The ID of the client to delete</param>
    /// <returns>A task representing the operation</returns>
    Task DeleteClientAsync(string clientId);
}

/// <summary>
/// Interface for retrieving identity resources (e.g. openid, profile, email)
/// </summary>
public interface IResourceStore
{
    /// <summary>
    /// Finds an identity resource by name
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <returns>The identity resource or null if not found</returns>
    Task<IdentityResource?> FindIdentityResourceAsync(string name);
    
    /// <summary>
    /// Finds identity resources by scope names
    /// </summary>
    /// <param name="scopeNames">The requested scopes</param>
    /// <returns>A collection of matching identity resources</returns>
    Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames);
    
    /// <summary>
    /// Gets all identity resources
    /// </summary>
    /// <returns>A collection of all identity resources</returns>
    Task<IEnumerable<IdentityResource>> GetAllResourcesAsync();

    /// <summary>
    /// Adds a new identity resource
    /// </summary>
    Task AddResourceAsync(IdentityResource resource);

    /// <summary>
    /// Updates an existing identity resource
    /// </summary>
    Task UpdateResourceAsync(IdentityResource resource);

    /// <summary>
    /// Deletes an identity resource
    /// </summary>
    Task DeleteResourceAsync(string name);
}

/// <summary>
/// Interface for retrieving API scopes
/// </summary>
public interface IApiScopeStore
{
    /// <summary>
    /// Finds an API scope by name
    /// </summary>
    /// <param name="name">The name of the scope</param>
    /// <returns>The API scope or null if not found</returns>
    Task<ApiScope?> FindApiScopeAsync(string name);
    
    /// <summary>
    /// Finds API scopes by their names
    /// </summary>
    /// <param name="scopeNames">The requested scopes</param>
    /// <returns>A collection of matching API scopes</returns>
    Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames);
    
    /// <summary>
    /// Gets all API scopes
    /// </summary>
    /// <returns>A collection of all API scopes</returns>
    Task<IEnumerable<ApiScope>> GetAllScopesAsync();

    /// <summary>
    /// Adds a new API scope
    /// </summary>
    Task AddScopeAsync(ApiScope scope);

    /// <summary>
    /// Updates an existing API scope
    /// </summary>
    Task UpdateScopeAsync(ApiScope scope);

    /// <summary>
    /// Deletes an API scope
    /// </summary>
    Task DeleteScopeAsync(string name);
}

/// <summary>
/// Interface for retrieving API resources
/// </summary>
public interface IApiResourceStore
{
    /// <summary>
    /// Finds an API resource by name
    /// </summary>
    /// <param name="name">The name of the API</param>
    /// <returns>The API resource or null if not found</returns>
    Task<ApiResource?> FindApiResourceAsync(string name);
    
    /// <summary>
    /// Finds API resources that contain specific scopes
    /// </summary>
    /// <param name="scopeNames">The requested scopes</param>
    /// <returns>A collection of matching API resources</returns>
    Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames);
    
    /// <summary>
    /// Gets all API resources
    /// </summary>
    /// <returns>A collection of all API resources</returns>
    Task<IEnumerable<ApiResource>> GetAllResourcesAsync();

    /// <summary>
    /// Adds a new API resource
    /// </summary>
    Task AddResourceAsync(ApiResource resource);

    /// <summary>
    /// Updates an existing API resource
    /// </summary>
    Task UpdateResourceAsync(ApiResource resource);

    /// <summary>
    /// Deletes an API resource
    /// </summary>
    Task DeleteResourceAsync(string name);
}

/// <summary>
/// Interface for user management and retrieval
/// </summary>
public interface IUserStore
{
    /// <summary>
    /// Finds a user by their unique subject ID
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <returns>The user or null if not found</returns>
    Task<User?> FindBySubjectIdAsync(string subjectId);
    
    /// <summary>
    /// Finds a user by their username
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>The user or null if not found</returns>
    Task<User?> FindByUsernameAsync(string username);
    
    /// <summary>
    /// Validates a user's credentials (username and password)
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The password</param>
    /// <returns>True if credentials are valid; otherwise false</returns>
    Task<bool> ValidateCredentialsAsync(string username, string password);
    
    /// <summary>
    /// Gets all users with pagination
    /// </summary>
    Task<(IEnumerable<User> Users, int TotalCount)> GetAllUsersAsync(string? search = null, int skip = 0, int take = 20);
    
    /// <summary>
    /// Adds a new user
    /// </summary>
    Task AddUserAsync(User user, string password);
    
    /// <summary>
    /// Updates an existing user
    /// </summary>
    Task UpdateUserAsync(User user);
    
    /// <summary>
    /// Deletes a user
    /// </summary>
    Task DeleteUserAsync(string subjectId);
    
    /// <summary>
    /// Sets/Updates user password
    /// </summary>
    Task SetPasswordAsync(string subjectId, string password);

    /// <summary>
    /// Increments the failed login attempts for a user
    /// </summary>
    Task IncrementFailedAttemptsAsync(string subjectId);

    /// <summary>
    /// Resets the failed login attempts for a user
    /// </summary>
    Task ResetFailedAttemptsAsync(string subjectId);

    /// <summary>
    /// Locks the user account until a specific date
    /// </summary>
    Task LockAccountAsync(string subjectId, DateTimeOffset? lockoutEnd);
}

/// <summary>
/// Interface for storing and retrieving persisted grants (authorization codes, refresh tokens, etc.)
/// </summary>
public interface IPersistedGrantStore
{
    /// <summary>
    /// Stores a new grant
    /// </summary>
    /// <param name="grant">The grant to store</param>
    /// <returns>A task representing the operation</returns>
    Task StoreAsync(PersistedGrant grant);
    
    /// <summary>
    /// Gets a grant by its key
    /// </summary>
    /// <param name="key">The unique key of the grant</param>
    /// <returns>The persisted grant or null if not found</returns>
    Task<PersistedGrant?> GetAsync(string key);
    
    /// <summary>
    /// Gets all grants for a specific subject ID
    /// </summary>
    /// <param name="subjectId">The user subject ID</param>
    /// <returns>A collection of grants for the user</returns>
    Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId);
    
    /// <summary>
    /// Removes a grant by its key
    /// </summary>
    /// <param name="key">The unique key of the grant</param>
    /// <returns>A task representing the operation</returns>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Removes all grants for a subject and client
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <param name="clientId">The client ID</param>
    /// <returns>A task representing the operation</returns>
    Task RemoveAllAsync(string subjectId, string clientId);
    
    /// <summary>
    /// Removes all grants for a subject, client and grant type
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <param name="clientId">The client ID</param>
    /// <param name="type">The grant type</param>
    /// <returns>A task representing the operation</returns>
    Task RemoveAllAsync(string subjectId, string clientId, string type);

    /// <summary>
    /// Gets all grants matching a filter
    /// </summary>
    Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter);

    /// <summary>
    /// Removes all grants matching a filter
    /// </summary>
    Task RemoveAllAsync(PersistedGrantFilter filter);
}

/// <summary>
/// Store for authorization codes used in OAuth 2.0 authorization code flow
/// </summary>
public interface IAuthorizationCodeStore
{
    /// <summary>
    /// Retrieves an authorization code
    /// </summary>
    /// <param name="code">The authorization code value</param>
    /// <returns>The authorization code object or null if not found</returns>
    Task<AuthorizationCode?> GetAuthorizationCodeAsync(string code);
    
    /// <summary>
    /// Stores a new authorization code
    /// </summary>
    /// <param name="authCode">The authorization code object to store</param>
    /// <returns>A task representing the operation</returns>
    Task StoreAuthorizationCodeAsync(AuthorizationCode authCode);
    
    /// <summary>
    /// Removes an authorization code (typically after use)
    /// </summary>
    /// <param name="code">The authorization code value</param>
    /// <returns>A task representing the operation</returns>
    Task RemoveAuthorizationCodeAsync(string code);
}

/// <summary>
/// Store for device flow codes
/// </summary>
public interface IDeviceFlowStore
{
    /// <summary>
    /// Stores device flow codes
    /// </summary>
    Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceFlowCodes data);
    
    /// <summary>
    /// Finds codes by user code
    /// </summary>
    Task<DeviceFlowCodes?> FindByUserCodeAsync(string userCode);
    
    /// <summary>
    /// Finds codes by device code
    /// </summary>
    Task<DeviceFlowCodes?> FindByDeviceCodeAsync(string deviceCode);
    
    /// <summary>
    /// Updates codes by user code
    /// </summary>
    Task UpdateByUserCodeAsync(string userCode, DeviceFlowCodes data);
    
    /// <summary>
    /// Removes codes by device code
    /// </summary>
    Task RemoveByDeviceCodeAsync(string deviceCode);
}

/// <summary>
/// Password hashing and verification service
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password for a user
    /// </summary>
    /// <param name="user">The user for whom the password is being hashed</param>
    /// <param name="password">The plain text password</param>
    /// <returns>The hashed password</returns>
    string HashPassword(User user, string password);
    
    /// <summary>
    /// Verifies a password against the user's stored hash
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password">The plain text password to verify</param>
    /// <returns>True if password is correct; otherwise false</returns>
    bool VerifyPassword(User user, string password);
}
/// <summary>
/// Interface for audit and event storage
/// </summary>
public interface IAuditStore
{
    /// <summary>
    /// Stores an audit event
    /// </summary>
    Task StoreAsync(AuditEvent evt);
    
    /// <summary>
    /// Gets recent audit events
    /// </summary>
    Task<IEnumerable<AuditEvent>> GetRecentEventsAsync(int count = 50);
    
    /// <summary>
    /// Gets stats for the dashboard
    /// </summary>
    Task<AuditStats> GetStatsAsync();
}

/// <summary>
/// Interface for server-side user session storage
/// </summary>
public interface ISessionStore
{
    /// <summary>
    /// Stores a session
    /// </summary>
    Task StoreAsync(UserSession session);
    
    /// <summary>
    /// Gets a session by ID
    /// </summary>
    Task<UserSession?> GetAsync(string sessionId);
    
    /// <summary>
    /// Gets all sessions for a user
    /// </summary>
    Task<IEnumerable<UserSession>> GetUserSessionsAsync(string subjectId);
    
    /// <summary>
    /// Removes a session
    /// </summary>
    Task RemoveAsync(string sessionId);
    
    /// <summary>
    /// Removes all sessions for a user
    /// </summary>
    Task RemoveUserSessionsAsync(string subjectId);
}

/// <summary>
/// Interface for user consent storage
/// </summary>
public interface IConsentStore
{
    /// <summary>
    /// Retrieves user consent
    /// </summary>
    Task<UserConsent?> GetAsync(string subjectId, string clientId);
    
    /// <summary>
    /// Stores user consent
    /// </summary>
    Task StoreAsync(UserConsent consent);
    
    /// <summary>
    /// Removes user consent
    /// </summary>
    Task RemoveAsync(string subjectId, string clientId);
}

/// <summary>
/// Represents an audit event for storage
/// </summary>
/// <summary>
/// Represents an audit event for storage
/// </summary>
public class AuditEvent
{
    /// <summary>Unique identifier for the event</summary>
    public long Id { get; set; }
    
    /// <summary>The type of event (e.g., "Login", "Logout")</summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>Descriptive message for the event</summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>Timestamp when the event occurred (UTC)</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>Subject ID of the user involved (if applicable)</summary>
    public string? SubjectId { get; set; }
    
    /// <summary>Client ID involved (if applicable)</summary>
    public string? ClientId { get; set; }
    
    /// <summary>IP address of the requester</summary>
    public string? IpAddress { get; set; }
    
    /// <summary>Additional data serialized as JSON</summary>
    public string? Data { get; set; }
}

/// <summary>
/// Aggregated statistics from audit logs
/// </summary>
public class AuditStats
{
    /// <summary>Number of tokens issued today</summary>
    public int TokensIssuedToday { get; set; }
    
    /// <summary>Number of failed login attempts today</summary>
    public int FailedLoginsToday { get; set; }
    
    /// <summary>Number of successful logins today</summary>
    public int SuccessLoginsToday { get; set; }
    
    /// <summary>Trend of logins over time</summary>
    public List<(string Label, int Value)> LoginTrend { get; set; } = new();
}
