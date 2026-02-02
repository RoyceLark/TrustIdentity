# TrustIdentity

**A complete, production-ready OAuth 2.0 / OpenID Connect server for .NET 10.0**

100% compatible with Duende IdentityServer • Free & Open Source • Enterprise-Ready

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package TrustIdentity.Server
```

### Basic Setup

```csharp
using TrustIdentity.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
})
.AddInMemoryClients(Config.Clients)
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddDeveloperSigningCredential();

var app = builder.Build();
app.UseTrustIdentity();
app.Run();
```

**See [SETUP_GUIDE.md](SETUP_GUIDE.md) for complete setup instructions**

---

## ✨ Key Features

### Core Protocols
- ✅ **OAuth 2.0** (RFC 6749) - Full implementation
- ✅ **OpenID Connect 1.0** - Complete OIDC support
- ✅ **SAML 2.0** - Identity Provider & Service Provider
- ✅ **WS-Federation** - Legacy enterprise support

### All Grant Types (9/9)
- ✅ Authorization Code
- ✅ Client Credentials
- ✅ Implicit Flow
- ✅ Hybrid Flow
- ✅ Resource Owner Password Credentials (ROPC)
- ✅ Device Authorization Flow (RFC 8628)
- ✅ Refresh Token
- ✅ Token Exchange (RFC 8693)
- ✅ Client-Initiated Backchannel Authentication (CIBA)

### Advanced Security
- ✅ **PKCE** (RFC 7636) - Proof Key for Code Exchange
- ✅ **DPoP** (RFC 9449) - Demonstrating Proof-of-Possession
- ✅ **Mutual TLS** (RFC 8705) - Certificate-bound tokens
- ✅ **PAR** (RFC 9126) - Pushed Authorization Requests
- ✅ **JAR** (RFC 9101) - JWT Secured Authorization Request
- ✅ **Resource Indicators** (RFC 8707) - Audience-restricted tokens
- ✅ **FAPI 1.0 & 2.0** - Financial-grade API compliance

### Enterprise Features
- ✅ **Multi-Tenancy** - Full tenant isolation with flexible resolution
- ✅ **External Providers** - Azure AD, Google, Facebook, GitHub
- ✅ **Dynamic Client Registration** (RFC 7591)
- ✅ **Automatic Key Rotation** - RSA/ECDSA key management
- ✅ **Backend-for-Frontend (BFF)** - Secure SPA authentication
- ✅ **Admin UI & API** - Complete administration interface
- ✅ **Rate Limiting** - DDoS protection and throttling

### AI/ML Features (Unique!)
- ✅ **Real-time Fraud Detection** - ML-based anomaly detection
- ✅ **Behavioral Analysis** - User behavior profiling
- ✅ **Risk Scoring** - Composite risk calculation
- ✅ **Adaptive Authentication** - AI-driven MFA triggers
- ✅ **Device Fingerprinting** - Track user devices

### Storage & Databases
- ✅ **Entity Framework Core** - SQL Server, PostgreSQL, MySQL, SQLite
- ✅ **In-Memory Stores** - For development
- ✅ **Redis Caching** - Distributed caching support

---

## 📊 Why TrustIdentity?

| Feature | Duende IdentityServer | TrustIdentity |
|---------|----------------------|---------------|
| OAuth 2.0 / OIDC | ✅ | ✅ |
| All Grant Types | ✅ | ✅ |
| SAML 2.0 | ✅ (Separate $$$) | ✅ **Included** |
| WS-Federation | ✅ (Separate $$$) | ✅ **Included** |
| Multi-Tenancy | ✅ (Enterprise+ $$$) | ✅ **Included** |
| Admin UI | ✅ | ✅ **Included** |
| AI Fraud Detection | ❌ | ✅ **Unique** |
| **Production License** | **$1,500-$15,000/year** | **FREE** |
| **Source Code** | Source Available | **Apache 2.0 (OSS)** |

---

## 📚 Documentation

### Essential Guides
- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Complete setup and configuration
- **[MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)** - Migrate from Duende IdentityServer
- **[DATABASE_SETUP.md](DATABASE_SETUP.md)** - Database configuration and migrations

### Advanced Topics
- **[EXTERNAL_PROVIDERS_GUIDE.md](EXTERNAL_PROVIDERS_GUIDE.md)** - Azure AD, Google, Facebook integration
- **[MIGRATION_AND_UI_GUIDE.md](MIGRATION_AND_UI_GUIDE.md)** - UI customization

---

## 🏗️ Architecture

```
TrustIdentity/
├── TrustIdentity.Core          # OAuth/OIDC engine
├── TrustIdentity.AspNetCore    # ASP.NET Core integration
├── TrustIdentity.Storage       # EF Core data access
├── TrustIdentity.UI            # Login/Consent UI
├── TrustIdentity.Admin         # Admin UI
├── TrustIdentity.AdminApi      # Admin REST API
├── TrustIdentity.Saml          # SAML 2.0 support
├── TrustIdentity.WsFederation  # WS-Federation support
├── TrustIdentity.AI            # AI fraud detection
├── TrustIdentity.ML            # ML.NET integration
├── TrustIdentity.Bff           # Backend-for-Frontend
└── TrustIdentity.ExternalProviders  # External IdP integration
```

---

## 🔧 Configuration Example

```csharp
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://identity.example.com";
    options.EnableAI = true;
    options.EnableFraudDetection = true;
    options.RequireHttps = true;
})
// Storage
.AddConfigurationStore(options =>
    options.UseSqlServer(connectionString))
