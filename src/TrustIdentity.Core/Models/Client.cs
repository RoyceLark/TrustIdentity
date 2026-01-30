using System;
using System.Collections.Generic;
using System.Security.Claims;
namespace TrustIdentity.Core.Models;

/// <summary>
/// Represents an OAuth 2.0 or OpenID Connect client application
/// </summary>
public class Client
{
    /// <summary>
    /// Unique identifier for the client
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Optional tenant identifier for multi-tenancy
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Client secrets for authentication
    /// </summary>
    public List<Secret> ClientSecrets { get; set; } = new();

    /// <summary>
    /// Display name of the client
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    /// Description of the client
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URI for the client logo
    /// </summary>
    public string? ClientUri { get; set; }

    /// <summary>
    /// URI for the client logo
    /// </summary>
    public string? LogoUri { get; set; }

    /// <summary>
    /// Specifies if client is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Protocol type (defaults to "oidc")
    /// </summary>
    public string ProtocolType { get; set; } = ProtocolTypes.OpenIdConnect;

    /// <summary>
    /// If set to false, no client secret is needed to request tokens
    /// </summary>
    public bool RequireClientSecret { get; set; } = true;

    /// <summary>
    /// Allowed grant types for the client
    /// </summary>
    public List<string> AllowedGrantTypes { get; set; } = new();

    /// <summary>
    /// Specifies whether a proof key is required for authorization code based token requests
    /// </summary>
    public bool RequirePkce { get; set; } = true;

    /// <summary>
    /// Specifies whether a proof key can be sent using plain method
    /// </summary>
    public bool AllowPlainTextPkce { get; set; } = false;

    /// <summary>
    /// Controls whether access tokens are transmitted via the browser
    /// </summary>
    public bool AllowAccessTokensViaBrowser { get; set; } = false;

    /// <summary>
    /// Allowed redirect URIs
    /// </summary>
    public List<string> RedirectUris { get; set; } = new();

    /// <summary>
    /// Allowed post-logout redirect URIs
    /// </summary>
    public List<string> PostLogoutRedirectUris { get; set; } = new();

    /// <summary>
    /// Front-channel logout URI
    /// </summary>
    public string? FrontChannelLogoutUri { get; set; }

    /// <summary>
    /// Specifies if user's session id should be sent in front-channel logout
    /// </summary>
    public bool FrontChannelLogoutSessionRequired { get; set; } = true;

    /// <summary>
    /// Back-channel logout URI
    /// </summary>
    public string? BackChannelLogoutUri { get; set; }

    /// <summary>
    /// Specifies if user's session id should be sent in back-channel logout
    /// </summary>
    public bool BackChannelLogoutSessionRequired { get; set; } = true;

    /// <summary>
    /// Specifies if user consent is required
    /// </summary>
    public bool RequireConsent { get; set; } = false;

    /// <summary>
    /// Specifies whether user can choose to store consent decisions
    /// </summary>
    public bool AllowRememberConsent { get; set; } = true;

    /// <summary>
    /// Allowed scopes for the client
    /// </summary>
    public List<string> AllowedScopes { get; set; } = new();

    /// <summary>
    /// When requesting both an id token and access token, should the user claims always be added to the id token
    /// </summary>
    public bool AlwaysIncludeUserClaimsInIdToken { get; set; } = false;

    /// <summary>
    /// Lifetime of identity token in seconds
    /// </summary>
    public int IdentityTokenLifetime { get; set; } = 300;

    /// <summary>
    /// Signing algorithm for identity token
    /// </summary>
    public List<string> AllowedIdentityTokenSigningAlgorithms { get; set; } = new();

    /// <summary>
    /// Lifetime of access token in seconds
    /// </summary>
    public int AccessTokenLifetime { get; set; } = 3600;

    /// <summary>
    /// Lifetime of authorization code in seconds
    /// </summary>
    public int AuthorizationCodeLifetime { get; set; } = 300;

    /// <summary>
    /// Maximum lifetime of a refresh token in seconds
    /// </summary>
    public int AbsoluteRefreshTokenLifetime { get; set; } = 2592000;

