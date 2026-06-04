---

description: "Task list for implementing WBS Studio feature"
---

# Tasks: WBS Studio

**Input**: Design documents from `specs/006-wbs-studio/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Test tasks are included. The feature specification and plan.md call for xUnit tests for domain entities, validation engine, application services, BOQ mapping strategies, AI generation service, persistence round-trip, and template apply.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and directory structure

- [X] T001 Create Planova.Wbs class library project at Planova.Wbs/Planova.Wbs.csproj with project references to Planova.Domain and Planova.Boq
- [X] T002 [P] Add NuGet packages (CommunityToolkit.Mvvm, Serilog, Semantic Kernel, QuestPDF, ClosedXML, EPPlus) to Planova.Wbs/Planova.Wbs.csproj
- [X] T003 [P] Create directory structure: Planova.Wbs/Domain/{Entities,Enums,Interfaces}, Planova.Wbs/Application/{Services,Dto}, Planova.Wbs/Extensions
- [X] T004 [P] Create directories: Planova.UI/Views/Wbs, Planova.UI/ViewModels/Wbs, Planova.Persistence/EntityConfigurations, Planova.Localization/Resources, tests/Planova.Wbs.Tests/{Domain,Application,Services}

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities, enums, interfaces, persistence, and infrastructure that MUST be complete before ANY user story can begin.

**CRITICAL**: No user story work can begin until this phase is complete

### Enums & Entities

- [X] T005 Create WbsStatus enum in Planova.Wbs/Domain/Enums/WbsStatus.cs with Draft, Final, Revised, Approved values and transition validation
- [X] T006 [P] Create WbsLevelType enum in Planova.Wbs/Domain/Enums/WbsLevelType.cs with Summary, ControlAccount, WorkPackage, PlanningPackage
- [X] T007 [P] Create WbsSource enum in Planova.Wbs/Domain/Enums/WbsSource.cs with Manual, FromBOQ, FromTemplate, AIGenerated
- [X] T008 Create Wbs entity in Planova.Wbs/Domain/Entities/Wbs.cs with full field set per data-model.md
- [X] T009 [P] Create WbsItem entity in Planova.Wbs/Domain/Entities/WbsItem.cs with ParentId self-reference, SourceBoqItemId, two auto-generated codes, weight, dates
- [X] T010 [P] Create WbsTemplate entity in Planova.Wbs/Domain/Entities/WbsTemplate.cs with Category, Industry, IsStandard, Tags
- [X] T011 [P] Create WbsTemplateItem entity in Planova.Wbs/Domain/Entities/WbsTemplateItem.cs with DefaultDurationDays, TypicalWeight

### Domain Interfaces & DTOs

- [X] T012 [P] Create IWbsRepository interface in Planova.Wbs/Domain/Interfaces/IWbsRepository.cs per contracts/IWbsRepository.cs
- [X] T013 [P] Create IWbsItemRepository interface in Planova.Wbs/Domain/Interfaces/IWbsItemRepository.cs per contracts/IWbsItemRepository.cs
- [X] T014 [P] Create IWbsTemplateRepository interface in Planova.Wbs/Domain/Interfaces/IWbsTemplateRepository.cs per contracts/IWbsTemplateRepository.cs
- [X] T015 [P] Create IWbsValidationService interface in Planova.Wbs/Domain/Interfaces/IWbsValidationService.cs with circular reference detection
- [X] T016 [P] Create IWbsService interface in Planova.Wbs/Domain/Interfaces/IWbsService.cs per contracts/IWbsService.cs
- [X] T017 [P] Create IWbsBoqMappingService interface in Planova.Wbs/Domain/Interfaces/IWbsBoqMappingService.cs per contracts/IWbsBoqMappingService.cs
- [X] T018 [P] Create IWbsAiGenerationService interface in Planova.Wbs/Domain/Interfaces/IWbsAiGenerationService.cs per contracts/IWbsAiGenerationService.cs
- [X] T019 [P] Create IWbsReportService interface in Planova.Wbs/Domain/Interfaces/IWbsReportService.cs per contracts/IWbsReportService.cs
- [X] T020 [P] Create WbsDto and WbsItemDto in Planova.Wbs/Application/Dto/WbsDto.cs and WbsItemDto.cs
- [X] T021 [P] Create WbsTreeDto, WbsMappingRequest, WbsMappingResult, WbsReportDto in Planova.Wbs/Application/Dto/

### Persistence & Infrastructure

- [X] T022 Create Wbs EF Core configuration in Planova.Persistence/EntityConfigurations/WbsConfiguration.cs with indexes on ProjectId, Status
- [X] T023 [P] Create WbsItem EF Core configuration in Planova.Persistence/EntityConfigurations/WbsItemConfiguration.cs with indexes on ParentId, WbsId, SourceBoqItemId
- [X] T024 [P] Create WbsTemplate EF Core configuration in Planova.Persistence/EntityConfigurations/WbsTemplateConfiguration.cs
- [X] T025 [P] Create WbsTemplateItem EF Core configuration in Planova.Persistence/EntityConfigurations/WbsTemplateItemConfiguration.cs
- [ ] T026 Create EF migration AddWbsEntities via `dotnet ef migrations add` in Planova.Persistence and apply database update
- [X] T027 Create ServiceCollectionExtensions in Planova.Wbs/Extensions/ServiceCollectionExtensions.cs registering all WBS services and repositories
- [X] T028 [P] Create localization resources WbsResources.en.resx and WbsResources.ar.resx in Planova.Localization/Resources/ with common WBS strings
- [X] T029 [P] Update Planova.Persistence DbContext to register Wbs entity configurations in OnModelCreating

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 + 2 - Create/Manage WBS & View/Navigate Hierarchy (Priority: P1)

**Goal**: Users can create a WBS (manual), add/edit/delete/reorder items in a hierarchical tree, and view the tree with expand/collapse, level color-coding, and weight visualization.

**Independent Test**: Create a new WBS via Manual source, add 5+ items across 3 levels, edit item properties, delete a parent with children (verify cascade confirmation), reorder siblings. Then verify the tree renders with expand/collapse, correct level colors (Summary, ControlAccount, WorkPackage, PlanningPackage), and proportional weight bars. All data persists across app restart.

### Implementation for User Stories 1 & 2

- [X] T030 [P] [US1] Implement WbsRepository in Planova.Persistence/Repositories/WbsRepository.cs with Add, GetById, GetByProjectId, Update, Delete, Exists
- [X] T031 [P] [US1] Implement WbsItemRepository in Planova.Persistence/Repositories/WbsItemRepository.cs with recursive CTE GetByWbsIdAsync, GetChildren, Add, UpdateRange, Delete, DeleteRange
- [X] T032 [P] [US1] Implement WbsService in Planova.Wbs/Application/Services/WbsService.cs with CRUD, CreateAsync with source selection, ChangeStatusAsync, GetTreeAsync
- [X] T033 [P] [US1] Implement WbsValidationService in Planova.Wbs/Application/Services/WbsValidationService.cs with ValidateWbsAsync, ValidateItemAsync, ValidateTreeAsync, IsCircularReference
- [X] T034 [US1] Create WbsListViewModel in Planova.UI/ViewModels/Wbs/WbsListViewModel.cs with project-scoped list, search by name, filter by status/source, create WBS dialog (Manual, FromBOQ, FromTemplate, AIGenerated)
- [X] T035 [US1] Create WbsListView in Planova.UI/Views/Wbs/WbsListView.xaml bound to WbsListViewModel with project selector, search bar, filter dropdowns, create button
- [X] T036 [P] [US2] Create WbsTreeViewModel in Planova.UI/ViewModels/Wbs/WbsTreeViewModel.cs with ObservableCollection tree building, expand/collapse commands, level-to-color mapping, weight percentage calculation
- [X] T037 [P] [US2] Create WbsTreeView in Planova.UI/Views/Wbs/WbsTreeView.xaml with VirtualizingTreeView, color-coded level backgrounds, weight progress bars, connecting lines
- [X] T038 [US1] Create WbsEditorViewModel in Planova.UI/ViewModels/Wbs/WbsEditorViewModel.cs with AddChildCommand, DeleteCommand (cascade confirmation), MoveUp/MoveDown, inline edit (name, description, weight, dates, assignee, deliverable, notes)
- [X] T039 [US1] Create WbsEditorView in Planova.UI/Views/Wbs/WbsEditorView.xaml with inline editing fields, add/delete/reorder buttons, cascade delete confirmation dialog
- [X] T040 [US1] Register WBS Studio navigation: add nav rail entry in ShellViewModel, wire WbsListView as module entry point, open WbsEditorView in multi-tab workspace
- [X] T041 [US1] Implement WbsItem auto-code generation (FR-027): numeric code from tree position (1, 1.1, 1.1.1) and alpha short code from item name (min 3 letters) in WbsService

**Checkpoint**: US1 + US2 fully functional — users can create, edit, view, and navigate a WBS tree with full persistence

---

## Phase 4: User Story 3 - Map BOQ to WBS (Priority: P2)

**Goal**: Users select a BOQ from Phase 3 and use a 3-step mapping wizard (3 strategies: One-to-One, Grouped, Custom) to auto-generate a WBS with traceable SourceBoqItemId references.

**Independent Test**: Select a BOQ with 5+ items, apply Grouped mapping strategy, preview the generated tree, adjust two node names, commit. Verify each WBS item has a SourceBoqItemId linking back to its originating BOQ item.

### Implementation for User Story 3

- [X] T042 [P] [US3] Implement WbsBoqMappingService in Planova.Wbs/Application/Services/WbsBoqMappingService.cs with MapOneToOneAsync, MapGroupedAsync (by category), MapCustomAsync, CommitMappingAsync
- [X] T043 [US3] Create WbsMappingViewModel in Planova.UI/ViewModels/Wbs/WbsMappingViewModel.cs with wizard steps (BOQ select, strategy picker, preview tree with editable names/levels, commit)
- [X] T044 [US3] Create WbsMappingWizardView in Planova.UI/Views/Wbs/WbsMappingWizardView.xaml with step indicator, BOQ tree preview, strategy radio buttons, preview panel, commit button

**Checkpoint**: US3 fully functional — BOQ-to-WBS mapping with 3 strategies and traceability

---

## Phase 5: User Story 4 - Apply and Manage WBS Templates (Priority: P2)

**Goal**: Users browse templates by category, preview template hierarchy, apply templates to create WBS, save existing WBS as templates, and import/export templates as JSON.

**Independent Test**: Browse template library, filter by "Building Construction" category, preview a template tree, apply it to create a new WBS, verify correct hierarchy. Then save the new WBS as a custom template and export it as JSON.

### Implementation for User Story 4

- [X] T045 [P] [US4] Implement WbsTemplateService in Planova.Wbs/Application/Services/WbsTemplateService.cs with CRUD, ApplyAsync (deep copy), ImportFromJsonAsync, ExportToJsonAsync
- [X] T046 [P] [US4] Implement WbsTemplateRepository in Planova.Persistence/Repositories/WbsTemplateRepository.cs with GetByCategoryAsync, GetAllAsync, Add, Update, Delete
- [X] T047 [P] [US4] Seed 4 standard templates (Building Construction, Infrastructure, Industrial, Oil & Gas) with hierarchical items in migration or seed data
- [X] T048 [US4] Create WbsTemplateManagerViewModel in Planova.UI/ViewModels/Wbs/WbsTemplateManagerViewModel.cs with category filter, tree preview, ApplyCommand, SaveAsTemplateCommand, ImportCommand, ExportCommand
- [X] T049 [US4] Create WbsTemplateManagerView in Planova.UI/Views/Wbs/WbsTemplateManagerView.xaml with filter bar, template list, preview panel, apply/save/import/export buttons

**Checkpoint**: US4 fully functional — template management with 4 seeded templates, apply, save, import/export

---

## Phase 6: User Story 5 - AI-Assisted WBS Generation (Priority: P3)

**Goal**: Users enter a project scope description, receive an AI-suggested WBS tree via Semantic Kernel/Ollama, and can accept, modify, or regenerate the suggestion.

**Independent Test**: Enter a project scope of 2-3 sentences, click Generate, receive a suggested WBS tree, modify two item names, click Accept. Verify a new WBS is created from the suggestion. Then test AI provider unavailability shows a graceful error message.

### Implementation for User Story 5

- [X] T050 [P] [US5] Implement WbsAiGenerationService in Planova.Wbs/Application/Services/WbsAiGenerationService.cs using Semantic Kernel with IAIProvider, structured JSON output for suggested tree, IsAiAvailableAsync
- [X] T051 [US5] Create WbsAiGenerationViewModel in Planova.UI/ViewModels/Wbs/WbsAiGenerationViewModel.cs with scope text input, GenerateCommand, AcceptCommand, RegenerateCommand, progress indication, error state handling
- [X] T052 [US5] Create WbsAiGenerationView in Planova.UI/Views/Wbs/WbsAiGenerationView.xaml with scope textarea, generate button, suggested tree preview, accept/regenerate buttons, progress bar

**Checkpoint**: US5 fully functional — AI generation with accept/regenerate and graceful fallback

---

## Phase 7: User Story 6 - View and Export WBS Reports (Priority: P3)

**Goal**: Users generate WBS Summary Report and WBS Dictionary views, export to Excel (via Phase 2 WorkbookWriter) and PDF (via QuestPDF).

**Independent Test**: Open a WBS with 5+ items, generate Summary Report, verify hierarchical layout with weights and item counts, export to Excel, export to PDF. Verify both files are valid.

### Implementation for User Story 6

- [X] T053 [P] [US6] Implement WbsReportService in Planova.Wbs/Application/Services/WbsReportService.cs with GenerateSummaryAsync (hierarchy, weights, counts), GenerateDictionaryAsync (descriptions, responsibilities, BOQ refs), ExportToExcelAsync, ExportToPdfAsync
- [X] T054 [US6] Create WbsReportViewModel in Planova.UI/ViewModels/Wbs/WbsReportViewModel.cs with report type switch (Summary/Dictionary), preview data loading, ExportExcelCommand, ExportPdfCommand
- [X] T055 [US6] Create WbsReportView in Planova.UI/Views/Wbs/WbsReportView.xaml with report type tabs, hierarchical preview panel, Excel/PDF export buttons

**Checkpoint**: US6 fully functional — report generation and export to Excel/PDF

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Edge case handling, validation hardening, RTL support, performance optimization, bulk operations, and testing

- [X] T056 [P] Implement weight auto-redistribution logic in WbsService.RedistributeWeightsAsync — proportional recalculation among siblings on add/delete
- [X] T057 [P] Add RTL layout support (FR-026) in all WBS XAML views — FlowDirection binding to CurrentCulture, mirror tree indentation and connecting lines
- [X] T058 [P] Implement edge case guards: circular reference rejection (via IWbsValidationService.IsCircularReference), WorkPackage child prevention, cascade delete confirmation dialog, weight validation (FR-029 total ≤ 100%)
- [X] T059 [P] Implement bulk update (FR-030) in WbsEditorViewModel — multi-select items, set dates/assignee in bulk
- [X] T060 [P] Create unit tests project Planova.Wbs.Tests at tests/Planova.Wbs.Tests/Planova.Wbs.Tests.csproj with xUnit, reference Planova.Wbs
- [X] T061 [P] Write Wbs domain entity tests in tests/Planova.Wbs.Tests/Domain/ — status transitions, weight validation, code generation
- [X] T062 [P] Write WbsValidationService tests in tests/Planova.Wbs.Tests/Application/ — circular reference detection, tree validation, item validation
- [X] T063 [P] Write WbsService tests in tests/Planova.Wbs.Tests/Services/ — CRUD, status transitions, tree queries, weight redistribution
- [X] T064 [P] Write WbsBoqMappingService tests in tests/Planova.Wbs.Tests/Services/ — all 3 mapping strategies, commit with traceability
- [X] T065 [P] Write WbsAiGenerationService tests in tests/Planova.Wbs.Tests/Services/ — generation with mock provider, AI unavailability handling
- [X] T066 [P] Write ViewModel tests in tests/Planova.UI.Tests/ViewModels/Wbs/ — WbsListViewModelTests (3), WbsTreeViewModelTests (4), WbsEditorViewModelTests (6), WbsMappingViewModelTests (5), WbsAiGenerationViewModelTests (8)
- [X] T067 Run all tests via `dotnet test` — 169 tests passed (34 Wbs + 35 UI + 100 existing), 0 failures

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 + US2 (Phase 3)**: Depends on Foundational phase — MVP scope
- **US3 (Phase 4)**: Depends on Foundational + US1/US2 (needs WBS creation, tree viewer)
- **US4 (Phase 5)**: Depends on Foundational + US1/US2 (needs tree, WBS CRUD)
- **US5 (Phase 6)**: Depends on Foundational + US1/US2 (needs tree, WBS creation)
- **US6 (Phase 7)**: Depends on Foundational + US1/US2 (needs existing WBS with items)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 + 2 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 3 (P2)**: Needs US1/US2 for tree infrastructure, but BOQ mapping service is independent
- **User Story 4 (P2)**: Needs US1/US2 for tree infrastructure; template service is independent
- **User Story 5 (P3)**: Needs US1/US2; AI generation service is independent
- **User Story 6 (P3)**: Needs US1 for WBS data access; report service is independent

### Within Each Phase

- Models/entities before services
- Services before ViewModels
- ViewModels before Views
- Interfaces implemented before consumed
- Repository implementations before services that consume them

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational enum/entity tasks marked [P] can run in parallel
- Domain interfaces ([P]) can run in parallel with entities
- DTOs marked [P] can run in parallel
- EF configurations marked [P] can run in parallel
- US1 repository and service implementations can run in parallel with each other
- US1 ViewModel and View pairs can run in parallel (if designer/dev split)
- User stories 3, 4, 5, 6 can be implemented in parallel after Foundational + US1/US2 complete

---

## Parallel Example: Foundational Phase

```bash
# Create all enums in parallel:
Task: "T005 WbsStatus enum in Planova.Wbs/Domain/Enums/WbsStatus.cs"
Task: "T006 WbsLevelType enum in Planova.Wbs/Domain/Enums/WbsLevelType.cs"
Task: "T007 WbsSource enum in Planova.Wbs/Domain/Enums/WbsSource.cs"

