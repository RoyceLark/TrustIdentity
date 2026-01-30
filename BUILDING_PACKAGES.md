# Building TrustIdentity NuGet Packages

This guide explains how to pack and publish the TrustIdentity libraries as NuGet packages for internal or public distribution.

## 📦 Package List

The following projects are designed to be packed:

1.  **TrustIdentity.Abstractions**: Core interfaces and models.
2.  **TrustIdentity.Core**: The main logic engine.
3.  **TrustIdentity.AspNetCore**: Integration layer for ASP.NET Core apps.
4.  **TrustIdentity.Storage**: Entity Framework Core implementation.
5.  **TrustIdentity.UI**: Razor Class Library for standard UI pages.
6.  **TrustIdentity.Bff**: Backend-for-Frontend security pattern.
7.  **TrustIdentity.Admin**: Admin API and Management UI.

## 🛠️ Build & Pack Commands

### 1. Versioning
Ensure the version in `Directory.Build.props` (or individual `.csproj` files) is correct before packing.
Current Version: **1.0.0**

### 2. Create Packages
Run the following command from the root of the solution to build all packable projects:

```bash
dotnet pack --configuration Release --output ./nupkgs
```

This will generate `.nupkg` files in the `nupkgs` directory.

### 3. Verify Content
You can inspect the generated packages using the NuGet Package Explorer or by unzipping them (they are zip files) to ensure all DLLs and dependencies are correct.

## 🚀 Publishing to NuGet

### Publish to NuGet.org
To publish to the public gallery (requires an API Key):

```bash
dotnet nuget push ./nupkgs/*.nupkg --api-key <YOUR_API_KEY> --source https://api.nuget.org/v3/index.json
```

### Publish to Private Feed (Azure Artifacts / MyGet)
To publish to a private feed:

```bash
dotnet nuget push ./nupkgs/*.nupkg --source <YOUR_FEED_URL> --api-key <YOUR_API_KEY>
```

## 🔄 CI/CD Pipeline Integration

For automated builds (e.g., GitHub Actions, Azure DevOps), add the following steps:

1.  **Restore:** `dotnet restore`
2.  **Build:** `dotnet build --configuration Release --no-restore`
3.  **Test:** `dotnet test --no-build --configuration Release`
4.  **Pack:** `dotnet pack --no-build --configuration Release --output ${{ env.DOTNET_ROOT }}/nupkgs`
5.  **Push:** `dotnet nuget push ...`

## ⚠️ Dependency Management

Projects have dependencies on each other. When releasing, ensure strictly versioned dependencies are maintained to avoid mismatches. Use the `Directory.Packages.props` (if using Central Package Management) to synchronize versions.
