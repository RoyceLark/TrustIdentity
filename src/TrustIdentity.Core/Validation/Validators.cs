using TrustIdentity.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.Core.Validation;

/// <summary>
/// Validates client configuration
/// </summary>
public class ClientValidator
{
    private readonly ILogger<ClientValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the ClientValidator
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public ClientValidator(ILogger<ClientValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates a client configuration
    /// </summary>
    public ValidationResult Validate(Client client)
    {
        var validator = new RequestValidator();
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(client.ClientId))
            errors.Add(new ValidationError() { Field = "ClientId", Message = "ClientId is required" });

        if (client.AllowedGrantTypes == null || !client.AllowedGrantTypes.Any())
            errors.Add(new ValidationError() { Field = "AllowedGrantTypes", Message = "At least one grant type is required"});

        // Validate Redirect URIs
        if (client.RedirectUris != null)
        {
            foreach (var uri in client.RedirectUris)
            {
                var result = validator.ValidateSecureRedirectUri(uri, "RedirectUri");
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                }
            }
        }

        if (client.RequireClientSecret && (client.ClientSecrets == null || !client.ClientSecrets.Any()))
            errors.Add(new ValidationError()
            {
                Field = "ClientSecrets",
                Message = "Client secret is required when RequireClientSecret is true"});

        if (client.AllowedGrantTypes != null && client.AllowedGrantTypes.Contains("authorization_code") && (client.RedirectUris == null || !client.RedirectUris.Any()))
            errors.Add(new ValidationError()
            {
                Field = "RedirectUris",
                Message = "Redirect URIs are required for authorization code flow"});

        // Validate CORS Origins
        if (client.AllowedCorsOrigins != null)
        {
            foreach (var origin in client.AllowedCorsOrigins)
            {
                var result = validator.ValidateCorsOrigin(origin, "AllowedCorsOrigin");
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                }
            }
        }

        return new ValidationResult(errors);
    }
}

/// <summary>
/// Validates requested scopes against allowed scopes
/// </summary>
public class ScopeValidator
{
    private readonly ILogger<ScopeValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the ScopeValidator
    /// </summary>
    public ScopeValidator(ILogger<ScopeValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates requested scopes against allowed scopes
    /// </summary>
    public ValidationResult ValidateScopes(IEnumerable<string> requestedScopes, IEnumerable<string> allowedScopes)
    {
        var validator = new RequestValidator();
        var errors = new List<ValidationError>();
        var requested = requestedScopes?.ToHashSet() ?? new HashSet<string>();
        var allowed = allowedScopes?.ToHashSet() ?? new HashSet<string>();

        foreach (var scope in requested)
        {
            // Security: Validate scope format
            var formatResult = validator.ValidateScopeFormat(scope, "scope");
            if (!formatResult.IsValid)
            {
                errors.AddRange(formatResult.Errors);
                continue;
            }

            if (!allowed.Contains(scope))
            {
                errors.Add(new ValidationError() { Field = "scope", Message = $"Scope '{scope}' is not allowed" });
            }
        }

        return new ValidationResult(errors);
    }
}

/// <summary>
/// Validates security tokens
/// </summary>
public class TokenValidator
{
    private readonly ILogger<TokenValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the TokenValidator
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public TokenValidator(ILogger<TokenValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates an access token
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>The token validation result</returns>
    public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token)
    {
        // In production, use proper JWT validation with Microsoft.IdentityModel.Tokens
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(token))
            return new TokenValidationResult { IsValid = false, Error = "Token is required" };

        // Basic format validation
        var parts = token.Split('.');
        if (parts.Length != 3)
            return new TokenValidationResult { IsValid = false, Error = "Invalid token format" };

        return new TokenValidationResult { IsValid = true };
    }
}

//public class ValidationResult
//{
//    public bool IsValid => !Errors.Any();
//    public List<string> Errors { get; }

//    public ValidationResult(List<string> errors)
//    {
//        Errors = errors ?? new List<string>();
//    }
//}

/// <summary>
/// Result of token validation
/// </summary>
public class TokenValidationResult
{
    /// <summary>Whether the token is valid</summary>
    public bool IsValid { get; set; }
    /// <summary>Error message if validation failed</summary>
    public string? Error { get; set; }
    /// <summary>Claims extracted from the token</summary>
    public Dictionary<string, object> Claims { get; set; } = new();
}