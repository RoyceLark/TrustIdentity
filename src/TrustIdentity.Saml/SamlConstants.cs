namespace TrustIdentity.Saml;

/// <summary>
/// SAML 2.0 Constants
/// </summary>
public static class SamlConstants
{
    /// <summary>
    /// SAML 2.0 Assertion Namespace
    /// </summary>
    public const string Saml2Namespace = "urn:oasis:names:tc:SAML:2.0:assertion";
    
    /// <summary>
    /// SAML 2.0 Protocol Namespace
    /// </summary>
    public const string Saml2ProtocolNamespace = "urn:oasis:names:tc:SAML:2.0:protocol";
    
    /// <summary>
    /// SAML 2.0 Metadata Namespace
    /// </summary>
    public const string Saml2MetadataNamespace = "urn:oasis:names:tc:SAML:2.0:metadata";
    
    /// <summary>
    /// XML Digital Signature Namespace
    /// </summary>
    public const string XmlDsigNamespace = "http://www.w3.org/2000/09/xmldsig#";
    
    /// <summary>
    /// XML Encryption Namespace
    /// </summary>
    public const string XmlEncNamespace = "http://www.w3.org/2001/04/xmlenc#";

    /// <summary>
    /// SAML Protocol Bindings
    /// </summary>
    public static class ProtocolBindings
    {
        /// <summary>HTTP POST Binding</summary>
        public const string HttpPost = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";
        /// <summary>HTTP Redirect Binding</summary>
        public const string HttpRedirect = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect";
        /// <summary>HTTP Artifact Binding</summary>
        public const string HttpArtifact = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Artifact";
        /// <summary>SOAP Binding</summary>
        public const string Soap = "urn:oasis:names:tc:SAML:2.0:bindings:SOAP";
    }

    /// <summary>
    /// SAML NameID Formats
    /// </summary>
    public static class NameIdFormats
    {
        /// <summary>Unspecified format</summary>
        public const string Unspecified = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";
        /// <summary>Email Address format</summary>
        public const string EmailAddress = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
        /// <summary>X509 Subject Name format</summary>
        public const string X509SubjectName = "urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName";
        /// <summary>Windows Domain Qualified Name format</summary>
        public const string WindowsDomainQualifiedName = "urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName";
        /// <summary>Persistent ID format</summary>
        public const string Persistent = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent";
        /// <summary>Transient ID format</summary>
        public const string Transient = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient";
        /// <summary>Entity ID format</summary>
        public const string Entity = "urn:oasis:names:tc:SAML:2.0:nameid-format:entity";
    }

    /// <summary>
    /// SAML Subject Confirmation Methods
    /// </summary>
    public static class SubjectConfirmationMethods
    {
        /// <summary>Bearer confirmation</summary>
        public const string Bearer = "urn:oasis:names:tc:SAML:2.0:cm:bearer";
        /// <summary>Holder of Key confirmation</summary>
        public const string HolderOfKey = "urn:oasis:names:tc:SAML:2.0:cm:holder-of-key";
        /// <summary>Sender Vouches confirmation</summary>
        public const string SenderVouches = "urn:oasis:names:tc:SAML:2.0:cm:sender-vouches";
    }

    /// <summary>
    /// SAML Authentication Context Classes
    /// </summary>
    public static class AuthnContextClasses
    {
        /// <summary>Password authentication</summary>
        public const string Password = "urn:oasis:names:tc:SAML:2.0:ac:classes:Password";
        /// <summary>Password Protected Transport</summary>
        public const string PasswordProtectedTransport = "urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport";
        /// <summary>X509 Certificate authentication</summary>
        public const string X509 = "urn:oasis:names:tc:SAML:2.0:ac:classes:X509";
        /// <summary>Smartcard authentication</summary>
        public const string SmartcardPKI = "urn:oasis:names:tc:SAML:2.0:ac:classes:SmartcardPKI";
        /// <summary>Kerberos authentication</summary>
        public const string Kerberos = "urn:oasis:names:tc:SAML:2.0:ac:classes:Kerberos";
        /// <summary>Unspecified context</summary>
        public const string Unspecified = "urn:oasis:names:tc:SAML:2.0:ac:classes:unspecified";
    }

