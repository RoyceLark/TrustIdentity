using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Services;
using TrustIdentity.Core.Models; // For SecretTypes
using Xunit;

using Client = TrustIdentity.Abstractions.Models.Client;
using Secret = TrustIdentity.Abstractions.Models.Secret;

namespace TrustIdentity.UnitTests;

public class ClientServiceTests
{
    private readonly Mock<IClientStore> _clientStoreMock;
    private readonly Mock<ILogger<ClientService>> _loggerMock;
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _clientStoreMock = new Mock<IClientStore>();
        _loggerMock = new Mock<ILogger<ClientService>>();
        _service = new ClientService(_clientStoreMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task FindClientByIdAsync_ReturnsClient()
    {
        // Arrange
        var client = new Client { ClientId = "client1" };
        _clientStoreMock.Setup(x => x.FindClientByIdAsync("client1")).ReturnsAsync(client);

        // Act
        var result = await _service.FindClientByIdAsync("client1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("client1", result!.ClientId);
    }

    [Fact]
    public async Task ValidateClientAsync_ReturnsFalse_WhenDisabled()
    {
        // Arrange
        var client = new Client { ClientId = "client1", Enabled = false };

        // Act
        var result = await _service.ValidateClientAsync(client);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateClientAsync_ReturnsTrue_WhenEnabled()
    {
        // Arrange
        var client = new Client { ClientId = "client1", Enabled = true };

        // Act
        var result = await _service.ValidateClientAsync(client);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateSecretAsync_ReturnsTrue_WhenNotRequired()
    {
        // Arrange
        var client = new Client { RequireClientSecret = false };

        // Act
        var result = await _service.ValidateSecretAsync(client, "any");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateSecretAsync_ReturnsTrue_ForValidSecret()
    {
        // Arrange
        var secret = "secret123";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = Convert.ToBase64String(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(secret)));

        var client = new Client 
        { 
            RequireClientSecret = true,
            ClientSecrets = new List<Secret> 
            { 
                new Secret { Value = hash, Type = SecretTypes.SharedSecret } 
            }
        };

        // Act
        var result = await _service.ValidateSecretAsync(client, secret);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateSecretAsync_ReturnsFalse_ForInvalidSecret()
    {
        // Arrange
        var client = new Client 
        { 
            RequireClientSecret = true,
            ClientSecrets = new List<Secret> 
            { 
                new Secret { Value = "hash", Type = SecretTypes.SharedSecret } 
            }
        };

        // Act
        var result = await _service.ValidateSecretAsync(client, "wrong");

        // Assert
        Assert.False(result);
    }
}
