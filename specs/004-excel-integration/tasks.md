# Tasks: Excel Integration

**Input**: Design documents from `/specs/004-excel-integration/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are OPTIONAL — only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Desktop app**: `src/` at repository root
- Planova.Excel module at `src/Planova.Excel/`, Planova.UI at `src/Planova.UI/`, Planova.Persistence at `src/Planova.Persistence/`
- Tests at `tests/Planova.Excel.Tests/`, `tests/Planova.UI.Tests/`, `tests/Planova.Integration.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create Planova.Excel .NET 8 class library project at `Planova.Excel/Planova.Excel.csproj`
- [X] T002 [P] Add ClosedXML NuGet package to `Planova.Excel/Planova.Excel.csproj`
- [X] T003 [P] Add EPPlus NuGet package to `Planova.Excel/Planova.Excel.csproj`
- [X] T004 [P] Add Serilog NuGet package to `Planova.Excel/Planova.Excel.csproj`
- [X] T005 Create `Planova.Excel/Extensions/ServiceCollectionExtensions.cs` with DI registration method stub
- [X] T006 Create Planova.Excel.Tests .NET 8 project at `tests/Planova.Excel.Tests/Planova.Excel.Tests.csproj` with xUnit

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 Create ExcelMappingProfile EF Core entity configuration in `Planova.Persistence/EntityConfigurations/ExcelMappingProfileConfiguration.cs`
- [X] T008 [P] Create `WorkbookInfo` and `WorksheetInfo` models in `Planova.Excel/Models/`
- [X] T009 [P] Create `ValidationResult` and `ValidationError` models in `Planova.Excel/Models/`
- [X] T010 Add EF migration for ExcelMappingProfiles table
- [X] T011 [P] Create `IWorkbookReader` interface in `Planova.Excel/Readers/IWorkbookReader.cs`
- [X] T012 [P] Create `IWorkbookWriter` interface in `Planova.Excel/Writers/IWorkbookWriter.cs`
- [X] T013 [P] Create `IWorkbookPreviewService` interface in `Planova.Excel/Services/IWorkbookPreviewService.cs`
- [X] T014 [P] Create `IValidationService` interface in `Planova.Excel/Validation/IValidationService.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 2 - Browse and Preview Excel Workbooks (Priority: P1) 🎯 MVP

**Goal**: Users can open an Excel workbook in the Workbook Browser, see all worksheets with metadata, and preview worksheet contents in a read-only paginated grid.

**Independent Test**: Open a valid .xlsx workbook in the Workbook Browser and confirm worksheets are listed and preview data is displayed in a paginated grid.

### Implementation for User Story 2

- [X] T015 [P] [US2] Create `PreviewData` model in `Planova.Excel/Models/PreviewData.cs`
- [X] T016 [US2] Implement `IWorkbookReader` using ClosedXML in `Planova.Excel/Readers/WorkbookReader.cs`
- [X] T017 [US2] Implement `IWorkbookPreviewService` in `Planova.Excel/Services/WorkbookPreviewService.cs`
- [X] T018 [US2] Create `WorkbookBrowserViewModel` in `Planova.UI/ViewModels/Excel/WorkbookBrowserViewModel.cs`
- [X] T019 [US2] Create `WorkbookBrowserView` XAML with preview grid in `Planova.UI/Views/Excel/WorkbookBrowserView.xaml`
- [X] T020 [US2] Register WorkbookBrowserViewModel and preview services in DI at `Planova.Excel/Extensions/ServiceCollectionExtensions.cs`

**Checkpoint**: At this point, User Story 2 should be fully functional — users can browse workbooks independently

---

## Phase 4: User Story 1 - Import Data from Excel to Planova (Priority: P1)

**Goal**: Users can import projects, activities, resources, costs, and risks from an Excel workbook. The Import Wizard guides them through file selection, column mapping, validation, preview, and batch-atomic commit with duplicate handling.

**Independent Test**: Import a valid .xlsx workbook with project data and verify all records appear in Planova.

### Implementation for User Story 1

- [X] T021 [P] [US1] Create `ImportRequest` and `ImportResult` models in `Planova.Excel/Models/`
- [X] T022 [P] [US1] Create `IImportService` interface in `Planova.Excel/Services/IImportService.cs`
- [X] T023 [US1] Implement `IValidationService` with pluggable validators in `Planova.Excel/Validation/ValidationService.cs`
- [X] T024 [US1] Implement `ImportService` with batch-atomic commit in `Planova.Excel/Services/ImportService.cs`
- [X] T025 [US1] Implement duplicate detection and handling in `Planova.Excel/Services/ImportService.cs`
- [X] T026 [US1] Create `ImportViewModel` in `Planova.UI/ViewModels/Excel/ImportViewModel.cs`
- [X] T027 [US1] Create `ImportWizardView` XAML (multi-step wizard) in `Planova.UI/Views/Excel/ImportWizardView.xaml`
- [X] T028 [US1] Register ImportService and ImportViewModel in DI at `Planova.Excel/Extensions/ServiceCollectionExtensions.cs`

**Checkpoint**: At this point, Users can fully import Excel data into Planova with validation and duplicate handling

---

## Phase 5: User Story 3 - Export Planova Data to Excel (Priority: P2)

**Goal**: Users can export Planova entities (projects, activities, resources) to a .xlsx workbook by selecting entity type, choosing columns, and generating the file.

**Independent Test**: Export Planova data and confirm the resulting .xlsx workbook contains the expected entities and columns when opened in Excel.

### Implementation for User Story 3

- [X] T029 [P] [US3] Create `ExportRequest` and `ExportResult` models in `Planova.Excel/Models/`
- [X] T030 [P] [US3] Create `IExportService` interface in `Planova.Excel/Services/IExportService.cs`
- [X] T031 [US3] Implement `IWorkbookWriter` using ClosedXML in `Planova.Excel/Writers/WorkbookWriter.cs`
- [X] T032 [US3] Implement `ExportService` in `Planova.Excel/Services/ExportService.cs`
- [X] T033 [US3] Create `ExportViewModel` in `Planova.UI/ViewModels/Excel/ExportViewModel.cs`
- [X] T034 [US3] Create `ExportWizardView` XAML in `Planova.UI/Views/Excel/ExportWizardView.xaml`
- [X] T035 [US3] Register ExportService and ExportViewModel in DI

**Checkpoint**: Users can export Planova data to Excel workbooks independently

---

## Phase 6: User Story 4 - Save and Reuse Mapping Profiles (Priority: P3)

**Goal**: Users can save, edit, delete, clone, and reuse column mapping profiles. Saved profiles auto-apply to matching imports.

**Independent Test**: Save a mapping profile after an import, then start a new import and verify the saved profile is available and auto-maps columns.

### Implementation for User Story 4

- [X] T036 [P] [US4] Create `MappingProfile` model with JSON serialization in `Planova.Excel/Models/MappingProfile.cs`
- [X] T037 [P] [US4] Create `IMappingProfileService` interface in `Planova.Excel/Services/IMappingProfileService.cs`
- [X] T038 [US4] Implement `MappingProfileService` with EF Core persistence in `Planova.Excel/Services/MappingProfileService.cs`
- [X] T039 [US4] Create `MappingProfilesViewModel` in `Planova.UI/ViewModels/Excel/MappingProfilesViewModel.cs`
- [X] T040 [US4] Create `MappingProfilesView` XAML (list, create, edit, delete, clone) in `Planova.UI/Views/Excel/MappingProfilesView.xaml`
- [X] T041 [US4] Integrate mapping profile selection into ImportViewModel/ImportWizardView
- [X] T042 [US4] Register MappingProfileService and MappingProfilesViewModel in DI

**Checkpoint**: Users can save, manage, and reuse mapping profiles across import sessions

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T043 [P] Add English (en) and Arabic (ar) `.resx` localization resource files for all Excel views
- [ ] T044 [P] Add RTL support for workbook browser, import wizard, export wizard, and mapping profiles views (requires XAML FlowDirection changes)
- [X] T045 [P] Add structured Serilog logging to all Planova.Excel services
- [X] T046 [P] Add CancellationToken support and progress reporting to long-running operations
- [X] T047 Security hardening: validate file extensions, strip external links, block macros
- [ ] T048 Performance optimization: implement streaming reads for large files (>10MB)
- [X] T049 Code cleanup and XML documentation on all public interfaces
- [X] T050 Run full validation against research.md decisions and quickstart.md scenarios

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 2 (Phase 3)**: Depends on Foundational completion — no dependencies on other stories
- **User Story 1 (Phase 4)**: Depends on Foundational completion; uses WorkbookReader from US2 but is independently testable with its own file handling
- **User Story 3 (Phase 5)**: Depends on Foundational completion — no dependencies on other stories
- **User Story 4 (Phase 6)**: Depends on Foundational + US1 completion (integrates into Import wizard)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 2 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **User Story 1 (P1)**: Can start after Phase 2 — independently testable with direct file selection
- **User Story 3 (P2)**: Can start after Phase 2 — fully independent of US1 and US2
- **User Story 4 (P3)**: Depends on US1 for import integration — UI and service can be built in parallel

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel
- Once Foundational completes: US2, US1, and US3 can all start in parallel
- Models within a story marked [P] can run in parallel
- Localization, RTL, and logging (Phase 7) marked [P] can run in parallel

---

## Parallel Example: User Story 1 (Import)

```bash
# Launch all models for User Story 1 together:
Task: "Create ImportRequest and ImportResult models"
Task: "Create IImportService interface"

# After models complete:
Task: "Implement ValidationService and ImportService"
Task: "Create ImportViewModel"

# After services complete:
Task: "Create ImportWizardView"
Task: "Register services in DI"
```

## Parallel Example: User Story 2 (Browse)

```bash
# Launch all models/interfaces for User Story 2 together:
Task: "Create PreviewData model"
Task: "Implement IWorkbookReader"
```

---

## Implementation Strategy

### MVP First (User Story 2 — Browse & Preview)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 2 (Browse & Preview) — enables workbook inspection
4. **STOP and VALIDATE**: Test workbook browsing independently — open any .xlsx and preview sheets
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add User Story 2 (Browse & Preview) → Test independently → Deploy/Demo (first MVP)
3. Add User Story 1 (Import) → Test independently → Deploy/Demo (core integration)
4. Add User Story 3 (Export) → Test independently → Deploy/Demo
5. Add User Story 4 (Mapping Profiles) → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Import)
   - Developer B: User Story 2 (Browse) — enables MVP
   - Developer C: User Story 3 (Export)
3. Developer D (or pooled) adds User Story 4 (Mapping Profiles) after US1 completes
4. Team picks up Polish tasks in parallel at the end

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
