# Implementation Plan: Project Management Foundation

**Branch**: `002-pm-foundation` | **Date**: 2026-06-01 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-pm-foundation/spec.md`

## Summary

Build the project management foundation (Projects, Clients, Contracts, User Profiles, Dashboard, Reporting) on the existing Phase 0 WPF shell, following Clean Architecture with .NET 8, SQLite/EF Core persistence, WPF+MVVM UI, and English/Arabic localization.

## Technical Context

**Language/Version**: .NET 8 / C# 12

**Primary Dependencies**: WPF, Fluent UI WPF, CommunityToolkit.Mvvm, EF Core 8, Serilog, SQLite

**Storage**: SQLite via EF Core (Code First Migrations, versioned schema)

**Testing**: xUnit, Moq (mocking), FluentAssertions

**Target Platform**: Windows 10+ (WPF desktop application)

**Project Type**: desktop-app

**Performance Goals**: Dashboard loads in under 2s for 1,000 records; list search/filter results in under 2s; report previews render in under 3s

**Constraints**: Clean Architecture enforced; MVVM pattern required; RTL + Arabic localization must work; Repository pattern prohibited unless justified; UI thread blocking prohibited; async operations with CancellationToken

**Scale/Scope**: Single concurrent user; up to 1,000 records per entity (Project, Client, Contract); desktop-only (no mobile/responsive)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Check I — Architecture First**: Feature aligns with Clean Architecture layers — Domain entities, Application use cases, Infrastructure persistence, WPF UI. No violations.

**Check II — MVVM & Fluent UI**: Feature builds WPF views with MVVM via CommunityToolkit.Mvvm and Fluent UI WPF styling. No violations.

**Check III — Modular Domain Design**: Project Management belongs under the Core module. It establishes the foundational domain model that later Studio modules will depend on. No violations.

**Check IV — Build vs Buy Strategy**: No build-vs-buy conflicts — feature builds proprietary entity management and dashboard UI, not commodity infrastructure. No violations.

**Check V — Automation Platform Agnostic**: No automation platform dependencies introduced. No violations.

**Check VI — AI Provider Agnostic**: No AI integration in scope. No violations.

**Check VII — Multilingual First**: English + Arabic localization specified, RTL support required. No violations.

**Check VIII — Performance & Scalability**: Async operations, CancellationToken support, efficient queries mandated. No violations.

**Result**: ALL GATES PASSED. Proceeding to Phase 0 research.

## Project Structure

### Documentation (this feature)

```text
specs/002-pm-foundation/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── ...
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

The existing multi-project solution aligns with Clean Architecture layers:

