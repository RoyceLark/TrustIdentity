# TrustIdentity.Server

**Meta-package for TrustIdentity - All-in-one OAuth 2.0 / OpenID Connect server**

---

## 📦 Overview

`TrustIdentity.Server` is the main meta-package that includes all necessary components to run a complete OAuth 2.0 / OpenID Connect server. This package is designed for easy installation and setup.

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.Server
```

---

## 📋 What's Included

This meta-package includes:

- ✅ **TrustIdentity.Core** - Core OAuth/OIDC engine
- ✅ **TrustIdentity.AspNetCore** - ASP.NET Core integration
- ✅ **TrustIdentity.Abstractions** - Interfaces and models
- ✅ **TrustIdentity.UI** - Login/Consent UI (optional)

---

## 🎯 Quick Start

### Basic Setup

```csharp
using TrustIdentity.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
})
.AddInMemoryClients(Config.GetClients())
.AddInMemoryIdentityResources(Config.GetIdentityResources())
.AddInMemoryApiScopes(Config.GetApiScopes())
.AddDeveloperSigningCredential();

var app = builder.Build();
app.UseTrustIdentity();
app.Run();
```

---

## 📚 Documentation

- **[Main Documentation](../../../README.md)** - Overview
- **[Setup Guide](../../../SETUP_GUIDE.md)** - Complete setup instructions
- **[Database Setup](../../../DATABASE_SETUP.md)** - Database configuration
- **[Migration Guide](../../../MIGRATION_GUIDE.md)** - Migrate from Duende

---

## 🔧 Additional Packages

### Optional Packages

```bash
# Database support
dotnet add package TrustIdentity.Storage

# SAML 2.0 support
dotnet add package TrustIdentity.Saml

# WS-Federation support
dotnet add package TrustIdentity.WsFederation

# AI fraud detection
dotnet add package TrustIdentity.AI

# External providers (Azure AD, Google, etc.)
dotnet add package TrustIdentity.ExternalProviders

# Admin UI
dotnet add package TrustIdentity.Admin

# Backend-for-Frontend
dotnet add package TrustIdentity.Bff
```

---

## ✨ Features

- ✅ OAuth 2.0 & OpenID Connect 1.0
- ✅ All 9 grant types
- ✅ PKCE, DPoP, mTLS, PAR, JAR
- ✅ FAPI 1.0 & 2.0 compliant
- ✅ 100% Duende IdentityServer compatible
- ✅ Free & Open Source (Apache 2.0)

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
