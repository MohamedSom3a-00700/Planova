# Planova Phase 10 Implementation Plan

**Phase**: 10 - Schedule Comparison Studio

**Date**: 2026-06-14

**Source of Truth**:
[docs/01-PRODUCT_VISION.md](./01-PRODUCT_VISION.md),
[docs/02-MASTER_ROADMAP.md](./02-MASTER_ROADMAP.md),
[docs/04-SYSTEM_ARCHITECTURE.md](./04-SYSTEM_ARCHITECTURE.md),
[docs/05-TECHNOLOGY_STACK.md](./05-TECHNOLOGY_STACK.md),
[docs/06-MODULE_CATALOG.md](./06-MODULE_CATALOG.md),
[docs/07-DATABASE_STRATEGY.md](./07-DATABASE_STRATEGY.md),
[docs/08-INTEGRATION_STRATEGY.md](./08-INTEGRATION_STRATEGY.md),
[docs/11-UI_UX_DESIGN_SYSTEM.md](./11-UI_UX_DESIGN_SYSTEM.md),
[docs/PLANOVA_CONSTITUTION_DRAFT.md](./PLANOVA_CONSTITUTION_DRAFT.md),
[docs/PHASE_8_IMPLEMENTATION_PLAN.md](./PHASE_8_IMPLEMENTATION_PLAN.md),
[docs/PHASE_9_IMPLEMENTATION_PLAN.md](./PHASE_9_IMPLEMENTATION_PLAN.md)

---

## Summary

Phase 10 delivers the **Schedule Comparison Studio** — a dedicated workspace for comparing two schedule snapshots side-by-side, detecting every type of change (activities, dates, durations, logic, resources, critical path, float), and exporting the results. It is the first consumer of the **optional service resolution** pattern, resolving schedule data from either native Planova studios (`IActivityService`, `IResourceAssignmentService`) or imported Primavera XER data (resolved via `IServiceProvider.GetService<IPrimaveraWorkspaceService>()`).

The studio introduces:

1. **Four comparison modes** — Baseline vs Update, Update vs Update, XER vs XER, and As-Planned vs As-Built
2. **Five comparison dimensions** — activity diff, logic diff, resource diff, critical path diff, and float impact
3. **Schedule snapshot capture** — freeze native Planova schedule data at a point in time for later comparison
4. **Cross-studio data resolution** — fallthrough chain: snapshot → Primavera → native services
5. **Phase 11 consumable output** — `ScheduleComparisonResult` versioned JSON envelope stored on `ComparisonSession.ResultJson`, ready for Delay Analysis attribution
6. **Queryable per-diff row storage** — `ComparisonResult` entities persisted for grids, filtering, paging, and history drill-down
7. **Primavera-style workspace** — dedicated TabControl workspace with `InitializeTabs(IServiceProvider)` pattern matching the established studio convention

**Phase 10/11 Boundary**: Phase 10 produces **unattributed diffs** — showing **what** changed between two schedules (variance in dates, logic, resources, float, and critical path). Phase 11 (Delay Analysis) will consume these diffs to attribute delays to causes and compute EOT entitlement.

---

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: WPF, Fluent UI WPF, CommunityToolkit.Mvvm, EF Core 8, SQLite, Microsoft.Extensions.Hosting, Serilog, ClosedXML, QuestPDF, and the existing Planova shared abstractions

**Comparison Engine Strategy**: Build focused comparers per entity type (Activity, Logic, Resource, Critical Path, Float). Each comparer takes two `ScheduleData` objects and produces a list of `ComparisonResult` records (the queryable per-diff row entities). The orchestration service (`IScheduleComparisonService`) resolves the source/target data through the fallthrough chain, runs all selected comparers, persists the individual `ComparisonResult` rows, and stores the complete `ScheduleComparisonResult` envelope as `ComparisonSession.ResultJson` for Phase 11.

**Supported Comparison Modes**:

| Mode | Source | Target | Schedule Data Source |
|------|--------|--------|---------------------|
| BaselineVsUpdate | A stored baseline | Current schedule | Native snapshot or Primavera |
| UpdateVsUpdate | Previous update snapshot | Current update snapshot | Native snapshot |
| XerVsXer | Primavera project (import) | Primavera project (import) | `IPrimaveraWorkspaceService` |
| AsPlannedVsAsBuilt | Planned snapshot | As-built snapshot | Native snapshot |

**Schedule Data Fallthrough Chain**: Each comparison resolves source/target schedule data via:
1. `ScheduleSnapshot` (frozen native data) — if a snapshot ID is provided
2. `IPrimaveraWorkspaceService?` — if Primavera data exists for the project
3. Native `IActivityService` / `IResourceAssignmentService` — fallback when no snapshot or Primavera data

**Storage**: SQLite via EF Core code-first migrations — `PlanovaDbContext` extended with comparison session, result, and snapshot tables. Imported comparison artifact metadata stored on disk under the app-managed project folder.

**Testing**: xUnit in a dedicated `Planova.ScheduleComparison.Tests` project, following the existing module test pattern. Test fixtures use generated schedule data at various sizes.

**Target Platform**: Windows WPF desktop application

**Project Type**: Desktop application module library (`Planova.ScheduleComparison`)

**Performance Benchmarks** (using moderate schedule = ~10,000 activities, 30,000 relationships, 1,000 resources):

| Operation | Target | Benchmark Definition |
|---|---|---|
| Activity comparison (10K activities) | < 3s | Diff activities between two moderate schedules |
| Logic comparison (30K relationships) | < 5s | Diff logic between two moderate schedules |
| Resource comparison (1K resources) | < 2s | Diff resource assignments |
| Critical path comparison | < 8s | CP membership and float comparison from normalized schedule fields |
| Full comparison (all dimensions) | < 15s | Run all comparers on moderate schedules |
| Session load from DB | < 2s | Load persisted session with results |
| UI responsiveness | < 100ms | Grid scrolling, filtering, cell view on moderate data |

**Constraints**: Async by default; CancellationToken support; no UI thread blocking; RTL layout support; consume data through service interfaces, not direct database access from other studios; comparison results are unattributed (no delay cause analysis — reserved for Phase 11)

**Scale/Scope**: Medium enterprise desktop application; support for large schedule datasets and many comparison sessions per project

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Architecture First** | PASS | Clean Architecture with `Planova.ScheduleComparison` domain/application layers, persistence in `Planova.Persistence`, UI in `Planova.UI` |
| **II. MVVM & Fluent UI** | PASS | WPF + Fluent UI WPF + CommunityToolkit.Mvvm; ViewModels under `UI/ViewModels/ScheduleComparison`, Views under `UI/Views/ScheduleComparison` |
| **III. Modular Domain** | PASS | New module with clear contracts; consumes other studio data through service interfaces (optional Primavera via `IServiceProvider.GetService<>`) |
| **IV. Build vs Buy** | PASS | Custom comparers built in-house; comparison logic is owned by Planova and feeds directly into Phase 11 |
| **V. Automation Agnostic** | PASS | No workflow engine or automation designer; comparison is interactive and user-driven |
| **VI. AI Agnostic** | PASS | Phase 10 does not require AI; stays compatible with the platform AI abstraction if future comparison narrative is added |
| **VII. Multilingual First** | PASS | English and Arabic supported for all screens, labels, comparison summaries, and export reports |
| **VIII. Performance** | PASS | Async comparison engine, virtualized diff grids, lazy-loaded comparison results |
| **Tech Standards** | PASS | Uses the platform's established dependency stack and persistence approach |

**No violations — all gates pass without justification needed.**

---

## Phase 10 Objectives

1. Create the `Planova.ScheduleComparison` class library project following the existing modular pattern used by the other studios.
2. Model comparison domain concepts: ComparisonSession, ComparisonResult, ScheduleSnapshot, ComparisonRule.
3. Implement the comparison engine with dedicated comparers for activities, logic, resources, critical path, and float.
4. Implement schedule snapshot capture and restore for native Planova data.
5. Build the Schedule Comparison Studio UI as a dedicated workspace with tabbed internal pages — following the same design pattern as Primavera Studio.
6. Implement comparison export to Excel and PDF, plus structured JSON output consumable by Phase 11.
7. Resolve schedule data through the fallthrough chain: snapshot → Primavera → native services.
8. Replace the `schedule-compare` placeholder nav target with the real studio.
9. Preserve Clean Architecture, MVVM, localization, RTL, and existing platform conventions.

