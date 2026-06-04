---
description: "Task list for BOQ Studio feature implementation"
---

# Tasks: BOQ Studio

**Input**: Design documents from `specs/005-boq-studio/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

## Format: `[ID] [P] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

Projects at repository root. New project: `Planova.Boq/`. UI additions in `Planova.UI/`. Persistence configs in `Planova.Persistence/`.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create Planova.Boq class library project targeting net8.0-windows
- [X] T002 [P] Create directory structure: Domain/Entities/, Domain/Enums/, Domain/Interfaces/, Application/Services/, Application/Dto/, Application/Mappings/, CsvReader/, Extensions/
- [X] T003 [P] Add NuGet package references to Planova.Boq.csproj: CsvHelper, QuestPDF; add project reference to Planova.Excel (for Phase 2 reuse)
- [X] T004 Add Planova.Boq project to Planova.slnx solution file

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities, interfaces, and persistence that MUST be complete before ANY user story can begin

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T005 [P] Create BoqStatus enum in Planova.Boq/Domain/Enums/BoqStatus.cs (Draft, Final, Revised, Approved with transition validation)
- [X] T006 [P] Create ItemType enum in Planova.Boq/Domain/Enums/ItemType.cs (Section, Item, SubItem)
- [X] T007 [P] Create Boq entity in Planova.Boq/Domain/Entities/Boq.cs (Id, ProjectId, Name, Description, Currency, Status, RevisionNumber, TotalAmount, ImportSource, ImportedAt, Version, CreatedAt, ModifiedAt, CreatedBy, ModifiedBy)
- [X] T008 [P] Create BoqItem entity in Planova.Boq/Domain/Entities/BoqItem.cs (Id, BoqId, ParentId, Code, Description, Unit, Quantity, Rate, Amount, ItemType, Level, SortOrder, ClassificationId, CostCode, IsActive)
- [X] T009 [P] Create IBoqRepository interface in Planova.Boq/Domain/Interfaces/IBoqRepository.cs
- [X] T010 [P] Create IBoqItemRepository interface in Planova.Boq/Domain/Interfaces/IBoqItemRepository.cs
- [X] T011 Create BoqConfiguration in Planova.Persistence/EntityConfigurations/BoqConfiguration.cs (EF Core config with concurrency token on Version field)
- [X] T012 Create BoqItemConfiguration in Planova.Persistence/EntityConfigurations/BoqItemConfiguration.cs (EF Core config with self-referencing ParentId FK and cascade delete)
- [X] T013 Create ServiceCollectionExtensions in Planova.Boq/Extensions/ServiceCollectionExtensions.cs with AddPlanovaBoq() DI registration
- [ ] T014 Run EF migration to add Boq and BoqItem tables

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 — Import BOQ from Excel Using Phase 2 Infrastructure (Priority: P1) 🎯 MVP

**Goal**: Planning engineer can import BOQ data from .xlsx (via Phase 2) or .csv (standalone reader), map columns, preview tree hierarchy, validate structure, and commit to database.

**Independent Test**: Import a multi-sheet .xlsx with hierarchical BOQ rows (Code, Description, Unit, Quantity, Rate), confirm flat rows assemble into correct tree, and the BOQ viewer displays the imported hierarchy.

### Implementation for User Story 1

- [X] T015 [P] [US1] Create BoqImportRequest DTO in Planova.Boq/Application/Dto/BoqImportRequest.cs
- [X] T016 [P] [US1] Create BoqImportResult DTO in Planova.Boq/Application/Dto/BoqImportResult.cs
- [X] T017 [P] [US1] Create ImportRow record and TreeBuildStrategy enum in Planova.Boq/Application/Dto/ImportRow.cs (shared with ITreeBuilder)
- [X] T018 [P] [US1] Create CsvImportOptions record in Planova.Boq/Application/Dto/CsvImportOptions.cs
- [X] T019 [P] [US1] Create ITreeBuilder interface in Planova.Boq/Domain/Interfaces/ITreeBuilder.cs (DetectStrategy, BuildTree)
- [X] T020 [P] [US1] Create IBoqCsvReader interface in Planova.Boq/CsvReader/IBoqCsvReader.cs
- [X] T021 [P] [US1] Create IBoqImportService interface in Planova.Boq/Application/Services/IBoqImportService.cs
- [X] T022 [US1] Implement BoqCsvReader in Planova.Boq/CsvReader/BoqCsvReader.cs (use CsvHelper, detect headers, parse rows)
- [X] T023 [US1] Implement TreeBuilderService in Planova.Boq/Application/Services/TreeBuilderService.cs (Level column > ParentId column > Code prefix matching priority)
- [X] T024 [US1] Implement BoqImportService in Planova.Boq/Application/Services/BoqImportService.cs (use Phase 2 IWorkbookReader for Excel, IBoqCsvReader for CSV, ITreeBuilder for hierarchy, minimal inline validation)
- [X] T025 [US1] Create BoqImportViewModel in Planova.UI/ViewModels/Boq/BoqImportViewModel.cs (file selection, column mapping UI state, tree preview, validation results, progress reporting)
- [X] T026 [US1] Create BoqImportWizardView.xaml in Planova.UI/Views/Boq/BoqImportWizardView.xaml (multi-step wizard: select file, map columns, preview tree, validate, commit)

