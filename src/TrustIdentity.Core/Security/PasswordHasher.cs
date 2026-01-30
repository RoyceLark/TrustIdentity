using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;
using System.Security.Cryptography;
using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TrustIdentity.Core.Security;

/// <summary>
/// Default implementation of password hasher using PBKDF2
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    // OWASP 2023 recommendation for PBKDF2-SHA256
    private const int Iterations = 600000;
    private const int SaltSize = 16; // 128 bit 
    private const int KeySize = 32;  // 256 bit

    private readonly ILogger<PasswordHasher> _logger;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Hashes a password per user
    /// </summary>
    public string HashPassword(User user, string password)
    {
        // Generate a 128-bit salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Derive a 256-bit subkey (use HMACSHA256 with 600,000 iterations - OWASP 2023)
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        var hashed = Convert.ToBase64String(hashBytes);

        // Format: {iterations}.{salt}.{hash}
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{hashed}";
    }

    /// <summary>
    /// Verifies the password
    /// </summary>
    public bool VerifyPassword(User user, string password)
    {
        // Check if password hash is present
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            _logger.LogWarning("Password hash is null or empty for user {UserId}", user.SubjectId);
            return false;
        }

        // SECURITY: Removed plaintext password fallback
        // All passwords MUST be properly hashed

        try
        {
            var parts = user.PasswordHash.Split('.');
            if (parts.Length != 3)
            {
                _logger.LogError("Invalid password hash format for user {UserId}. Expected format: iterations.salt.hash", user.SubjectId);
                return false;
            }

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var storedHash = parts[2];

            var newHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            var newHash = Convert.ToBase64String(newHashBytes);

            // Use constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(storedHash),
                Encoding.UTF8.GetBytes(newHash));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password for user {UserId}", user.SubjectId);
            return false;
        }
    }
}