---

## Phase 10 Scope

### In Scope

- **New domain module** (`Planova.ScheduleComparison`) following the same pattern as `Planova.Reporting` and `Planova.Primavera`
- `ComparisonSession` entity — comparison run configuration, source/target metadata, scope, summary, and serialized `ScheduleComparisonResult` JSON for Phase 11 (`ResultJson`)
- `ComparisonResult` entity — queryable per-diff row records per changed entity field, with ChangeType, OldValue, NewValue, MatchConfidence, and Severity (used for UI filtering, paging, and history drill-down)
- `ScheduleSnapshot` entity — frozen native schedule data at a point in time (activities, relationships, resource assignments)
- `ComparisonRule` entity — configurable thresholds and comparison settings
- Four comparison modes: BaselineVsUpdate, UpdateVsUpdate, XerVsXer, AsPlannedVsAsBuilt
- Five comparers: `ActivityComparer`, `LogicComparer`, `ResourceComparer`, `CriticalPathComparer`, `FloatComparer`
- `IScheduleComparisonService` — orchestration, session management, diff queries
- `IScheduleSnapshotService` — capture, restore, list, delete snapshots
- `IComparisonExportService` — export to Excel, PDF, and Phase 11 consumable JSON
- Schedule Comparison Studio ViewModels and Views in `Planova.UI` under a dedicated `ScheduleComparison/` folder
- Tabbed workspace matching Primavera Studio design: Compare, Activities, Logic, Resources, Critical Path, Float, History, Export
- Database tables and EF Core configurations for comparison sessions, results, and snapshots
- Cross-studio nullable injection of `IPrimaveraWorkspaceService?` for Primavera data
- Consumption of `IActivityService` and `IResourceAssignmentService` for native data
- Localization resources in English and Arabic
- Unit and integration tests for comparers, services, persistence, and cross-studio resolution
- Test fixtures with generated schedule data at various sizes

### Out of Scope

- Full delay analysis, TIA, or forensic schedule analysis (Phase 11)
- Delay cause attribution or EOT entitlement computation (Phase 11)
- Automated baseline updates or schedule health scoring
- Multi-user comparison sharing or approval workflows
- Web-based comparison viewer
- Custom comparison rule designer (thresholds are configurable but not drag-drop)
- Full CPM forward/backward pass recalculation — Phase 10 consumes existing CPM/float/critical fields from sources and compares them; a CPM recalculation engine can be a later enhancement
- Live Primavera P6 server connection (Phase 9 handles XER import)

---

## Phase 10 / Phase 11 Boundary

This section defines the explicit contract between Phase 10 (Schedule Comparison) and Phase 11 (Delay Analysis).

### Phase 10 Owns

- **Unattributed schedule diffs** — pure comparison of any two schedule snapshots
- **As-Planned vs As-Built** as a comparison mode (the diff belongs here; Phase 11 attributes delays from it)
- `ComparisonSession.ResultJson` — full serialized diff data stored per session
- Variance reporting — what dates, logic, resources, float, and critical path changed
- Side-by-side Gantt overlay, float impact report, critical path drift visualization

### Phase 11 Will Own

- Consuming `ComparisonSession.ResultJson` from Phase 10 as input
- Attributing each variance to a delay cause, event, or responsibility
- Running method-specific analyses: TIA windows, Collapsed As Built, Impacted As Planned
- Computing EOT entitlement from attributed variances
- Producing claim-ready exhibits, narratives, and forensic reports

### Cross-Phase Contract

```csharp
// Phase 10 produces this on each ComparisonSession
// Stored as ComparisonSession.ResultJson (serialized, immutable once the session completes)
public class ScheduleComparisonResult
{
    public string SchemaVersion { get; set; } = "1.0";
    public Guid SessionId { get; set; }
    public int ProjectId { get; set; }
    public ComparisonMode Mode { get; set; }
    public DateTime ComparedAt { get; set; }

    public ComparisonSourceInfo Source { get; set; } = new();
    public ComparisonSourceInfo Target { get; set; } = new();

    public List<string> IncludedScopes { get; set; } = new();
    public string GeneratedByVersion { get; set; } = "10.0.0.0";

    public List<ActivityDiff> ActivityDiffs { get; set; } = new();
    public List<LogicDiff> LogicDiffs { get; set; } = new();
    public List<ResourceDiff> ResourceDiffs { get; set; } = new();
    public CriticalPathDiff? CriticalPathDiffResult { get; set; }
    public FloatImpactReport? FloatReport { get; set; }
    public ComparisonSummary Summary { get; set; } = new();
}

public class ComparisonSourceInfo
{
    public string SourceKind { get; set; } = string.Empty;       // "Snapshot", "Primavera", "Native"
    public int ProjectId { get; set; }
    public Guid? SnapshotId { get; set; }
    public int? PrimaveraProjectId { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime? CapturedAt { get; set; }
}

// Phase 11 consumes this to attribute delays
```

---

## Neutral ScheduleData Model

All comparison sources (native snapshots, Primavera, live services) are normalized into a uniform `ScheduleData` model in `Planova.ScheduleComparison.Application.Models` before comparison. This decouples the comparers from any external schema and ensures a single matching/diffing pipeline.

### Project Structure

```text
Planova.ScheduleComparison/
└── Application/
    └── Models/
        ├── ScheduleData.cs
        ├── ScheduleActivity.cs
        ├── ScheduleRelationship.cs
        ├── ScheduleResourceAssignment.cs
        └── ScheduleCalendar.cs
```

### ScheduleActivity Fields

| Field | Type | Unit | Description |
|-------|------|------|-------------|
| `MatchKey` | `string` | — | Deterministic key used for matching (see matching rules) |
| `ActivityId` | `string` | — | Native activity ID / code |
| `ActivityName` | `string` | — | Activity description / name |
| `WbsPath` | `string` | — | Full WBS path (e.g. "1.1.2.4") |
| `CalendarName` | `string` | — | Assigned calendar name |
| `Start` | `DateTime?` | — | Early start date |
| `Finish` | `DateTime?` | — | Early finish date |
| `Duration` | `double?` | hours | Original duration in hours |
| `RemainingDuration` | `double?` | hours | Remaining duration in hours |
| `ActualDuration` | `double?` | hours | Actual duration in hours |
| `PercentComplete` | `double?` | % | Percent complete (0–100) |
| `TotalFloat` | `double?` | hours | Total float |
| `FreeFloat` | `double?` | hours | Free float |
| `IsCritical` | `bool` | — | Whether activity is on the critical path |
| `Status` | `string` | — | Status (e.g. "Not Started", "In Progress", "Complete") |
| `ActivityCodes` | `Dictionary<string, string>` | — | Activity code dictionary (code type → value) |
| `ProvenanceId` | `string` | — | Original source identifier (Primavera internal ID, native GUID, etc.) for traceability |

### ScheduleRelationship Fields

| Field | Type | Unit | Description |
|-------|------|------|-------------|
| `PredecessorMatchKey` | `string` | — | Match key of the predecessor activity |
| `SuccessorMatchKey` | `string` | — | Match key of the successor activity |
| `RelationshipType` | `string` | — | "FS", "SS", "FF", "SF" |
| `Lag` | `double` | hours | Lag duration (positive = delay, negative = overlap) |

### ScheduleResourceAssignment Fields

| Field | Type | Unit | Description |
|-------|------|------|-------------|
| `ActivityMatchKey` | `string` | — | Match key of the parent activity |
| `ResourceId` | `string` | — | Resource ID / code |
| `ResourceName` | `string` | — | Resource name |
| `Role` | `string` | — | Assignment role (e.g. "Labor", "Material", "Equipment") |
| `PlannedUnits` | `double?` | hours | Planned units / quantity |
| `ActualUnits` | `double?` | hours | Actual units / quantity |
| `RemainingUnits` | `double?` | hours | Remaining units / quantity |
| `UnitCost` | `decimal?` | currency per unit | Cost per unit |
| `TotalCost` | `decimal?` | currency | Total planned cost |
| `ActualCost` | `decimal?` | currency | Total actual cost |

### ScheduleCalendar Fields

