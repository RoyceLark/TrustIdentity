using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TrustIdentity.Abstractions.Configuration;

namespace TrustIdentity.AspNetCore.Endpoints;

/// <summary>
/// Provides extension methods to map TrustIdentity endpoints
/// </summary>
public static class TrustIdentityEndpoints
{
    /// <summary>
    /// Maps the TrustIdentity OIDC/OAuth 2.0 endpoints to the routing system
    /// </summary>
    /// <param name="endpoints">The endpoint route builder</param>
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // OAuth 2.0 / OpenID Connect Discovery
        endpoints.MapGet("/.well-known/openid-configuration", async context =>
        {
            var options = context.RequestServices.GetRequiredService<TrustIdentityOptions>();
            var issuer = options.IssuerUri.TrimEnd('/');
            
            await context.Response.WriteAsJsonAsync(new
            {
                issuer = issuer,
                authorization_endpoint = $"{issuer}/connect/authorize",
                token_endpoint = $"{issuer}/connect/token",
                userinfo_endpoint = $"{issuer}/connect/userinfo",
                jwks_uri = $"{issuer}/.well-known/jwks",
                end_session_endpoint = $"{issuer}/connect/endsession",
                revocation_endpoint = $"{issuer}/connect/revocation",
                introspection_endpoint = $"{issuer}/connect/introspect",
                device_authorization_endpoint = $"{issuer}/connect/device",
                backchannel_authentication_endpoint = $"{issuer}/connect/ciba",
                pushed_authorization_request_endpoint = $"{issuer}/connect/par",
                registration_endpoint = $"{issuer}/connect/register",
                
                scopes_supported = new[] { "openid", "profile", "email", "offline_access" },
                id_token_signing_alg_values_supported = context.RequestServices.GetService<SigningCredentialStore>() != null ? new[] { "RS256" } : new[] { "HS256" },
                response_types_supported = new[] { "code", "token", "id_token" },
                grant_types_supported = new[] { "authorization_code", "client_credentials", "password", "refresh_token", "urn:ietf:params:oauth:grant-type:device_code", "urn:openid:params:grant-type:ciba" },
                subject_types_supported = new[] { "public" },
                code_challenge_methods_supported = new[] { "S256", "plain" },
                request_parameter_supported = true,
                request_uri_parameter_supported = true,
                require_request_uri_registration = false,

                // RFC 9449 - DPoP
                dpop_signing_alg_values_supported = new[] { "RS256", "ES256" },

                // RFC 8705 - Mutual TLS
                tls_client_certificate_bound_access_tokens = true,

                // RFC 9101 - JAR
                request_object_signing_alg_values_supported = new[] { "RS256", "ES256" }
            });
        });

        // RFC 9126 - Pushed Authorization Request endpoint
        endpoints.MapPost("/connect/par", async context =>
        {
            var parService = context.RequestServices.GetRequiredService<TrustIdentity.Abstractions.Services.IPushedAuthorizationService>();
            var clientStore = context.RequestServices.GetRequiredService<TrustIdentity.Abstractions.Stores.IClientStore>();
            var loggerFactory = context.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("TrustIdentity.AspNetCore.Endpoints.PushedAuthorizationEndpoint");
            
            await PushedAuthorizationEndpoint.HandleAsync(context, parService, clientStore, logger);
        }).RequireRateLimiting("auth");

        // RFC 7591 - Dynamic Client Registration
        endpoints.MapPost("/connect/register", async context =>
        {
             var service = context.RequestServices.GetRequiredService<TrustIdentity.Abstractions.Services.IDynamicClientRegistrationService>();
             await DynamicClientRegistrationEndpoint.Handle(context, service);
        }).RequireRateLimiting("auth");

        // SAML 2.0 Metadata
        endpoints.MapGet("/saml/metadata", context => 
        {
            return TrustIdentity.Saml.Endpoints.SamlMetadataEndpoint.HandleIdpMetadataAsync(context);
        });

        // SAML 2.0 Single Sign-On
        endpoints.MapGet("/saml/sso", context => 
        {
            return TrustIdentity.Saml.Endpoints.SamlSsoEndpoint.HandleAsync(context);
        });

        endpoints.MapPost("/saml/sso", context => 
        {
            return TrustIdentity.Saml.Endpoints.SamlSsoEndpoint.HandleAsync(context);
        });

        // SAML 2.0 Assertion Consumer Service (for SP role)
        endpoints.MapPost("/saml/acs", context => 
        {
            return TrustIdentity.Saml.Endpoints.SamlAcsEndpoint.HandleAsync(context);
        });

        // OAuth 2.0 Authorization endpoint
        endpoints.MapGet("/connect/authorize", AuthorizationEndpoint.HandleAsync)
            .RequireRateLimiting("auth");

        // OAuth 2.0 Token endpoint
        endpoints.MapPost("/connect/token", TokenEndpointHandlers.HandleTokenRequestAsync)
            .RequireRateLimiting("token");

        // OpenID Connect UserInfo endpoint
        endpoints.MapGet("/connect/userinfo", UserInfoEndpoint.HandleAsync);

        // OAuth 2.0 Revocation endpoint
        endpoints.MapPost("/connect/revocation", RevocationEndpoint.HandleAsync)
            .RequireRateLimiting("token");

        // OAuth 2.0 Introspection endpoint
        endpoints.MapPost("/connect/introspect", IntrospectionEndpoint.HandleAsync)
            .RequireRateLimiting("token");

        // OIDC End Session endpoint
        endpoints.MapGet("/connect/endsession", EndSessionEndpoint.HandleAsync);

        // Device Authorization endpoint
        endpoints.MapPost("/connect/device", DeviceAuthorizationEndpoint.HandleAsync)
            .RequireRateLimiting("auth");

        // CIBA Backchannel Authentication endpoint
        endpoints.MapPost("/connect/ciba", BackchannelAuthenticationEndpoint.HandleAsync)
            .RequireRateLimiting("auth");

        // JWKS endpoint
        endpoints.MapGet("/.well-known/jwks", async context =>
        {
            var credentialStore = context.RequestServices.GetService<SigningCredentialStore>();
            if (credentialStore != null)
            {
                var key = new Microsoft.IdentityModel.Tokens.X509SecurityKey(credentialStore.Certificate);
                var jwk = Microsoft.IdentityModel.Tokens.JsonWebKeyConverter.ConvertFromSecurityKey(key);
                
                // Set metadata
                jwk.Kid = credentialStore.Certificate.Thumbprint;
                jwk.Use = "sig";
                jwk.Alg = Microsoft.IdentityModel.Tokens.SecurityAlgorithms.RsaSha256;

                await context.Response.WriteAsJsonAsync(new
                {
                    keys = new[] { 
                        new {
                            kty = jwk.Kty,
                            use = jwk.Use,
                            kid = jwk.Kid,
                            alg = jwk.Alg,
                            e = jwk.E,
                            n = jwk.N,
                            x5c = new[] { Convert.ToBase64String(credentialStore.Certificate.RawData) }
                        }
                    }
                });
            }
            else
            {
                await context.Response.WriteAsJsonAsync(new
                {
                    keys = new object[] { }
                });
            }
        });
    }
}