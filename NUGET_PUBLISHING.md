# Royce Lark - TrustIdentity NuGet Publishing Guide

This document outlines the package structure, dependency chain, and publishing requirements for the **TrustIdentity** ecosystem by **Royce Lark**. All packages are locked to **v1.0.1** for the initial production-ready release.

## 📦 Package List (Publishing Order)

Follow this order when publishing to ensure all internal dependencies are available on the NuGet feed. All packages should be published with version `1.0.1`.

| Order | Package Name | Description | Base Dependencies |
| :--- | :--- | :--- | :--- |
| **1** | `TrustIdentity.Abstractions` | Core interfaces, models, and constants. | None |
| **2** | `TrustIdentity.Licensing` | License verification and enforcement logic. | Abstractions |
| **3** | `TrustIdentity.ML` | internal Machine Learning logic for fraud detection. | Abstractions |
| **4** | `TrustIdentity.Core` | Main authentication engine and token services. | Abstractions, Licensing |
| **5** | `TrustIdentity.AI` | AI-powered fraud detection and behavior analysis. | Core, ML |
| **6** | `TrustIdentity.Storage` | Entity Framework and distributed caching implementations. | Core |
| **7** | `TrustIdentity.AspNetCore` | Middleware, tag helpers, and ASP.NET Core integration. | Core |
| **8** | `TrustIdentity.Saml` | SAML 2.0 protocol support. | Core |
| **9** | `TrustIdentity.WsFederation` | WS-Federation protocol support. | Core |
| **10** | `TrustIdentity.ExternalProviders` | Pre-configured support for OIDC/OAuth social brokers. | Core |
| **11** | `TrustIdentity.Bff` | Backend-for-Frontend patterns and security. | Core |

---

## 🎨 Branding & Assets

All packages are configured to include the **TrustIdentity Signature Icon**.

- **Icon File**: `icon.png` (256x256, Green Shield + Tech Box)
- **License**: MIT
- **Project URL**: [https://github.com/roycelark/TrustIdentity](https://github.com/roycelark/TrustIdentity)

## 🚀 Publishing Checklist

1. **Version Update**: Update `<Version>` in `Directory.Build.props` or individual csprj.
2. **Build**: Run `dotnet build -c Release`
3. **Pack**: Run `dotnet pack -c Release -o ./artifacts`
4. **Push**:
   ```powershell
   dotnet nuget push ./artifacts/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
   ```

---

## 🏗️ Technical Support
For issues with package distribution, contact the Core DevOps team.
