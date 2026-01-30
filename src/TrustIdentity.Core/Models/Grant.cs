namespace TrustIdentity.Core.Models;

using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
//public class PersistedGrant
//{
//    public string Key { get; set; } = string.Empty;
//    public string Type { get; set; } = string.Empty;
//    public string SubjectId { get; set; } = string.Empty;
//    public string? SessionId { get; set; }
//    public string ClientId { get; set; } = string.Empty;
//    public string? Description { get; set; }
//    public DateTime CreationTime { get; set; }
//    public DateTime? Expiration { get; set; }
//    public DateTime? ConsumedTime { get; set; }
//    public string Data { get; set; } = string.Empty;
//}

/// <summary>
/// Represents codes used for device flow authorization
/// </summary>
public class DeviceFlowCodes
{
    /// <summary>The device code</summary>
    public string DeviceCode { get; set; } = string.Empty;
    /// <summary>The user code</summary>
    public string UserCode { get; set; } = string.Empty;
    /// <summary>The subject ID</summary>
    public string SubjectId { get; set; } = string.Empty;
    /// <summary>The session ID</summary>
    public string? SessionId { get; set; }
    /// <summary>The client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The description</summary>
    public string? Description { get; set; }
    /// <summary>When the codes were created</summary>
    public DateTime CreationTime { get; set; }
    /// <summary>When the codes expire</summary>
    public DateTime Expiration { get; set; }
    /// <summary>Additional data associated with the request (e.g. scopes)</summary>
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Represents a pushed authorization request (PAR)
/// </summary>
public class PushedAuthorizationRequest
{
    /// <summary>The reference value (request_uri)</summary>
    public string ReferenceValue { get; set; } = string.Empty;
    /// <summary>When the request expires</summary>
    public DateTime ExpiresAtUtc { get; set; }
    /// <summary>The serialized request parameters</summary>
    public string Parameters { get; set; } = string.Empty;
}
