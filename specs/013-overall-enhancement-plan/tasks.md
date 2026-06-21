---

description: "Task list for Overall Enhancement Plan feature implementation"

---

# Tasks: Overall Enhancement Plan

**Input**: Design documents from `/specs/013-overall-enhancement-plan/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Test tasks are NOT included — spec does not request TDD approach.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Module paths relative to repository root — e.g., `Planova.UI/`, `Planova.Boq/`, `Planova.Wbs/`, `Planova.Persistence/`, `Planova.Localization/`
- Tests at `tests/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold new files, add localization resource files, and register DI extensions

- [x] T001 [P] Create `Planova.Boq/Domain/Entities/BoqImportSession.cs` — import session entity per data-model.md
- [x] T002 [P] Create `Planova.Boq/Domain/Entities/BoqWorksheetMapping.cs` — per-worksheet mapping entity
- [x] T003 [P] Create `Planova.Boq/Domain/Enums/ImportMode.cs` — Single/Multiple/Merge enum
- [x] T004 [P] Create `Planova.Boq/Domain/Enums/BoqSheetType.cs` — BOQ vs non-BOQ enum
- [x] T005 [P] Create `Planova.Boq/Domain/Interfaces/IBoqImportService.cs` per contracts/cross-module-interfaces.md
- [x] T006 [P] Create `Planova.Boq/Domain/Interfaces/IBoqColumnMappingService.cs` per contracts
- [x] T007 [P] Create `Planova.Boq/Application/Models/BoqImportResult.cs` — import result model
- [x] T008 [P] Create `Planova.Boq/Application/Models/BoqPreviewRow.cs` — preview row model
- [x] T009 [P] Create `Planova.Wbs/Domain/Entities/WbsEditLock.cs` per data-model.md
- [x] T010 [P] Enhance `Planova.Wbs/Domain/Enums/WbsSource.cs` — added Imported, Primavera values
- [x] T011 [P] Enhance `Planova.Wbs/Domain/Enums/WbsStatus.cs` — updated to Draft/UnderReview/Approved/Archived
- [x] T012 [P] Create `Planova.Wbs/Domain/Enums/WbsViewMode.cs` — Standard/PrimaveraColors/WeightView/ResponsibilityView
- [x] T013 [P] Create `Planova.Wbs/Domain/Interfaces/IWbsMappingService.cs` per contracts
- [x] T014 [P] Enhance `Planova.Wbs/Domain/Interfaces/IWbsAiGenerationService.cs` per contracts
- [x] T015 [P] Create `Planova.Wbs/Domain/Interfaces/IWbsCodeGenerationService.cs` per contracts
- [x] T016 [P] Create `Planova.Wbs/Domain/Interfaces/IWbsLockService.cs` per contracts
- [x] T017 [P] Create `Planova.Application/Interfaces/IProjectFolderService.cs` per contracts
- [x] T018 [P] Create `Planova.Application/Interfaces/IPartyService.cs` per contracts
- [x] T019 [P] Create `Planova.UI/Styles/FluentCardStyles.xaml` — glass-effect card styles (acrylic, rounded corners, shadow)
- [x] T020 [P] Create `Planova.UI/Styles/DashboardStyles.xaml` — dashboard-specific styles
- [x] T021 [P] Create `Planova.UI/Converters/HealthToColorConverter.cs` — health enum to color
- [x] T022 [P] Create `Planova.UI/Converters/WbsStatusToIconConverter.cs` — status to icon
- [x] T023 [P] Create `Planova.Localization/Resources/DashboardResources.en.resx`
- [x] T024 [P] Create `Planova.Localization/Resources/DashboardResources.ar.resx`
- [x] T025 [P] Create `Planova.Localization/Resources/PartyResources.en.resx`
- [x] T026 [P] Create `Planova.Localization/Resources/PartyResources.ar.resx`
- [x] T027 [P] Create `Planova.Localization/Resources/BoqImportResources.en.resx`
- [x] T028 [P] Create `Planova.Localization/Resources/BoqImportResources.ar.resx`
- [x] T029 [P] WbsResources already existed — confirmed
- [x] T030 [P] WbsResources already existed — confirmed
- [x] T031 Register services in Boq and Wbs DI extensions, add IProjectFolderService and IPartyService to App.xaml.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Persistence entities, EF Core configurations, and schema migration that MUST be complete before user stories

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T032 [P] Create `Planova.Persistence/EntityConfigurations/ProjectMetadataConfiguration.cs` — enhanced Project fields per data-model.md
- [x] T033 [P] Create `Planova.Persistence/EntityConfigurations/PartyConfiguration.cs` per data-model.md
- [x] T034 [P] Create `Planova.Persistence/EntityConfigurations/ProjectPartyLinkConfiguration.cs` — many-to-many link (new entity)
- [x] T035 [P] Create `Planova.Persistence/EntityConfigurations/BoqImportSessionConfiguration.cs` per data-model.md
- [x] T036 [P] Create `Planova.Persistence/EntityConfigurations/WbsEditLockConfiguration.cs` per data-model.md
- [x] T037 Create `Planova.Persistence/Migrations/20260617092014_OverallEnhancementPlan.cs` — schema migration for new/enhanced entities
- [x] T038 Update `Planova.Persistence/PlanovaDbContext.cs` — register new DbSets and apply configurations
- [x] T039 Ensure migration `dotnet ef migrations add` runs clean against SQLite

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Dashboard Overview with Project Health (Priority: P1) 🎯 MVP

