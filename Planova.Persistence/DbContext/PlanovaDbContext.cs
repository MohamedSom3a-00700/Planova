using Microsoft.EntityFrameworkCore;
using Planova.Domain.Entities;
using Planova.Persistence.EntityConfigurations;

namespace Planova.Persistence.DbContext;

public class PlanovaDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<SchemaVersion> SchemaVersions => Set<SchemaVersion>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();

    public PlanovaDbContext(DbContextOptions<PlanovaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserPreferencesConfiguration());

        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Version).IsRequired();
            entity.HasIndex(e => e.Version).IsUnique();
            entity.Property(e => e.AppliedAt).HasDefaultValueSql("datetime('now')");
        });
    }
}
