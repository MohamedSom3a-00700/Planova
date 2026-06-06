---

description: "Task list for Cost Studio feature implementation"

---

# Tasks: Cost Studio — Cost Management System

**Input**: Design documents from `specs/009-cost-studio/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not requested in feature specification — test tasks are excluded. Focus on implementation-only tasks.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Module project**: `Planova.Cost/` at repository root
- **Persistence**: `Planova.Persistence/`
- **UI**: `Planova.UI/`
- **Localization**: `Planova.Localization/`
- **Excel**: `Planova.Excel/`
- **Tests**: `tests/Planova.Cost.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and module structure

- [X] T001 Create Planova.Cost class library project at Planova.Cost/Planova.Cost.csproj targeting net8.0
- [X] T002 Add Planova.Cost to solution in Planova.slnx
- [X] T003 [P] Add project references: Planova.Domain, Planova.Shared, Planova.Activity, Planova.Resource
- [X] T004 Create Planova.Cost.Tests xunit project at tests/Planova.Cost.Tests/Planova.Cost.Tests.csproj targeting net8.0
- [X] T005 Add Planova.Cost.Tests to solution and reference Planova.Cost

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain entities, enums, interfaces, persistence, and DI — MUST be complete before ANY user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Enums

- [X] T006 [P] Create BudgetRevisionType enum in Planova.Cost/Domain/Enums/BudgetRevisionType.cs
- [X] T007 [P] Create BudgetRevisionStatus enum in Planova.Cost/Domain/Enums/BudgetRevisionStatus.cs
- [X] T008 [P] Create BudgetStatus enum in Planova.Cost/Domain/Enums/BudgetStatus.cs
- [X] T009 [P] Create DirectCostCategory enum in Planova.Cost/Domain/Enums/DirectCostCategory.cs
- [X] T010 [P] Create DirectCostScope enum in Planova.Cost/Domain/Enums/DirectCostScope.cs
- [X] T011 [P] Create ActualCostSource enum in Planova.Cost/Domain/Enums/ActualCostSource.cs

### Entities

- [X] T012 [P] Create Budget entity in Planova.Cost/Domain/Entities/Budget.cs
- [X] T013 [P] Create BudgetRevision entity in Planova.Cost/Domain/Entities/BudgetRevision.cs
- [X] T014 [P] Create DirectCost entity in Planova.Cost/Domain/Entities/DirectCost.cs
- [X] T015 [P] Create CostBaseline entity in Planova.Cost/Domain/Entities/CostBaseline.cs
- [X] T016 [P] Create CostBaselineRow entity in Planova.Cost/Domain/Entities/CostBaselineRow.cs
- [X] T017 [P] Create ActualCost entity in Planova.Cost/Domain/Entities/ActualCost.cs

### Repository Interfaces

- [X] T018 [P] Create IBudgetRepository interface in Planova.Cost/Domain/Interfaces/IBudgetRepository.cs
- [X] T019 [P] Create IBudgetRevisionRepository interface in Planova.Cost/Domain/Interfaces/IBudgetRevisionRepository.cs
- [X] T020 [P] Create IDirectCostRepository interface in Planova.Cost/Domain/Interfaces/IDirectCostRepository.cs
- [X] T021 [P] Create ICostBaselineRepository interface in Planova.Cost/Domain/Interfaces/ICostBaselineRepository.cs
- [X] T022 [P] Create IActualCostRepository interface in Planova.Cost/Domain/Interfaces/IActualCostRepository.cs

### Service Interfaces

- [X] T023 [P] Create ICostService interface in Planova.Cost/Domain/Interfaces/ICostService.cs
- [X] T024 [P] Create IDirectCostService interface in Planova.Cost/Domain/Interfaces/IDirectCostService.cs
- [X] T025 [P] Create IActualCostService interface in Planova.Cost/Domain/Interfaces/IActualCostService.cs
- [X] T026 [P] Create ICashFlowService interface in Planova.Cost/Domain/Interfaces/ICashFlowService.cs
- [X] T027 [P] Create IEvmService interface in Planova.Cost/Domain/Interfaces/IEvmService.cs
- [X] T028 [P] Create ICostAiService interface in Planova.Cost/Domain/Interfaces/ICostAiService.cs
- [X] T029 [P] Create ICostReportService interface in Planova.Cost/Domain/Interfaces/ICostReportService.cs

### DTOs & Mappings

