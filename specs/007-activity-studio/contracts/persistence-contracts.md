# Persistence Contracts: Activity Studio

## DbContext Integration

Add the following DbSets to `PlanovaDbContext`:

```csharp
public class PlanovaDbContext : DbContext
{
    // Existing DbSets...

    // NEW: Activity Studio
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityRelationship> ActivityRelationships => Set<ActivityRelationship>();
    public DbSet<Calendar> Calendars => Set<Calendar>();
    public DbSet<CalendarDay> CalendarDays => Set<CalendarDay>();
    public DbSet<ActivityBank> ActivityBanks => Set<ActivityBank>();
    public DbSet<ActivityBankItem> ActivityBankItems => Set<ActivityBankItem>();
    public DbSet<ActivityBankItemRelationship> ActivityBankItemRelationships => Set<ActivityBankItemRelationship>();
}
```

## Entity Configuration Pattern

All configurations follow the existing `IEntityTypeConfiguration<T>` pattern:

```csharp
// Planova.Persistence/EntityConfigurations/ActivityConfiguration.cs
public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Code).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Notes).HasMaxLength(4000);
        builder.Property(e => e.Weight).HasColumnType("decimal(5,2)");
        builder.Property(e => e.PercentComplete).HasColumnType("decimal(5,2)");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ActivityType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.WbsItemId);
        builder.HasIndex(e => e.CalendarId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ParentActivityId);
        builder.HasIndex(e => new { e.ProjectId, e.Code }).IsUnique();

        builder.HasOne(e => e.ParentActivity)
               .WithMany(e => e.Children)
               .HasForeignKey(e => e.ParentActivityId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WbsItem)
               .WithMany()
               .HasForeignKey(e => e.WbsItemId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
```

## DI Registration

```csharp
// Planova.Activity/Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaActivity(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IActivityRelationshipRepository, ActivityRelationshipRepository>();
        services.AddScoped<ICalendarRepository, CalendarRepository>();
        services.AddScoped<ICalendarDayRepository, CalendarDayRepository>();
        services.AddScoped<IActivityBankRepository, ActivityBankRepository>();
        services.AddScoped<IActivityBankItemRepository, ActivityBankItemRepository>();
        services.AddScoped<IActivityBankItemRelationshipRepository, ActivityBankItemRelationshipRepository>();

        // Services
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IActivityRelationshipService, ActivityRelationshipService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IActivityBankService, ActivityBankService>();
        services.AddScoped<IWbsGenerationService, WbsGenerationService>();
        services.AddScoped<IActivityReportService, ActivityReportService>();

        return services;
    }
}
```

## App.xaml.cs Registration

```csharp
protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    // ... existing registrations ...
    services.AddPlanovaActivity();

    // Transient Views (Activity Studio)
    services.AddTransient<ActivityStudioView>();
    services.AddTransient<ActivityListView>();
    services.AddTransient<ActivityEditorView>();
    services.AddTransient<GanttChartView>();
    services.AddTransient<RelationshipEditorView>();
    services.AddTransient<CalendarManagerView>();
    services.AddTransient<CalendarDayGridView>();
    services.AddTransient<ActivityBankBrowserView>();
    services.AddTransient<ActivityBankPreviewView>();
    services.AddTransient<WbsGenerationWizardView>();
    services.AddTransient<ScheduleReportView>();

    // Transient ViewModels (Activity Studio)
    services.AddTransient<ActivityStudioViewModel>();
    services.AddTransient<ActivityListViewModel>();
    services.AddTransient<ActivityEditorViewModel>();
    services.AddTransient<GanttChartViewModel>();
    services.AddTransient<RelationshipEditorViewModel>();
    services.AddTransient<CalendarManagerViewModel>();
    services.AddTransient<CalendarDayGridViewModel>();
    services.AddTransient<ActivityBankBrowserViewModel>();
    services.AddTransient<ActivityBankPreviewViewModel>();
    services.AddTransient<WbsGenerationWizardViewModel>();
    services.AddTransient<ScheduleReportViewModel>();
}
```