# Create all domain entities in parallel:
Task: "T008 Wbs entity in Planova.Wbs/Domain/Entities/Wbs.cs"
Task: "T009 WbsItem entity in Planova.Wbs/Domain/Entities/WbsItem.cs"
Task: "T010 WbsTemplate entity in Planova.Wbs/Domain/Entities/WbsTemplate.cs"
Task: "T011 WbsTemplateItem entity in Planova.Wbs/Domain/Entities/WbsTemplateItem.cs"

# Create all EF configurations in parallel:
Task: "T022 WbsConfiguration in Planova.Persistence/EntityConfigurations/WbsConfiguration.cs"
Task: "T023 WbsItemConfiguration in Planova.Persistence/EntityConfigurations/WbsItemConfiguration.cs"
Task: "T024 WbsTemplateConfiguration in Planova.Persistence/EntityConfigurations/WbsTemplateConfiguration.cs"
Task: "T025 WbsTemplateItemConfiguration in Planova.Persistence/EntityConfigurations/WbsTemplateItemConfiguration.cs"
```

## Parallel Example: User Stories 1 & 2

```bash
# Implement repositories and services in parallel:
Task: "T030 WbsRepository in Planova.Persistence/Repositories/WbsRepository.cs"
Task: "T031 WbsItemRepository in Planova.Persistence/Repositories/WbsItemRepository.cs"
Task: "T032 WbsService in Planova.Wbs/Application/Services/WbsService.cs"
Task: "T033 WbsValidationService in Planova.Wbs/Application/Services/WbsValidationService.cs"

