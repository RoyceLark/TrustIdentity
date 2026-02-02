# TrustIdentity.AspNetCore

**ASP.NET Core integration for TrustIdentity**

---

## 📦 Overview

`TrustIdentity.AspNetCore` provides ASP.NET Core integration, including middleware, endpoints, and dependency injection configuration.

---

## 🎯 Purpose

This package bridges TrustIdentity.Core with ASP.NET Core, providing:

- HTTP endpoints for OAuth/OIDC
- Middleware integration
- Dependency injection setup
- Request/response handling

---

## 📋 Key Components

### Endpoints

- **`/connect/authorize`** - Authorization endpoint
- **`/connect/token`** - Token endpoint
- **`/connect/userinfo`** - UserInfo endpoint
- **`/connect/introspect`** - Token introspection
- **`/connect/revocation`** - Token revocation
- **`/connect/endsession`** - Logout endpoint
- **`/connect/device`** - Device authorization
- **`/connect/ciba`** - Backchannel authentication
- **`/connect/par`** - Pushed authorization request
- **`/connect/register`** - Dynamic client registration
- **`/.well-known/openid-configuration`** - Discovery document
- **`/.well-known/jwks`** - JSON Web Key Set

### Middleware

- **`TrustIdentityMiddleware`** - Main middleware
- **`RateLimitingMiddleware`** - Rate limiting
- **`DDoSProtectionMiddleware`** - DDoS protection
- **`TenantResolutionMiddleware`** - Multi-tenancy support

### Extensions

- **`ServiceCollectionExtensions`** - DI configuration
- **`ApplicationBuilderExtensions`** - Middleware setup

---

## 🔧 Usage

### Basic Setup

```csharp
using TrustIdentity.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add TrustIdentity services
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
})
.AddInMemoryClients(Config.GetClients())
.AddInMemoryIdentityResources(Config.GetIdentityResources())
.AddInMemoryApiScopes(Config.GetApiScopes())
.AddDeveloperSigningCredential();

var app = builder.Build();

// Use TrustIdentity middleware
app.UseTrustIdentity();

app.Run();
```

### Advanced Configuration

```csharp
builder.Services.AddTrustIdentity(options =>
{
    // Issuer
    options.IssuerUri = "https://identity.example.com";
    
    // Security
    options.RequireHttps = true;
    options.RequirePkce = true;
    
    // Features
    options.EnableAI = true;
    options.EnableMultiTenancy = true;
    
    // Endpoints
    options.EnableDiscoveryEndpoint = true;
    options.EnableTokenEndpoint = true;
    options.EnableUserInfoEndpoint = true;
    options.EnableIntrospectionEndpoint = true;
    options.EnableRevocationEndpoint = true;
    
    // Token lifetimes
    options.AccessTokenLifetime = 3600;
    options.IdentityTokenLifetime = 300;
    options.RefreshTokenLifetime = 2592000;
});
```

---

## 🏗️ Architecture

```
TrustIdentity.AspNetCore/
├── Endpoints/          # HTTP endpoints
├── Middleware/         # ASP.NET Core middleware
├── Extensions/         # Service/app extensions
└── Handlers/          # Request handlers
```

---

## 📚 Documentation

- **[Setup Guide](../../../SETUP_GUIDE.md)** - Complete setup
- **[Database Setup](../../../DATABASE_SETUP.md)** - Database configuration

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
