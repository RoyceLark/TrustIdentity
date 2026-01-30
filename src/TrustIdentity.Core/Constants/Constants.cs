namespace TrustIdentity.Core.Constants;

/// <summary>
/// Supported OAuth 2.0 and OpenID Connect response types
/// </summary>
public static class ResponseTypes
{
    /// <summary>Authorization Code</summary>
    public const string Code = "code";
    /// <summary>Access Token</summary>
    public const string Token = "token";
    /// <summary>ID Token</summary>
    public const string IdToken = "id_token";
    /// <summary>ID Token and Access Token</summary>
    public const string IdTokenToken = "id_token token";
    /// <summary>Authorization Code and ID Token</summary>
    public const string CodeIdToken = "code id_token";
    /// <summary>Authorization Code and Access Token</summary>
    public const string CodeToken = "code token";
    /// <summary>Authorization Code, ID Token and Access Token</summary>
    public const string CodeIdTokenToken = "code id_token token";
}

/// <summary>
/// Standard OpenID Connect scopes
/// </summary>
public static class StandardScopes
{
    /// <summary>OpenID scope (required for OIDC)</summary>
    public const string OpenId = "openid";
    /// <summary>Profile information</summary>
    public const string Profile = "profile";
    /// <summary>Email address</summary>
    public const string Email = "email";
    /// <summary>Phone number</summary>
    public const string Phone = "phone";
    /// <summary>Postal address</summary>
    public const string Address = "address";
    /// <summary>Offline access (for refresh tokens)</summary>
    public const string OfflineAccess = "offline_access";
}

/// <summary>
/// Standard claim types used in JWT and OIDC
/// </summary>
public static class ClaimTypes
{
    /// <summary>Subject (User ID)</summary>
    public const string Subject = "sub";
    /// <summary>Full Name</summary>
    public const string Name = "name";
    /// <summary>Given Name</summary>
    public const string GivenName = "given_name";
    /// <summary>Family Name</summary>
    public const string FamilyName = "family_name";
    /// <summary>Middle Name</summary>
    public const string MiddleName = "middle_name";
    /// <summary>Nickname</summary>
    public const string Nickname = "nickname";
    /// <summary>Preferred Username</summary>
    public const string PreferredUsername = "preferred_username";
    /// <summary>Profile URL</summary>
    public const string Profile = "profile";
    /// <summary>Picture URL</summary>
    public const string Picture = "picture";
    /// <summary>Website URL</summary>
    public const string Website = "website";
    /// <summary>Email Address</summary>
    public const string Email = "email";
    /// <summary>Email Verified Status</summary>
    public const string EmailVerified = "email_verified";
    /// <summary>Gender</summary>
    public const string Gender = "gender";
    /// <summary>Birth Date</summary>
    public const string BirthDate = "birthdate";
    /// <summary>Zone Information</summary>
    public const string ZoneInfo = "zoneinfo";
    /// <summary>Locale Information</summary>
    public const string Locale = "locale";
    /// <summary>Phone Number</summary>
    public const string PhoneNumber = "phone_number";
    /// <summary>Phone Number Verified Status</summary>
    public const string PhoneNumberVerified = "phone_number_verified";
    /// <summary>Postal Address</summary>
    public const string Address = "address";
    /// <summary>Updated At</summary>
    public const string UpdatedAt = "updated_at";
    /// <summary>Scope(s)</summary>
    public const string Scope = "scope";
    /// <summary>Client ID</summary>
    public const string ClientId = "client_id";
    /// <summary>Audience</summary>
    public const string Audience = "aud";
    /// <summary>Issuer</summary>
    public const string Issuer = "iss";
    /// <summary>Not Before Time</summary>
    public const string NotBefore = "nbf";
    /// <summary>Expiration Time</summary>
    public const string Expiration = "exp";
    /// <summary>Issued At Time</summary>
    public const string IssuedAt = "iat";
    /// <summary>Authentication Time</summary>
    public const string AuthenticationTime = "auth_time";
    /// <summary>Identity Provider</summary>
    public const string IdentityProvider = "idp";
    /// <summary>Authentication Method Reference</summary>
    public const string AuthenticationMethod = "amr";
    /// <summary>Authentication Context Class Reference</summary>
    public const string AuthenticationContextClassReference = "acr";
    /// <summary>Session ID</summary>
    public const string SessionId = "sid";
    /// <summary>JWT ID</summary>
    public const string JwtId = "jti";
    /// <summary>Nonce</summary>
    public const string Nonce = "nonce";
    /// <summary>Access Token Hash</summary>
    public const string AtHash = "at_hash";
    /// <summary>Code Hash</summary>
    public const string CHash = "c_hash";
    /// <summary>State Hash</summary>
    public const string SHash = "s_hash";
}

/// <summary>
/// Standard protocol route paths for OIDC/OAuth
/// </summary>
public static class ProtocolRoutePaths
{
    /// <summary>Authorization endpoint</summary>
    public const string Authorize = "connect/authorize";
    /// <summary>Token endpoint</summary>
    public const string Token = "connect/token";
    /// <summary>Device authorization endpoint</summary>
    public const string DeviceAuthorization = "connect/deviceauthorization";
    /// <summary>Discovery document endpoint</summary>
    public const string Discovery = ".well-known/openid-configuration";
    /// <summary>JWKS endpoint</summary>
    public const string Jwks = ".well-known/openid-configuration/jwks";
    /// <summary>User info endpoint</summary>
    public const string UserInfo = "connect/userinfo";
    /// <summary>Introspection endpoint</summary>
    public const string Introspection = "connect/introspect";
    /// <summary>Revocation endpoint</summary>
    public const string Revocation = "connect/revocation";
    /// <summary>End session endpoint</summary>
    public const string EndSession = "connect/endsession";
    /// <summary>Check session iframe</summary>
    public const string CheckSession = "connect/checksession";
    /// <summary>Backchannel authentication endpoint (CIBA)</summary>
    public const string BackchannelAuthentication = "connect/ciba";
    /// <summary>Pushed authorization endpoint (PAR)</summary>
    public const string PushedAuthorization = "connect/par";
}