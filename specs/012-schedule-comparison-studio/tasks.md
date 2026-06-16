---

description: "Task list for Schedule Comparison Studio implementation"
---

# Tasks: Schedule Comparison Studio

**Input**: Design documents from `/specs/012-schedule-comparison-studio/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Module project**: `Planova.ScheduleComparison/`
- **Persistence**: `Planova.Persistence/`
- **UI**: `Planova.UI/`
- **Localization**: `Planova.Localization/`
- **Tests**: `tests/Planova.ScheduleComparison.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create `Planova.ScheduleComparison` class library project at `Planova.ScheduleComparison/Planova.ScheduleComparison.csproj`
- [X] T002 [P] Create test project at `tests/Planova.ScheduleComparison.Tests/Planova.ScheduleComparison.Tests.csproj`
- [X] T003 Create folder structure: `Domain/Entities/`, `Domain/Enums/`, `Domain/Interfaces/`, `Domain/Constants/`, `Application/Services/`, `Application/Dto/`, `Application/Models/`, `Application/Comparers/`, `Application/Mappings/`, `Extensions/`
- [X] T004 [P] Create test folder structure: `Domain/`, `Application/Comparers/`, `Application/Services/`, `Fixtures/`
- [X] T005 Add project references: `Planova.ScheduleComparison` → `Planova.Shared`, `Planova.Domain`; contract-only references to `Planova.Primavera`, `Planova.Activity`, `Planova.Resource`
- [X] T006 Add infrastructure references: `Planova.Excel`, `QuestPDF`
- [X] T007 [P] Add test project references: `Planova.ScheduleComparison`, `xunit`, test fixtures

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities, enums, interfaces, EF Core persistence, and DI registrations that MUST be complete before ANY user story implementation.

- [X] T008 [P] Define `ComparisonMode` enum in `Planova.ScheduleComparison/Domain/Enums/ComparisonMode.cs`
- [X] T009 [P] Define `ChangeType` enum in `Planova.ScheduleComparison/Domain/Enums/ChangeType.cs`
- [X] T010 [P] Define `ComparisonScope` enum in `Planova.ScheduleComparison/Domain/Enums/ComparisonScope.cs`
- [X] T011 [P] Define `MatchConfidence` enum in `Planova.ScheduleComparison/Domain/Enums/MatchConfidence.cs`
- [X] T012 [P] Define `SessionState` enum in `Planova.ScheduleComparison/Domain/Enums/SessionState.cs`
- [X] T013 [P] Define `ComparisonFieldNames` constants in `Planova.ScheduleComparison/Domain/Constants/ComparisonFieldNames.cs`
- [X] T014 Define `ComparisonSession` entity in `Planova.ScheduleComparison/Domain/Entities/ComparisonSession.cs`
- [X] T015 Define `ComparisonResult` entity in `Planova.ScheduleComparison/Domain/Entities/ComparisonResult.cs`
- [X] T016 Define `ScheduleSnapshot` entity in `Planova.ScheduleComparison/Domain/Entities/ScheduleSnapshot.cs`
- [X] T017 Define `ComparisonRule` entity in `Planova.ScheduleComparison/Domain/Entities/ComparisonRule.cs`
- [X] T018 [P] Define `IComparisonRepository` interface in `Planova.ScheduleComparison/Domain/Interfaces/IComparisonRepository.cs`
- [X] T019 [P] Define `IScheduleComparisonService` interface in `Planova.ScheduleComparison/Domain/Interfaces/IScheduleComparisonService.cs`
- [X] T020 [P] Define `IScheduleSnapshotService` interface in `Planova.ScheduleComparison/Domain/Interfaces/IScheduleSnapshotService.cs`
- [X] T021 [P] Define `IComparisonExportService` interface in `Planova.ScheduleComparison/Domain/Interfaces/IComparisonExportService.cs`
- [X] T022 Add `ComparisonSessions`, `ComparisonResults`, `ScheduleSnapshots`, `ComparisonRules` DbSets to `Planova.Persistence/PlanovaDbContext.cs`
- [X] T023 [P] Create entity configuration for `ComparisonSession` in `Planova.Persistence/EntityConfigurations/ComparisonSessionConfiguration.cs`
- [X] T024 [P] Create entity configuration for `ComparisonResult` in `Planova.Persistence/EntityConfigurations/ComparisonResultConfiguration.cs`
- [X] T025 [P] Create entity configuration for `ScheduleSnapshot` in `Planova.Persistence/EntityConfigurations/ScheduleSnapshotConfiguration.cs`
- [X] T026 [P] Create entity configuration for `ComparisonRule` in `Planova.Persistence/EntityConfigurations/ComparisonRuleConfiguration.cs`
- [X] T027 Implement `ComparisonRepository` in `Planova.Persistence/Repositories/ComparisonRepository.cs`
- [X] T028 Register `IComparisonRepository → ComparisonRepository` in `Planova.Persistence.Extensions.ServiceCollectionExtensions`
- [X] T029 Register `AddPlanovaScheduleComparison()` in `Planova.ScheduleComparison/Extensions/ServiceCollectionExtensions.cs` (application services only — no repository)
- [X] T030 Generate EF Core migration `AddScheduleComparisonEntities`
- [X] T031 Verify migration runs clean against existing SQLite database

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 2 - Capture and Restore Schedule Snapshots (Priority: P1)

