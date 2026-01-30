# TrustIdentity

TrustIdentity is a comprehensive OAuth 2.0 / OpenID Connect framework for .NET 10.0, designed to provide a secure, standards-compliant identity solution for modern applications. It is built as a robust alternative to Duende IdentityServer, offering enterprise-grade features out of the box.

## 🌟 Key Features

### Core Protocols
- **OAuth 2.0 & OpenID Connect 1.1:** Full compliance with core specs.
- **Certified Grant Types:** Authorization Code, Client Credentials, Refresh Token, Device Flow, CIBA, Token Exchange.

### Security First
- **Pushed Authorization Requests (PAR):** Mandatory support for RFC 9126 to prevent parameter tampering.
- **DPoP (Demonstrating Proof-of-Possession):** RFC 9449 support for sender-constrained tokens.
- **Mutual TLS (mTLS):** RFC 8705 support for certificate-bound access tokens.
- **Financial-grade API (FAPI):** Ready for FAPI 1.0 Advanced and FAPI 2.0 compliance.
- **Resource Indicators:** RFC 8707 support for audience-restricted tokens.

### Enterprise Ready
- **Multi-Tenancy:** Full support for multi-tenant deployments with flexible tenant resolution strategies.
- **External Providers:** Azure AD, Azure AD B2C, Google, Facebook, GitHub integration.
- **Dynamic Client Registration:** RFC 7591 support for automated client onboarding.
- **Token Exchange:** RFC 8693 support for complex delegation and impersonation scenarios.
- **Automatic Key Rotation:** Built-in service for RSA/ECDSA signing key lifecycle management.
- **AI Fraud Detection:** Intelligent behavioral analysis to detect and block suspicious access.
- **Backend-for-Frontend (BFF):** Secure cookie-based session management for SPAs.
- **Admin UI:** Comprehensive administration interface for managing tenants, clients, users, and resources.

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- Docker (optional, for persistent stores)

### Quick Start
To run the sample Identity Provider and UI:

```bash
cd samples/QuickStart
dotnet run
```

Access the UI at `https://localhost:5001`.
Default credentials: `alice` / `Password123!`

## 📚 Documentation

- **[Setup Guide](SETUP_GUIDE.md):** Step-by-step instructions for integrating TrustIdentity into your apps.
- **[Multi-Tenancy Guide](MULTITENANCY_GUIDE.md):** Complete guide for multi-tenant deployments.
- **[External Providers Guide](EXTERNAL_PROVIDERS_GUIDE.md):** Configure Azure AD B2C, Google, and other providers.
- **[Migration & UI Guide](MIGRATION_AND_UI_GUIDE.md):** Database migration and UI customization.
- **[Testing Guide](TESTING_GUIDE.md):** How to run the test suite and benchmarks.
- **[Building Packages](BUILDING_PACKAGES.md):** Guide for creating NuGet packages for distribution.
- **[Contributing](CONTRIBUTING.md):** Guidelines for contributing to the project.

## 📦 Architecture

The solution is modularized for flexibility:

- **TrustIdentity.Core:** The heart of the OpenID Connect engine.
- **TrustIdentity.AspNetCore:** Integration with the ASP.NET Core pipeline.
- **TrustIdentity.Storage:** Data access layer (EF Core support) with multi-tenancy.
- **TrustIdentity.UI:** Pre-built Razor Pages for Login, Consent, and Logout.
- **TrustIdentity.Admin:** Administration API and UI with tenant management.
- **TrustIdentity.ExternalProviders:** Azure AD B2C, Google, Facebook, GitHub integration.
- **TrustIdentity.AI:** AI-powered fraud detection and behavioral analysis.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
