# TrustIdentity.ExternalProviders

**External identity provider integration**

---

## 📦 Overview

`TrustIdentity.ExternalProviders` enables integration with external identity providers like Azure AD, Google, Facebook, and GitHub for federated authentication.

---

## ✨ Supported Providers

- ✅ **Azure AD** - Microsoft Azure Active Directory
- ✅ **Azure AD B2C** - Azure AD B2C tenants
- ✅ **Google** - Google authentication
- ✅ **Facebook** - Facebook login
- ✅ **GitHub** - GitHub authentication
- ✅ **Generic OIDC** - Any OpenID Connect provider

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.ExternalProviders
```

---

## 🔧 Usage

### Azure AD

```csharp
using TrustIdentity.ExternalProviders.Extensions;

builder.Services.AddTrustIdentity(options => { ... })
    .AddExternalProvider("AzureAD", options =>
    {
        options.ClientId = "your-azure-client-id";
        options.ClientSecret = "your-azure-client-secret";
        options.TenantId = "your-tenant-id";
        options.CallbackPath = "/signin-azuread";
    });
```

### Google

```csharp
builder.Services.AddTrustIdentity(options => { ... })
    .AddExternalProvider("Google", options =>
    {
        options.ClientId = "your-google-client-id.apps.googleusercontent.com";
        options.ClientSecret = "your-google-client-secret";
        options.CallbackPath = "/signin-google";
    });
```

### Facebook

```csharp
builder.Services.AddTrustIdentity(options => { ... })
    .AddExternalProvider("Facebook", options =>
    {
        options.AppId = "your-facebook-app-id";
        options.AppSecret = "your-facebook-app-secret";
        options.CallbackPath = "/signin-facebook";
    });
```

### GitHub

```csharp
builder.Services.AddTrustIdentity(options => { ... })
    .AddExternalProvider("GitHub", options =>
    {
        options.ClientId = "your-github-client-id";
        options.ClientSecret = "your-github-client-secret";
        options.CallbackPath = "/signin-github";
    });
```

---

## 📋 Configuration

### Azure AD B2C

```csharp
builder.Services.AddExternalProvider("AzureADB2C", options =>
{
    options.Instance = "https://yourtenant.b2clogin.com";
    options.Domain = "yourtenant.onmicrosoft.com";
    options.TenantId = "your-tenant-id";
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
    options.SignUpSignInPolicyId = "B2C_1_signupsignin";
    options.CallbackPath = "/signin-azureadb2c";
});
```

### Generic OIDC Provider

```csharp
builder.Services.AddExternalProvider("CustomOIDC", options =>
{
    options.Authority = "https://custom-idp.example.com";
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
    options.ResponseType = "code";
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.CallbackPath = "/signin-custom";
});
```

---

## 🎯 Use Cases

### Social Login

Allow users to login with their social accounts:

```csharp
builder.Services.AddTrustIdentity(options => { ... })
    .AddExternalProvider("Google", googleOptions)
    .AddExternalProvider("Facebook", facebookOptions)
    .AddExternalProvider("GitHub", githubOptions);
```

### Enterprise Federation

Integrate with corporate identity providers:

```csharp
builder.Services.AddTrustIdentity(options => { ... })
    .AddExternalProvider("AzureAD", azureOptions)
    .AddExternalProvider("Okta", oktaOptions)
    .AddExternalProvider("Auth0", auth0Options);
```

---

## 🔧 Advanced Configuration

### Claims Mapping

```csharp
builder.Services.AddExternalProvider("Google", options =>
{
    options.ClientId = "...";
    options.ClientSecret = "...";
    
    // Map external claims to internal claims
    options.ClaimActions.MapJsonKey("picture", "picture");
    options.ClaimActions.MapJsonKey("locale", "locale");
    
    // Save tokens
    options.SaveTokens = true;
    
    // Additional scopes
    options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
});
```

### Custom Events

```csharp
builder.Services.AddExternalProvider("AzureAD", options =>
{
    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            // Custom logic after successful authentication
            var email = context.Principal.FindFirst(ClaimTypes.Email)?.Value;
            await CreateOrUpdateUserAsync(email);
        },
        
        OnRemoteFailure = async context =>
        {
            // Handle authentication failures
            await LogFailureAsync(context.Failure);
        }
    };
});
```

---

## 📊 Provider Setup Guides

### Google Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Create a new project
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add authorized redirect URI: `https://yourdomain.com/signin-google`
6. Copy Client ID and Client Secret

### Azure AD Setup

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to Azure Active Directory
3. Go to App registrations → New registration
4. Add redirect URI: `https://yourdomain.com/signin-azuread`
5. Create a client secret
6. Copy Application (client) ID, Directory (tenant) ID, and client secret

### Facebook Setup

1. Go to [Facebook Developers](https://developers.facebook.com)
2. Create a new app
3. Add Facebook Login product
4. Configure OAuth redirect URI: `https://yourdomain.com/signin-facebook`
5. Copy App ID and App Secret

### GitHub Setup

1. Go to [GitHub Settings](https://github.com/settings/developers)
2. Create a new OAuth App
3. Set Authorization callback URL: `https://yourdomain.com/signin-github`
4. Copy Client ID and Client Secret

---

## 🏗️ Architecture

```
TrustIdentity.ExternalProviders/
├── Providers/         # Provider implementations
│   ├── AzureADProvider.cs
│   ├── GoogleProvider.cs
│   ├── FacebookProvider.cs
│   └── GitHubProvider.cs
├── Extensions/       # Configuration extensions
└── Models/          # Provider models
```

---

## 📚 Documentation

- **[External Providers Guide](../../../EXTERNAL_PROVIDERS_GUIDE.md)** - Detailed setup
- **[Setup Guide](../../../SETUP_GUIDE.md)** - General setup

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
