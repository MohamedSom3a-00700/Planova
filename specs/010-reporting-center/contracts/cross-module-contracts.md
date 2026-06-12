# Cross-Module Contracts — Reporting Center

The Reporting Center consumes data from existing studio modules through their published service interfaces. It does NOT depend on any module's domain layer directly — only on service interfaces.

## Consumed Interfaces

### From Planova.Activity

| Interface | Methods Used | Purpose |
|-----------|-------------|---------|
| `IActivityService` | `GetActivitiesByDateAsync`, `GetActivitiesByWbsAsync`, `GetDelayedActivitiesAsync`, `GetActivitiesByProjectAsync` | Activity progress, dates, delays, look-ahead data |

### From Planova.Resource

| Interface | Methods Used | Purpose |
|-----------|-------------|---------|
| `IResourceAssignmentService` | `GetResourceUsageByDateAsync`, `GetResourceUsageByWeekAsync`, `GetResourceUsageByMonthAsync` | Labour/equipment/material counts |

### From Planova.Cost

| Interface | Methods Used | Purpose |
|-----------|-------------|---------|
| `ICostService` | `GetBudgetSummaryAsync` | Budget vs actual comparison |
| `IEvmService` | `ComputeMetricsAsync` | EVM metric cards (CPI, SPI, CV, SV, EAC, ETC, VAC) |
| `ICashFlowService` | `GetCashFlowAsync` | S-Curve data points |

### From Planova.Wbs

| Interface | Methods Used | Purpose |
|-----------|-------------|---------|
| `IWbsService` | `GetWbsTreeAsync`, `GetWbsProgressAsync` | WBS breakdown for progress by WBS sections |

### From Planova.Project (or Planova.Shared)

| Interface | Methods Used | Purpose |
|-----------|-------------|---------|
| `IProjectService` | `GetProjectInfoAsync` | Project name, code, dates, location |
| `IProjectDocumentService` | `GetPhotosByProjectAsync`, `GetDocumentsByDateAsync` | Attached photos for Daily/Weekly reports |

### Shared Abstractions

| Interface | Location | Purpose |
|-----------|----------|---------|
| `IAIProvider` | `Planova.Shared.Abstractions` | AI narrative generation (Semantic Kernel) |
| `IWorkbookWriter` | `Planova.Excel` | Excel export (reused from Phase 2) |

## Data Flow

```
DailyReportDataProvider ──→ IActivityService, IResourceAssignmentService,
                             IProjectService, IProjectDocumentService

WeeklyReportDataProvider ──→ IActivityService, IResourceAssignmentService,
                              IWbsService, IProjectService

MonthlyReportDataProvider ─→ IActivityService, IWbsService, ICostService,
                              IEvmService, ICashFlowService, IProjectService

ExecutiveReportDataProvider → IActivityService, IWbsService, ICostService,
                               IEvmService, ICashFlowService, IProjectService
```

## Contract Versioning

All consumed interfaces are assumed stable within this phase. No version negotiation is required. If an interface signature changes in a future phase, the Reporting Center's data providers must be updated to match.
