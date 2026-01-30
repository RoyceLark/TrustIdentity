namespace TrustIdentity.Abstractions.Models;

using System;
using System.Collections.Generic;
/// <summary>
/// Represents an OAuth 2.0/OpenID Connect Client
/// </summary>
public class Client
{
    /// <summary>
    /// Unique ID of the client
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Optional tenant identifier for multi-tenancy
    /// </summary>
    public string? TenantId { get; set; }
    
    /// <summary>
    /// Specifies if client is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Display name of the client
    /// </summary>
    public string? ClientName { get; set; }
    
    /// <summary>
    /// Description of the client
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// URI for further information about the client
    /// </summary>
    public string? ClientUri { get; set; }
    
    /// <summary>
    /// URI to the client's logo
    /// </summary>
    public string? LogoUri { get; set; }
    
    /// <summary>
    /// Primary color for themed UI (hex)
    /// </summary>
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// Secondary color for themed UI (hex)
    /// </summary>
    public string? SecondaryColor { get; set; }

    /// <summary>
    /// Custom CSS snippet to be injected into the UI
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Protocol Type (oidc, saml, wsfed)
    /// </summary>
    public string ProtocolType { get; set; } = "oidc";
    
    /// <summary>
    /// Specifies whether a consent screen is required
    /// </summary>
    public bool RequireConsent { get; set; } = true;
    
    /// <summary>
    /// Specifies whether user consent can be remembered
    /// </summary>
    public bool AllowRememberConsent { get; set; } = true;
    
    /// <summary>
    /// Specifies whether user claims are always included in the identity token
    /// </summary>
    public bool AlwaysIncludeUserClaimsInIdToken { get; set; }
    
    /// <summary>
    /// Allowed grant types for the client (e.g. authorization_code, client_credentials)
    /// </summary>
    public List<string> AllowedGrantTypes { get; set; } = new();
    
    /// <summary>
    /// Specifies whether Proof Key for Code Exchange is required
    /// </summary>
    public bool RequirePkce { get; set; } = true;
    
    /// <summary>
    /// Specifies whether a plain text PKCE is allowed
    /// </summary>
    public bool AllowPlainTextPkce { get; set; }
    
    /// <summary>
    /// Specifies whether a client secret is needed to request tokens
    /// </summary>
    public bool RequireClientSecret { get; set; } = true;
    
    /// <summary>
    /// List of client secrets
    /// </summary>
    public List<Secret> ClientSecrets { get; set; } = new();
    
    /// <summary>
    /// Specifies whether access tokens can be transmitted via browser
    /// </summary>
    public bool AllowAccessTokensViaBrowser { get; set; }
    
    /// <summary>
    /// Allowed URIs to redirect to after login
    /// </summary>
    public List<string> RedirectUris { get; set; } = new();
    
    /// <summary>
    /// Allowed URIs to redirect to after logout
    /// </summary>
    public List<string> PostLogoutRedirectUris { get; set; } = new();
    
    /// <summary>
    /// URI for front-channel logout
    /// </summary>
    public string? FrontChannelLogoutUri { get; set; }
    
    /// <summary>
    /// Specifies whether the session ID is sent with front-channel logout
    /// </summary>
    public bool FrontChannelLogoutSessionRequired { get; set; } = true;
    
    /// <summary>
    /// URI for back-channel logout
    /// </summary>
    public string? BackChannelLogoutUri { get; set; }
    
    /// <summary>
    /// Specifies whether the session ID is sent with back-channel logout
    /// </summary>
    public bool BackChannelLogoutSessionRequired { get; set; } = true;
    
    /// <summary>
    /// Specifies whether offline access (refresh tokens) is allowed
    /// </summary>
    public bool AllowOfflineAccess { get; set; }
    
    /// <summary>
    /// Allowed scopes for the client
    /// </summary>
    public List<string> AllowedScopes { get; set; } = new();
    
    /// <summary>
    /// Lifetime of identity token in seconds
    /// </summary>
    public int IdentityTokenLifetime { get; set; } = 300;
    
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
    /// Refresh token usage (OneTimeOnly or Reuse)
    /// </summary>
    public int RefreshTokenUsage { get; set; } = 1;
    
    /// <summary>
    /// Update access token claims on refresh
    /// </summary>
    public bool UpdateAccessTokenClaimsOnRefresh { get; set; }
    
    /// <summary>
    /// Refresh token expiration (Absolute or Sliding)
    /// </summary>
    public int RefreshTokenExpiration { get; set; } = 1;
    
    /// <summary>
    /// Access token type (Jwt or Reference)
    /// </summary>
    public int AccessTokenType { get; set; }
    
    /// <summary>
    /// Enable local login
    /// </summary>
    public bool EnableLocalLogin { get; set; } = true;
    
    /// <summary>
    /// List of allowed identity providers
    /// </summary>
    public List<string> IdentityProviderRestrictions { get; set; } = new();
    
    /// <summary>
    /// Include JWT ID
    /// </summary>
    public bool IncludeJwtId { get; set; }
    
    /// <summary>
    /// List of client claims
    /// </summary>
    public List<ClientClaim> Claims { get; set; } = new();
    
    /// <summary>
    /// Always send client claims
    /// </summary>
    public bool AlwaysSendClientClaims { get; set; }
    
    /// <summary>
    /// Prefix for client claims
    /// </summary>
    public string? ClientClaimsPrefix { get; set; } = "client_";
    
    /// <summary>
    /// Salt for pairwise subject calculation
    /// </summary>
    public string? PairWiseSubjectSalt { get; set; }
    
    /// <summary>
    /// Lifetime of user SSO session
    /// </summary>
    public int? UserSsoLifetime { get; set; }
    
    /// <summary>
    /// Type of user code
    /// </summary>
    public string? UserCodeType { get; set; }
    
    /// <summary>
    /// Lifetime of device code
    /// </summary>
    public int DeviceCodeLifetime { get; set; } = 300;
    
    /// <summary>
    /// Allowed CORS origins
    /// </summary>
    public List<string> AllowedCorsOrigins { get; set; } = new();
    
    /// <summary>
    /// Date when the client was created
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date when the client was last updated
    /// </summary>
    public DateTime? Updated { get; set; }
    
    /// <summary>
    /// Date when the client was last accessed
    /// </summary>
    public DateTime LastAccessed { get; set; }
    
    /// <summary>
    /// AI-powered settings for this client
    /// </summary>
    public ClientAISettings? AISettings { get; set; }
}

/// <summary>
/// AI-powered settings for a client
/// </summary>
public class ClientAISettings
{
    /// <summary>Enable AI fraud detection for this client</summary>
    public bool EnableFraudDetection { get; set; }
    
    /// <summary>Enable AI behavior analysis for this client</summary>
    public bool EnableBehaviorAnalysis { get; set; }
    
    /// <summary>Fraud detection threshold (0.0 to 1.0)</summary>
    public double FraudThreshold { get; set; } = 0.7;
    
    /// <summary>Risk scoring threshold (0.0 to 1.0)</summary>
    public double RiskThreshold { get; set; } = 0.5;
    
    /// <summary>Enable adaptive authentication based on risk</summary>
    public bool EnableAdaptiveAuthentication { get; set; }
}

/// <summary>
/// Represents a claim for a client
/// </summary>
public class ClientClaim
{
    /// <summary>Primary key</summary>
    public int Id { get; set; }
    /// <summary>Claim type</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Claim value</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Claim value type</summary>
    public string? ValueType { get; set; }
}