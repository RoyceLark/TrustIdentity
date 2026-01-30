using System;

namespace TrustIdentity.Licensing;

/// <summary>
/// Authority for licensing cryptographic keys.
/// This class holds the platform's root public key, allowing licenses
/// to be validated using only the License String itself.
/// </summary>
public static class KeyAuthority
{
    /// <summary>
    /// The default public key embedded in the library. 
    /// All TrustIdentity licenses are verified against this key by default.
    /// </summary>
    public const string DefaultPublicKeyXml = @"<RSAKeyValue><Modulus>3BIA9OWBbZ4+hhA3qhew8mgbLKHCxvddJXFiYB0ZA4yKwdJWdRkGwaAqxdbP8vVEsVoqBSy4Ovq5Dg+zTHbuY1v5DuxWUn0DZdzWSDo6KsZ2Po9nE3tSBMnDaUDRvFLH7vNKnALHez8b1ASmXZPXgh9jqpcn6Xx9f0H0mnJrkIUeDYwt3tB8Fag6liTGNvny1wBpR078qLyfk5VWqeki0y9HLj8vSPvSVIaVF6EKPr4aixhDsJStj1gHyKHTLWFBlxOqpJ8zjwWCNFbTCQYKnyMQzeNWw9lIrFxKx2ercDjZtaSHOJ3LRyZqeHqEKii2g129qt3H6aIMSjGTcG/CBQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

    /// <summary>
    /// The private key should be kept in a secure vault or appsettings.
    /// For local development, this can be set in the Licensing Manager configuration.
    /// </summary>
    public const string DefaultPrivateKeyXml = null; 
}
