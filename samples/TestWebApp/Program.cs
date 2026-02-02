using TrustIdentity.AspNetCore.Extensions;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Storage.Extensions;
using TrustIdentity.Storage.EntityFramework;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add TrustIdentity
// Add TrustIdentity
var identityBuilder = builder.Services.AddTrustIdentity(options =>
{
    builder.Configuration.GetSection("TrustIdentity").Bind(options);
});

// Security: Add Rate Limiting and other security services
builder.Services.AddTrustIdentitySecurity();

// ==============================================================================
// DATABASE SUPPORT (Production Ready)
// ==============================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Use SQLite for this sample (Production can use SqlServer, Postgres, etc.)
builder.Services.AddTrustIdentityConfigurationStore(options => 
    options.UseSqlite(connectionString));

builder.Services.AddTrustIdentityOperationalStore(options => 
    options.UseSqlite(connectionString));

builder.Services.AddTrustIdentityUserStore(options => 
    options.UseSqlite(connectionString));

// ==============================================================================
// SIGNING CREDENTIALS
// ==============================================================================
// In production, load from a secure location (Azure KeyVault, Certificate Store)
// Here we look for a local PFX, otherwise fallback to developer credential
var certPath = "signing_key.pfx";
if (File.Exists(certPath))
{
    var cert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(certPath, "password"); // Use secure password config
    identityBuilder.AddSigningCredential(cert);
}
else
{
    // WARN: Only for Dev/First run
    identityBuilder.AddDeveloperSigningCredential();
}

var app = builder.Build();

// ==============================================================================
// AUTOMATIC MIGRATION & SEEDING
// ==============================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        // Migrate Databases
        var configContext = services.GetRequiredService<ConfigurationDbContext>();
        configContext.Database.Migrate();

        var grantContext = services.GetRequiredService<PersistedGrantDbContext>();
        grantContext.Database.Migrate();

        var userContext = services.GetRequiredService<TrustIdentityDbContext>();
        userContext.Database.Migrate();

        // Seed Data if Empty
        if (!configContext.Clients.Any())
        {
            var seedClients = new List<Client>
            {
                new Client
                {
                    ClientId = "web-client",
                    ClientName = "Web Application",
                    ClientSecrets = new List<Secret> { "secret".Sha256() },
                    AllowedGrantTypes = new List<string> { "authorization_code", "password", "urn:ietf:params:oauth:grant-type:device_code" },
                    RedirectUris = new List<string> { "https://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = new List<string> { "https://localhost:5002/signout-callback-oidc" },
                    AllowedScopes = new List<string> { "openid", "profile", "email", "api" },
                    RequireClientSecret = false // For demo ease
                },
                new Client
                {
                    ClientId = "api-client",
                    ClientName = "API Client",
                    ClientSecrets = new List<Secret> { "secret".Sha256() },
                    AllowedGrantTypes = new List<string> { "client_credentials" },
                    AllowedScopes = new List<string> { "api" }
                }
            };
            configContext.Clients.AddRange(seedClients);
            configContext.SaveChanges();
        }

        if (!configContext.IdentityResources.Any())
        {
            var seedResources = new List<IdentityResource>
            {
                new IdentityResource { Name = "openid", DisplayName = "Your user identifier", UserClaims = new List<string> { "sub" }, Required = true },
                new IdentityResource { Name = "profile", DisplayName = "User profile", UserClaims = new List<string> { "name", "family_name", "given_name", "email" } },
                new IdentityResource { Name = "email", DisplayName = "Your email address", UserClaims = new List<string> { "email", "email_verified" } }
            };
            configContext.IdentityResources.AddRange(seedResources);
            configContext.SaveChanges();
        }

        if (!configContext.ApiScopes.Any())
        {
            configContext.ApiScopes.Add(new ApiScope { Name = "api", DisplayName = "API Access", UserClaims = new List<string> { "name", "email" } });
            configContext.SaveChanges();
        }
        
        if (!configContext.ApiResources.Any())
        {
            configContext.ApiResources.Add(new ApiResource { Name = "api1", DisplayName = "My API", Scopes = new List<string> { "api" } });
            configContext.SaveChanges();
        }

        if (!userContext.Users.Any())
        {
            var hasher = services.GetRequiredService<TrustIdentity.Abstractions.Stores.IPasswordHasher>();
            var seedUsers = new List<TrustIdentity.Core.Models.TrustIdentityUser>
            {
                new TrustIdentity.Core.Models.TrustIdentityUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = hasher.HashPassword(new User(), "password"), // Properly hash password
                    Email = "alice@example.com",
                    IsActive = true,
                    Claims = new List<TrustIdentity.Core.Models.UserClaim>
                    {
                        new TrustIdentity.Core.Models.UserClaim { Type = "name", Value = "Alice Smith" },
                        new TrustIdentity.Core.Models.UserClaim { Type = "email", Value = "alice@example.com" },
                        new TrustIdentity.Core.Models.UserClaim { Type = "email_verified", Value = "true" }
                    }
                },
                new TrustIdentity.Core.Models.TrustIdentityUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = hasher.HashPassword(new User(), "password"), // Properly hash password
                    Email = "bob@example.com",
                    IsActive = true,
                    Claims = new List<TrustIdentity.Core.Models.UserClaim>
                    {
                        new TrustIdentity.Core.Models.UserClaim { Type = "name", Value = "Bob Jones" },
                        new TrustIdentity.Core.Models.UserClaim { Type = "email", Value = "bob@example.com" },
                        new TrustIdentity.Core.Models.UserClaim { Type = "email_verified", Value = "true" }
                    }
                }
            };
            userContext.Users.AddRange(seedUsers);
            userContext.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// ==============================================================================
