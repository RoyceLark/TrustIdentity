# OpenID Certification & Conformance Testing

TrustIdentity is designed to be fully compliant with OpenID Connect and OAuth 2.0 standards. To verify compliance, we recommend using the standard OIDC Conformance Suite.

## Prerequisites
1. Docker installed and running.
2. A publicly accessible URL for your TrustIdentity instance (e.g., via `ngrok`).

## Running the Conformance Suite Locally

1. **Pull the OIDC Conformance Suite image:**
   ```bash
   docker pull welcome.openid.net/conformance-suite:latest
   ```

2. **Run the suite:**
   ```bash
   docker run -it -p 8080:8080 welcome.openid.net/conformance-suite:latest
   ```

3. **Configure the test plan:**
   - Access `http://localhost:8080`.
   - Select "OIDCC-Server-Tests" (OpenID Connect Core).
   - Provide your TrustIdentity discovery URL: `https://your-domain/.well-known/openid-configuration`.

## TrustIdentity Configuration for Certification
To pass standard certification, ensure the following settings are enabled in your `appsettings.json`:

```json
{
  "TrustIdentity": {
    "Compliance": {
      "EnforceNonce": true,
      "EnforceState": true,
      "RequirePkceForPublicClients": true
    }
  }
}
```

## Supported Profiles
TrustIdentity targets certification for:
- Basic OP
- Implicit OP
- Hybrid OP
- Config OP
- Dynamic OP
- Form Post OP
