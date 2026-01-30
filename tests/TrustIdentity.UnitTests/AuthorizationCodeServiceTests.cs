using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests;

public class AuthorizationCodeServiceTests
{
    private readonly Mock<IPersistedGrantStore> _grantStoreMock;
    private readonly Mock<ILogger<AuthorizationCodeService>> _loggerMock;
    private readonly AuthorizationCodeService _service;

    public AuthorizationCodeServiceTests()
    {
        _grantStoreMock = new Mock<IPersistedGrantStore>();
        _loggerMock = new Mock<ILogger<AuthorizationCodeService>>();
        _service = new AuthorizationCodeService(_grantStoreMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAuthorizationCodeAsync_StoresGrantAndReturnsCode()
    {
        // Arrange
        var code = new AuthorizationCode
        {
            ClientId = "client",
            SubjectId = "sub",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        var result = await _service.CreateAuthorizationCodeAsync(code);

        // Assert
        Assert.NotNull(result);
        _grantStoreMock.Verify(x => x.StoreAsync(It.Is<PersistedGrant>(g => 
            g.Type == "authorization_code" && 
            g.ClientId == "client" && 
            g.Key == result)), Times.Once);
    }

    [Fact]
    public async Task GetAuthorizationCodeAsync_ReturnsCode_WhenFound()
    {
        // Arrange
        var codeKey = "testcode";
        var expectedCode = new AuthorizationCode
        {
            ClientId = "client",
            SubjectId = "sub"
        };
        var grant = new PersistedGrant
        {
            Key = codeKey,
            Type = "authorization_code",
            Data = System.Text.Json.JsonSerializer.Serialize(expectedCode),
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };

        _grantStoreMock.Setup(x => x.GetAsync(codeKey)).ReturnsAsync(grant);

        // Act
        var result = await _service.GetAuthorizationCodeAsync(codeKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("client", result!.ClientId);
    }

    [Fact]
    public async Task GetAuthorizationCodeAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _grantStoreMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((PersistedGrant?)null);

        // Act
        var result = await _service.GetAuthorizationCodeAsync("invalid");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAuthorizationCodeAsync_ReturnsNull_WhenExpired()
    {
        // Arrange
        var codeKey = "expired";
        var grant = new PersistedGrant
        {
            Key = codeKey,
            Type = "authorization_code",
            Expiration = DateTime.UtcNow.AddMinutes(-5)
        };

        _grantStoreMock.Setup(x => x.GetAsync(codeKey)).ReturnsAsync(grant);

        // Act
        var result = await _service.GetAuthorizationCodeAsync(codeKey);

        // Assert
        Assert.Null(result);
        _grantStoreMock.Verify(x => x.RemoveAsync(codeKey), Times.Once);
    }

    [Fact]
    public async Task ConsumeAuthorizationCodeAsync_RemovesGrant()
    {
        // Arrange
        var codeKey = "code";

        // Act
        await _service.ConsumeAuthorizationCodeAsync(codeKey);

        // Assert
        _grantStoreMock.Verify(x => x.RemoveAsync(codeKey), Times.Once);
    }
}