// SECURITY MIDDLEWARE (Order matters!)
// ==============================================================================

// 1. DDoS Protection (Block malicious traffic FIRST)
app.UseTrustIdentityDDoSProtection(new TrustIdentity.AspNetCore.Middleware.DDoSProtectionOptions
{
    Enabled = true,
    MaxRequestsPerSecond = 10.0,
    MaxRequestSize = 10 * 1024 * 1024, // 10 MB
    BlockThreshold = 20,
    BlockDuration = TimeSpan.FromMinutes(15)
});

// 2. Rate Limiting (Control legitimate traffic)
var rateLimitOptions = new TrustIdentity.AspNetCore.Middleware.RateLimitingOptions();
builder.Configuration.GetSection("TrustIdentity:RateLimiting").Bind(rateLimitOptions);
app.UseTrustIdentityRateLimiting(rateLimitOptions);

// 3. Security Headers
app.UseTrustIdentitySecurityHeaders();

// 4. Development Exception Page
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// 5. HTTPS Redirection
app.UseHttpsRedirection();

// 6. Static Files
app.UseStaticFiles();

// 7. Routing
app.UseRouting();

// 8. TrustIdentity Endpoints
app.UseTrustIdentity();

app.MapGet("/diagnostics", () => Results.Text(@"<!DOCTYPE html><html><head><title>Claims</title></head><body><h1>User Claims</h1><p>No active session found.</p></body></html>", "text/html"));
app.MapGet("/grants", () => Results.Text(@"<!DOCTYPE html><html><head><title>Grants</title></head><body><h1>Stored Grants</h1><p>No grants found.</p></body></html>", "text/html"));
app.MapGet("/sessions", () => Results.Text(@"<!DOCTYPE html><html><head><title>Sessions</title></head><body><h1>Server Side Sessions</h1><p>No active sessions.</p></body></html>", "text/html"));
app.MapGet("/ciba", () => Results.Text(@"<!DOCTYPE html><html><head><title>CIBA</title></head><body><h1>Pending CIBA Requests</h1><p>No pending requests.</p></body></html>", "text/html"));

app.MapGet("/", () => Results.Text(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>TrustIdentity Server</title>
    <link rel='shortcut icon' href='/favicon.ico' />
    <style>
        body { font-family: 'Segoe UI', 'Helvetica Neue', Arial, sans-serif; margin: 0; padding: 0; color: #333; line-height: 1.5; }
        .header { background-color: #fff; padding: 20px 40px; border-bottom: 1px solid #e5e5e5; display: flex; align-items: center; }
        .logo-icon { width: 40px; height: 40px; background-color: #6963FF; border-radius: 5px; margin-right: 15px; position: relative; }
        .logo-icon::before { content: ''; position: absolute; top: 10px; left: 10px; width: 20px; height: 2px; background: white; box-shadow: 0 8px 0 white, 0 16px 0 white; }
        .logo-text { font-size: 24px; font-weight: 600; color: #333; }
        .content { max-width: 1000px; margin: 40px auto; padding: 0 40px; }
        h1 { font-weight: 300; font-size: 42px; margin-bottom: 30px; margin-top: 0; }
        h1 .version { font-size: 24px; color: #888; margin-left: 10px; }
        ul { list-style-type: none; padding-left: 0; margin-top: 20px; }
        li { margin-bottom: 15px; background: url('data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI4IiBoZWlnaHQ9IjgiIHZpZXdCb3g9IjAgMCA4IDgiPjxjaXJjbGUgY3g9IjQiIGN5PSI0IiByPSI0IiBmaWxsPSIjMzMzIi8+PC9zdmc+') no-repeat 0 9px; padding-left: 20px; }
        a { color: #007bff; text-decoration: none; }
        a:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <div class='header'>
        <div class='logo-icon'></div>
        <div class='logo-text'>TrustIdentity</div>
    </div>
    
    <div class='content'>
        <h1>Welcome to TrustIdentity Server <span class='version'>(version 1.0.0)</span></h1>
        
        <ul>
            <li>TrustIdentity publishes a <a href='/.well-known/openid-configuration'>discovery document</a> where you can find metadata and links to all the endpoints, key material, etc.</li>
            <li>Click <a href='/diagnostics'>here</a> to see the claims for your current session.</li>
            <li>Click <a href='/grants'>here</a> to manage your stored grants.</li>
            <li>Click <a href='/sessions'>here</a> to view the server side sessions.</li>
            <li>Click <a href='/ciba'>here</a> to view your pending CIBA login requests.</li>
            <li>Here are links to the <a href='https://github.com/'>source code repository</a>, and <a href='https://github.com/'>ready to use samples</a>.</li>
        </ul>
    </div>
</body>
</html>
", "text/html"));

app.Run();

public partial class Program { }
