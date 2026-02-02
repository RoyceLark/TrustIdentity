# TrustIdentity.WsFederation

**WS-Federation support for TrustIdentity**

---

## 📦 Overview

`TrustIdentity.WsFederation` provides WS-Federation protocol support for legacy enterprise integrations. This is **included for free** (unlike Duende which sells it separately).

---

## ✨ Features

- ✅ **WS-Federation 1.2** - Complete protocol support
- ✅ **Passive Sign-In** - Browser-based authentication
- ✅ **Passive Sign-Out** - Logout support
- ✅ **Metadata** - Automatic metadata generation
- ✅ **SAML 1.1 & 2.0 Tokens** - Token format support
- ✅ **Claims Mapping** - Flexible claims transformation

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.WsFederation
```

---

## 🔧 Usage

### Basic Setup

```csharp
using TrustIdentity.WsFederation.Extensions;

builder.Services.AddTrustIdentity(options => { ... })
    .AddWsFederation(options =>
    {
        options.Issuer = "https://identity.example.com/wsfed";
        options.SigningCertificate = certificate;
    });
```

### Advanced Configuration

```csharp
builder.Services.AddWsFederation(options =>
{
    // Issuer
    options.Issuer = "https://identity.example.com/wsfed";
    
    // Endpoints
    options.SignInUrl = "https://identity.example.com/wsfed";
    options.SignOutUrl = "https://identity.example.com/wsfed/signout";
    options.MetadataUrl = "https://identity.example.com/wsfed/metadata";
    
    // Certificates
    options.SigningCertificate = signingCertificate;
    
    // Token options
    options.TokenType = "urn:oasis:names:tc:SAML:2.0:assertion";
    options.TokenLifetime = TimeSpan.FromMinutes(5);
    
    // Relying parties
    options.RelyingParties = new[]
    {
        new RelyingParty
        {
            Realm = "https://app.example.com/",
            ReplyUrl = "https://app.example.com/signin-wsfed",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion"
        }
    };
});
```

---

## 📋 WS-Federation Endpoints

```
GET  /wsfed                      # Sign-in endpoint
GET  /wsfed/signout              # Sign-out endpoint
GET  /wsfed/metadata             # Federation metadata
```

---

## 🔧 Configuration

### Relying Party Configuration

```csharp
options.RelyingParties = new[]
{
    new RelyingParty
    {
        // Realm (Application identifier)
        Realm = "https://app.example.com/",
        
        // Reply URL (where to send token)
        ReplyUrl = "https://app.example.com/signin-wsfed",
        
        // Token type
        TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
        
        // Token lifetime
        TokenLifetime = TimeSpan.FromMinutes(5),
        
        // Claims to include
        ClaimTypesOffered = new[]
        {
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        }
    }
};
```

---

## 🎯 Use Cases

### SharePoint Integration

```csharp
builder.Services.AddWsFederation(options =>
{
    options.RelyingParties = new[]
    {
        new RelyingParty
        {
            Realm = "urn:sharepoint:portal",
            ReplyUrl = "https://sharepoint.example.com/_trust/",
            TokenType = "urn:oasis:names:tc:SAML:1.1:assertion"
        }
    };
});
```

### ADFS Integration

```csharp
builder.Services.AddWsFederation(options =>
{
    options.Issuer = "https://identity.example.com/wsfed";
    options.RelyingParties = new[]
    {
        new RelyingParty
        {
            Realm = "https://adfs.example.com/",
            ReplyUrl = "https://adfs.example.com/adfs/ls/",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion"
        }
    };
});
```

---

## 📊 WS-Federation Request Example

```http
GET /wsfed?wa=wsignin1.0
    &wtrealm=https://app.example.com/
    &wreply=https://app.example.com/signin-wsfed
    &wctx=rm=0&id=passive&ru=/
```

### Response (SAML Token)

```xml
<t:RequestSecurityTokenResponse xmlns:t="http://schemas.xmlsoap.org/ws/2005/02/trust">
  <t:Lifetime>
    <wsu:Created>2026-02-02T12:00:00Z</wsu:Created>
    <wsu:Expires>2026-02-02T12:05:00Z</wsu:Expires>
  </t:Lifetime>
  <t:RequestedSecurityToken>
    <saml:Assertion xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion">
      <saml:Issuer>https://identity.example.com/wsfed</saml:Issuer>
      <saml:Subject>
        <saml:NameID>user@example.com</saml:NameID>
      </saml:Subject>
      <saml:AttributeStatement>
        <saml:Attribute Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name">
          <saml:AttributeValue>John Doe</saml:AttributeValue>
        </saml:Attribute>
      </saml:AttributeStatement>
    </saml:Assertion>
  </t:RequestedSecurityToken>
</t:RequestSecurityTokenResponse>
```

---

## 🔒 Security

### Token Signing

```csharp
options.SigningCertificate = certificate;
options.SignTokens = true;
```

### Claims Mapping

```csharp
options.ClaimsMapping = new Dictionary<string, string>
{
    { "sub", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" },
    { "name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" },
    { "email", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" },
    { "role", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" }
};
```

---

## 🏗️ Architecture

```
TrustIdentity.WsFederation/
├── Services/          # WS-Fed services
│   ├── WsFederationService.cs
│   ├── TokenService.cs
│   └── MetadataService.cs
├── Endpoints/        # WS-Fed endpoints
├── Models/           # WS-Fed models
└── Extensions/       # Configuration extensions
```

---

## 📚 Documentation

- **[Setup Guide](../../../SETUP_GUIDE.md)** - General setup
- **[Main Documentation](../../../README.md)** - Overview

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
