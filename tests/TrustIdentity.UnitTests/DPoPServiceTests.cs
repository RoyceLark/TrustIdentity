using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests
{
    public class DPoPServiceTests
    {
        private readonly DPoPService _service;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ILogger<DPoPService>> _mockLogger;

        public DPoPServiceTests()
        {
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<DPoPService>>();
            _service = new DPoPService(_mockLogger.Object);
        }

        [Fact]
        public async Task ValidateProofAsync_ShouldFail_WhenProofIsMissing()
        {
            var result = await _service.ValidateDPoPProofAsync(null!, "POST", "https://server/token");
            Assert.False(result.IsValid);
            Assert.Contains("Validation error", result.Error);
        }

        [Fact]
        public async Task ValidateProofAsync_ShouldFail_WhenProofIsMalformed()
        {
            var result = await _service.ValidateDPoPProofAsync("not.a.jwt", "POST", "https://server/token");
            Assert.False(result.IsValid);
            Assert.Contains("Validation error", result.Error);
        }
    }
}
