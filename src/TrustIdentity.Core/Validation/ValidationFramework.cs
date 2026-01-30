namespace TrustIdentity.Core.Validation;

using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// Comprehensive Validation Result
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Initializes a new instance of the ValidationResult
    /// </summary>
    /// <param name="errors">Initial list of errors</param>
    public ValidationResult(List<ValidationError> errors)
    {
        Errors = errors ?? new List<ValidationError>();
    }

    /// <summary>Whether the validation succeeded</summary>
    public bool IsValid { get; set; } = true;
    /// <summary>List of validation errors</summary>
    public List<ValidationError> Errors { get; set; } = new();
    /// <summary>List of validation warnings</summary>
    public List<ValidationWarning> Warnings { get; set; } = new();

    /// <summary>
    /// Adds an error to the result
    /// </summary>
    /// <param name="field">The field name</param>
    /// <param name="message">The error message</param>
    /// <param name="code">The optional error code</param>
    public void AddError(string field, string message, string? code = null)
    {
        IsValid = false;
        Errors.Add(new ValidationError
        {
            Field = field,
            Message = message,
            Code = code
        });
    }

    /// <summary>
    /// Adds a warning to the result
    /// </summary>
    /// <param name="field">The field name</param>
    /// <param name="message">The warning message</param>
    public void AddWarning(string field, string message)
    {
        Warnings.Add(new ValidationWarning
        {
            Field = field,
            Message = message
        });
    }

    /// <summary>
    /// Gets a summarized string of all errors
    /// </summary>
    /// <returns>A semicolon separated list of errors</returns>
    public string GetErrorSummary()
    {
        return string.Join("; ", Errors.Select(e => $"{e.Field}: {e.Message}"));
    }
}

/// <summary>
/// Represents a single validation error
/// </summary>
public class ValidationError
{
    /// <summary>The field that failed validation</summary>
    public string Field { get; set; } = string.Empty;
    /// <summary>The error message</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>The error code</summary>
    public string? Code { get; set; }
}

/// <summary>
/// Represents a single validation warning
/// </summary>
public class ValidationWarning
{
    /// <summary>The field that has a warning</summary>
    public string Field { get; set; } = string.Empty;
    /// <summary>The warning message</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request Validator with comprehensive checks
/// </summary>
public class RequestValidator
{
    /// <summary>
    /// Validates that a string value is not empty
    /// </summary>
    /// <param name="value">The value to test</param>
    /// <param name="fieldName">The name of the field</param>
    /// <returns>Validation result</returns>
    public ValidationResult ValidateRequired(string value, string fieldName)
    {
        var result = new ValidationResult(new List<ValidationError>());
        
        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError(fieldName, $"{fieldName} is required", "required");
        }

        return result;
    }

    /// <summary>
    /// Validates that a string is a valid absolute URL
    /// </summary>
    public ValidationResult ValidateUrl(string url, string fieldName)
    {
        var result = new ValidationResult(new List<ValidationError>());

        if (string.IsNullOrWhiteSpace(url))
        {
            result.AddError(fieldName, $"{fieldName} is required", "required");
            return result;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            result.AddError(fieldName, $"{fieldName} must be a valid absolute URL", "invalid_url");
            return result;
        }

        if (uri.Scheme != "https" && uri.Scheme != "http")
        {
            result.AddError(fieldName, $"{fieldName} must use HTTP or HTTPS scheme", "invalid_scheme");
        }

        return result;
    }

    /// <summary>
    /// Validates a redirect URI for security
    /// </summary>
    public ValidationResult ValidateSecureRedirectUri(string url, string fieldName)
    {
        var result = ValidateUrl(url, fieldName);
        if (!result.IsValid) return result;

        var uri = new Uri(url);

        // Enforce HTTPS unless localhost
        if (uri.Scheme != "https" && uri.Host != "localhost" && uri.Host != "127.0.0.1" && uri.Host != "::1")
        {
            result.AddError(fieldName, "Redirect URI must use HTTPS for non-localhost addresses", "insecure_scheme");
        }

        // Prevent some common open redirect tricks
        if (uri.Host.Contains("..") || url.Contains("\\"))
        {
            result.AddError(fieldName, "Redirect URI contains invalid characters or sequences", "invalid_uri_content");
        }

        return result;
    }

    /// <summary>
    /// Validates the format of a scope
    /// </summary>
    public ValidationResult ValidateScopeFormat(string scope, string fieldName)
    {
        var result = new ValidationResult(new List<ValidationError>());
        if (string.IsNullOrWhiteSpace(scope))
        {
            result.AddError(fieldName, "Scope cannot be empty", "invalid_scope");
            return result;
        }

        // Scopes should only contain alphanumeric, underscore, hyphen, and dot
        if (!System.Text.RegularExpressions.Regex.IsMatch(scope, @"^[a-zA-Z0-9_\-\.]+$"))
        {
            result.AddError(fieldName, $"Scope '{scope}' contains invalid characters", "invalid_scope_format");
        }

        return result;
    }

    /// <summary>
    /// Validates a CORS origin for security
    /// </summary>
    public ValidationResult ValidateCorsOrigin(string origin, string fieldName)
    {
        var result = new ValidationResult(new List<ValidationError>());
        
        if (string.IsNullOrWhiteSpace(origin))
            return result;

        if (origin == "*")
        {
            result.AddError(fieldName, "Wildcard CORS origins are not allowed for security reasons", "invalid_cors_wildcard");
            return result;
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            result.AddError(fieldName, $"Invalid CORS origin: {origin}", "invalid_cors_origin");
            return result;
        }

        if (uri.Scheme != "https" && uri.Host != "localhost" && uri.Host != "127.0.0.1" && uri.Host != "::1")
        {
            result.AddError(fieldName, "CORS origin must use HTTPS for non-localhost addresses", "insecure_cors_scheme");
        }

        return result;
    }

    /// <summary>
    /// Validates the length of a string
    /// </summary>
    /// <param name="value">The string to test</param>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="minLength">Minimum required length</param>
    /// <param name="maxLength">Maximum allowed length</param>
    /// <returns>Validation result</returns>
    public ValidationResult ValidateLength(string value, string fieldName, int minLength, int maxLength)
    {
        var result = new ValidationResult(new List<ValidationError>());

        if (string.IsNullOrEmpty(value))
            return result;

        if (value.Length < minLength)
        {
            result.AddError(fieldName, 
                $"{fieldName} must be at least {minLength} characters", 
                "min_length");
        }

        if (value.Length > maxLength)
        {
            result.AddError(fieldName, 
                $"{fieldName} must not exceed {maxLength} characters", 
                "max_length");
        }

        return result;
    }

    /// <summary>
    /// Validates that a string value is a valid enum member
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="value">The value to test</param>
    /// <param name="fieldName">The name of the field</param>
    /// <returns>Validation result</returns>
    public ValidationResult ValidateEnum<T>(string value, string fieldName) where T : struct, Enum
    {
        var result = new ValidationResult(new List<ValidationError>());

        if (!Enum.TryParse<T>(value, ignoreCase: true, out _))
        {
            var validValues = string.Join(", ", Enum.GetNames(typeof(T)));
            result.AddError(fieldName, 
                $"{fieldName} must be one of: {validValues}", 
                "invalid_enum");
        }

        return result;
    }
}