| Field | Type | Description |
|-------|------|-------------|
| `Name` | `string` | Calendar name |
| `IsDefault` | `bool` | Whether this is the project default calendar |
| `WorkWeek` | `List<(DayOfWeek, TimeSpan, TimeSpan)>` | Work week definition (day, start, end) |

### ScheduleData

```csharp
public class ScheduleData
{
    public List<ScheduleActivity> Activities { get; set; } = new();
    public List<ScheduleRelationship> Relationships { get; set; } = new();
    public List<ScheduleResourceAssignment> ResourceAssignments { get; set; } = new();
    public List<ScheduleCalendar> Calendars { get; set; } = new();
    public string? ProjectName { get; set; }
    public DateTime? DataDate { get; set; }
}
```

---

## Deterministic Matching Rules

Matching determines which entities correspond between source and target schedules. Matches are computed once per comparison session and drive all five comparers.

### Activity Match Priority

For each activity in the source schedule, the comparer attempts to find a match in the target schedule using the following priority order:

| Priority | Strategy | When Applicable |
|----------|----------|-----------------|
| 1 | **Primavera internal task ID** (`ScheduleActivity.ProvenanceId`) | Both sides are Primavera-derived (XER import or Primavera snapshot) |
| 2 | **Native activity Guid** (`ScheduleActivity.ProvenanceId`) | Both sides are native Planova snapshots |
| 3 | **Stable activity code / ID** (`ActivityId`) | Always available; exact string match |
| 4 | **WBS path + activity code** (`WbsPath + "|" + ActivityId`) | Fallback when activity IDs alone are ambiguous |
| 5 | **Fuzzy name matching** | Optional, surfaced for user review; enabled only by explicit user choice |

If priority 1–4 produce exactly one candidate, the match is **high-confidence**. If a match is found only by fuzzy name (priority 5), it is flagged as **low-confidence**.

### Relationship Match Priority

1. Predecessor activity match key + successor activity match key + relationship type (FS/SS/FF/SF)
2. Lag is compared as a field, not used for matching

### Resource Assignment Match Priority

1. Activity match key + resource ID/code + role (where available)
2. If role is absent, match by activity match key + resource ID/code only

### Acceptance Criteria

- Every unmatched source entity is surfaced as **Added** with `ScheduleActivity.MatchKey` populated for traceability
- Every unmatched target entity is surfaced as **Removed** with its original key mapped onto the `OldValue` field
- Low-confidence matches (fuzzy name or ambiguous match) are flagged with `MatchConfidence` set to `Low` on the `ComparisonResult` record
- The Activity tab UI includes a **"Show Low-Confidence Only"** filter to help users review uncertain matches

---

## Cross-Studio Integration Pattern

Phase 10 resolves Primavera data through **`IServiceProvider` optional resolution** rather than nullable constructor injection. This avoids wiring failures when Primavera is not registered — `GetService<IPrimaveraWorkspaceService>()` returns `null` cleanly when the service is absent.

> **Registration note**: `App.xaml.cs` currently registers Primavera via `services.AddPlanovaPrimavera()`. When Primera is present, `GetService<IPrimaveraWorkspaceService>()` resolves the workspace service. When absent, it returns `null` and the fallthrough chain skips the Primavera step.

### Schedule Data Resolution

```csharp
public class ScheduleComparisonService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IActivityService _nativeActivityService;
    private readonly IResourceAssignmentService _resourceService;
    private readonly IScheduleSnapshotService _snapshotService;

    public ScheduleComparisonService(
        IServiceProvider serviceProvider,
        IActivityService nativeActivityService,
        IResourceAssignmentService resourceService,
        IScheduleSnapshotService snapshotService)
    {
        _serviceProvider = serviceProvider;
        _nativeActivityService = nativeActivityService;
        _resourceService = resourceService;
        _snapshotService = snapshotService;
    }

    private IPrimaveraWorkspaceService? ResolvePrimavera() =>
        _serviceProvider.GetService<IPrimaveraWorkspaceService>();

    public async Task<ScheduleData> ResolveScheduleAsync(
        int projectId, Guid? snapshotId, int? primaveraProjectId, CancellationToken ct)
    {
        // 1. Frozen snapshot (most specific)
        if (snapshotId.HasValue)
            return await _snapshotService.RestoreSnapshotAsync(snapshotId.Value, ct);

        // 2. Primavera data — resolved via IServiceProvider, returns null cleanly if not registered
        var primavera = ResolvePrimavera();
        if (primaveraProjectId.HasValue && primavera != null
            && await primavera.HasDataAsync(primaveraProjectId.Value, ct))
        {
            var snapshot = await primavera.GetSnapshotAsync(primaveraProjectId.Value, ct);
            // PrimaveraWorkspaceSnapshot (Planova.Primavera.Application.Models) must be
            // mapped into Phase 10's neutral ScheduleData model before comparison.
            return MapPrimaveraSnapshotToScheduleData(snapshot);
        }

        // 3. Native Planova services (fallback)
        return await MapNativeScheduleAsync(projectId, ct);
    }
}
```

### Consumed Interfaces

| Interface | Module | Purpose |
|-----------|--------|---------|
| `IPrimaveraWorkspaceService` | `Planova.Primavera` | Primavera schedule data (optional — resolved via `IServiceProvider.GetService<>`) |
| `IActivityService` | `Planova.Activity` | Native activity data |
| `IResourceAssignmentService` | `Planova.Resource` | Native resource assignments |
| `ICurrentProjectService` | `Planova.Shared` | Active project context |

### Registration

```csharp
// In Planova.ScheduleComparison.Extensions.ServiceCollectionExtensions
// Only schedule comparison application services, comparers, and export services are registered here.
// Repository implementation belongs in Planova.Persistence.Extensions.ServiceCollectionExtensions
// matching the existing persistence pattern. Planova.ScheduleComparison must NOT reference
// Planova.Persistence or EF Core.
public static IServiceCollection AddPlanovaScheduleComparison(this IServiceCollection services)
{
    services.AddScoped<IScheduleComparisonService, ScheduleComparisonService>();
    services.AddScoped<IScheduleSnapshotService, ScheduleSnapshotService>();
    services.AddScoped<IComparisonExportService, ComparisonExportService>();
    return services;
}
```

### Rationale

- Follows the same proven pattern as Phase 8 (`IReportDataProvider<T>`) and Phase 9 (`IPrimaveraWorkspaceService`)
- `IServiceProvider.GetService<>` resolution handles "Primavera unavailable" without nullable constructor injection that would fail if the service is absent
- Fallthrough chain keeps resolution logic in one place (the comparison service)
- No direct DB access — all data flows through service interfaces
- `ScheduleComparisonResult` is the cross-phase contract for Phase 11
- Repository is registered in `Planova.Persistence.Extensions.ServiceCollectionExtensions` alongside all other persistence registrations, keeping Clean Architecture intact

---

## Project Structure

### Documentation

```text
docs/
├── PHASE_10_IMPLEMENTATION_PLAN.md   # This file
├── 02-MASTER_ROADMAP.md              # Phase 10 roadmap entry
├── 04-SYSTEM_ARCHITECTURE.md         # Module placement and dependencies
├── 06-MODULE_CATALOG.md              # Schedule Comparison responsibilities
├── 07-DATABASE_STRATEGY.md           # Comparison data category
├── 08-INTEGRATION_STRATEGY.md        # Integration direction
└── 11-UI_UX_DESIGN_SYSTEM.md         # Dedicated workspace guidance
```

### Source Code

