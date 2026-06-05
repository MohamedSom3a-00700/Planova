---

description: "Task list for implementing Activity Studio feature"

---

# Tasks: Activity Studio

**Input**: Design documents from `specs/007-activity-studio/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Test tasks are included. The feature specification and plan.md call for xUnit tests for domain entities, application services, circular reference detector, calendar date calculator, WBS generation service, Activity Bank service, and report services.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and directory structure

- [ ] T001 Create Planova.Activity class library project at Planova.Activity/Planova.Activity.csproj with project references to Planova.Domain, Planova.Shared, and Planova.Wbs
- [ ] T002 [P] Add NuGet packages (CommunityToolkit.Mvvm, QuestPDF, ClosedXML) to Planova.Activity/Planova.Activity.csproj
- [ ] T003 [P] Create directory structure: Planova.Activity/Domain/{Entities,Enums,Interfaces}, Planova.Activity/Application/{Services,Dto}, Planova.Activity/Extensions
- [ ] T004 [P] Create directories: Planova.UI/Views/Activity, Planova.UI/ViewModels/Activity, Planova.UI/Controls, Planova.Persistence/EntityConfigurations, Planova.Localization/Resources, tests/Planova.Activity.Tests/{Domain,Application,Services}

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities, enums, interfaces, persistence, and infrastructure that MUST be complete before ANY user story can begin.

**CRITICAL**: No user story work can begin until this phase is complete

### Enums

- [ ] T005 Create ActivityType enum in Planova.Activity/Domain/Enums/ActivityType.cs with Task, Milestone, LevelOfEffort, WbsSummary
- [ ] T006 [P] Create ActivityStatus enum in Planova.Activity/Domain/Enums/ActivityStatus.cs with NotStarted, InProgress, Completed, OnHold, Revise and transition validation
- [ ] T007 [P] Create RelationshipType enum in Planova.Activity/Domain/Enums/RelationshipType.cs with FS, SS, FF, SF
- [ ] T008 [P] Create CalendarType enum in Planova.Activity/Domain/Enums/CalendarType.cs with Global, Project
- [ ] T009 [P] Create CalendarDayStatus enum in Planova.Activity/Domain/Enums/CalendarDayStatus.cs with Working, NonWorking, Exception

### Entities

- [ ] T010 Create Activity entity in Planova.Activity/Domain/Entities/Activity.cs with full field set per data-model.md (Id, ProjectId, ParentActivityId, WbsItemId, CalendarId, Code, Name, Description, ActivityType, Status, Duration, PlannedStart, PlannedFinish, ActualStart, ActualFinish, PercentComplete, Weight, Notes, SortOrder, IsActive, timestamps) — note: WbsSummary fields are not directly editable (enforce via service layer, not entity)
- [ ] T011 [P] Create ActivityRelationship entity in Planova.Activity/Domain/Entities/ActivityRelationship.cs with Id, ProjectId, PredecessorId, SuccessorId, Type, LagDays, Description, timestamps
- [ ] T012 [P] Create Calendar entity in Planova.Activity/Domain/Entities/Calendar.cs with Id, ProjectId, Name, Type, HoursPerDay, DaysPerWeek, Monday-Sunday bools, IsDefault, Description, timestamps
- [ ] T013 [P] Create CalendarDay entity in Planova.Activity/Domain/Entities/CalendarDay.cs with Id, CalendarId, Date, Status, Label, timestamps
- [ ] T014 [P] Create ActivityBank entity in Planova.Activity/Domain/Entities/ActivityBank.cs with Id, Category, Subcategory, Code, Name, Description, IsStandard, Version, Tags (JSON string), timestamps
- [ ] T015 [P] Create ActivityBankItem entity in Planova.Activity/Domain/Entities/ActivityBankItem.cs with Id, BankId, ParentId (self-ref), Code, Name, Description, Level, SortOrder, DefaultDuration, DefaultActivityType, timestamps
- [ ] T016 [P] Create ActivityBankItemRelationship entity in Planova.Activity/Domain/Entities/ActivityBankItemRelationship.cs with Id, BankId, PredecessorItemId, SuccessorItemId, Type, DefaultLagDays

### Domain Interfaces

- [ ] T017 [P] Create IActivityRepository interface in Planova.Activity/Domain/Interfaces/IActivityRepository.cs per contracts/domain-interfaces.md
- [ ] T018 [P] Create IActivityRelationshipRepository interface in Planova.Activity/Domain/Interfaces/IActivityRelationshipRepository.cs per contracts/domain-interfaces.md
- [ ] T019 [P] Create ICalendarRepository interface in Planova.Activity/Domain/Interfaces/ICalendarRepository.cs per contracts/domain-interfaces.md
- [ ] T020 [P] Create ICalendarDayRepository interface in Planova.Activity/Domain/Interfaces/ICalendarDayRepository.cs per contracts/domain-interfaces.md
- [ ] T021 [P] Create IActivityBankRepository interface in Planova.Activity/Domain/Interfaces/IActivityBankRepository.cs per contracts/domain-interfaces.md
- [ ] T022 [P] Create IActivityBankItemRepository interface in Planova.Activity/Domain/Interfaces/IActivityBankItemRepository.cs per contracts/domain-interfaces.md
- [ ] T023 [P] Create IActivityBankItemRelationshipRepository interface in Planova.Activity/Domain/Interfaces/IActivityBankItemRelationshipRepository.cs per contracts/domain-interfaces.md
- [ ] T024 [P] Create IActivityService interface in Planova.Activity/Domain/Interfaces/IActivityService.cs per contracts/domain-interfaces.md
- [ ] T025 [P] Create IActivityRelationshipService interface in Planova.Activity/Domain/Interfaces/IActivityRelationshipService.cs per contracts/domain-interfaces.md
- [ ] T026 [P] Create ICalendarService interface in Planova.Activity/Domain/Interfaces/ICalendarService.cs per contracts/domain-interfaces.md
- [ ] T027 [P] Create IActivityBankService interface in Planova.Activity/Domain/Interfaces/IActivityBankService.cs per contracts/domain-interfaces.md
- [ ] T028 [P] Create IWbsGenerationService interface in Planova.Activity/Domain/Interfaces/IWbsGenerationService.cs per contracts/domain-interfaces.md
- [ ] T029 [P] Create IActivityReportService interface in Planova.Activity/Domain/Interfaces/IActivityReportService.cs per contracts/domain-interfaces.md

### DTOs

- [ ] T030 [P] Create ActivityDto, CreateActivityRequest, UpdateActivityRequest, ActivityFilter in Planova.Activity/Application/Dto/ActivityDto.cs per contracts/application-dtos.md
- [ ] T031 [P] Create ActivityRelationshipDto, CreateRelationshipRequest, UpdateRelationshipRequest, CircularReferenceCheckResult in Planova.Activity/Application/Dto/ActivityRelationshipDto.cs per contracts/application-dtos.md
- [ ] T032 [P] Create CalendarDto, CreateCalendarRequest, UpdateCalendarRequest, CalendarDayDto in Planova.Activity/Application/Dto/CalendarDto.cs per contracts/application-dtos.md
- [ ] T033 [P] Create ActivityBankDto, ActivityBankItemDto, ActivityBankItemRelationshipDto, CreateBankEntryRequest, UpdateBankEntryRequest in Planova.Activity/Application/Dto/ActivityBankDto.cs per contracts/application-dtos.md
- [ ] T034 [P] Create WbsGenerationRequest, WbsGenerationPreviewDto, ActivityPreviewItem in Planova.Activity/Application/Dto/WbsGenerationDto.cs per contracts/application-dtos.md
- [ ] T035 [P] Create ScheduleReportDto, ScheduleReportRowDto in Planova.Activity/Application/Dto/ScheduleReportDto.cs per contracts/application-dtos.md

### Persistence & Infrastructure

- [ ] T036 Create Activity EF Core configuration in Planova.Persistence/EntityConfigurations/ActivityConfiguration.cs with indexes on ProjectId, WbsItemId, CalendarId, Status, ParentActivityId, and unique composite (ProjectId, Code)
- [ ] T037 [P] Create ActivityRelationship EF Core configuration in Planova.Persistence/EntityConfigurations/ActivityRelationshipConfiguration.cs with indexes on ProjectId, PredecessorId, SuccessorId
- [ ] T038 [P] Create Calendar EF Core configuration in Planova.Persistence/EntityConfigurations/CalendarConfiguration.cs with indexes on ProjectId
- [ ] T039 [P] Create CalendarDay EF Core configuration in Planova.Persistence/EntityConfigurations/CalendarDayConfiguration.cs with unique composite index (CalendarId, Date)
- [ ] T040 [P] Create ActivityBank EF Core configuration in Planova.Persistence/EntityConfigurations/ActivityBankConfiguration.cs with indexes on Category and unique composite (Category, Code)
- [ ] T041 [P] Create ActivityBankItem EF Core configuration in Planova.Persistence/EntityConfigurations/ActivityBankItemConfiguration.cs with indexes on BankId, ParentId
- [ ] T042 [P] Create ActivityBankItemRelationship EF Core configuration in Planova.Persistence/EntityConfigurations/ActivityBankItemRelationshipConfiguration.cs with index on BankId
- [ ] T043 Update PlanovaDbContext to add DbSet properties for all 7 new entities and register configurations via modelBuilder.ApplyConfigurationsFromAssembly
- [ ] T044 Create EF migration AddActivityStudioEntities via `dotnet ef migrations add` in Planova.Persistence and apply database update
- [ ] T045 Create GlobalUsings.cs in Planova.Activity/ with common namespaces
- [ ] T046 [P] Create localization resources ActivityResources.en.resx and ActivityResources.ar.resx in Planova.Localization/Resources/ with common Activity Studio strings
- [ ] T047 Create ServiceCollectionExtensions in Planova.Activity/Extensions/ServiceCollectionExtensions.cs registering all Activity Studio services and repositories via AddPlanovaActivity()

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 + 2 - Create/Manage Activities & View Gantt Chart (Priority: P1) 🎯 MVP

**Goal**: Users can create, edit, view, and delete schedule activities. Activities appear in a list and on a custom Canvas-based Gantt chart with bars positioned by dates, milestones as diamonds, zoom controls (day/week/month), and relationship arrows between linked activities.

**Independent Test**: Create a new activity with name and duration, assign to a WBS item, verify auto-generated code (A-001) and NotStarted status. Edit fields, verify persistence. Create a second activity linked as FS relationship (milestone with zero duration), verify diamond marker on Gantt. Delete first activity with relationship confirmation warning. Verify zoom controls switch timeline granularity.

### Implementation for User Stories 1 & 2

#### Infrastructure

- [ ] T048 [P] [US1] Implement ActivityRepository in Planova.Persistence/Repositories/ActivityRepository.cs with GetByIdAsync, GetByProjectIdAsync, GetByWbsItemIdAsync, GetByStatusAsync, GetChildrenAsync, GetNextCodeAsync, AddAsync, UpdateAsync, DeleteAsync, ExistsAsync
- [ ] T049 [P] [US1] Implement IActivityRelationshipRepository in Planova.Persistence/Repositories/ActivityRelationshipRepository.cs
- [ ] T050 [P] [US1] Implement ICalendarRepository and ICalendarDayRepository in Planova.Persistence/Repositories/

#### Services

- [ ] T051 [P] [US1] Implement ActivityService in Planova.Activity/Application/Services/ActivityService.cs with CRUD, CreateAsync (auto-code generation via GetNextCodeAsync), UpdateAsync (guarding WbsSummary read-only fields), DeleteAsync (checking predecessor relationships), ChangeStatusAsync (validating state machine transitions per data-model.md), GetByProjectAsync with optional filter/search, GetWbsSummaryChildrenAsync, RecalculateWbsSummaryAsync (aggregate min PlannedStart, max PlannedFinish, sum Duration, average PercentComplete from children)
- [ ] T052 [P] [US1] Implement auto-code generation logic in ActivityService.CreateAsync — sequential "A-001", "A-002" per project with optional WBS code prefix inheritance when WbsItemId is provided

#### Gantt Chart (US2)

- [ ] T053 [P] [US2] Create ActivityGanttCanvas custom control in Planova.UI/Controls/ActivityGanttCanvas.cs — DrawingVisual-based Canvas with virtualized rendering (only visible time range rendered), time-to-pixel scale factor for Day/Week/Month zoom, DrawingVisual batching for performance. Render activity bars as rectangles, milestones as diamond polygons, WbsSummary activities with distinct visual style, relationship arrows as polylines between bars. RTL-aware by respecting FlowDirection (mirror time axis). Expose dependency properties: Activities, Relationships, ViewStart, ViewEnd, ZoomLevel, TimeScale
- [ ] T054 [P] [US2] Create GanttChartViewModel in Planova.UI/ViewModels/Activity/GanttChartViewModel.cs with Activities and Relationships observable collections, ZoomIn/ZoomOut commands (toggles Day/Week/Month), LoadDataAsync

#### ViewModels

- [ ] T055 [US1] Create ActivityStudioViewModel in Planova.UI/ViewModels/Activity/ActivityStudioViewModel.cs with Tabs ObservableCollection and SelectedTab, following WbsStudioViewModel pattern exactly
- [ ] T056 [US1] Create ActivityListViewModel in Planova.UI/ViewModels/Activity/ActivityListViewModel.cs with Activities ObservableCollection, SelectedActivity, SearchText, StatusFilter, TypeFilter, LoadActivitiesAsync (with CancellationToken), DeleteActivityAsync (with predecessor warning dialog), sorting by SortOrder/Code
- [ ] T057 [US1] Create ActivityEditorViewModel in Planova.UI/ViewModels/Activity/ActivityEditorViewModel.cs with two-way binding for all activity fields, SaveCommand (triggers Create or Update), validation (PlannedStart before PlannedFinish, Duration > 0 for Task/LOE, Duration = 0 for Milestone, WbsSummary field read-only), Status change with dropdown

#### Views

- [ ] T058 [US1] Create ActivityStudioView in Planova.UI/Views/Activity/ActivityStudioView.xaml + .cs with TabControl bound to Tabs, InitializeTabs(IServiceProvider) method registering all 8 tab views, per WbsStudioView pattern
- [ ] T059 [US1] Create ActivityListView in Planova.UI/Views/Activity/ActivityListView.xaml bound to ActivityListViewModel with DataGrid showing Code, Name, Type, Status, Duration, PlannedStart, PlannedFinish, PercentComplete columns, search bar, status/type filter dropdowns, delete button
- [ ] T060 [US1] Create ActivityEditorView in Planova.UI/Views/Activity/ActivityEditorView.xaml bound to ActivityEditorViewModel with fields arranged in a form layout: Name (textbox), Description (textarea), Type (combobox), Status (combobox), Duration (numeric), PlannedStart/PlannedFinish (date pickers), PercentComplete (slider), Weight (numeric), WBS Item (read-only reference), Calendar (combobox), Notes (textarea), Save/Delete buttons. WbsSummary fields greyed out when Type = WbsSummary
- [ ] T061 [US2] Create GanttChartView in Planova.UI/Views/Activity/GanttChartView.xaml bound to GanttChartViewModel — hosted in a Grid with the ActivityGanttCanvas control occupying the right panel, left panel showing activity names list (synchronized scroll with canvas), zoom buttons in toolbar, timeline header above canvas with date labels

#### Shell Integration

- [ ] T062 [US1] Register Activity Studio navigation: replace placeholder in ShellViewModel.cs with real ActivityStudioView factory (nav.RegisterTarget("activity", "Activity Studio", "CalendarDay24", true, false, () => { var view = ...; view.InitializeTabs(...); return view; }))

**Checkpoint**: US1 + US2 fully functional — users can create/edit/delete activities, see them on Gantt with bars, milestones, zoom, and relationship arrows

---

## Phase 4: User Story 3 - Define Activity Relationships and Validate Logic (Priority: P2)

**Goal**: Users create predecessor-successor relationships (FS, SS, FF, SF) with lag days. The system detects and rejects circular references and self-referencing relationships.

**Independent Test**: Create two activities, link with FS + 2 days lag, verify in relationship editor and Gantt arrow. Attempt FS relationship on same activity (self-reference) — rejected. Attempt A→B→C→A cycle — rejected with loop identification message. Change relationship type FS→SS, verify Gantt arrow updates. Delete relationship, verify removal.

### Implementation for User Story 3

#### Services

- [ ] T063 [P] [US3] Implement CircularReferenceDetector in Planova.Activity/Application/Services/CircularReferenceDetector.cs — DFS-based cycle detection using adjacency list. Takes proposed predecessor/successor pair, builds subgraph, runs iterative DFS with HashSet visited tracking. Returns CircularReferenceCheckResult with HasCycle, CycleActivities list, descriptive Message identifying the loop. Target: < 1s for 10,000 activity network. Must also detect self-referencing (predecessorId == successorId)
- [ ] T064 [P] [US3] Implement ActivityRelationshipService in Planova.Activity/Application/Services/ActivityRelationshipService.cs with CreateAsync (validates: activities exist in same project, no self-reference, no duplicate, no cycle via CircularReferenceDetector), UpdateAsync (type/lag change), DeleteAsync, GetByProjectAsync, GetByActivityAsync, ValidateNewRelationshipAsync

#### ViewModel & View

- [ ] T065 [US3] Create RelationshipEditorViewModel in Planova.UI/ViewModels/Activity/RelationshipEditorViewModel.cs with Relationships list, AvailableActivities list, SelectedPredecessorId/SelectedSuccessorId, SelectedType dropdown (FS/SS/FF/SF), LagDays spinner, ErrorMessage display, CreateRelationshipAsync (calls ValidateNewRelationshipAsync first), DeleteRelationshipAsync
- [ ] T066 [US3] Create RelationshipEditorView in Planova.UI/Views/Activity/RelationshipEditorView.xaml bound to RelationshipEditorViewModel — split into two panels: top panel with relationship creation form (predecessor combobox, successor combobox, type combobox, lag spinner, add button, error label), bottom panel with relationship list DataGrid (PredecessorCode, PredecessorName, Type, LagDays, SuccessorCode, SuccessorName, delete button)

**Checkpoint**: US3 fully functional — relationships with all 4 types, lag, circular reference detection, self-reference rejection

---

## Phase 5: User Story 4 - Manage Working Time Calendars (Priority: P2)

**Goal**: Users create/edit/delete calendars (Global & Project), configure working days per week, add exception dates (holidays), bulk-set date ranges, assign calendars to activities. Activity date calculations respect assigned calendar.

**Independent Test**: Create a new calendar named "Sunday-Thursday", set working days, add a public holiday exception, bulk-set a week as non-working. Assign to an activity. Verify AddWorkingDays(Mon, 5, calendar) skips Friday and the holiday.

### Implementation for User Story 4

#### Services

- [ ] T067 [P] [US4] Implement CalendarDateCalculator in Planova.Activity/Application/Services/CalendarDateCalculator.cs — pure in-memory date engine: AddWorkingDaysAsync (start + working days → end date), CountWorkingDaysAsync (date range → count of working days). Both methods load CalendarDay exceptions for the relevant calendar for the date range. Fallback to 7-day continuous calendar if calendar has all non-working days (with warning). Cache CalendarDay lookups per request for performance
- [ ] T068 [P] [US4] Implement CalendarService in Planova.Activity/Application/Services/CalendarService.cs with CRUD, SetDayStatusAsync (single date), BulkSetDaysAsync (date range), GetDayRangeAsync, GetDefaultForProjectAsync. Seed 3 global calendars on first access: Standard 5-Day (Sun-Thu), Standard 6-Day (Sun-Fri), Standard 7-Day (all days)

#### ViewModel & View

- [ ] T069 [US4] Create CalendarManagerViewModel in Planova.UI/ViewModels/Activity/CalendarManagerViewModel.cs with Calendars list (grouped by Global/Project), SelectedCalendar, CalendarDays for current month, CurrentMonth navigation, CreateCalendarAsync, ToggleDayStatusAsync, BulkSetNonWorkingAsync (prompt for date range), AssignDefaultCommand
- [ ] T070 [US4] Create CalendarManagerView in Planova.UI/Views/Activity/CalendarManagerView.xaml bound to CalendarManagerViewModel — left panel: calendar list grouped by type with add/edit/delete; right panel: month calendar grid (7 columns × 6 rows) with day status color coding (green=working, red=non-working, orange=exception), click to toggle, bulk-select date range with right-click context menu
- [ ] T071 [US4] Create CalendarDayGridView in Planova.UI/Views/Activity/CalendarDayGridView.xaml — reusable month grid user control used within CalendarManagerView

**Checkpoint**: US4 fully functional — calendar CRUD, day-specific exceptions, bulk operations, activity assignment, date calculations

---

## Phase 6: User Story 5 - Generate Activities from WBS Work Packages (Priority: P3)

**Goal**: Users select WBS items and generate activities in 2 modes: Simple 1:1 (one activity per WBS item) or Activity Bank (bank entry applied to each WBS item). Preview shows generated activities before commit. Handle replace vs merge when activities already exist.

**Independent Test**: Select 3 WBS items, choose Simple 1:1 mode, preview shows 3 activities, commit, verify activities appear in list with WBS code prefix. Select 1 WBS item, choose Bank mode with "Column Concrete" entry, preview shows 5+ sub-task activities, commit, verify relationships are created. Re-apply bank to same WBS item, verify warn dialog with replace/merge options.

### Implementation for User Story 5

#### Services

- [ ] T072 [P] [US5] Implement WbsGenerationService in Planova.Activity/Application/Services/WbsGenerationService.cs with PreviewSimpleGenerationAsync (creates in-memory ActivityPreviewItem list — one per WbsItem, auto-generated Code with WBS prefix, Name from WbsItem.Name, default Task type), PreviewBankGenerationAsync (deep copies bank entry items for each WbsItem, generating unique codes per WBS item), CommitGenerationAsync (persists all activities + relationships in single transaction via EF Core AddRange, detects existing activities under target WBS items, returns conflict information for UI to prompt replace/merge). For merge mode, append new activities alongside existing. For replace mode, soft-delete existing and add new. Detect orphaned activities (not part of any bank entry) and prompt to save as new custom bank entry per FR-018

#### ViewModel & View

- [ ] T073 [US5] Create WbsGenerationWizardViewModel in Planova.UI/ViewModels/Activity/WbsGenerationWizardViewModel.cs — multi-step wizard: Step 0 (Select WBS items from tree), Step 1 (Choose Simple or Bank mode — if Bank, select bank entry), Step 2 (Preview generated activities — editable rows for removal/adjustment, conflict warning if existing activities), Step 3 (Commit with progress bar — ProgressPercentage, ProgressMessage). WbsItem selection uses existing Planova.Wbs IWbsItemService
- [ ] T074 [US5] Create WbsGenerationWizardView in Planova.UI/Views/Activity/WbsGenerationWizardView.xaml bound to WbsGenerationWizardViewModel — step indicator at top, content panel changes per step, back/next/commit navigation buttons, preview DataGrid with checkboxes per row (allow removal), replace/merge radio buttons when conflicts detected, progress bar on commit

**Checkpoint**: US5 fully functional — WBS-to-activity generation with Simple and Bank modes, preview, replace/merge handling, orphan detection

---

## Phase 7: User Story 6 - Browse and Apply Activity Bank Templates (Priority: P3)

**Goal**: Users browse a library of 50+ pre-seeded construction method templates organized by 13 categories (Preliminary, Earthworks, Concrete, Formwork, Reinforcement, Steel, Masonry, Waterproofing, MEP, Finishing, Infrastructure, Landscaping, Testing & Handover). Preview entry sub-task breakdown. Apply entry to WBS items from the bank browser directly. Users can save custom entries from existing activity groups.

**Independent Test**: Open Activity Bank, expand "Concrete" category, see "Column Concrete" entry, preview sub-tasks with default durations and FS arrows. Apply to a WBS item (select in inline WBS selector), confirm activities generated. Save a group of existing activities as a new custom bank entry, verify it appears in custom category.

### Implementation for User Story 6

#### Services

- [ ] T075 [P] [US6] Implement ActivityBankService in Planova.Activity/Application/Services/ActivityBankService.cs with GetByIdAsync, BrowseAsync (by category, search by name/keyword), GetCategoriesAsync (returns 13 standard categories), CreateCustomAsync (from existing activity group — deep copies selected activities' structure as bank items), UpdateCustomAsync, DeleteCustomAsync (standard entries cannot be deleted), SeedIfEmptyAsync (loads seed JSON from embedded resource or config file, creates 50+ entries across 13 categories with items and relationships per data-model.md structure)
- [ ] T076 [P] [US6] Create seed JSON file (e.g., Planova.Activity/Application/Data/activity-bank-seed.json) with 50+ entries across 13 categories, each with items (3-10 sub-tasks) and FS relationships with default durations. Follow the structure documented in data-model.md seed section
- [ ] T077 [P] [US6] Implement ApplyBankToWbsAsync in IWbsGenerationService (reuses WbsGenerationService logic) — called from ActivityBankBrowserViewModel when user clicks "Apply to WBS" with inline WBS item selection
- [ ] T078 [P] [US6] Implement SaveActivitiesAsBankEntryAsync in ActivityBankService — takes selected activity IDs, reverse-engineers the hierarchy and relationships, creates ActivityBank + items + relationships, saves as custom entry (IsStandard = false)

#### ViewModel & View

- [ ] T079 [US6] Create ActivityBankBrowserViewModel in Planova.UI/ViewModels/Activity/ActivityBankBrowserViewModel.cs with Categories tree (expandable), Entries list filtered by selected category and SearchText, SelectedEntry, PreviewEntry (full breakdown with items and relationship arrows), ApplyToWbsCommand (opens inline WBS item selector or reuses wizard), SaveAsCustomEntryCommand (pick activities from existing project), DeleteCustomEntryCommand
- [ ] T080 [US6] Create ActivityBankBrowserView in Planova.UI/Views/Activity/ActivityBankBrowserView.xaml bound to ActivityBankBrowserViewModel — left panel: category tree with expand/collapse, search bar; right panel: entry list with name/code/category columns; detail panel: entry preview showing hierarchical sub-task tree with duration badges and relationship arrows between items using mini-canvas or TreeView with connector lines
- [ ] T081 [US6] Create ActivityBankPreviewView in Planova.UI/Views/Activity/ActivityBankPreviewView.xaml — reusable user control for rendering bank entry sub-task breakdown with hierarchy indentation, default duration, and relationship type indicators (FS→, SS↗, etc.)

**Checkpoint**: US6 fully functional — bank browsing by category, preview, apply to WBS, save custom entries, 50+ seeded entries

---

## Phase 8: User Story 7 - Generate and Export Schedule Reports (Priority: P3)

**Goal**: Users generate schedule summary report showing activity code, name, dates, duration, status, percent complete, predecessors, and successors. Export to Excel (via Phase 2 IWorkbookWriter/ClosedXML) and PDF (via QuestPDF).

**Independent Test**: Open reports view, generate schedule summary for a project with 10+ activities and relationships, verify sortable table shows all columns. Export to Excel — verify .xlsx file opens correctly with proper columns. Export to PDF — verify .pdf has formatted table with headers and page layout suitable for distribution.

### Implementation for User Story 7

#### Services

- [ ] T082 [P] [US7] Implement ActivityReportService in Planova.Activity/Application/Services/ActivityReportService.cs with GenerateScheduleReportAsync (builds ScheduleReportDto with all activities and their predecessor/successor code strings), ExportToExcelAsync (uses Planova.Excel IWorkbookWriter — creates workbook, adds worksheet "Schedule Report", writes header row and data rows with formatting), ExportToPdfAsync (uses QuestPDF — creates Document with page headers, table with columns, alternating row colors, page numbers)

#### ViewModel & View

- [ ] T083 [US7] Create ScheduleReportViewModel in Planova.UI/ViewModels/Activity/ScheduleReportViewModel.cs with Report (ScheduleReportDto), IsLoading, HasData, GenerateReportAsync, ExportToExcelAsync, ExportToPdfAsync — progress indication during generation
- [ ] T084 [US7] Create ScheduleReportView in Planova.UI/Views/Activity/ScheduleReportView.xaml bound to ScheduleReportViewModel — DataGrid showing all report columns (Code, Name, Type, Status, Duration, PlannedStart, PlannedFinish, %Complete, Predecessors, Successors) with sortable column headers, Excel export button, PDF export button, loading spinner during generation

**Checkpoint**: US7 fully functional — report generation with sortable table, Excel export, PDF export

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Edge case handling, validation hardening, RTL support, performance optimization, Activity Bank seeding, and testing

- [ ] T085 [P] Implement WbsSummary auto-rollup in ActivityService.RecalculateWbsSummaryAsync — when any child activity changes (dates, duration, percent complete), trigger async recalculation on parent WbsSummary. On load, computed on read via GetWbsSummaryChildrenAsync. For large hierarchies, consider deferred recalculation (mark dirty, recalculate on next read)
- [ ] T086 [P] Implement edge case guards: reject PlannedStart > PlannedFinish (FR-026 edge case), handle calendar change on activity with dates via CalendarService (warn and offer recalculation), handle WBS item deletion (orphaned activities remain with null WbsItemId), handle all-non-working calendar gracefully (fallback to 7-day continuous + warning), handle 10k+ Gantt rendering via virtualized ActivityGanttCanvas (only render visible range)
- [ ] T087 [P] Add RTL layout support (FR-024) in all Activity Studio XAML views — FlowDirection binding to CultureInfo via ILocalizationService.IsRtl(). GanttCanvas mirrors time axis: dates flow right-to-left, activity bars positioned from right edge, relationship arrows drawn in reverse direction
- [ ] T088 [P] Implement bulk operations: multi-select activities in list for batch status change, batch calendar assignment, batch delete (with confirmation). Bulk calendar day status set via CalendarDayRepository.BulkSetRangeAsync
- [ ] T089 [P] Create Planova.Activity.Tests project at tests/Planova.Activity.Tests/Planova.Activity.Tests.csproj with xUnit, Moq, FluentAssertions, reference to Planova.Activity
- [ ] T090 [P] Write Activity domain entity tests in tests/Planova.Activity.Tests/Domain/ActivityTests.cs — status transitions (NotStarted→InProgress→Completed, InProgress→OnHold, InProgress→Revise, OnHold→InProgress, invalid transitions rejected), WbsSummary type guard (rollup fields not directly settable at service level — test via service mock)
- [ ] T091 [P] Write Calendar domain entity tests in tests/Planova.Activity.Tests/Domain/CalendarTests.cs — working day flag validation, IsDefault uniqueness within project
- [ ] T092 [P] Write CircularReferenceDetector tests in tests/Planova.Activity.Tests/Application/CircularReferenceDetectorTests.cs — simple 2-node cycle (A→B→A), complex 5-node cycle, chain A→B→C→D (no cycle), self-reference (A→A), 10,000-node performance test (target < 1s), empty graph edge case
- [ ] T093 [P] Write CalendarDateCalculator tests in tests/Planova.Activity.Tests/Application/CalendarDateCalculatorTests.cs — AddWorkingDays on Standard 5-Day (skip weekends), AddWorkingDays with holiday exception, CountWorkingDays in range with mixed status, all-non-working calendar fallback, edge cases: start on holiday, start on weekend, negative duration, leap year date
- [ ] T094 [P] Write ActivityService tests in tests/Planova.Activity.Tests/Application/ActivityServiceTests.cs — CRUD, auto-code generation sequence, filter by status/type/search, WbsSummary rollup calculation, delete with predecessor check
- [ ] T095 [P] Write ActivityRelationshipService tests in tests/Planova.Activity.Tests/Application/ActivityRelationshipServiceTests.cs — create relationship with validation (exists check, no self-ref, no duplicate, no cycle), update type/lag, delete, get by activity
- [ ] T096 [P] Write CalendarService tests in tests/Planova.Activity.Tests/Application/CalendarServiceTests.cs — CRUD, SetDayStatusAsync, BulkSetRangeAsync, global vs project scoping
- [ ] T097 [P] Write ActivityBankService tests in tests/Planova.Activity.Tests/Application/ActivityBankServiceTests.cs — browse by category, search by keyword, preview entry, create custom entry, standard entry cannot be deleted
- [ ] T098 [P] Write WbsGenerationService tests in tests/Planova.Activity.Tests/Application/WbsGenerationServiceTests.cs — simple 1:1 generation (1 activity per WBS item), bank mode generation (N sub-tasks per WBS item), existing activity detection (replace vs merge), orphan detection, large batch (20 WBS items × 10 sub-tasks = 200 activities)
- [ ] T099 [P] Write ActivityReportService tests in tests/Planova.Activity.Tests/Application/ActivityReportServiceTests.cs — report generation data completeness, Excel export (verify IWorkbookWriter called correctly), PDF export (verify QuestPDF Document generated)
- [ ] T100 [P] Write ViewModel tests in tests/Planova.UI.Tests/ViewModels/Activity/ — ActivityListViewModelTests (load, filter, delete), GanttChartViewModelTests (zoom toggle, data binding), RelationshipEditorViewModelTests (create, validate cycle, delete), WbsGenerationWizardViewModelTests (step navigation, preview, commit) — use Moq for service dependencies
- [ ] T101 [P] Run all tests via `dotnet test` — verify all pass with 0 failures

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 + US2 (Phase 3)**: Depends on Foundational phase — MVP scope
- **US3 (Phase 4)**: Depends on Foundational + US1 (needs activity CRUD)
- **US4 (Phase 5)**: Depends on Foundational — calendar service is mostly independent but needs Activity entity for assignment
- **US5 (Phase 6)**: Depends on Foundational + US1 (needs activity CRUD) + US6 (needs Activity Bank)
- **US6 (Phase 7)**: Depends on Foundational + US1 (needs activity CRUD for save-as-custom)
- **US7 (Phase 8)**: Depends on Foundational + US1 + US3 (needs activities + relationships for report data)
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 + 2 (P1)**: Can start after Foundational — No dependencies on other stories
- **User Story 3 (P2)**: Needs US1 for activity CRUD; relationship service is independent
- **User Story 4 (P2)**: Can start after Foundational — calendar service is largely independent; needs Activity for calendar assignment
- **User Story 5 (P3)**: Needs US1 + US6 (needs Activity Bank entries available)
- **User Story 6 (P3)**: Needs US1 for save-as-custom entry feature; browsing is independent
- **User Story 7 (P3)**: Needs US1 + US3 for complete report data

### Within Each Phase

- Models/entities before services
- Services before ViewModels
- ViewModels before Views
- Interfaces implemented before consumed
- Repository implementations before services that consume them
- GanttCanvas (T053) before GanttChartView (T061)

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational enum/entity tasks marked [P] can run in parallel
- Domain interfaces ([P]) can run in parallel with entities
- DTOs marked [P] can run in parallel
- EF configurations marked [P] can run in parallel
- ActivityGanttCanvas (T053) and GanttChartViewModel (T054) can run in parallel (different files)
- ActivityListViewModel and ActivityEditorViewModel can run in parallel (different files)
- User stories 3, 4 can be implemented in parallel after Foundational + US1/US2 complete
- User stories 6 (bank browsing) can start in parallel with US3/US4 (minimal dependency)
- User stories 5 depends on US6 — sequential

---

## Parallel Example: Foundational Phase

```bash
# Create all enums in parallel:
Task: "T005 ActivityType enum in Planova.Activity/Domain/Enums/ActivityType.cs"
Task: "T006 ActivityStatus enum in Planova.Activity/Domain/Enums/ActivityStatus.cs"
Task: "T007 RelationshipType enum in Planova.Activity/Domain/Enums/RelationshipType.cs"
Task: "T008 CalendarType enum in Planova.Activity/Domain/Enums/CalendarType.cs"
Task: "T009 CalendarDayStatus enum in Planova.Activity/Domain/Enums/CalendarDayStatus.cs"