**Goal**: Users can freeze current schedule data at a point in time with a descriptive label and later restore it as a comparison source or target.

**Independent Test**: Capture a snapshot of the current schedule, then select it as a source in the Compare tab and run a comparison.

### Implementation for User Story 2

- [X] T032 [P] [US2] Create `ScheduleSnapshotDto` in `Planova.ScheduleComparison/Application/Dto/ScheduleSnapshotDto.cs`
- [X] T033 [P] [US2] Create `ScheduleData` model in `Planova.ScheduleComparison/Application/Models/ScheduleData.cs`
- [X] T034 [P] [US2] Create `ScheduleActivity` model in `Planova.ScheduleComparison/Application/Models/ScheduleActivity.cs`
- [X] T035 [P] [US2] Create `ScheduleRelationship` model in `Planova.ScheduleComparison/Application/Models/ScheduleRelationship.cs`
- [X] T036 [P] [US2] Create `ScheduleResourceAssignment` model in `Planova.ScheduleComparison/Application/Models/ScheduleResourceAssignment.cs`
- [X] T037 [P] [US2] Create `ScheduleCalendar` model in `Planova.ScheduleComparison/Application/Models/ScheduleCalendar.cs`
- [X] T038 [US2] Implement `ScheduleSnapshotService` in `Planova.ScheduleComparison/Application/Services/ScheduleSnapshotService.cs` with capture/restore/list/delete methods
- [X] T039 [US2] Add snapshot capture mapping from native `IActivityService` / `IResourceAssignmentService` DTOs to `ScheduleData`
- [X] T040 [US2] Add snapshot restore — deserialize frozen data back into `ScheduleData`
- [X] T041 [P] [US2] Create `SnapshotViewModel` and `SnapshotView` for snapshot management UI in `Planova.UI/ViewModels/ScheduleComparison/` and `Planova.UI/Views/ScheduleComparison/`
- [X] T042 [US2] Integrate snapshot picker into Compare tab source/target selectors

**Checkpoint**: Users can capture, list, restore, and delete snapshots independently

---

## Phase 4: User Story 1 - Compare Two Schedule Versions and View Diffs (Priority: P1) 🎯 MVP

**Goal**: Users select source and target schedules, choose comparison dimensions, and view color-coded diffs across all five dimensions (activities, logic, resources, critical path, float).

**Independent Test**: Select any two schedules in the comparison workspace, run a comparison, and verify diff results appear in all five dimension tabs with correct counts and field-level detail.

### Tests for User Story 1

- [X] T043 [P] [US1] Unit test `ActivityComparer` with known input/output pairs in `tests/Planova.ScheduleComparison.Tests/Application/Comparers/ActivityComparerTests.cs`
- [X] T044 [P] [US1] Unit test `LogicComparer` with known input/output pairs in `tests/Planova.ScheduleComparison.Tests/Application/Comparers/LogicComparerTests.cs`
- [X] T045 [P] [US1] Unit test `ResourceComparer` with known input/output pairs in `tests/Planova.ScheduleComparison.Tests/Application/Comparers/ResourceComparerTests.cs`
- [X] T046 [P] [US1] Unit test `CriticalPathComparer` with known input/output pairs in `tests/Planova.ScheduleComparison.Tests/Application/Comparers/CriticalPathComparerTests.cs`
- [X] T047 [P] [US1] Unit test `FloatComparer` with known input/output pairs in `tests/Planova.ScheduleComparison.Tests/Application/Comparers/FloatComparerTests.cs`
- [X] T048 [P] [US1] Test deterministic matching strategies (all 5 priority levels) in `tests/Planova.ScheduleComparison.Tests/Application/Comparers/ActivityComparerTests.cs`
- [X] T049 [P] [US1] Test low-confidence and unmatched entity flagging in `tests/Planova.ScheduleComparison.Tests/Application/Comparers/`

