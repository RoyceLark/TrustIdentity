using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrustIdentity.Abstractions.Models
{
    /// <summary>
    /// Represents a request for dynamic client registration
    /// </summary>
    public class DynamicClientRegistrationRequest
    {
        /// <summary>Name of the client</summary>
        [JsonPropertyName("client_name")]
        public string ClientName { get; set; } = string.Empty;

        /// <summary>URI of the client</summary>
        [JsonPropertyName("client_uri")]
        public string ClientUri { get; set; } = string.Empty;

        /// <summary>URI of the client logo</summary>
        [JsonPropertyName("logo_uri")]
        public string LogoUri { get; set; } = string.Empty;

        /// <summary>Redirect URIs for the client</summary>
        [JsonPropertyName("redirect_uris")]
        public List<string> RedirectUris { get; set; } = new();

        /// <summary>Grant types allowed for the client</summary>
        [JsonPropertyName("grant_types")]
        public List<string> GrantTypes { get; set; } = new();

        /// <summary>Response types allowed for the client</summary>
        [JsonPropertyName("response_types")]
        public List<string> ResponseTypes { get; set; } = new();

        /// <summary>Scopes allowed for the client</summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        /// <summary>Token endpoint authentication method</summary>
        [JsonPropertyName("token_endpoint_auth_method")]
        public string TokenEndpointAuthMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a response for dynamic client registration
    /// </summary>
    public class DynamicClientRegistrationResponse
    {
        /// <summary>The issued client ID</summary>
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>The issued client secret</summary>
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>Epoch time when the client ID was issued</summary>
        [JsonPropertyName("client_id_issued_at")]
        public long ClientIdIssuedAt { get; set; }

        /// <summary>Epoch time when the client secret expires</summary>
        [JsonPropertyName("client_secret_expires_at")]
        public long ClientSecretExpiresAt { get; set; }

        /// <summary>Name of the client</summary>
        [JsonPropertyName("client_name")]
        public string ClientName { get; set; } = string.Empty;

        /// <summary>Redirect URIs for the client</summary>
        [JsonPropertyName("redirect_uris")]
        public List<string> RedirectUris { get; set; } = new();

        /// <summary>Grant types allowed for the client</summary>
        [JsonPropertyName("grant_types")]
        public List<string> GrantTypes { get; set; } = new();

        /// <summary>Response types allowed for the client</summary>
        [JsonPropertyName("response_types")]
        public List<string> ResponseTypes { get; set; } = new();

        /// <summary>Scopes allowed for the client</summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        /// <summary>Token endpoint authentication method</summary>
        [JsonPropertyName("token_endpoint_auth_method")]
        public string TokenEndpointAuthMethod { get; set; } = string.Empty;
    }
}
