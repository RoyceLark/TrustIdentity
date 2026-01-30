using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Configuration;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests;

public class SessionManagementServiceTests
{
    private readonly SessionManagementService _service;
    private readonly Mock<ISessionStore> _sessionStoreMock = new();
    private readonly List<TrustIdentity.Abstractions.Models.UserSession> _sessions = new();

    public SessionManagementServiceTests()
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

        _sessionStoreMock.Setup(s => s.RemoveAsync(It.IsAny<string>()))
            .Callback<string>(id => 
            {
                var existing = _sessions.FirstOrDefault(s => s.SessionId == id);
                if (existing != null) _sessions.Remove(existing);
            })
            .Returns(Task.CompletedTask);

        _service = new SessionManagementService(loggerMock.Object, _sessionStoreMock.Object, options);
    }

    [Fact]
    public async Task CreateSessionAsync_CreatesValidSession()
    {
        // Act
        var sessionId = await _service.CreateSessionAsync("user1", "client1");

        // Assert
        Assert.False(string.IsNullOrEmpty(sessionId));
        
        var session = await _service.GetSessionAsync(sessionId);
        Assert.NotNull(session);
        Assert.Equal("user1", session!.SubjectId);
        Assert.Contains("client1", session.ClientIds);
    }

    [Fact]
    public async Task AddClientToSessionAsync_AddsClient()
    {
        // Arrange
        var sessionId = await _service.CreateSessionAsync("user1", "client1");

        // Act
        await _service.AddClientToSessionAsync(sessionId, "client2");
        var session = await _service.GetSessionAsync(sessionId);

        // Assert
        Assert.NotNull(session);
        Assert.Contains("client1", session!.ClientIds);
        Assert.Contains("client2", session.ClientIds);
    }

    [Fact]
    public async Task GetUserSessionsAsync_ReturnsUserSessions()
    {
        // Arrange
        await _service.CreateSessionAsync("user1", "client1");
        await _service.CreateSessionAsync("user1", "client2");
        await _service.CreateSessionAsync("user2", "client3");

        // Act
        var sessions = await _service.GetUserSessionsAsync("user1");

        // Assert
        Assert.Equal(2, System.Linq.Enumerable.Count(sessions));
    }

    [Fact]
    public async Task RemoveSessionAsync_RemovesSession()
    {
        // Arrange
        var sessionId = await _service.CreateSessionAsync("user1", "client1");

        // Act
        await _service.RemoveSessionAsync(sessionId);
        var session = await _service.GetSessionAsync(sessionId);

        // Assert
        Assert.Null(session);
    }
}
