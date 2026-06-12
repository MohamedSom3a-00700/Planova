# Implementation Plan: Reporting Center

**Branch**: `010-reporting-center` | **Date**: 2026-06-12 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/010-reporting-center/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Reporting Center (Phase 8) adds a unified cross-studio report orchestration hub to Planova, compositing data from Activity, Resource, Cost, WBS, and Project services into four consolidated report types (Daily, Weekly, Monthly, Executive). It provides an in-process scheduled report generation engine, AI narrative generation via Semantic Kernel for all four report types, three-format export (Excel via ClosedXML, PDF via QuestPDF, Word via DocumentFormat.OpenXml), report history and template management, and project parties management (Client, Main Contractor, Sub Contractors with logos). The module follows the established modular Clean Architecture and MVVM patterns from prior studios.

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: WPF, Fluent UI WPF, CommunityToolkit.Mvvm, EF Core 8, SQLite, Semantic Kernel, LiveCharts2, ClosedXML, QuestPDF, DocumentFormat.OpenXml, Microsoft.Extensions.Hosting, Serilog

**Storage**: SQLite via EF Core (Code First Migrations) — single PlanovaDbContext with new entity tables for ReportTemplates, ReportInstances, ReportSections, ReportSchedules, ReportExports, ReportSettings, ProjectParties; logo files stored on disk under `{AppData}/Planova/Projects/{projectId}/Parties/`

**Testing**: xUnit (following existing test project pattern: Planova.Reporting.Tests targeting net8.0)

**Target Platform**: Windows (WPF desktop application)

**Project Type**: Desktop Application (WPF WinExe) — Module library (Class Library for Planova.Reporting)

**Performance Goals**: Report tab load <5s (500 activities); AI narrative gen <15s; export <15s; schedule creation <1min; scheduled gen within 2min of configured time; History grid <3s (200 reports); section visibility config <30s; project parties config <1min

**Constraints**: Async by default; CancellationToken support; no UI thread blocking; background processing for scheduled generation and AI; RTL layout support for Arabic; report data must be snapshotted at generation time; in-process scheduler (app must be running); all studio data consumed through existing service interfaces

**Scale/Scope**: Up to 500 report instances, 100 schedules, 200 export records, 20 project parties per project; Medium enterprise desktop application (~60 screens across all modules)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Architecture First** | ✅ PASS | Clean Architecture with Planova.Reporting module (Domain → Application layers), UI in Planova.UI, persistence in Planova.Persistence |
| **II. MVVM & Fluent UI** | ✅ PASS | WPF + Fluent UI WPF + CommunityToolkit.Mvvm; ViewModels in UI/ViewModels/Reporting, Views in UI/Views/Reporting |
| **III. Modular Domain** | ✅ PASS | New Planova.Reporting module with clear contracts; depends on existing studio service interfaces (Activity, Resource, Cost, WBS, Project) and shared abstractions (IAIProvider) |
| **IV. Build vs Buy** | ✅ PASS | QuestPDF (approved), ClosedXML (approved), DocumentFormat.OpenXml (standard SDK) used for export. Template editor is section reorder/visibility configuration, NOT a reporting designer (which is explicitly excluded) |
| **V. Automation Agnostic** | ✅ PASS | Scheduled report generation is in-process timer, not a workflow engine; no automation designer built |
| **VI. AI Agnostic** | ✅ PASS | AI narrative via IAIProvider abstraction (Semantic Kernel); supports Ollama, OpenAI, Claude, Gemini |
| **VII. Multilingual First** | ✅ PASS | Reporting Center UI + reports must support EN/AR with RTL; new resx files in Planova.Localization |
| **VIII. Performance** | ✅ PASS | Async everywhere, CancellationToken, background scheduled generation, efficient queries, data snapshots avoid live-query lock contention |
| **Tech Standards** | ✅ PASS | All technology choices match approved stack; repository pattern used (justified in Complexity Tracking) |

**Post-Phase 1 Re-check**: All gates still pass. The design (data-model.md, contracts/) remains fully compliant with Clean Architecture (Domain → Application layer separation, no outward dependencies), MVVM (ViewModels in UI project, no code-behind logic), Modular Domain (Planova.Reporting as separate project with clear contracts), AI Agnostic (Semantic Kernel abstraction via IAIProvider), Multilingual First (resx files for EN/AR), and Performance (async operations, background processing, data snapshots).

## Project Structure

### Documentation (this feature)

