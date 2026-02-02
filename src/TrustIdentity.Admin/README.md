# TrustIdentity.Admin

**Administration UI for TrustIdentity**

---

## 📦 Overview

`TrustIdentity.Admin` provides a comprehensive web-based administration interface for managing TrustIdentity servers.

---

## ✨ Features

- ✅ **Client Management** - Create, edit, delete OAuth/OIDC clients
- ✅ **User Management** - Manage users and their claims
- ✅ **Resource Management** - Configure identity resources and API scopes
- ✅ **Tenant Management** - Multi-tenant administration
- ✅ **Session Monitoring** - View active sessions
- ✅ **Audit Logs** - Security and access logs
- ✅ **Dashboard** - Overview and statistics

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.Admin
dotnet add package TrustIdentity.AdminApi  # REST API backend
```

---

## 🔧 Usage

### Basic Setup

```csharp
using TrustIdentity.Admin.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTrustIdentity(options => { ... });
builder.Services.AddTrustIdentityAdmin();  // Add Admin UI
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseTrustIdentity();
app.MapRazorPages();

app.Run();
```

### With Admin API

```csharp
builder.Services.AddTrustIdentity(options => { ... });
builder.Services.AddTrustIdentityAdmin();
builder.Services.AddTrustIdentityAdminApi();  // Add REST API
builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseTrustIdentity();
app.MapControllers();
app.MapRazorPages();

app.Run();
```

---

## 🎨 Admin UI Pages

### Dashboard
- Overview statistics
- Recent activity
- System health

### Clients
- List all clients
- Create new client
- Edit client configuration
- Delete client
- View client secrets

### Users
- List all users
- Create new user
- Edit user details
- Manage user claims
- Reset password

### Resources
- Identity resources (openid, profile, email)
- API scopes
- API resources

### Tenants (Multi-Tenancy)
- List all tenants
- Create new tenant
- Edit tenant configuration
- Manage tenant subscriptions

### Sessions
- Active sessions
- Session details
- Revoke sessions

### Audit Logs
- Security events
- Access logs
- Filter and search

---

## 🔒 Security

### Securing Admin UI

```csharp
builder.Services.AddTrustIdentityAdmin(options =>
{
    options.RequireAuthentication = true;
    options.RequireRole = "Administrator";
    options.AllowedUsers = new[] { "admin@example.com" };
});
```

### With OAuth/OIDC Protection

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://identity.example.com";
    options.ClientId = "admin-ui";
    options.ClientSecret = "secret";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add("admin");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Administrator"));
});
```

---

## 🌐 Admin API Endpoints

### Clients

```http
GET    /api/clients              # List all clients
GET    /api/clients/{id}         # Get client by ID
POST   /api/clients              # Create new client
PUT    /api/clients/{id}         # Update client
DELETE /api/clients/{id}         # Delete client
```

### Users

```http
GET    /api/users                # List all users
GET    /api/users/{id}           # Get user by ID
POST   /api/users                # Create new user
PUT    /api/users/{id}           # Update user
DELETE /api/users/{id}           # Delete user
```

### Resources

```http
GET    /api/identity-resources   # List identity resources
GET    /api/api-scopes           # List API scopes
GET    /api/api-resources        # List API resources
POST   /api/identity-resources   # Create identity resource
POST   /api/api-scopes           # Create API scope
POST   /api/api-resources        # Create API resource
```

### Tenants

```http
GET    /api/tenants              # List all tenants
GET    /api/tenants/{id}         # Get tenant by ID
POST   /api/tenants              # Create new tenant
PUT    /api/tenants/{id}         # Update tenant
DELETE /api/tenants/{id}         # Delete tenant
```

---

## 🎨 Customization

### Custom Branding

```csharp
builder.Services.AddTrustIdentityAdmin(options =>
{
    options.ApplicationName = "My Identity Server";
    options.LogoUrl = "/images/logo.png";
    options.Theme = "dark";
});
```

### Custom Pages

Override default Razor Pages by creating pages in your project:

```
Pages/
├── Admin/
│   ├── Clients/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   └── Users/
│       ├── Index.cshtml
│       └── Create.cshtml
```

---

## 🏗️ Architecture

```
TrustIdentity.Admin/
├── Pages/             # Razor Pages
│   ├── Clients/
│   ├── Users/
│   ├── Resources/
│   ├── Tenants/
│   ├── Sessions/
│   └── Audit/
├── wwwroot/          # Static files (CSS, JS)
└── Extensions/       # Configuration extensions
```

---

## 📚 Documentation

- **[Setup Guide](../../../SETUP_GUIDE.md)** - General setup
- **[Main Documentation](../../../README.md)** - Overview

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
