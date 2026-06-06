# Persistence Contracts — Cost Studio

## EF Core Entity Configurations

Each entity has a corresponding `IEntityTypeConfiguration<T>` class in `Planova.Persistence.EntityConfigurations`.

### BudgetConfiguration

```csharp
public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    // Table: Budgets
    // PK: Id (Guid)
    // Required: ProjectId, ResourceCostTotal, DirectCostTotal, TotalBudget, Currency, Status
    // MaxLength: Currency(3), Status(20)
    // Precision: decimal(18,2) for all monetary fields
    // Precision: decimal(5,2) for ContingencyPercent
    // Unique Index: (ProjectId)
    // Navigation: Revisions → BudgetRevision (BudgetId FK)
}
```

### BudgetRevisionConfiguration

```csharp
public class BudgetRevisionConfiguration : IEntityTypeConfiguration<BudgetRevision>
{
    // Table: BudgetRevisions
    // PK: Id (Guid)
    // Required: BudgetId, RevisionNumber, RevisionType, Amount, Status, CreatedBy
    // MaxLength: RevisionType(20), Status(20), Reason(500), ApprovedBy(100)
    // Precision: decimal(18,2) for Amount
    // Unique Index: (BudgetId, RevisionNumber)
    // FK: BudgetId → Budget.Id (Cascade delete)
}
```

### DirectCostConfiguration

```csharp
public class DirectCostConfiguration : IEntityTypeConfiguration<DirectCost>
{
    // Table: DirectCosts
    // PK: Id (Guid)
    // Required: ProjectId, Category, Description, Quantity, UnitOfMeasure,
    //           UnitRate, Currency, TotalAmount, Scope
    // MaxLength: Category(30), CustomCategoryName(200), Description(500),
    //           UnitOfMeasure(50), Currency(3), Scope(20)
    // Precision: decimal(18,4) for Quantity, UnitRate
    // Precision: decimal(18,2) for TotalAmount
    // Index: (ProjectId, Scope)
    // Index: (ActivityId) — filtered, nullable
    // FK: ActivityId → Activity.Id (optional, SetNull on delete)
}
```

### CostBaselineConfiguration

```csharp
public class CostBaselineConfiguration : IEntityTypeConfiguration<CostBaseline>
{
    // Table: CostBaselines
    // PK: Id (Guid)
    // Required: ProjectId, IsActive, CreatedBy
    // MaxLength: Description(500), CreatedBy(100)
    // Filtered Unique Index: (ProjectId) WHERE IsActive = 1
    // Navigation: Rows → CostBaselineRow (BaselineId FK)
}
```

### CostBaselineRowConfiguration

```csharp
public class CostBaselineRowConfiguration : IEntityTypeConfiguration<CostBaselineRow>
{
    // Table: CostBaselineRows
    // PK: Id (Guid)
    // Required: BaselineId, ActivityId, PlannedCost, PlannedStart, PlannedFinish, BudgetAtCompletion
    // Precision: decimal(18,2) for all monetary fields
    // Index: (BaselineId, ActivityId)
    // FK: BaselineId → CostBaseline.Id (Cascade delete)
    // FK: ActivityId → Activity.Id (Restrict)
}
```

### ActualCostConfiguration

```csharp
public class ActualCostConfiguration : IEntityTypeConfiguration<ActualCost>
{
    // Table: ActualCosts
    // PK: Id (Guid)
    // Required: ProjectId, ActivityId, Amount, Currency, Source, EntryDate
    // MaxLength: Currency(3), Source(20), ImportBatchId(50)
    // Precision: decimal(18,2) for Amount
    // Unique Index: (ActivityId)
    // Index: (ProjectId)
    // FK: ActivityId → Activity.Id (Restrict)
}
```

## DbContext Registration

In `PlanovaDbContext.OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... existing configurations ...
    
    modelBuilder.ApplyConfiguration(new BudgetConfiguration());
    modelBuilder.ApplyConfiguration(new BudgetRevisionConfiguration());
    modelBuilder.ApplyConfiguration(new DirectCostConfiguration());
    modelBuilder.ApplyConfiguration(new CostBaselineConfiguration());
    modelBuilder.ApplyConfiguration(new CostBaselineRowConfiguration());
    modelBuilder.ApplyConfiguration(new ActualCostConfiguration());
}
```

## Repository Implementations

Repository implementations follow the existing pattern in `Planova.Persistence.Repositories`:

```csharp
public class BudgetRepository : IBudgetRepository
{
    private readonly PlanovaDbContext _context;
    // Constructor injection
    
    // All methods use _context.Set<Budget>()
    // AsNoTracking() for read queries
    // Include() for navigation properties
}
```

All repositories are registered in `ServiceCollectionExtensions` as scoped services.