    /// <summary>
    /// Sliding lifetime of a refresh token in seconds
    /// </summary>
    public int SlidingRefreshTokenLifetime { get; set; } = 1296000;

    /// <summary>
    /// Lifetime of a user consent in seconds
    /// </summary>
    public int? ConsentLifetime { get; set; }

    /// <summary>
    /// ReUse: the refresh token handle will stay the same when refreshing tokens
    /// OneTime: the refresh token handle will be updated when refreshing tokens
    /// </summary>
    public TokenUsage RefreshTokenUsage { get; set; } = TokenUsage.OneTimeOnly;

    /// <summary>
    /// Gets or sets a value indicating whether the access token (and its claims) should be updated on a refresh token request
    /// </summary>
    public bool UpdateAccessTokenClaimsOnRefresh { get; set; } = false;

    /// <summary>
    /// Absolute: the refresh token will expire on a fixed point in time
    /// Sliding: when refreshing the token, the lifetime of the refresh token will be renewed
    /// </summary>
    public TokenExpiration RefreshTokenExpiration { get; set; } = TokenExpiration.Absolute;

    /// <summary>
    /// Specifies whether the access token is a reference token or a self contained JWT token
    /// </summary>
    public AccessTokenType AccessTokenType { get; set; } = AccessTokenType.Jwt;

    /// <summary>
    /// Gets or sets a value indicating whether the local login is allowed for this client
    /// </summary>
    public bool EnableLocalLogin { get; set; } = true;

