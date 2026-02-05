using Microsoft.EntityFrameworkCore;
using TrustIdentity.AdminApi.Extensions;
using TrustIdentity.Storage.Extensions;
using TrustIdentity.Core.Security;
using TrustIdentity.Abstractions.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add TrustIdentity Admin API services (Controllers and Policies)
builder.Services.AddTrustIdentityAdminApi();

// Register Storage Providers
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=TrustIdentity.db";

builder.Services.AddTrustIdentityConfigurationStore(options =>
    options.UseSqlite(connectionString));

builder.Services.AddTrustIdentityOperationalStore(options =>
    options.UseSqlite(connectionString));

builder.Services.AddTrustIdentityUserStore(options =>
    options.UseSqlite(connectionString));

// Add required security services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrustIdentity Admin API v1"));
}

app.UseHttpsRedirection();

// Use authentication and authorization if configured
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
