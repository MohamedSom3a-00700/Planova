# Tasks: Primavera Studio

**Input**: Design documents from `specs/011-primavera-studio/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are included per spec — acceptance criteria require automated tests for import, export, validation, repair, baseline storage, raw table round-trip, and cross-studio consumption.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Multi-project desktop app**: `Planova.Primavera/`, `Planova.Persistence/`, `Planova.UI/`, `tests/` at repository root
- Paths reflect the project structure from plan.md

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create Planova.Primavera class library project at `Planova.Primavera/Planova.Primavera.csproj`
- [X] T002 [P] Create Planova.Primavera.Tests xUnit project at `tests/Planova.Primavera.Tests/Planova.Primavera.Tests.csproj`
- [X] T003 [P] Add NuGet dependencies to Planova.Primavera: CommunityToolkit.Mvvm, EF Core 8, Microsoft.Extensions.Hosting, Serilog
- [X] T004 [P] Add project reference from Planova.Primavera to Planova.Shared
- [X] T005 [P] Add project reference from tests to Planova.Primavera
- [X] T006 Create test fixture XER files directory at `tests/Planova.Primavera.Tests/Fixtures/`
- [X] T007 Create sample XER files (small: ~100 activities, medium: ~2K activities, moderate: ~10K activities) in `tests/Planova.Primavera.Tests/Fixtures/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T008 Create `PrimaveraEntityType` enum in `Planova.Primavera/Domain/Enums/PrimaveraEntityType.cs`
- [X] T009 [P] Create `PrimaveraImportStatus` enum in `Planova.Primavera/Domain/Enums/PrimaveraImportStatus.cs`
- [X] T010 [P] Create `PrimaveraValidationSeverity` enum in `Planova.Primavera/Domain/Enums/PrimaveraValidationSeverity.cs`
- [X] T011 [P] Create `PrimaveraRepairStatus` enum in `Planova.Primavera/Domain/Enums/PrimaveraRepairStatus.cs`
- [X] T012 [P] Create `XerFieldNames` constants in `Planova.Primavera/Domain/Constants/XerFieldNames.cs`
- [X] T013 [P] Create `PrimaveraProject` entity in `Planova.Primavera/Domain/Entities/PrimaveraProject.cs`
- [X] T014 [P] Create `XerImportSession` entity in `Planova.Primavera/Domain/Entities/XerImportSession.cs`
- [X] T015 [P] Create `XerExportProfile` entity in `Planova.Primavera/Domain/Entities/XerExportProfile.cs`
- [X] T016 [P] Create `XerRawTable` entity in `Planova.Primavera/Domain/Entities/XerRawTable.cs`
- [X] T017 [P] Create `PrimaveraActivity` entity in `Planova.Primavera/Domain/Entities/PrimaveraActivity.cs`
- [X] T018 [P] Create `PrimaveraRelationship` entity in `Planova.Primavera/Domain/Entities/PrimaveraRelationship.cs`
- [X] T019 [P] Create `PrimaveraResourceAssignment` entity in `Planova.Primavera/Domain/Entities/PrimaveraResourceAssignment.cs`
- [X] T020 [P] Create `PrimaveraCalendar` entity in `Planova.Primavera/Domain/Entities/PrimaveraCalendar.cs`
- [X] T021 [P] Create `PrimaveraCode` entity in `Planova.Primavera/Domain/Entities/PrimaveraCode.cs`
- [X] T022 [P] Create `PrimaveraBaseline` entity in `Planova.Primavera/Domain/Entities/PrimaveraBaseline.cs`
- [X] T023 [P] Create `PrimaveraUdf` entity in `Planova.Primavera/Domain/Entities/PrimaveraUdf.cs`
- [X] T024 [P] Create `PrimaveraValidationRule` entity in `Planova.Primavera/Domain/Entities/PrimaveraValidationRule.cs`
- [X] T025 [P] Create `PrimaveraValidationIssue` entity in `Planova.Primavera/Domain/Entities/PrimaveraValidationIssue.cs`
- [X] T026 [P] Create `PrimaveraRepairAction` entity in `Planova.Primavera/Domain/Entities/PrimaveraRepairAction.cs`
- [X] T027 [P] Create EF Core configuration for PrimaveraProject at `Planova.Persistence/EntityConfigurations/PrimaveraProjectConfiguration.cs`
- [X] T028 [P] Create EF Core configuration for XerImportSession at `Planova.Persistence/EntityConfigurations/XerImportSessionConfiguration.cs`
- [X] T029 [P] Create EF Core configuration for XerExportProfile at `Planova.Persistence/EntityConfigurations/XerExportProfileConfiguration.cs`
- [X] T030 [P] Create EF Core configuration for XerRawTable at `Planova.Persistence/EntityConfigurations/XerRawTableConfiguration.cs`
- [X] T031 [P] Create EF Core configuration for PrimaveraActivity at `Planova.Persistence/EntityConfigurations/PrimaveraActivityConfiguration.cs`
- [X] T032 [P] Create EF Core configuration for PrimaveraRelationship at `Planova.Persistence/EntityConfigurations/PrimaveraRelationshipConfiguration.cs`
- [X] T033 [P] Create EF Core configuration for PrimaveraResourceAssignment at `Planova.Persistence/EntityConfigurations/PrimaveraResourceAssignmentConfiguration.cs`
- [X] T034 [P] Create EF Core configuration for PrimaveraCalendar at `Planova.Persistence/EntityConfigurations/PrimaveraCalendarConfiguration.cs`
- [X] T035 [P] Create EF Core configuration for PrimaveraCode at `Planova.Persistence/EntityConfigurations/PrimaveraCodeConfiguration.cs`
- [X] T036 [P] Create EF Core configuration for PrimaveraBaseline at `Planova.Persistence/EntityConfigurations/PrimaveraBaselineConfiguration.cs`
- [X] T037 [P] Create EF Core configuration for PrimaveraUdf at `Planova.Persistence/EntityConfigurations/PrimaveraUdfConfiguration.cs`
- [X] T038 [P] Create EF Core configuration for PrimaveraValidationRule at `Planova.Persistence/EntityConfigurations/PrimaveraValidationRuleConfiguration.cs`
- [X] T039 [P] Create EF Core configuration for PrimaveraValidationIssue at `Planova.Persistence/EntityConfigurations/PrimaveraValidationIssueConfiguration.cs`
- [X] T040 [P] Create EF Core configuration for PrimaveraRepairAction at `Planova.Persistence/EntityConfigurations/PrimaveraRepairActionConfiguration.cs`
- [X] T041 Extend `PlanovaDbContext` with all new DbSet entries for Primavera entities
- [X] T042 Create DTOs in `Planova.Primavera/Application/Dto/`: PrimaveraProjectDto, XerImportPreviewDto, XerImportResultDto, PrimaveraActivityDto, PrimaveraRelationshipDto, PrimaveraResourceAssignmentDto, PrimaveraCalendarDto, PrimaveraCodeDto, PrimaveraBaselineDto, PrimaveraUdfDto, PrimaveraValidationIssueDto, PrimaveraRepairActionDto
- [X] T043 Create `PrimaveraWorkspaceSnapshot` model in `Planova.Primavera/Application/Models/PrimaveraWorkspaceSnapshot.cs`
- [X] T044 Create `PrimaveraMappingProfile` in `Planova.Primavera/Application/Mappings/PrimaveraMappingProfile.cs`
- [X] T045 Create `ServiceCollectionExtensions` in `Planova.Primavera/Extensions/ServiceCollectionExtensions.cs` with `AddPlanovaPrimavera()` registration
- [X] T046 Create localization resource files: `PrimaveraResources.en.resx` and `PrimaveraResources.ar.resx` in `Planova.Localization/Resources/`

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Import XER File with Preview (Priority: P1) 🎯 MVP

