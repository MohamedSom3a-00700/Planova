# Implementation Plan: Cost Studio — Cost Management System

**Branch**: `009-cost-studio` | **Date**: 2026-06-06 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/009-cost-studio/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Cost Studio (Phase 7) adds cost management capabilities to Planova, including automatic cost loading from Resource Studio assignments, direct cost management (permits, insurance, overhead), budget management with revisions and contingency, cash flow forecasting with S-Curve visualization, Earned Value Management (EVM) with CPI/SPI metrics, actual cost entry with Excel import, AI-powered cost estimation/anomaly detection/forecasting via Semantic Kernel, and cost reporting (Cost Breakdown, Cash Flow, EVM, Budget Summary) with Excel/PDF export. The module follows the existing modular Clean Architecture pattern established by BOQ, WBS, Activity, and Resource studios.

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: WPF, Fluent UI WPF, CommunityToolkit.Mvvm, EF Core 8, SQLite, Semantic Kernel, LiveCharts2, ClosedXML, QuestPDF, Microsoft.Extensions.Hosting, Serilog

**Storage**: SQLite via EF Core (Code First Migrations) — single PlanovaDbContext with all entities

**Testing**: xUnit (following existing test project pattern: Planova.Cost.Tests targeting net8.0)

**Target Platform**: Windows (WPF desktop application)

**Project Type**: Desktop Application (WPF WinExe) — Module library (Class Library for Planova.Cost)

**Performance Goals**: Cost breakdown tree <3s (500 activities); direct cost add <30s; actual cost import <10s (1000 rows); cash flow toggle <2s (2yr project); EVM compute <3s; AI services <15s; report gen <10s; export <15s

**Constraints**: Async by default; CancellationToken support; no UI thread blocking; background processing for AI and report generation; RTL layout support for Arabic; offline-capable (AI requires network); import soft limit 5000 rows / hard cap 10000 rows

**Scale/Scope**: Up to 100 budget revisions, 500 direct cost items, 500 actual cost records per project; Medium enterprise desktop application (~50 screens across all modules)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Architecture First** | ✅ PASS | Clean Architecture with Planova.Cost module (Domain → Application layers), UI in Planova.UI, persistence in Planova.Persistence |
| **II. MVVM & Fluent UI** | ✅ PASS | WPF + Fluent UI WPF + CommunityToolkit.Mvvm; ViewModels in UI/ViewModels/Cost, Views in UI/Views/Cost |
| **III. Modular Domain** | ✅ PASS | New Planova.Cost module with clear contracts; depends on Planova.Domain + Planova.Shared + Planova.Activity + Planova.Resource |
| **IV. Build vs Buy** | ✅ PASS | Cost management is proprietary project controls value; no commodity infrastructure being built |
| **V. Automation Agnostic** | ✅ PASS | No workflow engine or automation designer; AI cost services use abstracted provider |
| **VI. AI Agnostic** | ✅ PASS | AI cost services via Semantic Kernel with abstraction layer; supports Ollama, OpenAI, Claude, Gemini |
| **VII. Multilingual First** | ✅ PASS | Cost Studio UI + reports must support EN/AR with RTL; new resx files in Planova.Localization |
| **VIII. Performance** | ✅ PASS | Async everywhere, CancellationToken, background AI/report processing, efficient queries |
| **Tech Standards** | ✅ PASS | All technology choices match approved stack (no deviations) |

**No violations — all gates pass without justification needed.**

**Post-Phase 1 Re-check**: All gates still pass. The design (data-model.md, contracts/) remains fully compliant with Clean Architecture (Domain → Application layer separation, no outward dependencies), MVVM (ViewModels in UI project, no code-behind logic), Modular Domain (Planova.Cost as separate project with clear contracts), AI Agnostic (Semantic Kernel abstraction), Multilingual First (resx files defined), and Performance (async operations, efficient queries, background processing).

## Project Structure

### Documentation (this feature)

