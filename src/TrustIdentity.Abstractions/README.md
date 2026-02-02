# TrustIdentity.Abstractions

**Core interfaces and models for TrustIdentity**

---

## 📦 Overview

`TrustIdentity.Abstractions` contains all interfaces, models, and contracts used throughout TrustIdentity. This package defines the core abstractions without implementation details.

---

## 📋 Key Components

### Interfaces

#### Services
- `ITokenService` - Token creation and validation
- `IAuthorizationCodeService` - Authorization code management
- `IRefreshTokenService` - Refresh token handling
- `IDeviceFlowService` - Device authorization flow
- `IUserService` - User management
- `IClientStore` - Client storage
- `IResourceStore` - Resource storage
- `IProfileService` - User profile data
- `IEventService` / `IEventSink` - Event handling

#### Stores
- `IClientStore` - Client persistence
- `IResourceStore` - Resource persistence
- `IPersistedGrantStore` - Token/code persistence
- `IUserStore` - User persistence

### Models

#### Configuration
- `Client` - OAuth/OIDC client configuration
- `IdentityResource` - OpenID Connect scopes
- `ApiScope` - OAuth 2.0 scopes
- `ApiResource` - Protected API resources
- `Secret` - Client/API secrets

#### Runtime
- `Token` - Token descriptor
- `AuthorizationCode` - Authorization code
- `RefreshToken` - Refresh token
- `DeviceCode` - Device flow code
- `PersistedGrant` - Generic grant storage

---

## 🔧 Usage

This package is typically referenced by:
- `TrustIdentity.Core` - Core implementations
- `TrustIdentity.Storage` - Storage implementations
- Custom extensions and plugins

### Implementing Custom Services

```csharp
using TrustIdentity.Abstractions.Services;

public class CustomProfileService : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        // Your implementation
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        // Your implementation
    }
}
```

### Implementing Custom Stores

```csharp
using TrustIdentity.Abstractions.Stores;

public class CustomClientStore : IClientStore
{
    public async Task<Client?> FindClientByIdAsync(string clientId)
    {
        // Your implementation
    }
}
```

---

## 🏗️ Architecture

```
TrustIdentity.Abstractions/
├── Services/          # Service interfaces
├── Stores/           # Store interfaces
├── Models/           # Domain models
├── Configuration/    # Configuration models
└── Validation/       # Validation interfaces
```

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
