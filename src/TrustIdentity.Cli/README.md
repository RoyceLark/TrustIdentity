# TrustIdentity.Cli

**Command-line interface for TrustIdentity**

---

## 📦 Overview

`TrustIdentity.Cli` provides a command-line tool for managing TrustIdentity servers.

---

## ✨ Features

- ✅ **Client Management** - Create, list, update clients
- ✅ **User Management** - Manage users
- ✅ **Key Management** - Generate signing keys
- ✅ **Database Migrations** - Run migrations
- ✅ **Configuration** - Export/import configuration

---

## 🚀 Installation

```bash
dotnet tool install --global TrustIdentity.Cli
```

---

## 🔧 Usage

```bash
# Create client
trustidentity client create --id web-app --name "Web Application"

# List clients
trustidentity client list

# Generate signing key
trustidentity key generate --algorithm RS256

# Run migrations
trustidentity database migrate
```

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
