using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Storage.EntityFramework;
using System.Linq;

namespace TrustIdentity.Storage.Stores;

/// <summary>
/// Entity Framework implementation of IUserStore
/// </summary>
public class EntityFrameworkUserStore : IUserStore
{
    private readonly TrustIdentityDbContext _context;
    // Note: TrustIdentityDbContext contains Users, but strictly speaking 
    // ConfigurationDbContext and PersistedGrantDbContext are separate. 
    // Ideally we assume a separate IdentityDbContext or use the Combined one.
    // For this implementation, let's assume we use TrustIdentityDbContext for users.
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// EntityFrameworkUserStore
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="passwordHasher">The password hasher</param>
    public EntityFrameworkUserStore(TrustIdentityDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }
    /// <summary>
    /// FindBySubjectIdAsync
    /// </summary>
    /// <param name="subjectId"></param>
    /// <returns></returns>
    public async Task<User?> FindBySubjectIdAsync(string subjectId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.SubjectId == subjectId);
            
        return user == null ? null : ToModel(user);
    }
    /// <summary>
    /// FindByUsernameAsync
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task<User?> FindByUsernameAsync(string username)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Username == username);

        return user == null ? null : ToModel(user);
    }
    /// <summary>
    /// ToModel
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private static User ToModel(Core.Models.TrustIdentityUser user)
    {
        return new User 
        { 
            SubjectId = user.SubjectId, 
            Username = user.Username ?? string.Empty,
            Email = user.Email, 
            IsActive = user.IsActive,
            PasswordHash = user.Password,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LastLoginDate = user.LastLogin,
            LockoutEnd = user.LockoutEnd.HasValue ? new DateTimeOffset(user.LockoutEnd.Value) : null,
            CreatedDate = user.Created,
            Claims = user.Claims.Select(c => c.ToClaim()).ToList()
        };
    }
    /// <summary>
    /// ValidateCredentialsAsync
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return false;
        
        // Check if locked
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            return false;
        }

        var absUser = new User
        {
            SubjectId = user.SubjectId,
            Username = user.Username ?? string.Empty,
            PasswordHash = user.Password
        };

        return _passwordHasher.VerifyPassword(absUser, password);
    }

    /// <summary>
    /// Gets all users with pagination
    /// </summary>
    public async Task<(IEnumerable<TrustIdentity.Abstractions.Models.User> Users, int TotalCount)> GetAllUsersAsync(string? search = null, int skip = 0, int take = 20)
    {
        var query = _context.Users.AsNoTracking();
        
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => (u.Username != null && u.Username.Contains(search)) || (u.Email != null && u.Email.Contains(search)));
        }
        
        var total = await query.CountAsync();
        var users = await query.OrderBy(u => u.Username)
            .Skip(skip)
            .Take(take)
            .Include(u => u.Claims)
            .ToListAsync();
            
        return (users.Select(ToModel), total);
    }

    /// <summary>
    /// Adds a new user
    /// </summary>
    public async Task AddUserAsync(User user, string password)
    {
        var entity = new Core.Models.TrustIdentityUser
        {
            SubjectId = user.SubjectId ?? Guid.NewGuid().ToString(),
            Username = user.Username,
            Email = user.Email,
            IsActive = true,
            Created = DateTime.UtcNow
        };
        
        entity.Password = _passwordHasher.HashPassword(user, password);
        
        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    public async Task UpdateUserAsync(User user)
    {
        var entity = await _context.Users.FindAsync(user.SubjectId);
        if (entity == null) return;
        
        entity.Email = user.Email;
        entity.Username = user.Username;
        entity.IsActive = user.IsActive;
        entity.FailedLoginAttempts = user.FailedLoginAttempts;
        entity.LockoutEnd = user.LockoutEnd?.DateTime;
        entity.Updated = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    public async Task DeleteUserAsync(string subjectId)
    {
        var entity = await _context.Users.FindAsync(subjectId);
        if (entity != null)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Sets/Updates user password
    /// </summary>
    public async Task SetPasswordAsync(string subjectId, string password)
    {
        var entity = await _context.Users.FindAsync(subjectId);
        if (entity == null) return;
        
        var user = ToModel(entity);
        entity.Password = _passwordHasher.HashPassword(user, password);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Increments the failed login attempts for a user
    /// </summary>
    public async Task IncrementFailedAttemptsAsync(string subjectId)
    {
        var entity = await _context.Users.FindAsync(subjectId);
        if (entity != null)
        {
            entity.FailedLoginAttempts++;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Resets the failed login attempts for a user
    /// </summary>
    public async Task ResetFailedAttemptsAsync(string subjectId)
    {
        var entity = await _context.Users.FindAsync(subjectId);
        if (entity != null)
        {
            entity.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Locks the user account until a specific date
    /// </summary>
    public async Task LockAccountAsync(string subjectId, DateTimeOffset? lockoutEnd)
    {
        var entity = await _context.Users.FindAsync(subjectId);
        if (entity != null)
        {
            entity.LockoutEnd = lockoutEnd?.DateTime;
            await _context.SaveChangesAsync();
        }
    }
}
