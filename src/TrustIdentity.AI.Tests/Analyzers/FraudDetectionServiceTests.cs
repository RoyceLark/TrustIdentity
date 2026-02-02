using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.AI.Analyzers;
using Xunit;

namespace TrustIdentity.AI.Tests.Analyzers
{
    public class FraudDetectionServiceTests
    {
        private readonly FraudDetectionService _service;
        private readonly Mock<IUserStore> _mockUserStore;

        public FraudDetectionServiceTests()
        {
            _mockUserStore = new Mock<IUserStore>();
            _service = new FraudDetectionService(
                NullLogger<FraudDetectionService>.Instance,
                _mockUserStore.Object
            );
        }

        [Fact]
        public async Task AnalyzeLoginAttemptAsync_ShouldReturnScore()
        {
            // Arrange
            _mockUserStore.Setup(x => x.FindBySubjectIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { SubjectId = "user1", FailedLoginAttempts = 0 });

            // Act
            var score = await _service.AnalyzeLoginAttemptAsync("user1", "127.0.0.1", "TestAgent");

            // Assert
            Assert.InRange(score, 0.0, 1.0);
        }

        [Fact]
        public async Task IsSuspiciousActivityAsync_ShouldReturnTrue_ForHighRiskPattern()
        {
            // Arrange
            var pattern = new BehaviorPattern
            {
                FailedAttempts = 10,
                LocationChanges = 5, 
                DeviceChanges = 5
            };

            // Act
            var result = await _service.IsSuspiciousActivityAsync(pattern);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsSuspiciousActivityAsync_ShouldReturnFalse_ForLowRiskPattern()
        {
            // Arrange
            var pattern = new BehaviorPattern
            {
                FailedAttempts = 0,
                LocationChanges = 0, 
                DeviceChanges = 0
            };

            // Act
            var result = await _service.IsSuspiciousActivityAsync(pattern);

            // Assert
            Assert.False(result);
        }
    }
}
