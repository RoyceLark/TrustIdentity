using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TrustIdentity Admin API",
        Version = "v1",
        Description = "Administrative API for managing TrustIdentity server",
        Contact = new OpenApiContact
        {
            Name = "TrustIdentity",
            Email = "web.html123@gmail.com"
        }
    });

    // Add OAuth2 security
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            ClientCredentials = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    { "admin_api", "Admin API Access" }
                }
            }
        }
    });

    // Note: Security requirement commented out due to OpenApi 2.x compatibility
    // The security is still enforced via [Authorize] attributes on controllers
    /*
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "admin_api" }
        }
    });
    */
});

// Add authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Authority"];
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminApiAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "admin_api");
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdmin", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TrustIdentity Admin API v1");
        options.OAuthClientId("admin-api-swagger");
        options.OAuthAppName("Admin API Swagger");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAdmin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
