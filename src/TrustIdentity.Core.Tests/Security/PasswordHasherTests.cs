using System;
using Microsoft.Extensions.Logging.Abstractions;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Core.Security;
using Xunit;

namespace TrustIdentity.Core.Tests.Security
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher;

        public PasswordHasherTests()
        {
            _hasher = new PasswordHasher(NullLogger<PasswordHasher>.Instance);
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedString()
        {
            // Arrange
            var user = new User { SubjectId = "test_user" };
            var password = "TestPassword123!";

            // Act
            var hash = _hasher.HashPassword(user, password);

            // Assert
            Assert.NotNull(hash);
            Assert.Contains(".", hash); // Should contain salt delimiter
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
        {
            // Arrange
            var user = new User { SubjectId = "test_user" };
            var password = "TestPassword123!";
            
            // Act
            var hash = _hasher.HashPassword(user, password);
            user.PasswordHash = hash;
            var result = _hasher.VerifyPassword(user, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
        {
            // Arrange
            var user = new User { SubjectId = "test_user" };
            var password = "TestPassword123!";
            var wrongPassword = "WrongPassword123!";
            
            // Act
            var hash = _hasher.HashPassword(user, password);
            user.PasswordHash = hash;
            var result = _hasher.VerifyPassword(user, wrongPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenHashIsEmpty()
        {
             // Arrange
            var user = new User { SubjectId = "test_user" };
            var password = "TestPassword123!";
            
            // Act - No password hash set
            var result = _hasher.VerifyPassword(user, password);

            // Assert
            Assert.False(result);
        }
    }
}
