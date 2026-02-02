using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// This interface allows TrustIdentity to connect to your custom user profile store.
/// This is the main extensibility point for adding custom claims to tokens.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint).
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    Task GetProfileDataAsync(ProfileDataRequestContext context);

    /// <summary>
    /// This method gets called whenever identity server needs to determine if the user is valid or active 
    /// (e.g. if the user's account has been deactivated since they logged in).
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    Task IsActiveAsync(IsActiveContext context);
}

/// <summary>
/// Context for profile data request
/// </summary>
public class ProfileDataRequestContext
{
    /// <summary>
    /// Gets or sets the subject (user) for which profile data is being requested
    /// </summary>
    public ClaimsPrincipal Subject { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client requesting the profile data
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requested claim types
    /// </summary>
    public ICollection<string> RequestedClaimTypes { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the claims to be issued
    /// </summary>
    public List<Claim> IssuedClaims { get; set; } = new List<Claim>();

    /// <summary>
    /// Gets or sets the caller (e.g., "ClaimsProviderAccessToken", "UserInfoEndpoint")
    /// </summary>
    public string Caller { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional context properties
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Context for IsActive check
/// </summary>
public class IsActiveContext
{
    /// <summary>
    /// Gets or sets the subject (user) being checked
    /// </summary>
    public ClaimsPrincipal Subject { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client making the request
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the caller
    /// </summary>
    public string Caller { get; set; } = string.Empty;
}
