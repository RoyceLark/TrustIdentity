# TrustIdentity Setup & User Guide

Welcome to TrustIdentity! This guide will help you set up and integrate the TrustIdentity Server into your .NET applications.

## 📋 Prerequisites

- **.NET 10.0 SDK** (or later) installed on your machine.
- A code editor (VS Code, Visual Studio 2026, or Rider).
- (Optional) Docker for containerized deployment.

## 🚀 Quick Start (Run the Sample)

The fastest way to see TrustIdentity in action is to run the QuickStart sample.

1.  **Clone the repository** (if you haven't already).
2.  **Navigate to the QuickStart folder:**
    ```bash
    cd samples/QuickStart
    ```
3.  **Run the project:**
    ```bash
    dotnet run
    ```
4.  **Open your browser** to `https://localhost:5001`.
    - You will see the TrustIdentity Welcome Page.
    - Click "Login" to test the authentication flow.
    - **Default Credentials:**
        - Username: `alice`
        - Password: `Password123!`

## 🛠️ Integration Guide (For New Projects)

To add TrustIdentity to your own ASP.NET Core project:

### 1. Install the NuGet Packages
Add the necessary packages to your web project:
```bash
dotnet add package TrustIdentity.AspNetCore
dotnet add package TrustIdentity.UI
dotnet add package TrustIdentity.Storage
```

### 2. Configure `Program.cs`

Add the TrustIdentity services and middleware to your application startup:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Add TrustIdentity Services
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
    
    // Enable features as needed
    options.EnableFraudDetection = true;
    options.EnableLicensing = false;
})
// Using In-Memory stores for development (Swap with EF Core for prod)
.AddInMemoryClients(Config.Clients)
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryApiResources(Config.ApiResources)
.AddTestUsers(Config.Users)
.AddDeveloperSigningCredential(); // DO NOT usage in production!

// Add UI pages (Login, Consent, etc.)
builder.Services.AddTrustIdentityUI();

var app = builder.Build();

// 2. Add TrustIdentity Middleware
app.UseTrustIdentity();

// 3. Map UI Endpoints
app.MapRazorPages();

app.Run();
```

### 3. Define Configuration Resources (`Config.cs`)

You need to define your Clients, Scopes, and Users. Create a `Config.cs` file:

```csharp
public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        { 
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[] { new ApiScope("api1", "My API") };

    public static IEnumerable<Client> Clients =>
        new Client[] 
        {
            new Client
            {
                ClientId = "client",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "api1" }
            },
            new Client
            {
                ClientId = "web_app",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "https://localhost:5002/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                AllowedScopes = { "openid", "profile", "email", "api1" }
            }
        };
        
    public static List<TestUser> Users => new List<TestUser>
    {
        new TestUser { SubjectId = "1", Username = "alice", Password = "Password123!" }
    };
}
```

## 🔒 Production Deployment Checklist

Before going to production, ensure you follow these steps:

1.  **Switch to Persistent Storage:**
    - Replace `.AddInMemory...` with `.AddConfigurationStore()` and `.AddOperationalStore()`.
    - Use `TrustIdentity.Storage` with Entity Framework Core (SQL Server, PostgreSQL, etc.).

2.  **Use Real Signing Keys:**
    - **Remove** `.AddDeveloperSigningCredential()`.
    - **Use** `.AddSigningCredential(cert)` with a valid X.509 certificate loaded from Azure KeyVault or a secure store.
    - Alternatively, enable **Automatic Key Rotation** service.

3.  **Enable Production Security Settings:**
    - Ensure HTTPS is enforced.
    - Set `AccumulatedRiskScore` thresholds in `FraudDetectionService`.
    - Configure robust CORS policies (do not allow `*`).

4.  **Logging & Monitoring:**
    - Integrate with Serilog or Application Insights.
    - **Warning:** Ensure PII (Personally Identifiable Information) is not logged in production.

## 📚 Advanced Features

### Dynamic Client Registration
Endpoint: `POST /connect/register`
Body: `{"client_name": "My App", "redirect_uris": ["https://app.com/cb"]}`

### Token Exchange
Grant Type: `urn:ietf:params:oauth:grant-type:token-exchange`
Support for impersonation and delegation flows.

### AI Fraud Detection
Automatically analyzes request patterns (IP velocity, user agent, behavior) to block suspicious login attempts.

---

**Need Help?**
Check the `docs/` folder for detailed architectural references or run the `TrustIdentity.IntegrationTests` project to see examples of valid requests.