```text
Planova.ScheduleComparison/
├── Domain/
│   ├── Entities/
│   │   ├── ComparisonSession.cs
│   │   ├── ComparisonResult.cs
│   │   ├── ScheduleSnapshot.cs
│   │   └── ComparisonRule.cs
│   ├── Enums/
│   │   ├── ComparisonMode.cs
│   │   ├── ChangeType.cs
│   │   ├── ComparisonScope.cs
│   │   └── MatchConfidence.cs
│   ├── Interfaces/
│   │   ├── IScheduleComparisonService.cs
│   │   ├── IScheduleSnapshotService.cs
│   │   ├── IComparisonExportService.cs
│   │   └── IComparisonRepository.cs
│   └── Constants/
│       └── ComparisonFieldNames.cs
├── Application/
│   ├── Services/
│   │   ├── ScheduleComparisonService.cs
│   │   ├── ScheduleSnapshotService.cs
│   │   └── ComparisonExportService.cs
│   ├── Dto/
│   │   ├── ComparisonSessionDto.cs
│   │   ├── ComparisonSummaryDto.cs
│   │   ├── ActivityDiffDto.cs
│   │   ├── LogicDiffDto.cs
│   │   ├── ResourceDiffDto.cs
│   │   ├── CriticalPathDiffDto.cs
│   │   ├── FloatImpactDto.cs
│   │   └── ScheduleSnapshotDto.cs
│   ├── Models/
│   │   ├── ScheduleComparisonResult.cs
│   │   ├── ComparisonSourceInfo.cs
│   │   ├── ScheduleData.cs
│   │   ├── ScheduleActivity.cs
│   │   ├── ScheduleRelationship.cs
│   │   ├── ScheduleResourceAssignment.cs
│   │   └── ScheduleCalendar.cs
│   ├── Comparers/
│   │   ├── ActivityComparer.cs
│   │   ├── LogicComparer.cs
│   │   ├── ResourceComparer.cs
│   │   ├── CriticalPathComparer.cs
│   │   └── FloatComparer.cs
│   └── Mappings/
│       └── ComparisonMappingProfile.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Planova.ScheduleComparison.csproj

tests/
└── Planova.ScheduleComparison.Tests/
    ├── Domain/
    ├── Application/
    │   ├── Comparers/
    │   │   ├── ActivityComparerTests.cs
    │   │   ├── LogicComparerTests.cs
    │   │   ├── ResourceComparerTests.cs
    │   │   ├── CriticalPathComparerTests.cs
    │   │   └── FloatComparerTests.cs
    │   └── Services/
    │       ├── ScheduleComparisonServiceTests.cs
    │       ├── ScheduleSnapshotServiceTests.cs
    │       └── ComparisonExportServiceTests.cs
    ├── Fixtures/
    │   ├── small-schedule.json
    │   └── moderate-schedule.json
    └── Planova.ScheduleComparison.Tests.csproj

Planova.Persistence/
├── EntityConfigurations/
│   ├── ComparisonSessionConfiguration.cs
│   ├── ComparisonResultConfiguration.cs
│   ├── ScheduleSnapshotConfiguration.cs
│   └── ComparisonRuleConfiguration.cs
└── Repositories/
    └── ComparisonRepository.cs

Planova.UI/
├── ViewModels/
│   └── ScheduleComparison/
│       ├── ScheduleComparisonViewModel.cs
│       ├── CompareViewModel.cs
│       ├── ActivityDiffViewModel.cs
│       ├── LogicDiffViewModel.cs
│       ├── ResourceDiffViewModel.cs
│       ├── CriticalPathDiffViewModel.cs
│       ├── FloatImpactViewModel.cs
│       ├── ComparisonHistoryViewModel.cs
│       └── ComparisonExportViewModel.cs
├── Views/
│   └── ScheduleComparison/
│       ├── ScheduleComparisonView.xaml
│       ├── ScheduleComparisonView.xaml.cs
│       ├── CompareView.xaml
│       ├── CompareView.xaml.cs
│       ├── ActivityDiffView.xaml
│       ├── ActivityDiffView.xaml.cs
│       ├── LogicDiffView.xaml
│       ├── LogicDiffView.xaml.cs
│       ├── ResourceDiffView.xaml
│       ├── ResourceDiffView.xaml.cs
│       ├── CriticalPathDiffView.xaml
│       ├── CriticalPathDiffView.xaml.cs
│       ├── FloatImpactView.xaml
│       ├── FloatImpactView.xaml.cs
│       ├── ComparisonHistoryView.xaml
│       ├── ComparisonHistoryView.xaml.cs
│       ├── ComparisonExportView.xaml
│       └── ComparisonExportView.xaml.cs
└── Converters/
    └── ComparisonColorConverter.cs

Planova.Localization/
└── Resources/
    ├── ComparisonResources.en.resx
    └── ComparisonResources.ar.resx
```

**Structure Decision**: `Planova.ScheduleComparison` follows the same module pattern as `Planova.Reporting` and `Planova.Primavera`. Domain contains entities, enums, constants, and contracts. Application contains the comparison engine, snapshot service, exporters, and comparers. Persistence owns EF Core configuration and repository. UI owns the dedicated workspace and diff viewing surfaces. Localization stays in the shared localization project.

---

## Non-Negotiable Constraints

- Clean Architecture must remain intact: domain logic in `Planova.ScheduleComparison`, persistence in `Planova.Persistence`, UI in `Planova.UI`.
- MVVM remains the UI pattern throughout.
- All comparison and export operations must be asynchronous and cancellation-aware.
- Large schedule comparisons must not block the UI thread.
- Schedule data must be resolved through the fallthrough chain: snapshot → Primavera → native services.
- Other studios' data must be consumed through domain service interfaces, not direct DB queries.
- `IPrimaveraWorkspaceService` must be resolved via `IServiceProvider.GetService<IPrimaveraWorkspaceService>()` — module compiles and functions without Primavera. Constructor injection of a nullable would fail when the service is not registered.
- Comparison sessions must store full result data as JSON (`ScheduleComparisonResult` on `ResultJson`) for Phase 11 consumption. This envelope is immutable once the session completes.
- `ComparisonResult` per-diff rows are persisted in the `ComparisonResults` table for queryable grids, filtering, paging, and history drill-down — separate from `ResultJson`.
- Comparison results must be unattributed — no delay cause analysis, no EOT computation (Phase 11).
- Localization must support English and Arabic, including RTL layouts.
- The studio must be project-gated and only available when a project is active.
- The internal page layout must match Primavera Studio's design: TabControl workspace with `InitializeTabs(IServiceProvider)` pattern, `<StudioName>Tab` inner class, `<StudioName>ViewModel` with `Tabs`/`SelectedTab`, and separate View/ViewModel per tab.
- Snapshot data must survive re-imports and be independently restorable.

---

## Implementation Plan

### Phase 10.1 — Foundation

- Create the `Planova.ScheduleComparison` module project and test project.
- Add all domain entities, enums, constants, and interfaces.
- Add shared DTOs and the `ScheduleComparisonResult` model for Phase 11.
- Register the new module in dependency injection.

### Phase 10.2 — Persistence and Storage + Cross-Studio Contracts

- Add EF Core configurations for ComparisonSession, ComparisonResult, ScheduleSnapshot, and ComparisonRule.
- Extend `PlanovaDbContext` with the new tables.
- Add the `ComparisonRepository` for persistence operations.
- Define app-managed file storage conventions for exported comparison reports.
- Wire nullable `IPrimaveraWorkspaceService?` injection with fallback to native `IActivityService`/`IResourceAssignmentService`.

### Phase 10.3 — Comparison Engine

- Implement `ActivityComparer` — match by deterministic priority rules, diff Start, Finish, Duration, %Complete, Status, Calendar.
- Implement `LogicComparer` — compare predecessor/successor sets using match keys, detect broken/new/type-changed relationships.
- Implement `ResourceComparer` — diff resource assignment units, costs, planned vs actual.
- Implement `CriticalPathComparer` — compare critical path membership and float values from normalized schedule fields; **no forward/backward pass recalculation** (see critical path design note below).
- Implement `FloatComparer` — compute float delta per activity using existing TotalFloat/FreeFloat fields, rank by float loss, highlight negative float.
- Build the orchestration service (`ScheduleComparisonService`) that resolves schedule data and runs selected comparers.
- **Critical path design note**: Phase 10 consumes existing CPM/float/critical fields from native or Primavera sources and compares them. It does not implement a full CPM scheduler. A complete CPM recalculation engine can be a future enhancement.

### Phase 10.4 — Snapshot Service

- Implement `ScheduleSnapshotService` for capturing native Planova schedule data (activities, relationships, resource assignments).
- Implement snapshot restore — deserialize frozen data back into `ScheduleData` for comparison.
- Support listing and deleting snapshots per project.

### Phase 10.5 — Export Service

