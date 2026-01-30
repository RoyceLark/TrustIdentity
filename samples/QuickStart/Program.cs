using TrustIdentity.AspNetCore.Extensions;
using TrustIdentity.Abstractions.Models;

var builder = WebApplication.CreateBuilder(args);

// Add TrustIdentity OAuth 2.0 & OpenID Connect
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
})
.AddInMemoryClients(new List<Client>
{
    new Client
    {
        ClientId = "demo-client",
        ClientSecrets = new List<Secret> { "secret".Sha256() },
        AllowedGrantTypes = new List<string> { "authorization_code" },
        RedirectUris = new List<string> { "https://localhost:5002/signin-oidc" },
        AllowedScopes = new List<string> { "openid", "profile", "email" }
    }
})
.AddInMemoryIdentityResources(new List<IdentityResource>
{
    new IdentityResource
    {
        Name = "openid",
        DisplayName = "OpenID",
        UserClaims = new List<string> { "sub" }
    },
    new IdentityResource
    {
        Name = "profile",
        DisplayName = "Profile",
        UserClaims = new List<string> { "name", "email" }
    }
})
.AddInMemoryApiScopes(new List<ApiScope>
{
    new ApiScope
    {
        Name = "api",
        DisplayName = "API Access"
    }
})
.AddTestUsers(new List<TestUser>
{
    new TestUser
    {
        SubjectId = "1",
        Username = "alice",
        Password = "password",
        Claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim("name", "Alice Smith"),
            new System.Security.Claims.Claim("email", "alice@example.com")
        }
    }
});

var app = builder.Build();

app.UseTrustIdentity();

app.MapGet("/", () => "TrustIdentity is running! Navigate to /.well-known/openid-configuration");

app.Run();
