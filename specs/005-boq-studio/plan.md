# Implementation Plan: BOQ Studio

**Branch**: `005-boq-studio` | **Date**: 2026-06-03 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/005-boq-studio/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Implement the BOQ Studio module — a hierarchical bill-of-quantities workspace for planning engineers. Users import BOQ data from Excel using Phase 2 infrastructure (IWorkbookReader, IMappingProfileService), view/edit as a virtualized tree (expand/collapse, inline edits, reorder), validate structural integrity, manage classification taxonomies, reuse item libraries, and generate/export reports (Excel via Phase 2 WorkbookWriter, PDF via QuestPDF). Built on .NET 8 + WPF, following Clean Architecture and MVVM.

## Technical Context

**Language/Version**: .NET 8, C# 12

**Primary Dependencies**: CommunityToolkit.Mvvm, Serilog, Microsoft.Extensions.DependencyInjection, ClosedXML (Phase 2), EPPlus (Phase 2), CsvHelper, QuestPDF, LiveCharts2 (if dashboard visualizations needed)

**Storage**: SQLite with EF Core. New entities: Boq, BoqItem, BoqLibrary, BoqLibraryItem, BoqClassification. Existing ExcelMappingProfiles table reused for import mapping.

**Testing**: xUnit — unit tests for domain entities, application services, tree builder, validation engine, CSV reader; integration tests for import/export workflows

**Target Platform**: Windows (WPF Desktop)

**Project Type**: Desktop application module (new "BOQ Studio" module per Modular Domain Design)

**Performance Goals**: Virtualized tree render 10k items < 2s, scrolling at 60fps, section subtotals accurate to 0.01, import 500 rows < 30s including mapping and preview

**Constraints**: No Excel installation dependency, async-first, CancellationToken everywhere, RTL support, virtualized tree view (VirtualizingTreeView or similar), optimistic locking for concurrent edit detection

**Scale/Scope**: Single-user desktop, up to 10k items per BOQ, up to 500 rows per import, .xlsx primary import format, CSV secondary import format

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Compliance | Notes |
|-----------|-----------|-------|
| I. Architecture First | ✅ PASS | Clean Architecture layers respected; Planova.Boq as Domain + Application, Planova.UI for Views/ViewModels, Planova.Persistence for EF configs |
| II. MVVM & Fluent UI | ✅ PASS | WPF + MVVM via CommunityToolkit.Mvvm; navigation rail integration, virtualized tree view |
| III. Modular Domain | ✅ PASS | Fits under "BOQ Studio" module with clear domain entities and application contracts |
| IV. Build vs Buy | ✅ PASS | QuestPDF for PDF generation (approved); CsvHelper for CSV parsing (mature OSS); no custom engine built |
| V. Automation Agnostic | ✅ PASS | N/A — no automation workflows in scope |
| VI. AI Provider Agnostic | ✅ PASS | N/A — no AI integration in scope |
| VII. Multilingual First | ✅ PASS | English + Arabic with runtime switching per FR-019; localized resource strings for all BOQ screens |
| VIII. Performance & Scalability | ✅ PASS | Async-first, CancellationToken, virtualized tree view, background import/export processing, optimized queries |

All gates pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/005-boq-studio/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Planova.Boq/                                         # NEW — Domain + Application + CSV reader
├── Domain/
│   ├── Entities/
│   │   ├── Boq.cs
│   │   ├── BoqItem.cs
│   │   ├── BoqLibrary.cs
│   │   ├── BoqLibraryItem.cs
│   │   └── BoqClassification.cs
│   ├── Enums/
│   │   └── BoqStatus.cs
│   └── Interfaces/
│       ├── IBoqRepository.cs
│       ├── IBoqLibraryRepository.cs
│       ├── IBoqClassificationRepository.cs
│       ├── ITreeBuilder.cs
│       └── IBoqValidationService.cs
├── Application/
│   ├── Services/
│   │   ├── BoqService.cs
│   │   ├── BoqImportService.cs
│   │   ├── BoqExportService.cs
│   │   ├── BoqValidationService.cs
│   │   ├── BoqReportService.cs
│   │   ├── ClassificationService.cs
│   │   ├── LibraryService.cs
│   │   └── TreeBuilderService.cs
│   ├── Dto/
│   │   ├── BoqDto.cs
│   │   ├── BoqItemDto.cs
│   │   ├── BoqImportRequest.cs
│   │   ├── BoqImportResult.cs
│   │   ├── BoqExportRequest.cs
│   │   └── ValidationResultDto.cs
│   └── Mappings/
│       └── BoqProfile.cs
├── CsvReader/
│   ├── BoqCsvReader.cs
│   └── BoqCsvReaderOptions.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Planova.Boq.csproj

Planova.UI/
├── Views/Boq/                                      # NEW
│   ├── BoqTreeView.xaml
│   ├── BoqImportWizardView.xaml
│   ├── BoqEditorView.xaml
│   ├── BoqValidationView.xaml
│   ├── BoqClassificationView.xaml
│   ├── BoqLibraryView.xaml
│   └── BoqReportView.xaml
└── ViewModels/Boq/                                 # NEW
    ├── BoqTreeViewModel.cs
    ├── BoqImportViewModel.cs
    ├── BoqEditorViewModel.cs
    ├── BoqValidationViewModel.cs
    ├── BoqClassificationViewModel.cs
    ├── BoqLibraryViewModel.cs
    └── BoqReportViewModel.cs

Planova.Persistence/
└── EntityConfigurations/                           # NEW
    ├── BoqConfiguration.cs
    ├── BoqItemConfiguration.cs
    ├── BoqLibraryConfiguration.cs
    ├── BoqLibraryItemConfiguration.cs
    └── BoqClassificationConfiguration.cs

tests/
├── Planova.Boq.Tests/                              # NEW
│   ├── Domain/
│   ├── Application/
│   ├── CsvReader/
│   └── Planova.Boq.Tests.csproj
└── Planova.UI.Tests/
    └── ViewModels/Boq/                             # NEW
        ├── BoqTreeViewModelTests.cs
        ├── BoqImportViewModelTests.cs
        └── BoqEditorViewModelTests.cs
```

**Structure Decision**: New `Planova.Boq` project following Clean Architecture with its own Domain (entities, enums, interfaces) and Application (services, DTOs) layers. UI components live in existing `Planova.UI` under `Views/Boq/` and `ViewModels/Boq/`. EF Core configurations live in existing `Planova.Persistence`. This follows the same pattern as other modules — a dedicated project with domain logic plus UI in the shared WPF project. The CSV reader lives inside Planova.Boq (not a separate project) since it's a simple adapter. Tests in a new `Planova.Boq.Tests` project plus additions to existing `Planova.UI.Tests`.

## Complexity Tracking

All gates pass. No complexity justification needed.
