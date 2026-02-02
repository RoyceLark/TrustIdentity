using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TrustIdentity.Abstractions.Configuration;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.Core.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly TrustIdentityOptions _options;

        public TokenServiceTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["JwtSettings:SigningKey"]).Returns("12345678901234567890123456789012"); // 32 chars

            _options = new TrustIdentityOptions
            {
                IssuerUri = "https://test.identity",
                Authentication = new AuthenticationOptions
                {
                    AccessTokenLifetime = 3600
                }
            };

            _tokenService = new TokenService(
                _mockConfig.Object,
                NullLogger<TokenService>.Instance,
                _options,
                null // Use symmetric key
            );
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ShouldCreateTokenWithCorrectProperties()
        {
            // Arrange
            var client = new Client { ClientId = "client1", AccessTokenLifetime = 3600 };
            var user = new User { SubjectId = "user1" };
            var scopes = new List<string> { "api1" };

            // Act
            var token = await _tokenService.CreateAccessTokenAsync(client, user, scopes);

            // Assert
            Assert.NotNull(token);
            Assert.Equal("client1", token.Audience);
            Assert.Equal("user1", token.SubjectId);
            Assert.Equal("https://test.identity", token.Issuer);
            Assert.Equal(3600, (token.ExpiresAt - token.IssuedAt).TotalSeconds, 1);
        }

        [Fact]
        public async Task GenerateJwtAsync_ShouldReturnString()
        {
             // Arrange
            var token = new Token 
            {
                Issuer = "https://test.identity",
                Audience = "client1",
                SubjectId = "user1",
                ClientId = "client1",
                Scopes = new List<string> { "api1" },
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            // Act
            var jwt = await _tokenService.GenerateJwtAsync(token);

            // Assert
            Assert.NotNull(jwt);
            Assert.NotEmpty(jwt);
            Assert.Contains(".", jwt); // JWT format
        }
    }
}
