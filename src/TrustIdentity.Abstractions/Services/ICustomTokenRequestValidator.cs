using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Allows inserting custom validation logic into the token request pipeline
/// </summary>
public interface ICustomTokenRequestValidator
{
    /// <summary>
    /// Custom validation logic for the token request
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    Task ValidateAsync(CustomTokenRequestValidationContext context);
}

/// <summary>
/// Context for custom token request validation
/// </summary>
public class CustomTokenRequestValidationContext
{
    /// <summary>
    /// Gets or sets the validated token request
    /// </summary>
    public ValidatedTokenRequest Result { get; set; } = new ValidatedTokenRequest();
}

/// <summary>
/// Represents a validated token request
/// </summary>
public class ValidatedTokenRequest
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
    /// Gets or sets the grant type
    /// </summary>
    public string GrantType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scopes
    /// </summary>
    public List<string> Scopes { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the subject (user)
    /// </summary>
    public ClaimsPrincipal? Subject { get; set; }

    /// <summary>
    /// Gets or sets custom response parameters
    /// </summary>
    public Dictionary<string, object> CustomResponse { get; set; } = new Dictionary<string, object>();
}
