using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Models;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for client management and validation
/// </summary>
public class ClientService
{
    private readonly IClientStore _clientStore;
    private readonly ILogger<ClientService> _logger;

    /// <summary>
    /// Initializes a new instance of the ClientService
    /// </summary>
    public ClientService(IClientStore clientStore, ILogger<ClientService> logger)
    {
        _clientStore = clientStore;
        _logger = logger;
    }

    /// <summary>
    /// Finds a client by its ID
    /// </summary>
    public async Task<Abstractions.Models.Client?> FindClientByIdAsync(string clientId)
    {
        _logger.LogDebug("Finding client: {ClientId}", clientId);
        var client = await _clientStore.FindClientByIdAsync(clientId);
        
        if (client != null)
        {
            client.LastAccessed = DateTime.UtcNow;
        }
        
        return client;
    }

    /// <summary>
    /// Validates if a client is enabled and authorized
    /// </summary>
    public async Task<bool> ValidateClientAsync(Abstractions.Models.Client client)
    {
        if (!client.Enabled)
        {
            _logger.LogWarning("Client is disabled: {ClientId}", client.ClientId);
            return false;
        }

        return await Task.FromResult(true);
    }

    /// <summary>
    /// Validates a client secret using constant-time comparison
    /// </summary>
    public async Task<bool> ValidateSecretAsync(Abstractions.Models.Client client, string secret)
    {
        if (!client.RequireClientSecret)
            return true;

        foreach (var clientSecret in client.ClientSecrets)
        {
            // Check expiration
            if (clientSecret.Expiration.HasValue && clientSecret.Expiration < DateTime.UtcNow)
            {
                _logger.LogDebug("Client secret expired for {ClientId}", client.ClientId);
                continue;
            }

            if (clientSecret.Type == SecretTypes.SharedSecret)
            {
                // 1. Try plaintext constant-time comparison
                if (SecureCompare(clientSecret.Value, secret))
                {
                    _logger.LogDebug("Client secret validated successfully for {ClientId} (plaintext)", client.ClientId);
                    return await Task.FromResult(true);
                }

                // 2. Try PBKDF2 comparison if the stored value is in iterations.salt.hash format
                if (clientSecret.Value.Contains("."))
                {
                    if (VerifyHashedSecret(clientSecret.Value, secret))
                    {
                        _logger.LogDebug("Client secret validated successfully for {ClientId} (PBKDF2)", client.ClientId);
                        return await Task.FromResult(true);
                    }
                }

                // 3. Try legacy SHA256 comparison (no salt)
                var legacyHash = HashSecretLegacy(secret);
                if (SecureCompare(clientSecret.Value, legacyHash))
                {
                    _logger.LogDebug("Client secret validated successfully for {ClientId} (legacy SHA256)", client.ClientId);
                    return await Task.FromResult(true);
                }
            }
        }

        _logger.LogWarning("Secret validation failed for client: {ClientId}", client.ClientId);
        return false;
    }

    /// <summary>
    /// Constant-time string comparison to prevent timing attacks
    /// </summary>
    private bool SecureCompare(string? a, string? b)
    {
        if (a == null || b == null)
            return false;
            
        // To avoid length-based timing attacks, hash both strings first
        // This ensures we're always comparing the same number of bytes (32 for SHA256)
        using var sha256 = SHA256.Create();
        var hashA = sha256.ComputeHash(Encoding.UTF8.GetBytes(a));
        var hashB = sha256.ComputeHash(Encoding.UTF8.GetBytes(b));

        return CryptographicOperations.FixedTimeEquals(hashA, hashB) && a.Length == b.Length;
    }

    private bool VerifyHashedSecret(string storedHash, string secret)
    {
        try
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 3) return false;

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var hash = parts[2];

            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(secret),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                32);

            var newHash = Convert.ToBase64String(hashBytes);
            
            return SecureCompare(hash, newHash);
        }
        catch
        {
            return false;
        }
    }

    private string HashSecretLegacy(string secret)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(secret);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}