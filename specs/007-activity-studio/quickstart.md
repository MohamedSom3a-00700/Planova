# Quickstart: Activity Studio

## Prerequisites

- .NET 8 SDK
- SQLite (included via EF Core)
- Planova solution (`Planova.slnx`) builds successfully
- WBS Studio (Phase 4) is complete — Activity Studio requires existing WBS data

## Project Creation

```powershell
# From solution root
dotnet new classlib -n Planova.Activity -o Planova.Activity
dotnet sln Planova.slnx add Planova.Activity/Planova.Activity.csproj

# Add project references
dotnet add Planova.Activity/Planova.Activity.csproj reference Planova.Domain/Planova.Domain.csproj
dotnet add Planova.Activity/Planova.Activity.csproj reference Planova.Shared/Planova.Shared.csproj
dotnet add Planova.Activity/Planova.Activity.csproj reference Planova.Wbs/Planova.Wbs.csproj

# Add NuGet packages
dotnet add Planova.Activity/Planova.Activity.csproj package CommunityToolkit.Mvvm
dotnet add Planova.Activity/Planova.Activity.csproj package QuestPDF
dotnet add Planova.Activity/Planova.Activity.csproj package ClosedXML
```

## Test Project Creation

```powershell
dotnet new xunit -n Planova.Activity.Tests -o tests/Planova.Activity.Tests
dotnet sln Planova.slnx add tests/Planova.Activity.Tests/Planova.Activity.Tests.csproj
dotnet add tests/Planova.Activity.Tests/Planova.Activity.Tests.csproj reference Planova.Activity/Planova.Activity.csproj
dotnet add tests/Planova.Activity.Tests/Planova.Activity.Tests.csproj package Moq
dotnet add tests/Planova.Activity.Tests/Planova.Activity.Tests.csproj package FluentAssertions
```

## Implementation Order

### Step 1: Domain Layer — Entities & Enums

Create `Planova.Activity/Domain/Enums/` and `Planova.Activity/Domain/Entities/` following `data-model.md`. Each entity is a plain C# class with `Guid Id`, timestamps, and navigation properties.

**Key files**:
- `Enums/ActivityType.cs`, `ActivityStatus.cs`, `RelationshipType.cs`, `CalendarType.cs`
- `Entities/Activity.cs`, `ActivityRelationship.cs`, `Calendar.cs`, `CalendarDay.cs`
- `Entities/ActivityBank.cs`, `ActivityBankItem.cs`, `ActivityBankItemRelationship.cs`

### Step 2: Domain Layer — Interfaces

Create `Planova.Activity/Domain/Interfaces/` with repository and service interfaces per `contracts/domain-interfaces.md`.

### Step 3: Application Layer — DTOs

Create `Planova.Activity/Application/Dto/` with all data transfer objects per `contracts/application-dtos.md`.

### Step 4: Application Layer — Services

Create `Planova.Activity/Application/Services/`. Key implementation challenges:

| Service | Challenge | Approach |
|---------|-----------|----------|
| `ActivityRelationshipService` | Circular reference detection | DFS-based cycle check validated before save |
| `CircularReferenceDetector` | Performance on 10k+ graphs | Adjacency list + iterative DFS with HashSet visited tracking |
| `CalendarService` | Working day calculations | Pure in-memory date engine; cache CalendarDay lookups |
| `CalendarDateCalculator` | Edge cases (all non-working days) | Fallback to continuous 7-day calendar if no working days |
| `WbsGenerationService` | Large batch generation (200+ activities) | Bulk insert with EF Core AddRange; progress reporting via IProgress<T> |
| `ActivityBankService` | 50+ seed entries | JSON config loaded on first access; seeded via repository |

### Step 5: Persistence Layer — EF Configurations

Add to `Planova.Persistence`:
- Entity configurations in `EntityConfigurations/`
- DbSet properties in `PlanovaDbContext`
- Indexes per `data-model.md` Indexes section

### Step 6: DI Registration

- Create `Planova.Activity/Extensions/ServiceCollectionExtensions.cs` with `AddPlanovaActivity()`
- Register in `App.xaml.cs` `ConfigureServices()`

### Step 7: UI Layer — Controls

Create `Planova.UI/Controls/ActivityGanttCanvas.cs`:

```csharp
// Custom DrawingVisual-based Canvas control
// - Virtualized rendering: only render visible time range
// - DrawingVisual batching for performance
// - Supports zoom levels: Day, Week, Month
// - Renders: activity bars (rectangles), milestones (diamonds), relationship arrows
// - RTL-aware (respects FlowDirection)
```

### Step 8: UI Layer — ViewModels & Views

Create ViewModels in `Planova.UI/ViewModels/Activity/` and Views in `Planova.UI/Views/Activity/` per `contracts/ui-contracts.md`. Each ViewModel uses `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm.

### Step 9: Shell Integration

Replace placeholder registration in `ShellViewModel.cs`:

```csharp
nav.RegisterTarget("activity", "Activity Studio", "CalendarDay24", true, false, () =>
{
    var view = _serviceProvider.GetRequiredService<ActivityStudioView>();
    view.InitializeTabs(_serviceProvider);
    return view;
});
```

### Step 10: Localization

Add `Planova.Localization/Resources/ActivityResources.en.resx` and `ActivityResources.ar.resx` with all UI strings.

### Step 11: Tests

Write tests in `Planova.Activity.Tests`:

| Test File | Coverage |
|-----------|----------|
| `ActivityTests.cs` | Entity validation, status transitions, WbsSummary rollup logic |
| `ActivityRelationshipTests.cs` | Relationship validation, self-reference rejection |
| `CircularReferenceDetectorTests.cs` | Simple cycles, complex cycles, no-cycles, 10k-node performance |
| `CalendarDateCalculatorTests.cs` | Working day addition, holiday exceptions, all-nonworking fallback, edge cases |
| `CalendarServiceTests.cs` | CRUD, day status toggle, bulk set |
| `ActivityBankServiceTests.cs` | Browse, search, apply to WBS, merge vs replace |
| `WbsGenerationServiceTests.cs` | Simple 1:1 generation, bank mode generation, existing activity detection |
| `ActivityReportServiceTests.cs` | Report generation, Excel export, PDF export |
| `ActivityServiceTests.cs` | CRUD, filter, search, code generation |

## Build & Verify

```powershell
# Build the solution
dotnet build Planova.slnx

# Run Activity Studio tests only
dotnet test tests/Planova.Activity.Tests/Planova.Activity.Tests.csproj

# Run all tests
dotnet test Planova.slnx
```

## Key Patterns Reference

| Pattern | Location | File Example |
|---------|----------|-------------|
| Clean Architecture layers | `Planova.Wbs/` | Follows same `Domain/`, `Application/`, `Extensions/` structure |
| Studio tab registration | `Planova.UI/Views/Wbs/WbsStudioView.xaml.cs` | `InitializeTabs(IServiceProvider)` populates tab collection |
| Navigation registration | `Planova.UI/ViewModels/ShellViewModel.cs` | `nav.RegisterTarget(...)` with factory lambda |
| DI extension method | `Planova.Wbs/Extensions/ServiceCollectionExtensions.cs` | `AddPlanovaWbs()` pattern |
| Localization | `Planova.Localization/Resources/WbsResources.en.resx` | Resx + `ILocalizationService` |
| Repository + DbContext | `Planova.Persistence/Repositories/` and `EntityConfigurations/` | EF Core with `IEntityTypeConfiguration` |
| Activity Bank seed data | JSON file (to be created) | Loaded on first `IActivityBankService.SeedIfEmptyAsync()` call |