### Implementation for User Story 1

- [X] T050 [US1] Implement `ActivityComparer` in `Planova.ScheduleComparison/Application/Comparers/ActivityComparer.cs` with deterministic matching priority and field diffing
- [X] T051 [US1] Implement `LogicComparer` in `Planova.ScheduleComparison/Application/Comparers/LogicComparer.cs` with predecessor/successor match key + type matching
- [X] T052 [US1] Implement `ResourceComparer` in `Planova.ScheduleComparison/Application/Comparers/ResourceComparer.cs` with activity match key + resource ID + role matching
- [X] T053 [US1] Implement `CriticalPathComparer` in `Planova.ScheduleComparison/Application/Comparers/CriticalPathComparer.cs` — compare CP membership and float from normalized schedule fields (no forward/backward pass)
- [X] T054 [US1] Implement `FloatComparer` in `Planova.ScheduleComparison/Application/Comparers/FloatComparer.cs` — total float and free float delta per activity
- [X] T055 [US1] Create DTOs in `Planova.ScheduleComparison/Application/Dto/`: `ComparisonSessionDto.cs`, `ComparisonSummaryDto.cs`, `ActivityDiffDto.cs`, `LogicDiffDto.cs`, `ResourceDiffDto.cs`, `CriticalPathDiffDto.cs`, `FloatImpactDto.cs`
- [X] T056 [US1] Create `ComparisonMappingProfile` in `Planova.ScheduleComparison/Application/Mappings/ComparisonMappingProfile.cs`
- [X] T057 [US1] Implement `ScheduleComparisonService` in `Planova.ScheduleComparison/Application/Services/ScheduleComparisonService.cs` — orchestrate resolution, comparison, and persistence of per-diff rows + `ResultJson` envelope
- [X] T058 [US1] Implement fallthrough chain: snapshot → Primavera (`IServiceProvider.GetService<>`) → native services in `ScheduleComparisonService`
- [X] T059 [US1] Implement `ComparisonResult` per-diff row persistence and `ScheduleComparisonResult` envelope storage as `ResultJson`
- [X] T060 [P] [US1] Create `CompareViewModel` and `CompareView` in `Planova.UI/ViewModels/ScheduleComparison/` and `Planova.UI/Views/ScheduleComparison/`
- [X] T061 [P] [US1] Create `ActivityDiffViewModel` and `ActivityDiffView` with color-coded grid in `Planova.UI/ViewModels/ScheduleComparison/` and `Planova.UI/Views/ScheduleComparison/`
- [X] T062 [P] [US1] Create `LogicDiffViewModel` and `LogicDiffView` in `Planova.UI/ViewModels/ScheduleComparison/` and `Planova.UI/Views/ScheduleComparison/`
- [X] T063 [P] [US1] Create `ResourceDiffViewModel` and `ResourceDiffView` in `Planova.UI/ViewModels/ScheduleComparison/` and `Planova.UI/Views/ScheduleComparison/`
- [X] T064 [P] [US1] Create `CriticalPathDiffViewModel` and `CriticalPathDiffView` in `Planova.UI/ViewModels/ScheduleComparison/` and `Planova.UI/Views/ScheduleComparison/`
- [X] T065 [P] [US1] Create `FloatImpactViewModel` and `FloatImpactView` in `Planova.UI/ViewModels/ScheduleComparison/` and `Planova.UI/Views/ScheduleComparison/`
- [X] T066 [P] [US1] Create `ComparisonColorConverter` in `Planova.UI/Converters/ComparisonColorConverter.cs`
- [X] T067 [P] [US1] Create `ScheduleComparisonTab` inner class matching Primavera Studio pattern
- [X] T068 [US1] Create `ScheduleComparisonViewModel` in `Planova.UI/ViewModels/ScheduleComparison/ScheduleComparisonViewModel.cs` with `Tabs`/`SelectedTab` pattern
- [X] T069 [US1] Create `ScheduleComparisonView` in `Planova.UI/Views/ScheduleComparison/ScheduleComparisonView.xaml` with `InitializeTabs(IServiceProvider)`
- [X] T070 [US1] Register all ViewModels and Views in `App.xaml.cs`
- [X] T071 [US1] Replace placeholder nav target with real studio registration in `Planova.UI/.../ShellViewModel.cs`

**Checkpoint**: MVP complete — users can compare two schedules and view diffs across all five dimensions

---

## Phase 5: User Story 3 - Export Comparison Results (Priority: P2)

**Goal**: Users export comparison results to Excel (per-dimension worksheets), PDF (formatted report), and JSON (Phase 11 consumable envelope).

