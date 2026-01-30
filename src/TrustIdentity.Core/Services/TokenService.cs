using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using TrustIdentity.Abstractions.Configuration;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for creating and validating JWT and Refresh tokens
/// </summary>
public class TokenService : ITokenService
{
    private readonly string? _jwtSigningKey;
    private readonly SigningCredentialStore? _credentialStore;
    private readonly string _issuer;
    private readonly int _accessTokenLifetimeHours;
    private readonly int _refreshTokenLifetimeDays;
    private readonly ILogger<TokenService> _logger;
    private readonly TrustIdentityOptions _options;

    /// <summary>
    /// Initializes a new instance of the TokenService
    /// </summary>
    public TokenService(
        IConfiguration configuration, 
        ILogger<TokenService> logger,
        TrustIdentityOptions options,
        SigningCredentialStore? credentialStore = null)
    {
        _logger = logger;
        _options = options;
        _credentialStore = credentialStore;
        
        _jwtSigningKey = configuration["JwtSettings:SigningKey"];
        _issuer = options.IssuerUri;
        _accessTokenLifetimeHours = options.Authentication.AccessTokenLifetime / 3600;
        _refreshTokenLifetimeDays = options.Authentication.RefreshTokenLifetime / 86400;

        if (_credentialStore == null)
        {
            if (string.IsNullOrEmpty(_jwtSigningKey))
            {
                throw new InvalidOperationException(
                    "JWT signing key not configured and no signing certificate found. " +
                    "Set JwtSettings:SigningKey or use AddDeveloperSigningCredential().");
            }

            if (_jwtSigningKey.Length < 32)
            {
                throw new InvalidOperationException("JWT symmetric signing key must be at least 32 characters long.");
            }
        }

        _logger.LogInformation("TokenService initialized with issuer: {Issuer}. Using {SigningMode} signing.", 
            _issuer, _credentialStore != null ? "Asymmetric (Certificate)" : "Symmetric (HMAC)");
    }

    /// <summary>
    /// Creates a new access token for a client and user
    /// </summary>
    public Task<Token> CreateAccessTokenAsync(Client client, User user, IEnumerable<string> scopes)
    {
        var lifetime = client.AccessTokenLifetime > 0 ? client.AccessTokenLifetime : _options.Authentication.AccessTokenLifetime;
        
        // Security check: Ensure lifetime doesn't exceed maximum
        if (lifetime > _options.Authentication.MaximumTokenLifetime)
        {
            _logger.LogWarning("Cliend {ClientId} requested token lifetime {Lifetime} which exceeds maximum {Max}", 
                client.ClientId, lifetime, _options.Authentication.MaximumTokenLifetime);
            lifetime = _options.Authentication.MaximumTokenLifetime;
        }

        var token = new Token
        {
            Issuer = _issuer,
            Audience = client.ClientId,
            SubjectId = user.SubjectId,
            ClientId = client.ClientId,
            Scopes = scopes.ToList(),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(lifetime)
        };

        return Task.FromResult(token);
    }

    /// <summary>
    /// Creates a new refresh token for a client and user
    /// </summary>
    public Task<Token> CreateRefreshTokenAsync(Client client, User user)
    {
        var lifetime = client.AbsoluteRefreshTokenLifetime > 0 ? client.AbsoluteRefreshTokenLifetime : _options.Authentication.RefreshTokenLifetime;

        // Security check: Ensure lifetime doesn't exceed maximum
        if (lifetime > _options.Authentication.MaximumTokenLifetime)
        {
            lifetime = _options.Authentication.MaximumTokenLifetime;
        }

        var token = new Token
        {
            Issuer = _issuer,
            Audience = client.ClientId,
            SubjectId = user.SubjectId,
            ClientId = client.ClientId,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(lifetime)
        };

        return Task.FromResult(token);
    }

    /// <summary>
    /// Generates a signed JWT string from a token model
    /// </summary>
    public Task<string> GenerateJwtAsync(Token token)
    {
        var handler = new JwtSecurityTokenHandler();
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, token.SubjectId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(token.IssuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var scope in token.Scopes)
        {
            claims.Add(new Claim("scope", scope));
        }

        SigningCredentials credentials;
        if (_credentialStore != null)
        {
            var key = new X509SecurityKey(_credentialStore.Certificate);
            credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        }
        else
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSigningKey!));
            credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        var jwtToken = new JwtSecurityToken(
            issuer: token.Issuer,
            audience: token.Audience,
            claims: claims,
            notBefore: token.IssuedAt,
            expires: token.ExpiresAt,
            signingCredentials: credentials
        );

        return Task.FromResult(handler.WriteToken(jwtToken));
    }

    /// <summary>
    /// Validates a JWT string
    /// </summary>
    public Task<bool> ValidateTokenAsync(string token)
    {
        return Task.FromResult(ValidateTokenInternal(token).IsValid);
    }

    /// <summary>
    /// Validates a token and returns its claims
    /// </summary>
    public Task<TokenValidationResultDetailed> ValidateTokenDetailedAsync(string token)
    {
        return Task.FromResult(ValidateTokenInternal(token));
    }

    private TokenValidationResultDetailed ValidateTokenInternal(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            
            SecurityKey key;
            if (_credentialStore != null)
            {
                key = new X509SecurityKey(_credentialStore.Certificate);
            }
            else
            {
                key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSigningKey!));
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = handler.ValidateToken(token, validationParameters, out _);
            return new TokenValidationResultDetailed 
            { 
                IsValid = true, 
                Principal = principal 
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return new TokenValidationResultDetailed 
            { 
                IsValid = false, 
                Error = ex.Message 
            };
        }
    }
}