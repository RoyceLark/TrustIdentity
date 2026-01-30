using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace TrustIdentity.Saml.Security;

/// <summary>
/// SAML XML Signing and Validation
/// </summary>
public class SamlSigningService
{
    /// <summary>
    /// Sign SAML XML using X509 certificate
    /// </summary>
    public string SignXml(string xml, X509Certificate2 certificate)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        using (var stringReader = new System.IO.StringReader(xml))
        using (var reader = XmlReader.Create(stringReader, settings))
        {
            doc.Load(reader);
        }

        // Create signature
        var signedXml = new SignedXml(doc);
        signedXml.SigningKey = certificate.GetRSAPrivateKey();

        // Add reference to the assertion/response
        var reference = new Reference("");
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigExcC14NTransform());
        reference.Uri = "#" + doc.DocumentElement!.GetAttribute("ID");
        signedXml.AddReference(reference);

        // Add KeyInfo
        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificate));
        signedXml.KeyInfo = keyInfo;

        // Compute signature
        signedXml.ComputeSignature();

        // Get signature XML
        var signatureXml = signedXml.GetXml();

        // Insert signature after Issuer element (SAML 2.0 schema requirement)
        var issuerNode = doc.DocumentElement.SelectSingleNode("*[local-name()='Issuer']");
        if (issuerNode?.NextSibling != null)
        {
            doc.DocumentElement.InsertAfter(doc.ImportNode(signatureXml, true), issuerNode);
        }
        else
        {
            doc.DocumentElement.AppendChild(doc.ImportNode(signatureXml, true));
        }

        return doc.OuterXml;
    }

    /// <summary>
    /// Validate SAML XML signature using a specific certificate
    /// </summary>
    public bool ValidateSignature(string xml, X509Certificate2 certificate)
    {
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            using (var stringReader = new System.IO.StringReader(xml))
            using (var reader = XmlReader.Create(stringReader, settings))
            {
                doc.Load(reader);
            }

            var signedXml = new SignedXml(doc);
            
            // SECURITY: Ensure we only pick the signature that is a direct child of the root element
            // to prevent signature wrapping attacks.
            var signatureNode = doc.DocumentElement!.SelectSingleNode("*[local-name()='Signature' and namespace-uri()='http://www.w3.org/2000/09/xmldsig#']") as XmlElement;
            
            if (signatureNode == null)
                return false;

            signedXml.LoadXml(signatureNode);

            var key = certificate.GetRSAPublicKey();
            if (key == null) return false;

            // Verify that the signature signs the root element
            if (!ValidateReference(signedXml, doc.DocumentElement.GetAttribute("ID")))
                return false;

            return signedXml.CheckSignature(key);
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateReference(SignedXml signedXml, string rootId)
    {
        if (signedXml.SignedInfo?.References == null || signedXml.SignedInfo.References.Count == 0) return false;
        
        var reference = signedXml.SignedInfo.References[0] as Reference;
        if (reference?.Uri == null) return false;

        var referenceId = reference.Uri.StartsWith("#") ? reference.Uri.Substring(1) : reference.Uri;
        
        return string.Equals(referenceId, rootId, StringComparison.Ordinal);
    }
}