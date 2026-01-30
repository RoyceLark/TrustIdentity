using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.Core.Validation;

/// <summary>
/// Validator for token requests
/// </summary>
public class TokenRequestValidator
{
    private readonly IClientStore _clientStore;
    private readonly ILogger<TokenRequestValidator> _logger;
    private readonly PkceValidator _pkceValidator;
    private readonly ScopeValidator _scopeValidator;
    private readonly ClientService _clientService;

    /// <summary>
    /// Initializes a new instance of the TokenRequestValidator
    /// </summary>
    public TokenRequestValidator(
        IClientStore clientStore,
        ILogger<TokenRequestValidator> logger,
        PkceValidator pkceValidator,
        ScopeValidator scopeValidator,
        ClientService clientService)
    {
        _clientStore = clientStore;
        _logger = logger;
        _pkceValidator = pkceValidator;
        _scopeValidator = scopeValidator;
        _clientService = clientService;
    }

    /// <summary>
    /// Validates a token request
    /// </summary>
    public async Task<TokenRequestValidationResult> ValidateAsync(TokenRequest request)
    {
        var result = new TokenRequestValidationResult();
        var requestValidator = new RequestValidator();

        // Validate client
        var client = await _clientStore.FindClientByIdAsync(request.ClientId);
        if (client == null)
        {
            result.IsError = true;
            result.Error = "invalid_client";
            _logger.LogWarning("Token request for unknown client: {ClientId}", request.ClientId);
            return result;
        }

        result.Client = client;

        // Validate client secret if required
        if (client.RequireClientSecret)
        {
            if (string.IsNullOrEmpty(request.ClientSecret))
            {
                result.IsError = true;
                result.Error = "invalid_client";
                result.ErrorDescription = "client_secret is required";
                _logger.LogWarning("Token request missing client_secret for client: {ClientId}", client.ClientId);
                return result;
            }

            if (!await _clientService.ValidateSecretAsync(client, request.ClientSecret))
            {
                result.IsError = true;
                result.Error = "invalid_client";
                result.ErrorDescription = "invalid client_secret";
                _logger.LogWarning("Token request with invalid client_secret for client: {ClientId}", client.ClientId);
                return result;
            }
        }

        // Validate grant type
        if (!client.AllowedGrantTypes.Contains(request.GrantType))
        {
            result.IsError = true;
            result.Error = "unsupported_grant_type";
            _logger.LogWarning("Unsupported grant type {GrantType} for client {ClientId}", request.GrantType, client.ClientId);
            return result;
        }

        // Validate scopes if provided
        if (!string.IsNullOrEmpty(request.Scope))
        {
            var requestedScopes = request.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var scopeResult = _scopeValidator.ValidateScopes(requestedScopes, client.AllowedScopes);
            if (!scopeResult.IsValid)
            {
                result.IsError = true;
                result.Error = "invalid_scope";
                result.ErrorDescription = scopeResult.GetErrorSummary();
                return result;
            }
        }

        // Validate Redirect URI if provided
        if (!string.IsNullOrEmpty(request.RedirectUri))
        {
            var redirectResult = requestValidator.ValidateSecureRedirectUri(request.RedirectUri, "redirect_uri");
            if (!redirectResult.IsValid)
            {
                result.IsError = true;
                result.Error = "invalid_request";
                result.ErrorDescription = redirectResult.GetErrorSummary();
                return result;
            }
        }

        // Grant type specific validation
        switch (request.GrantType)
        {
            case "authorization_code":
                await ValidateAuthorizationCodeRequestAsync(request, client, result);
                break;
            case "client_credentials":
                ValidateClientCredentialsRequest(request, client, result);
                break;
            case "password":
                ValidateResourceOwnerPasswordRequest(request, client, result);
                break;
            case "refresh_token":
                ValidateRefreshTokenRequest(request, client, result);
                break;
            case "urn:openid:params:grant-type:ciba":
                ValidateCibaRequest(request, client, result);
                break;
            case "urn:ietf:params:oauth:grant-type:token-exchange":
                ValidateTokenExchangeRequest(request, client, result);
                break;
        }

        return result;
    }

