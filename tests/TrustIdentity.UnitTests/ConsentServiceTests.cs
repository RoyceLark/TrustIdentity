using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Configuration;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests;

public class ConsentServiceTests
{
    private readonly ConsentService _service;
    private readonly Mock<IConsentStore> _consentStoreMock = new();
    private readonly Mock<IClientStore> _clientStoreMock = new();
    private readonly Mock<IResourceStore> _resourceStoreMock = new();
    private readonly Mock<IApiScopeStore> _apiScopeStoreMock = new();
    private readonly List<UserConsent> _consents = new();

    public ConsentServiceTests()
    {
        var loggerMock = new Mock<ILogger<ConsentService>>();
        var options = new TrustIdentityOptions();

        _consentStoreMock.Setup(s => s.StoreAsync(It.IsAny<UserConsent>()))
            .Callback<UserConsent>(c => 
            {
                var existing = _consents.FirstOrDefault(x => x.SubjectId == c.SubjectId && x.ClientId == c.ClientId);
                if (existing != null) _consents.Remove(existing);
                _consents.Add(c);
            })
            .Returns(Task.CompletedTask);

        _consentStoreMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string sub, string clientId) => 
                _consents.FirstOrDefault(c => c.SubjectId == sub && c.ClientId == clientId));

        _consentStoreMock.Setup(s => s.RemoveAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((sub, clientId) =>
            {
                var existing = _consents.FirstOrDefault(c => c.SubjectId == sub && c.ClientId == clientId);
                if (existing != null) _consents.Remove(existing);
            })
            .Returns(Task.CompletedTask);

        _service = new ConsentService(
            loggerMock.Object, 
            _consentStoreMock.Object, 
            _clientStoreMock.Object,
            _resourceStoreMock.Object,
            _apiScopeStoreMock.Object,
            options);
    }

    [Fact]
    public async Task RequiresConsentAsync_ReturnsFalse_WhenClientDoesNotRequireConsent()
    {
        // Arrange
        var client = new Client { RequireConsent = false };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "user1") }));

        // Act
        var result = await _service.RequiresConsentAsync(client, user, new[] { "openid" });

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RequiresConsentAsync_ReturnsTrue_WhenConsentMissing()
    {
        // Arrange
        var client = new Client { ClientId = "client1", RequireConsent = true };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "user1") }));

        // Act
        var result = await _service.RequiresConsentAsync(client, user, new[] { "openid" });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RequiresConsentAsync_ReturnsFalse_WhenConsentExistsForScopes()
    {
        // Arrange
        var client = new Client { ClientId = "client1", RequireConsent = true };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "user1") }));
        
        await _service.StoreUserConsentAsync(new UserConsent
        {
            SubjectId = "user1",
            ClientId = "client1",
            Scopes = new List<string> { "openid", "profile" }
        });

        // Act
        var result = await _service.RequiresConsentAsync(client, user, new[] { "openid" });

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RequiresConsentAsync_ReturnsTrue_WhenNewScopeRequested()
    {
        // Arrange
        var client = new Client { ClientId = "client1", RequireConsent = true };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "user1") }));
        
        await _service.StoreUserConsentAsync(new UserConsent
        {
            SubjectId = "user1",
            ClientId = "client1",
            Scopes = new List<string> { "openid" }
        });

        // Act
        var result = await _service.RequiresConsentAsync(client, user, new[] { "openid", "profile" });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task StoreAndRetrieveConsent()
    {
        // Arrange
        var consent = new UserConsent
        {
            SubjectId = "u1",
            ClientId = "c1",
            Scopes = new List<string> { "s1" }
        };

        // Act
        await _service.StoreUserConsentAsync(consent);
        var retrieved = await _service.GetUserConsentAsync("u1", "c1");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("u1", retrieved!.SubjectId);
    }

    [Fact]
    public async Task RemoveUserConsentAsync_RemovesConsent()
    {
        // Arrange
        var consent = new UserConsent
        {
            SubjectId = "u1",
            ClientId = "c1"
        };
        await _service.StoreUserConsentAsync(consent);

        // Act
        await _service.RemoveUserConsentAsync("u1", "c1");
        var retrieved = await _service.GetUserConsentAsync("u1", "c1");

        // Assert
        Assert.Null(retrieved);
    }
}
