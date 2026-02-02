# Database Setup Guide - TrustIdentity

**Complete guide for setting up and configuring databases with TrustIdentity**

---

## 📋 Overview

TrustIdentity supports multiple databases through Entity Framework Core:
- ✅ SQL Server
- ✅ PostgreSQL
- ✅ MySQL
- ✅ SQLite
- ✅ In-Memory (Development only)

---

## 🚀 Quick Start

### 1. Install Required Packages

```bash
# For SQL Server
dotnet add package TrustIdentity.Storage
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# For PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# For MySQL
dotnet add package Pomelo.EntityFrameworkCore.MySql

# For SQLite
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

### 2. Configure Database

```csharp
using TrustIdentity.AspNetCore.Extensions;
using TrustIdentity.Storage.EntityFramework.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
})
// Configuration Store (Clients, Resources, Scopes)
.AddConfigurationStore(options =>
    options.UseSqlServer(connectionString))
// Operational Store (Tokens, Codes, Consents)
.AddOperationalStore(options =>
    options.UseSqlServer(connectionString))
.AddSigningCredential(certificate);
```

### 3. Create Migrations

```bash
# Configuration Store
dotnet ef migrations add InitialConfigurationDb -c ConfigurationDbContext -o Data/Migrations/Configuration

# Operational Store
dotnet ef migrations add InitialPersistedGrantDb -c PersistedGrantDbContext -o Data/Migrations/PersistedGrant
```

### 4. Update Database

```bash
# Apply migrations
dotnet ef database update -c ConfigurationDbContext
dotnet ef database update -c PersistedGrantDbContext
```

**Done!** Your database is ready.

---

## 🗄️ Database Contexts

TrustIdentity uses two separate database contexts:

### 1. ConfigurationDbContext
Stores configuration data (long-lived):
- **Clients** - OAuth/OIDC client applications
- **IdentityResources** - OpenID Connect scopes (openid, profile, email)
- **ApiScopes** - OAuth 2.0 scopes
- **ApiResources** - Protected APIs

### 2. PersistedGrantDbContext
Stores operational data (short-lived):
- **PersistedGrants** - Authorization codes, refresh tokens, reference tokens
- **DeviceFlowCodes** - Device flow codes
- **Keys** - Signing keys
- **ServerSideSessions** - Server-side sessions

---

## 📊 Database Schemas

### Configuration Tables

```sql
-- Clients table
CREATE TABLE Clients (
    Id INT PRIMARY KEY IDENTITY,
    ClientId NVARCHAR(200) NOT NULL,
    ClientName NVARCHAR(200),
    Description NVARCHAR(1000),
    Enabled BIT NOT NULL,
    RequireClientSecret BIT NOT NULL,
    RequirePkce BIT NOT NULL,
    AllowPlainTextPkce BIT NOT NULL,
    -- ... more columns
);

-- ClientScopes table
CREATE TABLE ClientScopes (
    Id INT PRIMARY KEY IDENTITY,
    ClientId INT NOT NULL,
    Scope NVARCHAR(200) NOT NULL,
    FOREIGN KEY (ClientId) REFERENCES Clients(Id)
);

-- ClientRedirectUris table
CREATE TABLE ClientRedirectUris (
    Id INT PRIMARY KEY IDENTITY,
    ClientId INT NOT NULL,
    RedirectUri NVARCHAR(2000) NOT NULL,
    FOREIGN KEY (ClientId) REFERENCES Clients(Id)
);

-- IdentityResources table
CREATE TABLE IdentityResources (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    DisplayName NVARCHAR(200),
    Description NVARCHAR(1000),
    Enabled BIT NOT NULL,
    Required BIT NOT NULL,
    Emphasize BIT NOT NULL,
    ShowInDiscoveryDocument BIT NOT NULL
);

-- ApiScopes table
CREATE TABLE ApiScopes (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    DisplayName NVARCHAR(200),
    Description NVARCHAR(1000),
    Enabled BIT NOT NULL,
    Required BIT NOT NULL,
    Emphasize BIT NOT NULL,
    ShowInDiscoveryDocument BIT NOT NULL
);