**Goal**: Users can import a Primavera P6 XER file, see a preview with row counts and validation issues, and commit the import to the workspace.

**Independent Test**: Import a known XER file, verify preview shows correct row counts per entity type, confirm validation issues are reported, and confirm committed data appears in workspace tabs.

### Implementation for User Story 1

- [X] T047 [P] [US1] Create `IPrimaveraImportRepository` interface in `Planova.Primavera/Domain/Interfaces/IPrimaveraImportRepository.cs`
- [X] T048 [P] [US1] Create `PrimaveraImportRepository` in `Planova.Persistence/Repositories/PrimaveraImportRepository.cs`
- [X] T049 [P] [US1] Create `IPrimaveraImportService` interface in `Planova.Primavera/Domain/Interfaces/IPrimaveraImportService.cs`
- [X] T050 [US1] Implement `XerParser` in `Planova.Primavera/Application/Parsers/XerParser.cs` — parse supported tables (CALENDAR, PROJECT, TASK, TASKPRED, TASKRSRC, RSPROJECT, RSOURCE, RCATTYPE, RCATVAL, PROJECTCODE, PROJCODECAT, PROJCODEVAL, UDFTYPE, UDFVALUE, PROJECTBASELINE, TASKUDF)
- [X] T051 [US1] Implement raw table preservation in `XerParser` — store unsupported tables (RISK, DOCUMENT, NOTE, etc.) as `XerRawTable`
- [X] T052 [P] [US1] Create `PrimaveraImportService` in `Planova.Primavera/Application/Services/PrimaveraImportService.cs` — preview generation, staged commit with atomic transaction (rollback on failure)
- [X] T053 [US1] Implement overwrite/merge logic in PrimaveraImportService — match by XER internal IDs (task_id, calendar_id, etc.), overwrite matched, insert unmatched, flag absent rows
- [X] T054 [US1] Implement import validation — detect malformed XER files early, show warnings for missing calendars and broken references in preview
- [X] T055 [P] [US1] Create `PrimaveraImportViewModel` in `Planova.UI/ViewModels/Primavera/PrimaveraImportViewModel.cs`
- [X] T056 [P] [US1] Create `PrimaveraImportView` XAML in `Planova.UI/Views/Primavera/PrimaveraImportView.xaml` — file picker, preview grid, validation warnings panel
- [X] T057 [US1] Wire import preview UI — display row counts per entity type, validation warnings with severity
- [X] T058 [US1] Wire import commit flow — overwrite/merge choice dialog, atomic commit, success confirmation
- [X] T059 [US1] Add audit logging for import attempts (success/failure with file name and row counts)
- [X] T060 [US1] Create `PrimaveraImportHostedService` in `Planova.Primavera/Background/PrimaveraImportHostedService.cs` for background parsing

