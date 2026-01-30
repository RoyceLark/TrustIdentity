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

public class TokenServiceTests
{
    private readonly TokenService _service;

    public TokenServiceTests()
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
    public async Task CreateAccessTokenAsync_ReturnsValidTokenObject()
    {
        // Arrange
        var client = new Client { ClientId = "test-client" };
        var user = new User { SubjectId = "123" };
        var scopes = new List<string> { "api1", "openid" };

        // Act
        var token = await _service.CreateAccessTokenAsync(client, user, scopes);

        // Assert
        Assert.NotNull(token);
        Assert.Equal("test-client", token.ClientId);
        Assert.Equal("123", token.SubjectId);
        Assert.Equal(scopes, token.Scopes);
        Assert.Equal("https://localhost:5001", token.Issuer);
    }

    [Fact]
    public async Task GenerateJwtAsync_CreatesValidJwt()
    {
        // Arrange
        var client = new Client { ClientId = "test-client" };
        var user = new User { SubjectId = "123" };
        var scopes = new List<string> { "api1" };
        var token = await _service.CreateAccessTokenAsync(client, user, scopes);

        // Act
        var jwt = await _service.GenerateJwtAsync(token);

        // Assert
        Assert.NotNull(jwt);
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(jwt));
        
        var parsedToken = handler.ReadJwtToken(jwt);
        Assert.Contains(parsedToken.Claims, c => c.Type == "sub" && c.Value == "123");
        Assert.Contains(parsedToken.Claims, c => c.Type == "scope" && c.Value == "api1");
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsTrue_ForValidToken()
    {
        // Arrange
        var user = new User { SubjectId = "123" };
        var scopes = new List<string> { "api1" };
        
        // Note: TokenService validation now uses _issuer for both issuer and audience by default in some configurations,
        // but CreateAccessTokenAsync uses client.ClientId as audience.
        // In our TokenService.ValidateTokenAsync, we set:
        // ValidIssuer = _issuer, ValidAudience = _issuer
        // So the token we generate must have Audience = _issuer to pass validation.
        
        var validClient = new Client { ClientId = "https://localhost:5001" };
        var validToken = await _service.CreateAccessTokenAsync(validClient, user, scopes);
        var validJwt = await _service.GenerateJwtAsync(validToken);
        
        var result = await _service.ValidateTokenAsync(validJwt);

        // Assert
        Assert.True(result);
    }
}
