# Quickstart — Cost Studio

## Overview

Cost Studio (Phase 7) adds cost management to Planova. It provides hierarchical cost breakdown, direct cost management, budget management with revisions, cash flow forecasting with S-Curve, Earned Value Management (EVM), actual cost entry with Excel import, AI-powered cost services, and four cost reports with Excel/PDF export.

## Architecture

```
Planova.Cost/              ← New module (Class Library)
├── Domain/                ← Entities, Enums, Interfaces
├── Application/           ← Services, DTOs, Mappings
├── Extensions/            ← DI registration

Planova.Persistence/       ← Existing, add configurations + repos
Planova.UI/                ← Existing, add Views + ViewModels
Planova.Localization/      ← Existing, add resx files
Planova.Excel/             ← Existing, add readers/writers

tests/Planova.Cost.Tests/  ← New test project
```

## Prerequisites

- .NET 8 SDK
- Planova solution built with all existing modules (Planova.Domain, Planova.Shared, Planova.Persistence, Planova.UI)
- Phase 5 (Activity Studio) completed — Cost Studio depends on Activity entities for cost breakdown, EVM
- Phase 6 (Resource Studio) completed — Cost Studio loads resource assignment costs
- Existing PlanovaDbContext with EF Core migrations infrastructure

## Setup Steps

### 1. Create the Cost Module Project

```bash
dotnet new classlib -n Planova.Cost -o Planova.Cost --framework net8.0
dotnet sln Planova.slnx add Planova.Cost/Planova.Cost.csproj
dotnet add Planova.Cost/Planova.Cost.csproj reference Planova.Domain/Planova.Domain.csproj
dotnet add Planova.Cost/Planova.Cost.csproj reference Planova.Shared/Planova.Shared.csproj
dotnet add Planova.Cost/Planova.Cost.csproj reference Planova.Activity/Planova.Activity.csproj
dotnet add Planova.Cost/Planova.Cost.csproj reference Planova.Resource/Planova.Resource.csproj
```

### 2. Create the Test Project

```bash
dotnet new xunit -n Planova.Cost.Tests -o tests/Planova.Cost.Tests --framework net8.0
dotnet sln Planova.slnx add tests/Planova.Cost.Tests/Planova.Cost.Tests.csproj
dotnet add tests/Planova.Cost.Tests/Planova.Cost.Tests.csproj reference Planova.Cost/Planova.Cost.csproj
```

### 3. Add Entities and Enums

Create folder structure under `Planova.Cost/Domain/`:

```
Domain/
├── Entities/
│   ├── Budget.cs
│   ├── BudgetRevision.cs
│   ├── DirectCost.cs
│   ├── CostBaseline.cs
│   ├── CostBaselineRow.cs
│   └── ActualCost.cs
├── Enums/
│   ├── BudgetRevisionType.cs
│   ├── BudgetRevisionStatus.cs
│   ├── BudgetStatus.cs
│   ├── DirectCostCategory.cs
│   ├── DirectCostScope.cs
│   └── ActualCostSource.cs
└── Interfaces/
    ├── IBudgetRepository.cs
    ├── IBudgetRevisionRepository.cs
    ├── IDirectCostRepository.cs
    ├── ICostBaselineRepository.cs
    ├── IActualCostRepository.cs
    ├── ICostService.cs
    ├── IDirectCostService.cs
    ├── IActualCostService.cs
    ├── ICashFlowService.cs
    ├── IEvmService.cs
    ├── ICostAiService.cs
    └── ICostReportService.cs
```

### 4. Add Application Services and DTOs

Create folder structure under `Planova.Cost/Application/`:

```
Application/
├── Services/
│   ├── CostService.cs
│   ├── DirectCostService.cs
│   ├── ActualCostService.cs
│   ├── CashFlowService.cs
│   ├── EvmService.cs
│   ├── CostAiService.cs
│   └── CostReportService.cs
├── Dto/
│   ├── BudgetDto.cs
│   ├── BudgetRevisionDto.cs
│   ├── DirectCostDto.cs
│   ├── CostBaselineDto.cs
│   ├── ActualCostDto.cs
│   ├── CashFlowPeriodDto.cs
│   ├── EvmMetricsDto.cs
│   ├── AiSuggestionDto.cs
│   └── ImportResultDto.cs
└── Mappings/
    └── CostMappingProfile.cs
```

### 5. Register DI

```csharp
// Planova.Cost/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddPlanovaCost(this IServiceCollection services)
{
    services.AddScoped<ICostService, CostService>();
    services.AddScoped<IDirectCostService, DirectCostService>();
    services.AddScoped<IActualCostService, ActualCostService>();
    services.AddScoped<ICashFlowService, CashFlowService>();
    services.AddScoped<IEvmService, EvmService>();
    services.AddScoped<ICostAiService, CostAiService>();
    services.AddScoped<ICostReportService, CostReportService>();
    return services;
}
```

Then call `services.AddPlanovaCost()` in `App.xaml.cs` alongside existing module registrations.

### 6. Add Persistence

Add to `Planova.Persistence`:

- Entity configurations: `BudgetConfiguration.cs`, `BudgetRevisionConfiguration.cs`, `DirectCostConfiguration.cs`, `CostBaselineConfiguration.cs`, `CostBaselineRowConfiguration.cs`, `ActualCostConfiguration.cs`
- Repository implementations: `BudgetRepository.cs`, `BudgetRevisionRepository.cs`, `DirectCostRepository.cs`, `CostBaselineRepository.cs`, `ActualCostRepository.cs`
- Register repositories in `ServiceCollectionExtensions.cs`
- Apply configurations in `PlanovaDbContext.OnModelCreating`
- Create EF Core migration: `dotnet ef migrations add AddCostEntities`