**Goal**: Redesigned dashboard with Fluent UI glass-effect cards showing project health indicators and cost/performance metrics

**Independent Test**: Open the dashboard after configuring a project with known health and cost values — verify all cards display correctly with icons and glass-effect styling.

### Implementation for User Story 1

- [x] T040 [P] [US1] Create `Planova.UI/ViewModels/Dashboard/HealthCardViewModel.cs` — schedule health %, cost health %, risk level, critical activities count
- [x] T041 [P] [US1] Create `Planova.UI/ViewModels/Dashboard/CostCardViewModel.cs` — original budget, current budget, actual cost, earned value, CPI, SPI
- [x] T042 [P] [US1] Enhance `Planova.UI/ViewModels/DashboardViewModel.cs` — orchestrates card collection, loads project data
- [x] T043 [P] [US1] Create `Planova.UI/Views/Dashboard/HealthCardView.xaml` + code-behind — glass-effect card with icon + health data
- [x] T044 [P] [US1] Create `Planova.UI/Views/Dashboard/CostCardView.xaml` + code-behind — glass-effect card with icon + cost data
- [x] T045 [US1] Update `Planova.UI/Views/Dashboard/DashboardView.xaml` — redesigned with Fluent UI, glass-effect cards, iconography per spec
- [x] T046 [US1] Register DashboardViewModel and card VMs in DI
- [x] T047 [US1] Add dashboard empty state in `DashboardView.xaml` — "No projects yet — create your first project" with guidance
- [x] T048 [US1] Add loading indicator for async dashboard data load

**Checkpoint**: Dashboard is fully functional — health and cost cards display with icons and glass-effect styling

---

## Phase 4: User Story 6 - Project Screen with Full Metadata (Priority: P2)

**Goal**: Enriched Projects screen showing all metadata fields, connected system info, document preview, Google Maps link, and cost/performance cards

**Independent Test**: Configure a project with all metadata fields populated — verify display on Projects screen with icons.

### Implementation for User Story 6

