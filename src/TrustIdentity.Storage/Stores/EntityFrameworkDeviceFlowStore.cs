using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Storage.EntityFramework;

namespace TrustIdentity.Storage.Stores;

/// <summary>
/// Entity Framework implementation of device flow store
/// </summary>
public class EntityFrameworkDeviceFlowStore : IDeviceFlowStore
{
    private readonly PersistedGrantDbContext _context;

    /// <summary>
    /// Initializes a new instance of the EntityFrameworkDeviceFlowStore
    /// </summary>
    /// <param name="context">The database context</param>
    public EntityFrameworkDeviceFlowStore(PersistedGrantDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Stores the device authorization request
    /// </summary>
    /// <param name="deviceCode">The device code</param>
    /// <param name="userCode">The user code</param>
    /// <param name="data">The data to store</param>
    public async Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceFlowCodes data)
    {
        _context.DeviceFlowCodes.Add(data);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Finds the device authorization by user code
    /// </summary>
    /// <param name="userCode">The user code</param>
    /// <returns>The device flow codes</returns>
    public async Task<DeviceFlowCodes?> FindByUserCodeAsync(string userCode)
    {
        return await _context.DeviceFlowCodes
            .FirstOrDefaultAsync(d => d.UserCode == userCode);
    }

    /// <summary>
    /// Finds the device authorization by device code
    /// </summary>
    /// <param name="deviceCode">The device code</param>
    /// <returns>The device flow codes</returns>
    public async Task<DeviceFlowCodes?> FindByDeviceCodeAsync(string deviceCode)
    {
        return await _context.DeviceFlowCodes.FindAsync(deviceCode);
    }

    /// <summary>
    /// Updates by user code.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <param name="data">The data.</param>
    public async Task UpdateByUserCodeAsync(string userCode, DeviceFlowCodes data)
    {
        var existing = await _context.DeviceFlowCodes
            .FirstOrDefaultAsync(d => d.UserCode == userCode);

        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(data);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Removes by device code.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    public async Task RemoveByDeviceCodeAsync(string deviceCode)
    {
        var item = await _context.DeviceFlowCodes.FindAsync(deviceCode);
        if (item != null)
        {
            _context.DeviceFlowCodes.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
