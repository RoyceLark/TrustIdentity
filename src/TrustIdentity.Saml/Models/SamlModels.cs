using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Xml;

namespace TrustIdentity.Saml.Models;

/// <summary>
/// SAML 2.0 Assertion (Security assertion provided by IdP)
/// </summary>
public class SamlAssertion
{
    /// <summary>
    /// Unique identifier for the assertion
    /// </summary>
    public string Id { get; set; } = $"_{Guid.NewGuid():N}";
    
    /// <summary>
    /// Time when the assertion was issued
    /// </summary>
    public DateTime IssueInstant { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Entity ID of the issuer (Identity Provider)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// The subject of the assertion (the user)
    /// </summary>
    public SamlSubject Subject { get; set; } = new();
    
    /// <summary>
    /// Conditions under which the assertion is valid
    /// </summary>
    public SamlConditions Conditions { get; set; } = new();
    
    /// <summary>
    /// List of attribute statements (user claims)
    /// </summary>
    public List<SamlAttributeStatement> AttributeStatements { get; set; } = new();
    
    /// <summary>
    /// List of authentication statements
    /// </summary>
    public List<SamlAuthnStatement> AuthnStatements { get; set; } = new();
    
    /// <summary>
    /// SAML version (typically 2.0)
    /// </summary>
    public string Version { get; set; } = "2.0";
}

/// <summary>
/// SAML Subject (Represents the authenticated principal)
/// </summary>
public class SamlSubject
{
    /// <summary>
    /// Name Identifier (e.g. username, email)
    /// </summary>
    public string NameId { get; set; } = string.Empty;
    
    /// <summary>
    /// Format of the NameID
    /// </summary>
    public string NameIdFormat { get; set; } = SamlConstants.NameIdFormats.EmailAddress;
    
    /// <summary>
    /// Confirmation method for the subject
    /// </summary>
    public SamlSubjectConfirmation? SubjectConfirmation { get; set; }
}

/// <summary>
/// SAML Subject Confirmation details
/// </summary>
public class SamlSubjectConfirmation
{
    /// <summary>
    /// Confirmation method (e.g. Bearer)
    /// </summary>
    public string Method { get; set; } = SamlConstants.SubjectConfirmationMethods.Bearer;
    
    /// <summary>
    /// Additional confirmation data
    /// </summary>
    public SamlSubjectConfirmationData? SubjectConfirmationData { get; set; }
}

/// <summary>
/// SAML Subject Confirmation Data
/// </summary>
public class SamlSubjectConfirmationData
{
    /// <summary>
    /// Time before which the confirmation is not valid
    /// </summary>
    public DateTime? NotBefore { get; set; }
    
    /// <summary>
    /// Time after which the confirmation is not valid
    /// </summary>
    public DateTime NotOnOrAfter { get; set; }
    
    /// <summary>
    /// Intended recipient of the confirmation
    /// </summary>
    public string? Recipient { get; set; }
    
    /// <summary>
    /// ID of the request this confirmation is in response to
    /// </summary>
    public string? InResponseTo { get; set; }
}

/// <summary>
/// SAML Conditions for assertion validity
/// </summary>
public class SamlConditions
{
    /// <summary>
    /// Time before which the assertion is invalid
    /// </summary>
    public DateTime NotBefore { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Time after which the assertion is invalid
    /// </summary>
    public DateTime NotOnOrAfter { get; set; } = DateTime.UtcNow.AddMinutes(5);
    
    /// <summary>
    /// Restrictions on the audience for the assertion
    /// </summary>
    public List<SamlAudienceRestriction> AudienceRestrictions { get; set; } = new();
}

/// <summary>
/// SAML Audience Restriction
/// </summary>
public class SamlAudienceRestriction
{
    /// <summary>
    /// List of allowed audiences (Entity IDs)
    /// </summary>
    public List<string> Audiences { get; set; } = new();
}

/// <summary>
/// SAML Attribute Statement (Container for attributes)
/// </summary>
public class SamlAttributeStatement
{
    /// <summary>
    /// List of attributes
    /// </summary>
    public List<SamlAttribute> Attributes { get; set; } = new();
}

/// <summary>
/// SAML Attribute (A user claim)
/// </summary>
public class SamlAttribute
{
    /// <summary>
    /// Name of the attribute
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Format of the attribute name
    /// </summary>
    public string? NameFormat { get; set; }
    
    /// <summary>
    /// Friendly name for display
    /// </summary>
    public string? FriendlyName { get; set; }
    
