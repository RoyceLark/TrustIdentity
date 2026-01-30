namespace TrustIdentity.WsFederation;

/// <summary>
/// Constants for WS-Federation protocol
/// </summary>
public static class WsFederationConstants
{
    /// <summary>The WS-Federation namespace</summary>
    public const string Namespace = "http://docs.oasis-open.org/wsfed/federation/200706";
    
    /// <summary>
    /// WS-Federation Actions
    /// </summary>
    public static class Actions
    {
        /// <summary>Sign-in action</summary>
        public const string SignIn = "wsignin1.0";
        /// <summary>Sign-out action</summary>
        public const string SignOut = "wsignout1.0";
        /// <summary>Sign-out cleanup action</summary>
        public const string SignOutCleanup = "wsignoutcleanup1.0";
    }
    
    /// <summary>
    /// WS-Federation Parameters
    /// </summary>
    public static class Parameters
    {
        /// <summary>The action parameter (wa)</summary>
        public const string Action = "wa";
        /// <summary>The realm parameter (wtrealm)</summary>
        public const string Realm = "wtrealm";
        /// <summary>The reply URL parameter (wreply)</summary>
        public const string Reply = "wreply";
        /// <summary>The context parameter (wctx)</summary>
        public const string Context = "wctx";
        /// <summary>The current time parameter (wct)</summary>
        public const string CurrentTime = "wct";
        /// <summary>The home realm parameter (whr)</summary>
        public const string HomeRealm = "whr";
        /// <summary>The request parameter (wreq)</summary>
        public const string Request = "wreq";
        /// <summary>The freshness parameter (wfresh)</summary>
        public const string Freshness = "fresh";
        /// <summary>The authentication type parameter (wauth)</summary>
        public const string AuthenticationType = "wauth";
        /// <summary>The result parameter (wresult)</summary>
        public const string Result = "wresult";
    }
}

/// <summary>
/// Constants for WS-Trust protocol
/// </summary>
public static class WsTrustConstants
{
    /// <summary>WS-Trust 1.3 namespace</summary>
    public const string Namespace13 = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
    /// <summary>WS-Trust 1.4 namespace</summary>
    public const string Namespace14 = "http://docs.oasis-open.org/ws-sx/ws-trust/200802";
    
    /// <summary>
    /// WS-Trust Request Types
    /// </summary>
    public static class RequestTypes
    {
        /// <summary>Issue token</summary>
        public const string Issue = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue";
        /// <summary>Renew token</summary>
        public const string Renew = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Renew";
        /// <summary>Validate token</summary>
        public const string Validate = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Validate";
        /// <summary>Cancel token</summary>
        public const string Cancel = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Cancel";
    }
    
    /// <summary>
    /// WS-Trust Token Types
    /// </summary>
    public static class TokenTypes
    {
        /// <summary>SAML 1.1 token type</summary>
        public const string Saml11 = "urn:oasis:names:tc:SAML:1.0:assertion";
        /// <summary>SAML 2.0 token type</summary>
        public const string Saml20 = "urn:oasis:names:tc:SAML:2.0:assertion";
        /// <summary>JWT token type</summary>
        public const string Jwt = "urn:ietf:params:oauth:token-type:jwt";
    }
    
    /// <summary>
    /// WS-Trust Key Types
    /// </summary>
    public static class KeyTypes
    {
        /// <summary>Bearer key type</summary>
        public const string Bearer = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Bearer";
        /// <summary>Symmetric key type</summary>
        public const string Symmetric = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/SymmetricKey";
        /// <summary>Asymmetric key type</summary>
        public const string Asymmetric = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey";
    }
}

/// <summary>
/// Constants for ADFS compatibility
/// </summary>
public static class AdfsConstants
{
    /// <summary>
    /// ADFS Claim Types
    /// </summary>
    public static class ClaimTypes
    {
        /// <summary>Windows account name claim</summary>
        public const string WindowsAccountName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname";
        /// <summary>Primary group SID claim</summary>
        public const string PrimaryGroupSid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid";
        /// <summary>Primary SID claim</summary>
        public const string PrimarySid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid";
        /// <summary>Group SID claim</summary>
        public const string GroupSid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid";
        /// <summary>Role claim</summary>
        public const string Role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        /// <summary>UPN claim</summary>
        public const string Upn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
        /// <summary>Email address claim</summary>
        public const string Email = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        /// <summary>Name claim</summary>
        public const string Name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        /// <summary>Given name claim</summary>
        public const string GivenName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        /// <summary>Surname claim</summary>
        public const string Surname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
    }
    
    /// <summary>
    /// ADFS Authentication Methods
    /// </summary>
    public static class AuthenticationMethods
    {
        /// <summary>Password authentication</summary>
        public const string Password = "urn:oasis:names:tc:SAML:2.0:ac:classes:Password";
        /// <summary>Kerberos authentication</summary>
        public const string Kerberos = "urn:federation:authentication:windows";
        /// <summary>X509 certificate authentication</summary>
        public const string X509 = "urn:oasis:names:tc:SAML:2.0:ac:classes:X509";
        /// <summary>Smart card authentication</summary>
        public const string SmartCard = "urn:oasis:names:tc:SAML:2.0:ac:classes:SmartcardPKI";
        /// <summary>Two-factor authentication</summary>
        public const string TwoFactor = "http://schemas.microsoft.com/claims/multipleauthn";
    }
}