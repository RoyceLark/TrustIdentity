using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TrustIdentity.IntegrationTests;

public class DiscoveryEndpointTests : IntegrationTestBase
{
    public DiscoveryEndpointTests()
    {
    }

    [Fact]
    public async Task DiscoveryEndpoint_ReturnsConfiguration()
    {
        // Act
        var response = await Client.GetAsync("/.well-known/openid-configuration");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var json = await response.Content.ReadAsStringAsync();
        var config = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.True(config.TryGetProperty("issuer", out _));
        Assert.True(config.TryGetProperty("authorization_endpoint", out _));
        Assert.True(config.TryGetProperty("token_endpoint", out _));
        Assert.True(config.TryGetProperty("userinfo_endpoint", out _));
        Assert.True(config.TryGetProperty("jwks_uri", out _));
    }

    [Fact]
    public async Task DiscoveryEndpoint_IncludesSupportedGrantTypes()
    {
        // Act
        var response = await Client.GetAsync("/.well-known/openid-configuration");
        var json = await response.Content.ReadAsStringAsync();
        var config = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.True(config.TryGetProperty("grant_types_supported", out var grantTypes));
        
        var grantTypesArray = grantTypes.EnumerateArray();
        var grantTypesList = new System.Collections.Generic.List<string>();
        
        foreach (var gt in grantTypesArray)
        {
            grantTypesList.Add(gt.GetString()!);
        }
        
        Assert.Contains("authorization_code", grantTypesList);
        Assert.Contains("client_credentials", grantTypesList);
        Assert.Contains("password", grantTypesList);
        Assert.Contains("refresh_token", grantTypesList);
    }

    [Fact]
    public async Task DiscoveryEndpoint_IncludesSupportedScopes()
    {
        // Act
        var response = await Client.GetAsync("/.well-known/openid-configuration");
        var json = await response.Content.ReadAsStringAsync();
        var config = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.True(config.TryGetProperty("scopes_supported", out var scopes));
        
        var scopesArray = scopes.EnumerateArray();
        var scopesList = new System.Collections.Generic.List<string>();
        
        foreach (var scope in scopesArray)
        {
            scopesList.Add(scope.GetString()!);
        }
        
        Assert.Contains("openid", scopesList);
        Assert.Contains("profile", scopesList);
        Assert.Contains("email", scopesList);
    }

    [Fact]
    public async Task DiscoveryEndpoint_IncludesResponseTypesSupported()
    {
        // Act
        var response = await Client.GetAsync("/.well-known/openid-configuration");
        var json = await response.Content.ReadAsStringAsync();
        var config = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.True(config.TryGetProperty("response_types_supported", out var responseTypes));
        
        var responseTypesArray = responseTypes.EnumerateArray();
        var responseTypesList = new System.Collections.Generic.List<string>();
        
        foreach (var rt in responseTypesArray)
        {
            responseTypesList.Add(rt.GetString()!);
        }
        
        Assert.Contains("code", responseTypesList);
        Assert.Contains("token", responseTypesList);
    }
}
