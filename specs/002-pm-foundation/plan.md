# Implementation Plan: Project Management Foundation

**Branch**: `002-pm-foundation` | **Date**: 2026-06-01 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-pm-foundation/spec.md`

## Summary

Deliver the core project management domain on top of the Phase 0 WPF shell: Projects, Clients, Contracts, User Profiles, Dashboard, and Reports. Uses Clean Architecture layers with EF Core/SQLite persistence, MVVM UI with Navigation Rail + Tab Workspace pattern, English/Arabic localization, and Serilog diagnostics. All entity CRUD, status lifecycle enforcement, relationship integrity, and dashboard aggregation served through Application-layer services.

## Technical Context

**Language/Version**: .NET 8, C# 12

**Primary Dependencies**: Fluent UI WPF, CommunityToolkit.Mvvm, Microsoft.Extensions.Hosting, EF Core 8, SQLite, Serilog, QuestPDF

**Storage**: SQLite via EF Core 8 (code-first migrations, unique indexes on Code/Name/Number fields)

**Testing**: xUnit, Moq, SQLite in-memory for persistence tests

**Target Platform**: Windows 10+ (WPF Desktop)

**Project Type**: Desktop Application (WPF + MVVM)

**Performance Goals**: <2s load time for list/detail views at 1,000 records; <2s dashboard render; report preview <3s

**Constraints**: Clean Architecture layering (no inward violations); MVVM (no code-behind logic); async by default with CancellationToken; runtime RTL switching; no hardcoded strings; repository pattern only per documented ADR

**Scale/Scope**: 1,000 records per entity (Project, Client, Contract); single concurrent user; 6 workspace views + dashboard + 3 report views

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Evidence |
|------|--------|----------|
| Clean Architecture compliant | PASS | Entities in Domain, services in Application, EF config in Persistence, views in UI. Dependencies point inward. |
| MVVM compliant | PASS | ViewModels use CommunityToolkit.Mvvm [RelayCommand], Views remain declarative. |
| Dependency Injection compliant | PASS | All services registered via Microsoft.Extensions.Hosting DI container in App.xaml.cs. |
| Localization (English + Arabic) | PASS | New RESX resource pairs added to Planova.Localization.Resources. |
| Performance (async, CancellationToken) | PASS | All service interfaces and ViewModel commands accept CancellationToken. |
| Repository pattern justified | PASS | See ADR in research.md — required for Clean Architecture dependency inversion (Application layer must not depend on EF Core). |
| No workflow engine / automation designer | PASS | Phase 1 scope explicitly excludes workflow engines, automation designers, rule builders. |
| AI Provider agnostic | PASS | No AI dependencies introduced in this phase. |

All gates pass. No violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/002-pm-foundation/
├── plan.md              # This file
├── spec.md              # Feature specification (input)
├── research.md          # Technology decisions and dependency analysis
├── data-model.md        # Entity definitions, relationships, validation rules
├── quickstart.md        # Implementation order and file mapping
├── contracts/           # Service interface contracts
│   ├── index.md
│   ├── IProjectService.cs.md
│   ├── IClientService.cs.md
│   └── IContractService.cs.md
└── tasks.md             # Task breakdown (Phase 1-9)
```

### Source Code (repository root)

```text
Planova.sln
├── Planova.Domain/
│   ├── Entities/
│   │   ├── Project.cs
│   │   ├── Client.cs
│   │   ├── Contract.cs
│   │   └── UserProfile.cs              (extends existing UserPreferences)
│   └── ValueObjects/
│       ├── ProjectStatus.cs             (7 states + transition map)
│       └── Currency.cs                  (ISO 3-letter code)
│
├── Planova.Application/
│   ├── Dto/
│   │   ├── ProjectSummaryDto.cs
│   │   ├── ProjectDetailDto.cs
│   │   ├── CreateProjectDto.cs
│   │   ├── UpdateProjectDto.cs
│   │   ├── ClientSummaryDto.cs
│   │   ├── ClientDetailDto.cs
│   │   ├── CreateClientDto.cs
│   │   ├── UpdateClientDto.cs
│   │   ├── ContractSummaryDto.cs
│   │   ├── ContractDetailDto.cs
│   │   ├── CreateContractDto.cs
│   │   ├── UpdateContractDto.cs
│   │   ├── DashboardSummaryDto.cs
│   │   └── UserProfileDto.cs
│   ├── Services/
│   │   ├── IProjectService.cs
│   │   ├── IClientService.cs
│   │   ├── IContractService.cs
│   │   ├── IUserProfileService.cs
│   │   ├── IDashboardService.cs
│   │   ├── IReportService.cs
│   │   ├── ProjectService.cs
│   │   ├── ClientService.cs
│   │   ├── ContractService.cs
│   │   ├── DashboardService.cs
│   │   ├── ReportService.cs
│   │   └── UserProfileService.cs
│   ├── Exceptions/
│   │   ├── DuplicateEntityException.cs
│   │   ├── EntityNotFoundException.cs
│   │   ├── EntityInUseException.cs
│   │   ├── InvalidTransitionException.cs
│   │   └── ValidationException.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs
│   └── Repositories/
│       ├── IProjectRepository.cs
│       ├── IClientRepository.cs
│       ├── IContractRepository.cs
│       └── IUserProfileRepository.cs
│
├── Planova.Persistence/
│   ├── DbContext/
│   │   └── PlanovaDbContext.cs          (extended with DbSet properties)
│   ├── EntityConfigurations/
│   │   ├── ProjectConfiguration.cs
│   │   ├── ClientConfiguration.cs
│   │   ├── ContractConfiguration.cs
│   │   └── UserProfileConfiguration.cs  (extend existing)
│   └── Repositories/
│       ├── ProjectRepository.cs
│       ├── ClientRepository.cs
│       ├── ContractRepository.cs
│       └── UserProfileRepository.cs
│
├── Planova.Localization/
│   └── Resources/
│       ├── Strings.en.resx              (extended with PM labels)
│       └── Strings.ar.resx              (extended with Arabic PM labels)
│
└── Planova.UI/
    ├── App.xaml.cs                      (extended DI registrations)
    ├── ViewModels/
    │   ├── ShellViewModel.cs            (extended navigation targets)
    │   ├── ProjectsWorkspaceViewModel.cs
    │   ├── ClientsWorkspaceViewModel.cs
    │   ├── ContractsWorkspaceViewModel.cs
    │   ├── UserProfileViewModel.cs
    │   ├── DashboardViewModel.cs
    │   └── ReportViewModel.cs
    └── Views/
        ├── Dashboard/
        │   └── DashboardView.xaml
        ├── Projects/
        │   └── ProjectsWorkspaceView.xaml
        ├── Clients/
        │   └── ClientsWorkspaceView.xaml
        ├── Contracts/
        │   └── ContractsWorkspaceView.xaml
        ├── Profile/
        │   └── UserProfileView.xaml
        └── Reports/
            └── ReportView.xaml
```

**Structure Decision**: Multi-project Clean Architecture layout with 6 core projects (Domain, Application, Persistence, Localization, UI) extending the Phase 0 solution. Each layer owns its concerns per Clean Architecture rules. Service implementations live in their respective layer (Application for orchestration, Persistence for data access). No additional projects added — Phase 0 project set is sufficient.

## Complexity Tracking

> All Constitution Check gates pass. No violations to justify.
