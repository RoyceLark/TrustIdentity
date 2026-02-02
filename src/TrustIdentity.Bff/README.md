# TrustIdentity.Bff

**Backend-for-Frontend (BFF) pattern for secure SPA authentication**

---

## 📦 Overview

`TrustIdentity.Bff` implements the Backend-for-Frontend security pattern for Single Page Applications (SPAs), keeping tokens server-side and using secure cookies for authentication.

---

## ✨ Features

- ✅ **Token Management** - Tokens stored server-side
- ✅ **Secure Cookies** - HttpOnly, Secure cookies
- ✅ **Automatic Token Refresh** - Transparent refresh token handling
- ✅ **Anti-Forgery Protection** - CSRF protection
- ✅ **Logout Coordination** - Coordinated logout across apps
- ✅ **API Proxy** - Secure API calls with automatic token injection

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.Bff
```

---

## 🔧 Usage

### Basic Setup

```csharp
using TrustIdentity.Bff.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTrustIdentity(options => { ... })
    .AddBff();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://identity.example.com";
    options.ClientId = "spa-bff";
    options.ClientSecret = "secret";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("api1");
    options.Scope.Add("offline_access");
});

var app = builder.Build();

app.UseBff();
app.MapBffManagementEndpoints();

app.Run();
```

---

## 📋 BFF Endpoints

### Management Endpoints

```
GET  /bff/login                  # Initiate login
GET  /bff/logout                 # Logout
GET  /bff/user                   # Get user info
GET  /bff/session                # Get session info
```

### API Proxy

```csharp
app.MapBffApiEndpoint("/api/data", "https://api.example.com/data")
    .RequireAuthorization();
```

---

## 🎯 Use Cases

### React SPA with BFF

**Backend (ASP.NET Core):**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBff();
builder.Services.AddAuthentication(/* ... */);

var app = builder.Build();

app.UseBff();
app.UseAuthentication();
app.UseAuthorization();

// BFF management endpoints
app.MapBffManagementEndpoints();

// API proxy endpoints
app.MapBffApiEndpoint("/api/products", "https://api.example.com/products")
    .RequireAuthorization();

app.MapBffApiEndpoint("/api/orders", "https://api.example.com/orders")
    .RequireAuthorization();

// Serve SPA
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
```

**Frontend (React):**

```javascript
// Check authentication status
const response = await fetch('/bff/user');
if (response.ok) {
  const user = await response.json();
  console.log('Logged in as:', user.name);
} else {
  // Redirect to login
  window.location.href = '/bff/login';
}

// Call API through BFF proxy
const products = await fetch('/api/products');
const data = await products.json();
```

### Vue.js SPA with BFF

```javascript
// Vue.js composable
export function useAuth() {
  const user = ref(null);
  const isAuthenticated = ref(false);

  async function checkAuth() {
    const response = await fetch('/bff/user');
    if (response.ok) {
      user.value = await response.json();
      isAuthenticated.value = true;
    }
  }

  async function login() {
    window.location.href = '/bff/login';
  }

  async function logout() {
    await fetch('/bff/logout', { method: 'POST' });
    window.location.href = '/';
  }

  return { user, isAuthenticated, checkAuth, login, logout };
}
```

---

## 🔒 Security Benefits

### 1. Tokens Never in Browser

Tokens are stored server-side in session storage, never exposed to JavaScript.

### 2. HttpOnly Cookies

Authentication cookies are HttpOnly, preventing XSS attacks.

### 3. CSRF Protection

Built-in anti-forgery token validation.

### 4. Automatic Token Refresh

Refresh tokens are handled server-side, transparently to the SPA.

---

## 🔧 Configuration

### BFF Options

```csharp
builder.Services.AddBff(options =>
{
    // Session management
    options.SessionManagement.Enabled = true;
    options.SessionManagement.CheckSessionInterval = 2000;
    
    // Anti-forgery
    options.AntiForgery.Enabled = true;
    options.AntiForgery.HeaderName = "X-CSRF-TOKEN";
    
    // API proxy
    options.ApiProxy.Enabled = true;
    options.ApiProxy.RequireAntiForgeryCheck = true;
    
    // Logout
    options.Logout.RevokeRefreshToken = true;
    options.Logout.BackchannelLogout = true;
});
```

### Cookie Configuration

```csharp
.AddCookie("Cookies", options =>
{
    options.Cookie.Name = "MyApp.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
```

---

## 📊 Architecture

```
Browser (SPA)
    ↓ (Secure Cookie)
BFF Backend (ASP.NET Core)
    ↓ (Access Token)
API (Protected Resource)
```

### Flow

1. User clicks "Login" in SPA
2. SPA redirects to `/bff/login`
3. BFF initiates OAuth/OIDC flow
4. User authenticates at Identity Server
5. BFF receives tokens, stores server-side
6. BFF sets secure HttpOnly cookie
7. SPA makes API calls through BFF proxy
8. BFF injects access token automatically
9. API validates token and returns data

---

## 🏗️ Project Structure

```
TrustIdentity.Bff/
├── Middleware/        # BFF middleware
├── Endpoints/        # Management endpoints
├── Services/         # Token management
└── Extensions/       # Configuration extensions
```

---

## 📚 Documentation

- **[Setup Guide](../../../SETUP_GUIDE.md)** - General setup
- **[Main Documentation](../../../README.md)** - Overview

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
