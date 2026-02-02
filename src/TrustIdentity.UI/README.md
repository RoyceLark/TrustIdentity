# TrustIdentity.UI

**Pre-built UI for login, consent, and logout**

---

## 📦 Overview

`TrustIdentity.UI` provides pre-built Razor Pages for login, consent, logout, and error pages with a modern, responsive design.

---

## ✨ Features

- ✅ **Login Page** - Username/password authentication
- ✅ **Consent Page** - User consent for scopes
- ✅ **Logout Page** - Logout confirmation
- ✅ **Error Pages** - User-friendly error messages
- ✅ **External Providers** - Social login buttons
- ✅ **Responsive Design** - Mobile-friendly
- ✅ **Customizable** - Easy to brand and customize

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.UI
```

---

## 🔧 Usage

### Basic Setup

```csharp
using TrustIdentity.UI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTrustIdentity(options => { ... });
builder.Services.AddTrustIdentityUI();  // Add UI
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseTrustIdentity();
app.MapRazorPages();

app.Run();
```

---

## 📋 UI Pages

### Login Page (`/Account/Login`)

- Username/password input
- "Remember me" checkbox
- External provider buttons (Google, Azure AD, etc.)
- "Forgot password" link
- "Register" link

### Consent Page (`/Account/Consent`)

- Client information
- Requested scopes
- "Remember my decision" checkbox
- Allow/Deny buttons

### Logout Page (`/Account/Logout`)

- Logout confirmation
- Post-logout redirect

### Error Page (`/Account/Error`)

- User-friendly error messages
- Error details (in development)

---

## 🎨 Customization

### Override Pages

Create your own Razor Pages to override defaults:

```
Pages/
└── Account/
    ├── Login.cshtml
    ├── Login.cshtml.cs
    ├── Consent.cshtml
    └── Consent.cshtml.cs
```

### Custom Branding

```csharp
builder.Services.AddTrustIdentityUI(options =>
{
    options.ApplicationName = "My Identity Server";
    options.LogoUrl = "/images/logo.png";
    options.PrimaryColor = "#007bff";
    options.BackgroundColor = "#f8f9fa";
});
```

### Custom CSS

Add your own CSS in `wwwroot/css/site.css`:

```css
.login-page {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-form {
    background: white;
    border-radius: 10px;
    box-shadow: 0 10px 40px rgba(0,0,0,0.1);
}
```

---

## 🔧 Configuration

### UI Options

```csharp
builder.Services.AddTrustIdentityUI(options =>
{
    // Branding
    options.ApplicationName = "My Identity Server";
    options.LogoUrl = "/images/logo.png";
    options.FaviconUrl = "/images/favicon.ico";
    
    // Theme
    options.Theme = "light"; // or "dark"
    options.PrimaryColor = "#007bff";
    options.SecondaryColor = "#6c757d";
    
    // Features
    options.EnableRememberMe = true;
    options.EnableRegistration = true;
    options.EnableForgotPassword = true;
    options.EnableExternalProviders = true;
    
    // Security
    options.RequireEmailConfirmation = false;
    options.RequirePhoneConfirmation = false;
    options.EnableCaptcha = false;
});
```

---

## 🏗️ Architecture

```
TrustIdentity.UI/
├── Pages/
│   └── Account/
│       ├── Login.cshtml
│       ├── Login.cshtml.cs
│       ├── Consent.cshtml
│       ├── Consent.cshtml.cs
│       ├── Logout.cshtml
│       ├── Logout.cshtml.cs
│       └── Error.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

---

## 📚 Documentation

- **[Migration & UI Guide](../../../MIGRATION_AND_UI_GUIDE.md)** - UI customization
- **[Setup Guide](../../../SETUP_GUIDE.md)** - General setup

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
