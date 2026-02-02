using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Default implementation of IProfileService
/// </summary>
public class DefaultProfileService : IProfileService
{
    private readonly ILogger<DefaultProfileService> _logger;

    /// <summary>
    /// Initializes a new instance of the DefaultProfileService
    /// </summary>
    public DefaultProfileService(ILogger<DefaultProfileService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets profile data for the user
    /// </summary>
    public virtual Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        _logger.LogDebug("GetProfileDataAsync called for subject: {Subject}", context.Subject.FindFirst("sub")?.Value);

        // By default, return all claims from the subject
        context.IssuedClaims.AddRange(context.Subject.Claims);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if the user is active
    /// </summary>
    public virtual Task IsActiveAsync(IsActiveContext context)
    {
        _logger.LogDebug("IsActiveAsync called for subject: {Subject}", context.Subject.FindFirst("sub")?.Value);

        // By default, all users are active
        context.IsActive = true;

        return Task.CompletedTask;
    }
}