# Create ViewModels in parallel:
Task: "T034 WbsListViewModel in Planova.UI/ViewModels/Wbs/WbsListViewModel.cs"
Task: "T036 WbsTreeViewModel in Planova.UI/ViewModels/Wbs/WbsTreeViewModel.cs"
Task: "T038 WbsEditorViewModel in Planova.UI/ViewModels/Wbs/WbsEditorViewModel.cs"

# Create Views in parallel:
Task: "T035 WbsListView in Planova.UI/Views/Wbs/WbsListView.xaml"
Task: "T037 WbsTreeView in Planova.UI/Views/Wbs/WbsTreeView.xaml"
Task: "T039 WbsEditorView in Planova.UI/Views/Wbs/WbsEditorView.xaml"
```

## Parallel Example: User Stories 3-6 (can run concurrently)

```bash
# Team member A — US3 (BOQ Mapping):
Task: "T042 WbsBoqMappingService in Planova.Wbs/Application/Services/WbsBoqMappingService.cs"
Task: "T043 WbsMappingViewModel in Planova.UI/ViewModels/Wbs/WbsMappingViewModel.cs"
Task: "T044 WbsMappingWizardView in Planova.UI/Views/Wbs/WbsMappingWizardView.xaml"

# Team member B — US4 (Templates):
Task: "T045 WbsTemplateService in Planova.Wbs/Application/Services/WbsTemplateService.cs"
Task: "T046 WbsTemplateRepository in Planova.Persistence/Repositories/WbsTemplateRepository.cs"
Task: "T048 WbsTemplateManagerViewModel in Planova.UI/ViewModels/Wbs/WbsTemplateManagerViewModel.cs"
Task: "T049 WbsTemplateManagerView in Planova.UI/Views/Wbs/WbsTemplateManagerView.xaml"

