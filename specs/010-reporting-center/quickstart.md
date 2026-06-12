# Reporting Center — Quickstart

## What is it?

The Reporting Center (Phase 8) is a cross-studio report orchestration hub that composites data from Activity, Resource, Cost, WBS, and Project services into four consolidated report types: Daily, Weekly, Monthly, and Executive.

## Key Features

- **4 report types** with auto-populated data from existing studios
- **AI narrative generation** for all report types (via Semantic Kernel)
- **3-format export**: Excel (ClosedXML), PDF (QuestPDF), Word (OpenXML)
- **Scheduled auto-generation** with timezone-aware timer
- **Report History** with filter, re-export, archive
- **Template editor** — reorder/toggle section visibility per report type
- **Project Parties** — Client, Main Contractor, Sub Contractors with logos
- **Legacy Project Directory** preserved under Executive tab

## Architecture

```
Planova.Reporting/
├── Domain/       → Entities, Enums, Repository interfaces, Service interfaces
├── Application/  → Services, Data Providers, DTOs, Mappings
├── Background/   → ReportGenerationHostedService (IHostedService)
└── Extensions/   → ServiceCollectionExtensions for DI

Depends on: Activity, Resource, Cost, WBS, Project (contracts only)
UI in: Planova.UI/Views/Reporting/, Planova.UI/ViewModels/Reporting/
Persistence in: Planova.Persistence (entity configs + repositories)
Localization: Planova.Localization/Resources/ReportingResources.{en,ar}.resx
```

## Getting Started

1. Add `Planova.Reporting` project to the solution
2. Run EF Core migration to create Reporting tables
3. Register `services.AddPlanovaReporting()` in `App.xaml.cs`
4. Register all Reporting ViewModels and Views
5. Update `ShellViewModel.cs` — change nav target to `ReportingHubView`, set `isStudio: true`, add `"reports"` to `_studioTargetIds`
6. Remove old `ReportView`/`ReportViewModel` registrations

## Workstreams

| WS | Area | Key Deliverables |
|----|------|-----------------|
| A | Domain Setup | Planova.Reporting project, entities, enums, interfaces |
| B | Persistence | DbSets, entity configs, repositories, migration |
| C | Data Providers | DailyReportDataProvider, Weekly, Monthly, Executive |
| D | Report Engine | ReportEngine.GenerateAsync, snapshot assembly |
| E | Export Service | Excel/PDF/Word export pipeline |
| F | AI Narrative | IReportAiService via Semantic Kernel |
| G | Scheduler | ReportGenerationHostedService, NextRunAt computation |
| H | UI — Daily/Weekly/Monthly | Tab views with auto-populated sections |
| I | UI — Executive/Schedule/History | Executive view, scheduler config, history grid |
| J | UI — Templates/Settings | Section visibility, reorder, project parties manager |
| K | Navigation | Shell integration, nav target update, backward compat |
| L | Localization | EN/AR resx files for all UI strings |
| M | Testing | Unit + integration tests for all layers |

## Key Decisions

- **Report data snapshots**: `DataSnapshotJson` is authoritative; `ReportSections` are derived views
- **Status lifecycle**: Draft → Final → Archived (forward-only)
- **Scheduler**: In-process `IHostedService` with 60-second tick
- **Concurrent generation**: Duplicates allowed, managed from History
- **Logging**: Critical operations only (generation, export, delete, schedule toggle)
- **Access**: Same as project — any user with project access has full report permissions
