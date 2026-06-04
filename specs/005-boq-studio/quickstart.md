# Quickstart: BOQ Studio

## Project Structure

```
Planova.Boq/                          # NEW — BOQ Studio module
├── Domain/Entities/                  # Boq, BoqItem, BoqLibrary, BoqLibraryItem, BoqClassification
├── Domain/Enums/                     # BoqStatus, ItemType, LibraryType, ClassificationScope
├── Domain/Interfaces/                # Repository and service contracts
├── Application/Services/             # Import, export, validation, tree builder, reports, classification, libraries
├── Application/Dto/                  # Request/result DTOs
├── CsvReader/                        # Standalone CSV import path
├── Extensions/                       # DI service registration
└── Planova.Boq.csproj

Planova.UI/
├── Views/Boq/                        # NEW — WPF views
└── ViewModels/Boq/                   # NEW — MVVM ViewModels

Planova.Persistence/
└── EntityConfigurations/             # NEW — EF Core configs for BOQ entities

tests/
├── Planova.Boq.Tests/                # NEW — Unit + integration tests
└── Planova.UI.Tests/ViewModels/Boq/  # NEW — ViewModel tests
```

## Getting Started

1. **Create the project** — `dotnet new classlib -n Planova.Boq` and add to solution
2. **Add NuGet references** — CommunityToolkit.Mvvm, CsvHelper, QuestPDF (already in solution)
3. **Reference Phase 2** — Add project reference to `Planova.Excel` for IWorkbookReader, IMappingProfileService, IWorkbookWriter
4. **Register services** — Call `services.AddPlanovaBoq()` in the DI setup (implement in `Planova.Boq.Extensions.ServiceCollectionExtensions`)
5. **Add EF migrations** — `dotnet ef migrations add AddBoqEntities --context PlanovaDbContext`
6. **Build** — `dotnet build Planova.slnx`

## Key Service Contracts (see `contracts/`)

| Interface | Responsibility | Consumed By |
|-----------|---------------|-------------|
| IBoqRepository | BOQ CRUD | BoqService |
| IBoqItemRepository | BOQ item CRUD + tree operations | BoqService, TreeBuilderService |
| IBoqLibraryRepository | Library CRUD | LibraryService |
| IBoqClassificationRepository | Classification CRUD | ClassificationService |
| ITreeBuilder | Assemble flat rows into hierarchy | BoqImportService |
| IBoqValidationService | Validate BOQ structure + data | BoqService, BoqImportService |
| IBoqImportService | Import from Excel/CSV | BoqImportViewModel |
| IBoqExportService | Export to Excel/CSV | BoqExportViewModel |
| IBoqReportService | Generate PDF/Excel reports | BoqReportViewModel |
| IBoqCsvReader | Read CSV files | BoqImportService |

## Phase 2 Reuse

| Phase 2 Component | BOQ Integration Point |
|---|---|
| IWorkbookReader | Read Excel during BOQ import (via BoqImportService) |
| IMappingProfileService | Save/reuse column mappings for BOQ imports |
| IWorkbookWriter | Export BOQ reports to Excel (via BoqExportService) |
| ClosedXML/EPPlus | Underlying Excel engine (indirect, through Phase 2) |

## UI Navigation

Feature pages under `Navigation Rail > BOQ Studio`:
- **BOQ Tree** — view, expand/collapse, navigate hierarchical BOQ
- **Import BOQ** — multi-step wizard (select file → map columns → preview tree → validate → commit)
- **Edit BOQ** — inline editing, add/delete/reorder items
- **Validate** — run structural validation checks
- **Classifications** — manage classification taxonomies and assign codes
- **Libraries** — manage reusable item libraries and insert into BOQs
- **Reports** — generate summary and itemized reports

## Performance Targets

| Operation | Target |
|-----------|--------|
| Virtualized tree render (10k items) | < 2s |
| Scrolling at full dataset | 60 fps |
| Subtotal/grand total computation | < 100ms |
| Import 500 rows (Excel, including mapping + preview) | < 30s |
| Export 10k items to Excel | < 3s |
| PDF report generation (1000 items) | < 5s |