- Implement comparison export to Excel — reuse `Planova.Excel.Writers.IWorkbookWriter` where its request/data model fits. If comparison multi-sheet export requires custom ClosedXML workbooks, implement that as a detail inside `ComparisonExportService`, not as a new cross-module abstraction.
- Implement comparison export to PDF (QuestPDF) — formatted diff report with summary, tables, and color-coded changes.
- Implement `ExportComparisonResultAsync()` — returns the `ScheduleComparisonResult` model for Phase 11 consumption.
- File storage to `{AppData}/Planova/Projects/{projectId}/Comparisons/{sessionId}/`.
- Export writes to a temp file first, then moves to final destination to avoid partial files on failure.

### Phase 10.6 — Studio UI

- Build the Schedule Comparison Studio workspace following the Primavera Studio design pattern:
  - `ScheduleComparisonView` (TabControl) with `InitializeTabs(IServiceProvider)`
  - `ScheduleComparisonTab` inner class (Header, Content)
  - `ScheduleComparisonViewModel` (Tabs, SelectedTab)
  - Each tab is a separate View + ViewModel resolved from DI
- **Compare tab** — source/target pickers, [Run Comparison] button, summary cards, side-by-side Gantt overlay
- **Activities tab** — color-coded diff grid (Added=Green, Removed=Red, Modified=Yellow), [Show Only Changed] toggle
- **Logic tab** — relationship diff grid, broken links alert
- **Resources tab** — resource assignment diff grid, quantity/cost deltas
- **Critical Path tab** — side-by-side CP list, CP drift detection, CP duration change
- **Float tab** — float delta grid, [Show Negative Only] filter, float histogram
- **History tab** — past sessions list, [Re-open], [Delete]
- **Export tab** — export format selectors, section visibility toggles
- Consistent color scheme for Added/Removed/Modified across all diff tabs

### Phase 10.7 — Navigation and Shell Integration

- Replace the placeholder nav target with the real studio:

```csharp
// Before:
nav.RegisterTarget("schedule-compare", "Schedule Compare", "ArrowSync24", true, true,
    () => CreateEmptyState("ArrowSync24", "Schedule Compare", "Schedule comparison module is coming soon."));

// After — matches Primavera Studio pattern:
nav.RegisterTarget("schedule-compare", "Schedule Compare", "ArrowSync24", true, false, () =>
{
    var view = _serviceProvider.GetRequiredService<ScheduleComparisonView>();
    view.InitializeTabs(_serviceProvider);
    return view;
});
```

- Register all ViewModels and Views in `App.xaml.cs`.
- Call `services.AddPlanovaScheduleComparison()` in DI setup.

### Phase 10.8 — Polish and Verification

- Add Arabic resources and RTL verification.
- Run performance benchmarks against the moderate schedule definition.
- Add unit tests for all five comparers.
- Add integration tests for persistence round-trip and full comparison pipeline.
- Add integration tests for cross-studio nullable injection with/without Primavera.
- Validate shell navigation, gating, and end-to-end comparison workflows.

---

## In Scope Detail

### Comparison Engine

- Four comparison modes: BaselineVsUpdate, UpdateVsUpdate, XerVsXer, AsPlannedVsAsBuilt
- Five comparers: Activity, Logic, Resource, Critical Path, Float
- Configurable comparison scope (all dimensions or selected subset)
- Deterministic activity matching by priority: provenance ID → activity ID → WBS + code → optional fuzzy (see matching rules section)
- Activity diff: Start, Finish, Duration, RemainingDuration, PercentComplete, Status, CalendarId
- Logic diff: predecessor/successor match keys, relationship type (FS/SS/FF/SF), lag duration
- Resource diff: units, planned units, actual units, cost per unit, total cost
- Critical path diff: CP membership and float comparison from normalized schedule fields (no forward/backward pass recalculation)
- Float diff: total float delta, free float delta, rank by float loss
- Severity classification: Critical (>30 day change), Major (7-30 days), Minor (1-7 days), Info (<1 day)
- All comparisons produce `ComparisonResult` rows stored in the `ComparisonResults` table for queryable access, plus a full `ScheduleComparisonResult` envelope stored as `Session.ResultJson` for Phase 11

### Schedule Snapshots

- Capture native Planova schedule data (activities, relationships, resource assignments) as frozen JSON
- Restore snapshots back into `ScheduleData` for comparison
- Snapshots are versioned and labelled (user-defined names)
- Each project can have multiple snapshots
- Snapshots can be deleted independently
- Baseline-linked snapshots preserve the baseline GUID for traceability

### Cross-Studio Data Resolution

- Fallthrough chain: snapshot → Primavera → native services
- Each consumer wraps resolution in a single "resolve schedule" helper method
- Clear logging when a data source is unavailable
- Primavera data provenance is preserved in comparison output
- Native data mapping converts `ActivityDto` → `ScheduleActivity` for uniform comparison

### Export

- Excel export — reuse `Planova.Excel.Writers.IWorkbookWriter` where applicable; custom multi-sheet ClosedXML workbooks implemented inside `ComparisonExportService` as a private detail
- PDF export via QuestPDF (formatted report with summary, tables, color coding)
- Phase 11 consumable JSON export (`ScheduleComparisonResult`)
- Export files stored under `{AppData}/Planova/Projects/{projectId}/Comparisons/{sessionId}/`
- Temp-file-then-move pattern to prevent partial/corrupted export files on failure

### Versioning and Provenance

- Each `ComparisonSession` records source/target labels, timestamps, and metadata (see `ComparisonSourceInfo`)
- `ComparisonResult` per-diff rows include entity match keys and `ComparisonResultId` for traceback and UI drill-down
- `ScheduleComparisonResult` JSON on `Session.ResultJson` is a complete, self-contained diff envelope for Phase 11 with `SchemaVersion` for contract evolution

### Error Handling

- Comparison failures are logged, surfaced in the UI, and recorded on the session (status = `Failed`, error message stored)
- **Failed comparison does not corrupt existing sessions** — a new session is created only after successful comparison
- **Cancelled comparison** (via `CancellationToken`) leaves no completed `ComparisonSession` or marks it `Cancelled` without partial result rows
- Partial comparison results (one comparer fails) are handled per comparer; a failed comparer does not abort the entire session unless unrecoverable
- Snapshot corruption is detected on restore with clear error messaging
- Export failures are logged and recoverable — **failed export does not delete or invalidate the comparison session**
- Export can be retried from the History tab
- All long-running comparison and export operations accept `CancellationToken`

---

## UI Structure — Schedule Comparison Studio

The internal page design follows the Primavera Studio pattern exactly: a `TabControl`-based workspace with each tab as a separate View/ViewModel resolved from DI via `InitializeTabs(IServiceProvider)`.

```
ScheduleComparisonView (TabControl)
├── Tab: Compare
│     CompareView
│     ├── Source Schedule picker (dropdown: snapshots + Primavera projects)
│     ├── Target Schedule picker (same)
│     ├── Scope checkboxes: [x] Activities [x] Logic [x] Resources [x] Critical Path [x] Float
│     ├── [Run Comparison] button
│     ├── Comparison Summary cards
│     │     ├── Activities: N added, M removed, K modified
│     │     ├── Logic: N added, M removed, K modified
│     │     ├── Resources: Δ cost, Δ quantity
│     │     ├── Float: N activities lost float, M gained float
│     │     └── CP: Δ duration, N activities drifted
│     └── Side-by-side Gantt overlay (read-only, colored by ChangeType)
│
├── Tab: Activities
│     ActivityDiffView
│     ├── Filter bar: [ChangeType] [Field] [Severity] [Search]
│     ├── Toggles: [x] Show Only Changed, [x] Group By Type
│     ├── DataGrid: WBS | Name | Old Start | New Start | Start Δ | Old Finish | New Finish | Finish Δ | Duration Δ | % Δ | Status Δ | Calendar Δ
│     ├── Row colors: Green=Added, Red=Removed, Yellow=Modified, Gray=Unchanged
│     ├── Context menu: [View Details], [Copy Row]
│     └── Pagination / virtual scrolling for large schedules
│
├── Tab: Logic
│     LogicDiffView
│     ├── Filter: [ChangeType] [Relationship Type]
│     ├── DataGrid: Pred | Succ | Old Type | New Type | Type Δ | Old Lag | New Lag | Lag Δ
│     ├── Alerts banner: "N broken predecessor references detected"
│     └── Context menu: [Highlight in Schedule]
│
├── Tab: Resources
│     ResourceDiffView
│     ├── Filter: [Resource Type] [Activity]
│     ├── DataGrid: Activity | Resource | Old Units | New Units | Units Δ | Old Cost | New Cost | Cost Δ
│     └── Summary bar: Total cost Δ: +$XX,XXX
│
├── Tab: Critical Path
│     CriticalPathDiffView
│     ├── Side-by-side panels: [Baseline CP] | [Current CP]
│     ├── CP Drift indicators: activities that entered/exited CP
│     ├── Metric: "CP Duration: 120d → 135d (+15d)"
│     └── List: Activities with changed CP membership
│
├── Tab: Float
│     FloatImpactView
│     ├── Filter: [Float Loss] [Float Gain] [Show Negative Only]
│     ├── DataGrid: Activity | Old TF | New TF | Float Δ | Old FF | New FF | FF Δ
│     ├── Sort by: Float Loss (ascending)
│     └── Histogram: float distribution comparison (old vs new)
│
├── Tab: History
│     ComparisonHistoryView
│     ├── DataGrid: Date | Mode | Source → Target | Scope | Summary | Actions
│     ├── [Re-open] — loads session into Compare tab
│     ├── [Delete] — confirm + soft delete
│     └── [Export Result JSON] — Phase 11 consumable
│
└── Tab: Export
      ComparisonExportView
      ├── Section visibility toggles per export
      ├── [Export to Excel]
      ├── [Export to PDF]
      └── [Export ComparisonResult (JSON)] → Phase 11 contract
```

