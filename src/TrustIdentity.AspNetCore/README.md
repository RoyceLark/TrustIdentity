# TrustIdentity - Complete Identity & Access Management for .NET

[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/badge/NuGet-1.0.0-blue)](https://www.nuget.org/packages/TrustIdentity.Server)

**TrustIdentity** is a complete, production-ready OpenID Connect and OAuth 2.0 framework for .NET 9/10 with advanced AI/ML capabilities. Built with 100% feature parity to Other IdentityServer, plus unique fraud detection and behavioral analysis features.

## 🎯 One-Command Installation

```bash
dotnet add package TrustIdentity.Server
```

That's it! One package includes everything.

## ✨ Key Features

### 🌐 Complete Protocol Support
- ✅ **OpenID Connect (OIDC)** 1.0
- ✅ **OAuth 2.0** (RFC 6749)
- ✅ **SAML 2.0** (Identity Provider & Service Provider)
- ✅ **WS-Federation** 1.2
- ✅ **All 8 Grant Types**: Authorization Code, Client Credentials, Implicit, Hybrid, ROPC, Device Flow, Refresh Token, Token Exchange

### 🔐 Production-Grade Security
- ✅ **PKCE** (RFC 7636) & **DPoP** (RFC 9449)
- ✅ **PAR** (RFC 9126) & **mTLS** (RFC 8705)
- ✅ **JWT & Reference tokens**
- ✅ **Token Encryption & Signing** (RSA, EC)
- ✅ **Key Management** & Automatic Rotation
- ✅ **Security Headers** & Rate Limiting
- ✅ **CORS** & Account Lockout

### 🧠 AI/ML Capabilities (Unique!)
- ✅ **Real-time fraud detection** with ML.NET
- ✅ **Behavioral analysis** - pattern recognition
- ✅ **Risk scoring** - composite risk calculation
- ✅ **Adaptive authentication** - AI-driven MFA
- ✅ **Anomaly detection** - suspicious activity alerts
- ✅ **Device fingerprinting** - track user devices

### 💾 Enterprise Storage
- ✅ **Entity Framework Core**
- ✅ SQL Server, PostgreSQL, MySQL, SQLite
- ✅ In-memory stores for development
- ✅ Distributed caching (Redis)

## 🚀 Quick Start

```csharp
using TrustIdentity.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add TrustIdentity with AI/ML
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
    options.EnableAI = true;
    options.EnableFraudDetection = true;
})
.AddInMemoryClients(Config.Clients)
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddDeveloperSigningCredential()
.AddAIFraudDetection()
.AddBehaviorAnalysis()
.AddRiskScoring();

var app = builder.Build();
app.UseTrustIdentity();
app.Run();
```

## 📦 Package Structure

**TrustIdentity.Server** (meta-package) includes:

- `TrustIdentity.Abstractions` - Core interfaces
- `TrustIdentity.Core` - Business logic & models
- `TrustIdentity.Storage` - EF Core persistence
- `TrustIdentity.AspNetCore` - Web integration
- `TrustIdentity.Saml` - SAML 2.0 implementation
- `TrustIdentity.WsFederation` - WS-Federation implementation
- `TrustIdentity.AI` - AI fraud detection
- `TrustIdentity.ML` - ML.NET models

## 🎓 Documentation

- [Getting Started](docs/getting-started.md)
- [OIDC & OAuth 2.0 Guide](docs/oidc-oauth.md)
- [SAML 2.0 Guide](src/TrustIdentity.Saml/README.md)
- [WS-Federation Guide](src/TrustIdentity.WsFederation/README.md)
- [NuGet Publishing Guide](docs/NUGET_GUIDE.md)
- [Project Overview](PROJECT_OVERVIEW.md)
- [Contributing](CONTRIBUTING.md)

## 💡 Why TrustIdentity?

| Feature | Others | TrustIdentity |
|---------|--------|---------------|
| OAuth 2.0 / OIDC | ✅ | ✅ |
| **SAML 2.0** | ✅ | ✅ |
| **WS-Federation** | ✅ | ✅ |
| All Grant Types | ✅ | ✅ |
| EF Core Storage | ✅ | ✅ |
| **AI Fraud Detection** | ❌ | ✅ |
| **Behavioral Analysis** | ❌ | ✅ |
| **Risk Scoring** | ❌ | ✅ |
| **License** | Commercial | **Apache 2.0 (FREE)** |
| **Cost** | $1,500+/year | **$0** |

##  Roadmap

- [x] Complete OAuth 2.0 & OpenID Connect
- [x] AI/ML fraud detection
- [x] Entity Framework Core support
- [x] SAML 2.0 support
- [x] WS-Federation support
- [ ] Azure AD B2C compatibility (External Provider)
- [ ] Admin UI
- [ ] Multi-tenancy

## 📄 License

Apache 2.0 - See [LICENSE](LICENSE) for details.
This project is completely free and open source. No license fees, ever.

## 🤝 Contributing

Contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## 🌟 Support

- 📧 Email: web.html123@gmail.com
- 💬 Discussions: [GitHub Discussions](https://github.com/roycelark/trustidentity/discussions)
- 🐛 Issues: [GitHub Issues](https://github.com/roycelark/trustidentity/issues)

---

**Built with ❤️ for the .NET community**

**TrustIdentity** - Enterprise Identity & Access Management, AI/ML-Powered.
