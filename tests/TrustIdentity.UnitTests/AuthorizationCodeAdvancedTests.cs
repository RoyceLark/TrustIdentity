using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests;

public class AuthorizationCodeAdvancedTests
{
    private readonly Mock<IPersistedGrantStore> _grantStoreMock;
    private readonly Mock<ILogger<AuthorizationCodeService>> _loggerMock;
    private readonly AuthorizationCodeService _service;

    public AuthorizationCodeAdvancedTests()
    {
        _grantStoreMock = new Mock<IPersistedGrantStore>();
        _loggerMock = new Mock<ILogger<AuthorizationCodeService>>();
        _service = new AuthorizationCodeService(_grantStoreMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AuthorizationCode_CannotBeReusedAfterConsumption()
    {
        // Arrange
        var code = new AuthorizationCode
        {
            ClientId = "client",
            SubjectId = "user1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        var codeValue = await _service.CreateAuthorizationCodeAsync(code);

        // Setup mock to return the code first, then null after consumption
        var grant = new PersistedGrant
        {
            Key = codeValue,
            Type = "authorization_code",
            Data = System.Text.Json.JsonSerializer.Serialize(code),
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };

        _grantStoreMock.SetupSequence(x => x.GetAsync(codeValue))
            .ReturnsAsync(grant)
            .ReturnsAsync((PersistedGrant?)null);

        // Act - First retrieval should succeed
        var firstRetrieval = await _service.GetAuthorizationCodeAsync(codeValue);
        
        // Consume the code
        await _service.ConsumeAuthorizationCodeAsync(codeValue);
        
        // Second retrieval should fail
        var secondRetrieval = await _service.GetAuthorizationCodeAsync(codeValue);

        // Assert
        Assert.NotNull(firstRetrieval);
        Assert.Null(secondRetrieval);
        _grantStoreMock.Verify(x => x.RemoveAsync(codeValue), Times.Once);
    }

    [Fact]
    public async Task AuthorizationCode_GeneratesUniqueCodeEachTime()
    {
        // Arrange
        var code1 = new AuthorizationCode
        {
            ClientId = "client",
            SubjectId = "user1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        var code2 = new AuthorizationCode
        {
            ClientId = "client",
            SubjectId = "user1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        var codeValue1 = await _service.CreateAuthorizationCodeAsync(code1);
        var codeValue2 = await _service.CreateAuthorizationCodeAsync(code2);

        // Assert
        Assert.NotEqual(codeValue1, codeValue2);
    }

    [Fact]
    public async Task AuthorizationCode_HasCorrectLifetime()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var expiresAt = createdAt.AddMinutes(5);
        
        var code = new AuthorizationCode
        {
            ClientId = "client",
            SubjectId = "user1",
            CreatedAt = createdAt,
            ExpiresAt = expiresAt
        };

        // Act
        var codeValue = await _service.CreateAuthorizationCodeAsync(code);

        // Assert
        _grantStoreMock.Verify(x => x.StoreAsync(It.Is<PersistedGrant>(g =>
            g.CreationTime == createdAt &&
            g.Expiration == expiresAt)), Times.Once);
    }

    [Fact]
    public async Task RemoveAuthorizationCodeAsync_CallsConsumeAuthorizationCodeAsync()
    {
        // Arrange
        var codeValue = "test-code";

        // Act
        await _service.RemoveAuthorizationCodeAsync(codeValue);

        // Assert
        _grantStoreMock.Verify(x => x.RemoveAsync(codeValue), Times.Once);
    }
}