    /// <summary>
    /// Values associated with the attribute
    /// </summary>
    public List<string> AttributeValues { get; set; } = new();
}

/// <summary>
/// SAML Authentication Statement (Details about the authentication event)
/// </summary>
public class SamlAuthnStatement
{
    /// <summary>
    /// Time when authentication occurred
    /// </summary>
    public DateTime AuthnInstant { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Session index associated with the authentication
    /// </summary>
    public string? SessionIndex { get; set; }
    
    /// <summary>
    /// Expiration time of the session
    /// </summary>
    public DateTime? SessionNotOnOrAfter { get; set; }
    
    /// <summary>
    /// Context of the authentication (mechanism used)
    /// </summary>
    public SamlAuthnContext AuthnContext { get; set; } = new();
}

/// <summary>
/// SAML Authentication Context
/// </summary>
public class SamlAuthnContext
{
    /// <summary>
    /// Reference to the authentication context class (e.g. PasswordProtectedTransport)
    /// </summary>
    public string AuthnContextClassRef { get; set; } = SamlConstants.AuthnContextClasses.PasswordProtectedTransport;
}

/// <summary>
/// SAML Response message
/// </summary>
public class SamlResponse
{
    /// <summary>
    /// Unique identifier for the response
    /// </summary>
    public string Id { get; set; } = $"_{Guid.NewGuid():N}";
    
    /// <summary>
    /// Time when the response was issued
    /// </summary>
    public DateTime IssueInstant { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// SAML version
    /// </summary>
    public string Version { get; set; } = "2.0";
    
    /// <summary>
    /// Issuer of the response
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Status of the response
    /// </summary>
    public SamlStatus Status { get; set; } = new();
    
    /// <summary>
    /// List of assertions
    /// </summary>
    public List<SamlAssertion> Assertions { get; set; } = new();
    
    /// <summary>
    /// ID of the request this is responding to
    /// </summary>
    public string? InResponseTo { get; set; }
    
    /// <summary>
    /// Destination URI
    /// </summary>
    public string? Destination { get; set; }
}

/// <summary>
/// SAML Status information
/// </summary>
public class SamlStatus
{
    /// <summary>
    /// Status code (e.g. Success)
    /// </summary>
    public string StatusCode { get; set; } = SamlConstants.StatusCodes.Success;
    
    /// <summary>
    /// Optional status message
    /// </summary>
    public string? StatusMessage { get; set; }
    
    /// <summary>
    /// Optional detail about status
    /// </summary>
    public string? StatusDetail { get; set; }
}

/// <summary>
/// SAML Authentication Request message
/// </summary>
public class SamlAuthnRequest
{
    /// <summary>
    /// Unique identifier for the request
    /// </summary>
    public string Id { get; set; } = $"_{Guid.NewGuid():N}";
    
    /// <summary>
    /// Time when the request was issued
    /// </summary>
    public DateTime IssueInstant { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// SAML version
    /// </summary>
    public string Version { get; set; } = "2.0";
    
    /// <summary>
    /// Issuer of the request
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// URL where the assertion should be sent
    /// </summary>
    public string? AssertionConsumerServiceURL { get; set; }
    
    /// <summary>
    /// Protocol binding to use (typically HTTP-POST)
    /// </summary>
    public string? ProtocolBinding { get; set; } = SamlConstants.ProtocolBindings.HttpPost;
    
    /// <summary>
    /// Policy for NameID generation
    /// </summary>
    public SamlNameIdPolicy? NameIdPolicy { get; set; }
    
    /// <summary>
    /// Whether to force authentication
    /// </summary>
    public bool ForceAuthn { get; set; }
    
    /// <summary>
    /// Whether the request should be passive (no user interaction)
    /// </summary>
    public bool IsPassive { get; set; }
}

/// <summary>
/// SAML NameID Policy
/// </summary>
public class SamlNameIdPolicy
{
    /// <summary>
    /// Requested NameID format
    /// </summary>
    public string Format { get; set; } = SamlConstants.NameIdFormats.EmailAddress;
    
    /// <summary>
    /// Whether to allow creation of new NameIDs
    /// </summary>
    public bool AllowCreate { get; set; } = true;
}

/// <summary>
/// SAML Metadata - Service Provider
/// </summary>
public class SamlServiceProviderMetadata
{
    /// <summary>
    /// Entity ID of the SP
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Assertion Consumer Services
    /// </summary>
    public List<SamlAssertionConsumerService> AssertionConsumerServices { get; set; } = new();
    
    /// <summary>
    /// Single Logout Services
    /// </summary>
    public List<SamlSingleLogoutService> SingleLogoutServices { get; set; } = new();
    
    /// <summary>
    /// Key descriptors (certificates)
    /// </summary>
    public List<SamlKeyDescriptor> KeyDescriptors { get; set; } = new();
    
    /// <summary>
    /// Organization details
    /// </summary>
    public SamlOrganization? Organization { get; set; }
    
    /// <summary>
    /// Contact persons
    /// </summary>
    public List<SamlContactPerson> ContactPersons { get; set; } = new();
}

/// <summary>
/// SAML Metadata - Identity Provider
/// </summary>
public class SamlIdentityProviderMetadata
{
    /// <summary>
    /// Entity ID of the IdP
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Single Sign-On Services
    /// </summary>
    public List<SamlSingleSignOnService> SingleSignOnServices { get; set; } = new();
    
