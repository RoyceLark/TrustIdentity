using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TrustIdentity.UnitTests;

/// <summary>
/// Tests for request validation logic
/// </summary>
public class ValidatorTests
{
    [Theory]
    [InlineData("openid")]
    [InlineData("profile")]
    [InlineData("email")]
    [InlineData("api1")]
    [InlineData("offline_access")]
    public void ValidateScope_ValidScope_ReturnsTrue(string scope)
    {
        // Arrange
        var validScopes = new[] { "openid", "profile", "email", "api1", "offline_access" };

        // Act
        var isValid = ValidateScope(scope, validScopes);

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("invalid_scope")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateScope_InvalidScope_ReturnsFalse(string? scope)
    {
        // Arrange
        var validScopes = new[] { "openid", "profile", "email" };

        // Act
        var isValid = ValidateScope(scope, validScopes);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateScopes_AllValid_ReturnsTrue()
    {
        // Arrange
        var requestedScopes = new[] { "openid", "profile", "email" };
        var validScopes = new[] { "openid", "profile", "email", "api1" };

        // Act
        var isValid = ValidateScopes(requestedScopes, validScopes);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateScopes_ContainsInvalid_ReturnsFalse()
    {
        // Arrange
        var requestedScopes = new[] { "openid", "invalid_scope" };
        var validScopes = new[] { "openid", "profile", "email" };

        // Act
        var isValid = ValidateScopes(requestedScopes, validScopes);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("https://example.com/callback")]
    [InlineData("http://localhost:3000/callback")]
    [InlineData("https://app.example.com/auth/callback")]
    public void ValidateRedirectUri_ValidUri_ReturnsTrue(string redirectUri)
    {
        // Arrange
        var allowedUris = new[]
        {
            "https://example.com/callback",
            "http://localhost:3000/callback",
            "https://app.example.com/auth/callback"
        };

        // Act
        var isValid = ValidateRedirectUri(redirectUri, allowedUris);

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("https://evil.com/callback")]
    [InlineData("http://example.com/callback")] // Different scheme
    [InlineData("")]
    [InlineData(null)]
    public void ValidateRedirectUri_InvalidUri_ReturnsFalse(string? redirectUri)
    {
        // Arrange
        var allowedUris = new[] { "https://example.com/callback" };

        // Act
        var isValid = ValidateRedirectUri(redirectUri, allowedUris);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateRedirectUri_CaseSensitive_ReturnsFalse()
    {
        // Arrange
        var redirectUri = "https://Example.com/callback"; // Different case
        var allowedUris = new[] { "https://example.com/callback" };

        // Act
        var isValid = ValidateRedirectUri(redirectUri, allowedUris);

        // Assert
        Assert.False(isValid); // URIs are case-sensitive
    }

    [Theory]
    [InlineData("authorization_code")]
    [InlineData("client_credentials")]
    [InlineData("password")]
    [InlineData("refresh_token")]
    [InlineData("urn:ietf:params:oauth:grant-type:device_code")]
    public void ValidateGrantType_ValidGrantType_ReturnsTrue(string grantType)
    {
        // Arrange
        var allowedGrantTypes = new[]
        {
            "authorization_code",
            "client_credentials",
            "password",
            "refresh_token",
            "urn:ietf:params:oauth:grant-type:device_code"
        };

        // Act
        var isValid = ValidateGrantType(grantType, allowedGrantTypes);

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("implicit")]
    [InlineData("invalid_grant")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateGrantType_InvalidGrantType_ReturnsFalse(string? grantType)
    {
        // Arrange
        var allowedGrantTypes = new[] { "authorization_code", "client_credentials" };

        // Act
        var isValid = ValidateGrantType(grantType, allowedGrantTypes);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("code")]
    [InlineData("token")]
    [InlineData("id_token")]
    [InlineData("code id_token")]
    [InlineData("code token")]
    public void ValidateResponseType_ValidResponseType_ReturnsTrue(string responseType)
    {
        // Arrange
        var allowedResponseTypes = new[]
        {
            "code",
            "token",
            "id_token",
            "code id_token",
            "code token",
            "id_token token",
            "code id_token token"
        };

        // Act
        var isValid = ValidateResponseType(responseType, allowedResponseTypes);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateClientId_ValidClientId_ReturnsTrue()
    {
        // Arrange
        var clientId = "web-client";

        // Act
        var isValid = ValidateClientId(clientId);

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidateClientId_InvalidClientId_ReturnsFalse(string? clientId)
    {
        // Act
        var isValid = ValidateClientId(clientId);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateState_ValidState_ReturnsTrue()
    {
        // Arrange
        var state = "abc123xyz";

        // Act
        var isValid = ValidateState(state);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateState_TooLong_ReturnsFalse()
    {
        // Arrange
        var state = new string('a', 1000); // Very long state

        // Act
        var isValid = ValidateState(state, maxLength: 500);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateNonce_ValidNonce_ReturnsTrue()
    {
        // Arrange
        var nonce = "random-nonce-value";

        // Act
        var isValid = ValidateNonce(nonce);

        // Assert
        Assert.True(isValid);
    }

    // Helper validation methods (these would typically be in actual validator classes)
    private bool ValidateScope(string? scope, IEnumerable<string> validScopes)
    {
        if (string.IsNullOrWhiteSpace(scope))
            return false;

        return validScopes.Contains(scope);
    }

    private bool ValidateScopes(IEnumerable<string> requestedScopes, IEnumerable<string> validScopes)
    {
        return requestedScopes.All(s => validScopes.Contains(s));
    }

    private bool ValidateRedirectUri(string? redirectUri, IEnumerable<string> allowedUris)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
            return false;

        return allowedUris.Contains(redirectUri, StringComparer.Ordinal);
    }

    private bool ValidateGrantType(string? grantType, IEnumerable<string> allowedGrantTypes)
    {
        if (string.IsNullOrWhiteSpace(grantType))
            return false;

        return allowedGrantTypes.Contains(grantType);
    }

    private bool ValidateResponseType(string? responseType, IEnumerable<string> allowedResponseTypes)
    {
        if (string.IsNullOrWhiteSpace(responseType))
            return false;

        return allowedResponseTypes.Contains(responseType);
    }

    private bool ValidateClientId(string? clientId)
    {
        return !string.IsNullOrWhiteSpace(clientId);
    }

    private bool ValidateState(string? state, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(state))
            return true; // State is optional

        return state.Length <= maxLength;
    }

    private bool ValidateNonce(string? nonce)
    {
        return !string.IsNullOrWhiteSpace(nonce);
    }
}
