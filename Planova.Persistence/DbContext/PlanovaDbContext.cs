using Microsoft.EntityFrameworkCore;
using Planova.Domain.Entities;
using Planova.Persistence.EntityConfigurations;
using Planova.Reporting.Domain.Entities;
using Planova.ScheduleComparison.Domain.Entities;

namespace Planova.Persistence.DbContext;

public class PlanovaDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<Subcontractor> Subcontractors => Set<Subcontractor>();
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
    public DbSet<Planova.Activity.Domain.Entities.Activity> Activities => Set<Planova.Activity.Domain.Entities.Activity>();
    public DbSet<Planova.Activity.Domain.Entities.ActivityRelationship> ActivityRelationships => Set<Planova.Activity.Domain.Entities.ActivityRelationship>();
    public DbSet<Planova.Activity.Domain.Entities.Calendar> Calendars => Set<Planova.Activity.Domain.Entities.Calendar>();
    public DbSet<Planova.Activity.Domain.Entities.CalendarDay> CalendarDays => Set<Planova.Activity.Domain.Entities.CalendarDay>();
    public DbSet<Planova.Activity.Domain.Entities.ActivityBank> ActivityBanks => Set<Planova.Activity.Domain.Entities.ActivityBank>();
    public DbSet<Planova.Activity.Domain.Entities.ActivityBankItem> ActivityBankItems => Set<Planova.Activity.Domain.Entities.ActivityBankItem>();
    public DbSet<Planova.Activity.Domain.Entities.ActivityBankItemRelationship> ActivityBankItemRelationships => Set<Planova.Activity.Domain.Entities.ActivityBankItemRelationship>();
    public DbSet<Planova.Resource.Domain.Entities.Resource> Resources => Set<Planova.Resource.Domain.Entities.Resource>();
    public DbSet<Planova.Resource.Domain.Entities.ResourceRate> ResourceRates => Set<Planova.Resource.Domain.Entities.ResourceRate>();
    public DbSet<Planova.Resource.Domain.Entities.Crew> Crews => Set<Planova.Resource.Domain.Entities.Crew>();
    public DbSet<Planova.Resource.Domain.Entities.CrewResource> CrewResources => Set<Planova.Resource.Domain.Entities.CrewResource>();
    public DbSet<Planova.Resource.Domain.Entities.ResourceAssignment> ResourceAssignments => Set<Planova.Resource.Domain.Entities.ResourceAssignment>();
    public DbSet<Planova.Resource.Domain.Entities.ResourceUsage> ResourceUsages => Set<Planova.Resource.Domain.Entities.ResourceUsage>();
    public DbSet<Planova.Cost.Domain.Entities.Budget> Budgets => Set<Planova.Cost.Domain.Entities.Budget>();
    public DbSet<Planova.Cost.Domain.Entities.BudgetRevision> BudgetRevisions => Set<Planova.Cost.Domain.Entities.BudgetRevision>();
    public DbSet<Planova.Cost.Domain.Entities.DirectCost> DirectCosts => Set<Planova.Cost.Domain.Entities.DirectCost>();
    public DbSet<Planova.Cost.Domain.Entities.CostBaseline> CostBaselines => Set<Planova.Cost.Domain.Entities.CostBaseline>();
    public DbSet<Planova.Cost.Domain.Entities.CostBaselineRow> CostBaselineRows => Set<Planova.Cost.Domain.Entities.CostBaselineRow>();
    public DbSet<Planova.Cost.Domain.Entities.ActualCost> ActualCosts => Set<Planova.Cost.Domain.Entities.ActualCost>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();

    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();
    public DbSet<ReportInstance> ReportInstances => Set<ReportInstance>();
    public DbSet<ReportSection> ReportSections => Set<ReportSection>();
    public DbSet<ReportSchedule> ReportSchedules => Set<ReportSchedule>();
    public DbSet<ReportExport> ReportExports => Set<ReportExport>();
    public DbSet<ReportSettings> ReportSettings => Set<ReportSettings>();
    public DbSet<ProjectParty> ProjectParties => Set<ProjectParty>();

    public DbSet<ComparisonSession> ComparisonSessions => Set<ComparisonSession>();
    public DbSet<ComparisonResult> ComparisonResults => Set<ComparisonResult>();
    public DbSet<ScheduleSnapshot> ScheduleSnapshots => Set<ScheduleSnapshot>();
    public DbSet<ComparisonRule> ComparisonRules => Set<ComparisonRule>();

    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraProject> PrimaveraProjects => Set<Planova.Primavera.Domain.Entities.PrimaveraProject>();
    public DbSet<Planova.Primavera.Domain.Entities.XerImportSession> XerImportSessions => Set<Planova.Primavera.Domain.Entities.XerImportSession>();
    public DbSet<Planova.Primavera.Domain.Entities.XerExportProfile> XerExportProfiles => Set<Planova.Primavera.Domain.Entities.XerExportProfile>();
    public DbSet<Planova.Primavera.Domain.Entities.XerRawTable> XerRawTables => Set<Planova.Primavera.Domain.Entities.XerRawTable>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraActivity> PrimaveraActivities => Set<Planova.Primavera.Domain.Entities.PrimaveraActivity>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraRelationship> PrimaveraRelationships => Set<Planova.Primavera.Domain.Entities.PrimaveraRelationship>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraResourceAssignment> PrimaveraResourceAssignments => Set<Planova.Primavera.Domain.Entities.PrimaveraResourceAssignment>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraCalendar> PrimaveraCalendars => Set<Planova.Primavera.Domain.Entities.PrimaveraCalendar>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraCode> PrimaveraCodes => Set<Planova.Primavera.Domain.Entities.PrimaveraCode>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraBaseline> PrimaveraBaselines => Set<Planova.Primavera.Domain.Entities.PrimaveraBaseline>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraUdf> PrimaveraUdfs => Set<Planova.Primavera.Domain.Entities.PrimaveraUdf>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraValidationRule> PrimaveraValidationRules => Set<Planova.Primavera.Domain.Entities.PrimaveraValidationRule>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraValidationIssue> PrimaveraValidationIssues => Set<Planova.Primavera.Domain.Entities.PrimaveraValidationIssue>();
    public DbSet<Planova.Primavera.Domain.Entities.PrimaveraRepairAction> PrimaveraRepairActions => Set<Planova.Primavera.Domain.Entities.PrimaveraRepairAction>();

    public PlanovaDbContext(DbContextOptions<PlanovaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserPreferencesConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new ContractConfiguration());
        modelBuilder.ApplyConfiguration(new ContractorConfiguration());
        modelBuilder.ApplyConfiguration(new SubcontractorConfiguration());
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
        modelBuilder.ApplyConfiguration(new ActivityConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityRelationshipConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarDayConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityBankConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityBankItemConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityBankItemRelationshipConfiguration());
        modelBuilder.ApplyConfiguration(new ResourceConfiguration());
        modelBuilder.ApplyConfiguration(new ResourceRateConfiguration());
        modelBuilder.ApplyConfiguration(new CrewConfiguration());
        modelBuilder.ApplyConfiguration(new CrewResourceConfiguration());
        modelBuilder.ApplyConfiguration(new ResourceAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new ResourceUsageConfiguration());

        modelBuilder.ApplyConfiguration(new BudgetConfiguration());
        modelBuilder.ApplyConfiguration(new BudgetRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new DirectCostConfiguration());
        modelBuilder.ApplyConfiguration(new CostBaselineConfiguration());
        modelBuilder.ApplyConfiguration(new CostBaselineRowConfiguration());
        modelBuilder.ApplyConfiguration(new ActualCostConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectDocumentConfiguration());

        modelBuilder.ApplyConfiguration(new ReportTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new ReportInstanceConfiguration());
        modelBuilder.ApplyConfiguration(new ReportSectionConfiguration());
        modelBuilder.ApplyConfiguration(new ReportScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new ReportExportConfiguration());
        modelBuilder.ApplyConfiguration(new ReportSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectPartyConfiguration());

        modelBuilder.ApplyConfiguration(new ComparisonSessionConfiguration());
        modelBuilder.ApplyConfiguration(new ComparisonResultConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduleSnapshotConfiguration());
        modelBuilder.ApplyConfiguration(new ComparisonRuleConfiguration());

        modelBuilder.ApplyConfiguration(new PrimaveraProjectConfiguration());
        modelBuilder.ApplyConfiguration(new XerImportSessionConfiguration());
        modelBuilder.ApplyConfiguration(new XerExportProfileConfiguration());
        modelBuilder.ApplyConfiguration(new XerRawTableConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraActivityConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraRelationshipConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraResourceAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraCalendarConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraCodeConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraBaselineConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraUdfConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraValidationRuleConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraValidationIssueConfiguration());
        modelBuilder.ApplyConfiguration(new PrimaveraRepairActionConfiguration());
    }
}