    /// <summary>
    /// SAML Status Codes
    /// </summary>
    public static class StatusCodes
    {
        /// <summary>Success status</summary>
        public const string Success = "urn:oasis:names:tc:SAML:2.0:status:Success";
        /// <summary>Requester error status</summary>
        public const string Requester = "urn:oasis:names:tc:SAML:2.0:status:Requester";
        /// <summary>Responder error status</summary>
        public const string Responder = "urn:oasis:names:tc:SAML:2.0:status:Responder";
        /// <summary>Version mismatch status</summary>
        public const string VersionMismatch = "urn:oasis:names:tc:SAML:2.0:status:VersionMismatch";
        /// <summary>Authentication failed status</summary>
        public const string AuthnFailed = "urn:oasis:names:tc:SAML:2.0:status:AuthnFailed";
        /// <summary>Invalid attribute name or value</summary>
        public const string InvalidAttrNameOrValue = "urn:oasis:names:tc:SAML:2.0:status:InvalidAttrNameOrValue";
        /// <summary>Invalid NameID policy</summary>
        public const string InvalidNameIDPolicy = "urn:oasis:names:tc:SAML:2.0:status:InvalidNameIDPolicy";
        /// <summary>No authentication context</summary>
        public const string NoAuthnContext = "urn:oasis:names:tc:SAML:2.0:status:NoAuthnContext";
        /// <summary>No available IDP</summary>
        public const string NoAvailableIDP = "urn:oasis:names:tc:SAML:2.0:status:NoAvailableIDP";
        /// <summary>No passive support</summary>
        public const string NoPassive = "urn:oasis:names:tc:SAML:2.0:status:NoPassive";
        /// <summary>No supported IDP</summary>
        public const string NoSupportedIDP = "urn:oasis:names:tc:SAML:2.0:status:NoSupportedIDP";
        /// <summary>Partial logout status</summary>
        public const string PartialLogout = "urn:oasis:names:tc:SAML:2.0:status:PartialLogout";
        /// <summary>Proxy count exceeded</summary>
        public const string ProxyCountExceeded = "urn:oasis:names:tc:SAML:2.0:status:ProxyCountExceeded";
        /// <summary>Request denied status</summary>
        public const string RequestDenied = "urn:oasis:names:tc:SAML:2.0:status:RequestDenied";
        /// <summary>Request unsupported</summary>
        public const string RequestUnsupported = "urn:oasis:names:tc:SAML:2.0:status:RequestUnsupported";
        /// <summary>Request version deprecated</summary>
        public const string RequestVersionDeprecated = "urn:oasis:names:tc:SAML:2.0:status:RequestVersionDeprecated";
        /// <summary>Request version too high</summary>
        public const string RequestVersionTooHigh = "urn:oasis:names:tc:SAML:2.0:status:RequestVersionTooHigh";
        /// <summary>Request version too low</summary>
        public const string RequestVersionTooLow = "urn:oasis:names:tc:SAML:2.0:status:RequestVersionTooLow";
        /// <summary>Resource not recognized</summary>
        public const string ResourceNotRecognized = "urn:oasis:names:tc:SAML:2.0:status:ResourceNotRecognized";
        /// <summary>Too many responses</summary>
        public const string TooManyResponses = "urn:oasis:names:tc:SAML:2.0:status:TooManyResponses";
        /// <summary>Unknown attribute profile</summary>
        public const string UnknownAttrProfile = "urn:oasis:names:tc:SAML:2.0:status:UnknownAttrProfile";
        /// <summary>Unknown principal</summary>
        public const string UnknownPrincipal = "urn:oasis:names:tc:SAML:2.0:status:UnknownPrincipal";
        /// <summary>Unsupported binding</summary>
        public const string UnsupportedBinding = "urn:oasis:names:tc:SAML:2.0:status:UnsupportedBinding";
    }

    /// <summary>
    /// SAML Attribute Name Formats
    /// </summary>
    public static class AttributeNameFormats
    {
        /// <summary>Unspecified format</summary>
        public const string Unspecified = "urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified";
        /// <summary>URI format</summary>
        public const string Uri = "urn:oasis:names:tc:SAML:2.0:attrname-format:uri";
        /// <summary>Basic format</summary>
        public const string Basic = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic";
    }
}