- [X] T030 [P] Create BudgetDto and related DTOs in Planova.Cost/Application/Dto/BudgetDto.cs
- [X] T031 [P] Create BudgetRevisionDto and request DTOs in Planova.Cost/Application/Dto/BudgetRevisionDto.cs
- [X] T032 [P] Create DirectCostDto and request DTOs in Planova.Cost/Application/Dto/DirectCostDto.cs
- [X] T033 [P] Create CostBreakdownDto in Planova.Cost/Application/Dto/CostBreakdownDto.cs
- [X] T034 [P] Create CostBaselineDto and related DTOs in Planova.Cost/Application/Dto/CostBaselineDto.cs
- [X] T035 [P] Create ActualCostDto, ActivityVarianceDto, ImportResultDto in Planova.Cost/Application/Dto/ActualCostDto.cs
- [X] T036 [P] Create CashFlowPeriodDto in Planova.Cost/Application/Dto/CashFlowPeriodDto.cs
- [X] T037 [P] Create EvmMetricsDto, ActivityEvmDto in Planova.Cost/Application/Dto/EvmMetricsDto.cs
- [X] T038 [P] Create AiSuggestionDto, CostAnomalyDto, AiForecastDto in Planova.Cost/Application/Dto/AiSuggestionDto.cs
- [X] T039 [P] Create ReportResultDto and CostReportType enum in Planova.Cost/Application/Dto/ReportResultDto.cs
- [X] T040 Create CostMappingProfile (AutoMapper profile) in Planova.Cost/Application/Mappings/CostMappingProfile.cs

### Persistence

- [X] T041 [P] Create Budget EF Core configuration in Planova.Persistence/EntityConfigurations/BudgetConfiguration.cs
- [X] T042 [P] Create BudgetRevision EF Core configuration in Planova.Persistence/EntityConfigurations/BudgetRevisionConfiguration.cs
- [X] T043 [P] Create DirectCost EF Core configuration in Planova.Persistence/EntityConfigurations/DirectCostConfiguration.cs
- [X] T044 [P] Create CostBaseline EF Core configuration in Planova.Persistence/EntityConfigurations/CostBaselineConfiguration.cs
- [X] T045 [P] Create CostBaselineRow EF Core configuration in Planova.Persistence/EntityConfigurations/CostBaselineRowConfiguration.cs
- [X] T046 [P] Create ActualCost EF Core configuration in Planova.Persistence/EntityConfigurations/ActualCostConfiguration.cs
- [X] T047 [P] Create BudgetRepository implementation in Planova.Persistence/Repositories/BudgetRepository.cs
- [X] T048 [P] Create BudgetRevisionRepository implementation in Planova.Persistence/Repositories/BudgetRevisionRepository.cs
- [X] T049 [P] Create DirectCostRepository implementation in Planova.Persistence/Repositories/DirectCostRepository.cs
- [X] T050 [P] Create CostBaselineRepository implementation in Planova.Persistence/Repositories/CostBaselineRepository.cs
- [X] T051 [P] Create ActualCostRepository implementation in Planova.Persistence/Repositories/ActualCostRepository.cs

### DI Registration

- [X] T052 Create ServiceCollectionExtensions in Planova.Cost/Extensions/ServiceCollectionExtensions.cs with AddPlanovaCost() extension method
- [X] T053 Add Cost entity configurations to PlanovaDbContext.OnModelCreating in Planova.Persistence
- [X] T054 Register repository implementations in Planova.Persistence ServiceCollectionExtensions
- [X] T055 Add `services.AddPlanovaCost()` call in App.xaml.cs startup alongside existing module registrations
- [X] T056 Create and apply EF Core migration: `dotnet ef migrations add AddCostEntities`

**Checkpoint**: Foundation ready — all domain entities, persistence, and DI are configured. User story implementation can now begin.

---

## Phase 3: User Story 1 — View Cost Breakdown and Manage Direct Costs (Priority: P1) 🎯 MVP

**Goal**: User sees hierarchical cost breakdown tree (WBS → Activities → Resource Costs + Direct Costs). User can add, edit, and delete direct cost items with auto-computed totals.

**Independent Test**: Load a project with activities that have resource assignments. Verify the cost breakdown tree displays the correct cost totals from resource assignments. Add a direct cost and verify it appears in the tree with correct computed total (Quantity × UnitRate). Edit quantity and confirm total updates. Delete and verify removal.

### Implementation for User Story 1

