using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Services;
using Xunit;

namespace TrustIdentity.UnitTests;

public class DeviceFlowAdvancedTests
{
    private readonly Mock<IDeviceFlowStore> _deviceFlowStoreMock;
    private readonly Mock<ILogger<DeviceFlowService>> _loggerMock;
    private readonly DeviceFlowService _service;

    public DeviceFlowAdvancedTests()
    {
        _deviceFlowStoreMock = new Mock<IDeviceFlowStore>();
        _loggerMock = new Mock<ILogger<DeviceFlowService>>();
        _service = new DeviceFlowService(_deviceFlowStoreMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UpdateByUserCodeAsync_UpdatesAuthorizationStatus()
    {
        // Arrange
        var userCode = "1234-5678";
        var originalCodes = new DeviceFlowCodes 
        { 
            UserCode = userCode, 
            DeviceCode = "dcode",
            SubjectId = null,
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };
        
        _deviceFlowStoreMock.Setup(x => x.FindByUserCodeAsync(userCode)).ReturnsAsync(originalCodes);

        var updatedCodes = new DeviceFlowCodes
        {
            UserCode = userCode,
            DeviceCode = "dcode",
            SubjectId = "user1",
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        await _service.UpdateByUserCodeAsync(userCode, updatedCodes);

        // Assert
        _deviceFlowStoreMock.Verify(x => x.UpdateByUserCodeAsync(userCode, 
            It.Is<DeviceFlowCodes>(c => c.SubjectId == "user1")), Times.Once);
    }

    [Fact]
    public async Task RemoveByDeviceCodeAsync_RemovesGrant()
    {
        // Arrange
        var deviceCode = "test-device-code";

        // Act
        await _service.RemoveByDeviceCodeAsync(deviceCode);

        // Assert
        _deviceFlowStoreMock.Verify(x => x.RemoveByDeviceCodeAsync(deviceCode), Times.Once);
    }

    [Fact]
    public async Task FindByUserCodeAsync_RemovesExpiredCode()
    {
        // Arrange
        var userCode = "1234-5678";
        var codes = new DeviceFlowCodes 
        { 
            UserCode = userCode, 
            DeviceCode = "dcode",
            Expiration = DateTime.UtcNow.AddMinutes(-5)
        };

        _deviceFlowStoreMock.Setup(x => x.FindByUserCodeAsync(userCode)).ReturnsAsync(codes);

        // Act
        var result = await _service.FindByUserCodeAsync(userCode);

        // Assert
        Assert.Null(result);
        _deviceFlowStoreMock.Verify(x => x.RemoveByDeviceCodeAsync("dcode"), Times.Once);
    }

    [Fact]
    public async Task UserCodeFormat_IsReadable()
    {
        // Act
        var result = await _service.CreateDeviceAuthorizationAsync("client1", new List<string> { "scope1" });

        // Assert
        Assert.NotNull(result.UserCode);
        Assert.Matches(@"^\d{4}-\d{4}$", result.UserCode); // Format: 1234-5678
    }

    [Fact]
    public async Task DeviceCodeHasCorrectLifetime()
    {
        // Act
        var result = await _service.CreateDeviceAuthorizationAsync("client1", new List<string> { "scope1" });

        // Assert
        var lifetime = result.Expiration - result.CreationTime;
        Assert.True(lifetime.TotalMinutes >= 4.9 && lifetime.TotalMinutes <= 5.1, 
            $"Expected lifetime around 5 minutes, but got {lifetime.TotalMinutes}");
    }
}
