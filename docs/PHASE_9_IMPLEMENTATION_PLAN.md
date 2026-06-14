# Planova Phase 9 Implementation Plan

**Phase**: 9 - Primavera Studio

**Date**: 2026-06-12

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
[docs/PHASE_8_IMPLEMENTATION_PLAN.md](./PHASE_8_IMPLEMENTATION_PLAN.md)

---

## Summary

Phase 9 delivers the **Primavera Studio** - a dedicated workspace for importing, editing, validating, repairing, and exporting Primavera P6 XER data. Unlike a narrow file-conversion utility, the studio acts as a schedule foundation for the wider platform: when Primavera data exists, other studios can consume it through shared service contracts instead of duplicating schedule logic.

The studio introduces:

1. **XER import and export** - parse Primavera P6 XER files, preview imported content, persist it, and export it back out.
2. **Primavera workspace editing** - activities, relationships, resources, calendars, codes, baselines, UDFs, and validation in a dedicated workspace.
3. **Repair and validation workflows** - detect broken references, missing calendars, invalid logic, duplicated definitions, and schedule integrity issues.
4. **Cross-studio data sharing** - expose Primavera-derived schedule data to Activity, Resource, WBS, Cost, Reporting, Schedule Comparison, Delay Analysis, Claims, Chronology, and Analytics whenever it is available.
5. **Project-gated studio navigation** - the existing `primavera` nav target becomes a real studio workspace, consistent with the other major modules.

Phase 9 is additive. It must not break native data entry in the other studios. Instead, it provides an optional, authoritative schedule source that can seed and enrich the rest of the application.

**UX Model**: The Primavera Studio and Activity Studio (Phase 5) are fully independent. Users choose which to use. No automatic bridging or UI changes in the Activity Studio. Native data entry remains completely valid when Primavera data is absent.

---

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: WPF, Fluent UI WPF, CommunityToolkit.Mvvm, EF Core 8, SQLite, Microsoft.Extensions.Hosting, Serilog, ClosedXML, QuestPDF, DocumentFormat.OpenXml, Semantic Kernel, and the existing Planova shared abstractions

**XER Parser Strategy**: Build a focused custom parser/writer for the supported XER table types (projects, activities, relationships, resources, calendars, codes, baselines, UDFs, headers). Unsupported XER table types are preserved as raw staging data (stored as serialized rows) during import and re-emitted verbatim during export to maintain round-trip fidelity. After Phase 9 stabilizes, the parser may be extracted into a standalone NuGet package for community use and independent versioning.

**Supported XER Tables**:
- `CALENDAR`, `PROJECT`, `TASK` (activities), `TASKPRED` (relationships), `TASKRSRC` (resource assignments), `RSPROJECT` (resource codes), `RSOURCE` (resources), `RCATTYPE`, `RCATVAL` (resource code types/values), `PROJECTCODE`, `PROJCODECAT`, `PROJCODEVAL` (project code types/values), `UDFTYPE`, `UDFVALUE` (UDFs), `PROJECTBASELINE`, `TASKUDF` (task-level UDFs)
- Unsupported tables (RISK, DOCUMENT, NOTE, etc.) are read as raw column+row data, stored per import session, and re-emitted on export.

**Baseline Storage Model**: Full copy per baseline. Each baseline creates duplicate entries for activities, relationships, resource assignments, and calendar overrides keyed by `BaselineId` + `BaselineVersionNumber`. This enables simple queries (no reconstruction needed), consistent performance regardless of baseline age, and clean EF Core relationships. Storage cost is accepted as a trade-off for query simplicity.

**Storage**: SQLite via EF Core code-first migrations - `PlanovaDbContext` extended with Primavera import/export and schedule tables; imported XER files and exported artifacts stored on disk under the app-managed project folder

**Testing**: xUnit in a dedicated `Planova.Primavera.Tests` project, following the existing module test pattern. Test fixture XER files (real-world samples at various sizes) are stored under `tests/Planova.Primavera.Tests/Fixtures/`.

**Target Platform**: Windows WPF desktop application

**Project Type**: Desktop application module library (`Planova.Primavera`)

**Performance Benchmarks** (using the defined "moderate schedule" = ~10,000 activities, 30,000 relationships, 1,000 resources, 10 calendars, 5 baselines):

