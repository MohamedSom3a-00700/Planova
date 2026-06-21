# Implementation Plan: Overall Enhancement Plan

**Branch**: `013-overall-enhancement-plan` | **Date**: 2026-06-17 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/013-overall-enhancement-plan/spec.md`

## Summary

Deliver a comprehensive visual and functional refresh across five Planova areas: Dashboard, Projects, Parties, BOQ Studio, and WBS Studio. The work spans UI redesign (icons, Fluent UI styling, glass-effect cards), rich metadata display (health indicators, cost/performance cards, project logos), multi-sheet BOQ import with auto-detection and merge, WBS Editor with drag-and-drop and auto-code generation, BOQ-to-WBS mapping, AI-powered WBS generation via external API, and project folder auto-creation. All UI changes are XAML-only; no new modules are created — existing `Planova.Boq`, `Planova.Wbs`, `Planova.UI`, `Planova.Persistence`, and `Planova.Localization` are extended.

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: WPF, Fluent UI WPF, CommunityToolkit.Mvvm, EF Core 8, SQLite, ClosedXML, QuestPDF, Semantic Kernel, Microsoft.Extensions.Hosting, Serilog

**Storage**: SQLite via EF Core Code First — `PlanovaDbContext` extended with new entity configurations for enhanced project metadata, BOQ import tracking (SourceSheet, import summaries), WBS entities (if not already present), edit locks, and party-project relationships.

**Testing**: xUnit in existing test projects (`Planova.UI.Tests`, future `Planova.Boq.Tests`, `Planova.Wbs.Tests`), following existing module test patterns.

**Target Platform**: Windows WPF desktop application

**Project Type**: Desktop application — multi-module enhancement across existing studios

**Performance Goals**: Dashboard loads all health/cost indicators for up to 20 projects in <3s; multi-sheet BOQ import completes in <2min for 5-sheet file; WBS creation from BOQ mapping in <1min; WBS tree operations (add/move/delete node) respond in <500ms; Excel/PDF exports complete in <30s for standard datasets.

**Constraints**: Async by default with CancellationToken support; no UI thread blocking; XAML-only UI changes (no code-behind logic); Fluent UI styling; English + Arabic localization; RTL layout support; WBS edit locks for concurrency; AI API calls degrade gracefully when unreachable; built-in heuristic column mapping for BOQ (no external AI dependency for import).

**Scale/Scope**: Medium enterprise desktop application; supports up to 1000 projects, 50K BOQ items, 10K WBS nodes per project; Excel files up to 50MB / 100K rows.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Architecture First** | PASS | All enhancements extend existing Clean Architecture modules. Dashboard/Projects/Parties in `Planova.UI` (UI layer), BOQ logic in `Planova.Boq`, WBS logic in `Planova.Wbs`, persistence configs in `Planova.Persistence`. No layer-skipping. |
| **II. MVVM & Fluent UI** | PASS | WPF + Fluent UI WPF + CommunityToolkit.Mvvm. XAML-only updates as specified. ViewModels under `Planova.UI/ViewModels/*`, Views under `Planova.UI/Views/*`. Dashboard cards, project grids, party panels all follow MVVM. |
| **III. Modular Domain** | PASS | Enhances existing modules. BOQ Studio stays in `Planova.Boq`, WBS Studio stays in `Planova.Wbs`. Cross-module integration (BOQ→WBS mapping) via existing application service contracts. |
| **IV. Build vs Buy** | PASS | No workflow engine, automation designer, or reporting designer. Uses QuestPDF for reports, ClosedXML for Excel, Semantic Kernel for AI abstraction. |
| **V. Automation Agnostic** | PASS | No internal workflow engine. Exposes relevant events/APIs for external automation platforms. |
| **VI. AI Agnostic** | PASS | WBS AI generation uses Semantic Kernel abstraction with configurable provider (default Ollama). BOQ column mapping uses built-in heuristics (no external AI). |
| **VII. Multilingual First** | PASS | English + Arabic for all new screens, labels, import summaries, WBS reports, and export content. New resources added to `Planova.Localization`. |
| **VIII. Performance** | PASS | Async BOQ import with progress, virtualized WBS tree, lazy-loaded dashboard data, CancellationToken on all async operations. |

**No violations — all gates pass without justification needed.**

## Project Structure

### Documentation (this feature)

```text
specs/013-overall-enhancement-plan/
├── plan.md              # This file
├── research.md          # Phase 0 output — key decisions consolidated
├── data-model.md        # Phase 1 output — entity definitions and relationships
├── quickstart.md        # Phase 1 output — onboarding for implementers
├── contracts/           # Phase 1 output — cross-module interface contracts
│   └── ...              # Cross-studio contracts
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
Planova.Boq/                                  # [ENHANCE]
├── Domain/
│   ├── Entities/
│   │   ├── BoqImportSession.cs               # NEW: tracks import metadata
│   │   └── BoqWorksheetMapping.cs            # NEW: per-worksheet column mapping
│   ├── Enums/
│   │   ├── ImportMode.cs                     # NEW: Single/Multiple/Merge
│   │   └── BoqSheetType.cs                   # NEW: BOQ vs non-BOQ detection
│   └── Interfaces/
│       ├── IBoqImportService.cs              # NEW: multi-sheet import contract
│       ├── IBoqColumnMappingService.cs       # NEW: heuristic column mapper
│       └── IBoqExportService.cs              # ENHANCE: add Tender/Client/PDF exports
├── Application/
│   ├── Services/
│   │   ├── BoqImportService.cs               # NEW: multi-sheet import engine
│   │   ├── BoqColumnMappingService.cs        # NEW: heuristic column mapping
│   │   └── BoqExportService.cs               # ENHANCE: new export formats
│   └── Models/
│       ├── BoqImportResult.cs                # NEW: import summary model
│       └── BoqPreviewRow.cs                  # NEW: import preview row model

Planova.Wbs/                                  # [ENHANCE]
├── Domain/
│   ├── Entities/
│   │   ├── WbsEditLock.cs                    # NEW: concurrent edit lock entity
│   ├── Enums/
│   │   ├── WbsSource.cs                      # ENHANCE: per spec FR-032
│   │   ├── WbsStatus.cs                      # ENHANCE: per spec FR-033 w/ transitions
│   │   └── WbsViewMode.cs                    # NEW: Standard/Primavera/Weight/Responsibility
│   └── Interfaces/
│       ├── IWbsMappingService.cs             # NEW: BOQ-to-WBS mapping
│       ├── IWbsAiGenerationService.cs        # NEW: AI WBS generation
│       ├── IWbsCodeGenerationService.cs      # NEW: auto-code generation
│       └── IWbsLockService.cs                # NEW: edit lock management
├── Application/
│   ├── Services/
│   │   ├── WbsMappingService.cs              # NEW: mapping engine
│   │   ├── WbsAiGenerationService.cs         # NEW: AI orchestration
│   │   ├── WbsCodeGenerationService.cs       # NEW: structured code gen
│   │   └── WbsLockService.cs                 # NEW: lock management
│   ├── Dto/
│   │   ├── WbsMappingPreviewDto.cs           # NEW: mapping preview data
│   │   └── WbsAiGenerationRequestDto.cs      # NEW: AI request model
│   └── Models/
│       ├── WbsMappingResult.cs               # NEW: mapping operation result
│       └── WbsAiGenerationResult.cs          # NEW: AI generation result

Planova.Persistence/                          # [ENHANCE]
├── EntityConfigurations/
│   ├── ProjectMetadataConfiguration.cs       # NEW: enhanced project fields
│   ├── PartyConfiguration.cs                 # NEW: party entity config
│   ├── ProjectPartyConfiguration.cs          # NEW: party-project link
│   ├── BoqImportSessionConfiguration.cs      # NEW: import session config
│   ├── WbsEditLockConfiguration.cs           # NEW: edit lock config
│   └── [Existing] WbsItemConfiguration.cs    # ENHANCE: new WBS properties
└── Migrations/
    ├── [Next Migration].cs                   # NEW: schema migration

Planova.UI/                                   # [ENHANCE]
├── ViewModels/
│   ├── Dashboard/
│   │   ├── DashboardViewModel.cs             # ENHANCE: health + cost cards
│   │   ├── HealthCardViewModel.cs            # NEW
│   │   └── CostCardViewModel.cs             # NEW
│   ├── Projects/
│   │   ├── ProjectListViewModel.cs           # ENHANCE: rich metadata
│   │   └── ProjectDetailViewModel.cs         # ENHANCE: folder, docs, maps
│   ├── Parties/
│   │   ├── PartyListViewModel.cs             # NEW
│   │   └── PartyDetailViewModel.cs           # NEW
│   ├── Boq/
│   │   ├── BoqImportViewModel.cs             # NEW: multi-sheet import
│   │   ├── BoqColumnMappingViewModel.cs      # NEW: column mapping UI
│   │   ├── BoqTreeViewModel.cs               # ENHANCE: split view
│   │   └── BoqExportViewModel.cs             # ENHANCE: new formats
│   └── Wbs/
│       ├── WbsListViewModel.cs               # NEW
│       ├── WbsTreeViewModel.cs               # ENHANCE: view modes
│       ├── WbsEditorViewModel.cs             # ENHANCE: drag-drop, properties
│       ├── WbsMappingViewModel.cs            # NEW
│       ├── WbsAiGenerationViewModel.cs       # NEW
│       ├── WbsReportsViewModel.cs            # NEW
│       └── WbsSettingsViewModel.cs           # NEW
├── Views/
│   ├── Dashboard/
│   │   ├── DashboardView.xaml               # ENHANCE: glass-effect cards
│   │   ├── HealthCardView.xaml              # NEW
│   │   └── CostCardView.xaml                # NEW
│   ├── Projects/
│   │   ├── ProjectListView.xaml             # ENHANCE: rich metadata grid
│   │   └── ProjectDetailView.xaml           # ENHANCE: folder/docs/maps
│   ├── Parties/
│   │   ├── PartyListView.xaml               # NEW
│   │   └── PartyDetailView.xaml             # NEW
│   ├── Boq/
│   │   ├── BoqImportView.xaml               # NEW: multi-sheet import
│   │   ├── BoqImportPreviewView.xaml        # NEW: preview grid
│   │   ├── BoqTreeView.xaml                 # ENHANCE: split view
│   │   └── BoqExportView.xaml               # ENHANCE: new formats
│   └── Wbs/
│       ├── WbsListView.xaml                 # NEW
│       ├── WbsTreeView.xaml                 # ENHANCE: view modes
│       ├── WbsEditorView.xaml               # ENHANCE: drag-drop, properties panel
│       ├── WbsMappingView.xaml              # NEW
│       ├── WbsAiGenerationView.xaml         # NEW
│       ├── WbsReportsView.xaml              # NEW
│       └── WbsSettingsView.xaml             # NEW
├── Converters/
│   ├── HealthToColorConverter.cs            # NEW
│   └── WbsStatusToIconConverter.cs          # NEW
└── Styles/
    ├── FluentCardStyles.xaml                # NEW: glass-effect card styles
    └── DashboardStyles.xaml                 # NEW: dashboard-specific styles

Planova.Localization/                         # [ENHANCE]
└── Resources/
    ├── DashboardResources.en.resx           # NEW
    ├── DashboardResources.ar.resx           # NEW
    ├── PartyResources.en.resx               # NEW
    ├── PartyResources.ar.resx               # NEW
    ├── BoqImportResources.en.resx           # NEW
    ├── BoqImportResources.ar.resx           # NEW
    ├── WbsResources.en.resx                 # NEW
    └── WbsResources.ar.resx                 # NEW

tests/
├── Planova.Boq.Tests/                        # NEW (if not exists)
│   ├── BoqImportServiceTests.cs
│   ├── BoqColumnMappingServiceTests.cs
│   └── BoqExportServiceTests.cs
├── Planova.Wbs.Tests/                        # NEW (if not exists)
│   ├── WbsMappingServiceTests.cs
│   ├── WbsAiGenerationServiceTests.cs
│   ├── WbsCodeGenerationServiceTests.cs
│   └── WbsLockServiceTests.cs
└── Planova.UI.Tests/                          # ENHANCE
    ├── DashboardViewModelTests.cs
    ├── ProjectListViewModelTests.cs
    ├── BoqImportViewModelTests.cs
    └── WbsEditorViewModelTests.cs
```

**Structure Decision**: This feature enhances multiple existing Planova modules (`Planova.Boq`, `Planova.Wbs`, `Planova.UI`, `Planova.Persistence`, `Planova.Localization`) following the same Clean Architecture module pattern established by the existing codebase. Each module retains its Domain/Application/Extensions structure. UI additions follow the established ViewModel/View pattern under `Planova.UI`. No new top-level modules are created.

## Complexity Tracking

> **No violations — this section is not applicable.**
