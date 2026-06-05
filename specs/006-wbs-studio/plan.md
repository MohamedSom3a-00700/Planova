# Implementation Plan: WBS Studio

**Branch**: `006-wbs-studio` | **Date**: 2026-06-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/006-wbs-studio/spec.md`

## Summary

Implement the WBS Studio module — a hierarchical Work Breakdown Structure workspace for project planners. Users create WBS structures manually, from BOQ data (Phase 3), from templates, or via AI generation. They view/edit as a virtualized tree with expand/collapse, level color-coding, and weight visualizations. The module includes a BOQ-to-WBS mapping wizard (3 strategies), a template manager (CRUD + standard seeded templates), AI-assisted generation via Semantic Kernel/Ollama, and WBS report export (Excel via Phase 2 WorkbookWriter, PDF via QuestPDF). Built on .NET 8 + WPF, following Clean Architecture and MVVM.

## Technical Context

**Language/Version**: .NET 8, C# 12

**Primary Dependencies**: CommunityToolkit.Mvvm, Serilog, Microsoft.Extensions.DependencyInjection, Semantic Kernel, ClosedXML (Phase 2), EPPlus (Phase 2), QuestPDF

**Storage**: SQLite with EF Core. New entities: Wbs, WbsItem, WbsTemplate, WbsTemplateItem. Existing Boq/BoqItem tables consumed (read-only) for BOQ mapping.

**Testing**: xUnit — unit tests for domain entities, validation engine, application services, BOQ mapping strategies, AI generation service (mock provider); integration tests for persistence round-trip, template apply

**Target Platform**: Windows (WPF Desktop)

**Project Type**: Desktop application module (new "WBS Studio" module per Modular Domain Design)

**Performance Goals**: Virtualized tree render 1,000+ items < 2s, expand/collapse < 200ms, BOQ-to-WBS mapping for 200 items < 5s commit, AI generation < 30s, weight redistribution < 500ms, report generation for 500 items < 5s

**Constraints**: Clean Architecture, MVVM, async-first, CancellationToken everywhere, RTL support, virtualized tree view, AI provider abstraction via IAIProvider, self-referencing tree with ParentId, nullable SourceBoqItemId traceability

**Scale/Scope**: Single-user desktop, up to 5,000+ items per WBS, 4 standard seeded templates, 3 mapping strategies, AI fallback to manual

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Compliance | Notes |
|-----------|-----------|-------|
| I. Architecture First | ✅ PASS | Clean Architecture layers respected; Planova.Wbs as Domain + Application, Planova.UI for Views/ViewModels, Planova.Persistence for EF configs |
| II. MVVM & Fluent UI | ✅ PASS | WPF + MVVM via CommunityToolkit.Mvvm; navigation rail integration, multi-tab workspace, virtualized tree view |
| III. Modular Domain | ✅ PASS | Fits under "WBS Studio" module with clear Wbs, WbsItem, WbsTemplate, WbsTemplateItem entities and application contracts |
| IV. Build vs Buy | ✅ PASS | QuestPDF for PDF generation (approved); Semantic Kernel for AI orchestration (approved); no custom engine built |
| V. Automation Agnostic | ✅ PASS | N/A — no automation workflows in scope |
| VI. AI Provider Agnostic | ✅ PASS | AI abstracted via IAIProvider per constitution; Semantic Kernel with Ollama default (Llama 3.2); provider swappable via config |
| VII. Multilingual First | ✅ PASS | English + Arabic with runtime switching per FR-026; localized resource strings for all WBS screens; RTL verification for tree views |
| VIII. Performance & Scalability | ✅ PASS | Async-first, CancellationToken, virtualized tree view, background AI generation and report processing, efficient EF queries with indexes on ParentId/WbsId/SourceBoqItemId |

All gates pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/006-wbs-studio/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Planova.Wbs/                                         # NEW — Domain + Application
├── Domain/
│   ├── Entities/
│   │   ├── Wbs.cs
│   │   ├── WbsItem.cs
│   │   ├── WbsTemplate.cs
│   │   └── WbsTemplateItem.cs
│   ├── Enums/
│   │   ├── WbsStatus.cs
│   │   ├── WbsLevelType.cs
│   │   └── WbsSource.cs
│   └── Interfaces/
│       ├── IWbsRepository.cs
│       ├── IWbsItemRepository.cs
│       ├── IWbsTemplateRepository.cs
│       ├── IWbsValidationService.cs
│       ├── IWbsBoqMappingService.cs
│       ├── IWbsAiGenerationService.cs
│       └── IWbsReportService.cs
├── Application/
│   ├── Services/
│   │   ├── WbsService.cs
│   │   ├── WbsItemService.cs
│   │   ├── WbsValidationService.cs
│   │   ├── WbsBoqMappingService.cs
│   │   ├── WbsTemplateService.cs
│   │   ├── WbsAiGenerationService.cs
│   │   └── WbsReportService.cs
│   └── Dto/
│       ├── WbsDto.cs
│       ├── WbsItemDto.cs
│       ├── WbsTreeDto.cs
│       ├── WbsMappingRequest.cs
│       ├── WbsMappingResult.cs
│       └── WbsReportDto.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Planova.Wbs.csproj

Planova.UI/
├── Views/Wbs/                                      # NEW
│   ├── WbsListView.xaml
│   ├── WbsTreeView.xaml
│   ├── WbsEditorView.xaml
│   ├── WbsMappingWizardView.xaml
│   ├── WbsTemplateManagerView.xaml
│   ├── WbsAiGenerationView.xaml
│   └── WbsReportView.xaml
└── ViewModels/Wbs/                                 # NEW
    ├── WbsListViewModel.cs
    ├── WbsTreeViewModel.cs
    ├── WbsEditorViewModel.cs
    ├── WbsMappingViewModel.cs
    ├── WbsTemplateManagerViewModel.cs
    ├── WbsAiGenerationViewModel.cs
    └── WbsReportViewModel.cs

Planova.Persistence/
└── EntityConfigurations/                           # NEW
    ├── WbsConfiguration.cs
    ├── WbsItemConfiguration.cs
    ├── WbsTemplateConfiguration.cs
    └── WbsTemplateItemConfiguration.cs

Planova.Localization/
└── Resources/                                       # NEW entries
    ├── WbsResources.en.resx
    └── WbsResources.ar.resx

tests/
├── Planova.Wbs.Tests/                              # NEW
│   ├── Domain/
│   ├── Application/
│   ├── Services/
│   └── Planova.Wbs.Tests.csproj
└── Planova.UI.Tests/
    └── ViewModels/Wbs/                             # NEW
        ├── WbsTreeViewModelTests.cs
        ├── WbsEditorViewModelTests.cs
        ├── WbsMappingViewModelTests.cs
        └── WbsAiGenerationViewModelTests.cs
```

**Structure Decision**: New `Planova.Wbs` project following Clean Architecture with its own Domain (entities, enums, interfaces) and Application (services, DTOs) layers. UI components live in existing `Planova.UI` under `Views/Wbs/` and `ViewModels/Wbs/`. EF Core configurations live in existing `Planova.Persistence`. Localization resources added to existing `Planova.Localization`. This follows the same pattern as Phase 3 BOQ Studio. Tests in a new `Planova.Wbs.Tests` project plus additions to existing `Planova.UI.Tests`.

## Complexity Tracking

All gates pass. No complexity justification needed.
