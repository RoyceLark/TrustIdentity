using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Configuration;
using TrustIdentity.Core.Services;
using TrustIdentity.Abstractions.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Builder;
using System.Threading;

namespace TrustIdentity.AspNetCore.Extensions;

/// <summary>
/// Extension methods for setting up TrustIdentity services in an <see cref="IServiceCollection" />
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds TrustIdentity services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">The action to configure TrustIdentity options</param>
    /// <returns>A TrustIdentity builder</returns>
    public static TrustIdentityBuilder AddTrustIdentity(
        this IServiceCollection services,
        Action<TrustIdentityOptions> configureOptions)
    {
        var options = new TrustIdentityOptions();
        configureOptions(options);

        services.AddSingleton(options);

        // Core services
        services.AddScoped<ClientService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>(); // Production Email Sender
        services.AddScoped<AuthorizationCodeService>();
        services.AddScoped<IAuthorizationCodeService>(sp => sp.GetRequiredService<AuthorizationCodeService>());
        services.AddScoped<IAuthorizationCodeStore>(sp => sp.GetRequiredService<AuthorizationCodeService>());
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IDeviceFlowService, DeviceFlowService>();
        services.AddScoped<SessionManagementService>();
        services.AddScoped<ConsentService>();
        services.AddScoped<CibaService>();
        services.AddScoped<TotpService>();
        services.AddScoped<AccountService>();
        services.AddScoped<EventService>();
        services.AddScoped<ClaimsService>(); 
        services.AddScoped<ResponseTypeHandler>();
        services.AddScoped<CorsPolicyService>();
        services.AddScoped<IPersistedGrantStore, InMemoryPersistedGrantStore>();
        services.AddScoped<IPasswordHasher, TrustIdentity.Core.Security.PasswordHasher>();
        
        // RFC 9126 - Pushed Authorization Requests
        services.AddScoped<IPushedAuthorizationService, TrustIdentity.Core.Services.PushedAuthorizationService>();
        
        // RFC 8707 - Resource Indicators
        services.AddScoped<IResourceIndicatorService, TrustIdentity.Core.Services.ResourceIndicatorService>();

        // RFC 9449 - DPoP
        services.AddScoped<IDPoPService, TrustIdentity.Core.Services.DPoPService>();

        // RFC 8693 - Token Exchange
        services.AddScoped<ITokenExchangeService, TrustIdentity.Core.Services.TokenExchangeService>();

        // RFC 8705 - Mutual TLS
        services.AddScoped<IMutualTlsService, TrustIdentity.Core.Services.MutualTlsService>();

        // RFC 9101 - JAR
        services.AddScoped<IJwtSecuredAuthorizationService, TrustIdentity.Core.Services.JwtSecuredAuthorizationService>();

        // Automated Key Management
        services.AddSingleton<IKeyManagementService, TrustIdentity.Core.Services.KeyManagementService>();

        // RFC 7591 - Dynamic Client Registration
        services.AddScoped<IDynamicClientRegistrationService, TrustIdentity.Core.Services.DynamicClientRegistrationService>();

        // Validation services
        services.AddScoped<TrustIdentity.Core.Validation.PkceValidator>();
        services.AddScoped<TrustIdentity.Core.Validation.ClientValidator>();
        services.AddScoped<TrustIdentity.Core.Validation.ScopeValidator>();
        services.AddScoped<TrustIdentity.Core.Validation.AuthorizeRequestValidator>();
        services.AddScoped<TrustIdentity.Core.Validation.TokenRequestValidator>();

        // Extensibility services 
        services.AddScoped<IProfileService, DefaultProfileService>();
        services.AddScoped<IEventSink, DefaultEventSink>();
        services.AddScoped<IEventService, DefaultEventService>();
        services.AddScoped<ICorsPolicyService, DefaultCorsPolicyService>();

        // Rate limiting
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddFixedWindowLimiter("auth", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(5);
                opt.PermitLimit = 10;
                opt.QueueLimit = 0;
            });

            rateLimiterOptions.AddFixedWindowLimiter("token", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 20;
                opt.QueueLimit = 0;
            });
        });
        
        // Licensing (Disabled by default)
        if (options.EnableLicensing)
        {
            // Note: Since we want to keep the core library lightweight, we use reflection or 
            // the user must manually add the project reference. For now, we just log/prepare.
        }

        return new TrustIdentityBuilder(services);
    }
}

/// <summary>
/// Builder for configuring TrustIdentity services
/// </summary>
public class TrustIdentityBuilder
{
    /// <summary>
    /// The service collection
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the TrustIdentityBuilder
    /// </summary>
    /// <param name="services">The service collection</param>
    public TrustIdentityBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Adds in-memory clients
    /// </summary>
    /// <param name="clients">The clients</param>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddInMemoryClients(IEnumerable<Client> clients)
    {
        var store = new InMemoryClientStore(clients);
        Services.AddSingleton<IClientStore>(store);
        return this;
    }

    /// <summary>
    /// Adds in-memory identity resources
    /// </summary>
    /// <param name="resources">The identity resources</param>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddInMemoryIdentityResources(IEnumerable<IdentityResource> resources)
    {
        var store = new InMemoryIdentityResourceStore(resources);
        Services.AddSingleton<IResourceStore>(store);
        return this;
    }

