# TrustIdentity.Storage

**Entity Framework Core storage for TrustIdentity**

---

## 📦 Overview

`TrustIdentity.Storage` provides Entity Framework Core-based persistence for TrustIdentity, supporting multiple database providers.

---

## 🎯 Supported Databases

- ✅ SQL Server
- ✅ PostgreSQL
- ✅ MySQL
- ✅ SQLite
- ✅ In-Memory (Development)

---

## 📋 Database Contexts

### ConfigurationDbContext
Stores configuration data (long-lived):
- **Clients** - OAuth/OIDC client applications
- **IdentityResources** - OpenID Connect scopes
- **ApiScopes** - OAuth 2.0 scopes
- **ApiResources** - Protected APIs

### PersistedGrantDbContext
Stores operational data (short-lived):
- **PersistedGrants** - Authorization codes, refresh tokens
- **DeviceFlowCodes** - Device flow codes
- **Keys** - Signing keys
- **ServerSideSessions** - Server-side sessions

---

## 🚀 Installation

```bash
# Base package
dotnet add package TrustIdentity.Storage

# Database provider (choose one)
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# EF Core tools
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## 🔧 Usage

### SQL Server

```csharp
using TrustIdentity.Storage.EntityFramework.Extensions;

var connectionString = "Server=localhost;Database=TrustIdentity;User Id=sa;Password=YourPassword;TrustServerCertificate=True";

builder.Services.AddTrustIdentity(options => { ... })
    .AddConfigurationStore(options =>
        options.UseSqlServer(connectionString, sql =>
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)))
    .AddOperationalStore(options =>
        options.UseSqlServer(connectionString, sql =>
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)));
```

### PostgreSQL

```csharp
var connectionString = "Host=localhost;Database=trustidentity;Username=postgres;Password=YourPassword";

builder.Services.AddTrustIdentity(options => { ... })
    .AddConfigurationStore(options =>
        options.UseNpgsql(connectionString, sql =>
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)))
    .AddOperationalStore(options =>
        options.UseNpgsql(connectionString, sql =>
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)));
```

### MySQL

```csharp
var connectionString = "Server=localhost;Database=trustidentity;User=root;Password=YourPassword";
var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

builder.Services.AddTrustIdentity(options => { ... })
    .AddConfigurationStore(options =>
        options.UseMySql(connectionString, serverVersion, sql =>
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)))
    .AddOperationalStore(options =>
        options.UseMySql(connectionString, serverVersion, sql =>
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)));
```

---

## 🔄 Migrations

### Create Migrations

```bash
# Configuration store
dotnet ef migrations add InitialConfigurationDb -c ConfigurationDbContext -o Data/Migrations/Configuration

# Operational store
dotnet ef migrations add InitialPersistedGrantDb -c PersistedGrantDbContext -o Data/Migrations/PersistedGrant
```

### Update Database

```bash
dotnet ef database update -c ConfigurationDbContext
dotnet ef database update -c PersistedGrantDbContext
```

---

## 📊 Database Schema

### Configuration Tables
- `Clients` - Client configurations
- `ClientScopes` - Client allowed scopes
- `ClientRedirectUris` - Redirect URIs
- `ClientSecrets` - Client secrets
- `IdentityResources` - OIDC identity resources
- `IdentityClaims` - Identity resource claims
- `ApiScopes` - OAuth 2.0 scopes
- `ApiScopeClaims` - API scope claims
- `ApiResources` - Protected APIs
- `ApiResourceScopes` - API resource scopes

### Operational Tables
- `PersistedGrants` - Tokens, codes, consents
- `DeviceFlowCodes` - Device flow codes
- `Keys` - Signing keys
- `ServerSideSessions` - Server-side sessions

---

## 🧹 Token Cleanup

Enable automatic cleanup of expired tokens:

```csharp
.AddOperationalStore(options =>
{
    options.UseSqlServer(connectionString);
    options.EnableTokenCleanup = true;
    options.TokenCleanupInterval = 3600; // 1 hour
})
```

---

## 🏗️ Architecture

```
TrustIdentity.Storage/
├── EntityFramework/
│   ├── DbContexts/     # EF Core contexts
│   ├── Entities/       # Database entities
│   ├── Stores/         # Store implementations
│   └── Extensions/     # Configuration extensions
└── InMemory/          # In-memory stores (dev)
```

---

## 📚 Documentation

- **[Database Setup Guide](../../../DATABASE_SETUP.md)** - Complete database setup
- **[Setup Guide](../../../SETUP_GUIDE.md)** - General setup

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
