# TrustIdentity CLI User Guide

This guide provides detailed instructions on how to use the `TrustIdentity.Cli` tool to manage your TrustIdentity server.

## 📋 Table of Contents
1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [Authentication](#authentication)
4. [Client Management](#client-management)
5. [User Administration](#user-administration)
6. [Key Management](#key-management)
7. [Database Operations](#database-operations)
8. [Configuration Portability](#configuration-portability)
9. [Troubleshooting](#troubleshooting)

---

## 1. Introduction
The `TrustIdentity.Cli` is a powerful administrative tool designed for DevOps and Identity Administrators. It allows for automation, server seeding, and rapid configuration without needing to access the web UI.

---

## 2. Getting Started

### Installation
Ensure you have the .NET SDK installed. You can install the CLI globally:
```bash
dotnet tool install --global TrustIdentity.Cli
```

### Basic Command Structure
All commands follow this pattern:
```bash
TrustIdentity.Cli [category] [action] [options]
```
To see help for any command:
```bash
TrustIdentity.Cli --help
TrustIdentity.Cli client --help
TrustIdentity.Cli client create --help
```

---

## 3. Authentication
Most commands require administrative privileges. You can provide authentication in two ways:

### Option A: Command Line Flag
Pass your admin access token directly:
```bash
TrustIdentity.Cli client list --token "your_admin_jwt_token"
```

### Option B: Environment Variable
Set the token once in your session:
```bash
# Windows (PowerShell)
$env:TRUSTIDENTITY_TOKEN="your_token"

# Linux / macOS
export TRUSTIDENTITY_TOKEN="your_token"
```

---

## 4. Client Management
Clients represent the applications (Web, Mobile, SPAs) that will use TrustIdentity for login.

### Creating a Web App Client
```bash
TrustIdentity.Cli client create my-web-app \
  --name "Customer Portal" \
  --grant-types authorization_code \
  --redirect-uris https://portal.com/callback \
  --scopes "openid profile email api1"
```

### Creating a Machine-to-Machine Client
```bash
TrustIdentity.Cli client create worker-service \
  --name "Background Worker" \
  --secret "SuperSecret123!" \
  --grant-types client_credentials \
  --scopes "api1"
```

### Updating a Client
```bash
TrustIdentity.Cli client update my-web-app --enabled false
```

---

## 5. User Administration
Manage your users directly from the terminal.

### User Search and Pagination
```bash
# List first 10 users with 'smith' in their name/email
TrustIdentity.Cli user list --page 1 --page-size 10 --search "smith"
```

### Account Lifecycle
```bash
# Create user
TrustIdentity.Cli user create jdoe --email jane@company.com --password "ChangeMe123!"

# Block access
TrustIdentity.Cli user lock {user-id}

# Grant access
TrustIdentity.Cli user unlock {user-id}

# Forced password reset
TrustIdentity.Cli user reset-password {user-id} --password "NewPass456!"
```

---

## 6. Key Management
Generate and export cryptographic keys for token signing.

### Generate a JWK (JSON) for OIDC
```bash
TrustIdentity.Cli key generate --algorithm RS256 --output signing-key.json
```

### Create a Production PFX Certificate
```bash
TrustIdentity.Cli key generate \
  --format pfx \
  --output identity-cert.pfx \
  --password "CertPass123"
```

### Export Public Key for Partners
```bash
TrustIdentity.Cli key export --input identity-cert.pfx --output public.pem --password "CertPass123"
```

---

## 7. Database Operations
Manage migrations and data seeding.

### Applying Migrations
```bash
TrustIdentity.Cli database migrate \
  --connection "Server=my-db;Database=TrustIdentity;..." \
  --provider PostgreSQL
```

### Data Seeding (Seed for Dev)
```bash
TrustIdentity.Cli database seed --connection "..." --type development
```

---

## 8. Configuration Portability
Easily move configurations between Environment (Dev > Staging > Prod).

### Export Full Setup
```bash
TrustIdentity.Cli config export --output prod-backup.json --include-secrets
```

### Import to New Server
```bash
TrustIdentity.Cli config import --input prod-backup.json --server https://new-server.com
```

### Validation (Dry Run)
Verify a config file without changing anything on the server:
```bash
TrustIdentity.Cli config import --input setup.json --dry-run
```

---

## 9. Troubleshooting

### Connection Errors
- Verify the `--server` URL is correct.
- Ensure the server is reachable and CIDR/Firewall allows your IP.
- Use `TrustIdentity.Cli info` to check connectivity.

### "401 Unauthorized"
- Your token might be expired. Generate a fresh one from the Master Identity Server.
- Ensure the token has the `identity_admin` scope.

### "403 Forbidden"
- The token is valid but the user does not have permission for that specific command.

---

*For more information, visit the [main documentation](https://github.com/roycelark/trustidentity/docs).*