**Checkpoint**: User Story 1 complete — import workflow functional and independently testable

---

## Phase 4: User Story 2 — View BOQ as Hierarchical Tree (Priority: P1)

**Goal**: Planning engineer can open an existing BOQ and navigate the hierarchical tree with expand/collapse, read section subtotals and grand total.

**Independent Test**: Create a multi-level BOQ, verify tree renders correct hierarchy, expand/collapse works, subtotals and grand total compute accurately.

### Implementation for User Story 2

- [X] T027 [P] [US2] Create BoqDto in Planova.Boq/Application/Dto/BoqDto.cs
- [X] T028 [P] [US2] Create BoqItemDto in Planova.Boq/Application/Dto/BoqItemDto.cs
- [X] T029 [US2] Implement BoqService in Planova.Boq/Application/Services/BoqService.cs (CRUD for BOQ and items, with computed subtotals and total in application layer)
- [X] T030 [US2] Create BoqTreeViewModel in Planova.UI/ViewModels/Boq/BoqTreeViewModel.cs (virtualized list management, expand/collapse state, subtotal/total display)
- [X] T031 [US2] Create BoqTreeView.xaml in Planova.UI/Views/Boq/BoqTreeView.xaml (flat VirtualizingStackPanel with level indentation, expand/collapse toggles, subtotal footer, grand total footer)

**Checkpoint**: User Story 2 complete — tree viewing functional and independently testable

---

## Phase 5: User Story 3 — Edit BOQ Items Inline (Priority: P2)

**Goal**: Planning engineer adjusts quantities, rates, descriptions after import. They edit inline, add/delete items, and reorder in the tree.

**Independent Test**: Edit quantity on an item, confirm amount and parent subtotals update immediately. Add, delete (cascade), and reorder items — verify tree reflects changes.

### Implementation for User Story 3

- [X] T032 [P] [US3] Add edit methods to BoqService in Planova.Boq/Application/Services/BoqService.cs (UpdateItem, AddItem, DeleteItem, ReorderItem)
- [X] T033 [P] [US3] Add value validation to BoqService (quantity >= 0, rate >= 0, code max 50 chars, required fields)
- [X] T034 [P] [US3] Implement optimistic locking in BoqService (version check on save, throw on conflict)
- [X] T035 [US3] Create BoqEditorViewModel in Planova.UI/ViewModels/Boq/BoqEditorViewModel.cs (inline edit state, validation feedback, add/delete/reorder commands)
- [X] T036 [US3] Create BoqEditorView.xaml in Planova.UI/Views/Boq/BoqEditorView.xaml (editable fields, add/delete/reorder buttons, confirm dialogs for cascade delete)

**Checkpoint**: User Story 3 complete — inline editing functional and independently testable

---

## Phase 6: User Story 4 — Validate BOQ Integrity (Priority: P2)

**Goal**: Planning engineer runs validation on a BOQ to check for structural issues — duplicate codes, orphan items, circular references, unrecognized units.

**Independent Test**: Create a BOQ with intentional errors (duplicate codes, orphan items, invalid units, circular refs), confirm validation engine catches all issues.

### Implementation for User Story 4

- [X] T037 [P] [US4] Create IBoqValidationService interface in Planova.Boq/Domain/Interfaces/IBoqValidationService.cs
- [X] T038 [P] [US4] Create ValidationResultDto and ValidationIssue records in Planova.Boq/Application/Dto/ValidationResultDto.cs
- [X] T039 [US4] Implement BoqValidationService in Planova.Boq/Application/Services/BoqValidationService.cs (duplicate detection, orphan detection, circular reference detection, unit recognition, range validation)
- [X] T040 [US4] Create BoqValidationViewModel in Planova.UI/ViewModels/Boq/BoqValidationViewModel.cs (run validation, display issues grouped by type, suggested fixes)
- [X] T041 [US4] Create BoqValidationView.xaml in Planova.UI/Views/Boq/BoqValidationView.xaml (validation results grid, issue details panel, fix suggestions)

