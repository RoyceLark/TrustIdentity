using System;
using System.Collections.Generic;

namespace TrustIdentity.Abstractions.Configuration;

/// <summary>
/// Top-level options for TrustIdentity
/// </summary>
public class TrustIdentityOptions
{
    /// <summary>The issuer URI for the identity server</summary>
    public string IssuerUri { get; set; } = "https://identity.yourdomain.com";
    /// <summary>Whether to enable AI features</summary>
    public bool EnableAI { get; set; } = false;
    /// <summary>Whether to enable fraud detection</summary>
    public bool EnableFraudDetection { get; set; } = false;
    /// <summary>Whether to block logins with high risk scores (determined by AI)</summary>
    public bool BlockHighRiskLogins { get; set; } = false;
    /// <summary>Whether to enable behavior analysis</summary>
    public bool EnableBehaviorAnalysis { get; set; } = false;
    /// <summary>Whether to enable license enforcement (defaulted to false/disabled for now)</summary>
    public bool EnableLicensing { get; set; } = false;
    /// <summary>Whether to require HTTPS</summary>
    public bool RequireHttps { get; set; } = true;
    /// <summary>Authentication-related options</summary>
    public AuthenticationOptions Authentication { get; set; } = new();
    /// <summary>Event-related options</summary>
    public EventsOptions Events { get; set; } = new();
    /// <summary>Endpoint-related options</summary>
    public EndpointsOptions Endpoints { get; set; } = new();
    /// <summary>Discovery-related options</summary>
    public DiscoveryOptions Discovery { get; set; } = new();
    /// <summary>User interaction configuration (URLs)</summary>
    public UserInteractionOptions UserInteraction { get; set; } = new();
    /// <summary>SMTP options</summary>
    public SmtpOptions? Smtp { get; set; }
    /// <summary>Caching-related options</summary>
    public CachingOptions Caching { get; set; } = new();
    /// <summary>CORS-related options</summary>
    public CorsOptions Cors { get; set; } = new();
    /// <summary>CSP-related options</summary>
    public CspOptions Csp { get; set; } = new();
    /// <summary>Validation-related options</summary>
    public ValidationOptions Validation { get; set; } = new();
    /// <summary>Device flow-related options</summary>
    public DeviceFlowOptions DeviceFlow { get; set; } = new();
    /// <summary>Mutual TLS-related options</summary>
    public MutualTlsOptions MutualTls { get; set; } = new();
    /// <summary>DPoP-related options</summary>
    public DPoPOptions DPoP { get; set; } = new();
}

/// <summary>
/// Options for authentication
/// </summary>
public class AuthenticationOptions
{
    /// <summary>The cookie authentication scheme name</summary>
    public string CookieAuthenticationScheme { get; set; } = "TrustIdentity";
    /// <summary>The cookie lifetime in seconds</summary>
    public int CookieLifetime { get; set; } = 3600;
    /// <summary>The cookie sliding expiration</summary>
    public bool CookieSlidingExpiration { get; set; } = true;
    
    /// <summary>Access token lifetime in seconds (default 1 hour)</summary>
    public int AccessTokenLifetime { get; set; } = 3600;
    /// <summary>Refresh token lifetime in seconds (default 30 days)</summary>
    public int RefreshTokenLifetime { get; set; } = 2592000;
    /// <summary>Authorization code lifetime in seconds (default 5 minutes)</summary>
    public int AuthorizationCodeLifetime { get; set; } = 300;
    /// <summary>Maximum allowed token lifetime in seconds (default 365 days)</summary>
    public int MaximumTokenLifetime { get; set; } = 31536000;
}

/// <summary>
/// Options for raising events
/// </summary>
public class EventsOptions
{
    /// <summary>Whether to raise success events</summary>
    public bool RaiseSuccessEvents { get; set; } = true;
    /// <summary>Whether to raise failure events</summary>
    public bool RaiseFailureEvents { get; set; } = true;
    /// <summary>Whether to raise error events</summary>
    public bool RaiseErrorEvents { get; set; } = true;
    /// <summary>Whether to raise information events</summary>
    public bool RaiseInformationEvents { get; set; } = false;
}

