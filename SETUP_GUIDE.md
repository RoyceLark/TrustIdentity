# TrustIdentity Setup Guide

**Complete guide to set up and configure TrustIdentity in your application**

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Installation](#installation)
3. [Basic Setup](#basic-setup)
4. [Database Configuration](#database-configuration)
5. [Production Setup](#production-setup)
6. [Configuration Options](#configuration-options)
7. [Testing](#testing)

---

## 🎯 Prerequisites

- **.NET 10.0 SDK** or later
- **Database** (SQL Server, PostgreSQL, MySQL, or SQLite)
- **X.509 Certificate** (for production)
- **HTTPS** enabled

---

## 📦 Installation

### Step 1: Create New Project

```bash
dotnet new web -n MyIdentityServer
cd MyIdentityServer
```

### Step 2: Install TrustIdentity

```bash
# Main package
dotnet add package TrustIdentity.Server

# Database support (choose one)
dotnet add package TrustIdentity.Storage
dotnet add package Microsoft.EntityFrameworkCore.SqlServer  # For SQL Server
# OR
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL    # For PostgreSQL
# OR
dotnet add package Pomelo.EntityFrameworkCore.MySql         # For MySQL

# Entity Framework tools
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## 🚀 Basic Setup

### Step 1: Configure Services (Program.cs)

```csharp
using TrustIdentity.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add TrustIdentity
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
})
.AddInMemoryClients(Config.GetClients())
.AddInMemoryIdentityResources(Config.GetIdentityResources())
.AddInMemoryApiScopes(Config.GetApiScopes())
.AddDeveloperSigningCredential();  // For development only!

var app = builder.Build();

// Use TrustIdentity
app.UseTrustIdentity();

app.Run();
```

### Step 2: Create Configuration (Config.cs)

```csharp
using TrustIdentity.Abstractions.Models;

public static class Config
{
    public static IEnumerable<Client> GetClients()
    {
        return new List<Client>
        {
            // Web application
            new Client
            {
                ClientId = "web-app",
                ClientName = "Web Application",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret("secret".Sha256()) },
                RedirectUris = { "https://localhost:5002/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                AllowedScopes = { "openid", "profile", "email", "api1" },
                RequirePkce = true,
                AllowOfflineAccess = true
            },
            
            // API client
            new Client
            {
                ClientId = "api-client",
                ClientName = "API Client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedScopes = { "api1" }
            }
        };
    }

    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope("api1", "My API")
        };
    }

    public static IEnumerable<ApiResource> GetApiResources()
    {
        return new List<ApiResource>
        {
            new ApiResource("api1", "My API")
            {
                Scopes = { "api1" }
            }
        };
    }
}
```

### Step 3: Run

```bash
dotnet run
```

Visit `https://localhost:5001/.well-known/openid-configuration` to verify.

---

## 🗄️ Database Configuration

### Step 1: Add Database Packages

```bash
dotnet add package TrustIdentity.Storage
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Step 2: Configure Connection String

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TrustIdentity;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
  }
}
```

### Step 3: Update Program.cs

```csharp
using TrustIdentity.Storage.EntityFramework.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
})
// Replace in-memory stores with database
.AddConfigurationStore(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.MigrationsAssembly(typeof(Program).Assembly.FullName)))
.AddOperationalStore(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.MigrationsAssembly(typeof(Program).Assembly.FullName)))
.AddDeveloperSigningCredential();
```

### Step 4: Create Migrations

```bash
# Configuration store
dotnet ef migrations add InitialConfigurationDb -c ConfigurationDbContext -o Data/Migrations/Configuration

# Operational store
dotnet ef migrations add InitialPersistedGrantDb -c PersistedGrantDbContext -o Data/Migrations/PersistedGrant
```

### Step 5: Update Database

```bash
dotnet ef database update -c ConfigurationDbContext
dotnet ef database update -c PersistedGrantDbContext
```

### Step 6: Seed Data

```csharp
public static class DatabaseInitializer
{
    public static void InitializeDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var context = serviceScope.ServiceProvider
            .GetRequiredService<ConfigurationDbContext>();
        
        context.Database.Migrate();

        if (!context.Clients.Any())
        {
            foreach (var client in Config.GetClients())
            {
                context.Clients.Add(client.ToEntity());
            }
            context.SaveChanges();
        }

        if (!context.IdentityResources.Any())
        {
            foreach (var resource in Config.GetIdentityResources())
            {
                context.IdentityResources.Add(resource.ToEntity());
            }
            context.SaveChanges();
        }

        if (!context.ApiScopes.Any())
        {
            foreach (var scope in Config.GetApiScopes())
            {
                context.ApiScopes.Add(scope.ToEntity());
            }
            context.SaveChanges();
        }
    }
}

// In Program.cs
var app = builder.Build();
DatabaseInitializer.InitializeDatabase(app);
app.UseTrustIdentity();
app.Run();
```

**See [DATABASE_SETUP.md](DATABASE_SETUP.md) for detailed database instructions**

---

## 🔒 Production Setup

### Step 1: Use Real Certificate

```csharp
using System.Security.Cryptography.X509Certificates;

var certificate = new X509Certificate2("path/to/certificate.pfx", "password");

builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://identity.example.com";
    options.RequireHttps = true;  // Enforce HTTPS
})
.AddConfigurationStore(options => options.UseSqlServer(connectionString))
.AddOperationalStore(options => options.UseSqlServer(connectionString))
.AddSigningCredential(certificate);  // Use real certificate
```

### Step 2: Secure Connection Strings

**Use User Secrets (Development):**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
```

**Use Environment Variables (Production):**
```bash
export ConnectionStrings__DefaultConnection="Server=..."
```

### Step 3: Enable Token Cleanup

```csharp
.AddOperationalStore(options =>
{
    options.UseSqlServer(connectionString);
    options.EnableTokenCleanup = true;
    options.TokenCleanupInterval = 3600;  // 1 hour
})
```

### Step 4: Configure HTTPS

**appsettings.Production.json:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/path/to/certificate.pfx",
          "Password": "certificate-password"
        }
      }
    }
  }
}
```

---

## ⚙️ Configuration Options

### TrustIdentityOptions

```csharp
builder.Services.AddTrustIdentity(options =>
{
    // Required
    options.IssuerUri = "https://identity.example.com";
    
    // Security
    options.RequireHttps = true;
    options.EnablePkce = true;
    options.RequirePkce = true;
    
    // Features
    options.EnableAI = true;
    options.EnableFraudDetection = true;
    options.EnableMultiTenancy = false;
    
    // Endpoints
    options.EnableDiscoveryEndpoint = true;
    options.EnableTokenEndpoint = true;
    options.EnableUserInfoEndpoint = true;
    options.EnableIntrospectionEndpoint = true;
    options.EnableRevocationEndpoint = true;
    options.EnableEndSessionEndpoint = true;
    options.EnableDeviceAuthorizationEndpoint = true;
    
    // Token lifetimes (seconds)
    options.AccessTokenLifetime = 3600;        // 1 hour
    options.IdentityTokenLifetime = 300;       // 5 minutes
    options.AuthorizationCodeLifetime = 300;   // 5 minutes
    options.RefreshTokenLifetime = 2592000;    // 30 days
    
    // CORS
    options.CorsOrigins = new[] 
    { 
        "https://app.example.com",
        "https://admin.example.com"
    };
});
```

### Client Configuration

```csharp
new Client
{
    ClientId = "web-app",
    ClientName = "Web Application",
    
    // Grant types
    AllowedGrantTypes = GrantTypes.Code,
    
    // Secrets
    ClientSecrets = { new Secret("secret".Sha256()) },
    
    // URIs
    RedirectUris = { "https://localhost:5002/signin-oidc" },
    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
    AllowedCorsOrigins = { "https://localhost:5002" },
    
    // Scopes
    AllowedScopes = { "openid", "profile", "email", "api1" },
    
    // Security
    RequirePkce = true,
    RequireClientSecret = true,
    AllowPlainTextPkce = false,
    
    // Tokens
    AllowOfflineAccess = true,
    AccessTokenLifetime = 3600,
    IdentityTokenLifetime = 300,
    RefreshTokenLifetime = 2592000,
    
    // Consent
    RequireConsent = false,
    AllowRememberConsent = true,
    
    // Additional settings
    Enabled = true,
    AlwaysIncludeUserClaimsInIdToken = false,
    UpdateAccessTokenClaimsOnRefresh = true
}
```

---

## 🎨 Adding UI

### Step 1: Install UI Package

```bash
dotnet add package TrustIdentity.UI
```

### Step 2: Configure UI

```csharp
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseTrustIdentity();
app.MapRazorPages();

app.Run();
```

The UI includes:
- Login page
- Consent page
- Logout page
- Error page

---

## 🔌 External Providers

### Step 1: Install Package

```bash
dotnet add package TrustIdentity.ExternalProviders
```

### Step 2: Configure Provider

```csharp
using TrustIdentity.ExternalProviders.Extensions;

builder.Services.AddTrustIdentity(options => { ... })
    .AddExternalProvider("Google", options =>
    {
        options.ClientId = "your-google-client-id";
        options.ClientSecret = "your-google-client-secret";
    })
    .AddExternalProvider("AzureAD", options =>
    {
        options.ClientId = "your-azure-client-id";
        options.ClientSecret = "your-azure-client-secret";
        options.TenantId = "your-tenant-id";
    });
```

**See [EXTERNAL_PROVIDERS_GUIDE.md](EXTERNAL_PROVIDERS_GUIDE.md) for detailed instructions**

---

## 🤖 AI Fraud Detection

### Enable AI Features

```csharp
builder.Services.AddTrustIdentity(options =>
{
    options.EnableAI = true;
    options.EnableFraudDetection = true;
})
.AddAIFraudDetection()
.AddBehaviorAnalysis()
.AddRiskScoring();
```

Features:
- Real-time fraud detection
- Behavioral analysis
- Risk scoring
- Adaptive authentication
- Device fingerprinting

---

## 🧪 Testing

### Test Discovery Endpoint

```bash
curl https://localhost:5001/.well-known/openid-configuration
```

### Test Token Endpoint

```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=api-client" \
  -d "client_secret=secret" \
  -d "scope=api1"
```

### Test with Postman

1. Import OpenID Connect discovery document
2. Configure OAuth 2.0 authorization
3. Get access token
4. Call protected API

---

## 🐳 Docker Deployment

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MyIdentityServer.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyIdentityServer.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'
services:
  identityserver:
    build: .
    ports:
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:443
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TrustIdentity;User Id=sa;Password=YourPassword
    depends_on:
      - sqlserver
  
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password
    ports:
      - "1433:1433"
```

```bash
docker-compose up -d
```

---

## 🎯 Common Scenarios

### Scenario 1: Web Application with API

```csharp
// Identity Server
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://identity.example.com";
})
.AddConfigurationStore(options => options.UseSqlServer(connectionString))
.AddOperationalStore(options => options.UseSqlServer(connectionString))
.AddSigningCredential(certificate);