- [X] T057 [P] [US1] Create CostBreakdownViewModel in Planova.UI/ViewModels/Cost/CostBreakdownViewModel.cs
- [X] T058 [P] [US1] Create DirectCostManagerViewModel in Planova.UI/ViewModels/Cost/DirectCostManagerViewModel.cs
- [X] T059 [US1] Create CostStudioViewModel (shell) in Planova.UI/ViewModels/Cost/CostStudioViewModel.cs
- [X] T060 [US1] Implement CostService (ICostService) for cost breakdown assembly in Planova.Cost/Application/Services/CostService.cs
- [X] T061 [US1] Implement DirectCostService (IDirectCostService) in Planova.Cost/Application/Services/DirectCostService.cs
- [X] T062 [P] [US1] Create CostBreakdownView.xaml and CostBreakdownView.xaml.cs in Planova.UI/Views/Cost/
- [X] T063 [P] [US1] Create DirectCostManagerView.xaml and DirectCostManagerView.xaml.cs in Planova.UI/Views/Cost/
- [X] T064 [US1] Create CostStudioView.xaml and CostStudioView.xaml.cs in Planova.UI/Views/Cost/
- [X] T065 [P] [US1] Create CostValueConverter in Planova.UI/Converters/CostValueConverter.cs
- [X] T066 [US1] Register Cost Studio navigation target in ShellViewModel

**Checkpoint**: Cost breakdown tree displays correctly. Direct costs can be created, edited, and deleted with auto-computed totals.

---

## Phase 4: User Story 2 — Manage Budget with Revisions and Contingency (Priority: P1)

**Goal**: User views budget summary (resource costs + direct costs + contingency = Total Budget), sets contingency, creates revisions with Pending/Approved workflow, and optionally overrides total budget.

**Independent Test**: Create a project budget, add a contingency amount, verify Total Budget updates. Create a budget revision, approve it, verify revision history shows correct status transitions.

### Implementation for User Story 2

- [X] T067 [US2] Implement BudgetService with contingency, override, and resource change detection logic in Planova.Cost/Application/Services/CostService.cs
- [X] T068 [P] [US2] Create BudgetViewModel in Planova.UI/ViewModels/Cost/BudgetViewModel.cs
- [X] T069 [P] [US2] Create BudgetRevisionViewModel in Planova.UI/ViewModels/Cost/BudgetRevisionViewModel.cs
- [X] T070 [P] [US2] Create BudgetView.xaml and BudgetView.xaml.cs in Planova.UI/Views/Cost/
- [X] T071 [P] [US2] Create BudgetRevisionView.xaml and BudgetRevisionView.xaml.cs in Planova.UI/Views/Cost/
- [X] T072 [US2] Implement budget revision approval workflow (Pending → Approved) in BudgetRevisionService
- [X] T073 [US2] Add resource cost change indicator logic (FR-030) — compare stored ResourceCostTotal against current resource assignment cost sum

**Checkpoint**: Budget summary displays correctly. Contingency, revisions, and manual override work. Approval workflow functions with audit logging.

---

## Phase 5: User Story 3 — Enter Actual Costs and Track Performance (Priority: P1)

**Goal**: User enters actual costs per activity manually or via Excel import. System matches imported rows to activities by code, shows planned vs actual cost variance, and handles orphaned records when activities are deleted.

**Independent Test**: Manually enter an actual cost for an activity and verify the variance column updates. Import an Excel spreadsheet and verify matched rows are imported while unmatched rows are reported. Verify upsert: re-import and confirm no duplicates.

### Implementation for User Story 3

- [X] T074 [US3] Implement ActualCostService with manual entry and import logic in Planova.Cost/Application/Services/ActualCostService.cs
- [X] T075 [P] [US3] Create ActualCostViewModel in Planova.UI/ViewModels/Cost/ActualCostViewModel.cs
- [X] T076 [P] [US3] Create ActualCostView.xaml and ActualCostView.xaml.cs in Planova.UI/Views/Cost/
- [X] T077 [US3] Implement ActualCostImportReader in Planova.Excel/Readers/ActualCostImportReader.cs for Excel file parsing
- [X] T078 [US3] Implement CostImportService in Planova.Excel/Services/CostImportService.cs with file validation, activity code matching, upsert logic, and error reporting
- [X] T079 [US3] Add orphaned record marking logic — handle ActivityDeleted event: mark associated DirectCosts and ActualCosts as orphaned

**Checkpoint**: Actual costs can be entered manually and imported via Excel. Variance displays correctly. Duplicate imports upsert without data loss. Orphaned records are marked on activity deletion.

---

## Phase 6: User Story 4 — View Cash Flow with S-Curve (Priority: P2)

**Goal**: User toggles between weekly/monthly periods to see planned vs actual costs over time. S-Curve chart shows cumulative planned and actual cost lines.