.AddOperationalStore(options =>
    options.UseSqlServer(connectionString))
// Signing
.AddSigningCredential(certificate)
// External Providers
.AddExternalProvider("Google", options => { ... })
.AddExternalProvider("AzureAD", options => { ... })
// AI Features
.AddAIFraudDetection()
.AddBehaviorAnalysis()
.AddRiskScoring();
```

---

## 🗄️ Database Support

### Supported Databases
- SQL Server
- PostgreSQL
- MySQL
- SQLite
- In-Memory (Development)

### Quick Database Setup

```bash
# Add migration
dotnet ef migrations add InitialCreate -c ConfigurationDbContext

# Update database
dotnet ef database update -c ConfigurationDbContext
```

**See [DATABASE_SETUP.md](DATABASE_SETUP.md) for detailed instructions**

---

## 🔐 Security & Compliance

### Standards Compliance
- ✅ OAuth 2.0 (RFC 6749)
- ✅ OpenID Connect 1.0
- ✅ PKCE (RFC 7636)
- ✅ DPoP (RFC 9449)
- ✅ Mutual TLS (RFC 8705)
- ✅ PAR (RFC 9126)
- ✅ JAR (RFC 9101)
- ✅ Token Exchange (RFC 8693)
- ✅ Device Flow (RFC 8628)
- ✅ CIBA (RFC 9396)
- ✅ Resource Indicators (RFC 8707)
- ✅ Dynamic Client Registration (RFC 7591)

### FAPI Compliance
- ✅ FAPI 1.0 Advanced
- ✅ FAPI 2.0 Security Profile

---

## 🚢 Production Deployment

### Requirements
- .NET 10.0 Runtime
- SQL Server / PostgreSQL / MySQL
- X.509 Certificate for signing
- HTTPS enabled

### Docker Support

```bash
docker-compose up -d
```

---

## 💡 Use Cases

### Perfect For
- ✅ Enterprise SSO (Single Sign-On)
- ✅ API Security & Authorization
- ✅ Multi-tenant SaaS applications
- ✅ Mobile app authentication
- ✅ Microservices security
- ✅ Financial applications (FAPI)
- ✅ Healthcare applications (HIPAA)
- ✅ Government applications

---

## 🆚 Migration from Duende

**TrustIdentity is 100% API-compatible with Duende IdentityServer**

### Simple Migration Steps

1. **Replace NuGet package**
   ```bash
   dotnet remove package Duende.IdentityServer
   dotnet add package TrustIdentity.Server
   ```

2. **Update namespaces**
   ```csharp
   // Old
   using Duende.IdentityServer;
   
   // New
   using TrustIdentity.AspNetCore.Extensions;
   ```

3. **Update service registration**
   ```csharp
   // Old
   services.AddIdentityServer()
   
   // New
   services.AddTrustIdentity()
   ```

**See [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) for complete migration instructions**

---

## 🤝 Extensibility

TrustIdentity provides 12 extensibility points for complete customization:

- `IProfileService` - Custom user claims
- `IResourceOwnerPasswordValidator` - Custom password validation
- `IEventService` / `IEventSink` - Custom event handling
- `ICustomTokenRequestValidator` - Token request customization
- `ICustomAuthorizeRequestValidator` - Authorize request customization
- And 7 more...

```csharp
builder.Services.AddTrustIdentity(options => { ... })
    .AddProfileService<CustomProfileService>()
    .AddResourceOwnerValidator<CustomPasswordValidator>()
    .AddEventSink<DatabaseEventSink>();
```

---

## 📦 NuGet Packages

### Main Package
```bash
dotnet add package TrustIdentity.Server
```

### Individual Packages
- `TrustIdentity.Abstractions` - Core interfaces
- `TrustIdentity.Core` - Business logic
- `TrustIdentity.Storage` - EF Core support
- `TrustIdentity.AspNetCore` - Web integration
- `TrustIdentity.UI` - Login/Consent UI
- `TrustIdentity.Admin` - Admin UI
- `TrustIdentity.Saml` - SAML 2.0
- `TrustIdentity.WsFederation` - WS-Federation
- `TrustIdentity.AI` - AI fraud detection
- `TrustIdentity.Bff` - Backend-for-Frontend

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TrustIdentity.IntegrationTests
```

---

## 📄 License

**Apache 2.0** - Completely free for commercial use

- ✅ No licensing fees
- ✅ No client limits
- ✅ No user limits
- ✅ No deployment limits
- ✅ Full source code access
- ✅ Commercial use allowed

---

## 🌟 Support

- 📧 **Email**: [EMAIL_ADDRESS]
- 💬 **Discussions**: [GitHub Discussions](https://github.com/roycelark/trustidentity/discussions)
- 🐛 **Issues**: [GitHub Issues](https://github.com/roycelark/trustidentity/issues)
- 📖 **Documentation**: [Full Documentation](SETUP_GUIDE.md)

---

## 🎯 Getting Help

1. Check [SETUP_GUIDE.md](SETUP_GUIDE.md) for setup instructions
2. Check [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) for migration from Duende
3. Check [DATABASE_SETUP.md](DATABASE_SETUP.md) for database configuration
4. Open an issue on GitHub

---

**Built with ❤️ for the .NET community**

**TrustIdentity** - Enterprise Identity & Access Management, Free & Open Source
