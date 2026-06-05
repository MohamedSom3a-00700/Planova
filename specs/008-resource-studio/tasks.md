---

description: "Task list for implementing Resource Studio feature"

---

# Tasks: Resource Studio

**Input**: Design documents from `specs/008-resource-studio/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Test tasks are included. The feature specification and plan.md call for xUnit tests for domain entities, application services, histogram computation, AI estimation service, crew blended rate calculation, rate resolution, import service, and report services.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and directory structure

- [X] T001 Create Planova.Resource class library project at `Planova.Resource/Planova.Resource.csproj` with project references to Planova.Domain, Planova.Shared, and Planova.Activity
- [X] T002 [P] Create test project at `tests/Planova.Resource.Tests/Planova.Resource.Tests.csproj` with xUnit, Moq, FluentAssertions, reference to Planova.Resource
- [X] T003 [P] Create directory structure: `Planova.Resource/Domain/{Entities,Enums,Interfaces}`, `Planova.Resource/Application/{Services,Dto,Mappings}`, `Planova.Resource/Extensions`
- [X] T004 [P] Create directories: `Planova.UI/Views/Resource`, `Planova.UI/ViewModels/Resource`, `Planova.Persistence/EntityConfigurations`, `Planova.Localization/Resources`, `tests/Planova.Resource.Tests/{Domain,Application}`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities, enums, interfaces, persistence, and infrastructure that MUST be complete before ANY user story can begin.

**CRITICAL**: No user story work can begin until this phase is complete

### Enums

- [X] T005 Create ResourceType enum in `Planova.Resource/Domain/Enums/ResourceType.cs` with Labour, Equipment, Material, Subcontractor
- [X] T006 [P] Create ResourceScope enum in `Planova.Resource/Domain/Enums/ResourceScope.cs` with Global, Project
- [X] T007 [P] Create ResourceStatus enum in `Planova.Resource/Domain/Enums/ResourceStatus.cs` with Active, Inactive — soft-delete support
- [X] T008 [P] Create CrewStatus enum in `Planova.Resource/Domain/Enums/CrewStatus.cs` with Draft, Active, Inactive — transition validation via CrewStatusTransitions static class
- [X] T009 [P] Create HistogramAggregation enum in `Planova.Resource/Domain/Enums/HistogramAggregation.cs` with Sum, Average, Peak

### Entities

- [X] T010 Create Resource entity in `Planova.Resource/Domain/Entities/Resource.cs` per data-model.md — Id (Guid), Code (auto-generated prefix per type), Name, ResourceType (discriminator), Scope, ProjectId (nullable for Global), Status, DefaultRate, UnitOfMeasure, MaxQuantity, Currency, Description, type-specific fields (Trade, SkillLevel, EquipmentType, Capacity, OperatingCost, UnitPrice, WastagePercent, Company, ContractValue, ContactName, ContactPhone), IsGlobal (computed), timestamps, navigation to Rates/CrewMemberships/Assignments/UsageRecords
- [X] T011 [P] Create ResourceRate entity in `Planova.Resource/Domain/Entities/ResourceRate.cs` per data-model.md — Id, ResourceId, EffectiveDate, Rate, Currency, UnitOfMeasure, IsDefault, Notes, timestamps. Unique constraint on (ResourceId, EffectiveDate)
- [X] T012 [P] Create Crew entity in `Planova.Resource/Domain/Entities/Crew.cs` per data-model.md — Id, Name, Description, ProjectId, Status, Category, timestamps, navigation to Resources
- [X] T013 [P] Create CrewResource entity in `Planova.Resource/Domain/Entities/CrewResource.cs` per data-model.md — Id, CrewId, ResourceId, Quantity, IsLead, SortOrder, Unique constraint on (CrewId, ResourceId)
- [X] T014 [P] Create ResourceAssignment entity in `Planova.Resource/Domain/Entities/ResourceAssignment.cs` per data-model.md — Id, ProjectId, ActivityId, ResourceId, CrewId (nullable), Quantity, Rate, Currency, UnitOfMeasure, StartDate, EndDate, TotalCost (computed on save), DurationDays, Notes, timestamps
- [X] T015 [P] Create ResourceUsage entity in `Planova.Resource/Domain/Entities/ResourceUsage.cs` per data-model.md — Id, AssignmentId, ResourceId, Date, PlannedQuantity, ActualQuantity

### Domain Interfaces

- [X] T016 [P] Create IResourceRepository interface in `Planova.Resource/Domain/Interfaces/IResourceRepository.cs` per contracts/domain-interfaces.md
- [X] T017 [P] Create IResourceRateRepository interface in `Planova.Resource/Domain/Interfaces/IResourceRateRepository.cs` per contracts/domain-interfaces.md — includes GetEffectiveRateAsync for date-based rate resolution
- [X] T018 [P] Create ICrewRepository interface in `Planova.Resource/Domain/Interfaces/ICrewRepository.cs` per contracts/domain-interfaces.md
- [X] T019 [P] Create ICrewResourceRepository interface in `Planova.Resource/Domain/Interfaces/ICrewResourceRepository.cs` per contracts/domain-interfaces.md
- [X] T020 [P] Create IResourceAssignmentRepository interface in `Planova.Resource/Domain/Interfaces/IResourceAssignmentRepository.cs` per contracts/domain-interfaces.md — includes HasAssignmentsForActivityAsync for activity deletion guard
- [X] T021 [P] Create IResourceUsageRepository interface in `Planova.Resource/Domain/Interfaces/IResourceUsageRepository.cs` per contracts/domain-interfaces.md — includes GetByProjectAsync with time-range and type filtering
- [X] T022 [P] Create IResourceService interface in `Planova.Resource/Domain/Interfaces/IResourceService.cs` per contracts/domain-interfaces.md
- [X] T023 [P] Create ICrewService interface in `Planova.Resource/Domain/Interfaces/ICrewService.cs` per contracts/domain-interfaces.md — includes ComputeBlendedRateAsync, ApplyToActivitiesAsync
- [X] T024 [P] Create IResourceAssignmentService interface in `Planova.Resource/Domain/Interfaces/IResourceAssignmentService.cs` per contracts/domain-interfaces.md — includes CheckActivityDeletionAsync
- [X] T025 [P] Create IResourceHistogramService interface in `Planova.Resource/Domain/Interfaces/IResourceHistogramService.cs` per contracts/domain-interfaces.md
- [X] T026 [P] Create IResourceAiEstimationService interface in `Planova.Resource/Domain/Interfaces/IResourceAiEstimationService.cs` per contracts/domain-interfaces.md
- [X] T027 [P] Create IResourceReportService interface in `Planova.Resource/Domain/Interfaces/IResourceReportService.cs` per contracts/domain-interfaces.md
- [X] T028 [P] Create IResourceImportService interface in `Planova.Resource/Domain/Interfaces/IResourceImportService.cs` per contracts/domain-interfaces.md

### DTOs

- [X] T029 [P] Create ResourceDto, CreateResourceRequest, UpdateResourceRequest, ResourceFilter, ResourceDuplicateCheckResult in `Planova.Resource/Application/Dto/ResourceDto.cs` per contracts/application-dtos.md — ResourceDto includes EffectiveRate (resolved at query time)
- [X] T030 [P] Create ResourceRateDto, CreateRateRequest in `Planova.Resource/Application/Dto/ResourceRateDto.cs` per contracts/application-dtos.md
- [X] T031 [P] Create CrewDto, CrewResourceDto, CreateCrewRequest, CrewResourceInput in `Planova.Resource/Application/Dto/CrewDto.cs` per contracts/application-dtos.md — CrewDto has BlendedRate (computed on read), ResourceCount
- [X] T032 [P] Create ResourceAssignmentDto, CreateAssignmentRequest, UpdateAssignmentRequest in `Planova.Resource/Application/Dto/ResourceAssignmentDto.cs` per contracts/application-dtos.md
- [X] T033 [P] Create ResourceHistogramDto, HistogramDayDto, HistogramFilter in `Planova.Resource/Application/Dto/ResourceHistogramDto.cs` per contracts/application-dtos.md — HistogramDayDto includes IsOverallocated flag, Breakdown list
- [X] T034 [P] Create AiSuggestionDto, AcceptedSuggestionDto in `Planova.Resource/Application/Dto/AiSuggestionDto.cs` per contracts/application-dtos.md — AiSuggestionDto has ConfidenceScore (0.0-1.0) and Reasoning
- [X] T035 [P] Create ResourceUsageReportDto, ResourceCostReportDto, ActivityResourceSection, CostSummarySection, CostLineItem in `Planova.Resource/Application/Dto/ResourceReportDto.cs` per contracts/application-dtos.md
- [X] T036 [P] Create ImportPreviewDto, ImportRowDto, ImportDuplicateDto, ImportRequest, ImportResultDto in `Planova.Resource/Application/Dto/ImportResultDto.cs` per contracts/application-dtos.md

### Persistence & Infrastructure

- [X] T037 Create Resource EF Core configuration in `Planova.Persistence/EntityConfigurations/ResourceConfiguration.cs` per contracts/persistence-contracts.md — TPH discriminator on ResourceType, indexes on (Code+Scope+ProjectId), (Scope+ProjectId), (ResourceType+Scope), Name
- [X] T038 [P] Create ResourceRate EF Core configuration in `Planova.Persistence/EntityConfigurations/ResourceRateConfiguration.cs` per contracts/persistence-contracts.md — unique index on (ResourceId, EffectiveDate), descending index for rate resolution query
- [X] T039 [P] Create Crew EF Core configuration in `Planova.Persistence/EntityConfigurations/CrewConfiguration.cs` per contracts/persistence-contracts.md
- [X] T040 [P] Create CrewResource EF Core configuration in `Planova.Persistence/EntityConfigurations/CrewResourceConfiguration.cs` per contracts/persistence-contracts.md — unique index on (CrewId, ResourceId)
- [X] T041 [P] Create ResourceAssignment EF Core configuration in `Planova.Persistence/EntityConfigurations/ResourceAssignmentConfiguration.cs` per contracts/persistence-contracts.md — indexes on ActivityId, (ProjectId+ResourceId)
- [X] T042 [P] Create ResourceUsage EF Core configuration in `Planova.Persistence/EntityConfigurations/ResourceUsageConfiguration.cs` per contracts/persistence-contracts.md — indexes on (ResourceId+Date), (Date+ResourceId)
- [X] T043 Update PlanovaDbContext to add DbSet properties for all 6 new entities and register configurations via modelBuilder.ApplyConfigurationsFromAssembly
- [ ] T044 Create EF migration `AddResourceStudioEntities` via `dotnet ef migrations add` in Planova.Persistence and apply database update
- [X] T045 Create GlobalUsings.cs in `Planova.Resource/` with common namespaces
- [X] T046 [P] Create `ResourceResources.en.resx` and `ResourceResources.ar.resx` in `Planova.Localization/Resources/` with common Resource Studio strings (entity names, field labels, status values, error messages, confirmation dialogs)
- [X] T047 Create ServiceCollectionExtensions in `Planova.Resource/Extensions/ServiceCollectionExtensions.cs` registering all Resource Studio services via `AddPlanovaResource()`
- [X] T048 Create ResourceMappingProfile in `Planova.Resource/Application/Mappings/ResourceMappingProfile.cs` with extension methods for Entity→Dto conversions per existing patterns

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Manage Resource Library (Priority: P1) 🎯 MVP

**Goal**: Users can create, view, edit, search, filter, and deactivate resources of four types (Labour, Equipment, Material, Subcontractor) with type-specific fields, auto-generated codes, and duplicate name warnings. Resources support Global and Project scoping.

**Independent Test**: Create a Labour resource with trade and skill level — verify auto-generated code R-LBR-001. Create an Equipment resource with capacity and operating cost — verify code R-EQP-001. Filter by type Labour — verify only Labour resources shown. Search by code R-LBR-001 — verify matching resource. Deactivate a resource — verify it's hidden from pickers but still in DB. Create a resource with a duplicate name — verify warning displays but save proceeds.

### Implementation for User Story 1

#### Repositories

- [X] T049 [P] [US1] Implement ResourceRepository in `Planova.Persistence/Repositories/ResourceRepository.cs` with GetByIdAsync, GetByProjectAsync (with scope/type filters), SearchAsync (name/code/category with type/scope/project filters), GetByTypeAsync, CodeExistsAsync, GenerateNextCodeAsync (sequential per type: MAX code → extract number → increment → format "R-{PREFIX}-{000}"), HasDuplicateNameAsync, AddAsync, UpdateAsync, DeleteAsync, GetCountAsync

#### Services

- [X] T050 [P] [US1] Implement ResourceService in `Planova.Resource/Application/Services/ResourceService.cs` — CreateAsync (auto-generates code via GenerateNextCodeAsync, validates type-specific required fields, checks duplicate name → includes warning in response), UpdateAsync (guards non-mutable fields: Code, ResourceType, Scope, ProjectId), DeactivateAsync (sets Status=Inactive — preserves historical assignments), ReactivateAsync, GetByIdAsync (resolves EffectiveRate for current date), SearchAsync with filtering
- [X] T051 [P] [US1] Implement auto-code generation in ResourceService.CreateAsync — format per type: Labour=R-LBR, Equipment=R-EQP, Material=R-MAT, Subcontractor=R-SUB — sequential 3-digit padding

#### ViewModels

- [X] T052 [US1] Create ResourceStudioViewModel in `Planova.UI/ViewModels/Resource/ResourceStudioViewModel.cs` with Tabs ObservableCollection and SelectedTab, following ActivityStudioViewModel pattern exactly — registers inner tabs: Library, Crew Templates, Histogram, Reports
- [X] T053 [US1] Create ResourceLibraryViewModel in `Planova.UI/ViewModels/Resource/ResourceLibraryViewModel.cs` with Resources ObservableCollection, SelectedResource, Filter (search query, type filter, scope filter, status filter), LoadResourcesAsync (with CancellationToken), SearchAsync, FilterByTypeAsync, CreateResourceCommand (opens editor), EditResourceCommand, DeactivateResourceAsync (with confirmation dialog), ReactivateResourceAsync, ImportResourcesCommand
- [X] T054 [US1] Create ResourceEditorViewModel in `Planova.UI/ViewModels/Resource/ResourceEditorViewModel.cs` with IsNew flag, Title, type-specific field visibility (ShowLabourFields, ShowEquipmentFields, ShowMaterialFields, ShowSubcontractorFields — updated when ResourceType changes), bindable properties for all fields, SaveCommand (triggers Create or Update with validation), CancelCommand. Duplicate name warning displayed as non-blocking toast/notification

#### Views

- [X] T055 [US1] Create ResourceStudioView in `Planova.UI/Views/Resource/ResourceStudioView.xaml` + .cs with TabControl bound to Tabs, InitializeTabs(IServiceProvider) method registering all 4 tab views, per WbsStudioView pattern
- [X] T056 [US1] Create ResourceLibraryView in `Planova.UI/Views/Resource/ResourceLibraryView.xaml` bound to ResourceLibraryViewModel — DataGrid showing Code, Name, Type, Status, Scope, DefaultRate, UnitOfMeasure columns with sorting; search bar; type filter dropdown (All/Labour/Equipment/Material/Subcontractor); scope filter dropdown; create/ edit/deactivate/reactivate buttons in toolbar; status indicator column (Active=green, Inactive=gray)
- [X] T057 [US1] Create ResourceEditorView in `Planova.UI/Views/Resource/ResourceEditorView.xaml` bound to ResourceEditorViewModel — form layout: Name (textbox), ResourceType (combobox — locked on edit), Scope (combobox — locked on edit), Project (combobox when Scope=Project), DefaultRate (numeric), UnitOfMeasure (combobox), MaxQuantity (numeric), Currency (combobox), Description (textarea). Type-specific section below: Labour fields (Trade combobox, SkillLevel combobox) shown when Labour selected; Equipment fields (EquipmentType, Capacity, OperatingCost) shown when Equipment selected; Material fields (UnitPrice, WastagePercent) shown when Material selected; Subcontractor fields (Company, ContractValue, ContactName, ContactPhone) shown when Subcontractor selected. Duplicate name warning banner displayed below name field when applicable

#### Shell Integration

- [X] T058 [US1] Register Resource Studio navigation in ShellViewModel — add resource icon to navigation rail, register navigation target that creates ResourceStudioView with InitializeTabs

**Checkpoint**: US1 fully functional — resource library CRUD with type-specific fields, auto-codes, search, filter, scope, duplicate warnings, soft delete

---

## Phase 4: User Story 2 - Manage Resource Rates with Effective Dating (Priority: P1)

**Goal**: Users can define multiple rates per resource with effective dates. Rate resolution returns correct rate for any given date. Rate history shows past, present, and future rates chronologically. Bulk rate update across multiple supported.

**Independent Test**: Add 3 rates for same resource at different effective dates — verify unique constraint rejects duplicate dates. Query rate for Feb 15 with rates Jan 1 ($50) and Mar 1 ($55) — returns $50. View rate history — all 3 shown chronologically. Add future-dated rate — saved and becomes active automatically on that date. Bulk update rates for all Labour resources — verify all Labour rates updated with new effective date.

### Implementation for User Story 2

#### Repositories

- [X] T059 [P] [US2] Implement ResourceRateRepository in `Planova.Persistence/Repositories/ResourceRateRepository.cs` with GetByIdAsync, GetByResourceAsync (ordered by EffectiveDate DESC), GetEffectiveRateAsync (SELECT TOP 1 WHERE ResourceId AND EffectiveDate <= @date ORDER BY EffectiveDate DESC — fallback to default rate if none found), HasDuplicateEffectiveDateAsync, AddAsync, UpdateAsync, DeleteAsync, BulkUpdateAsync (takes list of (ResourceId, NewRate) pairs, creates new rate records with given effective date)

#### Services

- [X] T060 [P] [US2] Implement ResourceRateService in `Planova.Resource/Application/Services/ResourceRateService.cs` with AddRateAsync (validates no duplicate effective date), UpdateRateAsync, DeleteRateAsync, GetRateHistoryAsync, GetEffectiveRateAsync (date-based resolution), BulkUpdateRatesAsync (type/category filter + new rate + effective date — creates rate records for all matching resources)

#### ViewModel & View

- [X] T061 [US2] Create ResourceRateManagerViewModel in `Planova.UI/ViewModels/Resource/ResourceRateManagerViewModel.cs` with ResourceId, Rates ObservableCollection (chronological), SelectedRate, AddRateCommand (opens inline form with EffectiveDate (date picker), Rate (numeric), Currency (combobox), UnitOfMeasure (combobox), IsDefault (checkbox), Notes (textarea)), RemoveRateCommand (with confirmation), BulkRateUpdateCommand (opens bulk update dialog with type/category filters, new rate, effective date)
- [X] T062 [US2] Create ResourceRateManagerView in `Planova.UI/Views/Resource/ResourceRateManagerView.xaml` bound to ResourceRateManagerViewModel — split into two panels: top panel shows current effective rate with "as of today" label and rate history DataGrid (EffectiveDate, Rate, Currency, UnitOfMeasure, IsDefault badge, delete button); bottom panel has Add Rate form and Bulk Update button. Rate history rows color-coded: past=gray, current=green, future=blue

**Checkpoint**: US2 fully functional — effective-dated rates, date-based resolution, rate history, bulk updates, rate-at-time-of-application for assignments

---

## Phase 5: User Story 3 - Compose and Apply Crew Templates (Priority: P2)

**Goal**: Users create reusable crew templates (e.g., "Concrete Crew: 3 Carpenters + 1 Foreman + 2 Laborers") with auto-calculated blended rates. Crews can be applied to one or more activities, generating individual resource assignments. Crew templates can be cloned.

**Independent Test**: Create crew with 2 Carpenters ($40/hr) + 1 Foreman ($55/hr) — verify blended rate = $135/hr. Apply crew to activity — verify 3 individual assignments created with correct quantities and rates. Apply same crew to 3 activities simultaneously — each activity gets the full set. Clone crew — verify duplicate with new name and same resources. Add zero resources — verify apply is disabled.

### Implementation for User Story 3

#### Repositories

- [X] T063 [P] [US3] Implement CrewRepository in `Planova.Persistence/Repositories/CrewRepository.cs` with GetByIdAsync (includes CrewResources with Resources and their rates), GetAllAsync (with optional project/status filter), SearchAsync, AddAsync, UpdateAsync, DeleteAsync (blocked if crew has been applied to activities — check via ResourceAssignmentRepository.HasAssignmentsForCrewAsync)
- [X] T064 [P] [US3] Implement CrewResourceRepository in `Planova.Persistence/Repositories/CrewResourceRepository.cs` with GetByCrewAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, DeleteByCrewAsync

#### Services

- [X] T065 [P] [US3] Implement CrewService in `Planova.Resource/Application/Services/CrewService.cs` — CreateAsync (validates at least 1 resource), UpdateAsync, DeleteAsync (blocked if crew has been applied), CloneAsync (deep copies all CrewResources with new name), ComputeBlendedRateAsync (SUM of CrewResource.Quantity × GetEffectiveRateAsync(resourceId, rateDate) for all crew members), AddResourceToCrewAsync (validates no duplicate), RemoveResourceFromCrewAsync, UpdateCrewResourceQuantityAsync, ApplyToActivitiesAsync (for each activity: iterate crew resources → create ResourceAssignment for each with rate snapshot at current date, expanded quantity × crew quantity, link CrewId. Returns created assignment DTOs)

#### ViewModels & Views

- [X] T066 [US3] Create CrewTemplateManagerViewModel in `Planova.UI/ViewModels/Resource/CrewTemplateManagerViewModel.cs` with Crews ObservableCollection, SelectedCrew, LoadCrewsAsync, CreateCrewCommand, EditCrewCommand, CloneCrewCommand (prompts for new name), DeleteCrewCommand (with confirmation — shows reference check), ApplyCrewCommand (opens activity multi-select dialog — uses IActivityService to get project activities)
- [X] T067 [US3] Create CrewTemplateEditorViewModel in `Planova.UI/ViewModels/Resource/CrewTemplateEditorViewModel.cs` with Crew edit state, Resources ObservableCollection, BlendedRate (computed on any change), AddResourceCommand (opens resource picker filtered to active resources with quantity input), RemoveResourceCommand, UpdateQuantityCommand, SaveCommand, CancelCommand
- [X] T068 [US3] Create CrewTemplateManagerView in `Planova.UI/Views/Resource/CrewTemplateManagerView.xaml` bound to CrewTemplateManagerViewModel — left panel: crew list grouped by status with search; right panel: crew detail with name, description, blended rate display, Resources DataGrid (ResourceCode, ResourceName, Quantity, IsLead badge, LineTotal), add/remove resource buttons, save/cancel, clone/delete/apply buttons in toolbar
- [X] T069 [US3] Create CrewTemplateEditorView in `Planova.UI/Views/Resource/CrewTemplateEditorView.xaml` bound to CrewTemplateEditorViewModel — inline editor within CrewTemplateManagerView for resource composition

**Checkpoint**: US3 fully functional — crew CRUD, blended rate computation, resource composition, apply to activities, clone

---

## Phase 6: User Story 4 - Load Resources onto Activities (Priority: P1)

**Goal**: Users assign resources (or crew templates) to Phase 5 activities with quantities, rates, and assignment dates. Total cost auto-calculates. Editing quantity/rate triggers cost recalculation. Activity deletion is blocked when assignments exist.

**Independent Test**: Select activity, assign resource with qty 10, rate $50/hr — verify total cost $500. Edit qty to 15 — cost updates to $750 (realtime). Remove assignment with confirmation — deleted, cost updated to $0. Verify activity deletion is blocked with message listing assignments.

### Implementation for User Story 4

#### Repositories

- [X] T070 [P] [US4] Implement ResourceAssignmentRepository in `Planova.Persistence/Repositories/ResourceAssignmentRepository.cs` with GetByIdAsync (includes Resource navigation for display), GetByActivityAsync, GetByProjectAsync, GetByResourceAsync, HasAssignmentsForActivityAsync, HasAssignmentsForResourceAsync, AddAsync, AddRangeAsync (for crew application), UpdateAsync, DeleteAsync, GetTotalCostForActivityAsync (SUM of TotalCost)

#### Services

- [X] T071 [P] [US4] Implement ResourceAssignmentService in `Planova.Resource/Application/Services/ResourceAssignmentService.cs` — CreateAsync (computes TotalCost = Quantity × Rate × DurationFactor. DurationFactor = 1 for unit/hourly, else 1. For hourly resources with date range: DurationFactor = CalendarService.CountWorkingDays(StartDate, EndDate, calendar) × HoursPerDay / HoursPerUnit). Triggers ResourceUsage regeneration for histogram. UpdateAsync (recalculates TotalCost, regenerates usage). DeleteAsync (removes usage records, then assignment). GetByActivityAsync, GetByProjectAsync, GetActivityTotalCostAsync, CheckActivityDeletionAsync (throws EntityInUseException with assignment list if assignments exist — called from ActivityService before activity delete)

#### ViewModel & View

- [X] T072 [US4] Create ResourceAssignmentViewModel in `Planova.UI/ViewModels/Resource/ResourceAssignmentViewModel.cs` with ActivityId, Assignments ObservableCollection, TotalCost (computed on any change), AddAssignmentCommand (opens inline form: resource picker filtered to project scope, quantity, rate (pre-filled from resource's effective rate), currency, unit, start/end date pickers, notes), EditAssignmentCommand, RemoveAssignmentCommand (with confirmation), ApplyCrewCommand (opens crew picker filtered to available crews)
- [X] T073 [US4] Create ResourceAssignmentView in `Planova.UI/Views/Resource/ResourceAssignmentView.xaml` bound to ResourceAssignmentViewModel — DataGrid showing ResourceCode, ResourceName, ResourceType icon, Quantity, Rate, Currency, UnitOfMeasure, StartDate, EndDate, TotalCost columns with edit/remove buttons per row; totals footer showing TotalCost; Add Assignment form in bottom panel with resource search/combobox, quantity spinner, rate input (pre-filled), currency dropdown, date range picker, notes textarea; Apply Crew button opens crew selection dialog

**Checkpoint**: US4 fully functional — resource loading onto activities, realtime cost calculation, activity deletion guard, crew application

---

## Phase 7: User Story 5 - View Resource Histogram (Priority: P2)

**Goal**: Users visualize daily resource usage across project timeline to identify peak demand and overallocation. Histogram shows bar chart with filtering by type, resource, time range, and aggregation. Overallocated days highlighted with warning indicators.

**Independent Test**: Create assignments with date ranges, open histogram — verify bars match daily quantities. Overallocate a resource (assign qty exceeding MaxQuantity on a day) — verify bar highlighted with warning. Filter by type Labour — only Labour resources shown. Export histogram data — file downloads in spreadsheet format.

### Implementation for User Story 5

#### Repositories & Services

- [X] T074 [P] [US5] Implement ResourceUsageRepository in `Planova.Persistence/Repositories/ResourceUsageRepository.cs` with GetByResourceAsync (with optional date range filter), GetByProjectAsync (with type/date filters — GROUP BY Date, ResourceId for histogram aggregation), AddRangeAsync, DeleteByAssignmentAsync, RegenerateForAssignmentAsync (deletes existing usage for assignment, recalculates daily distribution across start-end date range, inserts new records)
- [X] T075 [P] [US5] Implement ResourceHistogramService in `Planova.Resource/Application/Services/ResourceHistogramService.cs` — GetHistogramAsync (queries ResourceUsage aggregated by date for given filters, computes daily totals, compares against Resource.MaxQuantity for overallocation, returns HistogramDayDto list with breakdown per resource). GetByResourceTypeAsync, GetByResourceAsync. ExportHistogramDataAsync (builds spreadsheet data using ClosedXML — columns: Date, ResourceName, ResourceCode, Type, PlannedQuantity, AvailableQuantity, Overallocated flag)

#### ViewModel & View

- [X] T076 [US5] Create ResourceHistogramViewModel in `Planova.UI/ViewModels/Resource/ResourceHistogramViewModel.cs` with HistogramData (ResourceHistogramDto), Filter (HistogramFilter — From, To, ResourceType, ResourceId, Aggregation), ChartSeries (LiveCharts2 SeriesCollection — ColumnSeries for daily bars, additional LineSeries for peak markers), XAxes (DateTimeAxis with Day/Week/Month labels), YAxes (quantity axis), LoadHistogramAsync, FilterByTypeCommand, FilterByResourceCommand, FilterByDateRangeCommand, ExportDataCommand, ZoomInCommand/ZoomOutCommand (switch between Day/Week/Month grouping)
- [X] T077 [US5] Create ResourceHistogramView in `Planova.UI/Views/Resource/ResourceHistogramView.xaml` bound to ResourceHistogramViewModel — LiveCharts2 CartesianChart filling main area; filter toolbar above with resource type combobox, resource search/combobox, date range picker, aggregation combobox (Sum/Average/Peak), zoom buttons; overallocated bars rendered in red/orange with tooltip showing "Overallocated by X units on [date]"; export button

**Checkpoint**: US5 fully functional — histogram with bar chart, filtering, overallocation highlighting, data export

---

## Phase 8: User Story 6 - Use AI to Estimate Resources (Priority: P3)

**Goal**: Users trigger AI-based resource estimation for an activity. AI suggests labour, equipment, and material requirements based on activity name, description, and WBS category. Suggestions are previewed with confidence indicators, then accepted (all or adjusted) or rejected.

**Independent Test**: Select activity, click "Estimate Resources" — suggestions displayed with confidence. Accept fully — assignments created. Adjust quantity before accepting — modified value used. Trigger estimation when AI provider unavailable — clear error message shown with manual suggestion.

### Implementation for User Story 6

#### Services

- [X] T078 [P] [US6] Implement ResourceAiEstimationService in `Planova.Resource/Application/Services/ResourceAiEstimationService.cs` — IsAvailableAsync (checks Semantic Kernel provider is configured and reachable). EstimateResourcesAsync (builds Semantic Kernel prompt with: activity name, description, WBS category, existing resource library context (list of available resources with codes, types, trade/skill/capacity info). Plugin: `ResourceEstimationPlugin` with `[KernelFunction] SuggestResourcesAsync`. Parses LLM response into List of AiSuggestionDto. Returns empty list with descriptive error if provider unavailable or timeout). AcceptSuggestionsAsync (creates ResourceAssignment records for each accepted suggestion — uses effective rate, sets default UOM)
- [X] T079 [P] [US6] Create ResourceEstimationPlugin in `Planova.Resource/Application/Services/ResourceEstimationPlugin.cs` — Semantic Kernel native plugin with `[KernelFunction("SuggestResources")]` that takes activity context string and available resources list, returns JSON array of suggestions

#### ViewModel & View

- [X] T080 [US6] Create ResourceAiEstimationViewModel in `Planova.UI/ViewModels/Resource/ResourceAiEstimationViewModel.cs` with ActivityId, IsEstimating (spinner state), IsAvailable, Suggestions ObservableCollection (each with editable Quantity), ErrorMessage, EstimateResourcesCommand, AcceptAllCommand (creates assignments for all suggestions), AcceptAdjustedCommand (takes adjusted quantities), RejectAllCommand (clears suggestions), AdjustQuantityCommand (per-suggestion quantity edit with live total cost preview)
- [X] T081 [US6] Create ResourceAiEstimationView in `Planova.UI/Views/Resource/ResourceAiEstimationView.xaml` bound to ResourceAiEstimationViewModel — activity info header, "Estimate Resources" button (disabled when IsEstimating or !IsAvailable), suggestions list when loaded: each item shows ResourceCode, ResourceName, ResourceType icon, SuggestedQuantity (editable), UnitOfMeasure, ConfidenceScore (progress bar or badge color: green>0.7, yellow 0.4-0.7, red<0.4), Reasoning tooltip; footer with Accept All, Accept As Adjusted, Reject All buttons; error state with explanatory message and manual suggestion link

**Checkpoint**: US6 fully functional — AI estimation with Semantic Kernel, suggestion preview, accept/adjust/reject, graceful unavailability handling

---

## Phase 9: User Story 7 - Generate Resource Reports (Priority: P3)

**Goal**: Users generate resource usage summary (grouped by activity) and resource cost report (grouped by type/crew/activity). Export to Excel (ClosedXML) and PDF (QuestPDF). Print preview.

**Independent Test**: Load resources onto activities, generate usage summary — verify data grouped by activity with quantities and costs. Generate cost report — verify grouped by type. Export to Excel — verify .xlsx opens with correct columns. Export to PDF — verify formatted document.

### Implementation for User Story 7

#### Services

- [X] T082 [P] [US7] Implement ResourceReportService in `Planova.Resource/Application/Services/ResourceReportService.cs` — GenerateUsageSummaryAsync (queries all ResourceAssignments for project with Activity/Resource includes, groups by Activity, returns ResourceUsageReportDto with ActivityResourceSection list and totals). GenerateCostReportAsync (groups assignments by ResourceType → Resource → Crew → Activity, returns ResourceCostReportDto with hierarchical CostSummarySections). ExportToExcelAsync (uses ClosedXML — creates workbook with formatted worksheet(s): Usage Summary sheet (Activity, Resource, Type, Qty, Rate, Cost columns with subtotals per activity) and Cost Report sheet (grouped sections with subtotals). ExportToPdfAsync (uses QuestPDF — formatted document with title page, table headers, alternating row colors, page numbers, section summaries)

#### ViewModel & View

- [X] T083 [US7] Create ResourceReportViewModel in `Planova.UI/ViewModels/Resource/ResourceReportViewModel.cs` with SelectedReportType (UsageSummary / CostReport), ReportData (object — cast to appropriate DTO), IsGenerating, HasData, GenerateReportCommand (with progress indication), ExportToExcelCommand, ExportToPdfCommand, PrintPreviewCommand
- [X] T084 [US7] Create ResourceReportView in `Planova.UI/Views/Resource/ResourceReportView.xaml` bound to ResourceReportViewModel — report type selector (radio buttons or tabs); generate button; report display area: DataGrid for usage summary (Activity, ResourceCode, ResourceName, Type, Quantity, Rate, TotalCost columns with activity subtotals) or hierarchical tree for cost report (group headers expandable with line items); export buttons (Excel, PDF); print preview button; loading spinner during generation

**Checkpoint**: US7 fully functional — usage and cost reports, Excel/PDF export, print preview

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Edge case handling, validation hardening, RTL support, performance optimization, resource import, and testing

- [ ] T085 [P] Implement edge case guards: reject resource hard-delete when referenced by CrewResource or ResourceAssignment (show message listing references — T086); warn on duplicate resource name save (FR-005b); block activity deletion when assignments exist (T071); skip assignments without dates in histogram (FR-025 edge case); handle AI estimation timeout gracefully (T078); prevent duplicate rate effective dates (T060); disable crew apply when CrewResources.Count == 0 (T065); handle all-inactive crew status
- [X] T086 [P] Implement resource reference checker in `ResourceService.DeleteAsync` — query CrewRepository for crews referencing this resource, query ResourceAssignmentRepository for assignments, build formatted message listing references, prevent hard-delete if any exist, suggest deactivation instead
- [ ] T087 [P] Add RTL layout support (FR-025) in all Resource Studio XAML views — FlowDirection binding to CultureInfo via ILocalizationService.IsRtl(). Histogram chart axis reversed for RTL. Report documents generated with RTL-aware QuestPDF layout
- [ ] T088 [P] Implement resource library import from Excel in `Planova.Excel/Readers/ResourceImportReader.cs` — parse spreadsheet rows into ImportRowDto list, detect duplicates by code and name, return ImportPreviewDto with valid/error/duplicate counts, execute import with skip/overwrite/rename handling based on user choice
- [ ] T089 [P] Implement ResourceImportService in `Planova.Resource/Application/Services/ResourceImportService.cs` — PreviewImportAsync (parses file, validates rows, detects duplicates), ExecuteImportAsync (creates resources for valid rows per duplicate handling strategy — Skip skips duplicates, Overwrite updates existing, Rename appends suffix to code/name)
- [ ] T090 [P] Implement ResourceCodeCounter persistence — store last-used sequence per ResourceType in a small DB table (e.g., a Sequence entity or use Resource table MAX query on first access, cache in memory with periodic persistence). Thread-safe via EF Core concurrency token
- [ ] T091 [P] Performance: ensure ResourceAssignment repository queries use AsNoTracking for read-only histogram/report queries. Index on ResourceUsage (ResourceId, Date) verified with query plan. Histogram data pre-aggregated in ResourceUsage table
- [ ] T092 [P] Write Resource domain entity tests in `tests/Planova.Resource.Tests/Domain/ResourceTests.cs` — status transitions (Active→Inactive, Inactive→Active, invalid transitions rejected), type discriminator validation
- [ ] T093 [P] Write Crew domain entity tests in `tests/Planova.Resource.Tests/Domain/CrewTests.cs` — status transitions (Draft→Active, Active→Inactive, Draft→Inactive), empty crew guard
- [ ] T094 [P] Write ResourceRate resolution tests in `tests/Planova.Resource.Tests/Application/ResourceRateServiceTests.cs` — exact date match returns correct rate, date-between-rates returns earlier rate, future date returns most recent, no rates returns default rate, unique effective date enforcement
- [ ] T095 [P] Write CrewService tests in `tests/Planova.Resource.Tests/Application/CrewServiceTests.cs` — blended rate calculation (2 carpenter + 1 foreman = expected total), apply to single activity (correct number of assignments created), apply to multiple activities, clone crew (all resources duplicated), empty crew apply blocked
- [ ] T096 [P] Write ResourceAssignmentService tests in `tests/Planova.Resource.Tests/Application/ResourceAssignmentServiceTests.cs` — create assignment with correct TotalCost calculation, update quantity triggers recalculation, delete with cleanup, activity deletion check blocks correctly
- [ ] T097 [P] Write ResourceHistogramService tests in `tests/Planova.Resource.Tests/Application/ResourceHistogramServiceTests.cs` — daily aggregation produces correct day-by-day totals, overallocation detection (assigned > available), date range filtering, empty assignments list returns empty histogram, assignments without dates excluded
- [ ] T098 [P] Write ResourceAiEstimationService tests in `tests/Planova.Resource.Tests/Application/ResourceAiEstimationServiceTests.cs` — successful estimation returns suggestions with confidence scores, provider unavailable returns empty with error message, accept suggestions creates correct assignments, adjust quantity before accept uses modified value
- [ ] T099 [P] Write ResourceReportService tests in `tests/Planova.Resource.Tests/Application/ResourceReportServiceTests.cs` — usage summary groups correctly by activity, cost report groups by type/crew/activity, Excel export generates valid workbook, PDF export generates document
- [ ] T100 [P] Write ResourceImportService tests in `tests/Planova.Resource.Tests/Application/ResourceImportServiceTests.cs` — preview detects duplicates, skip strategy works, overwrite updates existing, rename appends suffix, validation errors reported correctly
- [ ] T101 [P] Write ViewModel tests in `tests/Planova.UI.Tests/ViewModels/Resource/` — ResourceLibraryViewModelTests (load, filter, search, deactivate), ResourceEditorViewModelTests (type-specific field visibility, save validation), CrewTemplateManagerViewModelTests (create, clone, apply), ResourceAssignmentViewModelTests (add, edit, delete, total cost binding), ResourceHistogramViewModelTests (filter, export), ResourceAiEstimationViewModelTests (estimate, accept, reject, error state) — use Moq for service dependencies
- [ ] T102 [P] Run all tests via `dotnet test` — verify all pass with 0 failures
- [ ] T103 Integrate Resource Studio into Planova solution — update `App.xaml.cs` to call `services.AddPlanovaResource()`, verify all DI registrations resolve correctly at startup, verify RTL switching works with Resource Studio views

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational — MVP scope
- **US2 (Phase 4)**: Depends on Foundational (needs Resource entity + rates repo)
- **US3 (Phase 5)**: Depends on Foundational + US1 (needs resource library for crew composition)
- **US4 (Phase 6)**: Depends on Foundational + US1 + Phase 5 Activity Studio (needs activity entity)
- **US5 (Phase 7)**: Depends on Foundational + US4 (needs resource assignments for histogram data)
- **US6 (Phase 8)**: Depends on Foundational + US1 (needs resource library for suggestions) + US4 (needs activity context)
- **US7 (Phase 9)**: Depends on Foundational + US4 (needs assignments for report data)
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — No dependencies on other stories
- **User Story 2 (P1)**: Needs US1 for resource CRUD; rate management is standalone
- **User Story 3 (P2)**: Needs US1 for resource library to compose crews
- **User Story 4 (P1)**: Needs US1 + Activity Studio from Phase 5 for activity data
- **User Story 5 (P2)**: Needs US4 for assignment data
- **User Story 6 (P3)**: Needs US1 + US4 for activity context
- **User Story 7 (P3)**: Needs US4 for assignment data

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
- User stories 1 and 2 can start in parallel after Foundational (US1 and US2 share Resource entity but different repos/services)
- User story 3 depends on US1 — sequential after US1 resource library
- User story 4 depends on US1 + Phase 5 Activity — can start after US1
- User stories 5 and 7 share US4 dependency — can run in parallel after US4
- User story 6 can start in parallel with US5/US7 after US4
- Polish phase tasks marked [P] can run in parallel

---

## Parallel Example: Foundational Phase

```bash
# Create all enums in parallel:
Task: "T005 ResourceType enum in Planova.Resource/Domain/Enums/ResourceType.cs"
Task: "T006 ResourceScope enum in Planova.Resource/Domain/Enums/ResourceScope.cs"
Task: "T007 ResourceStatus enum in Planova.Resource/Domain/Enums/ResourceStatus.cs"
Task: "T008 CrewStatus enum in Planova.Resource/Domain/Enums/CrewStatus.cs"
Task: "T009 HistogramAggregation enum in Planova.Resource/Domain/Enums/HistogramAggregation.cs"

