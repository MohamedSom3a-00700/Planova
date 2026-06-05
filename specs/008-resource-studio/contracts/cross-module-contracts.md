# Cross-Module Contracts — Resource Studio

## Planova.Resource → Planova.Activity

Resource assignments reference Phase 5 activities. The Resource module depends on `Planova.Activity` for the `Activity` entity.

### Activity Reference

```csharp
// ResourceAssignment references Activity by ActivityId (Guid)
// No direct navigation property to Activity entity to avoid tight coupling.
// Activity name/Code are stored denormalized in the DTO layer for display.
//
// Queries requiring activity details will join through the Activity repository
// via the Application layer service, or use a soft reference pattern:
public record ResourceAssignmentDto
{
    // ...
    public Guid ActivityId { get; init; }
    public string ActivityCode { get; init; } // denormalized for display
    public string ActivityName { get; init; } // denormalized for display
}
```

### Activity Deletion Guard

```csharp
// Before deleting an Activity, check IResourceAssignmentRepository.HasAssignmentsForActivityAsync()
// If assignments exist, block deletion and return error listing the assignments.
// This is enforced in the ActivityService (or via a domain event).
```

---

## Planova.Resource → Planova.Project (Planova.Domain)

Resources can be scoped to a project (`Resource.Scope == Project`). The `ProjectId` references `Planova.Domain.Entities.Project`.

```csharp
// Resource.ProjectId → int? → FK → Project.Id
// Global resources have ProjectId == null
```

---

## Planova.Resource → Semantic Kernel (AI)

The AI estimation service uses Semantic Kernel via an abstracted provider.

```csharp
// IResourceAiEstimationService implementation uses:
// - Microsoft.SemanticKernel.Kernel (abstraction)
// - KernelPluginFactory.CreateFromType<ResourceEstimationPlugin>()
// - KernelArguments with activity context
//
// The plugin is instantiated with DI:
// - IKernel provides model access
// - ResourceRepository provides resource library context
// - ActivityRepository provides activity details
```

---

## Planova.Resource → Planova.Excel

Resource import/export uses the existing Planova.Excel infrastructure.

```csharp
// Planova.Excel provides:
// - IWorkbookReader for reading import files
// - IWorkbookWriter for writing export files
//
// Planova.Resource adds resource-specific readers/writers:
// ResourceImportReader : reads spreadsheet rows → List<ImportRowDto>
// ResourceReportWriter : writes report data → Excel file
```

---

## Planova.Resource → Planova.Localization

All user-facing strings in Resource Studio must have entries in both `ResourceResources.en.resx` and `ResourceResources.ar.resx`.

```csharp
// Key naming convention: "ResourceStudio.{Feature}.{Element}"
// Examples:
//   "ResourceStudio.Library.Title"
//   "ResourceStudio.Crew.BlendedRate"
//   "ResourceStudio.Histogram.Overallocated"
//   "ResourceStudio.AI.Estimating"
//   "ResourceStudio.Report.UsageSummary"
```

---

## Planova.Resource → Planova.UI (Shell Integration)

The Resource Studio registers a navigation target in `ShellViewModel`:

```csharp
// In ShellViewModel.RegisterNavigationTargets():
// _navigationService.RegisterTarget<ResourceStudioViewModel>(
//     "Resource Studio",
//     icon: NavigationIcons.Resource, // new icon
//     tooltip: "Manage project resources"
// );
```

The `ResourceStudioViewModel` manages its own inner tabs (Library, Crew Templates, Histogram, Reports) following the same pattern as `ActivityStudioViewModel`.

---

## Planova.Resource → Planova.Persistence

Resource entities added to `PlanovaDbContext` with configurations:

| Entity | DbSet |
|--------|-------|
| `Resource` | `DbSet<Resource>` |
| `ResourceRate` | `DbSet<ResourceRate>` |
| `Crew` | `DbSet<Crew>` |
| `CrewResource` | `DbSet<CrewResource>` |
| `ResourceAssignment` | `DbSet<ResourceAssignment>` |
| `ResourceUsage` | `DbSet<ResourceUsage>` |

New EF Core migration required after adding entity configurations.
