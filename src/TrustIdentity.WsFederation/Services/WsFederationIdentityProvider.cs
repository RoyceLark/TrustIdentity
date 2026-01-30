using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using TrustIdentity.WsFederation.Models;
using TrustIdentity.WsFederation.Security;
namespace TrustIdentity.WsFederation.Services;

/// <summary>
/// WS-Federation Identity Provider Service
/// </summary>
public class WsFederationIdentityProvider
{
    private WsTrustSecurityTokenService _tokenService;
    private ILogger<WsFederationIdentityProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the WsFederationIdentityProvider
    /// </summary>
    /// <param name="tokenService">The WS-Trust security token service</param>
    /// <param name="logger">The logger instance</param>
    public WsFederationIdentityProvider(
        WsTrustSecurityTokenService tokenService,
        ILogger<WsFederationIdentityProvider> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Process WS-Federation Sign-In Request
    /// </summary>
    /// <param name="request">The sign-in request details</param>
    /// <param name="user">The authenticated user principal</param>
    /// <param name="config">The WS-Federation configuration</param>
    /// <returns>A sign-in response containing the issued token</returns>
    public WsFederationSignInResponse ProcessSignInRequest(
        WsFederationSignInRequest request,
        ClaimsPrincipal user,
        WsFederationConfiguration config)
    {
        try
        {
            _logger.LogInformation("Processing WS-Federation sign-in for realm: {Realm}", request.Wtrealm);

            // Validate realm
            if (string.IsNullOrEmpty(request.Wtrealm))
            {
                throw new WsFederationException("wtrealm parameter is required");
            }

            if (!config.AllowedRealms.Contains(request.Wtrealm))
            {
                throw new WsFederationException($"Realm {request.Wtrealm} is not authorized");
            }

            // Create security token
            var token = _tokenService.IssueToken(new RequestSecurityToken
            {
                AppliesTo = request.Wtrealm,
                TokenType = WsTrustConstants.TokenTypes.Saml20
            }, user, config);

            // Create sign-in response
            var response = new WsFederationSignInResponse
            {
                Wa = WsFederationConstants.Actions.SignIn,
                Wresult = token,
                Wctx = request.Wctx
            };

            _logger.LogInformation("Successfully created WS-Federation response for user: {User}", 
                user.Identity?.Name);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WS-Federation sign-in request");
            throw;
        }
    }

    /// <summary>
    /// Generate Sign-In Request URL (for IdP-initiated flow)
    /// </summary>
    /// <param name="realm">The target realm</param>
    /// <param name="reply">Optional reply URL</param>
    /// <param name="context">Optional context string</param>
    /// <returns>The generated sign-in URL</returns>
    public string GenerateSignInUrl(string realm, string? reply = null, string? context = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            [WsFederationConstants.Parameters.Action] = WsFederationConstants.Actions.SignIn,
            [WsFederationConstants.Parameters.Realm] = realm
        };

        if (!string.IsNullOrEmpty(reply))
            queryParams[WsFederationConstants.Parameters.Reply] = reply;

        if (!string.IsNullOrEmpty(context))
            queryParams[WsFederationConstants.Parameters.Context] = context;

        var query = string.Join("&", queryParams.Select(kvp => 
            $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));

        return $"?{query}";
    }

    /// <summary>
    /// Generate metadata XML
    /// </summary>
    /// <param name="config">The WS-Federation configuration</param>
    /// <returns>The serialized metadata XML string</returns>
    public string GenerateMetadata(WsFederationConfiguration config)
    {
        var metadata = new WsFederationMetadata
        {
            EntityId = config.Issuer,
            PassiveRequestorEndpoints = new List<string> { config.PassiveRequestorEndpoint },
            SecurityTokenServiceEndpoints = new List<string> { config.SecurityTokenServiceEndpoint }
        };

        if (config.SigningCertificate != null)
        {
            metadata.KeyDescriptors.Add(new WsFederationKeyDescriptor
            {
                Use = "signing",
                X509Certificate = Convert.ToBase64String(config.SigningCertificate.Export(X509ContentType.Cert))
            });
        }

        // Add common claim types
        metadata.ClaimTypes.AddRange(new[]
        {
            new WsFederationClaimType 
            { 
                Uri = ClaimTypes.Name, 
                DisplayName = "Name" 
            },
            new WsFederationClaimType 
            { 
                Uri = ClaimTypes.Email, 
                DisplayName = "Email" 
            },
            new WsFederationClaimType 
            { 
                Uri = AdfsConstants.ClaimTypes.Upn, 
                DisplayName = "UPN" 
            }
        });

        return SerializeMetadata(metadata);
    }

    private string SerializeMetadata(WsFederationMetadata metadata)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<EntityDescriptor entityID=""{metadata.EntityId}"" 
    xmlns=""urn:oasis:names:tc:SAML:2.0:metadata""
    xmlns:fed=""http://docs.oasis-open.org/wsfed/federation/200706"">
  <RoleDescriptor xsi:type=""fed:SecurityTokenServiceType""
      protocolSupportEnumeration=""http://docs.oasis-open.org/wsfed/federation/200706""
      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <fed:PassiveRequestorEndpoint>
      <EndpointReference xmlns=""http://www.w3.org/2005/08/addressing"">
        <Address>{metadata.PassiveRequestorEndpoints[0]}</Address>
      </EndpointReference>
    </fed:PassiveRequestorEndpoint>
  </RoleDescriptor>
</EntityDescriptor>";
    }
}

/// <summary>
/// WS-Federation Configuration
/// </summary>
public class WsFederationConfiguration
{
    /// <summary>The issuer identifier</summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>The passive requestor endpoint URL</summary>
    public string PassiveRequestorEndpoint { get; set; } = string.Empty;
    /// <summary>The security token service endpoint URL</summary>
    public string SecurityTokenServiceEndpoint { get; set; } = string.Empty;
    /// <summary>The certificate used for signing tokens</summary>
    public X509Certificate2? SigningCertificate { get; set; }
    /// <summary>The list of allowed realms (audience restrictions)</summary>
    public List<string> AllowedRealms { get; set; } = new();
    /// <summary>The token lifetime duration</summary>
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(1);
}

/// <summary>
/// WS-Federation Exception
/// </summary>
public class WsFederationException : Exception
{
    /// <summary>Initializes a new instance of the WsFederationException</summary>
    /// <param name="message">The message</param>
    public WsFederationException(string message) : base(message) { }
    /// <summary>Initializes a new instance of the WsFederationException with inner exception</summary>
    /// <param name="message">The message</param>
    /// <param name="inner">The inner exception</param>
    public WsFederationException(string message, Exception inner) : base(message, inner) { }
}