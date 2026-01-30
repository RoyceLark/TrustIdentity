using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Core.Services;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace TrustIdentity.UnitTests;

public class TokenServiceAdvancedTests
{
    private readonly TokenService _service;

    public TokenServiceAdvancedTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SigningKey"] = "your-secret-key-must-be-at-least-32-characters-long",
                ["JwtSettings:Issuer"] = "https://localhost:5001"
            })
            .Build();

        var loggerMock = new Mock<ILogger<TokenService>>();
        var options = new TrustIdentity.Abstractions.Configuration.TrustIdentityOptions
        {
            IssuerUri = "https://localhost:5001"
        };
        _service = new TokenService(config, loggerMock.Object, options);
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsFalse_ForExpiredToken()
    {
        // Arrange
        var client = new Client { ClientId = "https://localhost:5001" };
        var user = new User { SubjectId = "123" };
        var token = await _service.CreateAccessTokenAsync(client, user, new[] { "api1" });
        
        // Create an expired token by setting both times in the past
        token.IssuedAt = DateTime.UtcNow.AddHours(-2);
        token.ExpiresAt = DateTime.UtcNow.AddHours(-1);
        var jwt = await _service.GenerateJwtAsync(token);

        // Act
        var result = await _service.ValidateTokenAsync(jwt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsFalse_ForTamperedToken()
    {
        // Arrange
        var client = new Client { ClientId = "https://localhost:5001" };
        var user = new User { SubjectId = "123" };
        var token = await _service.CreateAccessTokenAsync(client, user, new[] { "api1" });
        var jwt = await _service.GenerateJwtAsync(token);

        // Tamper with the token by changing a character
        var tamperedJwt = jwt.Substring(0, jwt.Length - 5) + "XXXXX";

        // Act
        var result = await _service.ValidateTokenAsync(tamperedJwt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsFalse_ForMalformedToken()
    {
        // Act
        var result = await _service.ValidateTokenAsync("not.a.valid.jwt.token");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GenerateJwtAsync_IncludesAllScopes()
    {
        // Arrange
        var client = new Client { ClientId = "test-client" };
        var user = new User { SubjectId = "123" };
        var scopes = new[] { "openid", "profile", "email", "api1", "api2" };
        var token = await _service.CreateAccessTokenAsync(client, user, scopes);

        // Act
        var jwt = await _service.GenerateJwtAsync(token);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var parsedToken = handler.ReadJwtToken(jwt);
        var scopeClaims = parsedToken.Claims.Where(c => c.Type == "scope").Select(c => c.Value).ToList();
        
        Assert.Equal(5, scopeClaims.Count);
        Assert.Contains("openid", scopeClaims);
        Assert.Contains("profile", scopeClaims);
        Assert.Contains("email", scopeClaims);
        Assert.Contains("api1", scopeClaims);
        Assert.Contains("api2", scopeClaims);
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_HasLongerLifetime()
    {
        // Arrange
        var client = new Client { ClientId = "test-client" };
        var user = new User { SubjectId = "123" };

        // Act
        var accessToken = await _service.CreateAccessTokenAsync(client, user, new[] { "api1" });
        var refreshToken = await _service.CreateRefreshTokenAsync(client, user);

        // Assert
        Assert.True(refreshToken.ExpiresAt > accessToken.ExpiresAt);
        Assert.True((refreshToken.ExpiresAt - refreshToken.IssuedAt).TotalDays >= 30);
    }
}