**Checkpoint**: At this point, User Story 1 should be fully functional — users can import XER files, preview, and commit

---

## Phase 4: User Story 2 — Browse and Edit Workspace Data (Priority: P1)

**Goal**: Users can browse imported schedule data in tabbed workspace views, sort/filter grids, and edit cell values inline. Baseline data is viewable in read-only mode.

**Independent Test**: Import a moderate XER file (10K activities), open each workspace tab, verify grid loading, filtering, cell editing, and baseline browsing work without performance degradation.

### Implementation for User Story 2

- [X] T061 [P] [US2] Create `IPrimaveraWorkspaceRepository` interface in `Planova.Primavera/Domain/Interfaces/IPrimaveraWorkspaceRepository.cs`
- [X] T062 [P] [US2] Create `PrimaveraWorkspaceRepository` in `Planova.Persistence/Repositories/PrimaveraWorkspaceRepository.cs`
- [X] T063 [P] [US2] Create `IPrimaveraWorkspaceService` interface in `Planova.Primavera/Domain/Interfaces/IPrimaveraWorkspaceService.cs`
- [X] T064 [US2] Implement `PrimaveraWorkspaceService` in `Planova.Primavera/Application/Services/PrimaveraWorkspaceService.cs` — CRUD operations for activities, relationships, resource assignments, calendars, codes, baselines, UDFs
- [X] T065 [P] [US2] Create workspace ViewModels in `Planova.UI/ViewModels/Primavera/`: PrimaveraActivitiesViewModel, PrimaveraRelationshipsViewModel, PrimaveraResourcesViewModel, PrimaveraCalendarsViewModel, PrimaveraCodesViewModel, PrimaveraBaselinesViewModel, PrimaveraUdfsViewModel
- [X] T066 [P] [US2] Create workspace Views XAML in `Planova.UI/Views/Primavera/`: PrimaveraActivitiesView, PrimaveraRelationshipsView, PrimaveraResourcesView, PrimaveraCalendarsView, PrimaveraCodesView, PrimaveraBaselinesView, PrimaveraUdfsView — each with virtualized grid, sort, filter, inline editing
- [X] T067 [US2] Create `PrimaveraWorkspaceViewModel` in `Planova.UI/ViewModels/Primavera/PrimaveraWorkspaceViewModel.cs` — tab management and orchestration
- [X] T068 [US2] Create `PrimaveraWorkspaceView` XAML in `Planova.UI/Views/Primavera/PrimaveraWorkspaceView.xaml` — tabbed container with all workspace tabs
- [X] T069 [US2] Implement baseline data view — read-only mode with visual distinction from active schedule
- [X] T070 [US2] Add virtualized grid support for large datasets — ensure scroll, filter, cell edit respond under 100ms for moderate schedule
- [X] T071 [US2] Add data provenance tracking — mark entities as imported/manual/edit/repair with source metadata
- [X] T072 [US2] Implement inline cell editing with save/commit per row or batch
- [X] T073 [US2] Add audit logging for workspace edits (batch summary: entity type, count modified, timestamp)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 — Validate Schedule Integrity (Priority: P1)

