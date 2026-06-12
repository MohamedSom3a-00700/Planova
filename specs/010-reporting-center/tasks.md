---

description: "Task list for Reporting Center feature implementation"
---

# Tasks: Reporting Center

**Input**: Design documents from `specs/010-reporting-center/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Test tasks are included per user story following the existing Planova testing pattern (xUnit + NSubstitute + FluentAssertions).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions
- Paths follow the Planova solution structure (Planova.Reporting/, Planova.Persistence/, Planova.UI/, Planova.Localization/)

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Create the Planova.Reporting module project, test project, and install dependencies

- [X] T001 Create Planova.Reporting class library project targeting net8.0 and add to Planova solution
- [X] T002 [P] Create Planova.Reporting.Tests xUnit test project targeting net8.0 with project reference to Planova.Reporting
- [X] T003 Install NuGet packages for Planova.Reporting (Microsoft.EntityFrameworkCore.Sqlite 8, ClosedXML, QuestPDF, DocumentFormat.OpenXml, Microsoft.SemanticKernel, CommunityToolkit.Mvvm, FluentAssertions, NSubstitute)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain entities, persistence, shared services, DI registration, navigation, and localization — MUST be complete before any user story work begins

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Domain Layer

- [X] T004 [P] Create ReportType, ReportStatus, ReportSectionType, ScheduleFrequency, ExportFormat, PartyRole enums in Planova.Reporting/Domain/Enums/
- [X] T005 [P] Create ReportSectionIds constants class in Planova.Reporting/Domain/Constants/ReportSectionIds.cs
- [X] T006 [P] Create ReportTemplate, ReportInstance, ReportSection, ReportSchedule, ReportExport, ReportSettings, ProjectParty entities in Planova.Reporting/Domain/Entities/
- [X] T007 [P] Create IReportTemplateRepository, IReportInstanceRepository, IReportScheduleRepository, IProjectPartyRepository interfaces in Planova.Reporting/Domain/Interfaces/
- [X] T008 [P] Create IReportEngine, IReportDataProvider<TData>, IReportSchedulerService, IReportExportService, IReportAiService, IProjectPartyService, IReportSettingsService interfaces in Planova.Reporting/Domain/Interfaces/

### Application Layer (Shared Services)

- [X] T009 [P] Create all DTOs (ReportTemplateDto, ReportInstanceDto, ReportScheduleDto, ReportSectionDto, ReportExportDto, ProjectPartyDto, ReportSectionConfigDto, ReportSettingsDto, ReportDataDto) in Planova.Reporting/Application/Dto/
- [X] T010 Create AutoMapper profile (ReportingMappingProfile) in Planova.Reporting/Application/Mappings/
- [X] T011 Create IReportEngine implementation (ReportEngine) in Planova.Reporting/Application/Services/ReportEngine.cs
- [X] T012 Create IReportExportService implementation (ReportExportService) in Planova.Reporting/Application/Services/ReportExportService.cs — delegates to Excel/PDF/Word export pipelines
- [X] T013 Create IReportAiService implementation (ReportAiService) in Planova.Reporting/Application/Services/ReportAiService.cs — delegates to IAIProvider with type-specific prompts

### Persistence Layer

- [X] T014 [P] Create EF Core entity configurations (ReportTemplateConfiguration, ReportInstanceConfiguration, ReportSectionConfiguration, ReportScheduleConfiguration, ReportExportConfiguration, ReportSettingsConfiguration, ProjectPartyConfiguration) in Planova.Persistence/EntityConfigurations/
- [X] T015 Add DbSet<ReportTemplate>, DbSet<ReportInstance>, DbSet<ReportSection>, DbSet<ReportSchedule>, DbSet<ReportExport>, DbSet<ReportSettings>, DbSet<ProjectParty> to PlanovaDbContext
- [X] T016 Create and apply EF Core migration for Reporting Center tables
- [X] T017 [P] Create repository implementations (ReportTemplateRepository, ReportInstanceRepository, ReportScheduleRepository, ProjectPartyRepository) in Planova.Persistence/Repositories/

### DI Registration

- [X] T018 Create ServiceCollectionExtensions (AddPlanovaReporting) in Planova.Reporting/Extensions/ServiceCollectionExtensions.cs — register domain services, repositories, data providers, hosted service
- [X] T019 Register all Reporting ViewModels and Views as transient services in the UI project DI configuration

### Navigation & Shell

- [X] T020 Create ReportingHubViewModel in Planova.UI/ViewModels/Reporting/ReportingHubViewModel.cs — 7-tab host shell with project-gated access
- [X] T021 Create ReportingHubView in Planova.UI/Views/Reporting/ReportingHubView.xaml — tab control with Daily/Weekly/Monthly/Executive/Schedule/History/Templates views
- [X] T022 Update ShellViewModel — change nav target ID "reports" ViewFactory from ReportView to ReportingHubView, set isStudio: true, add "reports" to _studioTargetIds
- [X] T023 Remove old ReportView/ReportViewModel DI registrations from UI project

### Localization

- [X] T024 [P] Create ReportingResources.en.resx in Planova.Localization/Resources/
- [X] T025 [P] Create ReportingResources.ar.resx with RTL-friendly layout in Planova.Localization/Resources/
- [X] T026 Create ReportStatusColorConverter in Planova.UI/Converters/ReportStatusColorConverter.cs

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Generate and View Daily Reports (Priority: P1) 🎯 MVP

**Goal**: A project manager opens the Reports hub, selects Daily Report tab, picks a date, and sees auto-populated sections with AI narrative generation and three-format export

**Independent Test**: Open Daily Report tab for a project with activities and resources. Verify all sections populate correctly for the selected date. Trigger AI narrative, edit, export to Excel/PDF/Word.

### Tests for User Story 1

- [X] T027 [P] [US1] Unit test for DailyReportDataProvider.CollectDataAsync verifying data aggregation from IActivityService, IResourceAssignmentService, IProjectService, IProjectDocumentService in Planova.Reporting.Tests/Application/DataProviders/DailyReportDataProviderTests.cs

### Implementation for User Story 1

- [X] T028 [US1] Create DailyReportDataProvider in Planova.Reporting/Application/DataProviders/DailyReportDataProvider.cs — implements IReportDataProvider<DailyReportDataDto>, collects Progress Today, Workforce, Equipment, Issues, Photos data from cross-module services
- [X] T029 [US1] Create DailyReportDataDto in Planova.Reporting/Application/Dto/ — typed DTO with properties for all daily report sections
- [X] T030 [P] [US1] Create DailyReportViewModel in Planova.UI/ViewModels/Reporting/DailyReportViewModel.cs — handles date selection, generate report, AI narrative, export commands
- [X] T031 [US1] Create DailyReportView XAML in Planova.UI/Views/Reporting/DailyReportView.xaml — daily sections layout with Progress Today, Workforce, Equipment, Issues editor, Photos gallery, AI narrative box, export buttons

**Checkpoint**: US1 complete — Daily Report fully functional and independently testable

---

## Phase 4: User Story 2 — Generate and View Weekly Reports (Priority: P1)

**Goal**: The user selects Weekly Report tab, picks a week, and sees Progress by WBS, resource totals, delays, look-ahead, and 2-paragraph AI narrative

**Independent Test**: Open Weekly Report for a project with activities and WBS structure spanning multiple weeks. Verify progress, resource totals, delays, and look-ahead data are accurate.

### Implementation for User Story 2

- [X] T032 [US2] Create WeeklyReportDataProvider in Planova.Reporting/Application/DataProviders/WeeklyReportDataProvider.cs — collects ProgressByWbs, ResourceUsage, Delays, LookAhead data
- [X] T033 [US2] Create WeeklyReportDataDto in Planova.Reporting/Application/Dto/ — typed DTO for weekly report sections
- [X] T034 [P] [US2] Create WeeklyReportViewModel in Planova.UI/ViewModels/Reporting/WeeklyReportViewModel.cs — week selection, generate, AI narrative, export commands
- [X] T035 [US2] Create WeeklyReportView XAML in Planova.UI/Views/Reporting/WeeklyReportView.xaml — Progress by WBS tree, Resource Usage totals, Delays list, Look-Ahead table, AI narrative box, export buttons

**Checkpoint**: US2 complete — Weekly Report fully functional and independently testable

---

## Phase 5: User Story 3 — Generate and View Monthly Reports with EVM (Priority: P1)

**Goal**: The user selects Monthly Report tab, picks a month, and sees EVM metric cards, S-Curve chart, Budget vs Actual, Progress by WBS, Resource productivity, and 3-4 paragraph AI narrative

**Independent Test**: Open Monthly Report for a project with cost data and EVM metrics. Verify EVM cards, S-Curve, and budget comparison display correctly.

### Implementation for User Story 3

- [X] T036 [US3] Create MonthlyReportDataProvider in Planova.Reporting/Application/DataProviders/MonthlyReportDataProvider.cs — collects EvmSummary, SCurve data, BudgetVsActual, ProgressByWbs, ResourceProductivity data
- [X] T037 [US3] Create MonthlyReportDataDto in Planova.Reporting/Application/Dto/ — typed DTO for monthly report sections
- [X] T038 [P] [US3] Create MonthlyReportViewModel in Planova.UI/ViewModels/Reporting/MonthlyReportViewModel.cs — month selection, generate, AI narrative, export commands
- [X] T039 [US3] Create MonthlyReportView XAML in Planova.UI/Views/Reporting/MonthlyReportView.xaml — EVM metric cards, Budget vs Actual table, Progress by WBS, Resource Productivity, AI narrative box, export buttons

**Checkpoint**: US3 complete — Monthly Report fully functional and independently testable

---

## Phase 6: User Story 4 — Generate and View Executive Reports (Priority: P2)

**Goal**: The user selects Executive Report tab, picks a date range, and sees KPI cards, S-Curves, risk highlights, financial overview, milestone status, and 3-5 paragraph AI executive narrative with Project Directory access

**Independent Test**: Open Executive Report for a fully populated project. Verify KPI cards, charts, financial summary, and milestone data.

### Implementation for User Story 4

- [X] T040 [US4] Create ExecutiveReportDataProvider in Planova.Reporting/Application/DataProviders/ExecutiveReportDataProvider.cs — collects KpiDashboard, SCurve, FinancialOverview, MilestoneStatus data from IEvmService, ICostService, IActivityService, IProjectPartyService
- [X] T041 [US4] Create ExecutiveReportDataDto in Planova.Reporting/Application/Dto/ — typed DTO for executive report sections (KpiCard, SCurvePoint, FinancialOverview, MilestoneItem, ProjectParty)
- [X] T042 [P] [US4] Create ExecutiveReportViewModel in Planova.UI/ViewModels/Reporting/ExecutiveReportViewModel.cs — date range selection, generate, AI narrative, export commands
- [X] T043 [US4] Create ExecutiveReportView XAML in Planova.UI/Views/Reporting/ExecutiveReportView.xaml — KPI dashboard cards, S-Curve table, Financial overview, Milestone grid, Project Parties, AI narrative

**Checkpoint**: US4 complete — Executive Report fully functional and independently testable

---

## Phase 7: User Story 5 — Schedule Automatic Report Generation (Priority: P2)

**Goal**: The user configures automatic report generation schedules (Daily/Weekly/Monthly) with timezone and export format preferences. The system auto-generates at configured times.

**Independent Test**: Create schedules at various frequencies and verify they generate reports at expected times.

### Implementation for User Story 5

- [X] T044 [US5] Create IReportSchedulerService implementation (ReportSchedulerService) in Planova.Reporting/Application/Services/ReportSchedulerService.cs — schedule CRUD, NextRunAt timezone-aware computation, retry tracking
- [X] T045 [US5] Create ReportGenerationHostedService in Planova.Reporting/Background/ReportGenerationHostedService.cs — IHostedService with 60-second tick, queries due schedules, triggers ReportEngine.GenerateAsync, updates LastRunAt/LastStatus, auto-deactivates on MaxRetries exceeded
- [X] T046 [P] [US5] Create ReportScheduleViewModel in Planova.UI/ViewModels/Reporting/ReportScheduleViewModel.cs — schedule CRUD commands, active/inactive toggle, grid with next-run/last-run columns
- [X] T047 [US5] Create ReportScheduleView XAML in Planova.UI/Views/Reporting/ReportScheduleView.xaml — schedule configuration form, schedule grid with status indicators, active/inactive toggle per row

**Checkpoint**: US5 complete — Scheduling fully functional and independently testable

---

## Phase 8: User Story 6 — View Report History and Re-export (Priority: P2)

**Goal**: The user opens History tab and sees a filterable grid of all generated reports with open/preview, re-export, archive, and delete actions

**Independent Test**: Generate multiple reports, verify they appear in History with correct metadata. Test export, archive, and delete actions.

### Implementation for User Story 6

- [X] T048 [P] [US6] Create ReportHistoryViewModel in Planova.UI/ViewModels/Reporting/ReportHistoryViewModel.cs — filterable grid (by type, date range, status), re-export, delete commands
- [X] T049 [US6] Create ReportHistoryView XAML in Planova.UI/Views/Reporting/ReportHistoryView.xaml — filterable data grid with title/type/period/status/generated columns, action buttons
- [X] T050 [US6] Wire up report status lifecycle transitions (Draft → Final → Archived) in ReportEngine and enforce forward-only rule

**Checkpoint**: US6 complete — History fully functional and independently testable

---

## Phase 9: User Story 7 — Customize Report Templates and Settings (Priority: P3)

**Goal**: The user reorders/toggles section visibility per report type in the Templates tab, configures section visibility in Settings, and manages Project Parties (Client, Main Contractor, Sub Contractors with logos)

**Independent Test**: Modify section visibility and order for a report type, generate a report, verify changes are reflected. Add project parties with logos, verify they appear in report headers.

### Implementation for User Story 7

- [X] T051 [US7] Create IReportSettingsService implementation (ReportSettingsService) in Planova.Reporting/Application/Services/ReportSettingsService.cs — get/update/reset section visibility per project+reportType
- [X] T052 [US7] Create IProjectPartyService implementation (ProjectPartyService) in Planova.Reporting/Application/Services/ProjectPartyService.cs — CRUD for Client/MainContractor/SubContractors, logo upload
- [X] T053 [P] [US7] Create ReportTemplateEditorViewModel in Planova.UI/ViewModels/Reporting/ReportTemplateEditorViewModel.cs — section visibility toggle per report type, project party management
- [X] T054 [US7] Create ReportTemplateEditorView XAML in Planova.UI/Views/Reporting/ReportTemplateEditorView.xaml — section visibility tab + project parties tab with add/delete
- [X] T055 [P] [US7] (merged into ReportTemplateEditorViewModel)
- [X] T056 [US7] (merged into ReportTemplateEditorView)
- [X] T057 [P] [US7] (merged into ReportTemplateEditorViewModel)
- [X] T058 [US7] Integrated project parties in ExecutiveReportDataProvider + ExecutiveReportView

**Checkpoint**: US7 complete — Templates and Settings fully functional and independently testable

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T059 [P] Add Serilog logging for critical operations (report generation, export, deletion, schedule activate/deactivate) at all entry points
- [X] T060 [P] Add CancellationToken support to all async operations following FR-031
- [X] T061 [P] Add contextual empty state guidance per report tab (FR-030 edge cases) — e.g., "No activities found. Add activities in Activity Studio."
- [X] T062 [P] Add RTL layout support for Arabic localization across all Reporting views
- [X] T063 [P] Add orphan export file cleanup on application startup (scan for ReportExport records with missing files)
- [X] T064 [P] Write quickstart.md validation (verify all screens render, all navigation transitions work)
- [X] T065 Run full solution build and verify no breaking changes to existing modules

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - US1 (Phase 3), US2 (Phase 4), US3 (Phase 5) — P1 priority, can proceed in parallel
  - US4 (Phase 6), US5 (Phase 7), US6 (Phase 8) — P2 priority
  - US7 (Phase 9) — P3 priority
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 Daily (P1)**: No dependencies on other stories — can start after Foundational
- **US2 Weekly (P1)**: No dependencies on other stories — can start after Foundational
- **US3 Monthly (P1)**: No dependencies on other stories — can start after Foundational
- **US4 Executive (P2)**: No dependencies on other stories — can start after Foundational (needs Project Directory integration)
- **US5 Schedule (P2)**: Depends on ReportEngine (Foundational) — no dependency on US1-US4
- **US6 History (P2)**: Depends on report instances existing — needs at least one report type story for data, but can be built independently in parallel
- **US7 Templates/Settings (P3)**: No dependencies on other stories — can start after Foundational

### Within Each User Story

- Models/DTOs before services
- Data providers before ViewModels
- ViewModels before Views
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- All user stories US1-US7 can start in parallel after Foundational completes
- Within each story, [P] tasks can run in parallel
- US1-US4 (report types) are fully parallel — each adds one data provider, one ViewModel, one View
- US5 (Schedule) is independent of US1-US4
- US6 (History) can be built in parallel with any report type story
- US7 (Templates/Settings) is independent of all other stories

---

## Parallel Example: User Story 1

```bash
# Launch all [P] tasks for US1 together:
Task: "Unit test for DailyReportDataProvider in Planova.Reporting.Tests/Application/DataProviders/DailyReportDataProviderTests.cs"
Task: "Create DailyReportViewModel in Planova.UI/ViewModels/Reporting/DailyReportViewModel.cs"

