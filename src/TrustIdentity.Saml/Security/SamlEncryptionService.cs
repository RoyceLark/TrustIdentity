using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace TrustIdentity.Saml.Security;

/// <summary>
/// SAML Assertion Encryption Service - AES-256 with RSA-OAEP
/// </summary>
public class SamlEncryptionService
{
    /// <summary>
    /// Encrypt SAML assertion using recipient's public key
    /// </summary>
    public string EncryptAssertion(string assertionXml, X509Certificate2 recipientCertificate)
    {
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            var assertionDoc = new XmlDocument();
            assertionDoc.PreserveWhitespace = true;
            using (var stringReader = new System.IO.StringReader(assertionXml))
            using (var reader = XmlReader.Create(stringReader, settings))
            {
                assertionDoc.Load(reader);
            }

            // Create symmetric encryption key (AES-GCM or AES-256-CBC)
            // Note: XmlEncryption standard usually uses AES-CBC with PKCS7 or ISO10126.
            // ISO10126 is used by many legacy SAML 2.0 implementations but PKCS7 is safer.
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7; // Use PKCS7 instead of ISO10126 for better security
            aes.GenerateKey();
            aes.GenerateIV();

            // Encrypt the assertion with AES
            var encryptedData = new EncryptedData();
            encryptedData.Type = EncryptedXml.XmlEncElementUrl;
            encryptedData.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES256Url);

            // Create encrypted XML
            var encryptedXml = new EncryptedXml();
            var encryptedElement = encryptedXml.EncryptData(assertionDoc.DocumentElement!, aes, false);
            encryptedData.CipherData.CipherValue = encryptedElement;

            // Encrypt the AES key with recipient's RSA public key using OAEP
            var encryptedKey = new EncryptedKey();
            var rsa = recipientCertificate.GetRSAPublicKey();
            if (rsa == null) throw new SamlEncryptionException("Recipient certificate does not have an RSA public key");
            
            // Enforce RSA-OAEP (useOAEP: true)
            var encryptedKeyValue = EncryptedXml.EncryptKey(aes.Key, rsa, true);
            
            encryptedKey.CipherData = new CipherData(encryptedKeyValue);
            encryptedKey.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSAOAEPUrl);

            // Add key info
            var keyInfo = new KeyInfo();
            var keyInfoName = new KeyInfoName();
            keyInfoName.Value = recipientCertificate.Subject;
            keyInfo.AddClause(keyInfoName);
            encryptedKey.KeyInfo = keyInfo;

            encryptedData.KeyInfo = new KeyInfo();
            encryptedData.KeyInfo.AddClause(new KeyInfoEncryptedKey(encryptedKey));

            // Create EncryptedAssertion element
            return CreateEncryptedAssertionXml(encryptedData);
        }
        catch (Exception ex) when (ex is not SamlEncryptionException)
        {
            throw new SamlEncryptionException("Failed to encrypt SAML assertion", ex);
        }
    }

    /// <summary>
    /// Decrypt SAML assertion using private key
    /// </summary>
    public string DecryptAssertion(string encryptedAssertionXml, X509Certificate2 recipientCertificate)
    {
        try
        {
            if (!recipientCertificate.HasPrivateKey)
                throw new SamlEncryptionException("Certificate does not contain a private key for decryption");

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            using (var stringReader = new System.IO.StringReader(encryptedAssertionXml))
            using (var reader = XmlReader.Create(stringReader, settings))
            {
                doc.Load(reader);
            }

            var encryptedXml = new EncryptedXml(doc);
            
            // Add key mapping
            var rsa = recipientCertificate.GetRSAPrivateKey();
            if (rsa == null)
                throw new SamlEncryptionException("Unable to retrieve RSA private key from certificate");

            encryptedXml.AddKeyNameMapping(recipientCertificate.Subject, rsa);

            // Decrypt
            encryptedXml.DecryptDocument();

            return doc.OuterXml;
        }
        catch (Exception ex) when (ex is not SamlEncryptionException)
        {
            throw new SamlEncryptionException("Failed to decrypt SAML assertion", ex);
        }
    }

    private string CreateEncryptedAssertionXml(EncryptedData encryptedData)
    {
        var doc = new XmlDocument();
        var encryptedAssertion = doc.CreateElement("saml", "EncryptedAssertion", "urn:oasis:names:tc:SAML:2.0:assertion");
        
        var encryptedDataElement = encryptedData.GetXml();
        encryptedAssertion.AppendChild(doc.ImportNode(encryptedDataElement, true));
        
        return encryptedAssertion.OuterXml;
    }
}

/// <summary>
/// Exception thrown for SAML encryption/decryption errors
/// </summary>
public class SamlEncryptionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the SamlEncryptionException
    /// </summary>
    /// <param name="message">The exception message</param>
    public SamlEncryptionException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the SamlEncryptionException with inner exception
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="inner">The inner exception</param>
    public SamlEncryptionException(string message, Exception inner) : base(message, inner) { }
}