/// <summary>
/// Options for enabling/disabling endpoints
/// </summary>
public class EndpointsOptions
{
    /// <summary>Enables the authorize endpoint</summary>
    public bool EnableAuthorizeEndpoint { get; set; } = true;
    /// <summary>Enables the token endpoint</summary>
    public bool EnableTokenEndpoint { get; set; } = true;
    /// <summary>Enables the user info endpoint</summary>
    public bool EnableUserInfoEndpoint { get; set; } = true;
    /// <summary>Enables the discovery endpoint</summary>
    public bool EnableDiscoveryEndpoint { get; set; } = true;
    /// <summary>Enables the introspection endpoint</summary>
    public bool EnableIntrospectionEndpoint { get; set; } = true;
    /// <summary>Enables the revocation endpoint</summary>
    public bool EnableRevocationEndpoint { get; set; } = true;
    /// <summary>Enables the end session endpoint</summary>
    public bool EnableEndSessionEndpoint { get; set; } = true;
    /// <summary>Enables the check session endpoint</summary>
    public bool EnableCheckSessionEndpoint { get; set; } = true;
    /// <summary>Enables the device authorization endpoint</summary>
    public bool EnableDeviceAuthorizationEndpoint { get; set; } = true;
}

/// <summary>
/// Options for the discovery document
/// </summary>
public class DiscoveryOptions
{
    /// <summary>Whether to show endpoints in the discovery document</summary>
    public bool ShowEndpoints { get; set; } = true;
    /// <summary>Whether to show the keyset in the discovery document</summary>
    public bool ShowKeySet { get; set; } = true;
    /// <summary>Whether to show response types in the discovery document</summary>
    public bool ShowResponseTypes { get; set; } = true;
    /// <summary>Whether to show grant types in the discovery document</summary>
    public bool ShowGrantTypes { get; set; } = true;
    /// <summary>Whether to show token endpoint auth methods in the discovery document</summary>
    public bool ShowTokenEndpointAuthenticationMethods { get; set; } = true;
}

/// <summary>
/// Options for user interaction URLs
/// </summary>
public class UserInteractionOptions
{
    /// <summary>The login URL</summary>
    public string LoginUrl { get; set; } = "/account/login";
    /// <summary>The logout URL</summary>
    public string LogoutUrl { get; set; } = "/account/logout";
    /// <summary>The consent URL</summary>
    public string ConsentUrl { get; set; } = "/consent";
    /// <summary>The error URL</summary>
    public string ErrorUrl { get; set; } = "/error";
    /// <summary>Lifetime of a normal consent in seconds (default 1 hour)</summary>
    public int ConsentLifetime { get; set; } = 3600;
    /// <summary>Lifetime of a remembered consent in seconds (default 1 year)</summary>
    public int RememberConsentLifetime { get; set; } = 31536000;
}

/// <summary>
/// Options for caching
/// </summary>
public class CachingOptions
{
    /// <summary>Expiration for the client store cache in seconds</summary>
    public int ClientStoreExpiration { get; set; } = 3600;
    /// <summary>Expiration for the resource store cache in seconds</summary>
    public int ResourceStoreExpiration { get; set; } = 3600;
}

/// <summary>
/// Options for CORS
/// </summary>
public class CorsOptions
{
    /// <summary>The name of the CORS policy</summary>
    public string CorsPolicyName { get; set; } = "TrustIdentity";
    /// <summary>The list of allowed origins</summary>
    public List<string> AllowedOrigins { get; set; } = new();
}

/// <summary>
/// Options for CSP
/// </summary>
public class CspOptions
{
    /// <summary>Whether CSP is enabled</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>The CSP level to use</summary>
    public string Level { get; set; } = "2";
}

/// <summary>
/// Options for validation
/// </summary>
public class ValidationOptions
{
    /// <summary>Whether to validate the issuer name</summary>
    public bool ValidateIssuerName { get; set; } = true;
}

/// <summary>
/// Options for device flow
/// </summary>
public class DeviceFlowOptions
{
    /// <summary>The length of the user code</summary>
    public int UserCodeLength { get; set; } = 8;
    /// <summary>The type of user code (e.g. Numeric)</summary>
    public string UserCodeType { get; set; } = "Numeric";
    /// <summary>The lifetime of the device code in seconds (default 5 minutes)</summary>
    public int DeviceCodeLifetime { get; set; } = 300;
}

/// <summary>
/// Options for Mutual TLS
/// </summary>
public class MutualTlsOptions
{
    /// <summary>Whether Mutual TLS is enabled</summary>
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// Options for DPoP (Demonstrating Proof-of-Possession)
/// </summary>
public class DPoPOptions
{
    /// <summary>Whether DPoP is enabled</summary>
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// Store for signing credentials
/// </summary>
public class SigningCredentialStore
{
    /// <summary>
    /// The signing certificate
    /// </summary>
    public System.Security.Cryptography.X509Certificates.X509Certificate2 Certificate { get; }

    /// <summary>
    /// Initializes a new instance of the SigningCredentialStore
    /// </summary>
    /// <param name="certificate">The certificate</param>
    public SigningCredentialStore(System.Security.Cryptography.X509Certificates.X509Certificate2 certificate) 
        => Certificate = certificate;
}
