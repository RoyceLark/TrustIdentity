using System;

namespace TrustIdentity.Saml.Errors;

/// <summary>
/// Comprehensive SAML Error Handling
/// </summary>
public class SamlError
{
    /// <summary>SAML Status Code URI</summary>
    public string StatusCode { get; set; } = string.Empty;
    
    /// <summary>Human-readable status message</summary>
    public string? StatusMessage { get; set; }
    
    /// <summary>Detailed error information</summary>
    public string? StatusDetail { get; set; }
    
    /// <summary>Line number where error occurred (for XML parsing)</summary>
    public int? LineNumber { get; set; }
    
    /// <summary>XML element name where error occurred</summary>
    public string? ElementName { get; set; }

    /// <summary>Creates a success status</summary>
    public static SamlError Success() => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:Success"
    };

    /// <summary>Creates an authentication failure error</summary>
    public static SamlError AuthnFailed(string message) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:AuthnFailed",
        StatusMessage = message
    };

    /// <summary>Creates an invalid NameID policy error</summary>
    public static SamlError InvalidNameIDPolicy(string message) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:InvalidNameIDPolicy",
        StatusMessage = message
    };

    /// <summary>Creates a request denied error</summary>
    public static SamlError RequestDenied(string message) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:RequestDenied",
        StatusMessage = message
    };

    /// <summary>Creates an invalid signature error</summary>
    public static SamlError InvalidSignature(string detail) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:Requester",
        StatusMessage = "Invalid XML signature",
        StatusDetail = detail
    };

    /// <summary>Creates an expired assertion error</summary>
    public static SamlError ExpiredAssertion(DateTime notOnOrAfter) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:Requester",
        StatusMessage = "Assertion has expired",
        StatusDetail = $"Assertion expired at {notOnOrAfter:o}"
    };

    /// <summary>Creates an invalid audience error</summary>
    public static SamlError InvalidAudience(string expected, string actual) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:Requester",
        StatusMessage = "Invalid audience restriction",
        StatusDetail = $"Expected: {expected}, Actual: {actual}"
    };

    /// <summary>Creates a replay detected error</summary>
    public static SamlError ReplayDetected(string assertionId) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:Requester",
        StatusMessage = "Replay attack detected",
        StatusDetail = $"Assertion ID {assertionId} has already been processed"
    };

    /// <summary>Creates an XML parsing error</summary>
    public static SamlError XmlParsingError(string message, int? lineNumber = null, string? elementName = null) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:Requester",
        StatusMessage = "XML parsing error",
        StatusDetail = message,
        LineNumber = lineNumber,
        ElementName = elementName
    };

    /// <summary>Creates a certificate validation error</summary>
    public static SamlError CertificateValidationFailed(string reason) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:Requester",
        StatusMessage = "Certificate validation failed",
        StatusDetail = reason
    };

    /// <summary>Creates a decryption failure error</summary>
    public static SamlError DecryptionFailed(string reason) => new()
    {
        StatusCode = "urn:oasis:names:tc:SAML:2.0:status:Responder",
        StatusMessage = "Assertion decryption failed",
        StatusDetail = reason
    };
}

/// <summary>
/// Exception representing a SAML error
/// </summary>
public class SamlException : Exception
{
    /// <summary>The associated SAML error details</summary>
    public SamlError Error { get; }

    /// <summary>Initializes a new instance of SamlException</summary>
    public SamlException(SamlError error) : base(error.StatusMessage ?? error.StatusCode)
    {
        Error = error;
    }

    /// <summary>Initializes a new instance of SamlException with inner exception</summary>
    public SamlException(SamlError error, Exception inner) : base(error.StatusMessage ?? error.StatusCode, inner)
    {
        Error = error;
    }
}

/// <summary>
/// Exception thrown when SAML validation fails
/// </summary>
public class SamlValidationException : SamlException
{
    /// <summary>Initializes a new instance of SamlValidationException</summary>
    public SamlValidationException(SamlError error) : base(error) { }
    
    /// <summary>Initializes a new instance of SamlValidationException with inner exception</summary>
    public SamlValidationException(SamlError error, Exception inner) : base(error, inner) { }
}

/// <summary>
/// Exception thrown when signature validation fails
/// </summary>
public class SamlSignatureException : SamlException
{
    /// <summary>Initializes a new instance of SamlSignatureException</summary>
    public SamlSignatureException(string detail) : base(SamlError.InvalidSignature(detail)) { }
}

/// <summary>
/// Exception thrown when a replay attack is detected
/// </summary>
public class SamlReplayException : SamlException
{
    /// <summary>Initializes a new instance of SamlReplayException</summary>
    public SamlReplayException(string assertionId) : base(SamlError.ReplayDetected(assertionId)) { }
}