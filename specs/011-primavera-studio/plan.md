# Implementation Plan: Primavera Studio

**Branch**: `011-primavera-studio` | **Date**: 2026-06-12 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/011-primavera-studio/spec.md`

## Summary

The Primavera Studio is a dedicated workspace for importing, editing, validating, repairing, and exporting Primavera P6 XER data. It serves as a schedule foundation for the wider platform — when Primavera data exists, other studios (Schedule Comparison, Delay Analysis, etc.) consume it through shared service contracts instead of duplicating schedule logic. Key capabilities: XER import with preview, tabbed workspace with virtualized grids for activities/relationships/resources/calendars/codes/baselines/UDFs, on-demand validation and repair, XER export with round-trip raw table preservation, and cross-studio data sharing via nullable direct service injection.

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: WPF, Fluent UI WPF, CommunityToolkit.Mvvm, EF Core 8, SQLite, Microsoft.Extensions.Hosting, Serilog, and existing Planova shared abstractions

**Storage**: SQLite via EF Core code-first migrations — `PlanovaDbContext` extended with Primavera import/export and schedule tables; imported XER files and exported artifacts stored on disk under the app-managed project folder

**Testing**: xUnit in a dedicated `Planova.Primavera.Tests` project, following existing module test pattern. Test fixture XER files stored under `tests/Planova.Primavera.Tests/Fixtures/`

**Target Platform**: Windows WPF desktop application

**Project Type**: Desktop application module library (`Planova.Primavera`)

**Performance Goals**: Import preview <10s, validation <5s, export <10s, workspace open <5s for moderate schedule (10K activities, 30K relationships, 1K resources, 10 calendars, 5 baselines); UI responsiveness <100ms for grid operations

**Constraints**: Async by default; CancellationToken support; no UI thread blocking; background parsing for large XER files; RTL layout support; preserve source provenance; consume data through service interfaces, not direct DB access from other studios

**Scale/Scope**: Medium enterprise desktop application; support for large schedule datasets, multiple baselines, and many imported XER revisions per project

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Architecture First** | PASS | Clean Architecture with `Planova.Primavera` domain/application layers, persistence in `Planova.Persistence`, UI in `Planova.UI` |
| **II. MVVM & Fluent UI** | PASS | WPF + Fluent UI WPF + CommunityToolkit.Mvvm; ViewModels under `UI/ViewModels/Primavera`, Views under `UI/Views/Primavera` |
| **III. Modular Domain** | PASS | New module with clear contracts; other studios consume schedule data through injected domain service interfaces |
| **IV. Build vs Buy** | PASS | Custom XER parser built in-house; may be extracted to NuGet post-stabilization; studio behavior is owned by Planova |
| **V. Automation Agnostic** | PASS | No workflow engine or automation designer; repair and validation are interactive and local |
| **VI. AI Agnostic** | PASS | Phase 9 does not require AI, but module stays compatible with platform AI abstraction |
| **VII. Multilingual First** | PASS | English and Arabic supported for all screens, labels, validation messages, and status summaries |
| **VIII. Performance** | PASS | Async parsing, staged import, virtualized grids, and full-copy baseline model prevent UI blocking |
| **Tech Standards** | PASS | Uses platform's established dependency stack and persistence approach |

**No violations — all gates pass without justification needed.**

## Project Structure

### Documentation (this feature)

```text
specs/011-primavera-studio/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── cross-studio-interfaces.md
└── checklists/
    └── requirements.md  # Spec quality checklist
