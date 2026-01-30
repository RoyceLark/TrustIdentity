using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.AI.Analyzers;
using Xunit;

namespace TrustIdentity.UnitTests
{
    public class FraudDetectionTests
    {
        private readonly Mock<ILogger<FraudDetectionService>> _mockLogger;
        private readonly Mock<IUserStore> _mockUserStore;
        private readonly FraudDetectionService _service;

        public FraudDetectionTests()
        {
            _mockLogger = new Mock<ILogger<FraudDetectionService>>();
            _mockUserStore = new Mock<IUserStore>();
            
            // Mock user store to return a user
            _mockUserStore.Setup(s => s.FindBySubjectIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { SubjectId = "user1", Username = "alice", FailedLoginAttempts = 0 });

            _service = new FraudDetectionService(_mockLogger.Object, _mockUserStore.Object);
        }

        [Fact]
        public async Task AnalyzeLoginAttemptAsync_ShouldReturnLowScore_WhenUserHasNoFailures()
        {
            // Act
            var score = await _service.AnalyzeLoginAttemptAsync("user1", "127.0.0.1", "Mozilla/5.0");

            // Assert
            // Since a model might be auto-trained, we check for a low probability (safe) rather than exact 0.0
            Assert.InRange(score, 0.0, 0.1);
        }

        [Fact]
        public async Task IsSuspiciousActivityAsync_ShouldReturnTrue_WhenRiskScoreHigh()
        {
            var pattern = new BehaviorPattern
            {
                PatternId = "test_pattern",
                FailedAttempts = 6, // > 5 -> +0.4
                LocationChanges = 4, // > 3 -> +0.3
                DeviceChanges = 3   // > 2 -> +0.3
                // Total = 1.0 (>= 0.7)
            };

            var result = await _service.IsSuspiciousActivityAsync(pattern);

            Assert.True(result);
        }

        [Fact]
        public async Task IsSuspiciousActivityAsync_ShouldReturnFalse_WhenRiskScoreLow()
        {
             var pattern = new BehaviorPattern
            {
                PatternId = "safe_pattern",
                FailedAttempts = 0,
                LocationChanges = 0, 
                DeviceChanges = 0
            };

            var result = await _service.IsSuspiciousActivityAsync(pattern);

            Assert.False(result);
        }
    }
}
