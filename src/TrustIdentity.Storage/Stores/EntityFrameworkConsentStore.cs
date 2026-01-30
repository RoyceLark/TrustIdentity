using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Storage.EntityFramework;

namespace TrustIdentity.Storage.Stores;

/// <summary>
/// Entity Framework implementation of consent store
/// </summary>
public class EntityFrameworkConsentStore : IConsentStore
{
    private readonly PersistedGrantDbContext _context;

    /// <summary>
    /// Initializes a new instance of the EntityFrameworkConsentStore
    /// </summary>
    /// <param name="context">The database context</param>
    public EntityFrameworkConsentStore(PersistedGrantDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a user consent
    /// </summary>
    /// <param name="subjectId">The user identifier</param>
    /// <param name="clientId">The client identifier</param>
    /// <returns>The user consent or null</returns>
    public async Task<UserConsent?> GetAsync(string subjectId, string clientId)
    {
        return await _context.UserConsents
            .FirstOrDefaultAsync(c => c.SubjectId == subjectId && c.ClientId == clientId);
    }

    /// <summary>
    /// Stores a user consent
    /// </summary>
    /// <param name="consent">The consent to store</param>
    public async Task StoreAsync(UserConsent consent)
    {
        var existing = await _context.UserConsents
            .FirstOrDefaultAsync(c => c.SubjectId == consent.SubjectId && c.ClientId == consent.ClientId);

        if (existing == null)
        {
            _context.UserConsents.Add(consent);
        }
        else
        {
            existing.Scopes = consent.Scopes;
            existing.ExpiresAt = consent.ExpiresAt;
            existing.CreatedAt = consent.CreatedAt;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a user consent
    /// </summary>
    /// <param name="subjectId">The user identifier</param>
    /// <param name="clientId">The client identifier</param>
    public async Task RemoveAsync(string subjectId, string clientId)
    {
        var consent = await _context.UserConsents
            .FirstOrDefaultAsync(c => c.SubjectId == subjectId && c.ClientId == clientId);

        if (consent != null)
        {
            _context.UserConsents.Remove(consent);
            await _context.SaveChangesAsync();
        }
    }
}