**Independent Test**: Run a comparison, export to each format, and verify output files contain the expected data.

### Tests for User Story 3

- [X] T072 [P] [US3] Unit test `ComparisonExportService` — Excel export generates correct worksheets in `tests/Planova.ScheduleComparison.Tests/Application/Services/ComparisonExportServiceTests.cs`
- [X] T073 [P] [US3] Test JSON export conforms to schema contract (SchemaVersion, Source, Target, IncludedScopes) in `tests/Planova.ScheduleComparison.Tests/Application/Services/ComparisonExportServiceTests.cs`
- [X] T074 [P] [US3] Test failed export does not invalidate session in `tests/Planova.ScheduleComparison.Tests/Application/Services/ComparisonExportServiceTests.cs`

### Implementation for User Story 3

- [X] T075 [US3] Implement `ComparisonExportService` in `Planova.ScheduleComparison/Application/Services/ComparisonExportService.cs`
- [X] T076 [US3] Implement `ExportToExcelAsync` — reuse `Planova.Excel.Writers.IWorkbookWriter` where data model fits; custom ClosedXML multi-sheet workbook as a `ComparisonExportService` detail
- [X] T077 [US3] Implement `ExportToPdfAsync` via QuestPDF — formatted report with summary, tables, color coding
- [X] T078 [US3] Implement `ExportComparisonResultAsync` — serialize `ScheduleComparisonResult` envelope with SchemaVersion, Source, Target, IncludedScopes, GeneratedByVersion
- [X] T079 [US3] Implement temp-file-then-move pattern; failed export does not invalidate session
- [X] T080 [US3] Create `ComparisonExportViewModel` and `ComparisonExportView` in `Planova.UI/ViewModels/ScheduleComparison/` and `Planova.UI/Views/ScheduleComparison/`
- [X] T081 [US3] Wire export file storage to `{AppData}/Planova/Projects/{projectId}/Comparisons/{sessionId}/`

**Checkpoint**: Users can export comparison results to Excel, PDF, and JSON independently

---

## Phase 6: User Story 4 - Review Past Comparison Sessions and Re-open (Priority: P2)

**Goal**: Users browse history of past sessions, re-open to view results, re-export, or delete old sessions.

**Independent Test**: Run a comparison, navigate to History tab, find the session, re-open it to verify diffs are preserved, re-export it, and delete it.

### Tests for User Story 4

- [X] T082 [P] [US4] Test session listing, re-open, and soft-delete in `tests/Planova.ScheduleComparison.Tests/Application/Services/ScheduleComparisonServiceTests.cs`
- [X] T083 [P] [US4] Test `ComparisonResult` rows can be queried and paged independently from `ResultJson` in `tests/Planova.ScheduleComparison.Tests/Application/Services/ScheduleComparisonServiceTests.cs`

### Implementation for User Story 4

- [X] T084 [US4] Add session query methods to `ScheduleComparisonService` — list, re-open, soft-delete
- [X] T085 [US4] Implement session re-open — load saved `ComparisonResult` rows and `ResultJson` into diff tabs
- [X] T086 [US4] Implement soft-delete for sessions
- [X] T087 [US4] Create `ComparisonHistoryViewModel` and `ComparisonHistoryView` in `Planova.UI/ViewModels/ScheduleComparison/` and `Planova.UI/Views/ScheduleComparison/`
- [X] T088 [US4] Add "Re-export" action to History tab — re-invoke export from stored results

**Checkpoint**: Users can browse, re-open, re-export, and delete past sessions independently

---

## Phase 7: User Story 5 - Compare Primavera Imports (Priority: P3)

**Goal**: Users compare schedules directly from imported Primavera XER data without creating native snapshots first.

**Independent Test**: Import two Primavera XER files, select them in XER-vs-XER mode, and verify comparison results with Primavera provenance preserved.

### Tests for User Story 5

- [X] T089 [P] [US5] Test `PrimaveraWorkspaceSnapshot` → `ScheduleData` mapping (all fields, provenance preservation) in `tests/Planova.ScheduleComparison.Tests/Application/Services/ScheduleComparisonServiceTests.cs`
- [X] T090 [P] [US5] Test XER-vs-XER mode with Primavera available and unavailable (optional service resolution) in `tests/Planova.ScheduleComparison.Tests/Application/Services/ScheduleComparisonServiceTests.cs`

### Implementation for User Story 5

- [X] T091 [US5] Implement `MapPrimaveraSnapshotToScheduleData` in `ScheduleComparisonService` — map `PrimaveraWorkspaceSnapshot` to `ScheduleData` preserving provenance IDs
- [X] T092 [US5] Add XER-vs-XER mode support to the comparison picker — show Primavera projects in source/target when `IPrimaveraWorkspaceService` is available
- [X] T093 [US5] Ensure XER-vs-XER is hidden/disabled when Primavera is not registered (via `GetService<>` returning null)

