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

public class DeviceFlowServiceTests
{
    private readonly Mock<IDeviceFlowStore> _deviceFlowStoreMock;
    private readonly Mock<ILogger<DeviceFlowService>> _loggerMock;
    private readonly DeviceFlowService _service;

    public DeviceFlowServiceTests()
    {
        _deviceFlowStoreMock = new Mock<IDeviceFlowStore>();
        _loggerMock = new Mock<ILogger<DeviceFlowService>>();
        _service = new DeviceFlowService(_deviceFlowStoreMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateDeviceAuthorizationAsync_StoresGrantAndReturnsCodes()
    {
        // Arrange
        var clientId = "client1";
        var scopes = new List<string> { "scope1" };

        // Act
        var result = await _service.CreateDeviceAuthorizationAsync(clientId, scopes);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.DeviceCode);
        Assert.NotNull(result.UserCode);
        _deviceFlowStoreMock.Verify(x => x.StoreDeviceAuthorizationAsync(
            result.DeviceCode, 
            result.UserCode, 
            It.Is<DeviceFlowCodes>(c => c.ClientId == clientId)), Times.Once);
    }

    [Fact]
    public async Task FindByUserCodeAsync_ReturnsCodes_WhenFound()
    {
        // Arrange
        var userCode = "1234-5678";
        var expectedCodes = new DeviceFlowCodes 
        { 
            UserCode = userCode, 
            DeviceCode = "dcode",
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };
        
        _deviceFlowStoreMock.Setup(x => x.FindByUserCodeAsync(userCode)).ReturnsAsync(expectedCodes);

        // Act
        var result = await _service.FindByUserCodeAsync(userCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userCode, result!.UserCode);
    }

    [Fact]
    public async Task FindByDeviceCodeAsync_ReturnsCodes_WhenFound()
    {
        // Arrange
        var deviceCode = "dcode";
        var expectedCodes = new DeviceFlowCodes 
        { 
            DeviceCode = deviceCode,
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };

        _deviceFlowStoreMock.Setup(x => x.FindByDeviceCodeAsync(deviceCode)).ReturnsAsync(expectedCodes);

        // Act
        var result = await _service.FindByDeviceCodeAsync(deviceCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(deviceCode, result!.DeviceCode);
    }

    [Fact]
    public async Task CheckExpiredDeviceCode_ReturnsNull()
    {
        // Arrange
        var deviceCode = "expired";
        var codes = new DeviceFlowCodes
        {
            DeviceCode = deviceCode,
            Expiration = DateTime.UtcNow.AddMinutes(-5)
        };
        _deviceFlowStoreMock.Setup(x => x.FindByDeviceCodeAsync(deviceCode)).ReturnsAsync(codes);

        // Act
        var result = await _service.FindByDeviceCodeAsync(deviceCode);

        // Assert
        Assert.Null(result);
        _deviceFlowStoreMock.Verify(x => x.RemoveByDeviceCodeAsync(deviceCode), Times.Once);
    }
}
