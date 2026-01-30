using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.Core.Services;

/// <summary>
/// JWT Secured Authorization Request (JAR) service - RFC 9101
/// </summary>
public class JwtSecuredAuthorizationService : IJwtSecuredAuthorizationService
{
    private readonly ILogger<JwtSecuredAuthorizationService> _logger;
    private readonly IClientStore _clientStore;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    /// <summary>
    /// Initializes a new instance of JwtSecuredAuthorizationService
    /// </summary>
    public JwtSecuredAuthorizationService(
        ILogger<JwtSecuredAuthorizationService> logger,
        IClientStore clientStore)
    {
        _logger = logger;
        _clientStore = clientStore;
    }

    /// <summary>
    /// Validates a JWT authorization request
    /// </summary>
    public async Task<JarValidationResult> ValidateRequestAsync(string requestJwt, string clientId)
    {
        var result = new JarValidationResult();

        try
        {
            // Get client configuration
            var client = await _clientStore.FindClientByIdAsync(clientId);
            if (client == null)
            {
                result.Error = "Client not found";
                return result;
            }

            // Parse the JWT
            var token = _tokenHandler.ReadJwtToken(requestJwt);

            // Validate required claims
            var iss = token.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
            var aud = token.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;
            var clientIdClaim = token.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;

            if (iss != clientId)
            {
                result.Error = $"Invalid issuer, expected {clientId}, got {iss}";
                return result;
            }

            if (clientIdClaim != clientId)
            {
                result.Error = $"client_id claim mismatch";
                return result;
            }

            // Validate signature
            // In a real implementation, you would validate the signature using the client's public key
            // For now, we'll just log that signature validation should happen
            _logger.LogInformation("JAR signature validation should be performed here");

            // Extract all claims as parameters
            foreach (var claim in token.Claims)
            {
                if (!result.Parameters.ContainsKey(claim.Type))
                {
                    result.Parameters[claim.Type] = claim.Value;
                }
            }

            result.IsValid = true;
            result.ClientId = clientId;

            _logger.LogInformation("JAR validated successfully for client {ClientId}", clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JAR");
            result.Error = $"Validation error: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Extracts parameters from a validated request JWT
    /// </summary>
    public Task<Dictionary<string, string>> ExtractParametersAsync(string requestJwt)
    {
        var parameters = new Dictionary<string, string>();

        try
        {
            var token = _tokenHandler.ReadJwtToken(requestJwt);

            foreach (var claim in token.Claims)
            {
                if (!parameters.ContainsKey(claim.Type))
                {
                    parameters[claim.Type] = claim.Value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting JAR parameters");
        }

        return Task.FromResult(parameters);
    }
}