**Checkpoint**: Users can compare Primavera imports when the Primavera module is available

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and finalize the feature.

- [X] T094 [P] Add `ComparisonResources.en.resx` and `ComparisonResources.ar.resx` in `Planova.Localization/Resources/`
- [X] T095 [P] Verify RTL layout support for Arabic in all diff grids and tabs
- [X] T096 [P] Add test fixtures with generated schedule data at `tests/Planova.ScheduleComparison.Tests/Fixtures/small-schedule.json` and `tests/Planova.ScheduleComparison.Tests/Fixtures/moderate-schedule.json`
- [X] T097 Run performance benchmarks against moderate schedule (10K activities / 30K relationships / 1K resources)
- [X] T098 Verify cancellation (CancellationToken) behavior across all long-running operations
- [X] T099 Verify session lifecycle enforcement (Draft → Running → Completed/Failed/Cancelled, no re-runs)
- [X] T100 Run persistence round-trip integration tests for all entity types
- [X] T101 Verify navigation shell integration — studio is project-gated, `schedule-compare` target is no longer a placeholder
- [X] T102 Final Clean Architecture compliance check — no EF Core or persistence references in `Planova.ScheduleComparison`
- [X] T103 Code cleanup and documentation pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 2 (Phase 3)**: Depends on Foundational — no dependency on other user stories
- **User Story 1 (Phase 4)**: Depends on Foundational — MVP scope; comparers are self-contained
- **User Story 3 (Phase 5)**: Depends on User Story 1 completion (needs comparison results to export)
- **User Story 4 (Phase 6)**: Depends on User Story 1 completion (needs comparison sessions to list)
- **User Story 5 (Phase 7)**: Depends on User Story 1 completion (extends data resolution in comparison service)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 2 (P1)**: Can start after Foundational — No dependencies on other stories
- **User Story 1 (P1)**: Can start after Foundational — depends on US2 only if snapshot data is the primary source; can use live data or pre-existing snapshots for independent testing
- **User Story 3 (P2)**: Needs US1 complete — requires comparison results to exist
- **User Story 4 (P2)**: Needs US1 complete — requires comparison sessions to exist
- **User Story 5 (P3)**: Needs US1 complete — extends ScheduleComparisonService data resolution

### Within Each User Story

- Tests (where defined) should be written and verified against known input/output
- Models before services
- Core implementation before UI
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel
- User Stories 2 and 1 (both P1) can start in parallel after Foundational
- Models within a story marked [P] can run in parallel
- Tests within a story marked [P] can run in parallel

---

## Parallel Example: Foundational Phase

```bash
# Launch all enum definitions together:
Task: "Define ComparisonMode enum"
Task: "Define ChangeType enum"
Task: "Define ComparisonScope enum"
Task: "Define MatchConfidence enum"
Task: "Define SessionState enum"

# Launch all entity configurations together:
Task: "Create entity config for ComparisonSession"
Task: "Create entity config for ComparisonResult"
Task: "Create entity config for ScheduleSnapshot"
Task: "Create entity config for ComparisonRule"
```

---

## Implementation Strategy

### MVP First (User Story 1 + Foundational)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 4: User Story 1 (comparison engine + diff UI)
4. **STOP and VALIDATE**: Select any two schedules, run comparison, verify all 5 diff tabs
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 2 (Snapshots) → Test independently → Deploy/Demo
3. Add User Story 1 (Comparison + Diffs) → Test independently → **MVP complete!**
4. Add User Story 3 (Export) → Test independently → Deploy/Demo
5. Add User Story 4 (History) → Test independently → Deploy/Demo
6. Add User Story 5 (Primavera) → Test independently → Deploy/Demo
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 2 (Snapshots) — models, service, snapshot UI
   - Developer B: User Story 1 comparers — all 5 comparers in parallel
3. After US1 comparers done:
   - Developer A: User Story 1 services (ScheduleComparisonService)
   - Developer B: User Story 1 UI (all diff tabs + Compare tab)
4. Remaining stories (US3, US4, US5) assigned as capacity allows

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Repository interface stays in Domain; implementation and registration belong in Planova.Persistence
- Primavera resolution uses `IServiceProvider.GetService<>()`, not nullable constructor injection
- CP comparer does NOT implement forward/backward pass — compares existing fields only
- Excel export reuses `Planova.Excel.Writers.IWorkbookWriter` where applicable
