# Resource Studio — Interface Contracts

This directory documents the public interfaces (contracts) of the Resource Studio module.

## Structure

| File | Purpose |
|------|---------|
| `README.md` | This file — overview and contract index |
| `domain-interfaces.md` | Repository and domain service interfaces |
| `application-dtos.md` | Data transfer objects for cross-boundary communication |
| `ui-contracts.md` | ViewModel and navigation contracts |
| `persistence-contracts.md` | EF Core configuration interfaces |
| `cross-module-contracts.md` | Contracts between Resource Studio and other modules |

## Layer Dependencies

```
Planova.Resource.Domain
  → Planova.Domain (base entities, value objects)
  → Planova.Shared (cross-cutting abstractions)
  → Planova.Activity.Domain (Activity entity reference for assignments)

Planova.Resource.Application
  → Planova.Resource.Domain
  → Planova.Domain
  → Planova.Shared

Planova.Persistence
  → Planova.Resource.Domain
  → Planova.Resource.Application (if service implementations)
  → Planova.Persistence (DbContext)

Planova.UI
  → Planova.Resource.Application
  → Planova.Shared
  → Planova.Localization
```

## Interface Naming Conventions

- **Repositories**: `I{Entity}Repository` — data access per entity
- **Services**: `I{Feature}Service` — business logic per feature area
- **DTOs**: `{Entity}Dto`, `Create{Entity}Request`, `Update{Entity}Request`

## Key Contracts Summary

| Interface | Location | Consumers |
|-----------|----------|-----------|
| `IResourceRepository` | Domain | Persistence, Application |
| `IResourceRateRepository` | Domain | Persistence, Application |
| `ICrewRepository` | Domain | Persistence, Application |
| `ICrewResourceRepository` | Domain | Persistence, Application |
| `IResourceAssignmentRepository` | Domain | Persistence, Application |
| `IResourceUsageRepository` | Domain | Persistence, Application |
| `IResourceService` | Domain | UI, Persistence |
| `ICrewService` | Domain | UI, Persistence |
| `IResourceAssignmentService` | Domain | UI, Persistence |
| `IResourceHistogramService` | Domain | UI |
| `IResourceAiEstimationService` | Domain | UI |
| `IResourceReportService` | Domain | UI |
| `IResourceImportService` | Domain | UI |
| `IResourceExportService` | Domain | UI |