-- ApiResources table
CREATE TABLE ApiResources (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    DisplayName NVARCHAR(200),
    Description NVARCHAR(1000),
    Enabled BIT NOT NULL,
    AllowedAccessTokenSigningAlgorithms NVARCHAR(100)
);
```

### Operational Tables

```sql
-- PersistedGrants table
CREATE TABLE PersistedGrants (
    Key NVARCHAR(200) PRIMARY KEY,
    Type NVARCHAR(50) NOT NULL,
    SubjectId NVARCHAR(200),
    SessionId NVARCHAR(100),
    ClientId NVARCHAR(200) NOT NULL,
    Description NVARCHAR(200),
    CreationTime DATETIME2 NOT NULL,
    Expiration DATETIME2,
    ConsumedTime DATETIME2,
    Data NVARCHAR(MAX) NOT NULL
);

-- DeviceFlowCodes table
CREATE TABLE DeviceFlowCodes (
    UserCode NVARCHAR(200) PRIMARY KEY,
    DeviceCode NVARCHAR(200) NOT NULL,
    SubjectId NVARCHAR(200),
    SessionId NVARCHAR(100),
    ClientId NVARCHAR(200) NOT NULL,
    Description NVARCHAR(200),
    CreationTime DATETIME2 NOT NULL,
    Expiration DATETIME2 NOT NULL,
    Data NVARCHAR(MAX) NOT NULL
);

-- Keys table
CREATE TABLE Keys (
    Id NVARCHAR(450) PRIMARY KEY,
    Version INT NOT NULL,
    Created DATETIME2 NOT NULL,
    Use NVARCHAR(450),
    Algorithm NVARCHAR(100) NOT NULL,
    IsX509Certificate BIT NOT NULL,
    DataProtected BIT NOT NULL,
    Data NVARCHAR(MAX) NOT NULL
);

-- ServerSideSessions table
CREATE TABLE ServerSideSessions (
    Id INT PRIMARY KEY IDENTITY,
    Key NVARCHAR(100) NOT NULL,
    Scheme NVARCHAR(100) NOT NULL,
    SubjectId NVARCHAR(100) NOT NULL,
    SessionId NVARCHAR(100),
    DisplayName NVARCHAR(100),
    Created DATETIME2 NOT NULL,
    Renewed DATETIME2 NOT NULL,
    Expires DATETIME2,
    Data NVARCHAR(MAX) NOT NULL
);
```

---

## 🔧 Database-Specific Configuration

### SQL Server

```csharp
var connectionString = "Server=localhost;Database=TrustIdentity;User Id=sa;Password=YourPassword;TrustServerCertificate=True";

builder.Services.AddTrustIdentity(options => { ... })
    .AddConfigurationStore(options =>
        options.UseSqlServer(connectionString, sql => 
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)))
    .AddOperationalStore(options =>
        options.UseSqlServer(connectionString, sql => 
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)));
```

**Connection String in appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TrustIdentity;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
  }
}
```

### PostgreSQL

```csharp
var connectionString = "Host=localhost;Database=trustidentity;Username=postgres;Password=YourPassword";

builder.Services.AddTrustIdentity(options => { ... })
    .AddConfigurationStore(options =>
        options.UseNpgsql(connectionString, sql => 
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)))
    .AddOperationalStore(options =>
        options.UseNpgsql(connectionString, sql => 
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)));
```

**Connection String in appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=trustidentity;Username=postgres;Password=YourPassword"
  }
}
```

### MySQL

```csharp
var connectionString = "Server=localhost;Database=trustidentity;User=root;Password=YourPassword";
var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

builder.Services.AddTrustIdentity(options => { ... })
    .AddConfigurationStore(options =>
        options.UseMySql(connectionString, serverVersion, sql => 
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)))
    .AddOperationalStore(options =>
        options.UseMySql(connectionString, serverVersion, sql => 
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)));
```

**Connection String in appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=trustidentity;User=root;Password=YourPassword"
  }
}
```