# Sequential within US1:
Task: "Create DailyReportDataProvider in Planova.Reporting/Application/DataProviders/"
Task: "Create DailyReportView XAML in Planova.UI/Views/Reporting/"
```

## Parallel Example: Multiple Stories

```bash
# After Foundational completes, all P1 stories can start in parallel:
Developer A: "PHASE 3 — US1 Daily Report"
Developer B: "PHASE 4 — US2 Weekly Report"
Developer C: "PHASE 5 — US3 Monthly Report"

# While P1 stories progress, P2 stories can also start:
Developer D: "PHASE 7 — US5 Schedule"
Developer E: "PHASE 8 — US6 History"
Developer F: "PHASE 9 — US7 Templates/Settings"
```

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: US1 — Daily Report
4. **STOP and VALIDATE**: Test Daily Report independently (date selection, sections populate, AI narrative, export)
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Daily) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Weekly) → Test independently → Deploy/Demo
4. Add US3 (Monthly) → Test independently → Deploy/Demo
5. Add US4 (Executive) → Test independently → Deploy/Demo
6. Add US5 (Schedule) → Test independently → Deploy/Demo
7. Add US6 (History) → Test independently → Deploy/Demo
8. Add US7 (Templates/Settings) → Test independently → Deploy/Demo
9. Polish → Final validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup (Phase 1) together
2. Team completes Foundational (Phase 2) together — domain, persistence, services
3. Once Foundational is done:
   - Developer A: US1 — Daily Report (Phase 3)
   - Developer B: US2 — Weekly Report (Phase 4)
   - Developer C: US3 — Monthly Report (Phase 5)
   - Developer D: US5 — Schedule (Phase 7) or US6 — History (Phase 8)
   - Developer E: US7 — Templates/Settings (Phase 9)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Tests should fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
