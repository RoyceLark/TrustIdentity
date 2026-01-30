# NuGet Package Publishing Guide

## Overview

This guide covers building, testing, and publishing TrustIdentity NuGet packages.

## Prerequisites

- .NET 9.0 SDK or .NET 10.0 SDK
- NuGet account at nuget.org
- API key from nuget.org

## Package Information

### TrustIdentity.Abstractions
**Package ID**: `TrustIdentity.Abstractions`  
**Description**: Core abstractions and interfaces for TrustIdentity  
**Dependencies**: Microsoft.Extensions.DependencyInjection.Abstractions

### TrustIdentity.Core
**Package ID**: `TrustIdentity.Core`  
**Description**: Core models, services, and business logic  
**Dependencies**: 
- TrustIdentity.Abstractions
- Microsoft.IdentityModel.Tokens
- System.IdentityModel.Tokens.Jwt

### TrustIdentity.Storage
**Package ID**: `TrustIdentity.Storage`  
**Description**: Entity Framework Core storage providers  
**Dependencies**:
- TrustIdentity.Core
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Relational

### TrustIdentity.AspNetCore
**Package ID**: `TrustIdentity.AspNetCore`  
**Description**: ASP.NET Core integration and middleware  
**Dependencies**:
- TrustIdentity.Core
- TrustIdentity.Storage
- Microsoft.AspNetCore.App (framework reference)

### TrustIdentity.AI
**Package ID**: `TrustIdentity.AI`  
**Description**: AI-powered fraud detection and analysis  
**Dependencies**:
- TrustIdentity.Core
- Microsoft.ML

### TrustIdentity.ML
**Package ID**: `TrustIdentity.ML`  
**Description**: Machine learning models and training  
**Dependencies**:
- TrustIdentity.Core
- TrustIdentity.AI
- Microsoft.ML
- Microsoft.ML.FastTree

### TrustIdentity.Server
**Package ID**: `TrustIdentity.Server`  
**Description**: Complete server package (meta-package)  
**Dependencies**: All of the above

## Building Packages

### 1. Clean Build

```bash
# Clean previous builds
dotnet clean TrustIdentity.sln

# Restore dependencies
dotnet restore TrustIdentity.sln
```

### 2. Build Solution

```bash
# Build in Release mode
dotnet build TrustIdentity.sln --configuration Release
```

### 3. Run Tests

```bash
# Run all tests
dotnet test TrustIdentity.sln --configuration Release --no-build
```

### 4. Create NuGet Packages

```bash
# Use the build script
chmod +x build.sh
./build.sh
```

Or manually for each project:

```bash
dotnet pack src/TrustIdentity.Abstractions/TrustIdentity.Abstractions.csproj \
    --configuration Release \
    --output ./artifacts/packages \
    /p:Version=1.0.0

dotnet pack src/TrustIdentity.Core/TrustIdentity.Core.csproj \
    --configuration Release \
    --output ./artifacts/packages \
    /p:Version=1.0.0

# Repeat for other projects...
```

## Package Contents

Each package should include:
- Compiled assemblies (.dll)
- XML documentation files
- Symbol packages (.snupkg)
- README.md
- LICENSE

## Package Metadata

All packages include:
- **Authors**: TrustIdentity Contributors
- **License**: Apache-2.0
- **Project URL**: https://github.com/trustidentity/trustidentity
- **Repository URL**: https://github.com/trustidentity/trustidentity
- **Tags**: oauth2, openid, oidc, security, identity, authentication, authorization, ai, ml
- **Icon**: icon.png (128x128 minimum)

## Publishing to NuGet

### 1. Get API Key

1. Sign in to nuget.org
2. Go to Account Settings
3. Create new API key with "Push" scope

### 2. Push Packages

```bash
# Push all packages
dotnet nuget push ./artifacts/packages/*.nupkg \
    --source https://api.nuget.org/v3/index.json \
    --api-key YOUR_API_KEY_HERE
```

### 3. Push Symbol Packages

```bash
# Push symbol packages for debugging support
dotnet nuget push ./artifacts/packages/*.snupkg \
    --source https://api.nuget.org/v3/index.json \
    --api-key YOUR_API_KEY_HERE
```

## Testing Packages Locally

### 1. Add Local Source

```bash
# Add local package source
dotnet nuget add source ./artifacts/packages --name TrustIdentity.Local
```

### 2. Create Test Project

```bash
dotnet new console -n TestPackage
cd TestPackage
dotnet add package TrustIdentity.Server --version 1.0.0 --source TrustIdentity.Local
```

### 3. Verify Installation

```csharp
using TrustIdentity.Core.Models;

var client = new Client
{
    ClientId = "test",
    ClientName = "Test Client"
};

Console.WriteLine($"Client: {client.ClientName}");
```

## Versioning

We follow Semantic Versioning (SemVer):
- **Major**: Breaking changes
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes, backward compatible

### Pre-release Versions

For preview releases:
```bash
dotnet pack --configuration Release /p:Version=1.0.0-preview.1
```

For release candidates:
```bash
dotnet pack --configuration Release /p:Version=1.0.0-rc.1
```

## Package Signing

### 1. Get Code Signing Certificate

Purchase or obtain a code signing certificate from a trusted CA.

### 2. Sign Packages

```bash
nuget sign ./artifacts/packages/*.nupkg \
    -CertificatePath certificate.pfx \
    -CertificatePassword password \
    -Timestamper http://timestamp.digicert.com
```

## Package Validation

Before publishing, validate packages:

```bash
# Install validation tool
dotnet tool install -g dotnet-validate

# Validate package
dotnet-validate package ./artifacts/packages/TrustIdentity.Core.1.0.0.nupkg
```

## Continuous Integration

### GitHub Actions Example

```yaml
name: Publish NuGet Packages

on:
  release:
    types: [created]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Pack
      run: dotnet pack --configuration Release --no-build --output ./packages
    
    - name: Push to NuGet
      run: dotnet nuget push ./packages/*.nupkg --api-key ${{secrets.NUGET_API_KEY}} --source https://api.nuget.org/v3/index.json
```

## Package Icons

Create a 128x128 PNG icon and include in each project:

```xml
<PropertyGroup>
  <PackageIcon>icon.png</PackageIcon>
</PropertyGroup>

<ItemGroup>
  <None Include="..\..\icon.png" Pack="true" PackagePath="\" />
</ItemGroup>
```

## Release Checklist

- [ ] Update version numbers in all .csproj files
- [ ] Update CHANGELOG.md
- [ ] Update README.md with new features
- [ ] Run all tests
- [ ] Build in Release mode
- [ ] Create packages
- [ ] Validate packages locally
- [ ] Sign packages
- [ ] Push to NuGet
- [ ] Create GitHub release
- [ ] Update documentation site
- [ ] Announce release

## Troubleshooting

### Package Already Exists
NuGet packages are immutable. Increment version number and republish.

### Missing Dependencies
Ensure all dependencies are properly referenced in .csproj files.

### Symbol Upload Fails
Verify .snupkg format and ensure PDB files are included.

### Package Won't Install
Check target framework compatibility and dependency versions.

## Support

For issues or questions:
- GitHub Issues: https://github.com/trustidentity/trustidentity/issues
- Documentation: https://docs.trustidentity.dev
- Email: support@trustidentity.dev