### SQLite

```csharp
var connectionString = "Data Source=trustidentity.db";

builder.Services.AddTrustIdentity(options => { ... })
    .AddConfigurationStore(options =>
        options.UseSqlite(connectionString, sql => 
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)))
    .AddOperationalStore(options =>
        options.UseSqlite(connectionString, sql => 
            sql.MigrationsAssembly(typeof(Program).Assembly.FullName)));
```

**Connection String in appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=trustidentity.db"
  }
}
```

---

## 🔄 Migration Commands

### Create Migrations

```bash
# Configuration Store
dotnet ef migrations add InitialConfigurationDb \
    -c ConfigurationDbContext \
    -o Data/Migrations/Configuration

# Operational Store
dotnet ef migrations add InitialPersistedGrantDb \
    -c PersistedGrantDbContext \
    -o Data/Migrations/PersistedGrant
```

### Update Database

```bash
# Apply all migrations
dotnet ef database update -c ConfigurationDbContext
dotnet ef database update -c PersistedGrantDbContext
```

### Remove Last Migration

```bash
dotnet ef migrations remove -c ConfigurationDbContext
dotnet ef migrations remove -c PersistedGrantDbContext
```

### Generate SQL Script

```bash
# Generate SQL script instead of applying directly
dotnet ef migrations script -c ConfigurationDbContext -o config.sql
dotnet ef migrations script -c PersistedGrantDbContext -o operational.sql
```

---

## 📝 Seeding Initial Data

### Seed Configuration Data

```csharp
public static class DatabaseInitializer
{
    public static void InitializeDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var configContext = serviceScope.ServiceProvider
            .GetRequiredService<ConfigurationDbContext>();
        
        // Apply migrations
        configContext.Database.Migrate();

        // Seed clients
        if (!configContext.Clients.Any())
        {
            foreach (var client in Config.GetClients())
            {
                configContext.Clients.Add(client.ToEntity());
            }
            configContext.SaveChanges();
        }

        // Seed identity resources
        if (!configContext.IdentityResources.Any())
        {
            foreach (var resource in Config.GetIdentityResources())
            {
                configContext.IdentityResources.Add(resource.ToEntity());
            }
            configContext.SaveChanges();
        }

        // Seed API scopes
        if (!configContext.ApiScopes.Any())
        {
            foreach (var scope in Config.GetApiScopes())
            {
                configContext.ApiScopes.Add(scope.ToEntity());
            }
            configContext.SaveChanges();
        }

        // Seed API resources
        if (!configContext.ApiResources.Any())
        {
            foreach (var resource in Config.GetApiResources())
            {
                configContext.ApiResources.Add(resource.ToEntity());
            }
            configContext.SaveChanges();
        }
    }
}

// In Program.cs
var app = builder.Build();
DatabaseInitializer.InitializeDatabase(app);
app.Run();
```

### Configuration Class

```csharp
public static class Config
{
    public static IEnumerable<Client> GetClients()
    {
        return new List<Client>
        {
            new Client
            {
                ClientId = "web-app",
                ClientName = "Web Application",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret("secret".Sha256()) },
                RedirectUris = { "https://localhost:5002/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                AllowedScopes = { "openid", "profile", "email", "api1" },
                RequirePkce = true,
                AllowOfflineAccess = true
            }
        };
    }

    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope("api1", "My API")
        };
    }

    public static IEnumerable<ApiResource> GetApiResources()
    {
        return new List<ApiResource>
        {
            new ApiResource("api1", "My API")
            {
                Scopes = { "api1" }
            }
        };
    }
}
```

---

## 🔒 Production Considerations

### 1. Connection String Security

**Don't hardcode connection strings!** Use:

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TrustIdentity;..."
  }
}

// Or use User Secrets (Development)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."

// Or use Environment Variables (Production)
export ConnectionStrings__DefaultConnection="Server=..."
```

### 2. Connection Pooling

