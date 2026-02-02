# TrustIdentity.Saml

**SAML 2.0 support for TrustIdentity**

---

## 📦 Overview

`TrustIdentity.Saml` provides SAML 2.0 Identity Provider (IdP) and Service Provider (SP) capabilities. This is **included for free** (unlike Duende which sells it separately).

---

## ✨ Features

- ✅ **SAML 2.0 Identity Provider** - Act as SAML IdP
- ✅ **SAML 2.0 Service Provider** - Act as SAML SP
- ✅ **Single Sign-On (SSO)** - SAML SSO support
- ✅ **Single Logout (SLO)** - SAML SLO support
- ✅ **Metadata** - Automatic metadata generation
- ✅ **Signature Validation** - XML signature support
- ✅ **Encryption** - SAML assertion encryption

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.Saml
```

---

## 🔧 Usage

### As SAML Identity Provider (IdP)

```csharp
using TrustIdentity.Saml.Extensions;

builder.Services.AddTrustIdentity(options => { ... })
    .AddSamlIdentityProvider(options =>
    {
        options.EntityId = "https://identity.example.com/saml";
        options.SigningCertificate = certificate;
        options.SingleSignOnServiceUrl = "https://identity.example.com/saml/sso";
        options.SingleLogoutServiceUrl = "https://identity.example.com/saml/slo";
    });
```

### As SAML Service Provider (SP)

```csharp
builder.Services.AddTrustIdentity(options => { ... })
    .AddSamlServiceProvider(options =>
    {
        options.EntityId = "https://app.example.com/saml";
        options.AssertionConsumerServiceUrl = "https://app.example.com/saml/acs";
        options.IdentityProviderMetadataUrl = "https://idp.example.com/saml/metadata";
    });
```

---

## 📋 SAML Endpoints

### Identity Provider Endpoints

```
GET  /saml/metadata              # SAML metadata
POST /saml/sso                   # Single Sign-On
POST /saml/slo                   # Single Logout
GET  /saml/slo                   # Single Logout (redirect)
```

### Service Provider Endpoints

```
GET  /saml/metadata              # SAML metadata
POST /saml/acs                   # Assertion Consumer Service
POST /saml/slo                   # Single Logout
```

---

## 🔧 Configuration

### SAML IdP Configuration

```csharp
builder.Services.AddSamlIdentityProvider(options =>
{
    // Entity ID
    options.EntityId = "https://identity.example.com/saml";
    
    // Endpoints
    options.SingleSignOnServiceUrl = "https://identity.example.com/saml/sso";
    options.SingleLogoutServiceUrl = "https://identity.example.com/saml/slo";
    
    // Certificates
    options.SigningCertificate = signingCertificate;
    options.EncryptionCertificate = encryptionCertificate;
    
    // Options
    options.RequireSignedRequests = true;
    options.SignAssertions = true;
    options.EncryptAssertions = false;
    options.NameIdFormat = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent";
    
    // Service Providers
    options.ServiceProviders = new[]
    {
        new ServiceProvider
        {
            EntityId = "https://app.example.com/saml",
            AssertionConsumerServiceUrl = "https://app.example.com/saml/acs",
            Certificate = spCertificate
        }
    };
});
```

### SAML SP Configuration

```csharp
builder.Services.AddSamlServiceProvider(options =>
{
    // Entity ID
    options.EntityId = "https://app.example.com/saml";
    
    // Endpoints
    options.AssertionConsumerServiceUrl = "https://app.example.com/saml/acs";
    options.SingleLogoutServiceUrl = "https://app.example.com/saml/slo";
    
    // Identity Provider
    options.IdentityProviderEntityId = "https://idp.example.com/saml";
    options.IdentityProviderMetadataUrl = "https://idp.example.com/saml/metadata";
    options.IdentityProviderSingleSignOnUrl = "https://idp.example.com/saml/sso";
    
    // Certificates
    options.SigningCertificate = signingCertificate;
    options.IdentityProviderCertificate = idpCertificate;
    
    // Options
    options.RequireSignedAssertions = true;
    options.RequireEncryptedAssertions = false;
    options.SignAuthenticationRequests = true;
});
```

---

## 🎯 Use Cases

### Enterprise SSO

Integrate with enterprise SAML providers:

```csharp
// Configure SAML SP to work with Azure AD, Okta, etc.
builder.Services.AddSamlServiceProvider(options =>
{
    options.IdentityProviderMetadataUrl = "https://login.microsoftonline.com/.../federationmetadata/2007-06/federationmetadata.xml";
});
```

### Provide SAML SSO to Applications

Act as SAML IdP for your applications:

```csharp
// Configure SAML IdP
builder.Services.AddSamlIdentityProvider(options =>
{
    options.ServiceProviders = new[]
    {
        new ServiceProvider
        {
            EntityId = "https://salesforce.com",
            AssertionConsumerServiceUrl = "https://company.my.salesforce.com/..."
        }
    };
});
```

---

## 📊 SAML Assertion Example

```xml
<saml:Assertion xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion" 
                ID="_abc123" 
                Version="2.0" 
                IssueInstant="2026-02-02T12:00:00Z">
  <saml:Issuer>https://identity.example.com/saml</saml:Issuer>
  <saml:Subject>
    <saml:NameID Format="urn:oasis:names:tc:SAML:2.0:nameid-format:persistent">
      user@example.com
    </saml:NameID>
  </saml:Subject>
  <saml:Conditions NotBefore="2026-02-02T12:00:00Z" 
                   NotOnOrAfter="2026-02-02T12:05:00Z">
    <saml:AudienceRestriction>
      <saml:Audience>https://app.example.com/saml</saml:Audience>
    </saml:AudienceRestriction>
  </saml:Conditions>
  <saml:AttributeStatement>
    <saml:Attribute Name="email">
      <saml:AttributeValue>user@example.com</saml:AttributeValue>
    </saml:Attribute>
    <saml:Attribute Name="name">
      <saml:AttributeValue>John Doe</saml:AttributeValue>
    </saml:Attribute>
  </saml:AttributeStatement>
</saml:Assertion>
```

---

## 🔒 Security

### Signature Validation

```csharp
options.RequireSignedRequests = true;
options.RequireSignedAssertions = true;
options.SigningCertificate = certificate;
```

### Assertion Encryption

```csharp
options.EncryptAssertions = true;
options.EncryptionCertificate = encryptionCertificate;
```

---

## 🏗️ Architecture

```
TrustIdentity.Saml/
├── Services/          # SAML services
│   ├── SamlService.cs
│   ├── AssertionService.cs
│   └── MetadataService.cs
├── Endpoints/        # SAML endpoints
├── Models/           # SAML models
└── Extensions/       # Configuration extensions
```

---

## 📚 Documentation

- **[Setup Guide](../../../SETUP_GUIDE.md)** - General setup
- **[Main Documentation](../../../README.md)** - Overview

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
