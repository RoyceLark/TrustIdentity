using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Interface for controlling the authorize endpoint response generation
/// This allows custom logic for login, consent, and error pages
/// </summary>
public interface IAuthorizeInteractionResponseGenerator
{
    /// <summary>
    /// Processes the interaction logic
    /// </summary>
    /// <param name="request">The validated authorize request</param>
    /// <param name="consent">The consent response (if any)</param>
    /// <returns></returns>
    Task<InteractionResponse> ProcessInteractionAsync(ValidatedAuthorizeRequest request, ConsentResponse? consent = null);
}

/// <summary>
/// Response from interaction processing
/// </summary>
public class InteractionResponse
{
    /// <summary>
    /// Gets or sets whether the user needs to login
    /// </summary>
    public bool IsLogin { get; set; }

    /// <summary>
    /// Gets or sets whether consent is required
    /// </summary>
    public bool IsConsent { get; set; }

    /// <summary>
    /// Gets or sets whether an error occurred
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// Gets or sets the error
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error description
    /// </summary>
    public string ErrorDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to redirect to a custom page
    /// </summary>
    public bool IsRedirect { get; set; }

    /// <summary>
    /// Gets or sets the redirect URL
    /// </summary>
    public string RedirectUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets custom data
    /// </summary>
    public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Consent response from the user
/// </summary>
public class ConsentResponse
{
    /// <summary>
    /// Gets or sets whether the user granted consent
    /// </summary>
    public bool Granted { get; set; }

    /// <summary>
    /// Gets or sets whether to remember the consent
    /// </summary>
    public bool RememberConsent { get; set; }

    /// <summary>
    /// Gets or sets the scopes the user consented to
    /// </summary>
    public List<string> ScopesValuesConsented { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the description provided by the user
    /// </summary>
    public string? Description { get; set; }
}
