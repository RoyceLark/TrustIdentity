using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Allows inserting custom validation logic into the authorize request pipeline
/// </summary>
public interface ICustomAuthorizeRequestValidator
{
    /// <summary>
    /// Custom validation logic for the authorize request
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    Task ValidateAsync(CustomAuthorizeRequestValidationContext context);
}

/// <summary>
/// Context for custom authorize request validation
/// </summary>
public class CustomAuthorizeRequestValidationContext
{
    /// <summary>
    /// Gets or sets the validated authorize request
    /// </summary>
    public ValidatedAuthorizeRequest Result { get; set; } = new ValidatedAuthorizeRequest();
}

/// <summary>
/// Represents a validated authorize request
/// </summary>
public class ValidatedAuthorizeRequest
{
    /// <summary>
    /// Gets or sets whether the request is valid
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
    /// Gets or sets the client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response type
    /// </summary>
    public string ResponseType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the redirect URI
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requested scopes
    /// </summary>
    public List<string> RequestedScopes { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the subject (user)
    /// </summary>
    public ClaimsPrincipal? Subject { get; set; }

    /// <summary>
    /// Gets or sets the state
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nonce
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets custom response parameters
    /// </summary>
    public Dictionary<string, object> CustomResponse { get; set; } = new Dictionary<string, object>();
}