# Create all domain entities in parallel:
Task: "T010 Resource entity in Planova.Resource/Domain/Entities/Resource.cs"
Task: "T011 ResourceRate entity in Planova.Resource/Domain/Entities/ResourceRate.cs"
Task: "T012 Crew entity in Planova.Resource/Domain/Entities/Crew.cs"
Task: "T013 CrewResource entity in Planova.Resource/Domain/Entities/CrewResource.cs"
Task: "T014 ResourceAssignment entity in Planova.Resource/Domain/Entities/ResourceAssignment.cs"
Task: "T015 ResourceUsage entity in Planova.Resource/Domain/Entities/ResourceUsage.cs"

# Create all EF configurations in parallel:
Task: "T037 ResourceConfiguration in Planova.Persistence/EntityConfigurations/ResourceConfiguration.cs"
Task: "T038 ResourceRateConfiguration in Planova.Persistence/EntityConfigurations/ResourceRateConfiguration.cs"
Task: "T039 CrewConfiguration in Planova.Persistence/EntityConfigurations/CrewConfiguration.cs"
Task: "T040 CrewResourceConfiguration in Planova.Persistence/EntityConfigurations/CrewResourceConfiguration.cs"
Task: "T041 ResourceAssignmentConfiguration in Planova.Persistence/EntityConfigurations/ResourceAssignmentConfiguration.cs"
Task: "T042 ResourceUsageConfiguration in Planova.Persistence/EntityConfigurations/ResourceUsageConfiguration.cs"
```

## Parallel Example: US1 + US2 (P1 — can run concurrently)

```bash
# Team member A — US1 (Resource Library):
Task: "T049 ResourceRepository in Planova.Persistence/Repositories/ResourceRepository.cs"
Task: "T050 ResourceService in Planova.Resource/Application/Services/ResourceService.cs"
Task: "T052 ResourceStudioViewModel in Planova.UI/ViewModels/Resource/ResourceStudioViewModel.cs"
Task: "T053 ResourceLibraryViewModel in Planova.UI/ViewModels/Resource/ResourceLibraryViewModel.cs"
Task: "T054 ResourceEditorViewModel in Planova.UI/ViewModels/Resource/ResourceEditorViewModel.cs"
Task: "T055 ResourceStudioView in Planova.UI/Views/Resource/ResourceStudioView.xaml"
Task: "T056 ResourceLibraryView in Planova.UI/Views/Resource/ResourceLibraryView.xaml"
Task: "T057 ResourceEditorView in Planova.UI/Views/Resource/ResourceEditorView.xaml"
Task: "T058 Shell navigation registration"

