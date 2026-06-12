# Reporting Center — Interface Contracts

This directory documents the public interfaces (contracts) of the Reporting Center module.

## Structure

| File | Purpose |
|------|---------|
| `README.md` | This file — overview and contract index |
| `domain-interfaces.md` | Repository and service interfaces in the Domain layer |
| `cross-module-contracts.md` | Contracts between Reporting Center and other modules |
| `ui-contracts.md` | ViewModel and navigation contracts |

## Layer Dependencies

```
Planova.Reporting.Domain
  → Planova.Shared (cross-cutting abstractions, IAIProvider)

Planova.Reporting.Application
  → Planova.Reporting.Domain
  → Planova.Activity (IActivityService — contract only)
  → Planova.Resource (IResourceAssignmentService — contract only)
  → Planova.Cost (ICostService, IEvmService, ICashFlowService — contract only)
  → Planova.Wbs (IWbsService — contract only)
  → Planova.Project (IProjectService, IProjectDocumentService — contract only)
  → Planova.Excel (IWorkbookWriter — for export)

Planova.Persistence
  → Planova.Reporting.Domain
  → Planova.Persistence (DbContext)

Planova.UI
  → Planova.Reporting.Application (service interfaces only)
  → Planova.Shared
  → Planova.Localization
```

## Interface Naming Conventions

- **Repositories**: `I{Entity}Repository` — data access per entity
- **Services**: `I{Feature}Service` — business logic per feature area
- **Data Providers**: `IReportDataProvider<TData>` — report-type-specific data collection
- **DTOs**: `{Entity}Dto`, typed report data DTOs

## Key Contracts Summary

| Interface | Location | Consumers |
|-----------|----------|-----------|
| `IReportTemplateRepository` | Domain | Persistence, Application |
| `IReportInstanceRepository` | Domain | Persistence, Application |
| `IReportScheduleRepository` | Domain | Persistence, Application |
| `IProjectPartyRepository` | Domain | Persistence, Application |
| `IReportEngine` | Domain | UI, Background |
| `IReportDataProvider<TData>` | Domain | Application (DataProviders) |
| `IReportSchedulerService` | Domain | UI, Background |
| `IReportExportService` | Domain | UI |
| `IReportAiService` | Domain | UI |
| `IProjectPartyService` | Domain | UI |
| `IReportSettingsService` | Domain | UI |