```text
Planova.Domain/                     # Domain layer - entities only
├── Entities/                       
│   ├── Project.cs                  # NEW
│   ├── Client.cs                   # NEW
│   ├── Contract.cs                 # NEW
│   └── UserPreferences.cs          # Existing
├── ValueObjects/                   # NEW - shared value objects
│   ├── ProjectStatus.cs
│   └── Currency.cs

Planova.Application/                # Application layer - use cases
├── Services/                       
│   ├── IProjectService.cs          # NEW
│   ├── IClientService.cs           # NEW
│   ├── IContractService.cs         # NEW
│   ├── IUserProfileService.cs      # NEW
│   ├── IDashboardService.cs        # NEW
│   └── IReportService.cs           # NEW
├── Dto/                            # NEW
│   ├── ProjectDto.cs
│   ├── ClientDto.cs
│   ├── ContractDto.cs
│   └── DashboardSummaryDto.cs
├── Mappings/                       # NEW
│   └── MappingProfile.cs

Planova.Persistence/                # Persistence layer - EF Core
├── EntityConfigurations/
│   ├── ProjectConfiguration.cs     # NEW
│   ├── ClientConfiguration.cs      # NEW
│   ├── ContractConfiguration.cs    # NEW
│   └── UserPreferencesConfiguration.cs  # Existing
├── Repositories/                   # NEW (justified by Clean Architecture boundaries)
│   ├── ProjectRepository.cs
│   ├── ClientRepository.cs
│   └── ContractRepository.cs
├── Migrations/                     # NEW migrations
│   └── ...

Planova.Infrastructure/             # Infrastructure - cross-cutting
└── ...

Planova.Localization/               # Localization resources
├── Resources/
│   ├── Strings.en.resx
│   ├── Strings.ar.resx
│   └── * (add PM module resources) # NEW

Planova.UI/                         # WPF UI
├── ViewModels/
│   ├── ShellViewModel.cs           # Existing
│   ├── ProjectsWorkspaceViewModel.cs   # NEW
│   ├── ClientsWorkspaceViewModel.cs    # NEW
│   ├── ContractsWorkspaceViewModel.cs  # NEW
│   ├── UserProfileViewModel.cs         # NEW
│   ├── DashboardViewModel.cs           # NEW
│   └── ReportViewModel.cs              # NEW
├── Views/
│   ├── Projects/
│   │   ├── ProjectListView.xaml        # NEW
│   │   ├── ProjectDetailView.xaml      # NEW
│   │   └── ProjectEditView.xaml        # NEW
│   ├── Clients/
│   │   ├── ClientListView.xaml         # NEW
│   │   ├── ClientDetailView.xaml       # NEW
│   │   └── ClientEditView.xaml         # NEW
│   ├── Contracts/
│   │   ├── ContractListView.xaml       # NEW
│   │   ├── ContractDetailView.xaml     # NEW
│   │   └── ContractEditView.xaml       # NEW
│   ├── Dashboard/
│   │   └── DashboardView.xaml          # NEW
│   ├── Reports/
│   │   └── ReportView.xaml             # NEW
│   └── Profile/
│       └── UserProfileView.xaml        # NEW
```

**Structure Decision**: The existing multi-project Clean Architecture layout is extended with new modules in the appropriate layers. The Planova.Domain project gets new entity classes. Planova.Application gets service interfaces and DTOs. Planova.Persistence gets entity configurations and repository implementations. Planova.UI gets new ViewModels and Views organized by feature (Projects, Clients, Contracts, Dashboard, Reports, Profile).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Repository pattern | Clean Architecture boundary: Application layer must not depend on EF Core. Repositories abstract persistence behind interfaces defined in Application layer. | Direct DbSet access from Application would couple use cases to EF Core, violating the Dependency Inversion Principle of Clean Architecture. |
| Value object for ProjectStatus | Encapsulates seven-status lifecycle with transition validation rules. Prevents invalid status strings from entering the system. | Plain string + enum would scatter transition validation across all call sites and allow invalid transitions. |

## Post-Design Constitution Check

*Re-check after Phase 1 design artifacts generated.*

**Check I — Architecture First**: Design follows Clean Architecture. Domain has Entities + ValueObjects (no framework dependencies). Application has service interfaces and DTOs (no persistence dependencies). Persistence has EF configurations and repository implementations. No violations.

**Check II — MVVM & Fluent UI**: All new UI follows MVVM with CommunityToolkit.Mvvm source generators. Views remain declarative. No violations.

**Check III — Modular Domain Design**: PM entities are in the Domain project, which acts as Core module. Later Studio modules can reference Domain entities without depending on UI or persistence. No violations.

**Check IV — Build vs Buy Strategy**: No commodity infrastructure being built. Report preview uses standard WPF controls with potential QuestPDF export (constitution-approved). No violations.

**Check V — Automation Platform Agnostic**: No automation dependencies introduced. No violations.

**Check VI — AI Provider Agnostic**: No AI integration. No violations.

**Check VII — Multilingual First**: Localization strategy extends existing RESX-based approach. English and Arabic both planned. No violations.

**Check VIII — Performance & Scalability**: Async operations with CancellationToken mandated. Efficient LINQ queries for 1,000-record scale. No UI thread blocking. No violations.

**Result**: ALL GATES REMAIN PASSED. Design is constitution-compliant.
