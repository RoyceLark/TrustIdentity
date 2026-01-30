using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Configuration;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests;

public class SessionManagementAdvancedTests
{
    private readonly SessionManagementService _service;
    private readonly Mock<ISessionStore> _sessionStoreMock = new();
    private readonly List<TrustIdentity.Abstractions.Models.UserSession> _sessions = new();

    public SessionManagementAdvancedTests()
    {
        var loggerMock = new Mock<ILogger<SessionManagementService>>();
        var options = new TrustIdentityOptions();

        _sessionStoreMock.Setup(s => s.StoreAsync(It.IsAny<TrustIdentity.Abstractions.Models.UserSession>()))
            .Callback<TrustIdentity.Abstractions.Models.UserSession>(s => 
            {
                var existing = _sessions.FirstOrDefault(x => x.SessionId == s.SessionId);
                if (existing != null) _sessions.Remove(existing);
                _sessions.Add(s);
            })
            .Returns(Task.CompletedTask);

        _sessionStoreMock.Setup(s => s.GetAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _sessions.FirstOrDefault(s => s.SessionId == id));

        _sessionStoreMock.Setup(s => s.GetUserSessionsAsync(It.IsAny<string>()))
            .ReturnsAsync((string sub) => _sessions.Where(s => s.SubjectId == sub));

        _service = new SessionManagementService(loggerMock.Object, _sessionStoreMock.Object, options);
    }

    [Fact]
    public async Task GetSessionAsync_ReturnsSession_WhenNotExpired()
    {
        // Arrange
        var sessionId = await _service.CreateSessionAsync("user1", "client1");

        // Act
        var session = await _service.GetSessionAsync(sessionId);

        // Assert
        Assert.NotNull(session);
        Assert.Equal("user1", session!.SubjectId);
        Assert.True(session.Expires > DateTime.UtcNow);
    }

    [Fact]
    public async Task AddClientToSessionAsync_DoesNotDuplicateClients()
    {
        // Arrange
        var sessionId = await _service.CreateSessionAsync("user1", "client1");

        // Act
        await _service.AddClientToSessionAsync(sessionId, "client1");
        await _service.AddClientToSessionAsync(sessionId, "client1");
        var session = await _service.GetSessionAsync(sessionId);

        // Assert
        Assert.Single(session!.ClientIds);
    }

    [Fact]
    public async Task CreateSessionAsync_GeneratesUniqueSessionIds()
    {
        // Act
        var sessionId1 = await _service.CreateSessionAsync("user1", "client1");
        var sessionId2 = await _service.CreateSessionAsync("user1", "client1");

        // Assert
        Assert.NotEqual(sessionId1, sessionId2);
    }

    [Fact]
    public async Task GetUserSessionsAsync_ReturnsAllActiveSessions()
    {
        // Arrange
        var sessionId1 = await _service.CreateSessionAsync("user1", "client1");
        var sessionId2 = await _service.CreateSessionAsync("user1", "client2");
        await _service.CreateSessionAsync("user2", "client3"); // Different user

        // Act
        var activeSessions = await _service.GetUserSessionsAsync("user1");

        // Assert
        Assert.Equal(2, activeSessions.Count());
        Assert.Contains(activeSessions, s => s.SessionId == sessionId1);
        Assert.Contains(activeSessions, s => s.SessionId == sessionId2);
    }

    [Fact]
    public async Task SessionHasCorrectDefaultLifetime()
    {
        // Act
        var sessionId = await _service.CreateSessionAsync("user1", "client1");
        var session = await _service.GetSessionAsync(sessionId);

        // Assert
        var lifetime = session!.Expires - session.Created;
        Assert.True(lifetime.TotalHours >= 1); // Default is 1 hour (3600s)
    }
}
