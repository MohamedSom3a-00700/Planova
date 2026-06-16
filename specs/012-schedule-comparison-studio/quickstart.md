# Quickstart: Schedule Comparison Studio

**Phase**: 1 — Design & Contracts

## Before You Start

Read these in order:
1. [spec.md](./spec.md) — what the feature does, user stories, acceptance criteria
2. [plan.md](./plan.md) — technical context, constitution check, project structure
3. [research.md](./research.md) — key decisions and rationale
4. [data-model.md](./data-model.md) — entity definitions, relationships, validation
5. [contracts/phase-11-json-contract.md](./contracts/phase-11-json-contract.md) — Phase 11 JSON envelope schema
6. [contracts/cross-module-interfaces.md](./contracts/cross-module-interfaces.md) — interface ownership and resolution rules

## Implementation Order (by Workstream)

### Workstream A: Domain Module Setup
- Create `Planova.ScheduleComparison` project with folder structure
- Define enums: `ComparisonMode`, `ChangeType`, `ComparisonScope`, `MatchConfidence`, `SessionState`
- Define entities: `ComparisonSession`, `ComparisonResult`, `ScheduleSnapshot`, `ComparisonRule`
- Define interfaces: `IComparisonRepository`, `IScheduleComparisonService`, `IScheduleSnapshotService`, `IComparisonExportService`

### Workstream B: EF Core Persistence
- Add DbSets to `PlanovaDbContext`: `ComparisonSessions`, `ComparisonResults`, `ScheduleSnapshots`, `ComparisonRules`
- Create entity configurations for all 4 entities in `Planova.Persistence/EntityConfigurations/`
- Implement `ComparisonRepository` in `Planova.Persistence/Repositories/`
- Register `IComparisonRepository → ComparisonRepository` in `Planova.Persistence.Extensions.ServiceCollectionExtensions`
- Generate EF Core migration

### Workstream C: Comparers
- Implement `ActivityComparer` — deterministic matching priority, field diffing
- Implement `LogicComparer` — predecessor/successor match key + type, lag comparison
- Implement `ResourceComparer` — activity match key + resource ID + role matching
- Implement `CriticalPathComparer` — compare CP membership and float from normalized fields (no forward/backward pass)
- Implement `FloatComparer` — total float and free float delta per activity
- Unit test each comparer

### Workstream D: Comparison Services
- Implement `ScheduleSnapshotService` — capture/restore/list/delete
- Implement `ScheduleComparisonService` — orchestrate resolution + comparison + persistence
- Implement fallthrough chain: snapshot → Primavera (via `GetService<>`) → native
- Map `PrimaveraWorkspaceSnapshot` → `ScheduleData` and native DTOs → `ScheduleData`
- Persist individual `ComparisonResult` rows + full `ScheduleComparisonResult` as `ResultJson`

### Workstream E: Export Service
- Reuse `Planova.Excel.Writers.IWorkbookWriter` for Excel export where data model fits
- Custom multi-sheet ClosedXML implementation inside `ComparisonExportService` for diff dimensions
- QuestPDF for PDF formatted report
- JSON export conforming to [phase-11-json-contract.md](./contracts/phase-11-json-contract.md)
- Temp-file-then-move pattern; failed export does not invalidate session

### Workstream F–H: Studio UI
- Follow Primavera Studio pattern: TabControl + `InitializeTabs(IServiceProvider)` + `ScheduleComparisonTab` inner class
- 8 tabs: Compare, Activities, Logic, Resources, Critical Path, Float, History, Export
- Color coding: Green=Added, Red=Removed, Yellow=Modified, Gray=Unchanged
- RTL support for Arabic; localized resources

### Workstream I: Navigation & DI
- Replace placeholder nav target with real studio
- Register VMs/Views in `App.xaml.cs`
- Wire `AddPlanovaScheduleComparison()` (application services only) in DI
- Wire repository registration in `Planova.Persistence.Extensions`
- Add `ComparisonResources.en.resx` and `ComparisonResources.ar.resx`

### Workstream J: Tests
- Comparer unit tests with known input/output pairs
- Primavera mapping tests (`PrimaveraWorkspaceSnapshot` → `ScheduleData`)
- Deterministic matching strategy tests (all 5 priority levels)
- Low-confidence/unmatched entity flagging tests
- Cancellation and failed export behavior tests
- JSON contract integrity tests (SchemaVersion, Source, Target, IncludedScopes)
- `ComparisonResult` query/paging independence tests
- Integration tests: persistence round-trip, optional Primavera resolution

## Key Rules to Follow

1. **Clean Architecture**: Module references no persistence layer, no EF Core NuGet
2. **Optional Primavera**: Use `IServiceProvider.GetService<>()`, never nullable constructor injection
3. **Repository registration**: In `Planova.Persistence.Extensions`, not in the module
4. **Session lifecycle**: Draft → Running → Completed/Failed/Cancelled (terminal states final)
5. **Comparers use `ScheduleData`**: All sources mapped to neutral model before comparison
6. **CP comparison**: Compare existing fields, do NOT implement forward/backward pass
7. **Excel export**: Reuse `IWorkbookWriter` from Planova.Excel; custom workbooks are a `ComparisonExportService` detail
8. **All async operations**: Accept `CancellationToken`; Run button disabled during execution
9. **Max supported size**: 50K activities / 150K relationships / 5K resources (graceful degradation beyond)
10. **Immutable JSON**: `ResultJson` is never modified once session reaches Completed state
