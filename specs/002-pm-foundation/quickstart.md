# Quickstart: Project Management Foundation

**Date**: 2026-06-01

## Scope

Implement the core Project Management entities, CRUD operations, dashboard, and reporting views on top of the existing Phase 0 WPF shell.

## Implementation Order

### 1. Domain Layer — Entities & Value Objects

| File | Path |
|------|------|
| Project.cs | `Planova.Domain/Entities/Project.cs` |
| Client.cs | `Planova.Domain/Entities/Client.cs` |
| Contract.cs | `Planova.Domain/Entities/Contract.cs` |
| UserProfile.cs | `Planova.Domain/Entities/UserProfile.cs` (extend existing UserPreferences) |
| ProjectStatus.cs | `Planova.Domain/ValueObjects/ProjectStatus.cs` |
| Currency.cs | `Planova.Domain/ValueObjects/Currency.cs` |

**Key decisions**: POCO entities, ProjectStatus value object with transition map, data annotations for required fields.

### 2. Persistence Layer — EF Core Configurations & Migrations

| File | Path |
|------|------|
| ProjectConfiguration.cs | `Planova.Persistence/EntityConfigurations/ProjectConfiguration.cs` |
| ClientConfiguration.cs | `Planova.Persistence/EntityConfigurations/ClientConfiguration.cs` |
| ContractConfiguration.cs | `Planova.Persistence/EntityConfigurations/ContractConfiguration.cs` |
| UserProfileConfiguration.cs | `Planova.Persistence/EntityConfigurations/UserProfileConfiguration.cs` (extend existing) |
| PlanovaDbContext.cs | Add DbSet<> properties and ApplyConfiguration calls |
| Migrations | `dotnet ef migrations add AddProjectManagement` |

**Key decisions**: Unique indexes on Code/Name/Number fields; SQLite datetime('now') defaults; cascade behavior configured to prevent accidental deletions.

### 3. Application Layer — Service Interfaces & DTOs

| File | Path |
|------|------|
| IProjectService.cs | `Planova.Application/Services/IProjectService.cs` |
| IClientService.cs | `Planova.Application/Services/IClientService.cs` |
| IContractService.cs | `Planova.Application/Services/IContractService.cs` |
| IUserProfileService.cs | `Planova.Application/Services/IUserProfileService.cs` |
| IDashboardService.cs | `Planova.Application/Services/IDashboardService.cs` |
| IReportService.cs | `Planova.Application/Services/IReportService.cs` |
| DTOs (all) | `Planova.Application/Dto/*.cs` |

**Key decisions**: Service interfaces define use cases; DTOs are records (immutable by default); CancellationToken on all async methods.

### 4. Infrastructure Layer — Service Implementations

| File | Path |
|------|------|
| ProjectService.cs | `Planova.Application/Services/ProjectService.cs` (consider moving implementations to a Services folder) |
| ClientService.cs | Same pattern |
| ContractService.cs | Same pattern |
| DashboardService.cs | Same pattern |
| ReportService.cs | Same pattern |
| UserProfileService.cs | Same pattern |

**Key decisions**: Services orchestrate validation → mapping → persistence → mapping to DTOs. Use EF Core directly via PlanovaDbContext injected through constructor.

### 5. Localization — RESX Resources

| File | Path |
|------|------|
| Strings.en.resx | Add PM entity labels, dashboard labels, report labels, validation messages |
| Strings.ar.resx | Arabic translations for all new strings |

### 6. UI Layer — Workspaces & Views

#### Register Navigation Targets

In `ShellViewModel.cs`, register new workspace targets:
```csharp
_navigationService.RegisterTarget("dashboard", "Dashboard", () => _serviceProvider.GetRequiredService<DashboardView>());
_navigationService.RegisterTarget("projects", "Projects", () => _serviceProvider.GetRequiredService<ProjectsWorkspaceView>());
// ... etc
```

#### Create Views & ViewModels

| ViewModel | View | Workspace |
|-----------|------|-----------|
| ProjectsWorkspaceViewModel | ProjectsWorkspaceView (list + detail pane) | projects |
| ClientsWorkspaceViewModel | ClientsWorkspaceView (list + detail pane) | clients |
| ContractsWorkspaceViewModel | ContractsWorkspaceView (list + detail pane) | contracts |
| UserProfileViewModel | UserProfileView (form) | profile |
| DashboardViewModel | DashboardView (cards + activity) | dashboard |
| ReportViewModel | ReportView (data grids) | reports |

**Key decisions**: Use existing TabWorkspace + NavigationRail pattern. Each workspace uses list-detail layout where applicable. Commands bound via `[RelayCommand]`.

### 7. Dashboard & Reports

**Dashboard**: Project health cards (counts by status), recent activity summary (last 5 updated entities), quick action buttons (New Project, New Client).

**Reports**: Read-only grids for Projects, Clients, Contracts with summary data. Print-friendly via FlowDocument or QuestPDF export.

## Testing Approach

| Layer | Tool | Focus |
|-------|------|-------|
| Domain | xUnit | Value object transition validation, entity invariants |
| Application | xUnit + Moq | Service orchestration, validation, error handling |
| Persistence | xUnit + SQLite in-memory | EF configuration, query correctness, unique constraints |
| UI | xUnit + ViewModel tests | Command execution, observable property changes, navigation |
