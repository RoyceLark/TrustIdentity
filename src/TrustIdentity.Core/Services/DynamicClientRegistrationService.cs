using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.Core.Services
{
    /// <summary>
    /// Implementation of the Dynamic Client Registration service
    /// </summary>
    public class DynamicClientRegistrationService : IDynamicClientRegistrationService
    {
        private readonly IClientStore _clientStore;

        /// <summary>
        /// Initializes a new instance of the DynamicClientRegistrationService
        /// </summary>
        public DynamicClientRegistrationService(IClientStore clientStore)
        {
            _clientStore = clientStore;
        }

        /// <inheritdoc/>
        public async Task<DynamicClientRegistrationResponse> RegisterClientAsync(DynamicClientRegistrationRequest request)
        {
            if (string.IsNullOrEmpty(request.ClientName))
            {
                throw new ArgumentException("Client Name is required.");
            }

            if (request.RedirectUris == null || request.RedirectUris.Count == 0)
            {
                throw new ArgumentException("Redirect URIs are required.");
            }

            var clientId = GenerateClientId();
            var plainSecret = GenerateClientSecret();
            
            // Default to authorization_code if not specified
            var grantTypes = request.GrantTypes != null && request.GrantTypes.Any() 
                ? request.GrantTypes 
                : new List<string> { "authorization_code" };

            var client = new Client
            {
                ClientId = clientId,
                ClientName = request.ClientName,
                ClientUri = request.ClientUri,
                LogoUri = request.LogoUri,
                AllowedScopes = request.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                RedirectUris = request.RedirectUris,
                AllowedGrantTypes = grantTypes,
                RequireClientSecret = true,
                ClientSecrets = new List<Secret> { plainSecret.Sha256() },
                Created = DateTime.UtcNow,
                Enabled = true,
                // Default settings for dynamically registered clients
                AccessTokenLifetime = 3600,
                AuthorizationCodeLifetime = 300,
                IdentityTokenLifetime = 300,
                AllowAccessTokensViaBrowser = grantTypes.Contains("implicit"),
                RequirePkce = true // Force PKCE for new clients
            };
            
            // Map TokenEndpointAuthMethod if needed, defaulting to client_secret_basic/post which is handled by secret presence.

            await _clientStore.AddClientAsync(client);

            return new DynamicClientRegistrationResponse
            {
                ClientId = clientId,
                ClientSecret = plainSecret,
                ClientName = client.ClientName,
                ClientIdIssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ClientSecretExpiresAt = 0,
                RedirectUris = client.RedirectUris,
                GrantTypes = client.AllowedGrantTypes,
                ResponseTypes = request.ResponseTypes,
                Scope = request.Scope ?? string.Empty,
                TokenEndpointAuthMethod = request.TokenEndpointAuthMethod ?? "client_secret_basic"
            };
        }

        private string GenerateClientId()
        {
             return Guid.NewGuid().ToString("N");
        }
        
        private string GenerateClientSecret()
        {
             // 32 bytes = 256 bits of entropy
             var bytes = new byte[32];
             using var rng = RandomNumberGenerator.Create();
             rng.GetBytes(bytes);
             return Convert.ToBase64String(bytes);
        }
    }
}
