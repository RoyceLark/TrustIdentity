using System;

namespace TrustIdentity.WsFederation.Errors;

/// <summary>
/// WS-Federation and WS-Trust Error Handling
/// </summary>
public class WsFederationError
{
    /// <summary>The fault code</summary>
    public string FaultCode { get; set; } = string.Empty;
    /// <summary>The fault string</summary>
    public string? FaultString { get; set; }
    /// <summary>The fault detail</summary>
    public string? FaultDetail { get; set; }

    /// <summary>Creates an invalid request error</summary>
    /// <param name="message">The message</param>
    /// <returns>A WS-Federation error</returns>
    public static WsFederationError InvalidRequest(string message) => new()
    {
        FaultCode = "wst:InvalidRequest",
        FaultString = message
    };

    /// <summary>Creates a failed authentication error</summary>
    /// <param name="message">The message</param>
    /// <returns>A WS-Federation error</returns>
    public static WsFederationError FailedAuthentication(string message) => new()
    {
        FaultCode = "wst:FailedAuthentication",
        FaultString = message
    };

    /// <summary>Creates an invalid security token error</summary>
    /// <param name="message">The message</param>
    /// <returns>A WS-Federation error</returns>
    public static WsFederationError InvalidSecurity(string message) => new()
    {
        FaultCode = "wst:InvalidSecurityToken",
        FaultString = message
    };

    /// <summary>Creates a request failed error</summary>
    /// <param name="message">The message</param>
    /// <returns>A WS-Federation error</returns>
    public static WsFederationError RequestFailed(string message) => new()
    {
        FaultCode = "wst:RequestFailed",
        FaultString = message
    };

    /// <summary>Creates an unauthorized realm error</summary>
    /// <param name="realm">The realm</param>
    /// <returns>A WS-Federation error</returns>
    public static WsFederationError UnauthorizedRealm(string realm) => new()
    {
        FaultCode = "wst:InvalidRequest",
        FaultString = "Realm not authorized",
        FaultDetail = $"The realm '{realm}' is not in the list of authorized realms"
    };

    /// <summary>Creates a token issuance failed error</summary>
    /// <param name="reason">The reason</param>
    /// <returns>A WS-Federation error</returns>
    public static WsFederationError TokenIssuanceFailed(string reason) => new()
    {
        FaultCode = "wst:RequestFailed",
        FaultString = "Token issuance failed",
        FaultDetail = reason
    };

    /// <summary>Creates a signing failed error</summary>
    /// <param name="reason">The reason</param>
    /// <returns>A WS-Federation error</returns>
    public static WsFederationError SigningFailed(string reason) => new()
    {
        FaultCode = "wst:RequestFailed",
        FaultString = "Token signing failed",
        FaultDetail = reason
    };
}

/// <summary>
/// Exception representing a WS-Federation error
/// </summary>
public class WsFederationException : Exception
{
    /// <summary>The underlaying WS-Federation error</summary>
    public WsFederationError Error { get; }

    /// <summary>Initializes a new instance of the WsFederationException</summary>
    /// <param name="error">The error</param>
    public WsFederationException(WsFederationError error) : base(error.FaultString ?? error.FaultCode)
    {
        Error = error;
    }

    /// <summary>Initializes a new instance of the WsFederationException with inner exception</summary>
    /// <param name="error">The error</param>
    /// <param name="inner">The inner exception</param>
    public WsFederationException(WsFederationError error, Exception inner) 
        : base(error.FaultString ?? error.FaultCode, inner)
    {
        Error = error;
    }
}