**Goal**: Users can run on-demand validation to detect schedule integrity issues: broken references, missing calendars, invalid relationships, duplicates, orphaned records, circular logic.

**Independent Test**: Import an XER file with known issues, run validation, verify all expected issue types are detected and reported with correct severity.

### Implementation for User Story 3

- [X] T074 [P] [US3] Create `IPrimaveraValidationRepository` interface in `Planova.Primavera/Domain/Interfaces/IPrimaveraValidationRepository.cs`
- [X] T075 [P] [US3] Create `PrimaveraValidationRepository` in `Planova.Persistence/Repositories/PrimaveraValidationRepository.cs`
- [X] T076 [P] [US3] Create `IPrimaveraValidationService` interface in `Planova.Primavera/Domain/Interfaces/IPrimaveraValidationService.cs`
- [X] T077 [US3] Implement `PrimaveraValidationService` in `Planova.Primavera/Application/Services/PrimaveraValidationService.cs` with rules: broken references, missing calendars, invalid relationships, duplicate codes, orphaned records, zero-duration activities, missing predecessors/successors, circular logic
- [X] T078 [US3] Group validation results by severity (Error, Warning, Info) and entity type with navigation links
- [X] T079 [P] [US3] Create `PrimaveraValidationViewModel` in `Planova.UI/ViewModels/Primavera/PrimaveraValidationViewModel.cs`
- [X] T080 [P] [US3] Create `PrimaveraValidationView` XAML in `Planova.UI/Views/Primavera/PrimaveraValidationView.xaml` — results grid with severity grouping, entity navigation
- [X] T081 [US3] Wire validation to run independently of import and from the workspace toolbar

**Checkpoint**: User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 — Repair Schedule Issues (Priority: P2)

**Goal**: Users can review suggested fixes for detected validation issues, approve individual or batch fixes, and have repairs logged for audit.