### Tab Control Pattern (matching PrimaveraStudioView)

```csharp
// ScheduleComparisonTab — matches PrimaveraStudioTab exactly
public partial class ScheduleComparisonTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public ScheduleComparisonTab(string header, object content)
    {
        _header = header;
        _content = content;
    }

    public override string ToString() => Header;
}

// ScheduleComparisonViewModel — matches PrimaveraStudioViewModel pattern
public partial class ScheduleComparisonViewModel : ObservableObject
{
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private ScheduleComparisonTab? _selectedTab;

    [ObservableProperty]
    private bool _isProjectActive;

    public ObservableCollection<ScheduleComparisonTab> Tabs { get; } = new();

    // Child ViewModels exposed for tab content
    public CompareViewModel CompareViewModel { get; }
    public ActivityDiffViewModel ActivityDiffViewModel { get; }
    // ... etc.

    public ScheduleComparisonViewModel(
        CompareViewModel compareVm,
        ActivityDiffViewModel activityDiffVm,
        // ... other VMs
        ICurrentProjectService currentProjectService) { ... }

    public async Task LoadAsync(int projectId, CancellationToken ct = default) { ... }
}

// ScheduleComparisonView — matches PrimaveraStudioView pattern
public partial class ScheduleComparisonView : UserControl
{
    public ScheduleComparisonView(ScheduleComparisonViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (ScheduleComparisonViewModel)DataContext;

        var compareView = serviceProvider.GetRequiredService<CompareView>();
        compareView.DataContext = vm.CompareViewModel;
        vm.Tabs.Add(new ScheduleComparisonTab("Compare", compareView));

        var activityDiffView = serviceProvider.GetRequiredService<ActivityDiffView>();
        activityDiffView.DataContext = vm.ActivityDiffViewModel;
        vm.Tabs.Add(new ScheduleComparisonTab("Activities", activityDiffView));

        // ... remaining tabs

        if (vm.Tabs.Count > 0)
            vm.SelectedTab = vm.Tabs[0];
    }
}
```

---

## Navigation Shell Integration

### Replace existing placeholder

**Current** (line 332 of `ShellViewModel.cs`):
```csharp
nav.RegisterTarget("schedule-compare", "Schedule Compare", "ArrowSync24", true, true,
    () => CreateEmptyState("ArrowSync24", "Schedule Compare", "Schedule comparison module is coming soon."));
```

**After Phase 10** (matches Primavera Studio pattern on line 326):
```csharp
nav.RegisterTarget("schedule-compare", "Schedule Compare", "ArrowSync24", true, false, () =>
{
    var view = _serviceProvider.GetRequiredService<ScheduleComparisonView>();
    view.InitializeTabs(_serviceProvider);
    return view;
});
```

Key changes:
- `isPlaceholder`: `true` → `false` (studio is real)
- View factory: `CreateEmptyState(...)` → `ScheduleComparisonView` with `InitializeTabs(_serviceProvider)`

### Already registered in `_studioTargetIds`

```csharp
private readonly List<string> _studioTargetIds = new()
{ "boq", "wbs", "activity", "resource", "cost", "excel-studio", "primavera",
  "schedule-compare", "delay-analysis", "chronology", "knowledge-base",
  "integration-hub", "reports" };
```

No change needed — `schedule-compare` is already in the list.

### DI Registration

**New — in `Planova.ScheduleComparison.Extensions.ServiceCollectionExtensions` (application services only):**
```csharp
public static IServiceCollection AddPlanovaScheduleComparison(this IServiceCollection services)
{
    services.AddScoped<IScheduleComparisonService, ScheduleComparisonService>();
    services.AddScoped<IScheduleSnapshotService, ScheduleSnapshotService>();
    services.AddScoped<IComparisonExportService, ComparisonExportService>();
    return services;
}
```

**In `Planova.Persistence.Extensions.ServiceCollectionExtensions` (repository registration — matching existing persistence pattern):**
```csharp
// Add alongside existing repository registrations
services.AddScoped<IComparisonRepository, ComparisonRepository>();
```

**In `App.xaml.cs`:**
```csharp
services.AddPlanovaScheduleComparison();

// Planova.Persistence.Extensions.ServiceCollectionExtensions is already called,
// which now includes the ComparisonRepository registration.

services.AddTransient<ScheduleComparisonViewModel>();
services.AddTransient<ScheduleComparisonView>();
services.AddTransient<CompareViewModel>();
services.AddTransient<CompareView>();
services.AddTransient<ActivityDiffViewModel>();
services.AddTransient<ActivityDiffView>();
services.AddTransient<LogicDiffViewModel>();
services.AddTransient<LogicDiffView>();
services.AddTransient<ResourceDiffViewModel>();
services.AddTransient<ResourceDiffView>();
services.AddTransient<CriticalPathDiffViewModel>();
services.AddTransient<CriticalPathDiffView>();
services.AddTransient<FloatImpactViewModel>();
services.AddTransient<FloatImpactView>();
services.AddTransient<ComparisonHistoryViewModel>();
services.AddTransient<ComparisonHistoryView>();
services.AddTransient<ComparisonExportViewModel>();
services.AddTransient<ComparisonExportView>();
```

### NavIconConverter

No change needed — `schedule-compare` is already mapped to glyph `\uE895` (ArrowSync24).

---

## Solution Structure Changes

### New project: `Planova.ScheduleComparison`
- **Direct references:** `Planova.Shared` (common DTOs, base types), `Planova.Domain` (Project entity reference)
- **Contract-only references:**
  - `Planova.Primavera` → `IPrimaveraWorkspaceService` (nullable)
  - `Planova.Activity` → `IActivityService`
  - `Planova.Resource` → `IResourceAssignmentService`
- **Infrastructure references:** `Planova.Excel` (for `IWorkbookWriter`), `QuestPDF` (for PDF export)
- **No direct references to:** `Planova.Persistence`, `Planova.UI`, or any EF Core NuGet packages
- **No UI references** — UI only references service interfaces from `Planova.ScheduleComparison.Application`

### `Planova.Persistence` changes
- New entity configurations: `ComparisonSessionConfiguration`, `ComparisonResultConfiguration`, `ScheduleSnapshotConfiguration`, `ComparisonRuleConfiguration`
- New repository implementation: `ComparisonRepository` in `Repositories/`
- New DI registration: `services.AddScoped<IComparisonRepository, ComparisonRepository>()` in `Planova.Persistence.Extensions.ServiceCollectionExtensions`
- New EF Core migration: `AddScheduleComparisonEntities`
- Register new DbSets in `PlanovaDbContext` — `ComparisonSessions`, `ComparisonResults`, `ScheduleSnapshots`, `ComparisonRules`

