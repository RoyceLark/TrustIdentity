using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Handles validation of resource owner password credentials
/// </summary>
public interface IResourceOwnerPasswordValidator
{
    /// <summary>
    /// Validates the resource owner password credential
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    Task ValidateAsync(ResourceOwnerPasswordValidationContext context);
}

/// <summary>
/// Context for resource owner password validation
/// </summary>
public class ResourceOwnerPasswordValidationContext
{
    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw request (allows access to custom parameters)
    /// </summary>
    public Dictionary<string, string> Request { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the result of the validation
    /// </summary>
    public GrantValidationResult Result { get; set; } = new GrantValidationResult();
}

/// <summary>
/// Result of grant validation
/// </summary>
public class GrantValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation was successful
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// Gets or sets the error description
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error description
    /// </summary>
    public string ErrorDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject (user identifier)
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the claims for the user
    /// </summary>
    public List<Claim> Claims { get; set; } = new List<Claim>();

    /// <summary>
    /// Gets or sets custom properties
    /// </summary>
    public Dictionary<string, object> CustomResponse { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static GrantValidationResult Success(string subject, IEnumerable<Claim>? claims = null)
    {
        return new GrantValidationResult
        {
            IsError = false,
            Subject = subject,
            Claims = claims?.ToList() ?? new List<Claim>()
        };
    }

    /// <summary>
    /// Creates a failed validation result
    /// </summary>
    public static GrantValidationResult Failed(string error, string? errorDescription = null)
    {
        return new GrantValidationResult
        {
            IsError = true,
            Error = error,
            ErrorDescription = errorDescription ?? string.Empty
        };
    }
}
