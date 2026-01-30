using TrustIdentity.AspNetCore.Extensions;
using TrustIdentity.Storage.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Add TrustIdentity Core & Configuration
builder.Services.AddTrustIdentity(options =>
{
    builder.Configuration.GetSection("TrustIdentity").Bind(options);
})
// 2. Add Entity Framework Storage (SQLite by default)
.AddTrustIdentityStorage(options => 
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
})
// 3. Add AI/ML Security Features
.AddTrustIdentityAI()
// 4. Add ASP.NET Core UI & API Support
.AddTrustIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();

// Use TrustIdentity Middleware (Auth & Endpoints)
app.UseTrustIdentity();

app.MapRazorPages();

app.Run();
