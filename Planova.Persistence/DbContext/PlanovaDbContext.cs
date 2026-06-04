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
    public DbSet<Planova.Wbs.Domain.Entities.Wbs> WbsEntries => Set<Planova.Wbs.Domain.Entities.Wbs>();
    public DbSet<Planova.Wbs.Domain.Entities.WbsItem> WbsItems => Set<Planova.Wbs.Domain.Entities.WbsItem>();
    public DbSet<Planova.Wbs.Domain.Entities.WbsTemplate> WbsTemplates => Set<Planova.Wbs.Domain.Entities.WbsTemplate>();
    public DbSet<Planova.Wbs.Domain.Entities.WbsTemplateItem> WbsTemplateItems => Set<Planova.Wbs.Domain.Entities.WbsTemplateItem>();

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
        modelBuilder.ApplyConfiguration(new WbsConfiguration());
        modelBuilder.ApplyConfiguration(new WbsItemConfiguration());
        modelBuilder.ApplyConfiguration(new WbsTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new WbsTemplateItemConfiguration());
    }
}