    /// <summary>
    /// Specifies which external IdPs can be used with this client
    /// </summary>
    public List<string> IdentityProviderRestrictions { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether JWT access tokens should include an identifier
    /// </summary>
    public bool IncludeJwtId { get; set; } = true;

    /// <summary>
    /// Allows settings claims for the client
    /// </summary>
    public List<ClientClaim> Claims { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether client claims should be always included in the access tokens
    /// </summary>
    public bool AlwaysSendClientClaims { get; set; } = false;

    /// <summary>
    /// Gets or sets a value to prefix client claim types
    /// </summary>
    public string? ClientClaimsPrefix { get; set; } = "client_";

    /// <summary>
    /// Gets or sets a salt value used in pair-wise subjectId generation for users of this client
    /// </summary>
    public string? PairWiseSubjectSalt { get; set; }

    /// <summary>
    /// The allowed CORS origins for JavaScript clients
    /// </summary>
    public List<string> AllowedCorsOrigins { get; set; } = new();

    /// <summary>
    /// Gets or sets the custom properties for the client
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// Creation time
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update time
    /// </summary>
    public DateTime? Updated { get; set; }

    /// <summary>
    /// Last time the client was used
    /// </summary>
    public DateTime? LastAccessed { get; set; }

    /// <summary>
    /// User session lifetime in seconds. Set to null to use server default.
    /// </summary>
    public int? UserSsoLifetime { get; set; }

    /// <summary>
    /// Gets or sets the type of the device flow user code
    /// </summary>
    public string? UserCodeType { get; set; }

    /// <summary>
    /// Gets or sets the device code lifetime
    /// </summary>
    public int DeviceCodeLifetime { get; set; } = 300;

    /// <summary>
    /// Non-editable flag
    /// </summary>
    public bool NonEditable { get; set; } = false;

    /// <summary>
    /// Pushed authorization request lifetime in seconds
    /// </summary>
    public int? PushedAuthorizationLifetime { get; set; }

    /// <summary>
    /// Require pushed authorization requests
    /// </summary>
    public bool RequirePushedAuthorization { get; set; } = false;

    /// <summary>
    /// DPoP validation mode
    /// </summary>
    public DPoPValidationMode DPoPValidationMode { get; set; } = DPoPValidationMode.Disabled;

    /// <summary>
    /// DPoP clock skew in seconds
    /// </summary>
    public int DPoPClockSkew { get; set; } = 300;

    /// <summary>
    /// Require request object
    /// </summary>
    public bool RequireRequestObject { get; set; } = false;

    /// <summary>
    /// Allowed request object signing algorithms
    /// </summary>
    public List<string> AllowedRequestObjectSigningAlgorithms { get; set; } = new();

    /// <summary>
    /// Coordinate OIDC request with authorization request
    /// </summary>
    public bool CoordinateLifetimeWithUserSession { get; set; } = false;

    /// <summary>
    /// AI/ML Settings for this client
    /// </summary>
    public ClientAISettings? AISettings { get; set; }

    /// <summary>
    /// Specifies whether offline access is allowed
    /// </summary>
    public bool AllowOfflineAccess { get; set; }
}

/// <summary>
/// Client secret
/// </summary>
public class Secret
{
    /// <summary>Description of the secret</summary>
    public string? Description { get; set; }
    /// <summary>Value of the secret (usually hashed)</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Expiration date of the secret</summary>
    public DateTime? Expiration { get; set; }
    /// <summary>Type of the secret</summary>
    public string Type { get; set; } = SecretTypes.SharedSecret;
    /// <summary>Creation time</summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Client claim
/// </summary>
public class ClientClaim
{
    /// <summary>The claim type</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>The claim value</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>The value type of the claim</summary>
    public string? ValueType { get; set; } = ClaimValueTypes.String;
}

/// <summary>
/// AI/ML settings for client
/// </summary>
public class ClientAISettings
{
    /// <summary>Whether to enable fraud detection</summary>
    public bool EnableFraudDetection { get; set; } = true;
    /// <summary>Whether to enable behavior analysis</summary>
    public bool EnableBehaviorAnalysis { get; set; } = true;
    /// <summary>Whether to enable risk scoring</summary>
    public bool EnableRiskScoring { get; set; } = true;
    /// <summary>The fraud score threshold</summary>
    public double FraudThreshold { get; set; } = 0.7;
    /// <summary>The risk score threshold</summary>
    public double RiskThreshold { get; set; } = 0.8;
    /// <summary>Whether to enable anomaly detection</summary>
    public bool EnableAnomalyDetection { get; set; } = true;
    /// <summary>Whether to enable adaptive authentication</summary>
    public bool EnableAdaptiveAuthentication { get; set; } = false;
}

/// <summary>
/// Supported protocol types
/// </summary>
public static class ProtocolTypes
{
    /// <summary>OpenID Connect</summary>
    public const string OpenIdConnect = "oidc";
    /// <summary>SAML 2.0</summary>
    public const string Saml2p = "saml2p";
    /// <summary>WS-Federation</summary>
    public const string WsFederation = "wsfed";
}

/// <summary>
/// Supported secret types
/// </summary>
public static class SecretTypes
{
    /// <summary>Shared secret</summary>
    public const string SharedSecret = "SharedSecret";
    /// <summary>X509 Certificate Thumbprint</summary>
    public const string X509CertificateThumbprint = "X509Thumbprint";
    /// <summary>X509 Certificate Distinguished Name</summary>
    public const string X509CertificateName = "X509Name";
    /// <summary>X509 Certificate Base64 encoded</summary>
    public const string X509CertificateBase64 = "X509CertificateBase64";
    /// <summary>JSON Web Key (JWK)</summary>
    public const string JWK = "JWK";
}

/// <summary>
/// Token usage modes
/// </summary>
public enum TokenUsage
{
    /// <summary>Re-use the same handle</summary>
    ReUse = 0,
    /// <summary>Issue a new handle on every use</summary>
    OneTimeOnly = 1
}

/// <summary>
/// Token expiration modes
/// </summary>
public enum TokenExpiration
{
    /// <summary>Sliding expiration</summary>
    Sliding = 0,
    /// <summary>Absolute expiration</summary>
    Absolute = 1
}

/// <summary>
/// Access token types
/// </summary>
public enum AccessTokenType
{
    /// <summary>JSON Web Token (JWT)</summary>
    Jwt = 0,
    /// <summary>Reference token</summary>
    Reference = 1
}

/// <summary>
/// DPoP validation modes
/// </summary>
public enum DPoPValidationMode
{
    /// <summary>Disabled</summary>
    Disabled = 0,
    /// <summary>Validate only when present</summary>
    ValidateWhenPresent = 1,
    /// <summary>Strictly required</summary>
    Required = 2
}
