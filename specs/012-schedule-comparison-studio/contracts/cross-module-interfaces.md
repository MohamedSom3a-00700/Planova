# Cross-Module Interface Contracts: Schedule Comparison Studio

**Date**: 2026-06-14

## Consumed Interfaces (from other modules)

| Interface | Module | Resolution | Purpose |
|-----------|--------|------------|---------|
| `IPrimaveraWorkspaceService` | `Planova.Primavera` | `IServiceProvider.GetService<>` | Primavera schedule data (optional) |
| `IActivityService` | `Planova.Activity` | Constructor injection | Native activity data for snapshot capture |
| `IResourceAssignmentService` | `Planova.Resource` | Constructor injection | Native resource assignments for snapshot capture |
| `ICurrentProjectService` | `Planova.Shared` | Constructor injection | Active project context |

## Exposed Interfaces (to other modules)

| Interface | Defined In | Consumers | Purpose |
|-----------|------------|-----------|---------|
| `IComparisonRepository` | `Planova.ScheduleComparison.Domain` | `Planova.Persistence` (implements) | Persistence contract for comparison entities |

## Persistence Registration Contract

```csharp
// Planova.Persistence.Extensions.ServiceCollectionExtensions
// Repository implementation is registered here, NOT in the ScheduleComparison module.
services.AddScoped<IComparisonRepository, ComparisonRepository>();
```

**Rule**: `Planova.ScheduleComparison` MUST NOT reference `Planova.Persistence` or any EF Core NuGet package. The module depends only on the `IComparisonRepository` interface defined in its own Domain layer.

## ScheduleComparisonService ResolvePrimavera

```csharp
private IPrimaveraWorkspaceService? ResolvePrimavera() =>
    _serviceProvider.GetService<IPrimaveraWorkspaceService>();
```

**Rule**: Must use `GetService<>` not `GetRequiredService<>`. Returns null when Primavera is not registered.
