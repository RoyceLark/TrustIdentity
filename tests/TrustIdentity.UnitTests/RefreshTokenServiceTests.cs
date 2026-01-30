using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Linq;

namespace TrustIdentity.UnitTests;

public class RefreshTokenServiceTests
{
    private readonly Mock<IPersistedGrantStore> _grantStoreMock;
    private readonly Mock<ILogger<RefreshTokenService>> _loggerMock;
    private readonly RefreshTokenService _service;

    public RefreshTokenServiceTests()
    {
        _grantStoreMock = new Mock<IPersistedGrantStore>();
        _loggerMock = new Mock<ILogger<RefreshTokenService>>();
        var options = new TrustIdentity.Abstractions.Configuration.TrustIdentityOptions();
        _service = new RefreshTokenService(_grantStoreMock.Object, _loggerMock.Object, options);
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_CreatesWithCorrectLifetime()
    {
        // Arrange
        var request = new TokenCreationRequest
        {
            ClientId = "client",
            Subject = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "user1") })),
            ValidatedScopes = new[] { "scope1" },
            SessionId = "session1"
        };

        // Act
        var token = await _service.CreateRefreshTokenAsync(request);

        // Assert
        Assert.NotNull(token);
        Assert.Equal("client", token.ClientId);
        Assert.Equal("user1", token.OriginalSubjectId);
        Assert.Equal("session1", token.SessionId);
        Assert.Equal(2592000, token.Lifetime); // 30 days
        Assert.Equal(request.ValidatedScopes, token.AuthorizedScopes);
    }

    [Fact]
    public async Task StoreRefreshTokenAsync_StoresGrant_ReturnsHandle()
    {
        // Arrange
        var token = new RefreshToken
        {
            ClientId = "client",
            OriginalSubjectId = "user1",
            CreationTime = DateTime.UtcNow,
            Lifetime = 3600
        };

        // Act
        var handle = await _service.StoreRefreshTokenAsync(token);

        // Assert
        Assert.False(string.IsNullOrEmpty(handle));
        _grantStoreMock.Verify(x => x.StoreAsync(It.Is<PersistedGrant>(g =>
            g.Type == "refresh_token" &&
            g.ClientId == "client" &&
            g.Key == handle)), Times.Once);
    }

    [Fact]
    public async Task GetRefreshTokenAsync_ReturnsToken_WhenValid()
    {
        // Arrange
        var handle = "handle";
        var expectedToken = new RefreshToken { ClientId = "client" };
        var grant = new PersistedGrant
        {
            Key = handle,
            Type = "refresh_token",
            Data = System.Text.Json.JsonSerializer.Serialize(expectedToken),
            Expiration = DateTime.UtcNow.AddHours(1)
        };
        _grantStoreMock.Setup(x => x.GetAsync(handle)).ReturnsAsync(grant);

        // Act
        var result = await _service.GetRefreshTokenAsync(handle);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("client", result!.ClientId);
    }

    [Fact]
    public async Task GetRefreshTokenAsync_ReturnsNull_WhenExpired()
    {
        // Arrange
        var handle = "expired";
        var grant = new PersistedGrant
        {
            Key = handle,
            Expiration = DateTime.UtcNow.AddHours(-1)
        };
        _grantStoreMock.Setup(x => x.GetAsync(handle)).ReturnsAsync(grant);

        // Act
        var result = await _service.GetRefreshTokenAsync(handle);

        // Assert
        Assert.Null(result);
        _grantStoreMock.Verify(x => x.RemoveAsync(handle), Times.Once);
    }

    [Fact]
    public async Task GetRefreshTokenAsync_ReturnsNull_WhenConsumed()
    {
        // Arrange
        var handle = "consumed";
        var grant = new PersistedGrant
        {
            Key = handle,
            ConsumedTime = DateTime.UtcNow.AddMinutes(-5)
        };
        _grantStoreMock.Setup(x => x.GetAsync(handle)).ReturnsAsync(grant);

        // Act
        var result = await _service.GetRefreshTokenAsync(handle);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ConsumeRefreshTokenAsync_UpdatesConsumedTime()
    {
        // Arrange
        var handle = "handle";
        var grant = new PersistedGrant { Key = handle };
        _grantStoreMock.Setup(x => x.GetAsync(handle)).ReturnsAsync(grant);

        // Act
        await _service.ConsumeRefreshTokenAsync(handle);

        // Assert
        Assert.NotNull(grant.ConsumedTime);
        _grantStoreMock.Verify(x => x.StoreAsync(grant), Times.Once);
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_CreatesNewToken_PreservesData()
    {
        // Arrange
        var original = new RefreshToken
        {
            ClientId = "client",
            OriginalSubjectId = "user1",
            SessionId = "session1",
            AuthorizedScopes = new List<string> { "scope1" },
            CreationTime = DateTime.UtcNow.AddDays(-1),
            Lifetime = 3600
        };
        var client = new Client { ClientId = "client" };

        // Act
        var newToken = await _service.UpdateRefreshTokenAsync(original, client);

        // Assert
        Assert.NotSame(original, newToken);
        Assert.Equal(original.ClientId, newToken.ClientId);
        Assert.Equal(original.OriginalSubjectId, newToken.OriginalSubjectId);
        Assert.Equal(original.SessionId, newToken.SessionId);
        Assert.Equal(original.AuthorizedScopes, newToken.AuthorizedScopes);
        Assert.True(newToken.CreationTime > original.CreationTime);
    }
}