**Independent Test**: Open cash flow view for a project with activities that have planned dates and resource costs. Verify planned costs are spread across weekly periods between project start and end dates. Toggle to monthly and verify aggregation. With actual costs entered, confirm both cumulative lines display on chart.

### Implementation for User Story 4

- [X] T080 [US4] Implement CashFlowService with daily cost spreading and period aggregation in Planova.Cost/Application/Services/CashFlowService.cs
- [X] T081 [P] [US4] Create CashFlowViewModel in Planova.UI/ViewModels/Cost/CashFlowViewModel.cs
- [X] T082 [P] [US4] Create CashFlowView.xaml and CashFlowView.xaml.cs in Planova.UI/Views/Cost/ with LiveCharts2 S-Curve CartesianChart
- [X] T083 [US4] Implement weekly/monthly period toggle logic in CashFlowViewModel

**Checkpoint**: Cash flow table and S-Curve display correctly. Weekly/monthly toggle works. Cumulative planned and actual cost lines render on chart.

---

## Phase 7: User Story 5 — Monitor Earned Value Management (EVM) (Priority: P2)

**Goal**: User sets a Data Date and sees EVM metrics (CPI, SPI, CV, SV, EAC, ETC, VAC) with color-coded health indicators. System warns if no baseline is set.

**Independent Test**: Open EVM view without a baseline — verify warning displays. Set a baseline, enter progress data and actual costs, set a Data Date, and verify all EVM metrics compute correctly. Confirm CPI/SPI color coding (green ≥1.0, amber 0.8-0.99, red <0.8).

### Implementation for User Story 5

- [X] T084 [US5] Implement EvmService with PV, EV, AC, CPI, SPI, CV, SV, EAC, ETC, VAC computation in Planova.Cost/Application/Services/EvmService.cs
- [X] T085 [US5] Implement CostBaseline snapshot logic (capture planned costs + dates from activities) in CostService
- [X] T086 [P] [US5] Create EvmViewModel in Planova.UI/ViewModels/Cost/EvmViewModel.cs
- [X] T087 [P] [US5] Create EvmView.xaml and EvmView.xaml.cs in Planova.UI/Views/Cost/ with color-coded metric display
- [X] T088 [US5] Implement no-baseline warning and baseline set/remove UI flow in EvmViewModel

**Checkpoint**: EVM metrics compute and display correctly. Baseline workflow functions. Color coding works for all thresholds.

---

## Phase 8: User Story 6 — Use AI-Powered Cost Services (Priority: P3)

**Goal**: User triggers AI cost estimation, anomaly detection, cost forecasting, and narrative generation. System degrades gracefully when AI is unavailable.

**Independent Test**: Trigger each AI action and verify output is generated. Disconnect AI provider and verify disabled state message shows clearly.

### Implementation for User Story 6

- [X] T089 [US6] Implement CostAiService (ICostAiService) with Semantic Kernel CostAiPlugin — estimate, anomalies, forecast, narrative functions in Planova.Cost/Application/Services/CostAiService.cs
- [X] T090 [P] [US6] Create CostAiViewModel in Planova.UI/ViewModels/Cost/CostAiViewModel.cs
- [X] T091 [P] [US6] Create CostAiView.xaml and CostAiView.xaml.cs in Planova.UI/Views/Cost/
- [X] T092 [US6] Add graceful degradation: disable AI buttons and show informative message when ICostAiService.IsAvailable is false
- [X] T093 [US6] Add AI configuration section (CostAiOptions) to appsettings.json with provider, model, and prompt templates

**Checkpoint**: AI cost services function with real provider. Graceful degradation works when provider is unavailable.

---

## Phase 9: User Story 7 — Generate and Export Reports (Priority: P3)

**Goal**: User generates any of four cost reports (Cost Breakdown, Cash Flow, EVM, Budget Summary) and exports to Excel or PDF. EVM report includes AI narrative.

**Independent Test**: Generate each report type and verify data is correct. Export to Excel and PDF — verify valid files are produced. Check EVM report has AI narrative tab with regenerate button.

### Implementation for User Story 7

- [X] T094 [US7] Implement CostReportService with strategy pattern (one strategy per report type) in Planova.Cost/Application/Services/CostReportService.cs
- [X] T095 [P] [US7] Create CostReportViewModel in Planova.UI/ViewModels/Cost/CostReportViewModel.cs
- [X] T096 [P] [US7] Create CostReportView.xaml and CostReportView.xaml.cs in Planova.UI/Views/Cost/
- [X] T097 [US7] Implement CostReportWriter for Excel export (ClosedXML) in Planova.Excel/Writers/CostReportWriter.cs
- [X] T098 [US7] Implement QuestPDF document generation for each report type (Cost Breakdown, Cash Flow, EVM, Budget Summary)
- [X] T099 [US7] Integrate AI narrative into EVM report — show narrative tab with regenerate button calling ICostAiService.GenerateNarrativeAsync