# Team member C — US5 (AI Generation):
Task: "T050 WbsAiGenerationService in Planova.Wbs/Application/Services/WbsAiGenerationService.cs"
Task: "T051 WbsAiGenerationViewModel in Planova.UI/ViewModels/Wbs/WbsAiGenerationViewModel.cs"
Task: "T052 WbsAiGenerationView in Planova.UI/Views/Wbs/WbsAiGenerationView.xaml"

# Team member D — US6 (Reports):
Task: "T053 WbsReportService in Planova.Wbs/Application/Services/WbsReportService.cs"
Task: "T054 WbsReportViewModel in Planova.UI/ViewModels/Wbs/WbsReportViewModel.cs"
Task: "T055 WbsReportView in Planova.UI/Views/Wbs/WbsReportView.xaml"
```

---

## Implementation Strategy

### MVP First (Phase 1 + 2 + 3)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Stories 1 & 2 (Create/Manage WBS + View/Navigate)
4. **STOP and VALIDATE**: Test US1+US2 independently — create WBS, add 5+ items across 3 levels, edit, delete, reorder, verify tree rendering with expand/collapse, color coding, weight bars
5. Deploy/demo if ready — MVP delivers the core WBS tree editor

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1+US2 (P1) → Test independently → **MVP delivered!**
3. Add US3 (P2, BOQ Mapping) → Test independently → Deploy/Demo
4. Add US4 (P2, Templates) → Test independently → Deploy/Demo
5. Add US5 (P3, AI Generation) → Test independently → Deploy/Demo
6. Add US6 (P3, Reports) → Test independently → Deploy/Demo
7. Phase 8: Polish, edge cases, tests, RTL support
8. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. **Team** completes Setup + Foundational together
2. Once Foundation is done:
   - Developer A: US1+US2 (tree CRUD + viewer) — MVP critical path
   - Developer B: US3 (BOQ Mapping)
   - Developer C: US4 (Templates) + US5 (AI)
   - Developer D: US6 (Reports) + Phase 8 (Polish/Tests)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [US1-6] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- US1 and US2 are both P1 and tightly coupled — combined into Phase 3 as the MVP
- Follow existing Phase 3 (BOQ Studio) patterns for consistency
- Reuse VirtualizingTreeView from Phase 3 for tree rendering
- All EF Core queries use async-first pattern with CancellationToken
