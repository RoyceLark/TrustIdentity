# OIDC & OAuth 2.0 Support in TrustIdentity

TrustIdentity provides a full-featured implementation of OpenID Connect (OIDC) and OAuth 2.0 protocols, built on top of ASP.NET Core 9/10.

## 🚀 Supported Grant Types

TrustIdentity supports all standard OAuth 2.0 grant types:

1.  **Authorization Code**: The most secure flow for web and mobile apps. Supports PKCE.
2.  **Client Credentials**: For machine-to-machine communication.
3.  **Implicit**: (Legacy) For legacy browser-based apps.
4.  **Hybrid**: Combines authorization code and implicit flows.
5.  **Resource Owner Password Credentials (ROPC)**: For highly trusted apps.
6.  **Device Flow**: For limited-input devices (e.g., Smart TVs).
7.  **Refresh Token**: To obtain new access tokens without user interaction.
8.  **Token Exchange**: For impersonation and delegation (RFC 8693).

## 🔐 Security Features

- **PKCE (Proof Key for Code Exchange)**: Mandatory for public clients to prevent code injection attacks.
- **DPoP (Demonstrating Proof-of-Possession)**: Prevents token replay attacks (RFC 9449).
- **PAR (Pushed Authorization Requests)**: Enhances security by moving authorization parameters from the URL to a back-channel request (RFC 9126).
- **Mutual TLS (mTLS)**: Support for certificate-based client authentication and sender-constrained tokens.

## 🛠️ Configuration

You can configure OIDC/OAuth features using the `TrustIdentityOptions` class during service registration:

```csharp
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://identity.example.com";
    options.Endpoints.EnableAuthorizeEndpoint = true;
    options.Endpoints.EnableTokenEndpoint = true;
    // ... other options
});
```

## 📜 Standards Compliance

TrustIdentity is built to be 100% compliant with the official specifications:
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
- [OAuth 2.0 (RFC 6749)](https://tools.ietf.org/html/rfc6749)
- [OAuth 2.1 (Draft)](https://oauth.net/2.1/)