# Team member B — US2 (Resource Rates):
Task: "T059 ResourceRateRepository in Planova.Persistence/Repositories/ResourceRateRepository.cs"
Task: "T060 ResourceRateService in Planova.Resource/Application/Services/ResourceRateService.cs"
Task: "T061 ResourceRateManagerViewModel in Planova.UI/ViewModels/Resource/ResourceRateManagerViewModel.cs"
Task: "T062 ResourceRateManagerView in Planova.UI/Views/Resource/ResourceRateManagerView.xaml"
```

## Parallel Example: US3-7 (staggered start based on dependencies)

```bash
# Team member A — US3 (Crew Templates) — wait for US1, then:
Task: "T063 CrewRepository in Planova.Persistence/Repositories/CrewRepository.cs"
Task: "T064 CrewResourceRepository in Planova.Persistence/Repositories/CrewResourceRepository.cs"
Task: "T065 CrewService in Planova.Resource/Application/Services/CrewService.cs"
Task: "T066 CrewTemplateManagerViewModel in Planova.UI/ViewModels/Resource/CrewTemplateManagerViewModel.cs"
Task: "T067 CrewTemplateEditorViewModel in Planova.UI/ViewModels/Resource/CrewTemplateEditorViewModel.cs"
Task: "T068 CrewTemplateManagerView in Planova.UI/Views/Resource/CrewTemplateManagerView.xaml"
Task: "T069 CrewTemplateEditorView in Planova.UI/Views/Resource/CrewTemplateEditorView.xaml"