```csharp
// SQL Server with connection pooling
"Server=localhost;Database=TrustIdentity;User Id=sa;Password=YourPassword;Min Pool Size=5;Max Pool Size=100;Pooling=true"
```

### 3. Connection Resilience

```csharp
.AddConfigurationStore(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.MigrationsAssembly(typeof(Program).Assembly.FullName);
        sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }))
```

### 4. Cleanup Old Tokens

```csharp
// Add token cleanup service
builder.Services.AddOperationalStore(options =>
{
    options.UseSqlServer(connectionString);
    options.EnableTokenCleanup = true;
    options.TokenCleanupInterval = 3600; // 1 hour
});
```

---

## 🐳 Docker Database Setup

### SQL Server

```yaml
# docker-compose.yml
version: '3.8'
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

volumes:
  sqlserver-data:
```

```bash
docker-compose up -d
```

### PostgreSQL

```yaml
# docker-compose.yml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=YourPassword
      - POSTGRES_DB=trustidentity
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data

volumes:
  postgres-data:
```

### MySQL

```yaml
# docker-compose.yml
version: '3.8'
services:
  mysql:
    image: mysql:8
    environment:
      - MYSQL_ROOT_PASSWORD=YourPassword
      - MYSQL_DATABASE=trustidentity
    ports:
      - "3306:3306"
    volumes:
      - mysql-data:/var/lib/mysql

volumes:
  mysql-data:
```

---

## 🧪 Testing Database Setup

### Verify Connection

```csharp
public static async Task TestDatabaseConnection(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    
    try
    {
        await configContext.Database.CanConnectAsync();
        Console.WriteLine("✅ Database connection successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database connection failed: {ex.Message}");
    }
}
```

### Verify Tables

```csharp
var tables = await configContext.Database
    .SqlQueryRaw<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES")
    .ToListAsync();

Console.WriteLine($"Found {tables.Count} tables");
```

---

## 📊 Database Maintenance

### Backup

```bash
# SQL Server
sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE TrustIdentity TO DISK='C:\backup\trustidentity.bak'"

# PostgreSQL
pg_dump -U postgres trustidentity > trustidentity_backup.sql

# MySQL
mysqldump -u root -p trustidentity > trustidentity_backup.sql
```

### Restore

```bash
# SQL Server
sqlcmd -S localhost -U sa -P YourPassword -Q "RESTORE DATABASE TrustIdentity FROM DISK='C:\backup\trustidentity.bak'"

# PostgreSQL
psql -U postgres trustidentity < trustidentity_backup.sql

# MySQL
mysql -u root -p trustidentity < trustidentity_backup.sql
```

### Cleanup Old Data

```sql
-- Delete expired tokens (older than 30 days)
DELETE FROM PersistedGrants 
WHERE Expiration < DATEADD(day, -30, GETUTCDATE());

-- Delete consumed authorization codes
DELETE FROM PersistedGrants 
WHERE Type = 'authorization_code' 
AND ConsumedTime IS NOT NULL;
```

---

## 🎯 Troubleshooting

### Issue: Migration Failed

```bash
# Reset migrations
dotnet ef database drop -c ConfigurationDbContext
dotnet ef migrations remove -c ConfigurationDbContext
dotnet ef migrations add InitialCreate -c ConfigurationDbContext
dotnet ef database update -c ConfigurationDbContext
```

### Issue: Connection Timeout

```csharp
// Increase connection timeout
"Server=localhost;Database=TrustIdentity;User Id=sa;Password=YourPassword;Connection Timeout=60"
```

### Issue: Table Already Exists

```bash
# Drop and recreate
dotnet ef database drop -c ConfigurationDbContext --force
dotnet ef database update -c ConfigurationDbContext
```

---

## ✅ Database Setup Checklist

- [ ] Install required NuGet packages
- [ ] Configure connection string
- [ ] Create migrations
- [ ] Update database
- [ ] Seed initial data
- [ ] Test connection
- [ ] Configure cleanup
- [ ] Set up backups
- [ ] Configure monitoring

---

**Your database is now ready for TrustIdentity!**
