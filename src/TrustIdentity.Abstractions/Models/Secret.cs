namespace TrustIdentity.Abstractions.Models;

using System;
/// <summary>
/// Represents a secret (e.g. client secret, api secret)
/// </summary>
public class Secret
{
    /// <summary>Primary key</summary>
    public int Id { get; set; }
    /// <summary>The secret value</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>The secret type (e.g. SharedSecret, X509CertificateThumbprint)</summary>
    public string Type { get; set; } = "SharedSecret";
    /// <summary>A description for the secret</summary>
    public string? Description { get; set; }
    /// <summary>Expiration date for the secret</summary>
    public DateTime? Expiration { get; set; }
}

/// <summary>
/// Represents a client secret (alias for Secret)
/// </summary>
public class ClientSecret : Secret
{
}

/// <summary>
/// Extensions for <see cref="Secret"/>
/// </summary>
public static class SecretExtensions
{
    /// <summary>
    /// Hashes the input string using SHA256 and returns a <see cref="Secret"/> object
    /// </summary>
    /// <param name="value">The plain text value</param>
    /// <returns>A hashed secret</returns>
    public static Secret Sha256(this string value)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        var hash = sha.ComputeHash(bytes);
        return new Secret
        {
            Value = Convert.ToBase64String(hash),
            Type = "SharedSecret"
        };
    }
}