# Team member B — US4 (Resource Assignments) — wait for US1, then:
Task: "T070 ResourceAssignmentRepository in Planova.Persistence/Repositories/ResourceAssignmentRepository.cs"
Task: "T071 ResourceAssignmentService in Planova.Resource/Application/Services/ResourceAssignmentService.cs"
Task: "T072 ResourceAssignmentViewModel in Planova.UI/ViewModels/Resource/ResourceAssignmentViewModel.cs"
Task: "T073 ResourceAssignmentView in Planova.UI/Views/Resource/ResourceAssignmentView.xaml"

# Team member C — US5 (Histogram) — wait for US4, then:
Task: "T074 ResourceUsageRepository in Planova.Persistence/Repositories/ResourceUsageRepository.cs"
Task: "T075 ResourceHistogramService in Planova.Resource/Application/Services/ResourceHistogramService.cs"
Task: "T076 ResourceHistogramViewModel in Planova.UI/ViewModels/Resource/ResourceHistogramViewModel.cs"
Task: "T077 ResourceHistogramView in Planova.UI/Views/Resource/ResourceHistogramView.xaml"

# Team member D — US6 (AI Estimation) — wait for US4, then:
Task: "T078 ResourceAiEstimationService in Planova.Resource/Application/Services/ResourceAiEstimationService.cs"
Task: "T079 ResourceEstimationPlugin in Planova.Resource/Application/Services/ResourceEstimationPlugin.cs"
Task: "T080 ResourceAiEstimationViewModel in Planova.UI/ViewModels/Resource/ResourceAiEstimationViewModel.cs"
Task: "T081 ResourceAiEstimationView in Planova.UI/Views/Resource/ResourceAiEstimationView.xaml"

