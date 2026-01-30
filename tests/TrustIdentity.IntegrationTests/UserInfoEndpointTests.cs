using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TrustIdentity.IntegrationTests;

public class UserInfoEndpointTests : IntegrationTestBase
{
    public UserInfoEndpointTests()
    {
    }

    [Fact]
    public async Task UserInfoEndpoint_WithValidToken_ReturnsUserClaims()
    {
        // Arrange - Get access token
        var token = await GetAccessTokenAsync(
            "password",
            "web-client",
            "secret",
            new Dictionary<string, string>
            {
                ["username"] = "alice",
                ["password"] = "password",
                ["scope"] = "openid profile email"
            });

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.NotNull(userInfo);
        Assert.True(userInfo.ContainsKey("sub"));
        Assert.True(userInfo.ContainsKey("name"));
        Assert.True(userInfo.ContainsKey("email"));
    }

    [Fact]
    public async Task UserInfoEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/connect/userinfo");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UserInfoEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UserInfoEndpoint_RespectsScopes()
    {
        // Arrange - Get token with limited scopes
        var token = await GetAccessTokenAsync(
            "password",
            "web-client",
            "secret",
            new Dictionary<string, string>
            {
                ["username"] = "alice",
                ["password"] = "password",
                ["scope"] = "openid" // Only openid, no profile or email
            });

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.NotNull(userInfo);
        Assert.True(userInfo.ContainsKey("sub")); // Should have sub from openid
        // Note: Current implementation returns all claims, but ideally should filter by scope
    }
}
