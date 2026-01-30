using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for user management and credential validation
/// </summary>
public class UserService : IUserService
{
    private readonly IUserStore _userStore;

    /// <summary>
    /// Initializes a new instance of the UserService
    /// </summary>
    /// <param name="userStore">The user store</param>
    public UserService(IUserStore userStore)
    {
        _userStore = userStore;
    }

    /// <summary>
    /// Finds a user by their subject ID
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <returns>The user if found; otherwise null</returns>
    public async Task<User?> FindBySubjectIdAsync(string subjectId)
    {
        return await _userStore.FindBySubjectIdAsync(subjectId);
    }

    /// <summary>
    /// Finds a user by their username
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>The user if found; otherwise null</returns>
    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await _userStore.FindByUsernameAsync(username);
    }

    /// <summary>
    /// Validates user credentials
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The password</param>
    /// <returns>True if credentials are valid; otherwise false</returns>
    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        return await _userStore.ValidateCredentialsAsync(username, password);
    }

    /// <summary>
    /// Gets claims for a user
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns>A collection of claims</returns>
    public Task<IEnumerable<Claim>> GetClaimsAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("sub", user.SubjectId),
            new Claim("name", user.Username)
        };

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim("email", user.Email));
            claims.Add(new Claim("email_verified", user.EmailVerified.ToString().ToLower()));
        }

        claims.AddRange(user.Claims);

        return Task.FromResult<IEnumerable<Claim>>(claims);
    }

    /// <summary>
    /// Increments the failed login attempts for a user
    /// </summary>
    public async Task IncrementFailedAttemptsAsync(string subjectId)
    {
        await _userStore.IncrementFailedAttemptsAsync(subjectId);
    }

    /// <summary>
    /// Resets the failed login attempts for a user
    /// </summary>
    public async Task ResetFailedAttemptsAsync(string subjectId)
    {
        await _userStore.ResetFailedAttemptsAsync(subjectId);
    }

    /// <summary>
    /// Locks the user account until a specific date
    /// </summary>
    public async Task LockAccountAsync(string subjectId, DateTimeOffset? lockoutEnd)
    {
        await _userStore.LockAccountAsync(subjectId, lockoutEnd);
    }
}