# Database Migration & UI Customization Guide

This guide details how to move from In-Memory development to a persistent database (via Entity Framework Core) and how to customize the User Interface.

## 🗄️ Database Migration

TrustIdentity uses modular Entity Framework Core contexts to allow you to split data across different databases if needed.

### 1. The Contexts

| Context Name | Description |
| :--- | :--- |
| `ConfigurationDbContext` | Stores static configuration (Clients, Resources, Scopes). |
| `PersistedGrantDbContext` | Stores operational data (Authorization Codes, Refresh Tokens, Consents). |
| `TrustIdentityDbContext` | Stores User accounts and AI profiles. |
| `LicensingDbContext` | Stores license keys and entitlements. |

### 2. Creating Migrations

You must create migrations for each context. Ensure you have the `dotnet-ef` tool installed:
`dotnet tool install --global dotnet-ef`

Run the following commands from the root of the solution (project references point to `src/TrustIdentity.Storage`):

```bash
# Configuration Context
dotnet ef migrations add InitialConfiguration -c ConfigurationDbContext -p src/TrustIdentity.Storage -s src/TrustIdentity.Admin

# Operational Context
dotnet ef migrations add InitialOperational -c PersistedGrantDbContext -p src/TrustIdentity.Storage -s src/TrustIdentity.Admin

# User Context
dotnet ef migrations add InitialUsers -c TrustIdentityDbContext -p src/TrustIdentity.Storage -s src/TrustIdentity.Admin
```

### 3. Applying Migrations

At runtime, you can apply migrations automatically in `Program.cs` or generating a SQL script:

```bash
dotnet ef migrations script -c ConfigurationDbContext -p src/TrustIdentity.Storage -o ConfigScript.sql
```

### 4. Configuring Connection Strings

In your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ConfigurationDb": "Server=(localdb)\\mssqllocaldb;Database=TrustIdentity_Config;Trusted_Connection=True;",
    "OperationalDb": "Server=(localdb)\\mssqllocaldb;Database=TrustIdentity_Operational;Trusted_Connection=True;",
    "UsersDb": "Server=(localdb)\\mssqllocaldb;Database=TrustIdentity_Users;Trusted_Connection=True;"
  }
}
```

In `Program.cs`, replace in-memory stores with SQL Server:

```csharp
builder.Services.AddTrustIdentity()
    .AddConfigurationStore(options => 
        options.UseSqlServer(connectionString))
    .AddOperationalStore(options => 
        options.UseSqlServer(connectionString));
```

---

## 🎨 UI Customization

TrustIdentity comes with a default UI package (`TrustIdentity.UI`) that provides Login, Consent, Error, and Logout pages. This is functionally approximate to the "TrustIdentity Quickstart UI".

### 1. Using the Default UI

Add the package to your host project:
```bash
dotnet add package TrustIdentity.UI
```

Register it in `Program.cs`:
```csharp
builder.Services.AddTrustIdentityUI();
// ...
app.MapRazorPages();
```

### 2. Customizing Pages (Scaffolding)

To customize the look and feel (branding, layout), you can "scaffold" or copy the Razor Pages from the library into your project.

**Structure to Override:**
Create the following directory structure in your web project to override not built-in pages:

```text
/Pages
  /Account
    Login.cshtml      <-- Override Login Page
    Login.cshtml.cs   <-- Override Login Logic
    Logout.cshtml
  /Consent
    Index.cshtml      <-- Override Consent Screen
  /Shared
    _Layout.cshtml    <-- Change Master Layout (CSS/Logo)
```

### 3. ViewModel Reference

When overriding `Login.cshtml`, ensure you bind to the correct logical models:

```csharp
public class LoginModel : PageModel
{
    private readonly ITrustIdentityInteractionService _interaction;
    
    [BindProperty]
    public string Username { get; set; }
    
    [BindProperty]
    public string Password { get; set; }
    
    [BindProperty]
    public string ReturnUrl { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        // 1. Validate User
        // 2. SignInAsync
        // 3. Redirect to ReturnUrl
    }
}
```

### 4. Static Assets

The default UI uses Bootstrap 5. To change CSS:
1.  Create `wwwroot/css/site.css` in your project.
2.  Override `Pages/Shared/_Layout.cshtml` to link to your custom CSS.

### 5. AI Fraud Integration in UI

To display Fraud warnings on the login page:
1.  Inject `IFraudDetectionService`.
2.  In `OnPostAsync`, catch `FraudException`.
3.  Display `ModelState.AddModelError(string.Empty, "Suspicious activity detected.")`.
