namespace TrustIdentity.Core.Errors;

using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// Comprehensive OAuth 2.0 Error Handling
/// </summary>
public class OAuthError
{
    /// <summary>The error code</summary>
    public string Error { get; set; } = string.Empty;
    /// <summary>The error description</summary>
    public string? ErrorDescription { get; set; }
    /// <summary>The error URI</summary>
    public string? ErrorUri { get; set; }
    /// <summary>The state value</summary>
    public string? State { get; set; }

    /// <summary>Creates an invalid_request error</summary>
    public static OAuthError InvalidRequest(string description) => new()
    {
        Error = "invalid_request",
        ErrorDescription = description
    };

    /// <summary>Creates an invalid_client error</summary>
    public static OAuthError InvalidClient(string description) => new()
    {
        Error = "invalid_client",
        ErrorDescription = description
    };

    /// <summary>Creates an invalid_grant error</summary>
    public static OAuthError InvalidGrant(string description) => new()
    {
        Error = "invalid_grant",
        ErrorDescription = description
    };

    /// <summary>Creates an unauthorized_client error</summary>
    public static OAuthError UnauthorizedClient(string description) => new()
    {
        Error = "unauthorized_client",
        ErrorDescription = description
    };

    /// <summary>Creates an unsupported_grant_type error</summary>
    public static OAuthError UnsupportedGrantType(string description) => new()
    {
        Error = "unsupported_grant_type",
        ErrorDescription = description
    };

    /// <summary>Creates an invalid_scope error</summary>
    public static OAuthError InvalidScope(string description) => new()
    {
        Error = "invalid_scope",
        ErrorDescription = description
    };

    /// <summary>Creates a server_error error</summary>
    public static OAuthError ServerError(string description) => new()
    {
        Error = "server_error",
        ErrorDescription = description
    };

    /// <summary>Creates a temporarily_unavailable error</summary>
    public static OAuthError TemporarilyUnavailable(string description) => new()
    {
        Error = "temporarily_unavailable",
        ErrorDescription = description
    };
}

/// <summary>Base exception for OAuth 2.0 errors</summary>
public class OAuthException : Exception
{
    /// <summary>The associated OAuth error</summary>
    public OAuthError Error { get; }

    /// <summary>Initializes a new instance of the OAuthException</summary>
    public OAuthException(OAuthError error) : base(error.ErrorDescription ?? error.Error)
    {
        Error = error;
    }

    /// <summary>Initializes a new instance of the OAuthException with inner exception</summary>
    public OAuthException(OAuthError error, Exception inner) : base(error.ErrorDescription ?? error.Error, inner)
    {
        Error = error;
    }
}

/// <summary>Exception thrown when an invalid grant is provided</summary>
public class InvalidGrantException : OAuthException
{
    /// <summary>Initializes a new instance of the InvalidGrantException</summary>
    public InvalidGrantException(string description) 
        : base(OAuthError.InvalidGrant(description)) { }
}

/// <summary>Exception thrown when an invalid client is detected</summary>
public class InvalidClientException : OAuthException
{
    /// <summary>Initializes a new instance of the InvalidClientException</summary>
    public InvalidClientException(string description) 
        : base(OAuthError.InvalidClient(description)) { }
}

/// <summary>Exception thrown when a grant type is not supported</summary>
public class UnsupportedGrantTypeException : OAuthException
{
    /// <summary>Initializes a new instance of the UnsupportedGrantTypeException</summary>
    public UnsupportedGrantTypeException(string description) 
        : base(OAuthError.UnsupportedGrantType(description)) { }
}