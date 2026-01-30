using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Core.Services
{
    /// <summary>
    /// Implementation of the Token Exchange service
    /// </summary>
    public class TokenExchangeService : ITokenExchangeService
    {
        private readonly ITokenService _tokenService;

        /// <summary>
        /// Initializes a new instance of the TokenExchangeService
        /// </summary>
        public TokenExchangeService(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <inheritdoc/>
        public async Task<TokenExchangeResult> ExchangeAsync(string subjectToken, string subjectTokenType, string? actorToken = null, string? actorTokenType = null)
        {
            // Only strictly support access token exchange for now
            if (subjectTokenType != "urn:ietf:params:oauth:token-type:access_token")
            {
                return new TokenExchangeResult 
                { 
                    IsError = true, 
                    Error = "invalid_request", 
                    ErrorDescription = "Only access_token subject_token_type is supported" 
                };
            }

            var validationResult = await _tokenService.ValidateTokenDetailedAsync(subjectToken);
            if (!validationResult.IsValid || validationResult.Principal == null)
            {
                return new TokenExchangeResult 
                { 
                    IsError = true, 
                    Error = "invalid_token", 
                    ErrorDescription = "Invalid subject_token" 
                };
            }

            // Extract user information from valid token claims
            var subIds = validationResult.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                        ?? validationResult.Principal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(subIds))
            {
                 return new TokenExchangeResult 
                { 
                    IsError = true, 
                    Error = "invalid_token", 
                    ErrorDescription = "Subject token missing sub claim" 
                };
            }
            
            var user = new User
            {
                SubjectId = subIds,
                Username = validationResult.Principal.Identity?.Name ?? subIds,
                // Additional claims propagation could happen here
            };

            return new TokenExchangeResult
            {
                IsError = false,
                User = user
            };
        }
    }
}
