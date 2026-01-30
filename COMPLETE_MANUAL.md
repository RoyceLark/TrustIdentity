# TrustIdentity Complete Manual

This document is the master index for all TrustIdentity documentation, setups, and workflows.

## 📦 Part 1: NuGet Distribution Strategy

To distribute TrustIdentity to your team or the public, you must publish the following **7 packages**. 
Use the guide `BUILDING_PACKAGES.md` for command-line specifics.

| Order | Project | Description | Dependency |
| :--- | :--- | :--- | :--- |
| 1 | `TrustIdentity.Abstractions` | Core Interfaces | Independent |
| 2 | `TrustIdentity.Core` | Core Logic | Depends on Abstractions |
| 3 | `TrustIdentity.Storage` | Data Access (EF Core) | Depends on Abstractions |
| 4 | `TrustIdentity.AspNetCore` | Web Integration | Depends on Core |
| 5 | `TrustIdentity.UI` | Razor Pages UI | Depends on AspNetCore |
| 6 | `TrustIdentity.Bff` | SPA Security | Depends on AspNetCore |
| 7 | `TrustIdentity.Admin` | Admin API | Depends on Storage |

**Note:** Always publish in this order if releasing manually, or use a single solution-level `dotnet pack` command.

## 🛠️ Part 2: Implementation & Setup

### 1. New Project Setup
Follow **`SETUP_GUIDE.md`** to install the packages into a blank ASP.NET Core Web App.
- **Key Step:** Register services in `Program.cs` using `.AddTrustIdentity()`.
- **Key Step:** Define your standard `Config.cs` with Clients and Resources.

### 2. Database Setup
For production, you must switch from In-Memory to SQL Server/PostgreSQL.
- Use `TrustIdentity.Storage`.
- Run EF Core Migrations: `dotnet ef migrations add InitialCreate`.

### 3. Key Management
- Configuring `KeyManagementService` in `Program.cs` is critical for production security.
- Refer to `SECURITY_GUIDE.md` (if available) or the `IKeyMaterialService` interface usage.

## 🧪 Part 3: Testing & Verification

You have three levels of testing available:

### 1. Automated Unit Tests
Run `dotnet test tests/TrustIdentity.UnitTests` to verify internal logic (Validators, Models, Crypto).

### 2. Integration Tests
Run `dotnet test tests/TrustIdentity.IntegrationTests` to verify HTTP endpoints (Discovery, Token, Authorize) work correctly in the ASP.NET pipeline.

### 3. Manual Feature Verification
Use **`FEATURE_VERIFICATION.md`** for specific `curl` commands to test:
- Pushed Authorization Requests (PAR)
- DPoP Proofs
- Dynamic Client Registration
- Token Exchange
- mTLS

## 📚 Documentation Index

- **README.md**: General Overview & Quick Start.
- **SETUP_GUIDE.md**: Detailed installation instructions.
- **BUILDING_PACKAGES.md**: How to create the artifacts.
- **TESTING_GUIDE.md**: How to run automated test suites.
- **FEATURE_VERIFICATION.md**: How to manually validate advanced features.
- **MIGRATION_AND_UI_GUIDE.md**: Database schemas and UI customization.
- **FAPI_COMPLIANCE.md**: Security configuration for Financial-grade APIs.
