# Persistence Contracts — Resource Studio

## EF Core Entity Configurations

Each entity has an `IEntityTypeConfiguration<T>` implementation following the existing pattern in `Planova.Persistence/EntityConfigurations/`.

### ResourceConfiguration

```csharp
public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ResourceType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Scope).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(r => r.DefaultRate).HasPrecision(18, 4).IsRequired();
        builder.Property(r => r.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(r => r.MaxQuantity).HasPrecision(18, 4);
        builder.Property(r => r.Currency).HasMaxLength(3).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.ProjectId);

        // Type-specific fields
        builder.Property(r => r.Trade).HasMaxLength(100);
        builder.Property(r => r.SkillLevel).HasMaxLength(50);
        builder.Property(r => r.EquipmentType).HasMaxLength(100);
        builder.Property(r => r.Capacity).HasMaxLength(100);
        builder.Property(r => r.OperatingCost).HasPrecision(18, 4);
        builder.Property(r => r.UnitPrice).HasPrecision(18, 4);
        builder.Property(r => r.WastagePercent).HasPrecision(5, 2);
        builder.Property(r => r.Company).HasMaxLength(200);
        builder.Property(r => r.ContractValue).HasPrecision(18, 2);
        builder.Property(r => r.ContactName).HasMaxLength(100);
        builder.Property(r => r.ContactPhone).HasMaxLength(50);

        builder.Property(r => r.IsGlobal).IsRequired();

        // Indexes
        builder.HasIndex(r => new { r.Code, r.Scope, r.ProjectId }).IsUnique();
        builder.HasIndex(r => new { r.Scope, r.ProjectId });
        builder.HasIndex(r => new { r.ResourceType, r.Scope });
        builder.HasIndex(r => r.Name);

        // Discriminator
        builder.HasDiscriminator(r => r.ResourceType)
            .HasValue<Resource>(ResourceType.Labour)
            .HasValue<Resource>(ResourceType.Equipment)
            .HasValue<Resource>(ResourceType.Material)
            .HasValue<Resource>(ResourceType.Subcontractor);

        // Navigation
        builder.HasMany(r => r.Rates).WithOne(rr => rr.Resource).HasForeignKey(rr => rr.ResourceId);
        builder.HasMany(r => r.CrewMemberships).WithOne(cr => cr.Resource).HasForeignKey(cr => cr.ResourceId);
        builder.HasMany(r => r.Assignments).WithOne(ra => ra.Resource).HasForeignKey(ra => ra.ResourceId);
        builder.HasMany(r => r.UsageRecords).WithOne(ru => ru.Resource).HasForeignKey(ru => ru.ResourceId);
    }
}
```

### ResourceRateConfiguration

```csharp
public class ResourceRateConfiguration : IEntityTypeConfiguration<ResourceRate>
{
    public void Configure(EntityTypeBuilder<ResourceRate> builder)
    {
        builder.ToTable("ResourceRates");
        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.EffectiveDate).IsRequired();
        builder.Property(rr => rr.Rate).HasPrecision(18, 4).IsRequired();
        builder.Property(rr => rr.Currency).HasMaxLength(3).IsRequired();
        builder.Property(rr => rr.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(rr => rr.IsDefault).IsRequired();
        builder.Property(rr => rr.Notes).HasMaxLength(500);

        builder.HasIndex(rr => new { rr.ResourceId, rr.EffectiveDate }).IsUnique();
        builder.HasIndex(rr => new { rr.ResourceId, rr.EffectiveDate }).IsDescending(false, true);
    }
}
```

### CrewConfiguration

```csharp
public class CrewConfiguration : IEntityTypeConfiguration<Crew>
{
    public void Configure(EntityTypeBuilder<Crew> builder)
    {
        builder.ToTable("Crews");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(c => c.Category).HasMaxLength(100);

        builder.HasMany(c => c.Resources).WithOne(cr => cr.Crew).HasForeignKey(cr => cr.CrewId);
    }
}
```

### CrewResourceConfiguration

```csharp
public class CrewResourceConfiguration : IEntityTypeConfiguration<CrewResource>
{
    public void Configure(EntityTypeBuilder<CrewResource> builder)
    {
        builder.ToTable("CrewResources");
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(cr => cr.IsLead).IsRequired();
        builder.Property(cr => cr.SortOrder).IsRequired();

        builder.HasIndex(cr => new { cr.CrewId, cr.ResourceId }).IsUnique();
    }
}
```

### ResourceAssignmentConfiguration

```csharp
public class ResourceAssignmentConfiguration : IEntityTypeConfiguration<ResourceAssignment>
{
    public void Configure(EntityTypeBuilder<ResourceAssignment> builder)
    {
        builder.ToTable("ResourceAssignments");
        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(ra => ra.Rate).HasPrecision(18, 4).IsRequired();
        builder.Property(ra => ra.Currency).HasMaxLength(3).IsRequired();
        builder.Property(ra => ra.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(ra => ra.TotalCost).HasPrecision(18, 2).IsRequired();
        builder.Property(ra => ra.DurationDays).HasPrecision(18, 2);
        builder.Property(ra => ra.Notes).HasMaxLength(500);

        builder.HasIndex(ra => ra.ActivityId);
        builder.HasIndex(ra => new { ra.ProjectId, ra.ResourceId });
    }
}
```

### ResourceUsageConfiguration

```csharp
public class ResourceUsageConfiguration : IEntityTypeConfiguration<ResourceUsage>
{
    public void Configure(EntityTypeBuilder<ResourceUsage> builder)
    {
        builder.ToTable("ResourceUsages");
        builder.HasKey(ru => ru.Id);

        builder.Property(ru => ru.Date).IsRequired();
        builder.Property(ru => ru.PlannedQuantity).HasPrecision(18, 4).IsRequired();
        builder.Property(ru => ru.ActualQuantity).HasPrecision(18, 4);

        builder.HasIndex(ru => new { ru.ResourceId, ru.Date });
        builder.HasIndex(ru => new { ru.Date, ru.ResourceId });
    }
}
```

## DbContext Registration

All Resource entities must be added to `PlanovaDbContext.OnModelCreating`:

```csharp
// In PlanovaDbContext.OnModelCreating:
modelBuilder.ApplyConfiguration(new ResourceConfiguration());
modelBuilder.ApplyConfiguration(new ResourceRateConfiguration());
modelBuilder.ApplyConfiguration(new CrewConfiguration());
modelBuilder.ApplyConfiguration(new CrewResourceConfiguration());
modelBuilder.ApplyConfiguration(new ResourceAssignmentConfiguration());
modelBuilder.ApplyConfiguration(new ResourceUsageConfiguration());
```

## Dependency Injection

```csharp
// In Planova.Persistence.Extensions.ServiceCollectionExtensions:
services.AddScoped<IResourceRepository, ResourceRepository>();
services.AddScoped<IResourceRateRepository, ResourceRateRepository>();
services.AddScoped<ICrewRepository, CrewRepository>();
services.AddScoped<ICrewResourceRepository, CrewResourceRepository>();
services.AddScoped<IResourceAssignmentRepository, ResourceAssignmentRepository>();
services.AddScoped<IResourceUsageRepository, ResourceUsageRepository>();
```