**Independent Test**: Run validation on a schedule with known fixable issues, review suggestions, apply fixes, re-run validation to confirm issues resolved.

### Implementation for User Story 4

- [X] T082 [P] [US4] Create `IPrimaveraRepairRepository` interface in `Planova.Primavera/Domain/Interfaces/IPrimaveraRepairRepository.cs`
- [X] T083 [P] [US4] Create `PrimaveraRepairRepository` in `Planova.Persistence/Repositories/PrimaveraRepairRepository.cs`
- [X] T084 [P] [US4] Create `IPrimaveraRepairService` interface in `Planova.Primavera/Domain/Interfaces/IPrimaveraRepairService.cs`
- [X] T085 [US4] Implement `PrimaveraRepairService` in `Planova.Primavera/Application/Services/PrimaveraRepairService.cs` — generate suggested fixes, apply individual/batch fixes, log repairs
- [X] T086 [US4] Implement repair action logging — record problem, resolution, target entity, user identity, timestamp
- [X] T087 [P] [US4] Create `PrimaveraRepairViewModel` in `Planova.UI/ViewModels/Primavera/PrimaveraRepairViewModel.cs`
- [X] T088 [P] [US4] Create `PrimaveraRepairView` XAML in `Planova.UI/Views/Primavera/PrimaveraRepairView.xaml` — suggestions list, approve/apply buttons, batch selection
- [X] T089 [US4] Wire repair to consume validation issues from US3 and offer fixes

**Checkpoint**: User Stories 1–4 should all work independently

---

## Phase 7: User Story 5 — Export Workspace to XER (Priority: P2)

**Goal**: Users can export the current workspace state to a valid XER file, including all supported entities and preserved raw tables (round-trip fidelity).

**Independent Test**: Import an XER file, make edits, export to new XER, verify exported file contains all entities and preserved raw tables.

### Implementation for User Story 5

- [X] T090 [P] [US5] Create `IPrimaveraExportService` interface in `Planova.Primavera/Domain/Interfaces/IPrimaveraExportService.cs`
- [X] T091 [US5] Implement `XerWriter` in `Planova.Primavera/Application/Writers/XerWriter.cs` — write supported entity tables and raw tables to XER format
- [X] T092 [US5] Implement `PrimaveraExportService` in `Planova.Primavera/Application/Services/PrimaveraExportService.cs` — serialize workspace state, invoke XerWriter, save output file
- [X] T093 [P] [US5] Create export profile handling — entity selection, raw table preservation flag, output path
- [X] T094 [P] [US5] Create `PrimaveraExportViewModel` in `Planova.UI/ViewModels/Primavera/PrimaveraExportViewModel.cs`
- [X] T095 [P] [US5] Create `PrimaveraExportView` XAML in `Planova.UI/Views/Primavera/PrimaveraExportView.xaml` — scope selection, export button, confirmation with file path and row counts
- [X] T096 [US5] Add audit logging for exports (file path and row counts)
- [X] T097 [US5] Verify round-trip fidelity — imported data exports with unsupported raw tables preserved verbatim

**Checkpoint**: User Stories 1–5 should all work independently

---

## Phase 8: User Story 6 — Cross-Studio Data Consumption (Priority: P2)

**Goal**: Other studios can inject `IPrimaveraWorkspaceService` (nullable) to read Primavera schedule data when available, falling back to native data when absent.

**Independent Test**: Create a consumer studio that injects the Primavera service, verify data returned when Primavera data exists, verify consumer works with native data when absent.

### Implementation for User Story 6

