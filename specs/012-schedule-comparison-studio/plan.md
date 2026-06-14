# Implementation Plan: Schedule Comparison Studio

**Branch**: `011-primavera-studio` | **Date**: 2026-06-14 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/012-schedule-comparison-studio/spec.md`

## Summary

Deliver the Schedule Comparison Studio — a dedicated WPF workspace for comparing two schedule snapshots side-by-side across activities, logic, resources, critical path, and float, with results persisted as both queryable per-diff rows and a versioned JSON envelope for Phase 11 delay analysis. The studio follows Clean Architecture with a new `Planova.ScheduleComparison` module, consumes existing `Planova.Primavera` data via `IServiceProvider.GetService<>`, and reuses `Planova.Excel` for export where applicable.

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: WPF, Fluent UI WPF, CommunityToolkit.Mvvm, EF Core 8, SQLite, Microsoft.Extensions.Hosting, Serilog, ClosedXML, QuestPDF, and existing Planova shared abstractions

**Storage**: SQLite via EF Core Code First — `PlanovaDbContext` extended with `ComparisonSessions`, `ComparisonResults`, `ScheduleSnapshots`, `ComparisonRules` tables. Export artifacts written to `{AppData}/Planova/Projects/{projectId}/Comparisons/{sessionId}/`.

**Testing**: xUnit in `Planova.ScheduleComparison.Tests`, following the existing module test pattern. Test fixtures with generated schedule data at various sizes (small, moderate=10K activities, max=50K activities).

**Target Platform**: Windows WPF desktop application

**Project Type**: Desktop application module library (`Planova.ScheduleComparison`)

**Performance Goals**: Full comparison (all 5 dimensions) of moderate schedule in <15s; tab switch <100ms; Excel export <30s

**Constraints**: Async by default with `CancellationToken` support; no UI thread blocking; RTL layout support; consume data through service interfaces (no direct DB access from comparison module); comparison results are unattributed (no delay cause analysis — Phase 11); session lifecycle: Draft → Running → Completed/Failed/Cancelled

**Scale/Scope**: Medium enterprise desktop application; hard max 50K activities / 150K relationships / 5K resources; supports many comparison sessions per project

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Architecture First** | PASS | `Planova.ScheduleComparison` domain/application layer, persistence in `Planova.Persistence`, UI in `Planova.UI`. No persistence or EF Core dependency in the module. |
| **II. MVVM & Fluent UI** | PASS | WPF + Fluent UI WPF + CommunityToolkit.Mvvm; ViewModels under `Planova.UI/ViewModels/ScheduleComparison/`, Views under `Planova.UI/Views/ScheduleComparison/`. |
| **III. Modular Domain** | PASS | New module with clear contracts; consumes other studio data through service interfaces (optional Primavera via `IServiceProvider.GetService<>`). |
| **IV. Build vs Buy** | PASS | Custom comparers built in-house; comparison logic owned by Planova and feeds directly into Phase 11. |
| **V. Automation Agnostic** | PASS | No workflow engine or automation designer; comparison is interactive and user-driven. |
| **VI. AI Agnostic** | PASS | Phase 10 does not require AI; stays compatible with Semantic Kernel abstraction if future comparison narrative is added. |
| **VII. Multilingual First** | PASS | English and Arabic for all screens, labels, comparison summaries, and export reports. |
| **VIII. Performance** | PASS | Async comparison engine, virtualized diff grids, lazy-loaded comparison results. |
| **Tech Standards** | PASS | Uses the platform's established dependency stack and persistence approach. |

**No violations — all gates pass without justification needed.**

## Project Structure

### Documentation (this feature)

```text
specs/012-schedule-comparison-studio/
├── plan.md              # This file
├── research.md          # Phase 0 output — key decisions consolidated
├── data-model.md        # Phase 1 output — entity definitions and relationships
├── quickstart.md        # Phase 1 output — onboarding for implementers
├── contracts/           # Phase 1 output — cross-phase contracts
│   └── ...              # (if external interface contracts needed)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

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
│       ├── ScheduleComparisonView.xaml / .xaml.cs
│       ├── CompareView.xaml / .xaml.cs
│       ├── ActivityDiffView.xaml / .xaml.cs
│       ├── LogicDiffView.xaml / .xaml.cs
│       ├── ResourceDiffView.xaml / .xaml.cs
│       ├── CriticalPathDiffView.xaml / .xaml.cs
│       ├── FloatImpactView.xaml / .xaml.cs
│       ├── ComparisonHistoryView.xaml / .xaml.cs
│       └── ComparisonExportView.xaml / .xaml.cs
└── Converters/
    └── ComparisonColorConverter.cs

Planova.Localization/
└── Resources/
    ├── ComparisonResources.en.resx
    └── ComparisonResources.ar.resx

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
```

**Structure Decision**: `Planova.ScheduleComparison` follows the same module pattern as `Planova.Reporting` and `Planova.Primavera`. Domain contains entities, enums, contracts, and constants. Application contains the comparison engine, snapshot service, exporters, the neutral `ScheduleData` model, and comparers. Persistence (in `Planova.Persistence`) owns EF Core configuration and repository. UI owns the dedicated workspace and diff viewing surfaces. Localization stays in the shared localization project.

## Complexity Tracking

> **No violations — this section is not applicable.**
