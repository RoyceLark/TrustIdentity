using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace TrustIdentity.UnitTests;

/// <summary>
/// Tests for PKCE (Proof Key for Code Exchange) implementation
/// </summary>
public class PkceTests
{
    [Fact]
    public void GenerateCodeVerifier_CreatesValidString()
    {
        // Act
        var verifier = GenerateCodeVerifier();

        // Assert
        Assert.NotNull(verifier);
        Assert.True(verifier.Length >= 43 && verifier.Length <= 128);
        Assert.Matches("^[A-Za-z0-9_-]+$", verifier); // URL-safe characters only
    }

    [Fact]
    public void GenerateCodeChallenge_S256_CreatesValidHash()
    {
        // Arrange
        var verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";

        // Act
        var challenge = GenerateCodeChallenge(verifier, "S256");

        // Assert
        Assert.NotNull(challenge);
        Assert.Equal("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM", challenge);
    }

    [Fact]
    public void GenerateCodeChallenge_Plain_ReturnsVerifier()
    {
        // Arrange
        var verifier = "test-verifier-123";

        // Act
        var challenge = GenerateCodeChallenge(verifier, "plain");

        // Assert
        Assert.Equal(verifier, challenge);
    }

    [Fact]
    public void VerifyCodeChallenge_S256_ValidVerifier_ReturnsTrue()
    {
        // Arrange
        var verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
        var challenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

        // Act
        var isValid = VerifyCodeChallenge(verifier, challenge, "S256");

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyCodeChallenge_S256_InvalidVerifier_ReturnsFalse()
    {
        // Arrange
        var verifier = "wrong-verifier";
        var challenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

        // Act
        var isValid = VerifyCodeChallenge(verifier, challenge, "S256");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyCodeChallenge_Plain_ValidVerifier_ReturnsTrue()
    {
        // Arrange
        var verifier = "test-verifier-123";
        var challenge = "test-verifier-123";

        // Act
        var isValid = VerifyCodeChallenge(verifier, challenge, "plain");

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyCodeChallenge_Plain_InvalidVerifier_ReturnsFalse()
    {
        // Arrange
        var verifier = "wrong-verifier";
        var challenge = "test-verifier-123";

        // Act
        var isValid = VerifyCodeChallenge(verifier, challenge, "plain");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void CodeVerifier_MeetsMinimumLength()
    {
        // Act
        var verifier = GenerateCodeVerifier();

        // Assert
        Assert.True(verifier.Length >= 43, "Code verifier must be at least 43 characters");
    }

    [Fact]
    public void CodeVerifier_MeetsMaximumLength()
    {
        // Act
        var verifier = GenerateCodeVerifier();

        // Assert
        Assert.True(verifier.Length <= 128, "Code verifier must not exceed 128 characters");
    }

    [Fact]
    public void GenerateCodeChallenge_UnsupportedMethod_ThrowsException()
    {
        // Arrange
        var verifier = "test-verifier";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => GenerateCodeChallenge(verifier, "unsupported"));
    }

    // Helper methods (these would typically be in the actual PKCE service)
    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Base64UrlEncode(bytes);
    }

    private string GenerateCodeChallenge(string verifier, string method)
    {
        if (method == "plain")
        {
            return verifier;
        }
        else if (method == "S256")
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(verifier));
            return Base64UrlEncode(hash);
        }
        else
        {
            throw new ArgumentException($"Unsupported code challenge method: {method}");
        }
    }

    private bool VerifyCodeChallenge(string verifier, string challenge, string method)
    {
        var computedChallenge = GenerateCodeChallenge(verifier, method);
        return computedChallenge == challenge;
    }

    private string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