    /// <summary>
    /// Adds in-memory API scopes
    /// </summary>
    /// <param name="scopes">The API scopes</param>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddInMemoryApiScopes(IEnumerable<ApiScope> scopes)
    {
        var store = new InMemoryApiScopeStore(scopes);
        Services.AddSingleton<IApiScopeStore>(store);
        return this;
    }

    /// <summary>
    /// Adds in-memory API resources
    /// </summary>
    /// <param name="resources">The API resources</param>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddInMemoryApiResources(IEnumerable<ApiResource> resources)
    {
        var store = new InMemoryApiResourceStore(resources);
        Services.AddSingleton<IApiResourceStore>(store);
        return this;
    }

    /// <summary>
    /// Adds test users
    /// </summary>
    /// <param name="users">The test users</param>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddTestUsers(IEnumerable<TestUser> users)
    {
        Services.AddSingleton<IUserStore>(sp => 
            new InMemoryUserStore(users, sp.GetService<IPasswordHasher>()));
        return this;
    }

    /// <summary>
    /// Adds a signing credential
    /// </summary>
    /// <param name="certificate">The certificate</param>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddSigningCredential(System.Security.Cryptography.X509Certificates.X509Certificate2 certificate)
    {
        Services.AddSingleton(new SigningCredentialStore(certificate));
        return this;
    }

    /// <summary>
    /// Adds a developer signing credential
    /// </summary>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddDeveloperSigningCredential()
    {
        var certificate = CreateDevelopmentCertificate();
        Services.AddSingleton(new SigningCredentialStore(certificate));
        return this;
    }

    /// <summary>
    /// Adds AI fraud detection services
    /// </summary>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddAIFraudDetection()
    {
        Services.AddScoped<IFraudDetectionService, TrustIdentity.AI.Analyzers.FraudDetectionService>();
        return this;
    }

    /// <summary>
    /// Adds behavior analysis services
    /// </summary>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddBehaviorAnalysis()
    {
        // Services.AddSingleton<TrustIdentity.AI.Analyzers.BehaviorAnalysisService>();
        return this;
    }

    /// <summary>
    /// Adds risk scoring services
    /// </summary>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddRiskScoring()
    {
        // Services.AddSingleton<TrustIdentity.AI.Analyzers.RiskScoringService>();
        return this;
    }

    /// <summary>
    /// Adds a custom profile service
    /// </summary>
    /// <typeparam name="T">The profile service type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddProfileService<T>() where T : class, IProfileService
    {
        Services.AddScoped<IProfileService, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom resource owner password validator
    /// </summary>
    /// <typeparam name="T">The validator type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddResourceOwnerValidator<T>() where T : class, IResourceOwnerPasswordValidator
    {
        Services.AddScoped<IResourceOwnerPasswordValidator, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom event sink
    /// </summary>
    /// <typeparam name="T">The event sink type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddEventSink<T>() where T : class, IEventSink
    {
        Services.AddScoped<IEventSink, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom token request validator
    /// </summary>
    /// <typeparam name="T">The validator type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddCustomTokenRequestValidator<T>() where T : class, ICustomTokenRequestValidator
    {
        Services.AddScoped<ICustomTokenRequestValidator, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom authorize request validator
    /// </summary>
    /// <typeparam name="T">The validator type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddCustomAuthorizeRequestValidator<T>() where T : class, ICustomAuthorizeRequestValidator
    {
        Services.AddScoped<ICustomAuthorizeRequestValidator, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom authorize interaction response generator
    /// </summary>
    /// <typeparam name="T">The generator type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddAuthorizeInteractionResponseGenerator<T>() where T : class, IAuthorizeInteractionResponseGenerator
    {
        Services.AddScoped<IAuthorizeInteractionResponseGenerator, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom CORS policy service
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddCorsPolicyService<T>() where T : class, ICorsPolicyService
    {
        Services.AddScoped<ICorsPolicyService, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom token creation service
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddTokenCreationService<T>() where T : class, ITokenCreationService
    {
        Services.AddScoped<ITokenCreationService, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom token response generator
    /// </summary>
    /// <typeparam name="T">The generator type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddTokenResponseGenerator<T>() where T : class, ITokenResponseGenerator
    {
        Services.AddScoped<ITokenResponseGenerator, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom claims service
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddClaimsService<T>() where T : class, IClaimsService
    {
        Services.AddScoped<IClaimsService, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom consent service
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddConsentService<T>() where T : class, IConsentService
    {
        Services.AddScoped<IConsentService, T>();
        return this;
    }

    /// <summary>
    /// Adds a custom return URL parser
    /// </summary>
    /// <typeparam name="T">The parser type</typeparam>
    /// <returns>The builder</returns>
    public TrustIdentityBuilder AddReturnUrlParser<T>() where T : class, IReturnUrlParser
    {
        Services.AddScoped<IReturnUrlParser, T>();
        return this;
    }

    private static System.Security.Cryptography.X509Certificates.X509Certificate2 CreateDevelopmentCertificate()
    {
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        var request = new System.Security.Cryptography.X509Certificates.CertificateRequest(
            "CN=TrustIdentity Development",
            rsa,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));

        return certificate;
    }
}