# Team member E — US7 (Reports) — wait for US4, then:
Task: "T082 ResourceReportService in Planova.Resource/Application/Services/ResourceReportService.cs"
Task: "T083 ResourceReportViewModel in Planova.UI/ViewModels/Resource/ResourceReportViewModel.cs"
Task: "T084 ResourceReportView in Planova.UI/Views/Resource/ResourceReportView.xaml"
```

---

## Implementation Strategy

### MVP First (Phase 1 + 2 + 3 + 6)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (Resource Library CRUD)
4. Complete Phase 6: User Story 4 (Resource Loading onto Activities) — requires US1 + Phase 5 Activity Studio
5. **STOP and VALIDATE**: Test US1+US4 independently — create resources, assign to activities, verify cost calculation
6. Deploy/demo if ready — MVP delivers core resource management + assignment

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (P1, Resource Library) → Test independently → **MVP core**
3. Add US4 (P1, Resource Loading) → Test independently → **MVP with cost**
4. Add US2 (P1, Rate Management) → Test independently → Deploy/Demo
5. Add US3 (P2, Crew Templates) → Test independently → Deploy/Demo
6. Add US5 (P2, Histogram) → Test independently → Deploy/Demo
7. Add US6 (P3, AI Estimation) → Test independently → Deploy/Demo
8. Add US7 (P3, Reports) → Test independently → Deploy/Demo
9. Phase 10: Polish, edge cases, tests, RTL, import
10. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. **Team** completes Setup + Foundational together
2. Once Foundation is done:
   - Developer A: US1 (Resource Library) — MVP critical path
   - Developer B: US2 (Rate Management) — can run parallel to US1
   - Developer C: Standby — wait for US1 to complete, then US3 (Crews)
3. After US1 + US4 setup:
   - Developer A: US4 (Resource Loading)
   - Developer B: US3 (Crew Templates)
   - Developer C: US2 (if not done) → US5 (Histogram)
4. After US4:
   - Developer A: US5 (Histogram)
   - Developer B: US6 (AI Estimation)
   - Developer C: US7 (Reports)
5. Phase 10: Polish, tests, RTL, import — shared across team

---

## Notes

- [P] tasks = different files, no dependencies
- [US1-7] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- US1, US2, and US4 are P1 — prioritize these for MVP delivery
- Follow existing Phase 5 (Activity Studio) patterns for consistency
- Histogram uses LiveCharts2 bar chart (per research.md decision — not custom canvas)
- AI estimation uses Semantic Kernel plugin (per constitution principle VI)
- Resource assignments must be atomic with daily usage regeneration (single transaction)
- Rate resolution uses SQL query (per research.md decision — not in-memory)
- Import uses existing Phase 2 ClosedXML infrastructure (per research.md decision)
- Resource code counter: use DB table for persistence + in-memory cache for performance
