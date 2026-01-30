# FAPI Compliance Guide

TrustIdentity is designed to support **Financial-grade API (FAPI)** security profiles. This guide outlines how to configure TrustIdentity to meet FAPI 1.0 Advanced and FAPI 2.0 requirements.

## 1. Required Configurations

To achieve FAPI compliance, you must configure the following:

### 1.1. Enforce MTLS or DPoP
FAPI requires sender-constrained access tokens.
- **MTLS:** Ensure your getting-started client has `RequireClientSecret = false` and authenticates via mTLS.
- **DPoP:** Use the new DPoP support. Ensure clients send `DPoP` headers.

### 1.2. Pushed Authorization Requests (PAR)
FAPI 2.0 requires the use of PAR.
- **Endpoint:** `/connect/par`
- **Configuration:** Set `RequirePushedAuthorization = true` on your clients.

### 1.3. JWT Secured Authorization Requests (JAR)
If not using PAR, JAR is required for FAPI 1.0 Advanced.
- **Configuration:** Set `RequireRequestObject = true`.

### 1.4. Key Rotation
- Regular rotation of signing keys is enforced by the `KeyManagementService`.
- Use `PS256` or `ES256` instead of `RS256` for signing if possible (configure in `TrustIdentityOptions`).

## 2. Client Configuration Example

```csharp
new Client
{
    ClientId = "fapi_client",
    AllowedGrantTypes = GrantTypes.Code,
    RequirePkce = true,
    RequireClientSecret = false,
    
    // FAPI Requirements
    RequirePushedAuthorization = true, // Enforce PAR
    AllowedCorsOrigins = { "https://financial-app.com" },
    RedirectUris = { "https://financial-app.com/cb" },
    
    // MTLS binding
    TlsClientCertificateBoundAccessTokens = true
}
```

## 3. Deployment Checklist

- [ ] **SSL/TLS:** Ensure strong ciphers (TLS 1.2+ or 1.3) are used.
- [ ] **BFF:** Use the `TrustIdentity.Bff` package for all browser-based clients to keep tokens out of the browser.
- [ ] **Logging:** Ensure Personal Identifiable Information (PII) is not logged in production.
- [ ] **HSM:** For production, replace `SigningCredentialStore` with an implementation backed by an HSM or Azure KeyVault.

## 4. Certification

TrustIdentity implements the protocols required for FAPI. To get official certification, you must run the [OpenID Foundation Conformance Suite](https://www.certification.openid.net/) against your deployment.