| Operation | Target | Benchmark Definition |
|---|---|---|
| XER import preview | < 10s | Parse a moderate XER file and show preview grid |
| Validation | < 5s | Run all validation rules against moderate schedule |
| XER export | < 10s | Export current workspace state to XER file |
| Workspace open | < 5s | Load Primavera Studio workspace with moderate data |
| Cross-studio read resolution | < 2s | Another studio requests schedule data via shared contracts |
| UI responsiveness | < 100ms | Grid scrolling, filtering, cell edit response on moderate data |

**Constraints**: Async by default; CancellationToken support; no UI thread blocking; background parsing for large XER files; RTL layout support; preserve source provenance; consume data through service interfaces, not direct database access from other studios

**Scale/Scope**: Medium enterprise desktop application; support for large schedule datasets, multiple baselines, and many imported XER revisions per project

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Architecture First** | PASS | Clean Architecture with `Planova.Primavera` domain/application layers, persistence in `Planova.Persistence`, UI in `Planova.UI` |
| **II. MVVM & Fluent UI** | PASS | WPF + Fluent UI WPF + CommunityToolkit.Mvvm; ViewModels under `UI/ViewModels/Primavera`, Views under `UI/Views/Primavera` |
| **III. Modular Domain** | PASS | New module with clear contracts; other studios consume schedule data through injected domain service interfaces |
| **IV. Build vs Buy** | PASS | Custom XER parser built in-house; may be extracted to NuGet post-stabilization; studio behavior is owned by Planova |
| **V. Automation Agnostic** | PASS | No workflow engine or automation designer; repair and validation are interactive and local |
| **VI. AI Agnostic** | PASS | Phase 9 does not require AI, but the module stays compatible with the platform AI abstraction if future schedule assistance is added |
| **VII. Multilingual First** | PASS | English and Arabic supported for all screens, labels, validation messages, and status summaries |
| **VIII. Performance** | PASS | Async parsing, staged import, virtualized grids, and full-copy baseline model prevent UI blocking |
| **Tech Standards** | PASS | Uses the platform's established dependency stack and persistence approach |

**No violations - all gates pass without justification needed.**

---

## Phase 9 Objectives

1. Create the `Planova.Primavera` class library project following the existing modular pattern used by the other studios.
2. Model Primavera domain concepts for XER projects, activities, relationships, resources, calendars, codes, baselines, UDFs, validation results, and repair actions.
3. Implement XER import with preview, validation, conflict reporting, and persistence into the Planova database.
4. Implement XER export so the current Primavera workspace state can be written back to XER.
5. Build the Primavera Studio UI as a dedicated workspace, not a dialog or wizard.
6. Add repair tools for common schedule integrity problems.
7. Add validation tools that can be run on demand.
8. Publish shared domain service interfaces (`IPrimaveraWorkspaceService`, etc.) so other studios can inject and consume Primavera data when available.
9. Keep the `primavera` navigation target project-gated and wired into the shell as a first-class studio.
10. Preserve Clean Architecture, MVVM, localization, RTL, and existing platform conventions.

---

## Phase 9 Scope

### In Scope

- **New domain module** (`Planova.Primavera`) following the same pattern as `Planova.Cost`, `Planova.Resource`, and `Planova.Reporting`
- `PrimaveraProject` entity for imported Primavera project metadata and source tracking
- `XerImportSession` entity for staged imports, validation status, warnings, and commit state
- `XerExportProfile` entity for export preferences and round-trip settings
- `PrimaveraActivity`, `PrimaveraRelationship`, `PrimaveraResourceAssignment`, `PrimaveraCalendar`, `PrimaveraCode`, `PrimaveraBaseline`, and `PrimaveraUdf` entities
- `PrimaveraValidationRule` and `PrimaveraValidationIssue` entities for validation and repair results
- `PrimaveraRepairAction` entity for user-approved fixes and auditability
- `IPrimaveraImportService` for parsing and staging XER files
- `IPrimaveraExportService` for exporting workspace state to XER
- `IPrimaveraWorkspaceService` for CRUD and editor operations inside the studio
- `IPrimaveraValidationService` for integrity checks and diagnostics
- `IPrimaveraRepairService` for repair suggestions and applied fixes
- Primavera Studio ViewModels and Views in `Planova.UI` under a dedicated `Primavera/` folder
- Import wizard surface only where it supports preview and commit, not as a replacement for the full workspace
- Database tables and EF Core configurations for Primavera import/export state and schedule data
- Public domain service interfaces for cross-studio consumption (other studios inject `IPrimaveraWorkspaceService` directly — no intermediate integration service or adapter layer)
- Localization resources in English and Arabic
- Unit and integration tests for import, validation, repair, export, and cross-studio resolution
- Test fixture XER files in `tests/Planova.Primavera.Tests/Fixtures/`
- Round-trip preservation of unsupported XER table types as raw staging data

