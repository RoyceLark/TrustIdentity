using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TrustIdentity.Core.Models;

/// <summary>
/// Extension methods for secret handling
/// </summary>
public static class SecretExtensions
{
    /// <summary>
    /// Computes the SHA-256 hash of the input string
    /// </summary>
    /// <param name="input">The string to hash</param>
    /// <returns>The base64 encoded hash</returns>
    public static string Sha256(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Computes the SHA-512 hash of the input string
    /// </summary>
    /// <param name="input">The string to hash</param>
    /// <returns>The base64 encoded hash</returns>
    public static string Sha512(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        using var sha = SHA512.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        
        return Convert.ToBase64String(hash);
    }
}