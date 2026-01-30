using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for managing authorization codes
/// </summary>
public class AuthorizationCodeService : IAuthorizationCodeStore, IAuthorizationCodeService
{
    private readonly IPersistedGrantStore _grantStore;
    private readonly ILogger<AuthorizationCodeService> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthorizationCodeService
    /// </summary>
    /// <param name="grantStore">The persisted grant store</param>
    /// <param name="logger">The logger instance</param>
    public AuthorizationCodeService(
        IPersistedGrantStore grantStore,
        ILogger<AuthorizationCodeService> logger)
    {
        _grantStore = grantStore;
        _logger = logger;
    }

    /// <summary>
    /// Creates and stores an authorization code
    /// </summary>
    /// <param name="code">The authorization code model</param>
    /// <returns>The generated code value</returns>
    public async Task<string> CreateAuthorizationCodeAsync(AuthorizationCode code)
    {
        return await StoreInternalAsync(code);
    }

    /// <summary>
    /// Stores a new authorization code (IAuthorizationCodeStore implementation)
    /// </summary>
    public async Task StoreAuthorizationCodeAsync(AuthorizationCode authCode)
    {
        await StoreInternalAsync(authCode);
    }

    private async Task<string> StoreInternalAsync(AuthorizationCode code)
    {
        var codeValue = GenerateCode();

        var grant = new PersistedGrant
        {
            Key = codeValue,
            Type = "authorization_code",
            SubjectId = code.SubjectId,
            SessionId = string.Empty, // AuthorizationCode model doesn't have SessionId in this version?
            ClientId = code.ClientId,
            CreationTime = code.CreatedAt,
            Expiration = code.ExpiresAt,
            Data = System.Text.Json.JsonSerializer.Serialize(code)
        };

        await _grantStore.StoreAsync(grant);
        
        _logger.LogDebug("Created authorization code for client {ClientId}", code.ClientId);
        return codeValue;
    }

    /// <summary>
    /// Retrieves an authorization code from the store
    /// </summary>
    /// <param name="code">The code value</param>
    /// <returns>The authorization code model if found and valid; otherwise null</returns>
    public async Task<AuthorizationCode?> GetAuthorizationCodeAsync(string code)
    {
        var grant = await _grantStore.GetAsync(code);
        
        if (grant == null)
        {
            _logger.LogWarning("Authorization code not found");
            return null;
        }

        if (grant.Expiration.HasValue && grant.Expiration < DateTime.UtcNow)
        {
            _logger.LogWarning("Authorization code expired");
            await _grantStore.RemoveAsync(code);
            return null;
        }

        var authCode = System.Text.Json.JsonSerializer.Deserialize<AuthorizationCode>(grant.Data);
        return authCode;
    }

    /// <summary>
    /// Consumes (removes) an authorization code from the store
    /// </summary>
    /// <param name="code">The code value to consume</param>
    /// <returns>A task representing the operation</returns>
    public async Task ConsumeAuthorizationCodeAsync(string code)
    {
        await _grantStore.RemoveAsync(code);
        _logger.LogDebug("Authorization code consumed and removed");
    }

    /// <summary>
    /// Removes an authorization code (IAuthorizationCodeStore implementation)
    /// </summary>
    public async Task RemoveAuthorizationCodeAsync(string code)
    {
        await ConsumeAuthorizationCodeAsync(code);
    }

    private string GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}