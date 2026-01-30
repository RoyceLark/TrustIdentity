using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Automatic key management and rotation service
/// </summary>
public class KeyManagementService : IKeyManagementService
{
    private readonly ILogger<KeyManagementService> _logger;
    private readonly List<SigningKey> _keys = new();
    private readonly object _lock = new();
    private readonly int _keyRotationDays = 90; // Rotate every 90 days
    private readonly int _keyRetentionDays = 180; // Keep old keys for 180 days

    /// <summary>
    /// Initializes a new instance of KeyManagementService
    /// </summary>
    public KeyManagementService(ILogger<KeyManagementService> logger)
    {
        _logger = logger;
        
        // Initialize with a key if none exist
        if (!_keys.Any())
        {
            Task.Run(async () => await InitializeAsync()).Wait();
        }
    }

    /// <summary>
    /// Initializes the service with a default key
    /// </summary>
    private async Task InitializeAsync()
    {
        lock (_lock)
        {
            if (!_keys.Any())
            {
                var key = GenerateNewKey();
                _keys.Add(key);
                _logger.LogInformation("Initialized key management with key {KeyId}", key.KeyId);
            }
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets the current active signing key
    /// </summary>
    public Task<SigningKey> GetCurrentSigningKeyAsync()
    {
        lock (_lock)
        {
            var activeKey = _keys.FirstOrDefault(k => k.IsActive && k.ActiveFrom <= DateTime.UtcNow && k.ExpiresAt > DateTime.UtcNow);
            
            if (activeKey == null)
            {
                // No active key, create one
                activeKey = GenerateNewKey();
                _keys.Add(activeKey);
                _logger.LogWarning("No active key found, generated new key {KeyId}", activeKey.KeyId);
            }
            
            return Task.FromResult(activeKey);
        }
    }

    /// <summary>
    /// Gets all valid signing keys (for token validation)
    /// </summary>
    public Task<IEnumerable<SigningKey>> GetAllValidKeysAsync()
    {
        lock (_lock)
        {
            var validKeys = _keys.Where(k => k.ExpiresAt > DateTime.UtcNow).ToList();
            return Task.FromResult<IEnumerable<SigningKey>>(validKeys);
        }
    }

    /// <summary>
    /// Rotates the signing key
    /// </summary>
    public Task RotateKeyAsync()
    {
        lock (_lock)
        {
            // Mark current active key as inactive
            var currentKey = _keys.FirstOrDefault(k => k.IsActive);
            if (currentKey != null)
            {
                currentKey.IsActive = false;
                _logger.LogInformation("Deactivated key {KeyId}", currentKey.KeyId);
            }

            // Generate new key
            var newKey = GenerateNewKey();
            _keys.Add(newKey);
            
            _logger.LogInformation("Rotated to new key {KeyId}", newKey.KeyId);

            // Clean up expired keys
            CleanupExpiredKeys();
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the JWKS (JSON Web Key Set) for discovery
    /// </summary>
    public Task<string> GetJwksAsync()
    {
        lock (_lock)
        {
            var validKeys = _keys.Where(k => k.ExpiresAt > DateTime.UtcNow).ToList();
            var jwks = new
            {
                keys = validKeys.Select(k => new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = k.KeyId,
                    alg = k.Algorithm,
                    n = k.RsaKey != null ? Convert.ToBase64String(k.RsaKey.ExportParameters(false).Modulus!) : null,
                    e = k.RsaKey != null ? Convert.ToBase64String(k.RsaKey.ExportParameters(false).Exponent!) : null
                }).ToArray()
            };

            return Task.FromResult(JsonSerializer.Serialize(jwks));
        }
    }

    /// <summary>
    /// Generates a new signing key
    /// </summary>
    private SigningKey GenerateNewKey()
    {
        var rsa = RSA.Create(2048);
        var keyId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;

        return new SigningKey
        {
            KeyId = keyId,
            RsaKey = rsa,
            CreatedAt = now,
            ActiveFrom = now,
            ExpiresAt = now.AddDays(_keyRotationDays + _keyRetentionDays),
            IsActive = true,
            Algorithm = "RS256"
        };
    }

    /// <summary>
    /// Removes expired keys from storage
    /// </summary>
    private void CleanupExpiredKeys()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _keys.Where(k => k.ExpiresAt <= now).ToList();
        
        foreach (var key in expiredKeys)
        {
            _keys.Remove(key);
            key.RsaKey?.Dispose();
            _logger.LogInformation("Removed expired key {KeyId}", key.KeyId);
        }
    }
}
