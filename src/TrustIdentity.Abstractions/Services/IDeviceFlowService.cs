using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for managing OAuth 2.0 Device Authorization Grant flow
/// </summary>
public interface IDeviceFlowService
{
    /// <summary>
    /// Creates a new device authorization request
    /// </summary>
    Task<DeviceFlowCodes> CreateDeviceAuthorizationAsync(string clientId, System.Collections.Generic.List<string> scopes);

    /// <summary>
    /// Finds device flow codes by user code
    /// </summary>
    Task<DeviceFlowCodes?> FindByUserCodeAsync(string userCode);

    /// <summary>
    /// Finds device flow codes by device code
    /// </summary>
    Task<DeviceFlowCodes?> FindByDeviceCodeAsync(string deviceCode);

    /// <summary>
    /// Updates device flow codes by user code
    /// </summary>
    Task UpdateByUserCodeAsync(string userCode, DeviceFlowCodes codes);

    /// <summary>
    /// Removes device flow codes by device code
    /// </summary>
    Task RemoveByDeviceCodeAsync(string deviceCode);

    /// <summary>
    /// Validates a user code and returns the result
    /// </summary>
    Task<DeviceFlowValidationResult> ValidateUserCodeAsync(string userCode);
}

/// <summary>
/// Result of device flow user code validation
/// </summary>
public class DeviceFlowValidationResult
{
    /// <summary>Whether the validation was successful</summary>
    public bool Success { get; set; }
    
    /// <summary>The return URL for the consent page</summary>
    public string? ConsentReturnUrl { get; set; }
    
    /// <summary>Error message if validation failed</summary>
    public string? Error { get; set; }
}
