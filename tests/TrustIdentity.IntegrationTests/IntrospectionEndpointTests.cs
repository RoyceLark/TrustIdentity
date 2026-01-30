using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TrustIdentity.IntegrationTests;

public class IntrospectionEndpointTests : IntegrationTestBase
{
    public IntrospectionEndpointTests()
    {
    }

    [Fact]
    public async Task IntrospectionEndpoint_WithValidToken_ReturnsActive()
    {
        // Arrange - Get a valid token
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
        var response = await Client.PostAsync("/connect/introspect", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("active"));
        Assert.True(result["active"].GetBoolean());
    }

    [Fact]
    public async Task IntrospectionEndpoint_WithInvalidToken_ReturnsInactive()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["token"] = "invalid-token",
            ["client_id"] = "api-client",
            ["client_secret"] = "secret"
        };

        // Act
        var response = await Client.PostAsync("/connect/introspect", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("active"));
        Assert.False(result["active"].GetBoolean());
    }

    [Fact]
    public async Task IntrospectionEndpoint_WithoutClientAuth_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["token"] = "some-token"
            // Missing client credentials
        };

        // Act
        var response = await Client.PostAsync("/connect/introspect", new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