### 7. Add UI Components

Add to `Planova.UI`:

- `ViewModels/Cost/` — Studio, Breakdown, Direct Cost Manager, Budget, Revision, Actual Cost, Cash Flow, EVM, AI, Report
- `Views/Cost/` — Corresponding XAML views
- Register Cost Studio as a navigation target in `ShellViewModel`

### 8. Add Localization Resources

Add to `Planova.Localization/Resources/`:

- `CostResources.en.resx` — English strings
- `CostResources.ar.resx` — Arabic strings (RTL)

### 9. Add Excel Import/Export

Add to `Planova.Excel`:

- `Readers/ActualCostImportReader.cs`
- `Writers/CostReportWriter.cs`
- `Services/CostImportService.cs`

## Key Workflows

### Viewing Cost Breakdown

1. Open Cost Studio → Cost Breakdown tab
2. Hierarchical tree: Project → WBS items → Activities → Resource costs + Direct costs
3. Expand/collapse nodes to navigate; totals computed at each level
4. Resource costs auto-loaded from Resource Studio assignments (read-only)
5. Direct costs editable inline or via Direct Cost Manager

### Managing Direct Costs

1. Select an activity (or project root) → click "Add Direct Cost"
2. Choose category (Permits, Insurance, etc.) or Custom with free-text name
3. Enter description, quantity, unit of measure, unit rate
4. Total computed automatically: Quantity × Unit Rate
5. Edit or delete existing direct costs; totals update in real time

### Managing Budget

1. Open Budget tab → view budget summary (Resource Costs + Direct Costs + Contingency = Total Budget)
2. Set contingency as absolute amount or percentage
3. Optionally override Total Budget with manual value (sets IsManualOverride)
4. Resource cost change indicator (FR-030): warning shown if costs changed since last save

### Managing Budget Revisions

1. Create a new revision → select type (Original, Revised, Approved)
2. Enter amount and reason → status is Pending
3. Approve pending revision → status changes to Approved, locked
4. Revision history displayed chronologically with approval metadata

### Setting Cost Baseline

1. Set baseline → snapshots current activity costs and schedule
2. Only one active baseline allowed per project
3. Baseline used as EVM reference; resource cost changes after baseline trigger indicator
4. Deactivate baseline to create a new one (historical rows preserved)

### Entering Actual Costs

1. Manual: select activity → enter total amount → save
2. Import: prepare Excel file with activity codes and amounts
3. Import process validates activity codes, reports unmatched rows
4. Existing records are upserted (not duplicated)
5. Import aborts if >20% unmatched or >10000 rows

### Viewing Cash Flow

1. Open Cash Flow tab → view planned vs actual costs over time
2. Toggle between weekly and monthly periods
3. S-Curve chart shows cumulative planned vs cumulative actual
4. Costs spread evenly across activity working days

### EVM Analysis

1. Set a Data Date (cutoff for EVM computation)
2. View metrics: PV, EV, AC, CV, SV, CPI, SPI, EAC, ETC, VAC
3. Color-coded: green (≥1.0), amber (0.8–0.99), red (<0.8)
4. If no baseline set: warning with prompt to set one

### AI Cost Services

1. Estimate Cost: select activity → AI suggests budget with confidence
2. Detect Anomalies: AI flags activities with significant cost deviations
3. Forecast: AI-adjusted EAC based on CPI trends
4. Generate Narrative: 2-4 paragraph professional cost status summary
5. Graceful fallback if AI provider unavailable

### Reports

1. Open Reports tab → select report type
2. Cost Breakdown: WBS → Activity → Resources → Direct Costs
3. Cash Flow: period table + S-Curve chart
4. EVM: metrics + activity details + AI narrative
5. Budget Summary: Original/Revised/Approved/Contingency
6. Export to Excel or PDF

## Testing

```bash
# Run Cost Studio tests
dotnet test tests/Planova.Cost.Tests/Planova.Cost.Tests.csproj

# Run all tests
dotnet test
```

Test categories:
- **Domain tests**: Entity behavior, validation, state transitions
- **Service tests**: Cost service, EVM service, cash flow service (mocked repositories)
- **Persistence tests**: Repository implementations against SQLite in-memory
- **UI tests**: ViewModel behavior (if applicable)
- **Import tests**: Excel import parsing, validation, upsert logic

## Dependencies

| Dependency | Version | Purpose |
|-----------|---------|---------|
| Planova.Domain | — | Base entities, value objects |
| Planova.Shared | — | Cross-cutting abstractions |
| Planova.Activity | — | Activity entity for cost breakdown, EVM |
| Planova.Resource | — | Resource assignment costs |
| Semantic Kernel | 1.x | AI cost services abstraction |
| LiveCharts2 | 2.x | S-Curve chart visualization |
| QuestPDF | 2024.x | PDF report generation |
| ClosedXML | 0.102.x | Excel import/export |

## Related Documents

- [spec.md](spec.md) — Feature specification
- [plan.md](plan.md) — Implementation plan
- [research.md](research.md) — Design decisions and research
- [data-model.md](data-model.md) — Entity definitions and relationships
- [contracts/](contracts/) — Interface contracts
