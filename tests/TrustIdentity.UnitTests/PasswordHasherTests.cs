using System.Security.Cryptography;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Core.Security;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace TrustIdentity.UnitTests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher;

    public PasswordHasherTests()
    {
        var loggerMock = new Mock<ILogger<PasswordHasher>>();
        _hasher = new PasswordHasher(loggerMock.Object);
    }

    [Fact]
    public void HashPassword_ReturnsHashedPassword()
    {
        // Arrange
        var user = new User { Username = "test" };
        var password = "password123";

        // Act
        var hash = _hasher.HashPassword(user, password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEqual(password, hash);
        Assert.Contains(".", hash); // PBKDF2 format iterations.salt.hash
        Assert.StartsWith("600000.", hash); // Verifying new iteration count
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_ForCorrectPassword()
    {
        // Arrange
        var user = new User { Username = "test" };
        var password = "password123";
        var hash = _hasher.HashPassword(user, password);
        user.PasswordHash = hash;

        // Act
        var result = _hasher.VerifyPassword(user, password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForIncorrectPassword()
    {
        // Arrange
        var user = new User { Username = "test" };
        var password = "password123";
        var hash = _hasher.HashPassword(user, password);
        user.PasswordHash = hash;

        // Act
        var result = _hasher.VerifyPassword(user, "wrongpassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForNullHash()
    {
        // Arrange
        var user = new User { Username = "test", PasswordHash = null };

        // Act
        var result = _hasher.VerifyPassword(user, "password");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_DoesNotHandleLegacyPlaintextPassword()
    {
        // Arrange
        var user = new User { Username = "test", PasswordHash = "plaintext" };

        // Act
        var result = _hasher.VerifyPassword(user, "plaintext");

        // Assert
        // SECURITY FIX: Legacy plaintext passwords are no longer supported
        Assert.False(result);
    }
}