**Checkpoint**: User Story 4 complete — validation functional and independently testable

---

## Phase 7: User Story 5 — Classify BOQ Items (Priority: P3)

**Goal**: Cost engineer manages classification taxonomies and assigns codes to BOQ items for structured cost reporting.

**Independent Test**: Create a classification taxonomy, assign codes to items, filter items by classification code.

### Implementation for User Story 5

- [X] T042 [P] [US5] Create BoqClassification entity in Planova.Boq/Domain/Entities/BoqClassification.cs (self-referencing parent-child, Scope, ProjectId)
- [X] T043 [P] [US5] Create ClassificationScope enum in Planova.Boq/Domain/Enums/ClassificationScope.cs (Project, Global)
- [X] T044 [P] [US5] Create IBoqClassificationRepository interface in Planova.Boq/Domain/Interfaces/IBoqClassificationRepository.cs
- [X] T045 [P] [US5] Create ClassificationDto in Planova.Boq/Application/Dto/ClassificationDto.cs
- [X] T046 Create BoqClassificationConfiguration in Planova.Persistence/EntityConfigurations/BoqClassificationConfiguration.cs
- [X] T047 [US5] Implement ClassificationService in Planova.Boq/Application/Services/ClassificationService.cs (taxonomy CRUD, assign codes to items, filter by classification)
- [X] T048 [US5] Create BoqClassificationViewModel in Planova.UI/ViewModels/Boq/BoqClassificationViewModel.cs
- [X] T049 [US5] Create BoqClassificationView.xaml in Planova.UI/Views/Boq/BoqClassificationView.xaml (taxonomy tree editor, bulk assignment, filter panel)

**Checkpoint**: User Story 5 complete — classification functional and independently testable

---

## Phase 8: User Story 6 — Manage and Use BOQ Libraries (Priority: P3)

**Goal**: Planning engineer creates libraries of standard BOQ items with default rates and inserts them into active BOQs.

**Independent Test**: Create a library with standard items, insert into a BOQ at a chosen position, verify inserted items have correct defaults.

### Implementation for User Story 6

- [X] T050 [P] [US6] Create BoqLibrary entity in Planova.Boq/Domain/Entities/BoqLibrary.cs
- [X] T051 [P] [US6] Create BoqLibraryItem entity in Planova.Boq/Domain/Entities/BoqLibraryItem.cs
- [X] T052 [P] [US6] Create LibraryType enum in Planova.Boq/Domain/Enums/LibraryType.cs (System, UserDefined)
- [X] T053 [P] [US6] Create IBoqLibraryRepository interface in Planova.Boq/Domain/Interfaces/IBoqLibraryRepository.cs
- [X] T054 [P] [US6] Create LibraryDto and LibraryItemDto in Planova.Boq/Application/Dto/LibraryDto.cs
- [X] T055 Create BoqLibraryConfiguration and BoqLibraryItemConfiguration in Planova.Persistence/EntityConfigurations/
- [X] T056 [US6] Implement LibraryService in Planova.Boq/Application/Services/LibraryService.cs (library CRUD, library item CRUD, insert into BOQ)
- [X] T057 [US6] Create BoqLibraryViewModel in Planova.UI/ViewModels/Boq/BoqLibraryViewModel.cs
- [X] T058 [US6] Create BoqLibraryView.xaml in Planova.UI/Views/Boq/BoqLibraryView.xaml (library browser, item editor, insert dialog)

**Checkpoint**: User Story 6 complete — library management functional and independently testable

---

## Phase 9: User Story 7 — Generate and Export BOQ Reports (Priority: P3)

**Goal**: Planning engineer generates formatted BOQ reports (summary and itemized) and exports to Excel (via Phase 2 WorkbookWriter) or PDF (via QuestPDF).

**Independent Test**: Generate summary report and itemized report, confirm Excel and PDF files contain correct data with proper formatting.

### Implementation for User Story 7

