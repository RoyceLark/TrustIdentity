# TrustIdentity.AdminApi

**REST API for TrustIdentity administration**

---

## 📦 Overview

`TrustIdentity.AdminApi` provides a RESTful API for managing TrustIdentity servers programmatically.

---

## ✨ Features

- ✅ **Client Management API** - CRUD operations for clients
- ✅ **User Management API** - User administration
- ✅ **Resource Management API** - Manage resources and scopes
- ✅ **Tenant Management API** - Multi-tenant administration
- ✅ **Session Management API** - View and revoke sessions
- ✅ **Audit API** - Access audit logs
- ✅ **OpenAPI/Swagger** - API documentation

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.AdminApi
```

---

## 🔧 Usage

```csharp
using TrustIdentity.AdminApi.Extensions;

builder.Services.AddTrustIdentity(options => { ... });
builder.Services.AddTrustIdentityAdminApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
```

---

## 📋 API Endpoints

### Clients
- `GET /api/clients` - List all clients
- `GET /api/clients/{id}` - Get client
- `POST /api/clients` - Create client
- `PUT /api/clients/{id}` - Update client
- `DELETE /api/clients/{id}` - Delete client

### Users
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
