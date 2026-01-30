using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TrustIdentity.IntegrationTests;

public class RevocationEndpointTests : IntegrationTestBase
{
    public RevocationEndpointTests()
    {
    }

    [Fact]
    public async Task RevocationEndpoint_WithValidToken_ReturnsSuccess()
    {
        // Arrange - Get a token to revoke
        var token = await GetAccessTokenAsync(
            "client_credentials",
            "api-client",
            "secret",
            new Dictionary<string, string>
            {
                ["scope"] = "api1"
            });

        var formData = new Dictionary<string, string>
        {
            ["token"] = token,
            ["client_id"] = "api-client",
            ["client_secret"] = "secret"
        };

        // Act
        var response = await Client.PostAsync("/connect/revoke", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RevocationEndpoint_WithRefreshToken_ReturnsSuccess()
    {
        // Arrange - Get a refresh token
        var formData1 = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "web-client",
            ["client_secret"] = "secret",
            ["username"] = "alice",
            ["password"] = "password",
            ["scope"] = "openid offline_access"
        };

        var tokenResponse = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(formData1));
        var json = await tokenResponse.Content.ReadAsStringAsync();
        var token = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(json);

        var formData2 = new Dictionary<string, string>
        {
            ["token"] = token!.refresh_token!,
            ["token_type_hint"] = "refresh_token",
            ["client_id"] = "web-client",
            ["client_secret"] = "secret"
        };

        // Act
        var response = await Client.PostAsync("/connect/revoke", new FormUrlEncodedContent(formData2));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RevocationEndpoint_WithInvalidToken_ReturnsSuccess()
    {
        // Arrange - Revocation should succeed even for invalid tokens (per spec)
        var formData = new Dictionary<string, string>
        {
            ["token"] = "invalid-token",
            ["client_id"] = "api-client",
            ["client_secret"] = "secret"
        };

        // Act
        var response = await Client.PostAsync("/connect/revoke", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RevocationEndpoint_WithoutClientAuth_ReturnsBadRequest()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["token"] = "some-token"
            // Missing client credentials
        };

        // Act
        var response = await Client.PostAsync("/connect/revoke", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RevocationEndpoint_RevokedToken_CannotBeUsed()
    {
        // Arrange - Get and revoke a token
        var token = await GetAccessTokenAsync(
            "client_credentials",
            "api-client",
            "secret",
            new Dictionary<string, string>
            {
                ["scope"] = "api1"
            });

        var revokeFormData = new Dictionary<string, string>
        {
            ["token"] = token,
            ["client_id"] = "api-client",
            ["client_secret"] = "secret"
        };

        await Client.PostAsync("/connect/revoke", new FormUrlEncodedContent(revokeFormData));

        // Act - Try to introspect the revoked token
        var introspectFormData = new Dictionary<string, string>
        {
            ["token"] = token,
            ["client_id"] = "api-client",
            ["client_secret"] = "secret"
        };

        var response = await Client.PostAsync("/connect/introspect", new FormUrlEncodedContent(introspectFormData));
        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(json);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("active"));
        Assert.False(result["active"].GetBoolean()); // Should be inactive after revocation
    }
}