```

### Source Code (repository root)

```text
Planova.Primavera/
├── Domain/
│   ├── Entities/
│   │   ├── PrimaveraProject.cs
│   │   ├── XerImportSession.cs
│   │   ├── XerExportProfile.cs
│   │   ├── XerRawTable.cs
│   │   ├── PrimaveraActivity.cs
│   │   ├── PrimaveraRelationship.cs
│   │   ├── PrimaveraResourceAssignment.cs
│   │   ├── PrimaveraCalendar.cs
│   │   ├── PrimaveraCode.cs
│   │   ├── PrimaveraBaseline.cs
│   │   ├── PrimaveraUdf.cs
│   │   ├── PrimaveraValidationRule.cs
│   │   ├── PrimaveraValidationIssue.cs
│   │   └── PrimaveraRepairAction.cs
│   ├── Enums/
│   │   ├── PrimaveraEntityType.cs
│   │   ├── PrimaveraImportStatus.cs
│   │   ├── PrimaveraValidationSeverity.cs
│   │   └── PrimaveraRepairStatus.cs
│   ├── Interfaces/
│   │   ├── IPrimaveraImportService.cs
│   │   ├── IPrimaveraExportService.cs
│   │   ├── IPrimaveraWorkspaceService.cs
│   │   ├── IPrimaveraValidationService.cs
│   │   ├── IPrimaveraRepairService.cs
│   │   ├── IPrimaveraImportRepository.cs
│   │   ├── IPrimaveraWorkspaceRepository.cs
│   │   ├── IPrimaveraValidationRepository.cs
│   │   └── IPrimaveraRepairRepository.cs
│   └── Constants/
│       └── XerFieldNames.cs
├── Application/
│   ├── Services/
│   │   ├── PrimaveraImportService.cs
│   │   ├── PrimaveraExportService.cs
│   │   ├── PrimaveraWorkspaceService.cs
│   │   ├── PrimaveraValidationService.cs
│   │   └── PrimaveraRepairService.cs
│   ├── Dto/
│   │   ├── PrimaveraProjectDto.cs
│   │   ├── XerImportPreviewDto.cs
│   │   ├── XerImportResultDto.cs
│   │   ├── PrimaveraActivityDto.cs
│   │   ├── PrimaveraRelationshipDto.cs
│   │   ├── PrimaveraResourceDto.cs
│   │   ├── PrimaveraCalendarDto.cs
│   │   ├── PrimaveraCodeDto.cs
│   │   ├── PrimaveraBaselineDto.cs
│   │   ├── PrimaveraUdfDto.cs
│   │   ├── PrimaveraValidationIssueDto.cs
│   │   └── PrimaveraRepairActionDto.cs
│   ├── Parsers/
│   │   └── XerParser.cs
│   ├── Writers/
│   │   └── XerWriter.cs
│   ├── Mappings/
│   │   └── PrimaveraMappingProfile.cs
│   └── Models/
│       └── PrimaveraWorkspaceSnapshot.cs
├── Background/
│   └── PrimaveraImportHostedService.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Planova.Primavera.csproj

tests/
└── Planova.Primavera.Tests/
    ├── Domain/
    ├── Application/
    ├── Fixtures/
    │   ├── small.xer
    │   ├── medium.xer
    │   └── moderate.xer
    └── Planova.Primavera.Tests.csproj

Planova.Persistence/
├── EntityConfigurations/
│   ├── PrimaveraProjectConfiguration.cs
│   ├── XerImportSessionConfiguration.cs
│   ├── XerExportProfileConfiguration.cs
│   ├── XerRawTableConfiguration.cs
│   ├── PrimaveraActivityConfiguration.cs
│   ├── PrimaveraRelationshipConfiguration.cs
│   ├── PrimaveraResourceAssignmentConfiguration.cs
│   ├── PrimaveraCalendarConfiguration.cs
│   ├── PrimaveraCodeConfiguration.cs
│   ├── PrimaveraBaselineConfiguration.cs
│   ├── PrimaveraUdfConfiguration.cs
│   ├── PrimaveraValidationRuleConfiguration.cs
│   ├── PrimaveraValidationIssueConfiguration.cs
│   └── PrimaveraRepairActionConfiguration.cs
└── Repositories/
    ├── PrimaveraImportRepository.cs
    ├── PrimaveraWorkspaceRepository.cs
    ├── PrimaveraValidationRepository.cs
    └── PrimaveraRepairRepository.cs

Planova.UI/
├── ViewModels/
│   └── Primavera/
│       ├── PrimaveraStudioViewModel.cs
│       ├── PrimaveraImportViewModel.cs
│       ├── PrimaveraWorkspaceViewModel.cs
│       ├── PrimaveraActivitiesViewModel.cs
│       ├── PrimaveraRelationshipsViewModel.cs
│       ├── PrimaveraResourcesViewModel.cs
│       ├── PrimaveraCalendarsViewModel.cs
│       ├── PrimaveraCodesViewModel.cs
│       ├── PrimaveraBaselinesViewModel.cs
│       ├── PrimaveraUdfsViewModel.cs
│       ├── PrimaveraValidationViewModel.cs
│       └── PrimaveraRepairViewModel.cs
├── Views/
│   └── Primavera/
│       ├── PrimaveraStudioView.xaml
│       ├── PrimaveraImportView.xaml
│       ├── PrimaveraWorkspaceView.xaml
│       ├── PrimaveraActivitiesView.xaml
│       ├── PrimaveraRelationshipsView.xaml
│       ├── PrimaveraResourcesView.xaml
│       ├── PrimaveraCalendarsView.xaml
│       ├── PrimaveraCodesView.xaml
│       ├── PrimaveraBaselinesView.xaml
│       ├── PrimaveraUdfsView.xaml
│       ├── PrimaveraValidationView.xaml
│       └── PrimaveraRepairView.xaml
└── Converters/
    └── PrimaveraStatusConverter.cs