# Create all domain entities in parallel:
Task: "T010 Activity entity in Planova.Activity/Domain/Entities/Activity.cs"
Task: "T011 ActivityRelationship entity in Planova.Activity/Domain/Entities/ActivityRelationship.cs"
Task: "T012 Calendar entity in Planova.Activity/Domain/Entities/Calendar.cs"
Task: "T013 CalendarDay entity in Planova.Activity/Domain/Entities/CalendarDay.cs"
Task: "T014 ActivityBank entity in Planova.Activity/Domain/Entities/ActivityBank.cs"
Task: "T015 ActivityBankItem entity in Planova.Activity/Domain/Entities/ActivityBankItem.cs"
Task: "T016 ActivityBankItemRelationship entity in Planova.Activity/Domain/Entities/ActivityBankItemRelationship.cs"

# Create all EF configurations in parallel:
Task: "T036 ActivityConfiguration in Planova.Persistence/EntityConfigurations/ActivityConfiguration.cs"
Task: "T037 ActivityRelationshipConfiguration in Planova.Persistence/EntityConfigurations/ActivityRelationshipConfiguration.cs"
Task: "T038 CalendarConfiguration in Planova.Persistence/EntityConfigurations/CalendarConfiguration.cs"
Task: "T039 CalendarDayConfiguration in Planova.Persistence/EntityConfigurations/CalendarDayConfiguration.cs"
Task: "T040 ActivityBankConfiguration in Planova.Persistence/EntityConfigurations/ActivityBankConfiguration.cs"
Task: "T041 ActivityBankItemConfiguration in Planova.Persistence/EntityConfigurations/ActivityBankItemConfiguration.cs"
Task: "T042 ActivityBankItemRelationshipConfiguration in Planova.Persistence/EntityConfigurations/ActivityBankItemRelationshipConfiguration.cs"
```

## Parallel Example: US1 + US2 (MVP)

```bash
# Implement repositories and services in parallel:
Task: "T048 ActivityRepository in Planova.Persistence/Repositories/ActivityRepository.cs"
Task: "T049 ActivityRelationshipRepository in Planova.Persistence/Repositories/ActivityRelationshipRepository.cs"
Task: "T050 CalendarRepository + CalendarDayRepository in Planova.Persistence/Repositories/"
Task: "T051 ActivityService in Planova.Activity/Application/Services/ActivityService.cs"
Task: "T053 ActivityGanttCanvas in Planova.UI/Controls/ActivityGanttCanvas.cs"
Task: "T054 GanttChartViewModel in Planova.UI/ViewModels/Activity/GanttChartViewModel.cs"

