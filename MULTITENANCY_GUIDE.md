# Multi-Tenancy Guide

## Overview

TrustIdentity now supports multi-tenancy, allowing you to serve multiple isolated tenants from a single deployment. Each tenant can have its own clients, users, and configuration while sharing the same infrastructure.

## Features

- **Flexible Tenant Resolution**: Resolve tenants from host, header, route, query string, or claims
- **Data Isolation**: Automatic tenant-scoped queries for all entities
- **Tenant Management**: Full CRUD operations for tenant administration
- **Scalable Architecture**: Support for shared database or database-per-tenant models

## Quick Start

### 1. Enable Multi-Tenancy

In your `Program.cs`:

```csharp
using TrustIdentity.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add TrustIdentity with multi-tenancy
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://identity.yourdomain.com";
})
.AddConfigurationStore(opt => opt.UseSqlServer(connString))
.AddOperationalStore(opt => opt.UseSqlServer(connString));

// Enable multi-tenancy
builder.Services.AddMultiTenancy();

var app = builder.Build();

// Add tenant resolution middleware (BEFORE UseTrustIdentity)
app.UseMultiTenancy();
app.UseTrustIdentity();

app.Run();
```

### 2. Create a Tenant

```csharp
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

// Inject ITenantStore
public class TenantController : Controller
{
    private readonly ITenantStore _tenantStore;
    
    public TenantController(ITenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }
    
    public async Task<IActionResult> CreateTenant()
    {
        var tenant = new Tenant
        {
            Name = "Acme Corporation",
            Identifier = "acme",
            Host = "acme.myapp.com",
            IsActive = true,
            IssuerUri = "https://acme.myapp.com",
            MaxUsers = 1000,
            MaxClients = 50,
            SubscriptionTier = "Enterprise"
        };
        
        await _tenantStore.CreateAsync(tenant);
        return Ok(tenant);
    }
}
```

## Tenant Resolution Strategies

### Host-Based Resolution

Resolve tenant from the domain/subdomain:

```
https://tenant1.myapp.com → Tenant: tenant1
https://tenant2.myapp.com → Tenant: tenant2
```

Configure DNS to point subdomains to your application.

### Header-Based Resolution

Resolve tenant from HTTP header:

```http
GET /api/users HTTP/1.1
Host: myapp.com
X-Tenant-Id: tenant1
```

Useful for API clients and mobile apps.

### Route-Based Resolution

Resolve tenant from URL path:

```
https://myapp.com/tenants/tenant1/users
https://myapp.com/tenants/tenant2/clients
```

Configure routes in your application.

### Query String Resolution

Resolve tenant from query parameter:

```
https://myapp.com/login?tenant=tenant1
```

Useful for shared login pages.

### Claim-Based Resolution

Resolve tenant from user claims after authentication:

```csharp
// Tenant ID stored in user's claims
var tenantClaim = User.FindFirst("tenant_id");
```

## Custom Resolution Strategy

Configure which strategies to use and their order:

```csharp
using TrustIdentity.Abstractions.Services;

builder.Services.AddMultiTenancy(new List<TenantResolutionStrategy>
{
    TenantResolutionStrategy.Host,      // Try host first
    TenantResolutionStrategy.Header,    // Then header
    TenantResolutionStrategy.Route      // Finally route
});
```

## Accessing Current Tenant

Inject `ITenantContext` to access the current tenant:

```csharp
using TrustIdentity.Abstractions.Services;

public class MyService
{
    private readonly ITenantContext _tenantContext;
    
    public MyService(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }
    
    public void DoSomething()
    {
        if (_tenantContext.HasTenant)
        {
            var tenantId = _tenantContext.TenantId;
            var tenant = _tenantContext.CurrentTenant;
            
            Console.WriteLine($"Current tenant: {tenant.Name}");
        }
    }
}
```

## Data Isolation

All entities (Clients, Users, etc.) automatically include a `TenantId` property. When querying data, filter by the current tenant:

```csharp
// Manually filter by tenant
var clients = await _context.Clients
    .Where(c => c.TenantId == _tenantContext.TenantId)
    .ToListAsync();
```

## Tenant-Specific Configuration

Each tenant can have custom configuration:

```csharp
var tenant = new Tenant
{
    Name = "Acme Corp",
    Identifier = "acme",
    IssuerUri = "https://acme.identity.com",  // Custom issuer
    Configuration = JsonSerializer.Serialize(new
    {
        AllowSelfRegistration = true,
        RequireEmailVerification = true,
        SessionTimeout = 3600,
        CustomBranding = new
        {
            LogoUrl = "https://acme.com/logo.png",
            PrimaryColor = "#FF6B35"
        }
    })
};
```

## Database Migration

Run migrations to add the Tenants table:

```bash
dotnet ef migrations add AddMultiTenancy -c ConfigurationDbContext -p src/TrustIdentity.Storage -s src/TrustIdentity.Admin
dotnet ef database update -c ConfigurationDbContext -s src/TrustIdentity.Admin
```

## Best Practices

1. **Always Validate Tenant Access**: Ensure users can only access their tenant's data
2. **Use Scoped Services**: Tenant context is scoped per-request
3. **Tenant Isolation**: Never expose one tenant's data to another
4. **Subscription Management**: Track tenant limits (users, clients, etc.)
5. **Audit Logging**: Log all tenant-related operations

## Advanced Scenarios

### Database-Per-Tenant

For complete isolation, use a separate database for each tenant:

```csharp
var tenant = new Tenant
{
    Name = "Acme Corp",
    Identifier = "acme",
    ConnectionString = "Server=...;Database=TrustIdentity_Acme;..."
};
```

Then configure your DbContext to use the tenant's connection string.

### Tenant Switching (Super Admin)

Allow super admins to switch between tenants:

```csharp
public class TenantSwitchService
{
    private readonly ITenantContext _tenantContext;
    private readonly ITenantStore _tenantStore;
    
    public async Task SwitchTenant(string tenantId)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId);
        if (tenant != null)
        {
            _tenantContext.SetTenant(tenant);
        }
    }
}
```

## Troubleshooting

### Tenant Not Resolved

- Check that middleware is registered before `UseTrustIdentity()`
- Verify tenant exists in database
- Check resolution strategy configuration
- Review logs for resolution attempts

### Data Leakage

- Always filter queries by `TenantId`
- Use global query filters in EF Core
- Implement tenant validation in authorization policies

## API Reference

### ITenantStore

- `GetByIdAsync(string tenantId)` - Get tenant by ID
- `GetByIdentifierAsync(string identifier)` - Get tenant by identifier
- `GetByHostAsync(string host)` - Get tenant by host
- `GetAllAsync(int skip, int take)` - Get all tenants (paginated)
- `CreateAsync(Tenant tenant)` - Create new tenant
- `UpdateAsync(Tenant tenant)` - Update existing tenant
- `DeleteAsync(string tenantId)` - Delete tenant
- `GetCountAsync()` - Get total tenant count

### ITenantContext

- `CurrentTenant` - Get current tenant
- `TenantId` - Get current tenant ID
- `HasTenant` - Check if tenant is set
- `SetTenant(Tenant tenant)` - Set current tenant

### ITenantResolver

- `ResolveAsync(HttpContext httpContext)` - Resolve tenant from HTTP context

## Next Steps

- Configure tenant resolution strategy for your deployment model
- Set up tenant administration UI
- Implement tenant-specific branding
- Configure subscription and billing integration
