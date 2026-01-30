# Configuration Reference

TrustIdentity is highly configurable via `appsettings.json` or Environment Variables.

## Top-Level Options

| Option | Default | Description |
|--------|---------|-------------|
| `IssuerUri` | `https://localhost:5001` | The public URI of your identity server. |
| `EnableAI` | `false` | Enable/Disable all AI-powered features. |
| `EnableFraudDetection` | `false` | Enable/Disable AI fraud detection. |
| `RequireHttps` | `true` | Enforce HTTPS for all requests. |

## Authentication Options

Under `TrustIdentity:Authentication`:

| Option | Default | Description |
|--------|---------|-------------|
| `CookieLifetime` | `3600` | Session cookie lifetime in seconds. |
| `AccessTokenLifetime` | `3600` | Default access token lifetime. |
| `RefreshTokenLifetime` | `2592000` | Default refresh token lifetime (30 days). |

## Endpoint Options

Enable or disable specific OIDC/OAuth2 endpoints under `TrustIdentity:Endpoints`:

*   `EnableAuthorizeEndpoint`
*   `EnableTokenEndpoint`
*   `EnableDiscoveryEndpoint`
*   `EnableUserInfoEndpoint`
*   `EnableIntrospectionEndpoint`

## Database Connection

TrustIdentity uses standard Entity Framework Core connection strings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TrustIdentity.db"
  }
}
```

## Example `appsettings.json`

```json
{
  "TrustIdentity": {
    "IssuerUri": "https://identity.example.com",
    "EnableAI": true,
    "Authentication": {
      "AccessTokenLifetime": 7200
    }
  }
}
```
