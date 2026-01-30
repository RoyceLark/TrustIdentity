using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using TrustIdentity.Saml.Models;
using TrustIdentity.Saml.Serialization;
using TrustIdentity.Saml.Security;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace TrustIdentity.Saml.Services;

/// <summary>
/// SAML Identity Provider - Issues SAML assertions to Service Providers
/// </summary>
public class SamlIdentityProvider
{
    private readonly SamlSerializer _serializer;
    private readonly SamlSigningService _signingService;
    private readonly ILogger<SamlIdentityProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the SamlIdentityProvider
    /// </summary>
    /// <param name="serializer">The SAML serializer</param>
    /// <param name="signingService">The SAML signing service</param>
    /// <param name="logger">The logger instance</param>
    public SamlIdentityProvider(
        SamlSerializer serializer,
        SamlSigningService signingService,
        ILogger<SamlIdentityProvider> logger)
    {
        _serializer = serializer;
        _signingService = signingService;
        _logger = logger;
    }

    /// <summary>
    /// Process SAML Authentication Request from Service Provider
    /// </summary>
    /// <param name="samlRequest">The base64 encoded SAML request</param>
    /// <returns>The deserialized SAML AuthnRequest or null if processing fails</returns>
    public SamlAuthnRequest? ProcessAuthnRequest(string samlRequest)
    {
        try
        {
            // Decode Base64
            var xmlBytes = Convert.FromBase64String(samlRequest);
            var xml = System.Text.Encoding.UTF8.GetString(xmlBytes);

            _logger.LogDebug("Processing SAML AuthnRequest");

            return _serializer.DeserializeAuthnRequest(xml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SAML AuthnRequest");
            return null;
        }
    }

    /// <summary>
    /// Create SAML Response with assertion for authenticated user
    /// </summary>
    /// <param name="user">The authenticated user claims principal</param>
    /// <param name="authnRequest">The original authentication request</param>
    /// <param name="config">The IdP configuration</param>
    /// <returns>The serialized (and optionally signed) SAML response XML</returns>
    public string CreateResponse(
        ClaimsPrincipal user,
        SamlAuthnRequest authnRequest,
        SamlIdentityProviderConfig config)
    {
        // Create assertion
        var assertion = new SamlAssertion
        {
            Issuer = config.EntityId,
            Subject = new SamlSubject
            {
                NameId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value ?? "",
                NameIdFormat = SamlConstants.NameIdFormats.EmailAddress,
                SubjectConfirmation = new SamlSubjectConfirmation
                {
                    Method = SamlConstants.SubjectConfirmationMethods.Bearer,
                    SubjectConfirmationData = new SamlSubjectConfirmationData
                    {
                        NotOnOrAfter = DateTime.UtcNow.AddMinutes(5),
                        Recipient = authnRequest.AssertionConsumerServiceURL,
                        InResponseTo = authnRequest.Id
                    }
                }
            },
            Conditions = new SamlConditions
            {
                NotBefore = DateTime.UtcNow,
                NotOnOrAfter = DateTime.UtcNow.AddMinutes(5),
                AudienceRestrictions = new List<SamlAudienceRestriction>
                {
                    new SamlAudienceRestriction
                    {
                        Audiences = new List<string> { authnRequest.Issuer }
                    }
                }
            }
        };

        // Add attributes from user claims
        var attributeStatement = new SamlAttributeStatement();
        
        foreach (var claim in user.Claims)
        {
            var attribute = new SamlAttribute
            {
                Name = claim.Type,
                NameFormat = SamlConstants.AttributeNameFormats.Uri,
                AttributeValues = new List<string> { claim.Value }
            };
            attributeStatement.Attributes.Add(attribute);
        }

        assertion.AttributeStatements.Add(attributeStatement);

        // Add authentication statement
        assertion.AuthnStatements.Add(new SamlAuthnStatement
        {
            AuthnInstant = DateTime.UtcNow,
            SessionIndex = Guid.NewGuid().ToString("N"),
            SessionNotOnOrAfter = DateTime.UtcNow.AddHours(8),
            AuthnContext = new SamlAuthnContext
            {
                AuthnContextClassRef = SamlConstants.AuthnContextClasses.PasswordProtectedTransport
            }
        });

        // Create response
        var response = new SamlResponse
        {
            Issuer = config.EntityId,
            Status = new SamlStatus
            {
                StatusCode = SamlConstants.StatusCodes.Success
            },
            Assertions = new List<SamlAssertion> { assertion },
            InResponseTo = authnRequest.Id,
            Destination = authnRequest.AssertionConsumerServiceURL
        };

        // Serialize
        var xml = _serializer.SerializeResponse(response);

        // Sign if certificate is provided
        if (config.SigningCertificate != null)
        {
            xml = _signingService.SignXml(xml, config.SigningCertificate);
        }

        return xml;
    }

    /// <summary>
    /// Create SAML Response for authentication failure
    /// </summary>
    /// <param name="authnRequest">The original authentication request</param>
    /// <param name="errorMessage">The error message to include</param>
    /// <param name="config">The IdP configuration</param>
    /// <returns>The serialized SAML error response XML</returns>
    public string CreateErrorResponse(
        SamlAuthnRequest authnRequest,
        string errorMessage,
        SamlIdentityProviderConfig config)
    {
        var response = new SamlResponse
        {
            Issuer = config.EntityId,
            Status = new SamlStatus
            {
                StatusCode = SamlConstants.StatusCodes.AuthnFailed,
                StatusMessage = errorMessage
            },
            InResponseTo = authnRequest.Id,
            Destination = authnRequest.AssertionConsumerServiceURL
        };

        var xml = _serializer.SerializeResponse(response);

        if (config.SigningCertificate != null)
        {
            xml = _signingService.SignXml(xml, config.SigningCertificate);
        }

        return xml;
    }

    /// <summary>
    /// Generate SAML Metadata for Identity Provider
    /// </summary>
    /// <param name="config">The IdP configuration</param>
    /// <returns>The serialized IdP metadata XML</returns>
    public string GenerateMetadata(SamlIdentityProviderConfig config)
    {
        var metadata = new SamlIdentityProviderMetadata
        {
            EntityId = config.EntityId,
            SingleSignOnServices = new List<SamlSingleSignOnService>
            {
                new SamlSingleSignOnService
                {
                    Binding = SamlConstants.ProtocolBindings.HttpPost,
                    Location = config.SingleSignOnServiceUrl
                },
                new SamlSingleSignOnService
                {
                    Binding = SamlConstants.ProtocolBindings.HttpRedirect,
                    Location = config.SingleSignOnServiceUrl
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

    private string SerializeMetadata(SamlIdentityProviderMetadata metadata)
    {
        var doc = new XmlDocument();
        var entityDescriptor = doc.CreateElement("EntityDescriptor", "urn:oasis:names:tc:SAML:2.0:metadata");
        entityDescriptor.SetAttribute("entityID", metadata.EntityId);

        var idpSsoDescriptor = doc.CreateElement("IDPSSODescriptor", "urn:oasis:names:tc:SAML:2.0:metadata");
        idpSsoDescriptor.SetAttribute("protocolSupportEnumeration", "urn:oasis:names:tc:SAML:2.0:protocol");

        foreach (var ssoService in metadata.SingleSignOnServices)
        {
            var ssoElement = doc.CreateElement("SingleSignOnService", "urn:oasis:names:tc:SAML:2.0:metadata");
            ssoElement.SetAttribute("Binding", ssoService.Binding);
            ssoElement.SetAttribute("Location", ssoService.Location);
            idpSsoDescriptor.AppendChild(ssoElement);
        }

        foreach (var keyDescriptor in metadata.KeyDescriptors)
        {
            var keyDescriptorElement = doc.CreateElement("KeyDescriptor", "urn:oasis:names:tc:SAML:2.0:metadata");
            keyDescriptorElement.SetAttribute("use", keyDescriptor.Use);
            
            var keyInfoElement = doc.CreateElement("ds", "KeyInfo", "http://www.w3.org/2000/09/xmldsig#");
            var x509DataElement = doc.CreateElement("ds", "X509Data", "http://www.w3.org/2000/09/xmldsig#");
            var x509CertificateElement = doc.CreateElement("ds", "X509Certificate", "http://www.w3.org/2000/09/xmldsig#");
            x509CertificateElement.InnerText = keyDescriptor.X509Certificate;
            
            x509DataElement.AppendChild(x509CertificateElement);
            keyInfoElement.AppendChild(x509DataElement);
            keyDescriptorElement.AppendChild(keyInfoElement);
            idpSsoDescriptor.AppendChild(keyDescriptorElement);
        }

        entityDescriptor.AppendChild(idpSsoDescriptor);
        doc.AppendChild(entityDescriptor);

        return doc.OuterXml;
    }
}

/// <summary>
/// SAML Identity Provider Configuration
/// </summary>
public class SamlIdentityProviderConfig
{
    /// <summary>
    /// Entity ID of the Identity Provider
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Single Sign-On Service URL
    /// </summary>
    public string SingleSignOnServiceUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Single Logout Service URL
    /// </summary>
    public string SingleLogoutServiceUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Certificate used for signing assertions and responses
    /// </summary>
    public X509Certificate2? SigningCertificate { get; set; }
    
    /// <summary>
    /// Sign assertions
    /// </summary>
    public bool SignAssertions { get; set; } = true;
    
    /// <summary>
    /// Sign responses
    /// </summary>
    public bool SignResponses { get; set; } = true;
    
    /// <summary>
    /// Lifetime of issued assertions
    /// </summary>
    public TimeSpan AssertionLifetime { get; set; } = TimeSpan.FromMinutes(5);
}