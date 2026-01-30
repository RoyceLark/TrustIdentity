using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests;

public class ClaimsServiceTests
{
    private readonly ClaimsService _service;

    public ClaimsServiceTests()
    {
        var loggerMock = new Mock<ILogger<ClaimsService>>();
        _service = new ClaimsService(loggerMock.Object);
    }

    [Fact]
    public async Task GetClaimsForScopeAsync_OpenId_ReturnsSub()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", "123")
        }));

        // Act
        var claims = await _service.GetClaimsForScopeAsync("openid", user);

        // Assert
        var claim = Assert.Single(claims);
        Assert.Equal("sub", claim.Type);
        Assert.Equal("123", claim.Value);
    }

    [Fact]
    public async Task GetClaimsForScopeAsync_Profile_ReturnsProfileClaims()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("name", "Test User"),
            new Claim("email", "test@example.com") // Should NOT be returned for profile scope
        }));

        // Act
        var claims = await _service.GetClaimsForScopeAsync("profile", user);

        // Assert
        var claim = Assert.Single(claims);
        Assert.Equal("name", claim.Type);
        Assert.Equal("Test User", claim.Value);
    }

    [Fact]
    public async Task GetClaimsForScopeAsync_Email_ReturnsEmailClaims()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("email", "test@example.com"),
            new Claim("email_verified", "true"),
            new Claim("name", "Test User") // Should be ignored
        }));

        // Act
        var claims = await _service.GetClaimsForScopeAsync("email", user);

        // Assert
        Assert.Equal(2, claims.Count());
        Assert.Contains(claims, c => c.Type == "email");
        Assert.Contains(claims, c => c.Type == "email_verified");
    }

    [Fact]
    public async Task GetClaimsForScopeAsync_OnlyReturnsPresentClaims()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("name", "Test User")
            // Missing family_name, etc.
        }));

        // Act
        var claims = await _service.GetClaimsForScopeAsync("profile", user);

        // Assert
        Assert.Single(claims);
        Assert.Equal("name", claims.First().Type);
    }
}
