# Implementation Plan: Resource Studio

**Branch**: `008-resource-studio` | **Date**: 2026-06-05 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/008-resource-studio/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Resource Studio (Phase 6) adds labour, equipment, material, and subcontractor resource management to Planova. It enables resource library management with type-specific fields, effective-dated rate management, reusable crew templates with blended rate computation, resource loading onto Phase 5 activities with auto-calculated costs, a resource histogram for visualizing daily usage, AI-powered resource estimation via Semantic Kernel, and resource usage/cost reporting. The module follows the existing modular Clean Architecture pattern established by BOQ, WBS, and Activity studios.

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: WPF, Fluent UI WPF, CommunityToolkit.Mvvm, EF Core 8, SQLite, Semantic Kernel, LiveCharts2, ClosedXML, QuestPDF, Microsoft.Extensions.Hosting, Serilog

**Storage**: SQLite via EF Core (Code First Migrations) — single PlanovaDbContext with all entities

**Testing**: xUnit (following existing test project pattern: Planova.Resource.Tests targeting net8.0)

**Target Platform**: Windows (WPF desktop application)

**Project Type**: Desktop Application (WPF WinExe) — Module library (Class Library for Planova.Resource)

**Performance Goals**: Resource library browsing <10s (200-1,000 resources); blended rate calc <3s (6+ crew members); total cost computation <2s; histogram render <5s (500+ assignments); AI estimation <30s; report generation/export <10s (500 assignments)

**Constraints**: Async by default; CancellationToken support; no UI thread blocking; background processing for AI and report generation; large dataset virtualization for histogram; RTL layout support for Arabic; offline-capable (AI requires network)

**Scale/Scope**: 200-1,000 resources per project; 500+ resource assignments; Medium enterprise desktop application (~50 screens across all modules)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Architecture First** | ✅ PASS | Clean Architecture with Planova.Resource module (Domain → Application layers), UI in Planova.UI, persistence in Planova.Persistence |
| **II. MVVM & Fluent UI** | ✅ PASS | WPF + Fluent UI WPF + CommunityToolkit.Mvvm; ViewModels in UI/ViewModels/Resource, Views in UI/Views/Resource |
| **III. Modular Domain** | ✅ PASS | New Planova.Resource module with clear contracts, depends on Planova.Domain + Planova.Shared + Planova.Activity (for activity assignments) |
| **IV. Build vs Buy** | ✅ PASS | Resource management is proprietary project controls value; no commodity infrastructure being built |
| **V. Automation Agnostic** | ✅ PASS | No workflow engine or automation designer; AI estimation uses abstracted provider |
| **VI. AI Agnostic** | ✅ PASS | AI estimation via Semantic Kernel with abstraction layer; supports Ollama, OpenAI, Claude, Gemini |
| **VII. Multilingual First** | ✅ PASS | Resource Studio UI + reports must support EN/AR with RTL; new resx files in Planova.Localization |
| **VIII. Performance** | ✅ PASS | Async everywhere, CancellationToken, background AI/report processing, histogram virtualization |
| **Tech Standards** | ✅ PASS | All technology choices match approved stack (no deviations) |

**No violations — all gates pass without justification needed.**

**Post-Phase 1 Re-check**: All gates still pass. The design (data-model.md, contracts/) remains fully compliant with Clean Architecture (Domain → Application layer separation, no outward dependencies), MVVM (ViewModels in UI project, no code-behind logic), Modular Domain (Planova.Resource as separate project with clear contracts), AI Agnostic (Semantic Kernel abstraction), Multilingual First (resx files defined), and Performance (async operations, pre-aggregated histogram data).

## Project Structure

### Documentation (this feature)

