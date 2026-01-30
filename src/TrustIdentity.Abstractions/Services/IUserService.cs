using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for user management and retrieval operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Finds a user by their unique subject ID
    /// </summary>
    /// <param name="subjectId">The user's unique subject ID</param>
    /// <returns>The user object or null if not found</returns>
    Task<User?> FindBySubjectIdAsync(string subjectId);
    
    /// <summary>
    /// Finds a user by their username
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>The user object or null if not found</returns>
    Task<User?> FindByUsernameAsync(string username);
    
    /// <summary>
    /// Validates a user's credentials
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The password</param>
    /// <returns>True if credentials are valid; otherwise false</returns>
    Task<bool> ValidateCredentialsAsync(string username, string password);
    
    /// <summary>
    /// Retrieves claims for a specific user
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns>A collection of claims</returns>
    Task<IEnumerable<System.Security.Claims.Claim>> GetClaimsAsync(User user);

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