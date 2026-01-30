using System.Security.Cryptography;
using System.Text;
using System;
using System.Linq;

namespace TrustIdentity.Core.Validation;

/// <summary>
/// Validator for Proof Key for Code Exchange (PKCE)
/// </summary>
public class PkceValidator
{
    /// <summary>
    /// Validates a code verifier against a code challenge and method
    /// </summary>
    public bool ValidateCodeChallenge(string codeVerifier, string codeChallenge, string codeChallengeMethod)
    {
        if (string.IsNullOrEmpty(codeVerifier) || string.IsNullOrEmpty(codeChallenge))
            return false;

        string computedChallenge;

        if (codeChallengeMethod == "plain" || string.IsNullOrEmpty(codeChallengeMethod))
        {
            computedChallenge = codeVerifier;
        }
        else if (codeChallengeMethod == "S256")
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            computedChallenge = Base64UrlEncode(hash);
        }
        else
        {
            return false;
        }

        // Use constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedChallenge),
            Encoding.UTF8.GetBytes(codeChallenge));
    }

    private string Base64UrlEncode(byte[] input)
    {
        var output = Convert.ToBase64String(input);
        output = output.Split('=')[0]; // Remove padding
        output = output.Replace('+', '-'); // 62nd char of encoding
        output = output.Replace('/', '_'); // 63rd char of encoding
        return output;
    }

    /// <summary>
    /// Validates if a string is a valid code verifier
    /// </summary>
    /// <param name="codeVerifier">The code verifier string</param>
    /// <returns>True if valid; otherwise false</returns>
    public bool IsValidCodeVerifier(string codeVerifier)
    {
        if (string.IsNullOrEmpty(codeVerifier))
            return false;

        if (codeVerifier.Length < 43 || codeVerifier.Length > 128)
            return false;

        // Must contain only [A-Z] / [a-z] / [0-9] / "-" / "." / "_" / "~"
        return codeVerifier.All(c => 
            char.IsLetterOrDigit(c) || 
            c == '-' || c == '.' || c == '_' || c == '~');
    }
}