### Out of Scope

- Full Primavera P6 parity for every advanced enterprise feature
- Direct live connection to an external Primavera server or cloud service
- Bi-directional sync with a remote Primavera database
- Multi-user collaboration or approval workflows
- Web-based Primavera viewer
- Automatic cloud backup/synchronization
- Claim drafting or forensic schedule analysis as a standalone feature set
- UI changes in Activity Studio or other studios (fully independent coexistence)

---

## Cross-Studio Integration Pattern

This section replaces the earlier `IPrimaveraIntegrationService` + `IPrimaveraSourceResolver` approach with the same **direct service injection** pattern already proven by Phase 8 (Reporting Center).

### How It Works

Other studios consume Primavera data by injecting `Planova.Primavera`'s public domain service interfaces directly — the same way Reporting injects `IActivityService` and `IResourceAssignmentService`. No intermediate integration layer, no source resolver, no adapter project.

```csharp
// Example: Schedule Comparison Studio (Phase 10) consuming Primavera activities
public class ScheduleComparisonService
{
    private readonly IPrimaveraWorkspaceService? _primaveraWorkspace;
    private readonly IActivityService _nativeActivityService;

    public ScheduleComparisonService(
        IPrimaveraWorkspaceService? primaveraWorkspace,
        IActivityService nativeActivityService)
    {
        _primaveraWorkspace = primaveraWorkspace;
        _nativeActivityService = nativeActivityService;
    }

    public async Task<ScheduleData> GetScheduleDataAsync(int projectId, CancellationToken ct)
    {
        if (_primaveraWorkspace != null && await _primaveraWorkspace.HasDataAsync(projectId, ct))
            return await _primaveraWorkspace.GetScheduleSnapshotAsync(projectId, ct);
        return await _nativeActivityService.GetScheduleAsync(projectId, ct);
    }
}
```

### Registration

In the DI setup, `IPrimaveraWorkspaceService` is registered only when the Primavera module is present:

```csharp
// In Planova.Primavera.Extensions.ServiceCollectionExtensions
public static IServiceCollection AddPlanovaPrimavera(this IServiceCollection services)
{
    services.AddScoped<IPrimaveraImportService, PrimaveraImportService>();
    services.AddScoped<IPrimaveraExportService, PrimaveraExportService>();
    services.AddScoped<IPrimaveraWorkspaceService, PrimaveraWorkspaceService>();
    services.AddScoped<IPrimaveraValidationService, PrimaveraValidationService>();
    services.AddScoped<IPrimaveraRepairService, PrimaveraRepairService>();
    return services;
}
```

Consumer modules use `IPrimaveraWorkspaceService?` (nullable injection) so they compile and function even when the Primavera module assembly is not loaded. The fallback logic lives in each consumer, not in a central resolver.

### Rationale

- Follows the same proven pattern as `IReportDataProvider<T>` in Phase 8
- Eliminates a dedicated integration service and source resolver that would need maintenance
- Keeps fallback decisions local to each consumer (they know their domain best)
- No direct DB access — all data flows through service interfaces
- Nullable injection naturally handles "Primavera unavailable" without a resolver abstraction

---

## Project Structure

### Documentation

```text
docs/
├── PHASE_9_IMPLEMENTATION_PLAN.md   # This file
├── 02-MASTER_ROADMAP.md             # Phase 9 roadmap entry
├── 04-SYSTEM_ARCHITECTURE.md        # Module placement and dependencies
├── 06-MODULE_CATALOG.md             # Primavera Studio responsibilities
├── 07-DATABASE_STRATEGY.md          # Primavera data category
├── 08-INTEGRATION_STRATEGY.md       # Integration direction and future interoperability
└── 11-UI_UX_DESIGN_SYSTEM.md        # Dedicated workspace guidance
```

### Source Code