- [x] T049 [P] [US6] Create `Planova.UI/ViewModels/Projects/ProjectListViewModel.cs` — rich metadata per spec (progress, health, dates, budget, activities, logo, cover, XER, DB, import/export dates)
- [x] T050 [P] [US6] Create `Planova.UI/ViewModels/Projects/ProjectDetailViewModel.cs` — folder open, document preview, layout selector, Google Maps
- [x] T051 [US6] Create `Planova.UI/Views/Projects/ProjectListView.xaml` — richer grid with icons, metadata columns per spec
- [x] T052 [US6] Create `Planova.UI/Views/Projects/ProjectDetailView.xaml` — add folder button, document preview panel, layout selector (images/DWG/PDF), Google Maps link with location preview
- [x] T053 [US6] Keep parties visible in a separate rail under project area (FR-052) in ProjectDetailView
- [x] T054 [US6] Add project logo + cover image display in ProjectDetailView
- [x] T055 [US6] Add empty state for Projects list — "No projects yet" with create guidance
- [x] T056 [US6] Add loading indicator for project list async load

**Checkpoint**: Projects screen shows all metadata, document preview, map integration, and parties rail

---

## Phase 5: User Story 4 - Project Folder Auto-Creation (Priority: P2)

**Goal**: Automatic creation of 13 standard subfolders when a project folder path is assigned

**Independent Test**: Create a new project, assign a folder path — verify all 13 subfolders are created on disk.

### Implementation for User Story 4

- [x] T057 [P] [US4] Implement `Planova.Application/Services/ProjectFolderService.cs` — `IProjectFolderService` contract
- [x] T058 [US4] Update `ProjectDetailViewModel` — trigger folder creation when ProjectFolderPath is assigned
- [x] T059 [US4] Handle folder path change — create new structure, do not delete old (edge case per spec)
- [x] T060 [US4] Add folder creation error handling — show user-friendly message on permission denied / disk full

**Checkpoint**: Project folder auto-creation works — 13 subfolders created on assign, non-destructive on change

---

## Phase 6: User Story 2 - Multi-Sheet BOQ Import with Auto-Detection (Priority: P1)

**Goal**: Multi-worksheet BOQ import with auto-detection, column mapping, merge modes, preview grid, and import summary

**Independent Test**: Import an Excel file with 3 BOQ worksheets and 2 non-BOQ worksheets using merge mode — verify one consolidated BOQ with 3 section nodes and correct summary.

### Implementation for User Story 2

- [x] T061 [P] [US2] Implement `Planova.Boq/Application/Services/BoqColumnMappingService.cs` — heuristic column name matching per research.md
- [x] T062 [US2] Implement `Planova.Boq/Application/Services/BoqImportService.cs` — scan, preview, import with Single/Multiple/Merge modes per spec (existing impl)
- [x] T063 [US2] Implement `Planova.Excel/Import/WorksheetScanner.cs` — scan worksheets, detect BOQ columns, skip non-BOQ sheets
- [x] T064 [US2] Implement merge-mode logic — root BOQ → one section node per worksheet, preserve sheet order (existing impl)
- [x] T065 [US2] Implement import summary — total sheets, sections, items, amount (existing impl)
- [x] T066 [US2] Save original worksheet name as `SourceSheet` per FR-018 (existing impl)
- [x] T067 [US2] Stream large Excel files (>50MB/100K rows) without loading all worksheets into memory per FR-020 (existing impl)
- [x] T068 [P] [US2] Create `Planova.UI/ViewModels/Boq/BoqImportViewModel.cs` — existed, verified
- [x] T069 [P] [US2] Create `Planova.UI/ViewModels/Boq/BoqColumnMappingViewModel.cs` — column mapping UI
- [x] T070 [P] [US2] Create `Planova.UI/Views/Boq/BoqImportView.xaml` — existed as BoqImportWizardView
- [x] T071 [P] [US2] Create `Planova.UI/Views/Boq/BoqImportPreviewView.xaml` — preview grid with split view per FR-025
- [x] T072 [P] [US2] Create `Planova.UI/ViewModels/Boq/BoqTreeViewModel.cs` — existed, verified
- [x] T073 [US2] Update `Planova.UI/Views/Boq/BoqTreeView.xaml` — add split view (existing impl)
- [x] T074 [US2] Remove tab editor, validate BOQ tab (merge into import flow), libraries tab per FR-022 (existing impl)
- [x] T075 [US2] Integrate BOQ Studio with ProjectId per FR-023 (existing impl)
- [x] T076 [US2] Implement `Planova.Boq/Application/Services/BoqExportService.cs` — existed, verified
- [x] T077 [P] [US2] Create `Planova.UI/ViewModels/Boq/BoqExportViewModel.cs`
- [x] T078 [P] [US2] Create `Planova.UI/Views/Boq/BoqExportView.xaml`
- [x] T079 [US2] Add BOQ reports: BOQ summary, cost summary, trade summary, CSI summary per FR-027 (existing impl)
- [x] T080 [US2] Add loading indicator for BOQ import async operations
- [x] T081 [US2] Add empty state for BOQ table — "No BOQ data — import an Excel file to get started"

