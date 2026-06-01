---

description: "Task list for Project Management Foundation feature implementation"
---

# Tasks: Project Management Foundation

**Input**: Design documents from `/specs/002-pm-foundation/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The tasks below include optional test tasks. Tests are only required if the project has an established testing infrastructure.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

All paths are relative to the repository root. The solution uses a multi-project Clean Architecture layout:
- `Planova.Domain/` — Domain layer (entities, value objects)
- `Planova.Application/` — Application layer (service interfaces, DTOs, mappings)
- `Planova.Persistence/` — Persistence layer (EF Core configurations, repositories, migrations)
- `Planova.Localization/` — Localization resources (RESX files)
- `Planova.UI/` — WPF UI layer (ViewModels, Views)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create project directory structure for PM module (add new folders: Planova.Domain/ValueObjects/, Planova.Application/Dto/, Planova.Application/Mappings/, Planova.Application/Exceptions/, Planova.Application/Repositories/, Planova.UI/Views/Projects/, Planova.UI/Views/Clients/, Planova.UI/Views/Contracts/, Planova.UI/Views/Dashboard/, Planova.UI/Views/Reports/, Planova.UI/Views/Profile/)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain model, shared DTOs, service interfaces, and infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T002 [P] Create Project entity in Planova.Domain/Entities/Project.cs
- [X] T003 [P] Create Client entity in Planova.Domain/Entities/Client.cs
- [X] T004 [P] Create Contract entity in Planova.Domain/Entities/Contract.cs
- [X] T005 [P] Extend UserPreferences entity with DisplayName, RoleLabel, OrganizationName, DefaultWorkspace fields in Planova.Domain/Entities/UserPreferences.cs
- [X] T006 [P] Create ProjectStatus value object with transition map in Planova.Domain/ValueObjects/ProjectStatus.cs
- [X] T007 [P] Create Currency value object in Planova.Domain/ValueObjects/Currency.cs
- [X] T008 [P] Create all DTOs (ProjectSummaryDto, ProjectDetailDto, CreateProjectDto, UpdateProjectDto, ClientSummaryDto, ClientDetailDto, CreateClientDto, UpdateClientDto, ContractSummaryDto, ContractDetailDto, CreateContractDto, UpdateContractDto, DashboardSummaryDto, UserProfileDto) in Planova.Application/Dto/
- [X] T009 [P] Create service interfaces (IProjectService, IClientService, IContractService, IUserProfileService, IDashboardService, IReportService) in Planova.Application/Services/
- [X] T010 [P] Create exception classes (DuplicateEntityException, EntityNotFoundException, EntityInUseException, InvalidTransitionException, ValidationException) in Planova.Application/Exceptions/
- [X] T011 [P] Create MappingProfile with entity-to-DTO mappings in Planova.Application/Mappings/MappingProfile.cs
- [X] T012 [P] Create repository interfaces (IProjectRepository, IClientRepository, IContractRepository, IUserProfileRepository) in Planova.Application/Repositories/ and EF Core implementations in Planova.Persistence/Repositories/
- [X] T013 [P] Create entity configurations (ProjectConfiguration, ClientConfiguration, ContractConfiguration, UserProfileConfiguration) in Planova.Persistence/EntityConfigurations/
- [X] T014 Update PlanovaDbContext with DbSet\<Project\>, DbSet\<Client\>, DbSet\<Contract\> properties and ApplyConfiguration calls in Planova.Persistence/DbContext/PlanovaDbContext.cs
- [X] T015 Add EF migration "AddProjectManagement" via `dotnet ef migrations add`
- [X] T016 [P] Add base localization strings for PM module navigation labels and common terms to Planova.Localization/Resources/Strings.en.resx and Strings.ar.resx
- [X] T017 Register all service interfaces with implementations in the DI container (Planova.UI/App.xaml.cs)
- [X] T018 Register all new workspace navigation targets in Planova.UI/ViewModels/ShellViewModel.cs (dashboard, projects, clients, contracts, profile, reports)

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 — Manage Projects (Priority: P1) 🎯 MVP

**Goal**: A user can create, view, edit, search, and track projects with key metadata including status, dates, client association, and contract linkage.

**Independent Test**: Create a new project with required fields (name, code, status), verify it appears in the project list, edit its details, confirm changes persist after refresh.

### Tests for User Story 1 (OPTIONAL) ⚠️

- [ ] T019 [P] [US1] Unit test ProjectStatus value object transition validation in tests/Domain.Tests/ProjectStatusTests.cs
- [ ] T020 [P] [US1] Unit test Project entity invariants in tests/Domain.Tests/ProjectTests.cs
- [ ] T021 [P] [US1] Unit test ProjectService CRUD, search, status transitions in tests/Application.Tests/ProjectServiceTests.cs
- [ ] T022 [P] [US1] Integration test project persistence, unique code constraint, and status query in tests/Persistence.Tests/ProjectRepositoryTests.cs

### Implementation for User Story 1

- [X] T023 [US1] Implement ProjectService (IProjectService) with CRUD, SearchAsync, ChangeStatusAsync, GetByStatusAsync, GetDashboardSummaryAsync in Planova.Application/Services/ProjectService.cs
- [X] T024 [US1] Create ProjectsWorkspaceViewModel with [RelayCommand] for CRUD, search, status filtering in Planova.UI/ViewModels/ProjectsWorkspaceViewModel.cs
- [X] T025 [P] [US1] Create ProjectsWorkspaceView.xaml with list, detail, and edit sections in Planova.UI/Views/Projects/
- [X] T026 [P] [US1] Project detail view integrated in ProjectsWorkspaceView.xaml with read-only display and status transitions
- [X] T027 [P] [US1] Project edit form integrated in ProjectsWorkspaceView.xaml with validation
- [X] T028 [US1] Add project code uniqueness validation and client linking logic in Planova.Application/Services/ProjectService.cs
- [X] T029 [US1] Add project-specific localization strings (field labels, status names, validation messages) to Planova.Localization/Resources/Strings.en.resx and Strings.ar.resx

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 — Manage Clients (Priority: P1)

**Goal**: A user can register and maintain client organizations with contact details, and view which projects are linked to each client.

**Independent Test**: Create a client record, verify it appears in the client list, link it to a project, confirm the relationship is visible from both client detail and project views.

### Tests for User Story 2 (OPTIONAL) ⚠️

- [ ] T030 [P] [US2] Unit test Client entity validation in tests/Domain.Tests/ClientTests.cs
- [ ] T031 [P] [US2] Unit test ClientService CRUD and search in tests/Application.Tests/ClientServiceTests.cs
- [ ] T032 [P] [US2] Integration test client persistence and unique constraints in tests/Persistence.Tests/ClientRepositoryTests.cs

### Implementation for User Story 2

- [X] T033 [US2] Implement ClientService (IClientService) with CRUD, SearchAsync in Planova.Application/Services/ClientService.cs
- [X] T034 [US2] Create ClientsWorkspaceViewModel with [RelayCommand] for CRUD and search in Planova.UI/ViewModels/ClientsWorkspaceViewModel.cs
- [X] T035 [P] [US2] Create ClientsWorkspaceView.xaml with list, detail, and edit sections in Planova.UI/Views/Clients/
- [X] T036 [P] [US2] Client detail integrated in ClientsWorkspaceView.xaml with linked projects list
- [X] T037 [P] [US2] Client edit form integrated in ClientsWorkspaceView.xaml
- [X] T038 [US2] Add unique code and name validation in Planova.Application/Services/ClientService.cs
- [X] T039 [US2] Add client-specific localization strings to Planova.Localization/Resources/Strings.en.resx and Strings.ar.resx

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 — Manage Contracts (Priority: P2)

**Goal**: A user can create and manage contracts tied to specific projects and clients, including contract value, currency, and date tracking.

**Independent Test**: Create a project and client, then create a contract linked to both, verify the contract appears under the project and client detail views.

### Tests for User Story 3 (OPTIONAL) ⚠️

- [ ] T040 [P] [US3] Unit test Contract entity validation in tests/Domain.Tests/ContractTests.cs
- [ ] T041 [P] [US3] Unit test ContractService CRUD, search, GetByProject, GetByClient in tests/Application.Tests/ContractServiceTests.cs
- [ ] T042 [P] [US3] Integration test contract persistence and unique number constraint in tests/Persistence.Tests/ContractRepositoryTests.cs

### Implementation for User Story 3

- [X] T043 [US3] Implement ContractService (IContractService) with CRUD, SearchAsync, GetByProjectAsync, GetByClientAsync in Planova.Application/Services/ContractService.cs
- [X] T044 [US3] Create ContractsWorkspaceViewModel with [RelayCommand] for CRUD, search, filtering in Planova.UI/ViewModels/ContractsWorkspaceViewModel.cs
- [X] T045 [P] [US3] Create ContractsWorkspaceView.xaml with list, detail, and edit sections in Planova.UI/Views/Contracts/
- [X] T046 [P] [US3] Contract detail integrated in ContractsWorkspaceView.xaml with project/client info
- [X] T047 [P] [US3] Contract edit form integrated in ContractsWorkspaceView.xaml with project/client selectors
- [X] T048 [US3] Add unique number validation, project/client existence checks, value and date validation in Planova.Application/Services/ContractService.cs
- [X] T049 [US3] Add contract-specific localization strings to Planova.Localization/Resources/Strings.en.resx and Strings.ar.resx

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 — Configure User Profile (Priority: P2)

**Goal**: A user can view and update their application profile including display name, language preference, theme, and default workspace behavior.

**Independent Test**: Change the display name and language preference, confirm the UI switches to Arabic, verify the preference persists after restart.

### Tests for User Story 4 (OPTIONAL) ⚠️

- [ ] T050 [P] [US4] Unit test UserProfileService read/update in tests/Application.Tests/UserProfileServiceTests.cs

### Implementation for User Story 4

- [X] T051 [US4] Implement UserProfileService (IUserProfileService) with GetProfileAsync, UpdateProfileAsync in Planova.Persistence/Services/UserProfileService.cs
- [X] T052 [US4] Create UserProfileViewModel with [RelayCommand] for save and language/theme switch in Planova.UI/ViewModels/UserProfileViewModel.cs
- [X] T053 [US4] Create UserProfileView.xaml with form fields for display name, language, theme, default workspace in Planova.UI/Views/Profile/
- [X] T054 [US4] Wire language switch to trigger LocalizationService refresh and RTL UI toggle in Planova.UI/ViewModels/UserProfileViewModel.cs
- [X] T055 [US4] Add profile-specific localization strings to Planova.Localization/Resources/Strings.en.resx and Strings.ar.resx

**Checkpoint**: User profile personalization works — language and theme preferences persist

---

## Phase 7: User Story 5 — View Project Dashboard (Priority: P2)

**Goal**: A user can see a dashboard with project health cards, status distribution, recent activity, and quick actions for an at-a-glance command center.

**Independent Test**: Create projects in different statuses, open the dashboard, verify health cards and status distribution reflect current data.

### Tests for User Story 5 (OPTIONAL) ⚠️

- [ ] T056 [P] [US5] Unit test DashboardService aggregation logic in tests/Application.Tests/DashboardServiceTests.cs

### Implementation for User Story 5

- [X] T057 [US5] Implement DashboardService (IDashboardService) with project status counts, recent activity, total counts in Planova.Persistence/Services/DashboardService.cs
- [X] T058 [US5] Create DashboardViewModel with observable properties for health cards and activity in Planova.UI/ViewModels/DashboardViewModel.cs
- [X] T059 [US5] Create DashboardView.xaml with health cards, status distribution display, recent activity list, quick action buttons in Planova.UI/Views/Dashboard/
- [X] T060 [US5] Wire quick action buttons to navigate to create project / create client workspaces in Planova.UI/ViewModels/DashboardViewModel.cs
- [X] T061 [US5] Add dashboard-specific localization strings to Planova.Localization/Resources/Strings.en.resx and Strings.ar.resx

**Checkpoint**: Dashboard provides accurate at-a-glance project overview

---

## Phase 8: User Story 6 — View Reports (Priority: P3)

**Goal**: A user can open summary report views for projects, clients, and contracts with printable or export-friendly formatting.

**Independent Test**: Populate projects, clients, and contracts, open each report view, verify data renders correctly in a formatted preview.

### Tests for User Story 6 (OPTIONAL) ⚠️

- [ ] T062 [P] [US6] Unit test ReportService aggregation logic in tests/Application.Tests/ReportServiceTests.cs

### Implementation for User Story 6

- [X] T063 [US6] Implement ReportService (IReportService) with project, client, contract summary queries in Planova.Persistence/Services/ReportService.cs
- [X] T064 [US6] Create ReportViewModel with tab selection for report types in Planova.UI/ViewModels/ReportViewModel.cs
- [X] T065 [US6] Create ReportView.xaml with summary grids for projects, clients, contracts in Planova.UI/Views/Reports/
- [ ] T066 [US6] Add QuestPDF export functionality in Planova.Application/Services/ReportService.cs and Planova.UI/ViewModels/ReportViewModel.cs (deferred - requires QuestPDF library setup)
- [X] T067 [US6] Add report-specific localization strings to Planova.Localization/Resources/Strings.en.resx and Strings.ar.resx

**Checkpoint**: All report views render correctly and support export

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T068 [P] Verify RTL layout rendering for all views in Planova.UI/Views/ when Arabic language is selected (RTL handled by ShellViewModel.OnLanguageChanged setting Window FlowDirection)
- [X] T069 [P] Add CancellationToken support to all async ViewModel commands in Planova.UI/ViewModels/
- [X] T070 [P] Add edge case handling (entity deletion protection, validation, status transitions) across Planova.Application/Services/ and Planova.UI/ViewModels/
- [X] T071 Run final validation: `dotnet build` succeeds with 0 warnings, 0 errors
- [ ] T072 [P] Performance optimization: deferred until load testing can identify bottlenecks

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3–8)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

| User Story | Blocks | Depends On | Parallelizable |
|------------|--------|------------|----------------|
| US1 Projects (P1) | — | Foundational | Yes — with US2, US4 |
| US2 Clients (P1) | — | Foundational | Yes — with US1, US4 |
| US3 Contracts (P2) | — | Foundational | Yes — entity references exist from Foundational |
| US4 Profile (P2) | — | Foundational | Yes — with US1, US2 |
| US5 Dashboard (P2) | — | Foundational | Yes — aggregates via services |
| US6 Reports (P3) | — | Foundational | Yes — aggregates via services |

All user stories are independently implementable because the entities, DTOs, service interfaces, and repository interfaces are in Foundational. Each story only needs to implement its own service and UI.

### Within Each User Story

- Tests (if included) should be written before implementation
- Models before services
- Services before ViewModels
- ViewModels before Views
- Core implementation before integration

### Parallel Opportunities

- All [P] tasks within a phase can run in parallel
- All Foundational [P] tasks can run in parallel
- US1, US2, US3, US4 can start simultaneously after Foundational
- Different user stories can be worked on in parallel by different developers

---

## Parallel Example: User Story 1

```bash
# Launch all US1 tests together (if tests requested):
Task: "Unit test ProjectStatus in tests/Domain.Tests/ProjectStatusTests.cs"
Task: "Unit test ProjectService in tests/Application.Tests/ProjectServiceTests.cs"

# Launch all US1 Views together (independent files):
Task: "Create ProjectListView.xaml in Planova.UI/Views/Projects/"
Task: "Create ProjectDetailView.xaml in Planova.UI/Views/Projects/"
Task: "Create ProjectEditView.xaml in Planova.UI/Views/Projects/"

# Then implement service and ViewModel (sequential):
Task: "Implement ProjectService in Planova.Application/Services/ProjectService.cs"
Task: "Create ProjectsWorkspaceViewModel in Planova.UI/ViewModels/"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (Manage Projects)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Projects) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Clients) → Test independently → Deploy/Demo
4. Add US3 (Contracts) → Test independently → Deploy/Demo
5. Add US4 (Profile) → Test independently → Deploy/Demo
6. Add US5 (Dashboard) → Test independently → Deploy/Demo
7. Add US6 (Reports) → Test independently → Deploy/Demo
8. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Projects) + US3 (Contracts)
   - Developer B: US2 (Clients) + US4 (Profile)
   - Developer C: US5 (Dashboard) + US6 (Reports)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Tests are optional — only include if testing infrastructure is in place
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same-file conflicts, cross-story dependencies that break independence