### `Planova.UI` changes
- New folder: `Views/ScheduleComparison/` — all Schedule Comparison XAML views
- New folder: `ViewModels/ScheduleComparison/` — all Schedule Comparison ViewModels
- `ShellViewModel.cs` — replace `CreateEmptyState` with `ScheduleComparisonView` + `InitializeTabs`
- `App.xaml.cs` — register all Schedule Comparison ViewModels/Views, call `services.AddPlanovaScheduleComparison()`

---

## Workstream Breakdown

### Workstream A: Domain Module Setup
1. Create `Planova.ScheduleComparison` project with folder structure
2. Define all enums: `ComparisonMode`, `ChangeType`, `ComparisonScope`, `MatchConfidence`
3. Define domain entities: `ComparisonSession` (with `ResultJson` for `ScheduleComparisonResult`), `ComparisonResult` (per-diff row), `ScheduleSnapshot`, `ComparisonRule`
4. Define repository interface: `IComparisonRepository` (interface stays in Domain, implementation lives in Planova.Persistence)
5. Define service interfaces: `IScheduleComparisonService`, `IScheduleSnapshotService`, `IComparisonExportService`
6. Add project references to solution

### Workstream B: EF Core Persistence
1. Add DbSets for all Comparison entities to `PlanovaDbContext` — `ComparisonSessions`, `ComparisonResults` (queryable per-diff rows), `ScheduleSnapshots`, `ComparisonRules`
2. Implement entity configurations (all 4 entities)
3. Implement `ComparisonRepository` in `Planova.Persistence/Repositories/` and register in `Planova.Persistence.Extensions.ServiceCollectionExtensions`
4. Generate EF Core migration `AddScheduleComparisonEntities`
5. Verify migration runs clean against existing database

### Workstream C: Comparers
1. Implement `ActivityComparer` — match + diff activity fields
2. Implement `LogicComparer` — relationship set diff
3. Implement `ResourceComparer` — resource assignment diff
4. Implement `CriticalPathComparer` — forward/backward pass + CP diff
5. Implement `FloatComparer` — total float/free float delta
6. Unit test each comparer with known schedule data

### Workstream D: Comparison Services
1. Implement `ScheduleSnapshotService` — capture/restore/list/delete snapshots
2. Implement `ScheduleComparisonService` — orchestrate resolution + comparison + persistence
3. Implement fallthrough chain: snapshot → Primavera → native services; map `PrimaveraWorkspaceSnapshot` → `ScheduleData`
4. Persist individual `ComparisonResult` rows in `ComparisonResults` table for queryable grids and paging
5. Store full `ScheduleComparisonResult` envelope (with `SchemaVersion`, `Source`, `Target`, `IncludedScopes`) as `ResultJson` on each `ComparisonSession` for Phase 11

### Workstream E: Export Service
1. Implement `IComparisonExportService.ExportToExcelAsync` — ClosedXML workbook, one worksheet per diff dimension
2. Implement `IComparisonExportService.ExportToPdfAsync` — QuestPDF formatted diff report
3. Implement `IComparisonExportService.ExportComparisonResultAsync` — returns `ScheduleComparisonResult` for Phase 11
4. File storage to `{AppData}/Planova/Projects/{projectId}/Comparisons/{sessionId}/`

### Workstream F: Studio UI — Shell and Compare Tab
1. Create `ScheduleComparisonView`, `ScheduleComparisonViewModel`, `ScheduleComparisonTab` (Primavera-style pattern)
2. Create `CompareView`/`CompareViewModel` — source/target selectors, scope checkboxes, run button, summary cards
3. Create side-by-side Gantt overlay (read-only, colored by ChangeType)

### Workstream G: Studio UI — Diff Tabs
1. Create `ActivityDiffView`/`ActivityDiffViewModel` — color-coded grid, filters, toggles
2. Create `LogicDiffView`/`LogicDiffViewModel` — relationship diff grid, broken links alert
3. Create `ResourceDiffView`/`ResourceDiffViewModel` — assignment diff grid, cost delta
4. Create `CriticalPathDiffView`/`CriticalPathDiffViewModel` — side-by-side CP, drift indicators
5. Create `FloatImpactView`/`FloatImpactViewModel` — float delta grid, histogram

### Workstream H: Studio UI — History and Export Tabs
1. Create `ComparisonHistoryView`/`ComparisonHistoryViewModel` — past sessions grid, re-open, delete
2. Create `ComparisonExportView`/`ComparisonExportViewModel` — format selection, section toggles, export actions

### Workstream I: Navigation and DI Integration
1. Replace placeholder nav target with real studio registration
2. Register all ViewModels and Views in `App.xaml.cs`
3. Wire `services.AddPlanovaScheduleComparison()` in DI setup (application services only)
4. Wire `IComparisonRepository` → `ComparisonRepository` registration in `Planova.Persistence.Extensions.ServiceCollectionExtensions` (repository belongs in persistence layer)
5. Add localization resource files (`ComparisonResources.en.resx`, `ComparisonResources.ar.resx`)

### Workstream J: Tests
1. Unit tests for all five comparers with known input/output pairs
2. Unit tests for `ScheduleComparisonService` — orchestration, fallthrough chain
3. Unit tests for `ScheduleSnapshotService` — capture/restore round-trip
4. Integration tests for persistence round-trip
5. Integration tests for optional `IPrimaveraWorkspaceService` resolution via `IServiceProvider` (with/without Primavera)
6. Unit tests for `PrimaveraWorkspaceSnapshot` → `ScheduleData` mapping (all fields, including provenance)
7. Unit tests for deterministic matching strategies: provenance ID, activity ID, WBS + code, fuzzy name, and priority ordering
8. Unit tests for low-confidence and unmatched entity flagging in each comparer
9. Unit tests for cancellation (`CancellationToken`) and failed export behavior (session not corrupted, export retryable)
10. Unit tests that `ScheduleComparisonResult` JSON includes `SchemaVersion`, `Source`, `Target`, `IncludedScopes`, and `GeneratedByVersion`
11. Integration tests that persisted `ComparisonResult` rows can be queried, filtered, and paged independently from `ResultJson`
12. Test fixtures with generated schedule data at various sizes

---

## Expected Deliverables

- `Planova.ScheduleComparison` module project
- Domain entities, enums, interfaces, and constants
- Five comparers: Activity, Logic, Resource, Critical Path, Float
- `ScheduleComparisonService`, `ScheduleSnapshotService`, `ComparisonExportService`
- Schedule Comparison Studio ViewModels and Views (8 tabbed workspaces)
- Persistence mappings and repository
- Localization resources (English + Arabic)
- Navigation and DI updates for the `schedule-compare` target
- Excel, PDF, and JSON (Phase 11) export pipelines
- Tests for comparers, services, persistence, and cross-studio integration
- Test fixtures with generated schedule data

---

## Dependencies and Touchpoints

### Primary Dependencies

- Existing project and shell infrastructure from Phase 0
- Project context and active project selection from Phase 1
- Activity Service (`IActivityService`) from Phase 5
- Resource Assignment Service (`IResourceAssignmentService`) from Phase 6
- Primavera Workspace Service (`IPrimaveraWorkspaceService?`) from Phase 9 (nullable)
- Excel export infrastructure (`IWorkbookWriter`) from Phase 2
- Reporting Center cross-studio consumption patterns from Phase 8
- Phase 11 contract definition (`ScheduleComparisonResult` JSON schema)

### Persistence Touchpoints

- New `DbSet` entries in `PlanovaDbContext`
- EF Core configuration classes in `Planova.Persistence/EntityConfigurations`
- Repository implementation in `Planova.Persistence/Repositories`
- File storage for comparison export artifacts

### UI Touchpoints

- Navigation rail entry for `schedule-compare` (already exists as placeholder)
- Studio view registration in shell DI
- Dedicated workspace layout matching Primavera Studio design
- Consistent Fluent UI styling, RTL behavior, and color coding for diffs
- Color scheme: Added (Green), Removed (Red), Modified (Yellow), Unchanged (Gray)

---

## Risks and Mitigations

### Risk: Activity matching between schedules may produce false positives/negatives
**Mitigation:** Support multiple matching strategies (WBS code, activity ID, name with fuzzy matching). Surface unmatched activities clearly. Allow user to override matches.

### Risk: Large schedule comparisons may cause memory pressure
**Mitigation:** Use streaming comparers where possible, virtualized grids for results, and lazy loading for drill-down details. Benchmark against moderate schedule (10K activities).

