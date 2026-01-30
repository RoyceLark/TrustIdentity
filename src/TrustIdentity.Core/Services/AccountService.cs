using System;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Services;
using System.Security.Cryptography;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for account-related actions like password reset and email confirmation.
/// </summary>
public class AccountService
{
    private readonly IPersistedGrantStore _grantStore;
    private readonly IUserStore _userStore;
    private readonly IEmailSender _emailSender;

    /// <summary>
    /// Initializes a new instance of the AccountService
    /// </summary>
    public AccountService(IPersistedGrantStore grantStore, IUserStore userStore, IEmailSender emailSender)
    {
        _grantStore = grantStore;
        _userStore = userStore;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Creates a password reset token for a user
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <returns>A one-time token for password reset</returns>
    public async Task<string> CreatePasswordResetTokenAsync(string email)
    {
        var user = await _userStore.FindByUsernameAsync(email); // Assuming email as username or searching by email
        if (user == null) return string.Empty;

        var token = Guid.NewGuid().ToString("N");
        var grant = new PersistedGrant
        {
            Key = token,
            Type = "password_reset",
            SubjectId = user.SubjectId,
            ClientId = "system",
            CreationTime = DateTime.UtcNow,
            Expiration = DateTime.UtcNow.AddHours(1),
            Data = user.Email ?? string.Empty // Store email to verify later
        };

        await _grantStore.StoreAsync(grant);
        return token;
    }

    /// <summary>
    /// Resets a user's password using a valid token
    /// </summary>
    /// <param name="token">The reset token</param>
    /// <param name="newPassword">The new password</param>
    /// <returns>True if password was successfully reset</returns>
    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var grant = await _grantStore.GetAsync(token);
        if (grant == null || grant.Type != "password_reset" || grant.Expiration < DateTime.UtcNow || string.IsNullOrEmpty(grant.SubjectId))
        {
            return false;
        }

        await _userStore.SetPasswordAsync(grant.SubjectId, newPassword);
        await _grantStore.RemoveAsync(token);
        return true;
    }

    /// <summary>
    /// Creates an email verification token for a user
    /// </summary>
    /// <param name="subjectId">The user's subject ID</param>
    /// <returns>A one-time token for email verification</returns>
    public async Task<string> CreateEmailVerificationTokenAsync(string subjectId)
    {
        var token = Guid.NewGuid().ToString("N");
        var grant = new PersistedGrant
        {
            Key = token,
            Type = "email_verification",
            SubjectId = subjectId,
            ClientId = "system",
            CreationTime = DateTime.UtcNow,
            Expiration = DateTime.UtcNow.AddDays(1)
        };

        await _grantStore.StoreAsync(grant);
        return token;
    }

    /// <summary>
    /// Verifies a user's email using a valid token
    /// </summary>
    /// <param name="token">The verification token</param>
    /// <returns>True if email was successfully verified</returns>
    public async Task<bool> VerifyEmailAsync(string token)
    {
        var grant = await _grantStore.GetAsync(token);
        if (grant == null || grant.Type != "email_verification" || grant.Expiration < DateTime.UtcNow || string.IsNullOrEmpty(grant.SubjectId))
        {
            return false;
        }

        var user = await _userStore.FindBySubjectIdAsync(grant.SubjectId);
        if (user != null)
        {
            user.EmailVerified = true;
            await _userStore.UpdateUserAsync(user);
            await _grantStore.RemoveAsync(token);
            return true;
        }

        return false;
    }
}