**Checkpoint**: Multi-sheet BOQ import works end-to-end with auto-detection, merge, preview, summary, and export

---

## Phase 7: User Story 5 - WBS Editor with Drag-and-Drop (Priority: P2)

**Goal**: Full WBS Editor tab with tree manipulation, drag-and-drop, properties panel, auto-code generation, and edit locks

**Independent Test**: Add child/sibling nodes, drag a node to new parent, edit properties in side panel — verify codes auto-regenerate consistently.

### Implementation for User Story 5

- [x] T082 [P] [US5] Implement `Planova.Wbs/Application/Services/WbsCodeGenerationService.cs` — auto-generate dot-notation codes (1, 1.1, 1.2, 1.2.1), regenerate on structure change per FR-041
- [x] T083 [P] [US5] Implement `Planova.Wbs/Application/Services/WbsLockService.cs` — acquire/release/refresh locks per FR-063, FR-064
- [x] T084 [P] [US5] Create `Planova.UI/ViewModels/Wbs/WbsEditorViewModel.cs` — existed, verified
- [x] T085 [US5] Create `Planova.UI/Views/Wbs/WbsEditorView.xaml` — existed, verified
- [x] T086 [US5] Implement tree manipulation: Add Root, Add Child, Add Sibling, Delete, Duplicate, Rename per FR-037 (existing impl)
- [x] T087 [US5] Implement button-based reordering: Move Up, Move Down, Indent, Outdent per FR-038 (existing impl)
- [x] T088 [US5] Implement drag-and-drop reordering: change order, change parent, change level per FR-038 (existing impl)
- [x] T089 [US5] Implement context menu: Add Child, Add Sibling, Duplicate, Rename, Delete, Move Up/Down, Indent/Outdent per FR-039 (existing impl)
- [x] T090 [US5] Implement properties panel: Code, Name, Description, Level, Weight, Owner, Discipline, Deliverable, Start/Finish Date, Notes per FR-040 (existing impl)
- [x] T091 [US5] Wire auto-code regeneration to trigger after every structural change per FR-041
- [x] T092 [US5] Implement edit lock UI — show lock notice when WBS is locked by another user per FR-064
- [x] T093 [US5] Add cycle detection for tree structure changes per edge case in spec (existing impl)
- [x] T094 [P] [US5] Create `Planova.UI/ViewModels/Wbs/WbsListViewModel.cs` — existed, verified
- [x] T095 [P] [US5] Create `Planova.UI/Views/Wbs/WbsListView.xaml` — existed, verified
- [x] T096 [P] [US5] Create `Planova.UI/ViewModels/Wbs/WbsTreeViewModel.cs` — existed, verified
- [x] T097 [P] [US5] Create `Planova.UI/Views/Wbs/WbsTreeView.xaml` — existed, verified
- [x] T098 [P] [US5] Create `Planova.UI/ViewModels/Wbs/WbsSettingsViewModel.cs` — existed, verified
- [x] T099 [P] [US5] Create `Planova.UI/Views/Wbs/WbsSettingsView.xaml` — existed, verified
- [x] T100 [US5] Add empty state for WBS list — "No WBS structures yet — create one from BOQ, template, or manually" (existing impl)
- [x] T101 [US5] Add loading indicator for WBS tree and editor async operations (existing impl)

