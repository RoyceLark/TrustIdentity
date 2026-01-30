using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Core.Services;

/// <summary>
/// DPoP (Demonstrating Proof-of-Possession) service - RFC 9449
/// </summary>
public class DPoPService : IDPoPService
{
    private readonly ILogger<DPoPService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    /// <summary>
    /// Initializes a new instance of DPoPService
    /// </summary>
    public DPoPService(ILogger<DPoPService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates a DPoP proof
    /// </summary>
    public Task<DPoPValidationResult> ValidateDPoPProofAsync(string dpopProof, string httpMethod, string httpUri)
    {
        var result = new DPoPValidationResult();

        try
        {
            // Parse the JWT
            var token = _tokenHandler.ReadJwtToken(dpopProof);

            // Validate header
            if (token.Header.Typ != "dpop+jwt")
            {
                result.Error = "Invalid typ header, must be dpop+jwt";
                return Task.FromResult(result);
            }

            if (token.Header.Alg != "RS256" && token.Header.Alg != "ES256")
            {
                result.Error = "Invalid alg header, must be RS256 or ES256";
                return Task.FromResult(result);
            }

            // Validate required claims
            var jti = token.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
            var htm = token.Claims.FirstOrDefault(c => c.Type == "htm")?.Value;
            var htu = token.Claims.FirstOrDefault(c => c.Type == "htu")?.Value;
            var iat = token.Claims.FirstOrDefault(c => c.Type == "iat")?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                result.Error = "Missing jti claim";
                return Task.FromResult(result);
            }

            if (htm != httpMethod)
            {
                result.Error = $"htm claim mismatch, expected {httpMethod}, got {htm}";
                return Task.FromResult(result);
            }

            if (htu != httpUri)
            {
                result.Error = $"htu claim mismatch, expected {httpUri}, got {htu}";
                return Task.FromResult(result);
            }

            // Validate timestamp (must be recent, within 60 seconds)
            if (!string.IsNullOrEmpty(iat))
            {
                var issuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(iat));
                var now = DateTimeOffset.UtcNow;
                
                if (Math.Abs((now - issuedAt).TotalSeconds) > 60)
                {
                    result.Error = "DPoP proof is too old or from the future";
                    return Task.FromResult(result);
                }
            }

            // Extract JWK from header
            var jwk = token.Header["jwk"] as JsonElement?;
            if (jwk == null)
            {
                result.Error = "Missing jwk in header";
                return Task.FromResult(result);
            }

            // Generate thumbprint
            var thumbprint = GenerateJwkThumbprint(jwk.Value);
            result.Thumbprint = thumbprint;
            result.PublicKey = jwk.Value.ToString();
            result.IsValid = true;

            _logger.LogInformation("DPoP proof validated successfully, thumbprint: {Thumbprint}", thumbprint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating DPoP proof");
            result.Error = $"Validation error: {ex.Message}";
        }

        return Task.FromResult(result);
    }

    /// <summary>
    /// Generates a DPoP token thumbprint
    /// </summary>
    public Task<string> GenerateThumbprintAsync(string dpopProof)
    {
        try
        {
            var token = _tokenHandler.ReadJwtToken(dpopProof);
            var jwk = token.Header["jwk"] as JsonElement?;
            
            if (jwk == null)
            {
                throw new InvalidOperationException("Missing jwk in DPoP proof");
            }

            var thumbprint = GenerateJwkThumbprint(jwk.Value);
            return Task.FromResult(thumbprint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating DPoP thumbprint");
            throw;
        }
    }

    /// <summary>
    /// Creates a DPoP-bound access token
    /// </summary>
    public Task<string> CreateDPoPBoundTokenAsync(string accessToken, string dpopThumbprint)
    {
        // In a real implementation, this would add the cnf claim to the access token
        // For now, we'll just return a marker that this token is DPoP-bound
        var boundToken = $"{accessToken}:dpop:{dpopThumbprint}";
        
        _logger.LogInformation("Created DPoP-bound token with thumbprint {Thumbprint}", dpopThumbprint);
        
        return Task.FromResult(boundToken);
    }

    /// <summary>
    /// Generates JWK thumbprint according to RFC 7638
    /// </summary>
    private string GenerateJwkThumbprint(JsonElement jwk)
    {
        // Extract required fields based on key type
        var kty = jwk.GetProperty("kty").GetString();
        
        string canonicalJson;
        
        if (kty == "RSA")
        {
            var e = jwk.GetProperty("e").GetString();
            var n = jwk.GetProperty("n").GetString();
            canonicalJson = $"{{\"e\":\"{e}\",\"kty\":\"RSA\",\"n\":\"{n}\"}}";
        }
        else if (kty == "EC")
        {
            var crv = jwk.GetProperty("crv").GetString();
            var x = jwk.GetProperty("x").GetString();
            var y = jwk.GetProperty("y").GetString();
            canonicalJson = $"{{\"crv\":\"{crv}\",\"kty\":\"EC\",\"x\":\"{x}\",\"y\":\"{y}\"}}";
        }
        else
        {
            throw new NotSupportedException($"Key type {kty} not supported for thumbprint");
        }

        // Hash the canonical JSON
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalJson));
        
        // Base64url encode
        return Base64UrlEncoder.Encode(hash);
    }
}