- [X] T098 [US6] Publish cross-studio service interfaces — ensure `IPrimaveraWorkspaceService`, `IPrimaveraImportService`, `IPrimaveraExportService`, `IPrimaveraValidationService`, `IPrimaveraRepairService` are public in `Planova.Primavera`
- [X] T099 [US6] Add nullable registration pattern in `ServiceCollectionExtensions` so other modules can optionally inject Primavera services
- [X] T100 [US6] Implement `HasDataAsync` on `PrimaveraWorkspaceService` — quick check if Primavera data exists for a project
- [X] T101 [US6] Implement `GetScheduleSnapshotAsync` on `PrimaveraWorkspaceService` — return frozen snapshot with provenance markers
- [X] T102 [US6] Verify cross-studio contract with a reference consumer test — inject `IPrimaveraWorkspaceService?` and verify graceful fallback when Primavera is absent
- [X] T103 [US6] Create cross-studio integration test — simulate a consumer studio reading data via nullable injection

**Checkpoint**: Cross-studio data sharing is operational — downstream phases can depend on these contracts

---

## Phase 9: User Story 7 — Project-Gated Navigation (Priority: P3)

**Goal**: The Primavera Studio appears as a navigation item only when a project is active, following the same gating pattern as other studios.

**Independent Test**: Open app with no project (item disabled), create/open a project (item enabled), open studio to verify workspace loads.

### Implementation for User Story 7

- [X] T104 [US7] Create `PrimaveraStudioViewModel` in `Planova.UI/ViewModels/Primavera/PrimaveraStudioViewModel.cs` — studio shell with tabbed areas
- [X] T105 [US7] Create `PrimaveraStudioView` XAML in `Planova.UI/Views/Primavera/PrimaveraStudioView.xaml` — main studio container
- [X] T106 [US7] Wire Primavera navigation target in the shell — replace placeholder `primavera` nav item with real studio
- [X] T107 [US7] Implement project-gating — disable/hide Primavera nav item when no project is active
- [X] T108 [US7] Create `PrimaveraStatusConverter` in `Planova.UI/Converters/PrimaveraStatusConverter.cs`
- [X] T109 [US7] Register `PrimaveraStudioViewModel` and all sub-ViewModels in shell DI

**Checkpoint**: All user stories should be independently functional

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T110 [P] Add Arabic localization strings in `Planova.Localization/Resources/PrimaveraResources.ar.resx`
- [X] T111 [P] Verify English localization in `Planova.Localization/Resources/PrimaveraResources.en.resx`
- [ ] T112 Verify RTL layout support across all Primavera views
- [X] T113 [P] Create unit tests for domain entities in `tests/Planova.Primavera.Tests/Domain/`
- [X] T114 [P] Create unit tests for XerParser in `tests/Planova.Primavera.Tests/Application/XerParserTests.cs`
- [X] T115 [P] Create unit tests for XerWriter in `tests/Planova.Primavera.Tests/Application/XerWriterTests.cs`
- [X] T116 [P] Create unit tests for PrimaveraValidationService in `tests/Planova.Primavera.Tests/Application/PrimaveraValidationServiceTests.cs`
- [X] T117 [P] Create unit tests for PrimaveraRepairService in `tests/Planova.Primavera.Tests/Application/PrimaveraRepairServiceTests.cs`
- [X] T118 [P] Create integration tests for import-export round-trip (raw table preservation) in `tests/Planova.Primavera.Tests/Integration/`
- [X] T119 [P] Create integration tests for cross-studio consumption in `tests/Planova.Primavera.Tests/Integration/`
- [ ] T120 Benchmark performance against moderate schedule definition — import <10s, validation <5s, export <10s, workspace open <5s, grid ops <100ms
- [ ] T121 Run existing test suite to confirm no regressions in other studios
- [ ] T122 Code cleanup and refactoring across all phases

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 — Import (Phase 3)**: Depends on Foundational — all entities and persistence must exist
- **US2 — Workspace (Phase 4)**: Depends on Foundational + US1 (needs imported data to display)
- **US3 — Validation (Phase 5)**: Depends on Foundational + US2 (needs workspace entities to validate)
- **US4 — Repair (Phase 6)**: Depends on US3 (needs validation issues to generate fixes)
- **US5 — Export (Phase 7)**: Depends on US1 + US2 (needs workspace state to export)
- **US6 — Cross-Studio (Phase 8)**: Depends on US2 (needs workspace service)
- **US7 — Navigation (Phase 9)**: Depends on Foundational (shell integration, independent of data)
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational — No dependencies on other stories
- **US2 (P1)**: Needs US1 for imported data, but workspace ViewModels/Views can be built in parallel with US1 implementation
- **US3 (P1)**: Depends on US2 for workspace entities to validate
- **US4 (P2)**: Depends on US3 (validation must exist)
- **US5 (P2)**: Depends on US1 (import) and US2 (workspace state)
- **US6 (P2)**: Depends on US2 (workspace service interface)
- **US7 (P3)**: Independent of data — shell integration only