**Checkpoint**: WBS Editor fully functional — tree manipulation, drag-drop, properties panel, auto-code gen, edit locks

---

## Phase 8: User Story 3 - WBS Creation from BOQ Mapping (Priority: P1)

**Goal**: Generate WBS hierarchies from existing BOQ data using multiple mapping methods, with preview and confirmation

**Independent Test**: Select a BOQ with sections, map by Section method — verify preview shows correct hierarchy, confirm creation — WBS appears in List tab with auto-generated codes.

**⚠️ Depends on**: US2 (BOQ Import) and US5 (WBS Editor) — BOQ entities and WBS infrastructure must exist

### Implementation for User Story 3

- [x] T102 [P] [US3] Implement `Planova.Wbs/Application/Services/WbsMappingService.cs` — mapping engine per FR-042
- [x] T103 [US3] Implement mapping methods: By Section, By CSI, By Cost Code, By Trade, By Discipline per FR-042
- [x] T104 [US3] Implement preview generation — show WBS hierarchy before creation per FR-042
- [ ] T105 [US3] Wire WBS mapping to BOQ integration per FR-028 (BOQ → Schedule integration) ⏳ Blocked by Schedule module
- [x] T106 [P] [US3] Create `Planova.UI/ViewModels/Wbs/WbsMappingViewModel.cs` — existed, verified
- [x] T107 [P] [US3] Create `Planova.UI/Views/Wbs/WbsMappingView.xaml` — mapping interface per spec
- [x] T108 [P] [US3] Create `Planova.UI/ViewModels/Wbs/WbsAiGenerationViewModel.cs` — existed, verified
- [x] T109 [P] [US3] Create `Planova.UI/Views/Wbs/WbsAiGenerationView.xaml` — existed, verified
- [x] T110 [US3] Implement `Planova.Wbs/Application/Services/WbsAiGenerationService.cs` — existed, verified
- [x] T111 [US3] Implement graceful AI degradation — error message + retry/cancel when AI API unreachable per FR-062 (existing impl)
- [x] T112 [US3] Implement AI generation workflow: select project → select BOQ → select documents → generate → accept → create WBS per FR-046 (existing impl)
- [x] T113 [P] [US3] Create `Planova.UI/ViewModels/Wbs/WbsReportsViewModel.cs` — reports per FR-048
- [x] T114 [P] [US3] Create `Planova.UI/Views/Wbs/WbsReportsView.xaml` — WBS Dictionary, Summary, Weight, Responsibility Matrix, Hierarchy Report
- [x] T115 [US3] Implement WBS export: Excel, PDF, CSV, JSON per FR-049
- [x] T116 [US3] Implement WBS Templates tab per FR-043, FR-044: building, infrastructure, landscape, roads, water network, sewer network, industrial, power plant templates
- [x] T117 [P] [US3] Create `Planova.UI/ViewModels/Wbs/WbsTemplatesViewModel.cs`
- [x] T118 [P] [US3] Create `Planova.UI/Views/Wbs/WbsTemplatesView.xaml`
- [x] T119 [US3] Add empty state for WBS mapping — "No BOQ available — import a BOQ first"
- [x] T120 [US3] Add loading indicator for AI generation and mapping preview