# Create ViewModels in parallel:
Task: "T055 ActivityStudioViewModel in Planova.UI/ViewModels/Activity/ActivityStudioViewModel.cs"
Task: "T056 ActivityListViewModel in Planova.UI/ViewModels/Activity/ActivityListViewModel.cs"
Task: "T057 ActivityEditorViewModel in Planova.UI/ViewModels/Activity/ActivityEditorViewModel.cs"

# Create Views in parallel:
Task: "T058 ActivityStudioView in Planova.UI/Views/Activity/ActivityStudioView.xaml"
Task: "T059 ActivityListView in Planova.UI/Views/Activity/ActivityListView.xaml"
Task: "T060 ActivityEditorView in Planova.UI/Views/Activity/ActivityEditorView.xaml"
Task: "T061 GanttChartView in Planova.UI/Views/Activity/GanttChartView.xaml"
```

## Parallel Example: US3-6 (can run mostly concurrently)

```bash
# Team member A — US3 (Relationships):
Task: "T063 CircularReferenceDetector in Planova.Activity/Application/Services/CircularReferenceDetector.cs"
Task: "T064 ActivityRelationshipService in Planova.Activity/Application/Services/ActivityRelationshipService.cs"
Task: "T065 RelationshipEditorViewModel in Planova.UI/ViewModels/Activity/RelationshipEditorViewModel.cs"
Task: "T066 RelationshipEditorView in Planova.UI/Views/Activity/RelationshipEditorView.xaml"

