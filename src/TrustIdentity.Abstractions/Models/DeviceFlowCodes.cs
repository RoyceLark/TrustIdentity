namespace TrustIdentity.Abstractions.Models;

using System;
/// <summary>
/// Represents device flow codes
/// </summary>
public class DeviceFlowCodes
{
    /// <summary>The device code</summary>
    public string DeviceCode { get; set; } = string.Empty;
    /// <summary>The user code</summary>
    public string UserCode { get; set; } = string.Empty;
    /// <summary>The client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The subject ID of the user</summary>
    public string? SubjectId { get; set; }
    /// <summary>Creation time</summary>
    public DateTime CreationTime { get; set; }
    /// <summary>Expiration time</summary>
    public DateTime Expiration { get; set; }
    /// <summary>Whether the device code is authorized</summary>
    public bool IsAuthorized { get; set; }
    /// <summary>Serialized data associated with the device flow</summary>
    public string Data { get; set; } = string.Empty;
}