# Integration Tests - README

## Overview

The integration tests validate the TrustIdentity endpoints by making real HTTP requests to a running instance of the Identity Server.

## Prerequisites

Before running the integration tests, you **must** have the TestWebApp running.

## How to Run Integration Tests

### Step 1: Start the TestWebApp

In one terminal, start the TestWebApp:

```powershell
cd c:\cnxaicoder\TrustIdentity
dotnet run --project samples/TestWebApp
```

Wait until you see:
```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
```

### Step 2: Run the Integration Tests

In a **separate** terminal, run the integration tests:

```powershell
cd c:\cnxaicoder\TrustIdentity
dotnet test tests/TrustIdentity.IntegrationTests/TrustIdentity.IntegrationTests.csproj
```

## Test Coverage

The integration tests cover the following endpoints:

### 1. Token Endpoint (`/connect/token`)
- ✅ Client Credentials flow
- ✅ Resource Owner Password flow
- ✅ Refresh Token flow
- ✅ Invalid client error handling
- ✅ Invalid grant error handling
- ✅ Unsupported grant type error handling
- ✅ Missing parameters error handling

### 2. UserInfo Endpoint (`/connect/userinfo`)
- ✅ Valid token returns user claims
- ✅ Missing token returns unauthorized
- ✅ Invalid token returns unauthorized
- ✅ Scope-based claim filtering

### 3. Discovery Endpoint (`/.well-known/openid-configuration`)
- ✅ Returns configuration
- ✅ Includes supported grant types
- ✅ Includes supported scopes
- ✅ Includes supported response types

### 4. Introspection Endpoint (`/connect/introspect`)
- ✅ Valid token returns active status
- ✅ Invalid token returns inactive status
- ✅ Missing client authentication returns error

### 5. Revocation Endpoint (`/connect/revoke`)
- ✅ Revoke access token
- ✅ Revoke refresh token
- ✅ Invalid token handling (per OAuth spec)
- ✅ Missing client authentication returns error
- ✅ Revoked token cannot be used

## Troubleshooting

### Error: "Connection refused" or "No connection could be made"

**Problem**: The TestWebApp is not running.

**Solution**: Make sure you started the TestWebApp in Step 1 before running the tests.

### Error: "Certificate validation failed"

**Problem**: HTTPS certificate issues.

**Solution**: The tests use `https://localhost:5001`. Ensure your development certificates are trusted:

```powershell
dotnet dev-certs https --trust
```

### Tests fail with "invalid_client"

**Problem**: The test clients don't match the TestWebApp configuration.

**Solution**: Ensure the TestWebApp has the following clients configured:
- `api-client` with secret `secret` (client_credentials grant)
- `web-client` with secret `secret` (password, refresh_token grants)

These should already be configured in `samples/TestWebApp/Program.cs`.

## Running Specific Tests

### Run only Token Endpoint tests
```powershell
dotnet test tests/TrustIdentity.IntegrationTests/TrustIdentity.IntegrationTests.csproj --filter "FullyQualifiedName~TokenEndpointTests"
```

### Run only UserInfo tests
```powershell
dotnet test tests/TrustIdentity.IntegrationTests/TrustIdentity.IntegrationTests.csproj --filter "FullyQualifiedName~UserInfoEndpointTests"
```

### Run with detailed output
```powershell
dotnet test tests/TrustIdentity.IntegrationTests/TrustIdentity.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

## Notes

- Integration tests make real HTTP requests to `https://localhost:5001`
- Tests are designed to be independent and can run in any order
- Each test cleans up after itself where possible
- Tests use the test users and clients configured in TestWebApp

## Test Data

The tests expect the following test data to be available in TestWebApp:

### Test Users
- Username: `alice`
- Password: `password`

### Test Clients
- Client ID: `api-client`
  - Secret: `secret`
  - Allowed Grants: `client_credentials`
  - Allowed Scopes: `api1`

- Client ID: `web-client`
  - Secret: `secret`
  - Allowed Grants: `password`, `refresh_token`, `authorization_code`
  - Allowed Scopes: `openid`, `profile`, `email`, `offline_access`