    /// <summary>
    /// Single Logout Services
    /// </summary>
    public List<SamlSingleLogoutService> SingleLogoutServices { get; set; } = new();
    
    /// <summary>
    /// Key descriptors (certificates)
    /// </summary>
    public List<SamlKeyDescriptor> KeyDescriptors { get; set; } = new();
    
    /// <summary>
    /// Supported attributes
    /// </summary>
    public List<SamlAttribute> Attributes { get; set; } = new();
    
    /// <summary>
    /// Organization details
    /// </summary>
    public SamlOrganization? Organization { get; set; }
    
    /// <summary>
    /// Contact persons
    /// </summary>
    public List<SamlContactPerson> ContactPersons { get; set; } = new();
}

/// <summary>
/// SAML Assertion Consumer Service Endpoint
/// </summary>
public class SamlAssertionConsumerService
{
    /// <summary>
    /// Protocol binding
    /// </summary>
    public string Binding { get; set; } = SamlConstants.ProtocolBindings.HttpPost;
    
    /// <summary>
    /// Location URL
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Endpoint index
    /// </summary>
    public int Index { get; set; }
    
    /// <summary>
    /// Is default endpoint
    /// </summary>
    public bool IsDefault { get; set; }
}

/// <summary>
/// SAML Single Sign-On Service Endpoint
/// </summary>
public class SamlSingleSignOnService
{
    /// <summary>
    /// Protocol binding
    /// </summary>
    public string Binding { get; set; } = SamlConstants.ProtocolBindings.HttpPost;
    
    /// <summary>
    /// Location URL
    /// </summary>
    public string Location { get; set; } = string.Empty;
}

/// <summary>
/// SAML Single Logout Service Endpoint
/// </summary>
public class SamlSingleLogoutService
{
    /// <summary>
    /// Protocol binding
    /// </summary>
    public string Binding { get; set; } = SamlConstants.ProtocolBindings.HttpPost;
    
    /// <summary>
    /// Location URL
    /// </summary>
    public string Location { get; set; } = string.Empty;
}

/// <summary>
/// SAML Key Descriptor (Certificate info)
/// </summary>
public class SamlKeyDescriptor
{
    /// <summary>
    /// Usage (signing or encryption)
    /// </summary>
    public string Use { get; set; } = "signing"; // signing or encryption
    
    /// <summary>
    /// X.509 Certificate data
    /// </summary>
    public string X509Certificate { get; set; } = string.Empty;
}

/// <summary>
/// SAML Organization Info
/// </summary>
public class SamlOrganization
{
    /// <summary>
    /// Organization Name
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Display Name
    /// </summary>
    public string OrganizationDisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization URL
    /// </summary>
    public string OrganizationURL { get; set; } = string.Empty;
}

/// <summary>
/// SAML Contact Person Info
/// </summary>
public class SamlContactPerson
{
    /// <summary>
    /// Type of contact (technical, support, etc.)
    /// </summary>
    public string ContactType { get; set; } = "technical"; // technical, support, administrative, billing, other
    
    /// <summary>
    /// Given Name
    /// </summary>
    public string? GivenName { get; set; }
    
    /// <summary>
    /// Surname
    /// </summary>
    public string? SurName { get; set; }
    
    /// <summary>
    /// Email Address
    /// </summary>
    public string? EmailAddress { get; set; }
    
    /// <summary>
    /// Telephone Number
    /// </summary>
    public string? TelephoneNumber { get; set; }
}

/// <summary>
/// SAML Logout Request message
/// </summary>
public class SamlLogoutRequest
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = $"_{Guid.NewGuid():N}";
    
    /// <summary>
    /// Issue instant
    /// </summary>
    public DateTime IssueInstant { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Version
    /// </summary>
    public string Version { get; set; } = "2.0";
    
    /// <summary>
    /// Issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// NameID of the subject to logout
    /// </summary>
    public SamlNameId NameId { get; set; } = new();
    
    /// <summary>
    /// Session Index to terminate
    /// </summary>
    public string? SessionIndex { get; set; }
}

/// <summary>
/// SAML Logout Response message
/// </summary>
public class SamlLogoutResponse
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = $"_{Guid.NewGuid():N}";
    
    /// <summary>
    /// Issue instant
    /// </summary>
    public DateTime IssueInstant { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Version
    /// </summary>
    public string Version { get; set; } = "2.0";
    
    /// <summary>
    /// Issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Status of the logout
    /// </summary>
    public SamlStatus Status { get; set; } = new();
    
    /// <summary>
    /// Request ID this is responding to
    /// </summary>
    public string? InResponseTo { get; set; }
}

/// <summary>
/// SAML NameID value and format
/// </summary>
public class SamlNameId
{
    /// <summary>
    /// The NameID value
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// The NameID Format URI
    /// </summary>
    public string Format { get; set; } = SamlConstants.NameIdFormats.EmailAddress;
}