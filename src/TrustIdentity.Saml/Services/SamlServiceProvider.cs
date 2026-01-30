using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using TrustIdentity.Saml.Models;
using TrustIdentity.Saml.Serialization;
using TrustIdentity.Saml.Security;
using Microsoft.Extensions.Logging;

namespace TrustIdentity.Saml.Services;

/// <summary>
/// SAML Service Provider - Consumes SAML assertions from Identity Providers
/// </summary>
public class SamlServiceProvider
{
    private readonly SamlSerializer _serializer;
    private readonly SamlSigningService _signingService;
    private readonly ILogger<SamlServiceProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the SamlServiceProvider
    /// </summary>
    /// <param name="serializer">The SAML serializer</param>
    /// <param name="signingService">The SAML signing service</param>
    /// <param name="logger">The logger instance</param>
    public SamlServiceProvider(
        SamlSerializer serializer,
        SamlSigningService signingService,
        ILogger<SamlServiceProvider> logger)
    {
        _serializer = serializer;
        _signingService = signingService;
        _logger = logger;
    }

    /// <summary>
    /// Create SAML Authentication Request (SP-initiated SSO)
    /// </summary>
    /// <param name="config">The SP configuration</param>
    /// <returns>The serialized (and optionally signed) SAML AuthnRequest XML</returns>
    public string CreateAuthnRequest(SamlServiceProviderConfig config)
    {
        var request = new SamlAuthnRequest
        {
            Issuer = config.EntityId,
            AssertionConsumerServiceURL = config.AssertionConsumerServiceUrl,
            ProtocolBinding = SamlConstants.ProtocolBindings.HttpPost,
            NameIdPolicy = new SamlNameIdPolicy
            {
                Format = SamlConstants.NameIdFormats.EmailAddress,
                AllowCreate = true
            }
        };

        var xml = _serializer.SerializeAuthnRequest(request);

        // Sign if certificate is provided
        if (config.SigningCertificate != null)
        {
            xml = _signingService.SignXml(xml, config.SigningCertificate);
        }

        return xml;
    }