# Team member B — US4 (Calendars):
Task: "T067 CalendarDateCalculator in Planova.Activity/Application/Services/CalendarDateCalculator.cs"
Task: "T068 CalendarService in Planova.Activity/Application/Services/CalendarService.cs"
Task: "T069 CalendarManagerViewModel in Planova.UI/ViewModels/Activity/CalendarManagerViewModel.cs"
Task: "T070 CalendarManagerView in Planova.UI/Views/Activity/CalendarManagerView.xaml"
Task: "T071 CalendarDayGridView in Planova.UI/Views/Activity/CalendarDayGridView.xaml"

# Team member C — US6 (Activity Bank):
Task: "T075 ActivityBankService in Planova.Activity/Application/Services/ActivityBankService.cs"
Task: "T076 Create seed JSON file in Planova.Activity/Application/Data/activity-bank-seed.json"
Task: "T079 ActivityBankBrowserViewModel in Planova.UI/ViewModels/Activity/ActivityBankBrowserViewModel.cs"
Task: "T080 ActivityBankBrowserView in Planova.UI/Views/Activity/ActivityBankBrowserView.xaml"
Task: "T081 ActivityBankPreviewView in Planova.UI/Views/Activity/ActivityBankPreviewView.xaml"

# Team member D — US5 (WBS Generation) — wait for US6 bank service, then:
Task: "T072 WbsGenerationService in Planova.Activity/Application/Services/WbsGenerationService.cs"
Task: "T073 WbsGenerationWizardViewModel in Planova.UI/ViewModels/Activity/WbsGenerationWizardViewModel.cs"
Task: "T074 WbsGenerationWizardView in Planova.UI/Views/Activity/WbsGenerationWizardView.xaml"

