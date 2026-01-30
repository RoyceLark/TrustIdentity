# Getting Started with TrustIdentity

This guide will help you set up TrustIdentity in your application.

## Installation

### Using .NET CLI

```bash
dotnet add package TrustIdentity.Server
```

### Using Package Manager Console

```powershell
Install-Package TrustIdentity.Server
```

## Quick Start

### 1. Create a new ASP.NET Core project

```bash
dotnet new web -n MyIdentityServer
cd MyIdentityServer
```

### 2. Add TrustIdentity

```bash
dotnet add package TrustIdentity.Server
```

### 3. Configure Program.cs

```csharp
using TrustIdentity.AspNetCore.Extensions;
using TrustIdentity.Core.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTrustIdentity()
    .AddInMemoryClients(new[]
    {
        new Client
        {
            ClientId = "client",
            ClientSecrets = { new Secret { Value = "secret" } },
            AllowedGrantTypes = new List<string> { "client_credentials" },
            AllowedScopes = { "api1" }
        }
    })
    .AddInMemoryIdentityResources(new[]
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
    })
    .AddInMemoryApiScopes(new[]
    {
        new ApiScope("api1", "My API")
    });

var app = builder.Build();
app.UseTrustIdentity();
app.Run();
```

### 4. Run the server

```bash
dotnet run
```

### 5. Test the server

Access the discovery document:
```
https://localhost:5001/.well-known/openid-configuration
```

## Next Steps

- [Configuration Guide](configuration.md)
- [Client Setup](clients.md)
- [Resources](resources.md)
- [AI/ML Features](ai-ml.md)
- [Deployment](deployment.md)