    private async Task ValidateAuthorizationCodeRequestAsync(
        TokenRequest request, Client client, TokenRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.Code))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "code is required";
            return;
        }

        if (string.IsNullOrEmpty(request.RedirectUri))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "redirect_uri is required";
            return;
        }

        // Validate PKCE if required
        if (client.RequirePkce)
        {
            if (string.IsNullOrEmpty(request.CodeVerifier))
            {
                result.IsError = true;
                result.Error = "invalid_request";
                result.ErrorDescription = "code_verifier is required";
                return;
            }

            if (!_pkceValidator.IsValidCodeVerifier(request.CodeVerifier))
            {
                result.IsError = true;
                result.Error = "invalid_request";
                result.ErrorDescription = "invalid code_verifier";
                return;
            }
        }

        await Task.CompletedTask;
    }

    private void ValidateClientCredentialsRequest(
        TokenRequest request, Client client, TokenRequestValidationResult result)
    {
        // Client credentials grant doesn't require additional validation
    }

    private void ValidateResourceOwnerPasswordRequest(
        TokenRequest request, Client client, TokenRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.Username))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "username is required";
            return;
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "password is required";
            return;
        }
    }

    private void ValidateRefreshTokenRequest(
        TokenRequest request, Client client, TokenRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "refresh_token is required";
            return;
        }
    }

    private void ValidateCibaRequest(
        TokenRequest request, Client client, TokenRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.AuthReqId))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "auth_req_id is required";
            return;
        }
    }
    private void ValidateTokenExchangeRequest(
        TokenRequest request, Client client, TokenRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.SubjectToken))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "subject_token is required";
            return;
        }

        if (string.IsNullOrEmpty(request.SubjectTokenType))
        {
            result.IsError = true;
            result.Error = "invalid_request";
            result.ErrorDescription = "subject_token_type is required";
            return;
        }
    }
}

/// <summary>
/// Represents an OAuth 2.0 or OpenID Connect token request
/// </summary>
public class TokenRequest
{
    /// <summary>The grant type</summary>
    public string GrantType { get; set; } = string.Empty;
    /// <summary>The client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>The client secret</summary>
    public string? ClientSecret { get; set; }
    /// <summary>The authorization code</summary>
    public string? Code { get; set; }
    /// <summary>The redirect URI</summary>
    public string? RedirectUri { get; set; }
    /// <summary>The PKCE code verifier</summary>
    public string? CodeVerifier { get; set; }
    /// <summary>The username for password grant</summary>
    public string? Username { get; set; }
    /// <summary>The password for password grant</summary>
    public string? Password { get; set; }
    /// <summary>The refresh token</summary>
    public string? RefreshToken { get; set; }
    /// <summary>The requested scopes</summary>
    public string? Scope { get; set; }
    /// <summary>The CIBA auth_req_id</summary>
    public string? AuthReqId { get; set; }
    
    /// <summary>The subject token for token exchange</summary>
    public string? SubjectToken { get; set; }
    /// <summary>The subject token type for token exchange</summary>
    public string? SubjectTokenType { get; set; }
    /// <summary>The actor token for token exchange</summary>
    public string? ActorToken { get; set; }
    /// <summary>The actor token type for token exchange</summary>
    public string? ActorTokenType { get; set; }
}

/// <summary>
/// Result of token request validation
/// </summary>
public class TokenRequestValidationResult
{
    /// <summary>Whether an error occurred</summary>
    public bool IsError { get; set; }
    /// <summary>The error code</summary>
    public string? Error { get; set; }
    /// <summary>The error description</summary>
    public string? ErrorDescription { get; set; }
    /// <summary>The validated client</summary>
    public Client? Client { get; set; }
    /// <summary>The validated request object</summary>
    public TokenRequest? ValidatedRequest { get; set; }
}