### Risk: Cross-studio nullable injection leads to scattered null checks
**Mitigation:** Centralize the fallthrough chain in `ScheduleComparisonService.ResolveScheduleAsync()`. Each consumer calls this single method instead of managing null checks.

### Risk: Comparison results become stale as source schedules change
**Mitigation:** Snapshot-based comparisons freeze data at comparison time. The `CapturedAt` timestamp on each snapshot and the `ComparedAt` timestamp on each session make staleness explicit.

### Risk: As-Planned vs As-Built diffs are consumed by Phase 11 but schema may drift
**Mitigation:** Define `ScheduleComparisonResult` with a `SchemaVersion` field from day one. Both phases reference the same shared contract in `Planova.ScheduleComparison.Application.Models`. Version bumps require explicit cross-phase coordination.

### Risk: Primavera data may not map cleanly to native schedule format
**Mitigation:** The fallthrough chain converts both sources into the uniform `ScheduleData` intermediate model. Dedicated mapping logic in `ScheduleComparisonService` translates `PrimaveraWorkspaceSnapshot` → `ScheduleData`, and native `ActivityDto` → `ScheduleActivity`. Field-level mapping is covered by unit tests.

### Risk: Export file write failures (disk full, permissions, network path)
**Mitigation:** Export writes to a temp file first, then moves it to the final destination. Failed export does not invalidate the comparison session. The user can retry export from the History tab. Errors are logged with full path details.

### Risk: Large result sets cause memory pressure from both persisted rows and JSON envelope
**Mitigation:** `ComparisonResult` rows are persisted as queryable entities — UI tabs load only visible rows via paging/skip-take. The `ResultJson` envelope is stored once per session and loaded on-demand. A moderate benchmark (10K activities) must pass before release.

---

## Acceptance Criteria

Phase 10 is complete when all of the following are true:

### Functional — Comparison Modes

- Users can compare Baseline vs Update within a project.
- Users can compare Update vs Update (two snapshots).
- Users can compare XER vs XER (two Primavera imports).
- Users can compare As-Planned vs As-Built (two snapshots).
- Each comparison mode correctly resolves schedule data from the appropriate source.

### Functional — Comparison Dimensions

- Activity diffs show: added, removed, modified activities with field-level changes.
- Logic diffs show: added, removed, modified relationships.
- Resource diffs show: added, removed, modified resource assignments with quantity/cost deltas.
- Critical path diff shows: CP membership changes, CP duration change, drifted activities.
- Float impact shows: total float delta per activity, negative float emergence, float gain/loss ranking.

### Functional — Snapshots

- Users can capture a snapshot of the current native schedule.
- Users can restore a snapshot for comparison.
- Snapshots are listed per project with labels and timestamps.
- Snapshots can be deleted independently.

### Functional — Export

- Comparison results can be exported to Excel (one sheet per dimension).
- Comparison results can be exported to PDF (formatted report with summary and color-coded tables).
- Comparison results can be exported as JSON (`ScheduleComparisonResult`) for Phase 11.
- Export files are stored in the app-managed project folder.

### Functional — UI

- The `schedule-compare` nav item opens the real Schedule Comparison Studio (no longer placeholder).
- The studio is project-gated (disabled when no project is active).
- The internal workspace uses a TabControl with `InitializeTabs(IServiceProvider)` matching Primavera Studio design.
- Each tab has its own View/ViewModel resolved from DI.
- Color coding is consistent across all diff tabs: Green=Added, Red=Removed, Yellow=Modified.
- The studio remains responsive at moderate benchmark size.
- The UI follows the same Fluent UI and MVVM conventions as the rest of the app.

### Functional — Localization

- English and Arabic resources are available for all new screens.
- RTL layouts render correctly.
- All async operations accept and respect `CancellationToken`.

### Failure, Cancellation, and Retry

- Cancelling a comparison via `CancellationToken` leaves no completed `ComparisonSession` or marks it `Cancelled` without partial result rows.
- A failed comparison records `ComparisonStatus.Failed` with an error message but does not corrupt existing sessions or data.
- A failed export does not delete or invalidate the comparison session — the session remains usable and re-exportable.
- Export can be retried from the History tab without re-running the comparison.
- All long-running operations (comparison, export, snapshot capture) accept `CancellationToken` and throw `OperationCanceledException` on cancellation.

### Matching Quality

- Activity matching follows the deterministic priority order (provenance ID → activity ID → WBS + code → optional fuzzy).
- Unmatched source activities are surfaced as **Added** with traceable match keys.
- Unmatched target activities are surfaced as **Removed** with original identifiers.
- Low-confidence matches (fuzzy or ambiguous) are flagged with `MatchConfidence.Low` on the `ComparisonResult` record and are filterable in the UI.
- Relationship matching uses predecessor match key + successor match key + type (lag is compared, not matched).
- Resource assignment matching uses activity match key + resource ID/code + role.

### JSON Contract Integrity

- `ScheduleComparisonResult.ResultJson` includes `SchemaVersion`, `Source`, `Target`, `IncludedScopes`, and `GeneratedByVersion`.
- The JSON envelope is immutable once the session completes.
- `ComparisonResult` per-diff rows are queryable and pageable independently from the JSON envelope.

### Quality

- Automated tests cover all five comparers with known input/output pairs.
- Automated tests cover snapshot capture/restore round-trip.
- Automated tests cover optional `IPrimaveraWorkspaceService` resolution via `IServiceProvider`.
- Automated tests for deterministic matching strategies (all five priority levels).
- Automated tests for low-confidence and unmatched entity flagging.
- Automated tests for cancellation and failed export behavior.
- Automated tests that `ScheduleComparisonResult.ResultJson` contains `SchemaVersion`, `Source`, `Target`, and `IncludedScopes`.
- Automated tests that persisted `ComparisonResult` rows can be queried and paged independently from `ResultJson`.
- Performance benchmarks pass against the moderate schedule definition:
  - Activity comparison: < 3s (10K activities)
  - Logic comparison: < 5s (30K relationships)
  - Resource comparison: < 2s (1K resources)
  - Full comparison: < 15s (all dimensions)
- There are no regressions in existing studio navigation or data entry.
- The implementation remains compliant with Clean Architecture and the platform's modular design.

---

## Definition of Done

- `Planova.ScheduleComparison` exists as a new module project.
- The domain model, interfaces, DTOs, and `ScheduleComparisonResult` contract are implemented.
- EF Core mappings and repository are added in `Planova.Persistence`.
- All five comparers are working and test-covered.
- The schedule snapshot service captures, restores, lists, and deletes snapshots.
- The comparison orchestration service resolves data through the fallthrough chain.
- The Schedule Comparison Studio UI is present in `Planova.UI` with all 8 tabs.
- The internal page design matches Primavera Studio's TabControl + `InitializeTabs` pattern.
- The `schedule-compare` navigation target is no longer a placeholder.
- Export works for Excel, PDF, and Phase 11 consumable JSON.
- Localization is complete for English and Arabic.
- All tests pass and existing phases remain functional.
- Performance benchmarks pass against the defined moderate schedule.

---

## Notes

- Phase 10 is the first consumer of the cross-studio optional service resolution pattern using `IServiceProvider.GetService<>`. It validates that pattern works for non-Primavera modules.
- The `ScheduleComparisonResult` JSON contract is the explicit cross-phase boundary with Phase 11. Keep it stable, versioned with `SchemaVersion`, and immutable once the session completes.
- The comparison engine is intentionally unattributed — no delay cause logic. This keeps Phase 10 focused and prevents scope creep into Phase 11 territory.
- The "As-Planned vs As-Built" comparison mode provides the raw variance data that Phase 11's delay analysis methods (TIA, Window Analysis, Collapsed As Built) will consume.
- All schedule data flows through service interfaces — no direct DB access from the comparison module.
- The Primavera-style tabbed workspace design is a deliberate choice to maintain visual and architectural consistency across all studios.
- Repository implementations belong in `Planova.Persistence`, not in the module's own DI extensions. `Planova.ScheduleComparison` must not reference `Planova.Persistence` or EF Core NuGet packages.
- Excel export reuses `Planova.Excel.Writers.IWorkbookWriter` where its request/data model fits. Custom ClosedXML multi-sheet workbooks are an implementation detail of `ComparisonExportService`, not a new cross-module abstraction.
