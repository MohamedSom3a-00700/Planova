using Microsoft.EntityFrameworkCore;
using Planova.Domain.Entities;
using Planova.Persistence.EntityConfigurations;

namespace Planova.Persistence.DbContext;

public class PlanovaDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ExcelMappingProfile> ExcelMappingProfiles => Set<ExcelMappingProfile>();

    public PlanovaDbContext(DbContextOptions<PlanovaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserPreferencesConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new ContractConfiguration());
        modelBuilder.ApplyConfiguration(new ExcelMappingProfileConfiguration());
    }
}
