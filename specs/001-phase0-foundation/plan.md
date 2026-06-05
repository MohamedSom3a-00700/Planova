# Implementation Plan: Planova Phase 0 Foundation

**Branch**: `001-phase0-foundation` | **Date**: 2026-06-01 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/001-phase0-foundation/spec.md`

## Summary

Phase 0 establishes the foundational architecture for Planova: a WPF desktop application shell with navigation rail and multi-tab workspace, dark/light theme system, English/Arabic localization with RTL support, Serilog structured logging, user settings persistence, and SQLite/EF Core database baseline. The implementation follows Clean Architecture and MVVM patterns as mandated by the Planova Constitution.

## Technical Context

**Language/Version**: .NET 8 (C#)

**Primary Dependencies**: 
- WPF + Fluent UI WPF + CommunityToolkit.Mvvm
- Microsoft.Extensions.Hosting
- Serilog
- SQLite + EF Core
- ClosedXML / EPPlus (Excel, deferred to later phases)

**Storage**: SQLite via EF Core (Code First Migrations)

**Testing**: Standard .NET test projects (xUnit or MSTest), to be determined per task breakdown

**Target Platform**: Windows 10/11 (desktop)

**Project Type**: Desktop application (WPF)

**Performance Goals**: Application launches within 5 seconds; theme/language switch completes within 2 seconds

**Constraints**: Clean Architecture layered dependency rules; MVVM separation of presentation and business logic; no workflow engine, automation designer, or AI provider coupling

**Scale/Scope**: Single-user desktop application; Phase 0 covers only foundation — no business feature modules

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Architecture First (Clean Architecture) | PASS | FR-013 enforces layered dependency rules; FR-014 enforces presentation/business logic separation |
| MVVM & Fluent UI Enforcement | PASS | FR-014 aligns with MVVM; spec avoids technology names intentionally but constitution mandates WPF+Fluent UI |
| Modular Domain Design | PASS | Phase 0 does not create modules; foundation leaves room for future modules |
| Build vs Buy Strategy | PASS | SC-008 prohibits workflow engine/automation designer |
| Automation Platform Agnostic | PASS | No automation features in scope |
| AI Provider Agnostic | PASS | No AI features in scope |
| Multilingual First | PASS | FR-005 to FR-008 cover English + Arabic, runtime switching, RTL |
| Performance & Scalability | PASS | Performance goals defined; async/await standard practice |

**No violations found.** Complexity tracking not required.

## Project Structure

### Documentation (this feature)

```text
specs/001-phase0-foundation/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
Planova.sln
├── Planova.UI/               # WPF shell, views, view models
│   ├── Views/
│   ├── ViewModels/
│   ├── Styles/
│   └── App.xaml
├── Planova.Application/      # Application services, use cases
│   ├── Services/
│   └── Interfaces/
├── Planova.Domain/           # Domain entities, value objects
│   ├── Entities/
│   └── Interfaces/
├── Planova.Infrastructure/   # External concerns: logging, file I/O
│   ├── Logging/
│   └── Configuration/
├── Planova.Persistence/      # EF Core, SQLite, migrations
│   ├── Migrations/
│   └── DbContext/
├── Planova.Localization/     # Resource files (EN/AR)
│   ├── Resources/
│   └── Services/
└── Planova.Shared/           # Shared abstractions, contracts
    └── Abstractions/
```

**Structure Decision**: Multi-project .NET solution adhering to Clean Architecture layers. Each layer is a separate project to enforce dependency direction. Projects mirror the constitution-mandated architecture.

## Complexity Tracking

> Not required — Constitution Check passed with no violations.