**Checkpoint**: All four reports generate on-screen. Excel and PDF exports produce valid files. EVM report includes AI narrative.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Localization, audit logging, and final validation

- [X] T100 [P] Create CostResources.en.resx in Planova.Localization/Resources/ with English strings for all Cost Studio UI labels, messages, and error texts
- [X] T101 [P] Create CostResources.ar.resx in Planova.Localization/Resources/ with Arabic (RTL) translations
- [X] T102 [P] Add audit logging for budget revision creation/approval, baseline set/remove, and manual BAC override using shared AuditLog infrastructure
- [X] T103 [P] Validate all Cost Studio UI screens render correctly in RTL layout with Arabic language selected
- [X] T104 Run quickstart.md validation — verify all workflows described in quickstart.md function correctly end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phases 3–9)**: All depend on Foundational phase completion
  - User stories are independent of each other and can proceed in parallel
  - Sequential execution follows priority order: US1 → US2 → US3 → US4 → US5 → US6 → US7
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

| Story | Priority | Depends On | Independent Test |
|-------|----------|------------|------------------|
| US1 — Cost Breakdown & Direct Costs | P1 | Phase 2 only | Load project with resource assignments, verify cost tree, add/edit/delete direct cost |
| US2 — Budget & Revisions | P1 | Phase 2 only | Create budget, set contingency, create/approve revision |
| US3 — Actual Cost & Import | P1 | Phase 2 only | Enter manual cost, import Excel, verify upsert and variance |
| US4 — Cash Flow & S-Curve | P2 | Phase 2 + US3 (actual costs for chart) | Verify planned cost spreading, toggle periods, S-Curve lines |
| US5 — EVM | P2 | Phase 2 + US3 (actual costs for EVM) | Set baseline, enter progress + actuals, verify metrics + color coding |
| US6 — AI Services | P3 | Phase 2 + US3 (anomaly data) + US5 (forecast data) | Trigger AI functions, verify graceful degradation |
| US7 — Reports | P3 | Phase 2 + US1/2/3/4/5 (all report data) | Generate each report, export to Excel/PDF, check AI narrative |

### Within Each User Story

- Core implementation before integration
- Service layer before ViewModel
- ViewModel before View
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- Models/enums within a phase marked [P] can run in parallel
- View + ViewModel pairs within a story marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
Task: "Create CostBreakdownViewModel in Planova.UI/ViewModels/Cost/CostBreakdownViewModel.cs"
Task: "Create DirectCostManagerViewModel in Planova.UI/ViewModels/Cost/DirectCostManagerViewModel.cs"

Task: "Create CostBreakdownView.xaml and CostBreakdownView.xaml.cs in Planova.UI/Views/Cost/"
Task: "Create DirectCostManagerView.xaml and DirectCostManagerView.xaml.cs in Planova.UI/Views/Cost/"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (Cost Breakdown & Direct Costs)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 (Cost Breakdown & Direct Costs) → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 (Budget & Revisions) → Test independently → Deploy/Demo
4. Add User Story 3 (Actual Cost & Import) → Test independently → Deploy/Demo
5. Add User Story 4 (Cash Flow) → Test independently → Deploy/Demo
6. Add User Story 5 (EVM) → Test independently → Deploy/Demo
7. Add User Story 6 (AI Services) → Test independently → Deploy/Demo
8. Add User Story 7 (Reports) → Test independently → Deploy/Demo

### Parallel Team Strategy

With multiple developers:

1. Team completes Phase 1 + Phase 2 together
2. Once Foundational is done:
   - Developer A: User Story 1 (Cost Breakdown & Direct Costs) — P1
   - Developer B: User Story 2 (Budget & Revisions) — P1
   - Developer C: User Story 3 (Actual Cost & Import) — P1
3. Then:
   - Developer A: User Story 4 (Cash Flow) — P2
   - Developer B: User Story 5 (EVM) — P2
4. Then:
   - Developer A: User Story 6 (AI Services) — P3
   - Developer B: User Story 7 (Reports) — P3
5. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies — can run in parallel
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- All Phase 2 foundational tasks must complete before any user story begins
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Tests are not included (not requested in feature specification)
