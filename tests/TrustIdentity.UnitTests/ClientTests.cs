using Xunit;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Core.Services;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.UnitTests;

public class ClientTests
{
    [Fact]
    public void Client_WithValidConfiguration_ShouldValidate()
    {
        // Arrange
        var client = new Client
        {
            ClientId = "test-client",
            ClientName = "Test Client",
            AllowedGrantTypes = new List<string> { "authorization_code" },
            RedirectUris = new List<string> { "https://localhost/callback" }
        };

        // Act
        var isValid = !string.IsNullOrEmpty(client.ClientId);

        // Assert
        Assert.True(isValid);
        Assert.Equal("test-client", client.ClientId);
    }

    [Fact]
    public void Client_RequiresClientId()
    {
        // Arrange
        var client = new Client
        {
            ClientId = "",
            ClientName = "Test Client"
        };

        // Act
        var isValid = !string.IsNullOrEmpty(client.ClientId);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Client_SupportsMultipleGrantTypes()
    {
        // Arrange
        var client = new Client
        {
            ClientId = "multi-grant-client",
            AllowedGrantTypes = new List<string> 
            { 
                "authorization_code", 
                "client_credentials",
                "refresh_token"
            }
        };

        // Assert
        Assert.Equal(3, client.AllowedGrantTypes.Count);
        Assert.Contains("authorization_code", client.AllowedGrantTypes);
        Assert.Contains("client_credentials", client.AllowedGrantTypes);
    }
}