    /// <summary>
    /// Process SAML Response from Identity Provider
    /// </summary>
    /// <param name="samlResponse">The base64 encoded SAML response</param>
    /// <param name="config">The SP configuration</param>
    /// <returns>A validation result containing claims if successful</returns>
    public SamlValidationResult ProcessResponse(string samlResponse, SamlServiceProviderConfig config)
    {
        try
        {
            // Decode Base64
            var xmlBytes = Convert.FromBase64String(samlResponse);
            var xml = System.Text.Encoding.UTF8.GetString(xmlBytes);

            _logger.LogDebug("Processing SAML Response");

            // Validate signature if required
            if (config.RequireSignedAssertion && config.IdpCertificate != null)
            {
                if (!_signingService.ValidateSignature(xml, config.IdpCertificate))
                {
                    _logger.LogWarning("SAML Response signature validation failed");
                    return new SamlValidationResult
                    {
                        IsValid = false,
                        Error = "Invalid signature"
                    };
                }
            }

            // Deserialize and validate
            var response = DeserializeResponse(xml);
            if (response == null)
            {
                return new SamlValidationResult
                {
                    IsValid = false,
                    Error = "Failed to deserialize SAML response"
                };
            }

            // Validate status
            if (response.Status.StatusCode != SamlConstants.StatusCodes.Success)
            {
                return new SamlValidationResult
                {
                    IsValid = false,
                    Error = $"SAML authentication failed: {response.Status.StatusMessage}"
                };
            }

            // Extract claims from assertions
            var claims = ExtractClaims(response);

            return new SamlValidationResult
            {
                IsValid = true,
                Claims = claims,
                NameId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SAML response");
            return new SamlValidationResult
            {
                IsValid = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Generate SAML Metadata for Service Provider
    /// </summary>
    /// <param name="config">The SP configuration</param>
    /// <returns>The serialized SP metadata XML</returns>
    public string GenerateMetadata(SamlServiceProviderConfig config)
    {
        var metadata = new SamlServiceProviderMetadata
        {
            EntityId = config.EntityId,
            AssertionConsumerServices = new List<SamlAssertionConsumerService>
            {
                new SamlAssertionConsumerService
                {
                    Binding = SamlConstants.ProtocolBindings.HttpPost,
                    Location = config.AssertionConsumerServiceUrl,
                    Index = 0,
                    IsDefault = true
                }
            }
        };

        if (config.SigningCertificate != null)
        {
            metadata.KeyDescriptors.Add(new SamlKeyDescriptor
            {
                Use = "signing",
                X509Certificate = Convert.ToBase64String(config.SigningCertificate.Export(X509ContentType.Cert))
            });
        }

        return SerializeMetadata(metadata);
    }

    private List<Claim> ExtractClaims(SamlResponse response)
    {
        var claims = new List<Claim>();

        foreach (var assertion in response.Assertions)
        {
            // Add NameID as claim
            claims.Add(new Claim(ClaimTypes.NameIdentifier, assertion.Subject.NameId));

            // Add attributes as claims
            foreach (var attrStatement in assertion.AttributeStatements)
            {
                foreach (var attr in attrStatement.Attributes)
                {
                    var claimType = MapAttributeToClaimType(attr.Name);
                    foreach (var value in attr.AttributeValues)
                    {
                        claims.Add(new Claim(claimType, value));
                    }
                }
            }
        }

        return claims;
    }

    private string MapAttributeToClaimType(string attributeName)
    {
        return attributeName.ToLower() switch
        {
            "email" or "emailaddress" => ClaimTypes.Email,
            "name" or "displayname" => ClaimTypes.Name,
            "givenname" or "firstname" => ClaimTypes.GivenName,
            "surname" or "lastname" => ClaimTypes.Surname,
            "upn" or "userprincipalname" => ClaimTypes.Upn,
            "role" or "roles" => ClaimTypes.Role,
            _ => attributeName
        };
    }

    private SamlResponse? DeserializeResponse(string xml)
    {
        // Implementation would parse XML into SamlResponse object
        // Simplified for brevity
        return new SamlResponse();
    }

    private string SerializeMetadata(SamlServiceProviderMetadata metadata)
    {
        // Implementation would generate SAML metadata XML
        // Simplified for brevity
        return $@"<EntityDescriptor entityID=""{metadata.EntityId}"" 
                    xmlns=""urn:oasis:names:tc:SAML:2.0:metadata"">
                    <SPSSODescriptor protocolSupportEnumeration=""urn:oasis:names:tc:SAML:2.0:protocol"">
                      <AssertionConsumerService Binding=""{metadata.AssertionConsumerServices[0].Binding}"" 
                        Location=""{metadata.AssertionConsumerServices[0].Location}"" index=""0"" isDefault=""true""/>
                    </SPSSODescriptor>
                  </EntityDescriptor>";
    }
}

/// <summary>
/// SAML Service Provider Configuration
/// </summary>
public class SamlServiceProviderConfig
{
    /// <summary>
    /// Entity ID of the Service Provider
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// URL where SAML Assertions are consumed
    /// </summary>
    public string AssertionConsumerServiceUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Single Sign-On URL of the Identity Provider
    /// </summary>
    public string IdentityProviderSsoUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Certificate used for signing requests
    /// </summary>
    public X509Certificate2? SigningCertificate { get; set; }
    
    /// <summary>
    /// Certificate of the Identity Provider
    /// </summary>
    public X509Certificate2? IdpCertificate { get; set; }
    
    /// <summary>
    /// Require signed assertions from IdP
    /// </summary>
    public bool RequireSignedAssertion { get; set; } = true;
    
    /// <summary>
    /// Require encrypted assertions from IdP
    /// </summary>
    public bool RequireEncryptedAssertion { get; set; } = false;
}

/// <summary>
/// SAML Validation Result
/// </summary>
public class SamlValidationResult
{
    /// <summary>
    /// Validation success status
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? Error { get; set; }
    
    /// <summary>
    /// NameID from the assertion
    /// </summary>
    public string? NameId { get; set; }
    
    /// <summary>
    /// Extracted claims from the assertion
    /// </summary>
    public List<Claim> Claims { get; set; } = new();
}