```text
specs/010-reporting-center/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

New module **Planova.Reporting** following the established pattern from Planova.Cost, Planova.Activity, and other modules:

```text
Planova.Reporting/
├── Domain/
│   ├── Entities/
│   │   ├── ReportTemplate.cs
│   │   ├── ReportInstance.cs
│   │   ├── ReportSection.cs
│   │   ├── ReportSchedule.cs
│   │   ├── ReportExport.cs
│   │   ├── ReportSettings.cs
│   │   └── ProjectParty.cs
│   ├── Enums/
│   │   ├── ReportType.cs
│   │   ├── ReportStatus.cs
│   │   ├── ReportSectionType.cs
│   │   ├── ScheduleFrequency.cs
│   │   ├── ExportFormat.cs
│   │   └── PartyRole.cs
│   ├── Interfaces/
│   │   ├── IReportTemplateRepository.cs
│   │   ├── IReportInstanceRepository.cs
│   │   ├── IReportScheduleRepository.cs
│   │   ├── IProjectPartyRepository.cs
│   │   ├── IReportEngine.cs
│   │   ├── IReportDataProvider.cs
│   │   ├── IReportSchedulerService.cs
│   │   ├── IReportExportService.cs
│   │   ├── IReportAiService.cs
│   │   ├── IProjectPartyService.cs
│   │   └── IReportSettingsService.cs
│   └── Constants/
│       └── ReportSectionIds.cs
├── Application/
│   ├── Services/
│   │   ├── ReportEngine.cs
│   │   ├── ReportSchedulerService.cs
│   │   ├── ReportExportService.cs
│   │   ├── ReportAiService.cs
│   │   ├── ProjectPartyService.cs
│   │   └── ReportSettingsService.cs
│   ├── DataProviders/
│   │   ├── DailyReportDataProvider.cs
│   │   ├── WeeklyReportDataProvider.cs
│   │   ├── MonthlyReportDataProvider.cs
│   │   └── ExecutiveReportDataProvider.cs
│   ├── Dto/
│   │   ├── ReportTemplateDto.cs
│   │   ├── ReportInstanceDto.cs
│   │   ├── ReportScheduleDto.cs
│   │   ├── ReportSectionDto.cs
│   │   ├── ReportExportDto.cs
│   │   ├── ProjectPartyDto.cs
│   │   ├── ReportSectionConfigDto.cs
│   │   ├── ReportSettingsDto.cs
│   │   └── ReportDataDto.cs
│   └── Mappings/
│       └── ReportingMappingProfile.cs
├── Background/
│   └── ReportGenerationHostedService.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Planova.Reporting.csproj

tests/
└── Planova.Reporting.Tests/
    ├── Domain/
    ├── Application/
    └── Planova.Reporting.Tests.csproj

Planova.Persistence/
├── EntityConfigurations/
│   ├── ReportTemplateConfiguration.cs
│   ├── ReportInstanceConfiguration.cs
│   ├── ReportSectionConfiguration.cs
│   ├── ReportScheduleConfiguration.cs
│   ├── ReportExportConfiguration.cs
│   ├── ReportSettingsConfiguration.cs
│   └── ProjectPartyConfiguration.cs
└── Repositories/
    ├── ReportTemplateRepository.cs
    ├── ReportInstanceRepository.cs
    ├── ReportScheduleRepository.cs
    └── ProjectPartyRepository.cs

Planova.UI/
├── ViewModels/
│   └── Reporting/
│       ├── ReportingHubViewModel.cs
│       ├── DailyReportViewModel.cs
│       ├── WeeklyReportViewModel.cs
│       ├── MonthlyReportViewModel.cs
│       ├── ExecutiveReportViewModel.cs
│       ├── ReportScheduleViewModel.cs
│       ├── ReportHistoryViewModel.cs
│       ├── ReportTemplateEditorViewModel.cs
│       ├── ReportSettingsViewModel.cs
│       └── ProjectPartyViewModel.cs
├── Views/
│   └── Reporting/
│       ├── ReportingHubView.xaml
│       ├── DailyReportView.xaml
│       ├── WeeklyReportView.xaml
│       ├── MonthlyReportView.xaml
│       ├── ExecutiveReportView.xaml
│       ├── ReportScheduleView.xaml
│       ├── ReportHistoryView.xaml
│       ├── ReportTemplateEditorView.xaml
│       └── ReportSettingsView.xaml
└── Converters/
    └── ReportStatusColorConverter.cs

Planova.Localization/
└── Resources/
    ├── ReportingResources.en.resx
    └── ReportingResources.ar.resx
```

**Structure Decision**: Module library (Planova.Reporting) following the exact pattern of existing modules. Domain layer contains entities, enums, constants, and repository interfaces. Application layer contains services, data providers, DTOs, and mapping profiles. A dedicated Background folder holds the IHostedService implementation for scheduled generation. Persistence configuration and repository implementations live in Planova.Persistence. UI views/ViewModels live in Planova.UI under a `Reporting/` subdirectory. Localized resources in Planova.Localization. Tests in a dedicated test project.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Repository pattern (4 repositories) | Required for data access abstraction per Clean Architecture domain layer; repositories encapsulate EF Core queries and keep domain independent of persistence | Direct EF Core DbContext usage from Application layer would violate Clean Architecture dependency rule (inward-pointing dependencies); prior modules (Cost, Activity, WBS, BOQ, Resource) all use the same repository pattern |
