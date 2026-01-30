using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests;

public class InMemoryStoreTests
{
    [Fact]
    public async Task InMemoryClientStore_FindClientByIdAsync_ReturnsClient()
    {
        // Arrange
        var clients = new List<Client>
        {
            new Client { ClientId = "client1", ClientName = "Test Client" }
        };
        var store = new InMemoryClientStore(clients);

        // Act
        var result = await store.FindClientByIdAsync("client1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("client1", result!.ClientId);
    }

    [Fact]
    public async Task InMemoryClientStore_FindClientByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var store = new InMemoryClientStore(new List<Client>());

        // Act
        var result = await store.FindClientByIdAsync("unknown");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InMemoryUserStore_FindByUsernameAsync_ReturnsUser()
    {
        // Arrange
        var users = new List<TestUser>
        {
            new TestUser { SubjectId = "1", Username = "alice", Password = "password" }
        };
        var store = new InMemoryUserStore(users);

        // Act
        var result = await store.FindByUsernameAsync("alice");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("alice", result!.Username);
        Assert.Equal("1", result.SubjectId);
        // Verify mapping fix
        Assert.Equal("password", result.PasswordHash);
    }

    [Fact]
    public async Task InMemoryUserStore_ValidateCredentialsAsync_ReturnsTrue_ForCorrectCredentials()
    {
        // Arrange
        var users = new List<TestUser>
        {
            new TestUser { SubjectId = "1", Username = "alice", Password = "password" }
        };
        
        var hasherMock = new Moq.Mock<TrustIdentity.Abstractions.Stores.IPasswordHasher>();
        hasherMock.Setup(h => h.VerifyPassword(Moq.It.IsAny<User>(), "password"))
                  .Returns(true);
        
        var store = new InMemoryUserStore(users, hasherMock.Object);

        // Act
        var result = await store.ValidateCredentialsAsync("alice", "password");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task InMemoryUserStore_ValidateCredentialsAsync_ReturnsFalse_ForInvalidPassword()
    {
        // Arrange
        var users = new List<TestUser>
        {
            new TestUser { SubjectId = "1", Username = "alice", Password = "password" }
        };
        
        var hasherMock = new Moq.Mock<TrustIdentity.Abstractions.Stores.IPasswordHasher>();
        hasherMock.Setup(h => h.VerifyPassword(Moq.It.IsAny<User>(), "wrong"))
                  .Returns(false);

        var store = new InMemoryUserStore(users, hasherMock.Object);

        // Act
        var result = await store.ValidateCredentialsAsync("alice", "wrong");

        // Assert
        Assert.False(result);
    }
}