Planova.Localization/
└── Resources/
    ├── PrimaveraResources.en.resx
    └── PrimaveraResources.ar.resx
```

**Structure Decision**: `Planova.Primavera` follows the same module pattern as the other studios. Domain contains entities, enums, constants, and contracts. Application contains the import/export pipeline, workspace services, validation, and repair. Persistence owns EF Core configuration and repositories. UI owns the dedicated workspace and editor surfaces. Localization stays in the shared localization project. Removed `IPrimaveraIntegrationService`/`IPrimaveraSourceResolver` — cross-studio consumption uses direct service injection (same pattern as Phase 8 Reporting Center). Added `XerRawTable` entity for round-trip preservation of unsupported XER tables.

## Complexity Tracking

Not applicable — no Constitution Check violations.

## Phase 0: Research

No unresolved NEEDS CLARIFICATION markers exist in the spec. The spec was validated through `/speckit.clarify` with 3 clarifications resolved (entity matching on re-import, import crash recovery, audit logging scope). All technical context is known from the Phase 9 implementation plan and constitution.

### Research Tasks

1. **XER parser design**: Research Primavera P6 XER format specifications for supported table types (CALENDAR, PROJECT, TASK, TASKPRED, TASKRSRC, RSPROJECT, RSOURCE, RCATTYPE, RCATVAL, PROJECTCODE, PROJCODECAT, PROJCODEVAL, UDFTYPE, UDFVALUE, PROJECTBASELINE, TASKUDF). Define parser tolerance for optional fields and version-specific differences across P6 versions 6–22.

2. **Raw table round-trip strategy**: Design raw staging storage for unsupported XER tables (RISK, DOCUMENT, NOTE, etc.) — serialized column+row data re-emitted verbatim on export.

3. **Performance benchmark validation**: Verify moderate schedule definition (10K activities, 30K relationships, 1K resources, 10 calendars, 5 baselines) against target metrics (import <10s, validation <5s, export <10s, workspace open <5s, grid operations <100ms).

4. **Cross-studio contract review**: Review existing Phase 8 `IReportDataProvider<T>` pattern for consistency with `IPrimaveraWorkspaceService` nullable injection approach.

## Phase 1: Design & Contracts

### Data Model

Extracted from spec Key Entities:

1. **PrimaveraProject**: Imported project metadata — source file name, import timestamp, project name from XER, active status
2. **XerImportSession**: Staged import transaction — status (Previewing/Committed/Failed), source file metadata, validation summary, row counts
3. **XerExportProfile**: Export preferences — entity selection, raw table preservation flag, output path
4. **XerRawTable**: Raw unsupported table data — table name, column headers, serialized row data
5. **PrimaveraActivity**: Schedule activity from TASK table — ID, WBS, dates, durations, status, UDF values
6. **PrimaveraRelationship**: FS/SS/FF/SF link between activities with lag duration
7. **PrimaveraResourceAssignment**: Resource allocation — resource ID, units, rates, cost accounts
8. **PrimaveraCalendar**: Work/non-work calendar — working days, shifts, exceptions
9. **PrimaveraCode**: Code type with values for categorization/filtering
10. **PrimaveraBaseline**: Frozen schedule snapshot — duplicated entities keyed by BaselineId + BaselineVersionNumber
11. **PrimaveraUdf**: User-defined field definitions and values attached to entities
12. **PrimaveraValidationRule**: Registered rule — name, description, severity, enabled/disabled
13. **PrimaveraValidationIssue**: Detected issue — severity, entity reference, rule violated, suggested fix
14. **PrimaveraRepairAction**: Applied repair — problem, resolution, target entity, user, timestamp

### Contracts

- `IPrimaveraWorkspaceService` — CRUD and editor operations, schedule snapshots for consumers
- `IPrimaveraImportService` — XER parsing and staging
- `IPrimaveraExportService` — workspace-to-XER export
- `IPrimaveraValidationService` — integrity checks
- `IPrimaveraRepairService` — repair suggestions and application

Cross-studio consumption pattern: nullable injection of `IPrimaveraWorkspaceService?` with consumer-owned fallback logic (same as Phase 8 `IReportDataProvider<T>`).

### Project Structure (above)

Complete project layout with module project, test project, persistence configurations, UI ViewModels/Views, localization resources.

### Agent Context

Update `AGENTS.md` to reference `specs/011-primavera-studio/plan.md` between SPECKIT markers.
