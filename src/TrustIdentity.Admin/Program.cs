using Microsoft.EntityFrameworkCore;
using TrustIdentity.Core.Security;
using TrustIdentity.Storage.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=TrustIdentity.db";

// Register TrustIdentity Stores
builder.Services.AddTrustIdentityConfigurationStore(options =>
    options.UseSqlite(connectionString));

builder.Services.AddTrustIdentityOperationalStore(options =>
    options.UseSqlite(connectionString));

builder.Services.AddTrustIdentityUserStore(options =>
    options.UseSqlite(connectionString));

// Add Password Hasher for User Management
builder.Services.AddScoped<TrustIdentity.Abstractions.Stores.IPasswordHasher, PasswordHasher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