- [X] T059 [P] [US7] Create ReportType and ReportFormat enums in Planova.Boq/Domain/Enums/ReportType.cs
- [X] T060 [P] [US7] Create IBoqReportService interface in Planova.Boq/Application/Services/IBoqReportService.cs
- [X] T061 [P] [US7] Create IBoqExportService interface in Planova.Boq/Application/Services/IBoqExportService.cs
- [X] T062 [P] [US7] Create ExportOptions and ExportResult DTOs in Planova.Boq/Application/Dto/ExportDto.cs
- [X] T063 [US7] Implement BoqReportService in Planova.Boq/Application/Services/BoqReportService.cs (summary report with section totals + item counts, itemized report with full item list — QuestPDF for PDF output)
- [X] T064 [US7] Implement BoqExportService in Planova.Boq/Application/Services/BoqExportService.cs (Excel export via Phase 2 IWorkbookWriter, CSV export via manual writer)
- [X] T065 [US7] Create BoqReportViewModel in Planova.UI/ViewModels/Boq/BoqReportViewModel.cs
- [X] T066 [US7] Create BoqReportView.xaml in Planova.UI/Views/Boq/BoqReportView.xaml (report type selector, format selector, generate button, preview/save)

**Checkpoint**: User Story 7 complete — reporting functional and independently testable

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T067 [P] Add Arabic localization resource strings for all BOQ screens in Planova.Localization/Resources/
- [ ] T068 [P] Verify RTL layout for all BOQ views (BoqTreeView, BoqImportWizardView, BoqEditorView, BoqValidationView, BoqClassificationView, BoqLibraryView, BoqReportView)
- [ ] T069 [P] Performance optimization: ensure virtualized tree handles 10k+ items at 60fps
- [ ] T070 [P] Performance optimization: lazy-load import preview for large files
- [ ] T071 Code cleanup and ensure CancellationToken passed through all async methods
- [ ] T072 Run quickstart.md validation steps and update docs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - US1 (Import) and US2 (View Tree) are P1 — start after Foundational
  - US3 (Edit) and US4 (Validate) are P2 — after P1 stories
  - US5, US6, US7 are P3 — after P2 stories
  - Stories within same priority tier can proceed in parallel
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — No dependencies on other stories
- **US2 (P1)**: Can start after Phase 2 — No dependencies on other stories (seed test data for independent testing)
- **US3 (P2)**: Depends on US2 (needs tree view to show edits) but independently testable with direct API
- **US4 (P2)**: Can start after Phase 2 — No dependencies on other stories (test with seeded data)
- **US5 (P3)**: Can start after Phase 2 — No dependencies on other stories
- **US6 (P3)**: Can start after Phase 2 — No dependencies on other stories
- **US7 (P3)**: Depends on US2 (needs tree data to report on) but independently testable with seed data

### Within Each User Story

- Models/entities before services
- Services before ViewModels
- ViewModels before Views
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel
- Within each user story, all [P]-marked tasks can run in parallel
- US1 and US2 (both P1) can start simultaneously after Phase 2
- US3 and US4 (both P2) can start simultaneously
- US5, US6, US7 (all P3) can start simultaneously

---

## Parallel Example: User Story 1

```bash
# Launch all DTOs and interfaces for US1 together:
Create Planova.Boq/Application/Dto/BoqImportRequest.cs
Create Planova.Boq/Application/Dto/BoqImportResult.cs
Create Planova.Boq/Application/Dto/ImportRow.cs
Create Planova.Boq/Application/Dto/CsvImportOptions.cs
Create Planova.Boq/Domain/Interfaces/ITreeBuilder.cs
Create Planova.Boq/CsvReader/IBoqCsvReader.cs
Create Planova.Boq/Application/Services/IBoqImportService.cs

# Launch all implementations for US1 together:
Implement BoqCsvReader.cs, TreeBuilderService.cs, BoqImportService.cs

# Launch UI for US1 together:
Create BoqImportViewModel.cs, BoqImportWizardView.xaml
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: US1 — Import BOQ from Excel
4. **STOP and VALIDATE**: Test US1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Import) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (View Tree) → Test independently → Deploy/Demo
4. Add US3 (Edit) and US4 (Validate) → Test independently → Deploy/Demo
5. Add US5 (Classify), US6 (Libraries), US7 (Reports) → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Import) — P1
   - Developer B: US2 (View Tree) — P1
3. After P1 stories:
   - Developer A: US3 (Edit) + US5 (Classify) — P2 + P3
   - Developer B: US4 (Validate) + US6 (Libraries) — P2 + P3
4. Developer C: US7 (Reports) — P3 (can start after US2)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Phase 2 IWorkbookReader, IMappingProfileService, IWorkbookWriter are already implemented — reference as project dependency
- CsvHelper and QuestPDF NuGet packages need to be added
- EF migrations should be run after each entity addition
