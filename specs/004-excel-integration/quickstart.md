# Quickstart: Excel Integration

## Project Structure

```
src/
├── Planova.Excel/          # Excel module (Infrastructure layer)
│   ├── Services/           # Import, Export, Preview, Mapping, Validation
│   ├── Readers/            # ClosedXML-based worksheet readers
│   ├── Writers/            # ClosedXML-based workbook writers
│   ├── Validation/         # Pluggable validators per entity type
│   ├── Mapping/            # Column mapping engine
│   ├── Models/             # Request/result DTOs
│   └── Extensions/         # DI service registration
├── Planova.UI/
│   ├── Views/Excel/        # WPF views
│   └── ViewModels/Excel/   # MVVM ViewModels
└── Planova.Persistence/
    └── Configurations/     # EF Core entity configs
```

## Getting Started

1. **Restore packages** — ClosedXML and EPPlus are the primary Excel libraries
2. **Register services** — Call `services.AddPlanovaExcel()` in the DI setup
3. **Add migrations** — `dotnet ef migrations add AddExcelMappingProfiles --context PlanovaDbContext`
4. **Build** — `dotnet build src/Planova.sln`

## Key Service Contracts (see `contracts/`)

| Interface | Responsibility | Consumed By |
|-----------|---------------|-------------|
| IWorkbookReader | Open/read Excel files | ImportService, WorkbookPreviewService |
| IWorkbookWriter | Generate Excel output | ExportService |
| IWorkbookPreviewService | Read-only preview for browser UI | WorkbookBrowserViewModel |
| IImportService | Validate + batch import | ImportViewModel |
| IExportService | Query + export | ExportViewModel |
| IMappingProfileService | CRUD for mapping profiles | MappingProfilesViewModel |
| IValidationService | Validate records pre-commit | ImportService |

## UI Navigation

Feature pages under `Navigation Rail > Excel Tools`:
- **Workbook Browser** — browse and preview Excel files
- **Import Wizard** — multi-step import (select → map → validate → commit)
- **Export Wizard** — select entity → choose columns → generate
- **Mapping Profiles** — save/edit/delete/clone mapping profiles

## Performance Targets

| Operation | Target |
|-----------|--------|
| Workbook load | < 1s |
| Preview 1000 rows | < 1s |
| Import 10000 rows | < 2s |
| Export 10000 rows | < 2s |
| Memory | < 500MB |