// Web App (Client)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://identity.example.com";
    options.ClientId = "web-app";
    options.ClientSecret = "secret";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("api1");
});

// API (Resource Server)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://identity.example.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });
```

### Scenario 2: Mobile App

```csharp
new Client
{
    ClientId = "mobile-app",
    ClientName = "Mobile Application",
    AllowedGrantTypes = GrantTypes.Code,
    RequirePkce = true,
    RequireClientSecret = false,  // Public client
    RedirectUris = { "myapp://callback" },
    AllowedScopes = { "openid", "profile", "api1" },
    AllowOfflineAccess = true,
    RefreshTokenUsage = TokenUsage.OneTimeOnly,
    RefreshTokenExpiration = TokenExpiration.Sliding
}
```

### Scenario 3: SPA with BFF

```csharp
builder.Services.AddTrustIdentity(options => { ... })
    .AddBff();

// In your SPA backend
app.UseBff();
app.MapBffManagementEndpoints();
```

---

## 📊 Monitoring & Logging

### Enable Logging

```csharp
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);
```

### Custom Event Sink

```csharp
public class DatabaseEventSink : IEventSink
{
    public async Task PersistAsync(Event evt)
    {
        // Log to database
        await _db.Events.AddAsync(new EventRecord
        {
            Name = evt.Name,
            Message = evt.Message,
            TimeStamp = evt.TimeStamp,
            Category = evt.Category.ToString()
        });
        await _db.SaveChangesAsync();
    }
}

