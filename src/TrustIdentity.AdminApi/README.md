# TrustIdentity.AdminApi

**REST API for TrustIdentity administration**

---

## 📦 Overview

`TrustIdentity.AdminApi` provides a RESTful API for managing TrustIdentity servers programmatically. It serves as the backend for the `TrustIdentity.Cli` and can be integrated into your own administrative workflows.

---

## ✨ Features

- ✅ **Client Management API** - Full CRUD operations for OAuth2/OIDC clients
- ✅ **User Management API** - Complete user administration, including lock/unlock and password resets
- ✅ **Resource Management API** - Manage Identity and API resources/scopes
- ✅ **Session Management API** - View and revoke active user sessions
- ✅ **Audit & Stats API** - Access audit logs and server performance statistics
- ✅ **OpenAPI/Swagger** - Interactive API documentation out of the box

---

## 🚀 Installation

Add the package to your web project:

```bash
dotnet add package TrustIdentity.AdminApi
```

---

## 🔧 Usage

Register the Admin API services in your `Program.cs`:

```csharp
using TrustIdentity.AdminApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add TrustIdentity Core & Storage
builder.Services.AddTrustIdentity(...)
    .AddConfigurationStore(...)
    .AddOperationalStore(...);

// Add Admin API Controllers and Authorization Policies
builder.Services.AddTrustIdentityAdminApi();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

---

## 📋 API Endpoints

All endpoints are prefixed with `/api/v1/admin`.

### Clients
- `GET /api/v1/admin/clients` - List all clients
- `GET /api/v1/admin/clients/{id}` - Get client details
- `POST /api/v1/admin/clients` - Create a new client
- `PUT /api/v1/admin/clients/{id}` - Update a client
- `DELETE /api/v1/admin/clients/{id}` - Delete a client

### Users
- `GET /api/v1/admin/users` - List users (with search and pagination)
- `GET /api/v1/admin/users/{id}` - Get user profile
- `POST /api/v1/admin/users` - Create a user
- `PUT /api/v1/admin/users/{id}` - Update user details
- `DELETE /api/v1/admin/users/{id}` - Delete a user
- `POST /api/v1/admin/users/{id}/reset-password` - Reset user password
- `POST /api/v1/admin/users/{id}/lock` - Lock user account
- `POST /api/v1/admin/users/{id}/unlock` - Unlock user account

### Resources & Scopes
- `GET /api/v1/admin/resources/identity` - List identity resources
- `GET /api/v1/admin/resources/api-resources` - List API resources
- `GET /api/v1/admin/resources/api-scopes` - List API scopes

### System
- `GET /api/v1/admin/info` - Connectivity check and version info
- `GET /api/v1/admin/stats` - Server statistics and trends
- `GET /api/v1/admin/audit` - Access audit logs

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
