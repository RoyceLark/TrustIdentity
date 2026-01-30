using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using TrustIdentity.Abstractions.Configuration;

namespace TrustIdentity.Core.Security;

/// <summary>
/// Manages security keys for signing and encryption
/// </summary>
public class KeyManager
{
    private readonly Dictionary<string, SecurityKey> _keys = new();
    private readonly object _lock = new object();

    /// <summary>
    /// Gets an existing key or creates a new one
    /// </summary>
    /// <param name="keyId">The key ID</param>
    /// <returns>The security key</returns>
    public SecurityKey GetOrCreateKey(string keyId)
    {
        lock (_lock)
        {
            if (_keys.TryGetValue(keyId, out var existingKey))
                return existingKey;

            var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
            _keys[keyId] = key;
            return key;
        }
    }

    /// <summary>
    /// Gets all managed keys
    /// </summary>
    /// <returns>A collection of security keys</returns>
    public IEnumerable<SecurityKey> GetAllKeys()
    {
        lock (_lock)
        {
            return _keys.Values.ToList();
        }
    }

    /// <summary>
    /// Rotates a key by removing the old one and ensuring a new one exists
    /// </summary>
    /// <param name="oldKeyId">The old key ID</param>
    /// <param name="newKeyId">The new key ID</param>
    public void RotateKey(string oldKeyId, string newKeyId)
    {
        lock (_lock)
        {
            _keys.Remove(oldKeyId);
            GetOrCreateKey(newKeyId);
        }
    }
}

/// <summary>
/// Provides signing credentials and validation parameters
/// </summary>
public class SigningCredentialsProvider
{
    private readonly KeyManager _keyManager;
    private readonly TrustIdentityOptions _options;
    private const string DefaultKeyId = "default-signing-key";

    /// <summary>
    /// Initializes a new instance of the SigningCredentialsProvider
    /// </summary>
    /// <param name="keyManager">The key manager</param>
    /// <param name="options">The TrustIdentity options</param>
    public SigningCredentialsProvider(KeyManager keyManager, TrustIdentityOptions options)
    {
        _keyManager = keyManager;
        _options = options;
    }

    /// <summary>
    /// Gets the default signing credentials
    /// </summary>
    /// <returns>The signing credentials</returns>
    public SigningCredentials GetSigningCredentials()
    {
        var key = _keyManager.GetOrCreateKey(DefaultKeyId);
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    /// <summary>
    /// Gets the token validation parameters
    /// </summary>
    /// <returns>The validation parameters</returns>
    public TokenValidationParameters GetValidationParameters()
    {
        var key = _keyManager.GetOrCreateKey(DefaultKeyId);
        
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _options.IssuerUri,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    }
}
