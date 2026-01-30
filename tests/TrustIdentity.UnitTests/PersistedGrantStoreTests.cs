using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests;

public class PersistedGrantStoreTests
{
    private readonly InMemoryPersistedGrantStore _store;

    public PersistedGrantStoreTests()
    {
        _store = new InMemoryPersistedGrantStore();
    }

    [Fact]
    public async Task StoreAsync_StoresGrant()
    {
        // Arrange
        var grant = new PersistedGrant
        {
            Key = "test-key",
            Type = "authorization_code",
            SubjectId = "user1",
            ClientId = "client1",
            CreationTime = DateTime.UtcNow,
            Expiration = DateTime.UtcNow.AddMinutes(5),
            Data = "{}"
        };

        // Act
        await _store.StoreAsync(grant);
        var retrieved = await _store.GetAsync("test-key");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("test-key", retrieved!.Key);
        Assert.Equal("authorization_code", retrieved.Type);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var result = await _store.GetAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_RemovesGrant()
    {
        // Arrange
        var grant = new PersistedGrant
        {
            Key = "test-key",
            Type = "authorization_code",
            Data = "{}"
        };
        await _store.StoreAsync(grant);

        // Act
        await _store.RemoveAsync("test-key");
        var retrieved = await _store.GetAsync("test-key");

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetAllAsync_FiltersBySubjectId()
    {
        // Arrange
        await _store.StoreAsync(new PersistedGrant
        {
            Key = "key1",
            Type = "authorization_code",
            SubjectId = "user1",
            Data = "{}"
        });
        await _store.StoreAsync(new PersistedGrant
        {
            Key = "key2",
            Type = "refresh_token",
            SubjectId = "user1",
            Data = "{}"
        });
        await _store.StoreAsync(new PersistedGrant
        {
            Key = "key3",
            Type = "authorization_code",
            SubjectId = "user2",
            Data = "{}"
        });

        // Act
        var results = await _store.GetAllAsync("user1");

        // Assert
        Assert.Equal(2, System.Linq.Enumerable.Count(results));
        Assert.All(results, g => Assert.Equal("user1", g.SubjectId));
    }

    [Fact]
    public async Task RemoveAllAsync_RemovesMatchingGrants()
    {
        // Arrange
        await _store.StoreAsync(new PersistedGrant
        {
            Key = "key1",
            Type = "authorization_code",
            SubjectId = "user1",
            ClientId = "client1",
            Data = "{}"
        });
        await _store.StoreAsync(new PersistedGrant
        {
            Key = "key2",
            Type = "refresh_token",
            SubjectId = "user1",
            ClientId = "client1",
            Data = "{}"
        });
        await _store.StoreAsync(new PersistedGrant
        {
            Key = "key3",
            Type = "authorization_code",
            SubjectId = "user2",
            ClientId = "client1",
            Data = "{}"
        });

        // Act
        await _store.RemoveAllAsync("user1", "client1");
        var remainingForUser1 = await _store.GetAllAsync("user1");
        var remainingForUser2 = await _store.GetAllAsync("user2");

        // Assert
        Assert.Empty(remainingForUser1);
        Assert.Single(remainingForUser2);
    }

    [Fact]
    public async Task StoreAsync_UpdatesExistingGrant()
    {
        // Arrange
        var grant = new PersistedGrant
        {
            Key = "test-key",
            Type = "authorization_code",
            Data = "original"
        };
        await _store.StoreAsync(grant);

        // Act
        grant.Data = "updated";
        await _store.StoreAsync(grant);
        var retrieved = await _store.GetAsync("test-key");

        // Assert
        Assert.Equal("updated", retrieved!.Data);
    }
}