builder.Services.AddTrustIdentity(options => { ... })
    .AddEventSink<DatabaseEventSink>();
```

---

## ✅ Setup Checklist

- [ ] Install TrustIdentity packages
- [ ] Configure services in Program.cs
- [ ] Create configuration (clients, resources, scopes)
- [ ] Set up database (if using EF Core)
- [ ] Create and apply migrations
- [ ] Seed initial data
- [ ] Configure signing certificate (production)
- [ ] Enable HTTPS
- [ ] Test discovery endpoint
- [ ] Test token issuance
- [ ] Configure logging
- [ ] Set up monitoring

---

## 🎓 Next Steps

1. **[DATABASE_SETUP.md](DATABASE_SETUP.md)** - Configure database
2. **[MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)** - Migrate from Duende
3. **[EXTERNAL_PROVIDERS_GUIDE.md](EXTERNAL_PROVIDERS_GUIDE.md)** - Add external providers

---

## 📞 Need Help?

- 📧 **Email**: support@trustidentity.dev
- 💬 **Discussions**: [GitHub Discussions](https://github.com/trustidentity/trustidentity/discussions)
- 🐛 **Issues**: [GitHub Issues](https://github.com/trustidentity/trustidentity/issues)

---

**You're ready to build secure applications with TrustIdentity!**
