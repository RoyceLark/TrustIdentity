# 📚 TrustIdentity Documentation Index

**Quick reference to all TrustIdentity documentation**

---

## 🎯 Essential Documentation

### 1. **[README.md](README.md)** - Start Here!
**Overview of TrustIdentity features and capabilities**
- What is TrustIdentity
- Key features
- Why choose TrustIdentity
- Quick start
- Feature comparison with Duende

### 2. **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Complete Setup
**Step-by-step guide to set up TrustIdentity**
- Installation
- Basic configuration
- Database setup
- Production deployment
- Testing
- Common scenarios

### 3. **[DATABASE_SETUP.md](DATABASE_SETUP.md)** - Database Configuration
**Complete database setup and migrations**
- Supported databases (SQL Server, PostgreSQL, MySQL, SQLite)
- Creating migrations
- Seeding data
- Database schemas
- Backup and maintenance
- Docker database setup

### 4. **[MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)** - Migrate from Duende
**How to migrate from Duende IdentityServer to TrustIdentity**
- 3-step quick migration
- Package mapping
- Configuration compatibility
- Custom implementations
- Cost savings calculator

---

## 📖 Additional Guides

### 5. **[EXTERNAL_PROVIDERS_GUIDE.md](EXTERNAL_PROVIDERS_GUIDE.md)** - External Identity Providers
**Integrate with Azure AD, Google, Facebook, GitHub**
- Azure AD B2C configuration
- Google authentication
- Facebook login
- GitHub integration
- Custom OIDC providers

### 6. **[MIGRATION_AND_UI_GUIDE.md](MIGRATION_AND_UI_GUIDE.md)** - UI Customization
**Customize login, consent, and logout pages**
- UI customization
- Branding
- Custom pages

---

## 🚀 Quick Links

### Getting Started
1. Read [README.md](README.md) for overview
2. Follow [SETUP_GUIDE.md](SETUP_GUIDE.md) for installation
3. Configure database with [DATABASE_SETUP.md](DATABASE_SETUP.md)

### Migrating from Duende
1. Read [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)
2. Follow 3-step migration process
3. Test thoroughly

### Advanced Features
1. External providers: [EXTERNAL_PROVIDERS_GUIDE.md](EXTERNAL_PROVIDERS_GUIDE.md)
2. UI customization: [MIGRATION_AND_UI_GUIDE.md](MIGRATION_AND_UI_GUIDE.md)

---

## 📊 Documentation Summary

| Document | Purpose | When to Use |
|----------|---------|-------------|
| **README.md** | Overview & features | First time learning about TrustIdentity |
| **SETUP_GUIDE.md** | Complete setup | Setting up new project |
| **DATABASE_SETUP.md** | Database config | Configuring database & migrations |
| **MIGRATION_GUIDE.md** | Duende migration | Migrating from Duende IdentityServer |
| **EXTERNAL_PROVIDERS_GUIDE.md** | External IdPs | Adding Azure AD, Google, etc. |
| **MIGRATION_AND_UI_GUIDE.md** | UI customization | Customizing login/consent pages |

---

## 🎯 Common Tasks

### I want to...

**...get started with TrustIdentity**
→ Read [README.md](README.md) then [SETUP_GUIDE.md](SETUP_GUIDE.md)

**...set up a database**
→ Follow [DATABASE_SETUP.md](DATABASE_SETUP.md)

**...migrate from Duende IdentityServer**
→ Follow [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)

**...add Google/Azure AD login**
→ Follow [EXTERNAL_PROVIDERS_GUIDE.md](EXTERNAL_PROVIDERS_GUIDE.md)

**...customize the login page**
→ Follow [MIGRATION_AND_UI_GUIDE.md](MIGRATION_AND_UI_GUIDE.md)

**...deploy to production**
→ See "Production Setup" in [SETUP_GUIDE.md](SETUP_GUIDE.md)

---

## ✅ What You Need to Know

### Core Concepts

1. **OAuth 2.0 / OpenID Connect**
   - TrustIdentity implements OAuth 2.0 and OIDC standards
   - Supports all 9 grant types
   - FAPI 1.0 & 2.0 compliant

2. **Clients, Resources, and Scopes**
   - **Clients**: Applications that request tokens
   - **Identity Resources**: User claims (openid, profile, email)
   - **API Scopes**: Permissions for APIs
   - **API Resources**: Protected APIs

3. **Database Contexts**
   - **ConfigurationDbContext**: Clients, resources, scopes (long-lived)
   - **PersistedGrantDbContext**: Tokens, codes, sessions (short-lived)

4. **Extensibility**
   - 12 extensibility interfaces
   - 100% compatible with Duende IdentityServer
   - Easy to customize

---

## 🎓 Learning Path

### Beginner
1. Read [README.md](README.md)
2. Follow [SETUP_GUIDE.md](SETUP_GUIDE.md) basic setup
3. Test with in-memory stores
4. Understand clients, resources, scopes

### Intermediate
1. Set up database with [DATABASE_SETUP.md](DATABASE_SETUP.md)
2. Configure production certificate
3. Add external providers
4. Customize UI

### Advanced
1. Implement custom `IProfileService`
2. Add custom event sinks
3. Configure multi-tenancy
4. Enable AI fraud detection
5. Deploy to production

---

## 📞 Support

- 📧 **Email**: support@trustidentity.dev
- 💬 **Discussions**: [GitHub Discussions](https://github.com/trustidentity/trustidentity/discussions)
- 🐛 **Issues**: [GitHub Issues](https://github.com/trustidentity/trustidentity/issues)
- 📖 **Documentation**: You're reading it!

---

## 🎉 You're Ready!

**All the documentation you need is here. Start with [README.md](README.md) and follow the guides.**

**TrustIdentity - Enterprise Identity & Access Management, Free & Open Source**