### Within Each User Story

- Models before services
- Services before ViewModels
- ViewModels before Views
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Phase 1 Setup tasks marked [P] can run in parallel
- All Phase 2 Foundational tasks marked [P] can run in parallel
- US1 and US7 can start in parallel (US7 is shell integration, independent of data)
- After US2 reaches checkpoint, US3, US6 can start partially in parallel
- After US3 reaches checkpoint, US4 can start
- After US2 reaches checkpoint, US5 can start
- All Phase 10 Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all models for User Story 1 together:
Task: "Create IPrimaveraImportRepository in Planova.Primavera/Domain/Interfaces/IPrimaveraImportRepository.cs"
Task: "Create PrimeraImportRepository in Planova.Persistence/Repositories/PrimaveraImportRepository.cs"
Task: "Create IPrimaveraImportService in Planova.Primavera/Domain/Interfaces/IPrimaveraImportService.cs"

# Launch all UI for User Story 1 together:
Task: "Create PrimaveraImportViewModel in Planova.UI/ViewModels/Primavera/PrimaveraImportViewModel.cs"
Task: "Create PrimaveraImportView in Planova.UI/Views/Primavera/PrimaveraImportView.xaml"
```

## Parallel Example: User Story 2

```bash
# Launch all ViewModels for User Story 2 together:
Task: "PrimaveraActivitiesViewModel, PrimaveraRelationshipsViewModel, PrimaveraResourcesViewModel, PrimaveraCalendarsViewModel, PrimaveraCodesViewModel, PrimaveraBaselinesViewModel, PrimaveraUdfsViewModel in Planova.UI/ViewModels/Primavera/"

# Launch all Views for User Story 2 together:
Task: "PrimaveraActivitiesView, PrimaveraRelationshipsView, PrimaveraResourcesView, PrimaveraCalendarsView, PrimaveraCodesView, PrimaveraBaselinesView, PrimaveraUdfsView in Planova.UI/Views/Primavera/"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 — Import XER
4. **STOP and VALIDATE**: Test US1 independently (import XER, preview, commit, verify in tabs)
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Import) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Workspace) → Test independently → Deploy/Demo
4. Add US3 (Validation) → Test independently → Deploy/Demo
5. Add US4 (Repair) → Test independently → Deploy/Demo
6. Add US5 (Export) → Test independently → Deploy/Demo
7. Add US6 (Cross-Studio) → Test independently → Deploy/Demo
8. Add US7 (Navigation) → Test independently → Deploy/Demo
9. Add Phase 10 (Polish) → Final validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Import — highest priority)
   - Developer B: US7 (Navigation — independent, shell integration)
3. After US1 checkpoint:
   - Developer A: US2 (Workspace)
   - Developer B: US3 (Validation) + US4 (Repair)
   - Developer C: US5 (Export) + US6 (Cross-studio)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Test fixture XER files need to be created (T007) before parser tests can run
- Import atomic transaction (T052, FR-005): wrap in single transaction, rollback entirely on failure
- Entity matching on re-import (T053, FR-006): use XER internal IDs (task_id, calendar_id), overwrite matched, insert unmatched, flag absent rows
- Audit logging scope (T059, T073, T096, FR-023): log import attempts, exports, repair actions, workspace edits. Validation runs NOT logged
