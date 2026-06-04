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
    public DbSet<Planova.Boq.Domain.Entities.Boq> Boqs => Set<Planova.Boq.Domain.Entities.Boq>();
    public DbSet<Planova.Boq.Domain.Entities.BoqItem> BoqItems => Set<Planova.Boq.Domain.Entities.BoqItem>();
    public DbSet<Planova.Boq.Domain.Entities.BoqClassification> BoqClassifications => Set<Planova.Boq.Domain.Entities.BoqClassification>();
    public DbSet<Planova.Boq.Domain.Entities.BoqLibrary> BoqLibraries => Set<Planova.Boq.Domain.Entities.BoqLibrary>();
    public DbSet<Planova.Boq.Domain.Entities.BoqLibraryItem> BoqLibraryItems => Set<Planova.Boq.Domain.Entities.BoqLibraryItem>();

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
        modelBuilder.ApplyConfiguration(new BoqConfiguration());
        modelBuilder.ApplyConfiguration(new BoqItemConfiguration());
        modelBuilder.ApplyConfiguration(new BoqClassificationConfiguration());
        modelBuilder.ApplyConfiguration(new BoqLibraryConfiguration());
        modelBuilder.ApplyConfiguration(new BoqLibraryItemConfiguration());
    }
}