```text
specs/008-resource-studio/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

New module **Planova.Resource** following the established pattern from Planova.Activity, Planova.Wbs, and Planova.Boq:

```text
Planova.Resource/
├── Domain/
│   ├── Entities/
│   │   ├── Resource.cs
│   │   ├── ResourceRate.cs
│   │   ├── Crew.cs
│   │   ├── CrewResource.cs
│   │   ├── ResourceAssignment.cs
│   │   └── ResourceUsage.cs
│   ├── Enums/
│   │   ├── ResourceType.cs
│   │   ├── ResourceScope.cs
│   │   ├── ResourceStatus.cs
│   │   ├── UnitOfMeasure.cs
│   │   └── CrewStatus.cs
│   └── Interfaces/
│       ├── IResourceRepository.cs
│       ├── IResourceRateRepository.cs
│       ├── ICrewRepository.cs
│       ├── ICrewResourceRepository.cs
│       ├── IResourceAssignmentRepository.cs
│       ├── IResourceUsageRepository.cs
│       ├── IResourceService.cs
│       ├── ICrewService.cs
│       ├── IResourceAssignmentService.cs
│       ├── IResourceHistogramService.cs
│       ├── IResourceAiEstimationService.cs
│       ├── IResourceReportService.cs
│       └── IResourceImportService.cs
├── Application/
│   ├── Services/
│   │   ├── ResourceService.cs
│   │   ├── CrewService.cs
│   │   ├── ResourceAssignmentService.cs
│   │   ├── ResourceHistogramService.cs
│   │   ├── ResourceAiEstimationService.cs
│   │   ├── ResourceRateService.cs
│   │   ├── ResourceReportService.cs
│   │   └── ResourceImportService.cs
│   ├── Dto/
│   │   ├── ResourceDto.cs
│   │   ├── ResourceRateDto.cs
│   │   ├── CrewDto.cs
│   │   ├── CrewResourceDto.cs
│   │   ├── ResourceAssignmentDto.cs
│   │   ├── ResourceHistogramDto.cs
│   │   ├── AiSuggestionDto.cs
│   │   ├── ResourceReportDto.cs
│   │   └── ImportResultDto.cs
│   └── Mappings/
│       └── ResourceMappingProfile.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Planova.Resource.csproj

tests/
└── Planova.Resource.Tests/
    ├── Domain/
    ├── Application/
    └── Planova.Resource.Tests.csproj

Planova.Persistence/
├── EntityConfigurations/
│   ├── ResourceConfiguration.cs
│   ├── ResourceRateConfiguration.cs
│   ├── CrewConfiguration.cs
│   ├── CrewResourceConfiguration.cs
│   ├── ResourceAssignmentConfiguration.cs
│   └── ResourceUsageConfiguration.cs
└── Repositories/
    ├── ResourceRepository.cs
    ├── ResourceRateRepository.cs
    ├── CrewRepository.cs
    ├── CrewResourceRepository.cs
    ├── ResourceAssignmentRepository.cs
    └── ResourceUsageRepository.cs

Planova.UI/
├── ViewModels/
│   └── Resource/
│       ├── ResourceStudioViewModel.cs
│       ├── ResourceLibraryViewModel.cs
│       ├── ResourceEditorViewModel.cs
│       ├── ResourceRateManagerViewModel.cs
│       ├── CrewTemplateManagerViewModel.cs
│       ├── CrewTemplateEditorViewModel.cs
│       ├── ResourceAssignmentViewModel.cs
│       ├── ResourceHistogramViewModel.cs
│       ├── ResourceAiEstimationViewModel.cs
│       └── ResourceReportViewModel.cs
├── Views/
│   └── Resource/
│       ├── ResourceStudioView.xaml
│       ├── ResourceLibraryView.xaml
│       ├── ResourceEditorView.xaml
│       ├── ResourceRateManagerView.xaml
│       ├── CrewTemplateManagerView.xaml
│       ├── CrewTemplateEditorView.xaml
│       ├── ResourceAssignmentView.xaml
│       ├── ResourceHistogramView.xaml
│       ├── ResourceAiEstimationView.xaml
│       └── ResourceReportView.xaml
└── Converters/
    └── ResourceTypeConverter.cs

Planova.Localization/
└── Resources/
    ├── ResourceResources.en.resx
    └── ResourceResources.ar.resx

Planova.Excel/
├── Readers/
│   └── ResourceImportReader.cs
├── Writers/
│   └── ResourceReportWriter.cs
└── Services/
    └── ResourceImportService.cs (if module-specific)
```

**Structure Decision**: Module library (Planova.Resource) following the exact pattern of existing modules (Planova.Boq, Planova.Wbs, Planova.Activity). Domain layer contains entities, enums, and interface contracts. Application layer contains services and DTOs with mapping profiles. Persistence configuration and repository implementations live in Planova.Persistence. UI views/ViewModels live in Planova.UI under a `Resource/` subdirectory. Localized resources in Planova.Localization. Excel import/export in Planova.Excel. Tests in a dedicated test project.

This module depends on Planova.Domain, Planova.Shared, and Planova.Activity (for activity entity references in resource assignments).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
