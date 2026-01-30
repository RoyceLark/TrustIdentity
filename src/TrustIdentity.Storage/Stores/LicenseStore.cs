using Microsoft.EntityFrameworkCore;
using TrustIdentity.Licensing;
using TrustIdentity.Storage.EntityFramework;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrustIdentity.Storage.Stores;
/// <summary>
/// LicenseStore
/// </summary>
public class LicenseStore : ILicenseStore
{
    private readonly LicensingDbContext _context;
    /// <summary>
    /// LicenseStore
    /// </summary>
    /// <param name="context"></param>
    public LicenseStore(LicensingDbContext context)
    {
        _context = context;
    }
    /// <summary>
    /// GetLicenseAsync
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<License?> GetLicenseAsync(Guid id)
    {
        return await _context.Licenses.FindAsync(id);
    }
    /// <summary>
    /// GetLicenseByKeyAsync
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<License?> GetLicenseByKeyAsync(string key)
    {
        return await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == key);
    }
    /// <summary>
    /// SaveLicenseAsync
    /// </summary>
    /// <param name="license"></param>
    /// <returns></returns>
    public async Task SaveLicenseAsync(License license)
    {
        if (_context.Licenses.Any(l => l.Id == license.Id))
        {
            _context.Licenses.Update(license);
        }
        else
        {
            await _context.Licenses.AddAsync(license);
        }
        await _context.SaveChangesAsync();
    }
    /// <summary>
    /// GetAllLicensesAsync
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<License>> GetAllLicensesAsync()
    {
        return await _context.Licenses.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }
}
