using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests
{
    public class TokenExchangeServiceTests
    {
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly TokenExchangeService _service;

        public TokenExchangeServiceTests()
        {
            _mockTokenService = new Mock<ITokenService>();
            _service = new TokenExchangeService(_mockTokenService.Object);
        }

        [Fact]
        public async Task ExchangeAsync_ShouldFail_WhenTokenTypeIsNotAccess()
        {
            var result = await _service.ExchangeAsync("token", "invalid_type");
            Assert.True(result.IsError);
            Assert.Equal("invalid_request", result.Error);
        }

        [Fact]
        public async Task ExchangeAsync_ShouldFail_WhenTokenValidationFails()
        {
            _mockTokenService.Setup(x => x.ValidateTokenDetailedAsync(It.IsAny<string>()))
                .ReturnsAsync(new TokenValidationResultDetailed { IsValid = false });

            var result = await _service.ExchangeAsync("invalid_token", "urn:ietf:params:oauth:token-type:access_token");
            Assert.True(result.IsError);
            Assert.Equal("invalid_token", result.Error);
        }

        [Fact]
        public async Task ExchangeAsync_ShouldReturnUser_WhenTokenIsValid()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", "user123"),
                new Claim("name", "Alice")
            }));

            _mockTokenService.Setup(x => x.ValidateTokenDetailedAsync(It.IsAny<string>()))
                .ReturnsAsync(new TokenValidationResultDetailed { IsValid = true, Principal = principal });

            var result = await _service.ExchangeAsync("valid_token", "urn:ietf:params:oauth:token-type:access_token");
            
            Assert.False(result.IsError);
            Assert.NotNull(result.User);
            Assert.Equal("user123", result.User.SubjectId);
        }
    }
}
