using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TrustIdentity.IntegrationTests;

public class TokenEndpointTests : IntegrationTestBase
{
    public TokenEndpointTests()
    {
    }

    [Fact]
    public async Task TokenEndpoint_ClientCredentials_ReturnsAccessToken()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "api-client",
            ["client_secret"] = "secret",
            ["scope"] = "api1"
        };

        // Act
        var response = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);
        
        Assert.NotNull(tokenResponse);
        Assert.NotNull(tokenResponse.access_token);
        Assert.Equal("Bearer", tokenResponse.token_type);
        Assert.True(tokenResponse.expires_in > 0);
    }

    [Fact]
    public async Task TokenEndpoint_Password_ReturnsAccessToken()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "web-client",
            ["client_secret"] = "secret",
            ["username"] = "alice",
            ["password"] = "password",
            ["scope"] = "openid profile"
        };

        // Act
        var response = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);
        
        Assert.NotNull(tokenResponse);
        Assert.NotNull(tokenResponse.access_token);
        Assert.NotNull(tokenResponse.refresh_token);
    }

    [Fact]
    public async Task TokenEndpoint_InvalidClient_ReturnsError()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "invalid-client",
            ["client_secret"] = "wrong-secret"
        };

        // Act
        var response = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(json);
        
        Assert.NotNull(errorResponse);
        Assert.Equal("invalid_client", errorResponse.error);
    }

    [Fact]
    public async Task TokenEndpoint_InvalidGrant_ReturnsError()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "web-client",
            ["client_secret"] = "secret",
            ["username"] = "alice",
            ["password"] = "wrong-password"
        };

        // Act
        var response = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(json);
        
        Assert.NotNull(errorResponse);
        Assert.Equal("invalid_grant", errorResponse.error);
    }

    [Fact]
    public async Task TokenEndpoint_UnsupportedGrantType_ReturnsError()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "unsupported_grant",
            ["client_id"] = "api-client",
            ["client_secret"] = "secret"
        };

        // Act
        var response = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(json);
        
        Assert.NotNull(errorResponse);
        Assert.Equal("unsupported_grant_type", errorResponse.error);
    }

    [Fact]
    public async Task TokenEndpoint_MissingParameters_ReturnsError()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
            // Missing client_id and client_secret
        };

        // Act
        var response = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TokenEndpoint_RefreshToken_ReturnsNewAccessToken()
    {
        // Arrange - First get a refresh token
        var initialFormData = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "web-client",
            ["client_secret"] = "secret",
            ["username"] = "alice",
            ["password"] = "password",
            ["scope"] = "openid offline_access"
        };

        var initialResponse = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(initialFormData));
        var initialJson = await initialResponse.Content.ReadAsStringAsync();
        var initialToken = JsonSerializer.Deserialize<TokenResponse>(initialJson);

        // Act - Use refresh token
        var refreshFormData = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = "web-client",
            ["client_secret"] = "secret",
            ["refresh_token"] = initialToken!.refresh_token!
        };

        var refreshResponse = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(refreshFormData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        
        var refreshJson = await refreshResponse.Content.ReadAsStringAsync();
        var newToken = JsonSerializer.Deserialize<TokenResponse>(refreshJson);
        
        Assert.NotNull(newToken);
        Assert.NotNull(newToken.access_token);
        Assert.NotEqual(initialToken.access_token, newToken.access_token);
    }
}
