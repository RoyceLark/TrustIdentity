using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Stores;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for managing OAuth 2.0 Device Authorization Grant flow
/// </summary>
public class DeviceFlowService : IDeviceFlowService
{
    private readonly IDeviceFlowStore _deviceFlowStore;
    private readonly ILogger<DeviceFlowService> _logger;

    /// <inheritdoc/>
    public async Task<DeviceFlowValidationResult> ValidateUserCodeAsync(string userCode)
    {
        var codes = await FindByUserCodeAsync(userCode);
        if (codes == null)
        {
            return new DeviceFlowValidationResult { Success = false, Error = "Invalid user code" };
        }

        // Generate a return URL that includes the user code for the consent process
        // In a real app, this would be more complex and signed/encrypted
        var consentReturnUrl = $"/connect/authorize?client_id={codes.ClientId}&user_code={userCode}";

        return new DeviceFlowValidationResult
        {
            Success = true,
            ConsentReturnUrl = consentReturnUrl
        };
    }

    /// <summary>
    /// Initializes a new instance of the DeviceFlowService
    /// </summary>
    /// <param name="deviceFlowStore">The device flow store</param>
    /// <param name="logger">The logger instance</param>
    public DeviceFlowService(
        IDeviceFlowStore deviceFlowStore,
        ILogger<DeviceFlowService> logger)
    {
        _deviceFlowStore = deviceFlowStore;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new device authorization request
    /// </summary>
    public async Task<DeviceFlowCodes> CreateDeviceAuthorizationAsync(string clientId, List<string> scopes)
    {
        var deviceCode = GenerateDeviceCode();
        var userCode = GenerateUserCode();
        
        var codes = new DeviceFlowCodes
        {
            DeviceCode = deviceCode,
            UserCode = userCode,
            ClientId = clientId,
            CreationTime = DateTime.UtcNow,
            Expiration = DateTime.UtcNow.AddMinutes(5),
            Data = System.Text.Json.JsonSerializer.Serialize(new { Scopes = scopes })
        };

        await _deviceFlowStore.StoreDeviceAuthorizationAsync(deviceCode, userCode, codes);
        
        _logger.LogDebug("Created device flow codes for client {ClientId}", clientId);
        return codes;
    }

    /// <summary>
    /// Finds device flow codes by user code
    /// </summary>
    public async Task<DeviceFlowCodes?> FindByUserCodeAsync(string userCode)
    {
        var codes = await _deviceFlowStore.FindByUserCodeAsync(userCode);
        
        if (codes != null && codes.Expiration < DateTime.UtcNow)
        {
            await _deviceFlowStore.RemoveByDeviceCodeAsync(codes.DeviceCode);
            return null;
        }

        return codes;
    }

    /// <summary>
    /// Finds device flow codes by device code
    /// </summary>
    public async Task<DeviceFlowCodes?> FindByDeviceCodeAsync(string deviceCode)
    {
        var codes = await _deviceFlowStore.FindByDeviceCodeAsync(deviceCode);
        
        if (codes != null && codes.Expiration < DateTime.UtcNow)
        {
            await _deviceFlowStore.RemoveByDeviceCodeAsync(deviceCode);
            return null;
        }

        return codes;
    }

    /// <summary>
    /// Updates device flow codes by user code
    /// </summary>
    public async Task UpdateByUserCodeAsync(string userCode, DeviceFlowCodes codes)
    {
        await _deviceFlowStore.UpdateByUserCodeAsync(userCode, codes);
    }

    /// <summary>
    /// Removes device flow codes by device code
    /// </summary>
    public async Task RemoveByDeviceCodeAsync(string deviceCode)
    {
        await _deviceFlowStore.RemoveByDeviceCodeAsync(deviceCode);
    }

    private string GenerateDeviceCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private string GenerateUserCode()
    {
        return $"{RandomNumberGenerator.GetInt32(0, 10000):D4}-{RandomNumberGenerator.GetInt32(0, 10000):D4}";
    }
}