```text
specs/009-cost-studio/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

New module **Planova.Cost** following the established pattern from Planova.Activity, Planova.Wbs, Planova.Boq, and Planova.Resource:

```text
Planova.Cost/
├── Domain/
│   ├── Entities/
│   │   ├── Budget.cs
│   │   ├── BudgetRevision.cs
│   │   ├── DirectCost.cs
│   │   ├── CostBaseline.cs
│   │   ├── ActualCost.cs
│   │   └── CashFlowPeriod.cs
│   ├── Enums/
│   │   ├── BudgetRevisionType.cs
│   │   ├── BudgetRevisionStatus.cs
│   │   ├── DirectCostCategory.cs
│   │   └── DirectCostScope.cs
│   └── Interfaces/
│       ├── IBudgetRepository.cs
│       ├── IBudgetRevisionRepository.cs
│       ├── IDirectCostRepository.cs
│       ├── ICostBaselineRepository.cs
│       ├── IActualCostRepository.cs
│       ├── ICostService.cs
│       ├── IDirectCostService.cs
│       ├── IActualCostService.cs
│       ├── ICashFlowService.cs
│       ├── IEvmService.cs
│       ├── ICostAiService.cs
│       └── ICostReportService.cs
├── Application/
│   ├── Services/
│   │   ├── CostService.cs
│   │   ├── DirectCostService.cs
│   │   ├── ActualCostService.cs
│   │   ├── CashFlowService.cs
│   │   ├── EvmService.cs
│   │   ├── CostAiService.cs
│   │   └── CostReportService.cs
│   ├── Dto/
│   │   ├── BudgetDto.cs
│   │   ├── BudgetRevisionDto.cs
│   │   ├── DirectCostDto.cs
│   │   ├── CostBaselineDto.cs
│   │   ├── ActualCostDto.cs
│   │   ├── CashFlowPeriodDto.cs
│   │   ├── EvmMetricsDto.cs
│   │   ├── AiSuggestionDto.cs
│   │   └── ImportResultDto.cs
│   └── Mappings/
│       └── CostMappingProfile.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Planova.Cost.csproj

tests/
└── Planova.Cost.Tests/
    ├── Domain/
    ├── Application/
    └── Planova.Cost.Tests.csproj

Planova.Persistence/
├── EntityConfigurations/
│   ├── BudgetConfiguration.cs
│   ├── BudgetRevisionConfiguration.cs
│   ├── DirectCostConfiguration.cs
│   ├── CostBaselineConfiguration.cs
│   ├── ActualCostConfiguration.cs
│   └── CashFlowPeriodConfiguration.cs
└── Repositories/
    ├── BudgetRepository.cs
    ├── BudgetRevisionRepository.cs
    ├── DirectCostRepository.cs
    ├── CostBaselineRepository.cs
    └── ActualCostRepository.cs

Planova.UI/
├── ViewModels/
│   └── Cost/
│       ├── CostStudioViewModel.cs
│       ├── CostBreakdownViewModel.cs
│       ├── DirectCostManagerViewModel.cs
│       ├── BudgetViewModel.cs
│       ├── BudgetRevisionViewModel.cs
│       ├── ActualCostViewModel.cs
│       ├── CashFlowViewModel.cs
│       ├── EvmViewModel.cs
│       ├── CostAiViewModel.cs
│       └── CostReportViewModel.cs
├── Views/
│   └── Cost/
│       ├── CostStudioView.xaml
│       ├── CostBreakdownView.xaml
│       ├── DirectCostManagerView.xaml
│       ├── BudgetView.xaml
│       ├── BudgetRevisionView.xaml
│       ├── ActualCostView.xaml
│       ├── CashFlowView.xaml
│       ├── EvmView.xaml
│       ├── CostAiView.xaml
│       └── CostReportView.xaml
└── Converters/
    └── CostValueConverter.cs

Planova.Localization/
└── Resources/
    ├── CostResources.en.resx
    └── CostResources.ar.resx

Planova.Excel/
├── Readers/
│   └── ActualCostImportReader.cs
├── Writers/
│   └── CostReportWriter.cs
└── Services/
    └── CostImportService.cs
```

**Structure Decision**: Module library (Planova.Cost) following the exact pattern of existing modules (Planova.Boq, Planova.Wbs, Planova.Activity, Planova.Resource). Domain layer contains entities, enums, and interface contracts. Application layer contains services and DTOs with mapping profiles. Persistence configuration and repository implementations live in Planova.Persistence. UI views/ViewModels live in Planova.UI under a `Cost/` subdirectory. Localized resources in Planova.Localization. Excel import/export in Planova.Excel. Tests in a dedicated test project.

This module depends on Planova.Domain, Planova.Shared, Planova.Activity (for activity entity references in cost assignments), and Planova.Resource (for resource assignment costs).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
