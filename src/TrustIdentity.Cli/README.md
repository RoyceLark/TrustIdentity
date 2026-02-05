# TrustIdentity.Cli

**Command-line interface for TrustIdentity**

---

## 📦 Overview

`TrustIdentity.Cli` provides a comprehensive command-line tool for managing TrustIdentity servers. It offers a rich set of commands for administrative tasks, configuration management, and operational workflows.

For a step-by-step tutorial and detailed command references, see the **[User Guide](GUIDE.md)**.

---

## ✨ Features

- ✅ **Client Management** - Create, list, update, and delete OAuth2/OIDC clients
- ✅ **User Management** - Full user administration with account controls
- ✅ **Key Management** - Generate signing keys (RSA, ECDSA) in multiple formats
- ✅ **Database Migrations** - Run and manage database migrations
- ✅ **Configuration** - Export/import server configuration with validation

---

## 🚀 Installation

Install as a global .NET tool:

```bash
dotnet tool install --global TrustIdentity.Cli
```

Or install locally in a project:

```bash
dotnet tool install TrustIdentity.Cli
```

---

## 🔧 Usage

### Global Options

```bash
--server, -s    Server URL (default: https://localhost:5001)
--token, -t     Authentication token for API access
```

### Client Management

```bash
# List all clients
TrustIdentity.Cli client list --server https://myserver.com

# Get a specific client
TrustIdentity.Cli client get web-app

# Create a new client
TrustIdentity.Cli client create my-app \
  --name "My Application" \
  --grant-types authorization_code \
  --redirect-uris https://myapp.com/callback \
  --scopes openid profile email

# Update a client
TrustIdentity.Cli client update my-app --name "Updated Name" --enabled true

# Delete a client
TrustIdentity.Cli client delete my-app --force
```

### User Management

```bash
# List users with pagination
TrustIdentity.Cli user list --page 1 --page-size 20

# Search users
TrustIdentity.Cli user list --search "john@example.com"

# Get a specific user
TrustIdentity.Cli user get {subject-id}

# Create a new user
TrustIdentity.Cli user create johndoe \
  --email john@example.com \
  --password SecurePass123!

# Update a user
TrustIdentity.Cli user update {subject-id} --email newemail@example.com

# Lock/unlock user accounts
TrustIdentity.Cli user lock {subject-id}
TrustIdentity.Cli user unlock {subject-id}

# Reset password
TrustIdentity.Cli user reset-password {subject-id} --password NewPass123!

# Delete a user
TrustIdentity.Cli user delete {subject-id} --force
```

### Key Management

```bash
# Generate RSA signing key (default: RS256)
TrustIdentity.Cli key generate --algorithm RS256 --output signing-key.json

# Generate ECDSA key
TrustIdentity.Cli key generate --algorithm ES256 --format pem --output key.pem

# Generate PFX certificate with password
TrustIdentity.Cli key generate \
  --algorithm RS256 \
  --format pfx \
  --output cert.pfx \
  --password MySecurePassword

# Export public key from certificate
TrustIdentity.Cli key export \
  --input cert.pfx \
  --output public-key.pem \
  --password MySecurePassword
```

**Supported Algorithms:**
- RSA: RS256, RS384, RS512
- ECDSA: ES256, ES384, ES512

**Supported Formats:**
- `json` - JWK format (default)
- `pem` - PEM format
- `pfx` - PKCS#12 certificate

### Database Management

```bash
# Run migrations
TrustIdentity.Cli database migrate \
  --connection "Server=localhost;Database=TrustIdentity;..." \
  --provider SqlServer

# Drop database (with confirmation)
TrustIdentity.Cli database drop --connection "..." --force

# Seed database
TrustIdentity.Cli database seed \
  --connection "..." \
  --type development

# Backup database
TrustIdentity.Cli database backup \
  --connection "..." \
  --output backup.bak
```

**Supported Providers:**
- SqlServer
- PostgreSQL
- MySQL
- SQLite

### Configuration Management

```bash
# Export configuration
TrustIdentity.Cli config export --output config.json

# Export with secrets
TrustIdentity.Cli config export --output config.json --include-secrets

# Import configuration
TrustIdentity.Cli config import --input config.json

# Import with merge (update existing)
TrustIdentity.Cli config import --input config.json --merge

# Dry run (validate without applying)
TrustIdentity.Cli config import --input config.json --dry-run

# Validate configuration file
TrustIdentity.Cli config validate --input config.json
```

### Utility Commands

```bash
# Display version
TrustIdentity.Cli --version

# Check server connectivity and info
TrustIdentity.Cli info --server https://myserver.com
```

---

## 📋 Configuration File Format

Export/import configuration files use the following JSON structure:

```json
{
  "ExportDate": "2026-02-04T16:00:00Z",
  "IncludesSecrets": false,
  "Clients": [
    {
      "ClientId": "web-app",
      "ClientName": "Web Application",
      "Enabled": true,
      "AllowedGrantTypes": ["authorization_code"],
      "RedirectUris": ["https://app.com/callback"],
      "AllowedScopes": ["openid", "profile"]
    }
  ],
  "IdentityResources": [...],
  "ApiResources": [...],
  "ApiScopes": [...]
}
```

---

## 🔐 Authentication

For protected endpoints, use the `--token` option:

```bash
TrustIdentity.Cli client list --token "your-access-token"
```

Or set the server URL globally:

```bash
export TRUSTIDENTITY_SERVER="https://myserver.com"
TrustIdentity.Cli client list
```

---

## 📖 Examples

### Complete Client Setup

```bash
# Create a web application client
TrustIdentity.Cli client create webapp \
  --name "My Web App" \
  --grant-types authorization_code \
  --redirect-uris https://webapp.com/signin-oidc \
  --scopes openid profile email api1 \
  --server https://identity.mycompany.com

# Create a machine-to-machine client
TrustIdentity.Cli client create api-client \
  --name "API Client" \
  --secret MyClientSecret123! \
  --grant-types client_credentials \
  --scopes api1 api2
```

### Backup and Restore

```bash
# Export current configuration
TrustIdentity.Cli config export \
  --output backup-$(date +%Y%m%d).json \
  --include-secrets

# Restore to new server
TrustIdentity.Cli config import \
  --input backup-20260204.json \
  --server https://newserver.com \
  --token "admin-token"
```

### Key Rotation

```bash
# Generate new signing key
TrustIdentity.Cli key generate \
  --algorithm RS256 \
  --format pfx \
  --output new-signing-key.pfx \
  --password SecurePassword123!

# Export public key for validation
TrustIdentity.Cli key export \
  --input new-signing-key.pfx \
  --output public-key.pem \
  --password SecurePassword123!
```

---

## 🛠️ Development

Build the tool locally:

```bash
dotnet build
```

Pack as a tool:

```bash
dotnet pack
```

Install locally for testing:

```bash
dotnet tool install --global --add-source ./nupkg TrustIdentity.Cli
```

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