```text
Planova.Primavera/
├── Domain/
│   ├── Entities/
│   │   ├── PrimaveraProject.cs
│   │   ├── XerImportSession.cs
│   │   ├── XerExportProfile.cs
│   │   ├── XerRawTable.cs                # Raw staging for unsupported tables
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
    ├── Fixtures/                         # Test XER files at various sizes
    │   ├── small.xer                     # ~100 activities
    │   ├── medium.xer                    # ~2,000 activities
    │   └── moderate.xer                  # ~10,000 activities
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

**Structure Decision**: `Planova.Primavera` follows the same module pattern as the other studios. Domain contains entities, enums, constants, and contracts. Application contains the import/export pipeline, workspace services, validation, and repair. Persistence owns EF Core configuration and repositories. UI owns the dedicated workspace and editor surfaces. Localization stays in the shared localization project.

**Key Structural Changes from Original**:
- Removed `IPrimaveraIntegrationService`, `IPrimaveraSourceResolver`, `PrimaveraIntegrationService`, `PrimaveraSourceResolver`, and `XerConflictResolutionMode` — cross-studio consumption uses direct service injection
- Added `XerRawTable` entity for round-trip preservation of unsupported XER tables
- Renamed `PrimaveraResource` to `PrimaveraResourceAssignment` to avoid confusion with `Planova.Resource`'s domain
- Added test fixtures directory for versioned XER sample files

---

## Non-Negotiable Constraints

- Clean Architecture must remain intact: domain logic in `Planova.Primavera`, persistence in `Planova.Persistence`, UI in `Planova.UI`.
- MVVM remains the UI pattern throughout.
- All import/export and validation operations must be asynchronous and cancellation-aware.
- Large XER files must not block the UI thread.
- Primavera-derived data must be consumable through domain service interfaces, not direct DB queries from other studios.
- Other studios consume Primavera data via nullable injection of `IPrimaveraWorkspaceService` (or similar) — no intermediate integration layer.
- Other studios must degrade gracefully when Primavera data is unavailable.
- Native studio data entry must remain valid even if Primavera data is absent or partial.
- Localization must support English and Arabic, including RTL layouts.
- The studio must be project-gated and only available when a project is active.
- Import and repair operations must preserve source metadata so users can tell what came from Primavera and what was edited locally.
- Unsupported XER table types are preserved as raw staging and re-emitted on export (round-trip fidelity).

---

## Implementation Plan

### Phase 9.1 — Foundation

- Create the `Planova.Primavera` module project and test project.
- Add all domain entities, enums, constants, and interfaces (including `XerRawTable`).
- Add shared DTOs and snapshot models for workspace and import preview use.
- Register the new module in dependency injection.

### Phase 9.2 — Persistence and Storage + Cross-Studio Contracts

- Add EF Core configurations for all Primavera entities (including `XerRawTable`).
- Extend `PlanovaDbContext` with the new tables.
- Add repositories for imports, workspace data, validation, and repair.
- Define app-managed file storage conventions for imported XER files and exported artifacts.
- **Publish shared domain service interfaces** (`IPrimaveraWorkspaceService`, `IPrimaveraImportService`, `IPrimaveraExportService`, `IPrimaveraValidationService`, `IPrimaveraRepairService`) as the cross-studio consumption contracts.
- Add nullable registration pattern so other modules can optionally inject Primavera services.

> **Why contracts are defined here (not 9.6):** Downstream phases (10 — Schedule Comparison, 11 — Delay Analysis, etc.) need to reference these interfaces from day one of their implementation. Defining them early ensures consumers can be built and tested against a stable contract without waiting for the full Primavera UI.

### Phase 9.3 — XER Import and Export

- Implement the custom XER parser for supported tables.
- Implement raw table preservation for unsupported tables.
- Build import preview, staged commit, and integrity reporting.
- Implement XER export from the canonical workspace model (supported tables + raw re-emission).
- Store import sessions and export metadata for audit and recovery.

### Phase 9.4 — Validation and Repair

- Implement validation rules: missing calendars, invalid relationship links, broken references, duplicate codes, orphaned records, zero-duration activities, missing predecessors/successors.
- Expose grouped validation results with severity and entity references.
- Add user-approved repair actions.
- Ensure repair outcomes are persisted and auditable.

### Phase 9.5 — Workspace UI

- Add the Primavera navigation target to the shell.
- Build the dedicated Primavera Studio workspace with tabbed areas for activities, relationships, resources, calendars, codes, baselines, UDFs, validation, and repair.
- Wire the ViewModels and commands with MVVM-friendly interactions.
- Add grid-based editing with virtualization for large schedules.
- Add baseline browsing and version awareness.
- Add read-only indicators for imported vs. edited data.

### Phase 9.6 — Polish and Verification

- Add Arabic resources and RTL verification.
- Run performance benchmarks against the moderate schedule definition (10K activities, 30K relationships, 1K resources, 10 calendars, 5 baselines).
- Add tests for import, export, validation, repair, baseline storage, raw table round-trip, and cross-studio service consumption.
- Validate shell navigation, gating, and end-to-end workflows.

---

## In Scope Detail

### Import and Export

- Parse XER headers, projects, activities, relationships, resource assignments, calendars, codes, baselines, and UDFs
- Preserve unsupported XER tables as raw `XerRawTable` entries (table name + column headers + serialized row data)
- Detect malformed or incomplete files early
- Show staged import preview before commit
- Support overwrite and merge behavior where appropriate
- Persist import sessions for history and troubleshooting
- Export current Primavera workspace data to XER using the same canonical schedule model
- Re-emit preserved raw tables verbatim during export

### Workspace Editing

- Dedicated Primavera workspace with tabbed areas for the major schedule domains
- Grid-based editing for activities, relationships, calendars, resource assignments, codes, and UDFs
- Baseline and version awareness (browse baseline data, compare with current)
- Data filters for large schedules
- Virtualized grids to maintain responsiveness at the moderate benchmark size
- Read-only indicators when data is sourced from imported snapshots rather than local edits

### Validation and Repair

- Validate: missing calendars, invalid relationship links, broken references, duplicate codes, orphaned records, zero-duration activities, missing predecessors/successors, circular logic
- Surface validation issues before commit and after edits
- Group issues by severity and entity type
- Offer fix suggestions with user approval
- Record repair actions for audit and troubleshooting

### Cross-Studio Consumption

- `IPrimaveraWorkspaceService` is the single public contract for other studios
- Other studios inject it as nullable (`IPrimaveraWorkspaceService?`)
- Each consumer implements its own fallback (return null, use native data, show "no Primavera data" state)
- No integration service, no source resolver, no adapter project
- Provenance is preserved in all DTOs returned by `IPrimaveraWorkspaceService`

### Navigation and Studio Shell

- Replace the placeholder `primavera` nav target with the real studio
- Keep project gating consistent with the other studios
- Ensure the studio opens as a dedicated workspace with its own tabs and command surface
- Follow the same shell conventions for icons, activation, and state restoration

### Data Provenance

- Mark imported rows with `SourceType = PrimaveraImport`, `ImportSessionId`, `SourceFileName`, `ImportedAt`
- Track whether a record came from import, manual edit, repair, or export
- Preserve the source file name and import timestamp when possible
- Avoid silent replacement of native domain data

### Error Handling

- Show parse and validation errors in a user-readable list
- Keep import preview available even when some rows fail validation
- Allow partial staging where safe, but surface the exact conflicts
- Keep export failures recoverable and log them centrally

---

## Expected Deliverables

- `Planova.Primavera` module project
- Primavera domain entities, enums, and interfaces
- XER parser and export pipeline (custom, with raw table preservation)
- Validation and repair services
- Primavera Studio ViewModels and Views
- Persistence mappings and repositories
- Localization files for `Planova.Localization`
- Navigation and DI updates for the `primavera` target
- Tests for import, export, validation, repair, baseline storage, raw table round-trip, and cross-studio consumption
- Test fixture XER files (small, medium, moderate)
- `planova-xer-parser` NuGet extraction candidate (documented, not packaged yet)

---

## Dependencies and Touchpoints

### Primary Dependencies

- Existing project and shell infrastructure from Phase 0
- Project context and active project selection from Phase 1
- File import/export infrastructure from Phase 2
- WBS and Activity models from earlier planning phases
- Resource and Cost data services for downstream schedule consumption
- Reporting Center data composition patterns from Phase 8

### Persistence Touchpoints

- New `DbSet` entries in `PlanovaDbContext`
- EF Core configuration classes in `Planova.Persistence/EntityConfigurations`
- Repository implementations in `Planova.Persistence/Repositories`
- File storage for imported XER artifacts and generated exports

### UI Touchpoints

- Navigation rail entry for `primavera`
- Studio view registration in shell DI
- Dedicated workspace layout with clear editing and validation surfaces
- Consistent Fluent UI styling and RTL behavior

---

## Risks and Mitigations

### Risk: XER parsing may vary across Primavera versions
**Mitigation:** Keep the parser tolerant of optional fields and version-specific sections. Validate with real sample files from P6 versions 6-22. Preserve unknown sections via `XerRawTable` for round-trip safety.

### Risk: Large schedules may cause memory pressure
**Mitigation:** Use staged parsing, chunked loading, and virtualized grids. Benchmark against the moderate definition (10K activities, 30K relationships). Keep `PrimaveraBaseline` full-copy storage but lazy-load baseline data on demand.

### Risk: Cross-studio nullable injection leads to scattered null checks
**Mitigation:** Each consumer wraps Primavera access in a single "resolve source" helper method, keeping null checks contained. Clear logging when Primavera is unavailable.

### Risk: Repair may mask data problems
**Mitigation:** Separate validation from repair. Show every fix proposal before applying it and record the action after it runs.

### Risk: Export round-trip fidelity may be partial
**Mitigation:** Preserve unsupported tables via `XerRawTable` (raw column+row data). Re-emit on export. Surface unsupported constructs in export warnings.

### Risk: Primavera data could conflict with native studio data
**Mitigation:** Treat Primavera as an authoritative source only for schedule-centric entities. Each consumer decides its own fallback. Provenance is preserved on all records.

---

## Acceptance Criteria

Phase 9 is complete when all of the following are true:

### Functional — Import and Export

- Users can import a Primavera XER file into a project.
- The system shows a preview before commit.
- Validation issues are displayed before and after import.
- Users can export the current Primavera workspace to XER.
- Unsupported XER tables are preserved through import→export round-trip.
- The import/export flow preserves source metadata and timestamps.

### Functional — Workspace and Editing

- The Primavera Studio opens as a dedicated workspace (project-gated).
- Users can browse and edit activities, relationships, resource assignments, calendars, codes, baselines, and UDFs.
- Baseline data is viewable and isolated from the active schedule.
- The workspace remains responsive at moderate benchmark size.
- The UI follows the same Fluent UI and MVVM conventions as the rest of the app.

### Functional — Validation and Repair

- The system detects: broken references, missing calendars, invalid relationships, duplicates, orphaned rows, circular logic, zero-duration activities.
- Users can apply repair actions after reviewing them.
- Repair actions are logged and persisted.
- Validation can be run independently of import.

### Functional — Cross-Studio Consumption

- Other studios can inject `IPrimaveraWorkspaceService` (nullable) to read Primavera schedule data.
- Those studios continue working with their native data when Primavera data is not present.
- No studio accesses Primavera tables directly.
- Provenance is preserved in all shared projections.

### Functional — Shell and Localization

- The `primavera` nav item opens the real studio and is project-gated.
- English and Arabic resources are available for the new screens.
- RTL layouts render correctly.
- All async operations respect cancellation.

### Quality

- Automated tests cover parsing, validation, repair, export, baseline operations, raw table round-trip, and cross-studio consumption.
- Performance benchmarks pass against the moderate schedule definition:
  - Import preview < 10s (10K activities, 30K relationships, 1K resources)
  - Validation < 5s
  - Export < 10s
  - Workspace open < 5s
- There are no regressions in existing studio navigation or data entry.
- The implementation remains compliant with Clean Architecture and the platform's modular design.

---

## Definition of Done

- `Planova.Primavera` exists as a new module project.
- The domain model, interfaces, and DTOs are implemented.
- EF Core mappings and repositories are added in `Planova.Persistence`.
- The XER import/export pipeline is working (including raw table preservation).
- The Primavera workspace UI is present in `Planova.UI`.
- Validation and repair flows are working and test-covered.
- Cross-studio consumption works via nullable service injection (no integration layer).
- The `primavera` navigation target is no longer a placeholder.
- Localization is complete for English and Arabic.
- All tests pass and existing phases remain functional.
- Performance benchmarks pass against the defined moderate schedule.

---

## Notes

- Phase 9 should be treated as a platform capability, not a one-off import tool.
- The Primavera Studio and Activity Studio are fully independent. Users choose which to use based on their workflow.
- Cross-studio integration follows the same pattern as Phase 8's Reporting Center: nullable injection of domain service interfaces, with fallback logic in each consumer.
- The XER parser is built custom, focused on supported tables, with a future extraction path to a standalone NuGet package.
- Always prefer clarity about source and provenance over hidden synchronization.