**Checkpoint**: WBS can be created from BOQ mapping and AI generation — full mapping lifecycle complete

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T121 [P] Add English localization strings for all new UI in resx files
- [x] T122 [P] Add Arabic localization strings for all new UI in resx files
- [x] T123 Verify RTL layout support for all new screens
- [x] T124 Run `dotnet build` — fix any compilation errors across all touched modules
- [x] T125 Code cleanup and consistency pass across all new/enhanced files
- [x] T126 Verify navigation rail + multi-tab workspace pattern is followed per constitution
- [x] T127 Verify Clean Architecture dependency direction in all new files
- [x] T128 Run `dotnet format` on all touched projects
- [x] T129 Run quickstart.md validation — verify all workstreams are covered

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - US1 (Phase 3): No story dependencies — can start first
  - US6 (Phase 4): Can start after Phase 2
  - US4 (Phase 5): Can start after Phase 2
  - US2 (Phase 6): Can start after Phase 2
  - US5 (Phase 7): Can start after Phase 2
  - US3 (Phase 8): Depends on US2 (BOQ data) and US5 (WBS infrastructure)
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies on other stories — MVP
- **User Story 2 (P1)**: No dependencies on other stories
- **User Story 3 (P1)**: Depends on US2 (needs BOQ data) and US5 (needs WBS Editor infrastructure)
- **User Story 4 (P2)**: No dependencies on other stories
- **User Story 5 (P2)**: No dependencies on other stories
- **User Story 6 (P2)**: No dependencies on other stories

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel
- Once Foundational completes: **US1, US2, US4, US5, US6** can all start in parallel
- US3 must wait for US2 + US5 to complete
- Within a story, [P] tasks can run in parallel
- Model/service/file creation tasks within each story phase that touch different files are independent

### Parallel Example: User Story 1

```powershell
# Launch all ViewModels for US1 together:
Task: T040 Create Planova.UI/ViewModels/Dashboard/HealthCardViewModel.cs
Task: T041 Create Planova.UI/ViewModels/Dashboard/CostCardViewModel.cs
Task: T042 Create Planova.UI/ViewModels/Dashboard/DashboardViewModel.cs

# Launch all Views for US1 in parallel:
Task: T043 Create Planova.UI/Views/Dashboard/HealthCardView.xaml
Task: T044 Create Planova.UI/Views/Dashboard/CostCardView.xaml
```

### Parallel Example: User Story 2

```powershell
# Launch import service and UI ViewModels in parallel:
Task: T061 Implement Planova.Boq/Application/Services/BoqColumnMappingService.cs
Task: T062 Implement Planova.Boq/Application/Services/BoqImportService.cs
Task: T068 Create Planova.UI/ViewModels/Boq/BoqImportViewModel.cs
Task: T070 Create Planova.UI/Views/Boq/BoqImportView.xaml
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 — both P1, no dependency on each other)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (Dashboard)
4. Complete Phase 6: User Story 2 (BOQ Import — parallel with US1)
5. **STOP and VALIDATE**: Test both P1 stories independently
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Dashboard) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (BOQ Import) → Test independently → Deploy/Demo
4. Add US5 (WBS Editor) → Test independently → Deploy/Demo
5. Add US3 (WBS Mapping) → Test independently → Deploy/Demo
6. Add US6 (Projects) → Test independently → Deploy/Demo
7. Add US4 (Folder) → Test independently → Deploy/Demo
8. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Dashboard)
   - Developer B: US2 (BOQ Import)
   - Developer C: US5 (WBS Editor)
   - Developer D: US6 (Projects) + US4 (Folder)
3. After US2 + US5 complete: Developer A/B/C pick up US3 (WBS Mapping)
4. All stories complete and integrate independently

---

## Summary

| Metric | Value |
|--------|-------|
| **Total tasks** | 129 (T001–T129) |
| **User stories** | 6 (US1–US6) |
| **Parallelizable tasks** | 57 marked [P] |
| **MVP scope** | US1 (Dashboard) + US2 (BOQ Import) |
| **P1 stories** | 3 (US1, US2, US3) |
| **P2 stories** | 3 (US4, US5, US6) |
