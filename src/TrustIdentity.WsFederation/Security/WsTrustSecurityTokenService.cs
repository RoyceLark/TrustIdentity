using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System;
using Microsoft.IdentityModel.Tokens;
using TrustIdentity.WsFederation.Models;
using TrustIdentity.WsFederation.Services;

namespace TrustIdentity.WsFederation.Security;

/// <summary>
/// WS-Trust Security Token Service - Issues security tokens
/// </summary>
public class WsTrustSecurityTokenService
{
    /// <summary>
    /// Issue security token based on RST
    /// </summary>
    /// <param name="request">The request security token (RST) details</param>
    /// <param name="user">The authenticated user principal</param>
    /// <param name="config">The WS-Federation configuration</param>
    /// <returns>A serialized RequestSecurityTokenResponse (RSTR) containing the issued token</returns>
    public string IssueToken(
        RequestSecurityToken request,
        ClaimsPrincipal user,
        Services.WsFederationConfiguration config)
    {
        var tokenType = request.TokenType ?? WsTrustConstants.TokenTypes.Saml20;

        return tokenType switch
        {
            WsTrustConstants.TokenTypes.Saml20 => IssueSaml20Token(request, user, config),
            WsTrustConstants.TokenTypes.Saml11 => IssueSaml11Token(request, user, config),
            WsTrustConstants.TokenTypes.Jwt => IssueJwtToken(request, user, config),
            _ => IssueSaml20Token(request, user, config)
        };
    }

    private string IssueSaml20Token(
        RequestSecurityToken request,
        ClaimsPrincipal user,
        WsFederationConfiguration config)
    {
        var now = DateTime.UtcNow;
        var expires = now.Add(config.TokenLifetime);

        var assertion = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<saml:Assertion xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion"" 
    ID=""_{Guid.NewGuid():N}"" 
    Version=""2.0"" 
    IssueInstant=""{now:yyyy-MM-ddTHH:mm:ssZ}"">
  <saml:Issuer>{config.Issuer}</saml:Issuer>
  <saml:Subject>
    <saml:NameID Format=""urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress"">
      {user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value}
    </saml:NameID>
    <saml:SubjectConfirmation Method=""urn:oasis:names:tc:SAML:2.0:cm:bearer"">
      <saml:SubjectConfirmationData NotOnOrAfter=""{expires:yyyy-MM-ddTHH:mm:ssZ}"" 
          Recipient=""{request.AppliesTo}""/>
    </saml:SubjectConfirmation>
  </saml:Subject>
  <saml:Conditions NotBefore=""{now:yyyy-MM-ddTHH:mm:ssZ}"" 
      NotOnOrAfter=""{expires:yyyy-MM-ddTHH:mm:ssZ}"">
    <saml:AudienceRestriction>
      <saml:Audience>{request.AppliesTo}</saml:Audience>
    </saml:AudienceRestriction>
  </saml:Conditions>
  <saml:AttributeStatement>
    {GenerateAttributes(user)}
  </saml:AttributeStatement>
  <saml:AuthnStatement AuthnInstant=""{now:yyyy-MM-ddTHH:mm:ssZ}"">
    <saml:AuthnContext>
      <saml:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport</saml:AuthnContextClassRef>
    </saml:AuthnContext>
  </saml:AuthnStatement>
</saml:Assertion>";

        // Sign if certificate provided
        if (config.SigningCertificate != null)
        {
            assertion = SignXml(assertion, config.SigningCertificate);
        }

        // Wrap in RSTR
        return WrapInRequestSecurityTokenResponse(assertion, request.AppliesTo ?? "");
    }

    private string IssueSaml11Token(
        RequestSecurityToken request,
        ClaimsPrincipal user,
        WsFederationConfiguration config)
    {
        // SAML 1.1 implementation (similar structure, different namespace)
        return IssueSaml20Token(request, user, config);
    }

    private string IssueJwtToken(
        RequestSecurityToken request,
        ClaimsPrincipal user,
        Services.WsFederationConfiguration config)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new X509SecurityKey(config.SigningCertificate!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(user.Claims),
            Expires = System.DateTime.UtcNow.Add(config.TokenLifetime),
            Issuer = config.Issuer,
            Audience = request.AppliesTo,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return WrapInRequestSecurityTokenResponse(jwt, request.AppliesTo ?? "");
    }

    private string GenerateAttributes(ClaimsPrincipal user)
    {
        var attributes = new System.Text.StringBuilder();
        
        foreach (var claim in user.Claims)
        {
            var attributeName = MapClaimTypeToAttributeName(claim.Type);
            attributes.AppendLine($@"
    <saml:Attribute Name=""{attributeName}"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:uri"">
      <saml:AttributeValue>{System.Security.SecurityElement.Escape(claim.Value)}</saml:AttributeValue>
    </saml:Attribute>");
        }

        return attributes.ToString();
    }

    private string MapClaimTypeToAttributeName(string claimType)
    {
        return claimType switch
        {
            ClaimTypes.Name => AdfsConstants.ClaimTypes.Name,
            ClaimTypes.Email => AdfsConstants.ClaimTypes.Email,
            ClaimTypes.GivenName => AdfsConstants.ClaimTypes.GivenName,
            ClaimTypes.Surname => AdfsConstants.ClaimTypes.Surname,
            ClaimTypes.Upn => AdfsConstants.ClaimTypes.Upn,
            ClaimTypes.Role => AdfsConstants.ClaimTypes.Role,
            _ => claimType
        };
    }

    private string SignXml(string xml, X509Certificate2 certificate)
    {
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.LoadXml(xml);

        var signedXml = new SignedXml(doc);
        signedXml.SigningKey = certificate.GetRSAPrivateKey();

        var reference = new Reference("");
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigExcC14NTransform());
        reference.Uri = "#" + doc.DocumentElement!.GetAttribute("ID");
        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificate));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();
        var signatureXml = signedXml.GetXml();

        var issuerNode = doc.DocumentElement.SelectSingleNode("*[local-name()='Issuer']");
        if (issuerNode?.NextSibling != null)
        {
            doc.DocumentElement.InsertAfter(doc.ImportNode(signatureXml, true), issuerNode);
        }

        return doc.OuterXml;
    }

    private string WrapInRequestSecurityTokenResponse(string token, string appliesTo)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<t:RequestSecurityTokenResponse xmlns:t=""http://docs.oasis-open.org/ws-sx/ws-trust/200512"">
  <t:TokenType>urn:oasis:names:tc:SAML:2.0:assertion</t:TokenType>
  <t:RequestedSecurityToken>
    {token}
  </t:RequestedSecurityToken>
  <wsp:AppliesTo xmlns:wsp=""http://schemas.xmlsoap.org/ws/2004/09/policy"">
    <wsa:EndpointReference xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
      <wsa:Address>{appliesTo}</wsa:Address>
    </wsa:EndpointReference>
  </wsp:AppliesTo>
</t:RequestSecurityTokenResponse>";
    }
}