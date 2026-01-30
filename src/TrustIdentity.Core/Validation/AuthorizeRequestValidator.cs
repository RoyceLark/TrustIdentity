using TrustIdentity.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.Core.Validation;

/// <summary>
/// Validator for authorization requests
/// </summary>
public class AuthorizeRequestValidator
{
    private readonly ILogger<AuthorizeRequestValidator> _logger;
    private readonly PkceValidator _pkceValidator;
    private readonly ScopeValidator _scopeValidator;

    /// <summary>
    /// Initializes a new instance of the AuthorizeRequestValidator
    /// </summary>
    public AuthorizeRequestValidator(
        ILogger<AuthorizeRequestValidator> logger,
        PkceValidator pkceValidator,
        ScopeValidator scopeValidator)
    {
        _logger = logger;
        _pkceValidator = pkceValidator;
        _scopeValidator = scopeValidator;
    }

    /// <summary>
    /// Validates an authorization request
    /// </summary>
    public AuthorizeRequestValidationResult Validate(AuthorizeRequest request, Client client)
    {
        var result = new AuthorizeRequestValidationResult();
        var requestValidator = new RequestValidator();

        // Validate response type
        if (string.IsNullOrEmpty(request.ResponseType))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "response_type is required";
            return result;
        }

        // Validate redirect URI
        if (string.IsNullOrEmpty(request.RedirectUri))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "redirect_uri is required";
            return result;
        }

        var redirectResult = requestValidator.ValidateSecureRedirectUri(request.RedirectUri, "redirect_uri");
        if (!redirectResult.IsValid)
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = redirectResult.GetErrorSummary();
            _logger.LogWarning("Invalid redirect_uri requested: {RedirectUri} for client {ClientId}", request.RedirectUri, client.ClientId);
            return result;
        }

        if (!client.RedirectUris.Contains(request.RedirectUri))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "redirect_uri not allowed";
            _logger.LogWarning("Redirect URI {RedirectUri} not registered for client {ClientId}", request.RedirectUri, client.ClientId);
            return result;
        }

        // Validate PKCE
        if (client.RequirePkce)
        {
            if (string.IsNullOrEmpty(request.CodeChallenge))
            {
                result.IsError = true;
                result.Error = "invalid_request";
                result.ErrorDescription = "code_challenge is required";
                return result;
            }

            if (!client.AllowPlainTextPkce && (string.IsNullOrEmpty(request.CodeChallengeMethod) || request.CodeChallengeMethod == "plain"))
            {
                result.IsError = true;
                result.Error = "invalid_request";
                result.ErrorDescription = "plain code challenge method not allowed";
                return result;
            }
        }

        // Validate scopes
        if (!string.IsNullOrEmpty(request.Scope))
        {
            var requestedScopes = request.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var scopeResult = _scopeValidator.ValidateScopes(requestedScopes, client.AllowedScopes);
            
            if (!scopeResult.IsValid)
            {
                result.IsError = true;
                result.Error = "invalid_scope";
                result.ErrorDescription = scopeResult.GetErrorSummary();
                _logger.LogWarning("Invalid scopes requested for client {ClientId}: {Errors}", client.ClientId, scopeResult.GetErrorSummary());
                return result;
            }
        }

        result.ValidatedRequest = request;
        return result;
    }
}

/// <summary>
/// Represents an OAuth 2.0 or OpenID Connect authorization request
/// </summary>
public class AuthorizeRequest
{
    /// <summary>The response type (e.g., "code")</summary>
    public string ResponseType { get; set; } = string.Empty;
    /// <summary>The client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The redirect URI</summary>
    public string RedirectUri { get; set; } = string.Empty;
    /// <summary>The requested scopes</summary>
    public string? Scope { get; set; }
    /// <summary>The state identifier</summary>
    public string? State { get; set; }
    /// <summary>The nonce value</summary>
    public string? Nonce { get; set; }
    /// <summary>The PKCE code challenge</summary>
    public string? CodeChallenge { get; set; }
    /// <summary>The PKCE code challenge method</summary>
    public string? CodeChallengeMethod { get; set; } = "S256";
    /// <summary>The response mode</summary>
    public string? ResponseMode { get; set; }
    /// <summary>Authentication Context Class Reference values</summary>
    public string? AcrValues { get; set; }
}

/// <summary>
/// Result of authorization request validation
/// </summary>
public class AuthorizeRequestValidationResult
{
    /// <summary>Whether an error occurred</summary>
    public bool IsError { get; set; }
    /// <summary>The error code</summary>
    public string? Error { get; set; }
    /// <summary>The error description</summary>
    public string? ErrorDescription { get; set; }
    /// <summary>The validated request object</summary>
    public AuthorizeRequest? ValidatedRequest { get; set; }
}