# Team member E — US7 (Reports):
Task: "T082 ActivityReportService in Planova.Activity/Application/Services/ActivityReportService.cs"
Task: "T083 ScheduleReportViewModel in Planova.UI/ViewModels/Activity/ScheduleReportViewModel.cs"
Task: "T084 ScheduleReportView in Planova.UI/Views/Activity/ScheduleReportView.xaml"
```

---

## Implementation Strategy

### MVP First (Phase 1 + 2 + 3)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Stories 1 & 2 (Activity CRUD + Gantt chart)
4. **STOP and VALIDATE**: Test US1+US2 independently — create/edit/delete activities, verify Gantt bars, milestones, zoom, relationship arrows
5. Deploy/demo if ready — MVP delivers the core activity scheduler

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1+US2 (P1) → Test independently → **MVP delivered!**
3. Add US3 (P2, Relationships) → Test independently → Deploy/Demo
4. Add US4 (P2, Calendars) → Test independently → Deploy/Demo
5. Add US6 (P3, Activity Bank) → Test independently → Deploy/Demo
6. Add US5 (P3, WBS Generation) → Test independently → Deploy/Demo
7. Add US7 (P3, Reports) → Test independently → Deploy/Demo
8. Phase 9: Polish, edge cases, tests, RTL support
9. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. **Team** completes Setup + Foundational together
2. Once Foundation is done:
   - Developer A: US1+US2 (Activity CRUD + Gantt) — MVP critical path
   - Developer B: US3 (Relationships) + US4 (Calendars)
   - Developer C: US6 (Activity Bank)
   - Developer D: US5 (WBS Generation — waits for US6 Bank service) + US7 (Reports)
   - Developer E: Phase 9 (Polish, edge cases, tests, RTL, Gantt perf)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [US1-7] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- US1 and US2 are both P1 and tightly coupled — combined into Phase 3 as the MVP
- Follow existing Phase 3 (BOQ Studio) and Phase 4 (WBS Studio) patterns for consistency
- GanttCanvas uses DrawingVisual — not a third-party library (per research.md decision)
- Circular reference detection uses DFS — target < 1s for 10k activities
- Calendar date calculations are in-memory — pure C# with CalendarDay lookups
- Activity Bank seed data loaded from JSON file on first access
- ShellViewModel already has placeholder registration — replace with real factory
