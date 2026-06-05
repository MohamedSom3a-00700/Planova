# Implementation Plan: Excel Integration

**Branch**: `004-excel-integration` | **Date**: 2026-06-03 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-excel-integration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Provide enterprise-grade Excel integration for Planova Desktop — import/export data, browse workbooks, preview worksheets, and save reusable column mappings. Excel is treated as an integration format only; the database remains the source of truth. Built on .NET 8 + WPF with ClosedXML/EPPlus, following Clean Architecture and MVVM.

## Technical Context

**Language/Version**: .NET 8, C# 12

**Primary Dependencies**: ClosedXML (primary), EPPlus (secondary), CommunityToolkit.Mvvm, Serilog, Microsoft.Extensions.DependencyInjection

**Storage**: SQLite with EF Core (ExcelMappingProfiles table)

**Testing**: Unit tests (xUnit) for readers, writers, validation, mapping; Integration tests for import/export workflows

**Target Platform**: Windows (WPF Desktop)

**Project Type**: Desktop application module

**Performance Goals**: Workbook load < 1s, preview 1000 rows < 1s, import 10000 rows < 2s, export 10000 rows < 2s, memory < 500MB

**Constraints**: No Excel installation dependency, read-only browsing, async-first, RTL support, CancellationToken everywhere, virtualized UI

**Scale/Scope**: Single-user desktop, up to 10000 rows per import, up to 10MB workbooks, .xlsx primary format

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Compliance | Notes |
|-----------|-----------|-------|
| I. Architecture First | ✅ PASS | Clean Architecture layers respected; Planova.Excel as Infrastructure, Planova.UI for Views/ViewModels |
| II. MVVM & Fluent UI | ✅ PASS | WPF + MVVM via CommunityToolkit.Mvvm; navigation rail + workspace layout |
| III. Modular Domain | ✅ PASS | Fits under "Integration Hub" module with clear contracts |
| IV. Build vs Buy | ✅ PASS | ClosedXML + EPPlus used per approved stack; no custom Excel engine |
| V. Automation Agnostic | ✅ PASS | N/A — no automation workflows in scope |
| VI. AI Provider Agnostic | ✅ PASS | N/A — no AI integration in scope |
| VII. Multilingual First | ✅ PASS | English + Arabic with runtime switching per FR-011 |
| VIII. Performance & Scalability | ✅ PASS | Async-first, CancellationToken, virtualized UI, background processing |

All gates pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/004-excel-integration/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Planova.Excel/
│   ├── Services/
│   │   ├── WorkbookReader.cs
│   │   ├── WorkbookWriter.cs
│   │   ├── WorkbookPreviewService.cs
│   │   ├── ImportService.cs
│   │   ├── ExportService.cs
│   │   ├── MappingProfileService.cs
│   │   └── ValidationService.cs
│   ├── Models/
│   │   ├── WorkbookInfo.cs
│   │   ├── WorksheetInfo.cs
│   │   ├── PreviewData.cs
│   │   ├── ImportRequest.cs
│   │   ├── ImportResult.cs
│   │   ├── ExportRequest.cs
│   │   ├── ExportResult.cs
│   │   ├── MappingProfile.cs
│   │   ├── ValidationResult.cs
│   │   └── ValidationError.cs
│   ├── Readers/
│   ├── Writers/
│   ├── Validation/
│   ├── Mapping/
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs
│   └── Planova.Excel.csproj
├── Planova.UI/
│   ├── Views/Excel/
│   │   ├── WorkbookBrowserView.xaml
│   │   ├── ImportWizardView.xaml
│   │   ├── ExportWizardView.xaml
│   │   └── MappingProfilesView.xaml
│   └── ViewModels/Excel/
│       ├── WorkbookBrowserViewModel.cs
│       ├── ImportViewModel.cs
│       ├── ExportViewModel.cs
│       └── MappingProfilesViewModel.cs
└── Planova.Persistence/
    └── Configurations/
        └── ExcelMappingProfileConfiguration.cs (EF Core)

tests/
├── Planova.Excel.Tests/
│   ├── Readers/
│   ├── Writers/
│   ├── Validation/
│   ├── Mapping/
│   └── Planova.Excel.Tests.csproj
├── Planova.UI.Tests/
│   ├── ViewModels/Excel/
│   └── Planova.UI.Tests.csproj
└── Planova.Integration.Tests/
    ├── ImportWorkflowTests.cs
    ├── ExportWorkflowTests.cs
    └── Planova.Integration.Tests.csproj
```

**Structure Decision**: Three-project WPF solution matching existing Planova architecture — Planova.Excel (Infrastructure/Excel services), Planova.UI (Views/ViewModels), Planova.Persistence (EF config). Tests follow same project-per-layer pattern.

## Complexity Tracking

All gates pass. No complexity justification needed.
