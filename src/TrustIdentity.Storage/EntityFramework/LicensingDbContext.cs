using Microsoft.EntityFrameworkCore;
using TrustIdentity.Licensing;

namespace TrustIdentity.Storage.EntityFramework;
/// <summary>
/// LicensingDbContext
/// </summary>
public class LicensingDbContext : DbContext
{
    /// <summary>
    /// LicensingDbContext
    /// </summary>
    /// <param name="options"></param>
    public LicensingDbContext(DbContextOptions<LicensingDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Licenses
    /// </summary>
    public DbSet<License> Licenses { get; set; } = null!;
    /// <summary>
    /// OnModelCreating
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<License>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.LicenseKey).IsRequired(); // Large text
            entity.Property(e => e.LicenseType).HasMaxLength(50);
            entity.HasIndex(e => e.CustomerEmail);
        });
    }
}
