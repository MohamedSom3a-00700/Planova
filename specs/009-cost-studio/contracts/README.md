# Cost Studio — Interface Contracts

This directory documents the public interfaces (contracts) of the Cost Studio module.

## Structure

| File | Purpose |
|------|---------|
| `README.md` | This file — overview and contract index |
| `domain-interfaces.md` | Repository and domain service interfaces |
| `application-dtos.md` | Data transfer objects for cross-boundary communication |
| `ui-contracts.md` | ViewModel and navigation contracts |
| `persistence-contracts.md` | EF Core configuration interfaces |
| `cross-module-contracts.md` | Contracts between Cost Studio and other modules |

## Layer Dependencies

```
Planova.Cost.Domain
  → Planova.Domain (base entities, value objects)
  → Planova.Shared (cross-cutting abstractions)
  → Planova.Activity.Domain (Activity entity reference)
  → Planova.Resource.Domain (ResourceAssignment cost data)

Planova.Cost.Application
  → Planova.Cost.Domain
  → Planova.Domain
  → Planova.Shared

Planova.Persistence
  → Planova.Cost.Domain
  → Planova.Persistence (DbContext)

Planova.UI
  → Planova.Cost.Application
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
| `IBudgetRepository` | Domain | Persistence, Application |
| `IBudgetRevisionRepository` | Domain | Persistence, Application |
| `IDirectCostRepository` | Domain | Persistence, Application |
| `ICostBaselineRepository` | Domain | Persistence, Application |
| `IActualCostRepository` | Domain | Persistence, Application |
| `ICostService` | Domain | UI, Persistence |
| `IDirectCostService` | Domain | UI, Persistence |
| `IActualCostService` | Domain | UI, Persistence |
| `ICashFlowService` | Domain | UI |
| `IEvmService` | Domain | UI |
| `ICostAiService` | Domain | UI |
| `ICostReportService` | Domain | UI |
