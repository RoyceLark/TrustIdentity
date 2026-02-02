# TrustIdentity.Core

**Core OAuth 2.0 / OpenID Connect engine**

---

## 📦 Overview

`TrustIdentity.Core` contains the core business logic for OAuth 2.0 and OpenID Connect protocols. This package implements all grant types, token generation, validation, and security features.

---

## 🎯 Purpose

This is the **heart of TrustIdentity**. It contains:

- OAuth 2.0 / OIDC protocol implementations
- Token generation and validation
- Grant type handlers
- Security features (PKCE, DPoP, mTLS, PAR, JAR)
- Service implementations

---

## 📋 Key Components

### Services

- **`TokenService`** - Token creation and validation
- **`AuthorizationCodeService`** - Authorization code management
- **`RefreshTokenService`** - Refresh token handling
- **`DeviceFlowService`** - Device authorization flow
- **`CibaService`** - Client-Initiated Backchannel Authentication
- **`TokenExchangeService`** - Token exchange (RFC 8693)
- **`DPoPService`** - Demonstrating Proof-of-Possession
- **`MutualTlsService`** - Mutual TLS support
- **`PushedAuthorizationService`** - PAR (RFC 9126)
- **`JwtSecuredAuthorizationService`** - JAR (RFC 9101)
- **`KeyManagementService`** - Automatic key rotation

### Validators

- **`ClientValidator`** - Client authentication
- **`ScopeValidator`** - Scope validation
- **`PkceValidator`** - PKCE validation
- **`AuthorizeRequestValidator`** - Authorization request validation
- **`TokenRequestValidator`** - Token request validation

### Models

- **`Client`** - OAuth/OIDC client configuration
- **`IdentityResource`** - OpenID Connect scopes
- **`ApiScope`** - OAuth 2.0 scopes
- **`ApiResource`** - Protected API resources

---

## 🔧 Usage

This package is typically not used directly. Use `TrustIdentity.Server` instead.

### Direct Usage (Advanced)

```csharp
using TrustIdentity.Core.Services;

// Inject services
services.AddScoped<ITokenService, TokenService>();
services.AddScoped<IAuthorizationCodeService, AuthorizationCodeService>();
services.AddScoped<IRefreshTokenService, RefreshTokenService>();
```

---

## 📚 Implemented RFCs

- ✅ RFC 6749 - OAuth 2.0 Authorization Framework
- ✅ RFC 6750 - OAuth 2.0 Bearer Token Usage
- ✅ RFC 7009 - OAuth 2.0 Token Revocation
- ✅ RFC 7519 - JSON Web Token (JWT)
- ✅ RFC 7591 - OAuth 2.0 Dynamic Client Registration
- ✅ RFC 7636 - PKCE
- ✅ RFC 7662 - OAuth 2.0 Token Introspection
- ✅ RFC 8628 - OAuth 2.0 Device Authorization Grant
- ✅ RFC 8693 - OAuth 2.0 Token Exchange
- ✅ RFC 8705 - OAuth 2.0 Mutual-TLS
- ✅ RFC 8707 - Resource Indicators
- ✅ RFC 9101 - JWT Secured Authorization Request (JAR)
- ✅ RFC 9126 - Pushed Authorization Requests (PAR)
- ✅ RFC 9396 - OAuth 2.0 CIBA
- ✅ RFC 9449 - DPoP

---

## 🏗️ Architecture

```
TrustIdentity.Core/
├── Services/           # Business logic
├── Validation/         # Request validators
├── Models/            # Domain models
├── Security/          # Security utilities
└── Extensions/        # Helper extensions
```

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
