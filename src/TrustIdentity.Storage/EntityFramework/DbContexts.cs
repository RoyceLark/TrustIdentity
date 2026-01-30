using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Storage.EntityFramework;

/// <summary>
/// Database context for configuration data
/// </summary>
public class ConfigurationDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Initializes a new instance of the ConfigurationDbContext
    /// </summary>
    /// <param name="options">The context options</param>
    /// <param name="tenantContext">The tenant context</param>
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    /// <summary>The collection of clients</summary>
    public DbSet<Client> Clients { get; set; } = null!;
    /// <summary>The collection of identity resources</summary>
    public DbSet<IdentityResource> IdentityResources { get; set; } = null!;
    /// <summary>The collection of API resources</summary>
    public DbSet<ApiResource> ApiResources { get; set; } = null!;
    /// <summary>The collection of API scopes</summary>
    public DbSet<ApiScope> ApiScopes { get; set; } = null!;
    /// <summary>The collection of tenants</summary>
    public DbSet<Tenant> Tenants { get; set; } = null!;

    /// <summary>
    /// Configures the model using the model builder
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Tenant Filter
        // If a tenant is active, filter entities by TenantId.
        // If no tenant is active (Host Admin), show all.
        // Capturing the specific ID in a variable for the expression tree
        
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId);
            entity.Property(e => e.ClientId).HasMaxLength(200);
            entity.Property(e => e.ClientName).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.ClientId).IsUnique();

            // Tenant Filter
            entity.HasQueryFilter(e => _tenantContext.CurrentTenant == null || e.TenantId == _tenantContext.CurrentTenant.Id);
        });

        modelBuilder.Entity<IdentityResource>(entity =>
        {
            entity.HasKey(e => e.Name);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
        });

        modelBuilder.Entity<ApiResource>(entity =>
        {
            entity.HasKey(e => e.Name);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
        });

        modelBuilder.Entity<ApiScope>(entity =>
        {
            entity.HasKey(e => e.Name);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Identifier).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Host).HasMaxLength(500);
            entity.Property(e => e.IssuerUri).HasMaxLength(500);
            entity.Property(e => e.SubscriptionTier).HasMaxLength(50);
            
            entity.HasIndex(e => e.Identifier).IsUnique();
            entity.HasIndex(e => e.Host);
            entity.HasIndex(e => e.IsActive);
        });
    }
}

/// <summary>
/// Database context for persisted grants
/// </summary>
public class PersistedGrantDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the PersistedGrantDbContext
    /// </summary>
    /// <param name="options">The context options</param>
    public PersistedGrantDbContext(DbContextOptions<PersistedGrantDbContext> options)
        : base(options)
    {
    }

    /// <summary>The collection of persisted grants</summary>
    public DbSet<PersistedGrant> PersistedGrants { get; set; } = null!;
    /// <summary>The collection of device flow codes</summary>
    public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; } = null!;
    /// <summary>The collection of audit events</summary>
    public DbSet<Abstractions.Stores.AuditEvent> AuditEvents { get; set; } = null!;
    /// <summary>The collection of user sessions</summary>
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    /// <summary>The collection of user consents</summary>
    public DbSet<UserConsent> UserConsents { get; set; } = null!;

    /// <summary>
    /// Configures the model using the model builder
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PersistedGrant>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SubjectId).HasMaxLength(200);
            entity.Property(e => e.ClientId).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(200);
            
            entity.HasIndex(e => new { e.SubjectId, e.ClientId, e.Type });
            entity.HasIndex(e => e.Expiration);
        });

        modelBuilder.Entity<DeviceFlowCodes>(entity =>
        {
            entity.HasKey(e => e.DeviceCode);
            entity.Property(e => e.DeviceCode).HasMaxLength(200);
            entity.Property(e => e.UserCode).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SubjectId).HasMaxLength(200);
            entity.Property(e => e.ClientId).HasMaxLength(200).IsRequired();
            
            entity.HasIndex(e => e.UserCode).IsUnique();
            entity.HasIndex(e => e.Expiration);
        });

        modelBuilder.Entity<Abstractions.Stores.AuditEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SubjectId).HasMaxLength(200);
            entity.Property(e => e.ClientId).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EventType);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.SessionId);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.SubjectId).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            
            entity.HasIndex(e => e.SubjectId);
            entity.HasIndex(e => e.Expires);
        });

        modelBuilder.Entity<UserConsent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SubjectId).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ClientId).HasMaxLength(200).IsRequired();
            
            entity.Property(e => e.Scopes)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            entity.HasIndex(e => new { e.SubjectId, e.ClientId }).IsUnique();
        });
    }
}

/// <summary>
/// Combined database context for the entire identity system
/// </summary>
public class TrustIdentityDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Initializes a new instance of the TrustIdentityDbContext
    /// </summary>
    /// <param name="options">The context options</param>
    /// <param name="tenantContext">The tenant context</param>
    public TrustIdentityDbContext(DbContextOptions<TrustIdentityDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    /// <summary>The collection of users</summary>
    public DbSet<Core.Models.TrustIdentityUser> Users { get; set; } = null!;
    /// <summary>
    /// Configures the model using the model builder
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Core.Models.TrustIdentityUser>(entity =>
        {
            entity.HasKey(e => e.SubjectId);
            entity.Property(e => e.SubjectId).HasMaxLength(200);
            entity.Property(e => e.Username).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email);

            entity.HasMany(e => e.Claims)
                  .WithOne()
                  .HasForeignKey("UserId")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.OwnsOne(e => e.AIProfile, builder => 
            {
                builder.ToJson();
                builder.OwnsMany(p => p.BehaviorPatterns, bp => bp.Ignore(x => x.Metadata));
                builder.OwnsMany(p => p.KnownDevices);
                builder.OwnsMany(p => p.RecentLogins);
            });

            // Tenant Filter
            entity.HasQueryFilter(e => _tenantContext.CurrentTenant == null || e.TenantId == _tenantContext.CurrentTenant.Id);
        });

        modelBuilder.Entity<Core.Models.UserClaim>(entity =>
        {
            entity.ToTable("UserClaims");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(250).IsRequired();
        });
    }
}