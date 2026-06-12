# Planova Phase 8 Implementation Plan

**Phase**: 8 — Reporting Center

**Date**: 2026-06-12

**Source of Truth**:
[docs/01-PRODUCT_VISION.md](./01-PRODUCT_VISION.md),
[docs/02-MASTER_ROADMAP.md](./02-MASTER_ROADMAP.md),
[docs/04-SYSTEM_ARCHITECTURE.md](./04-SYSTEM_ARCHITECTURE.md),
[docs/05-TECHNOLOGY_STACK.md](./05-TECHNOLOGY_STACK.md),
[docs/06-MODULE_CATALOG.md](./06-MODULE_CATALOG.md),
[docs/07-DATABASE_STRATEGY.md](./07-DATABASE_STRATEGY.md),
[docs/08-INTEGRATION_STRATEGY.md](./08-INTEGRATION_STRATEGY.md),
[docs/09-AI_STRATEGY.md](./09-AI_STRATEGY.md),
[docs/11-UI_UX_DESIGN_SYSTEM.md](./11-UI_UX_DESIGN_SYSTEM.md),
[docs/PLANOVA_CONSTITUTION_DRAFT.md](./PLANOVA_CONSTITUTION_DRAFT.md),
[docs/PHASE_7_IMPLEMENTATION_PLAN.md](./PHASE_7_IMPLEMENTATION_PLAN.md)

---

## Summary

Phase 8 delivers the **Reporting Center** — a unified, cross-studio report orchestration hub that composites data from all prior studios into four consolidated report types (Daily, Weekly, Monthly, Executive). Unlike the per-studio report tabs that already exist inside each studio workspace (BOQ Report, Resource Report, Cost Report, etc.), the Reporting Center adds:

1. **Cross-studio orchestration** — queries data from Activity, Resource, Cost, WBS, BOQ, and Project services through existing contracts
2. **Four consolidated report types** — Daily, Weekly, Monthly, Executive, each with its own data provider and rendered preview
3. **Scheduled generation** — `IHostedService`-based timer that auto-generates reports on configured schedules (daily/weekly/monthly)
4. **AI narrative generation** — `IAIProvider`-powered contextual narratives for all four report types
5. **Unified report management** — history, re-export, archive, template management

The Reporting Center replaces the current placeholder "Reports" nav target (which currently shows a simple Projects/Clients/Contracts entity list) with a full 7-tab Reporting Hub workspace, gated behind project selection like other studios.

---

## Phase 8 Objectives

1. Create `Planova.Reporting` Class Library project following the same module pattern as `Planova.Cost`, `Planova.Resource`
2. Build the Report Engine — an orchestrator that resolves the correct data provider per report type, collects data from studio contracts, snapshots it, and produces a `ReportInstance`
3. Implement four `IReportDataProvider` implementations — Daily, Weekly, Monthly, Executive — each consuming existing studio service interfaces
4. Implement report scheduling — CRUD for schedules, `IHostedService` timer-based background generation
5. Implement full export pipeline — Excel (ClosedXML, reuse Phase 2), PDF (QuestPDF), Word (DOCX via DocumentFormat.OpenXml)
6. Implement AI narrative generation via `IAIProvider` for all four report types with regenerate/edit/accept workflow
7. Build the Reporting Hub UI — 7-tab workspace (Daily, Weekly, Monthly, Executive, Schedule, History, Settings) with per-type preview, export, and management
8. Add template management — reorder/visibility of report sections per report type
9. Build a Reporting Settings tab with per-report-type section visibility toggles (add/remove content sections independently for each report type)
10. Implement Project Parties management — define Client, Main Contractor, and Sub Contractors with names and logos per project, persisted in database and rendered in report headers/footers
11. Replace the existing "Reports" nav target (entity list) with the new `ReportingHubView`
12. Make the Reports nav item project-gated (`isStudio: true`) so it requires an active project
13. Preserve Clean Architecture, MVVM, localization (English + Arabic), RTL, and theme consistency with all prior phases

---

## Phase 8 Scope

### In Scope

- **New domain module** (`Planova.Reporting`) following the same pattern as `Planova.Cost`, `Planova.Resource`
- `ReportTemplate` entity — defines structure/sections per report type, project or global scope
- `ReportInstance` entity — a generated report with frozen data snapshot, status, AI narrative
- `ReportSection` entity — a section within a report (Text, Table, Chart, Image, AiNarrative)
- `ReportSchedule` entity — scheduled generation config per project + report type
- `ReportExport` entity — tracking record for each exported format
- Four `IReportDataProvider` implementations consuming:
  - `IActivityService` — activities, progress %, dates, delays
  - `IResourceAssignmentService` — resource usage, crew data
  - `ICostService`, `IEvmService`, `ICashFlowService` — cost metrics, S-curves
  - `IWbsService` — WBS breakdown
  - `IProjectService` — project info, client, dates
  - `IProjectDocumentService` — attached photos/documents
- `IReportEngine` — orchestrator that drives report generation end-to-end
- `IReportExportService` — Excel/PDF/Word export with format-specific rendering
- `IReportAiService` — AI narrative generation with `IAIProvider` (Semantic Kernel)
- `IReportSchedulerService` — schedule CRUD + next-run computation
- `ReportGenerationHostedService` — `IHostedService`, 60-second tick, runs due schedules
- Daily Report view — date picker, auto-populated sections, AI narrative, photo attach, export
- Weekly Report view — week picker, progress/resources/delays/look-ahead sections
- Monthly Report view — month picker, EVM cards, S-curve chart, budget, AI narrative
- Executive Report view — period picker, KPI cards, S-curves, risk summary, AI executive narrative
- Report Schedule view — grid of schedules, add/edit/delete, active toggle, next-run indicator
- Report History view — generated reports list with filter, open, re-export, archive, delete
- Report Template editor — reorder/visibility toggle per section
- Reporting Settings tab — per-report-type section visibility toggles (add/remove individual sections from each report type's content independently)
- ProjectParty entity — defines Client, Main Contractor, Sub Contractors per project with name and logo path
- Project parties manager in Settings tab — CRUD for parties, logo upload, role assignment
- Report header/footer auto-populated with project parties (Client logo + name, Contractor logo + name, Subcontractors list)
- Navigation rail replacement — old `ReportView` replaced by `ReportingHubView`
- Nav item changed to project-gated (`isStudio: true`, added to `_studioTargetIds`)
- Database: new tables in `PlanovaDbContext` — ReportTemplates, ReportInstances, ReportSections, ReportSchedules, ReportExports, ProjectParties
- EF Core migration for all new Reporting entities
- Localization — English and Arabic for all Reporting Center screens
- Unit tests — domain logic, data providers, export service, AI service, scheduler, project parties
- Integration tests — persistence round-trip, full generation pipeline

### Out of Scope

- Email delivery of scheduled reports (deferred to Phase 19 — Enterprise Collaboration)
- Multi-user report approval workflows (Phase 19)
- Custom report designer / drag-drop layout editor (deferred)
- Dashboard widgets from report data (Phase 18 — Analytics Center)
- PDF/Word template customization (use fixed templates)
- Report sharing/public links
- Report version diff/comparison
- Web-based report viewer (desktop only)

---

## Non-Negotiable Constraints

- Clean Architecture must remain intact — Report domain logic in `Planova.Reporting`, persistence in `Planova.Persistence`, UI in `Planova.UI`.
- MVVM must remain the UI pattern throughout.
- Localization must support English and Arabic; RTL layout must remain correct.
- Theme support must remain consistent with Phases 0–7.
- Existing per-studio report tabs (inside BOQ, WBS, Resource, Cost studios) must remain unchanged — Reporting Center is additive, not a replacement.
- All studio data must be consumed through existing service interfaces — no direct DB queries from the Reporting module.
- Report data must be snapshotted (frozen) at generation time so that the report reflects the state when generated, even if source data changes later.
- AI provider must be abstracted via `IAIProvider` — no direct vendor lock-in.
- All async operations must accept `CancellationToken`.
- The existing `IReportService` (Projects/Clients/Contracts entity lists with PDF export) must be retained and accessible for backward compatibility. In the new UI, this functionality is exposed as a **"Project Directory" sub-tab** under the Executive Report tab (accessible via a toggle or a dedicated sub-tab). Alternatively, a small "Directory" quick-export button is available in the Hub header when no specific report is being edited. The old `ReportViewModel` is removed; its logic is subsumed into the Reporting Hub's `ExecutiveReportViewModel` or a lightweight `DirectoryExportViewModel`.
- The existing nav ID `"reports"` must be kept; only the view factory and studio flag change.

---

## Phase 8 Product Shape

Phase 8 should feel like the natural cross-studio reporting companion to all prior studios.

The user should be able to:

- Click "Reports" in the navigation rail and see a 7-tab Reporting Hub workspace (requires an active project)
- Open the Daily Report tab, pick a date, and see auto-populated sections: activities started/completed today, workforce on site, equipment deployed, issues/remarks
- Click "Generate AI Narrative" and see a contextual daily summary paragraph
- Attach site photos from the project documents library
- Export the daily report to Excel, PDF, or Word
- Open the Weekly Report tab, pick a week, and see progress by WBS, resource usage summary, delay items, look-ahead for next 2 weeks
- Open the Monthly Report tab and see EVM metric cards (CPI, SPI, CV, SV), an S-Curve chart, budget vs actual table, and a 3-4 paragraph AI narrative
- Open the Executive Report tab and see high-level KPI cards, S-Curves, risk highlights, financial summary, and an AI executive narrative
- Navigate to the Schedule tab and configure daily/weekly/monthly auto-generation with time-of-day and export format preferences
- View the History tab to see all previously generated reports, filter by type/status/date range, and re-export any report
- Open the Templates tab and reorder sections or toggle visibility per report type
- Open the Settings tab and configure which content sections appear in each report type — e.g., hide "Equipment" from Daily or add "Weather" to Daily, independently per report type
- In Settings, define the project's parties: Client name with logo, Main Contractor name with logo, and one or more Sub Contractors each with name and logo
- See all report headers auto-populated with the Client and Main Contractor logos and names; Sub Contractors listed in a dedicated section
- Have all reports available in English or Arabic based on the current UI language

---

## Data Model

### New Module: `Planova.Reporting`

Following the same structure as `Planova.Cost`:

```
Planova.Reporting/
  Domain/
    Entities/
      ReportTemplate.cs
      ReportInstance.cs
      ReportSection.cs
      ReportSchedule.cs
      ReportExport.cs
      ProjectParty.cs
    Enums/
      ReportType.cs
      ReportStatus.cs
      ReportSectionType.cs
      ScheduleFrequency.cs
      ExportFormat.cs
      PartyRole.cs
    Interfaces/
      IReportTemplateRepository.cs
      IReportInstanceRepository.cs
      IReportScheduleRepository.cs
      IReportEngine.cs
      IReportDataProvider.cs
      IReportSchedulerService.cs
      IReportExportService.cs
      IReportAiService.cs
      IProjectPartyRepository.cs
      IProjectPartyService.cs
      IReportSettingsService.cs
  Application/
    Dto/
      ReportTemplateDto.cs
      ReportInstanceDto.cs
      ReportScheduleDto.cs
      ReportSectionDto.cs
      ReportExportDto.cs
      ProjectPartyDto.cs
      ReportSectionConfigDto.cs
      ReportSettingsDto.cs
      DailyReportDataDto.cs
      WeeklyReportDataDto.cs
      MonthlyReportDataDto.cs
      ExecutiveReportDataDto.cs
      ReportDataDto.cs
    Services/
      ReportEngine.cs
      ReportSchedulerService.cs
      ReportExportService.cs
      ReportAiService.cs
      ProjectPartyService.cs
      ReportSettingsService.cs
    DataProviders/
      DailyReportDataProvider.cs
      WeeklyReportDataProvider.cs
      MonthlyReportDataProvider.cs
      ExecutiveReportDataProvider.cs
    Mappings/
      ReportingMappingProfile.cs
  Background/
    ReportGenerationHostedService.cs
  Extensions/
    ServiceCollectionExtensions.cs
```

---

### Entity: ReportTemplate

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int?` | No | null = global template |
| `ReportType` | `ReportType` | Yes | Daily, Weekly, Monthly, Executive |
| `Name` | `string(100)` | Yes | e.g. "Standard Daily Report" |
| `Description` | `string(500)?` | No | |
| `IsDefault` | `bool` | Yes | Default template for this type per project |
| `LayoutJson` | `string` | Yes | Section ordering and layout config as JSON array |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique constraint:** Filtered unique index on `(ProjectId, ReportType)` WHERE `IsDefault = 1` — only one default per type per project.

---

### Entity: ReportInstance

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ReportType` | `ReportType` | Yes | Daily, Weekly, Monthly, Executive |
| `TemplateId` | `Guid?` | No | FK → ReportTemplate |
| `Title` | `string(200)` | Yes | e.g. "Weekly Report — Week 24, 2026" |
| `Status` | `ReportStatus` | Yes | Draft, Final, Archived |
| `PeriodStart` | `DateTime` | Yes | Report period start |
| `PeriodEnd` | `DateTime` | Yes | Report period end |
| `GeneratedAt` | `DateTime` | Yes | |
| `GeneratedBy` | `string(100)?` | No | |
| `DataSnapshotJson` | `string` | Yes | Frozen report data at generation time |
| `AiNarrative` | `string(5000)?` | No | AI-generated narrative text |
| `Notes` | `string(1000)?` | No | User-added notes |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Index:** `(ProjectId, ReportType, PeriodStart)`, `(ProjectId, Status)`.

**Snapshot model (authoritative source vs derived views):**
- `DataSnapshotJson` is the **single authoritative source** — it stores the complete frozen report data at generation time as serialized JSON of the typed DTO (e.g., `DailyReportDataDto`). This is the source of truth for all report viewing and re-export.
- `ReportSections` (the collection of `ReportSection` records) are **derived, denormalized views** of the snapshot, generated at creation time for efficient UI display (e.g., showing a section list without deserializing the full JSON). They can be regenerated from `DataSnapshotJson` if lost or corrupted.
- `AiNarrative` is a **convenience field** on `ReportInstance` that duplicates the AI narrative text stored within `DataSnapshotJson`. It exists to avoid full snapshot deserialization when only the narrative text is needed (e.g., History list preview). It is always written in sync with the snapshot.

---

### Entity: ReportSection

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ReportInstanceId` | `Guid` | Yes | FK → ReportInstance |
| `SectionType` | `ReportSectionType` | Yes | Text, Table, Chart, Image, AiNarrative |
| `Title` | `string(200)` | Yes | Section heading |
| `OrderIndex` | `int` | Yes | Display ordering |
| `ContentJson` | `string` | Yes | Section data serialized to JSON |
| `CreatedAt` | `DateTime` | Yes | |

---

### Entity: ReportSchedule

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ReportType` | `ReportType` | Yes | Daily, Weekly, Monthly |
| `TemplateId` | `Guid?` | No | FK → ReportTemplate |
| `Frequency` | `ScheduleFrequency` | Yes | Daily, Weekly, Monthly |
| `DayOfWeek` | `int?` | No | For Weekly — 0=Sunday..6=Saturday |
| `DayOfMonth` | `int?` | No | For Monthly — 1..31 |
| `TimeOfDay` | `TimeSpan` | Yes | Generation time. Interpreted in the timezone specified by `TimeZoneId` (default UTC). |
| `TimeZoneId` | `string(100)` | Yes | IANA or Windows timezone ID, e.g. `"UTC"` or `"Arab Standard Time"`. Defaults to UTC. |
| `ExportFormats` | `string` | Yes | JSON array of format strings, e.g. `["excel","pdf","docx"]`. |
| `RecipientEmails` | `string(500)?` | No | For future email delivery |
| `IsActive` | `bool` | Yes | |
| `LastRunAt` | `DateTime?` | No | Most recent generation attempt start time (UTC). |
| `LastStatus` | `string(20)?` | No | `"Success"`, `"Failed"`, or `"Skipped"` from the last run. |
| `LastErrorMessage` | `string(2000)?` | No | Exception message from the last failed run. |
| `LastSuccessfulRunAt` | `DateTime?` | No | Timestamp of the last successful generation (UTC). |
| `RetryCount` | `int` | Yes | Number of consecutive failures since last success. Reset to 0 on success. |
| `MaxRetries` | `int` | Yes | Max retries before auto-deactivating. Default 3. |
| `NextRunAt` | `DateTime` | Yes | Pre-computed next scheduled run in UTC. |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique constraint:** `(ProjectId, ReportType)` — one schedule per type per project.

---

### Entity: ReportExport

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ReportInstanceId` | `Guid` | Yes | FK → ReportInstance |
| `Format` | `ExportFormat` | Yes | Excel, PDF, Word |
| `FilePath` | `string(1000)` | Yes | Absolute path to exported file |
| `FileSizeBytes` | `long` | Yes | |
| `ExportedAt` | `DateTime` | Yes | |
| `ExportedBy` | `string(100)?` | No | |

---

### Entity: ProjectParty

Defines the parties involved in a project for report headers and footers. Each project can have one Client, one Main Contractor, and multiple Sub Contractors.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `Role` | `PartyRole` | Yes | Client, MainContractor, SubContractor |
| `Name` | `string(200)` | Yes | Legal name of the party |
| `LogoPath` | `string(1000)?` | No | Path to uploaded logo image file |
| `Address` | `string(500)?` | No | Registered address |
| `ContactPerson` | `string(100)?` | No | |
| `ContactEmail` | `string(100)?` | No | |
| `ContactPhone` | `string(50)?` | No | |
| `DisplayOrder` | `int` | Yes | Ordering for Sub Contractors |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique constraint:** Filtered unique index on `(ProjectId, Role)` WHERE `Role IN (0, 1)` (i.e., Client and MainContractor) — at most one Client and one MainContractor per project. SubContractor records have no uniqueness constraint on Role; multiple SubContractors are allowed per project.

---

### Entity: ReportSettings

Stored as a dedicated table (not a JSON column on Project). Each row defines which content sections are enabled for a specific report type within a project. This avoids schema coupling with the Project entity and allows independent querying.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ReportType` | `ReportType` | Yes | Daily, Weekly, Monthly, Executive |
| `EnabledSectionsJson` | `string` | Yes | JSON array of enabled section identifiers, e.g. `["projectInfo","progressToday","workforce","equipment","issues","photos","aiNarrative"]` |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique constraint:** `(ProjectId, ReportType)` — one settings row per report type per project.

**Default enabled sections** (all sections enabled by default):

| Report Type | Default Enabled Sections |
|-------------|------------------------|
| Daily | Project Info, Progress Today, Workforce, Equipment, Issues, Photos, AI Narrative |
| Weekly | Project Info, Progress by WBS, Resource Usage, Delays, Look-Ahead, Photos, AI Narrative |
| Monthly | Project Info, EVM Summary, S-Curve, Budget vs Actual, Progress, Resource Productivity, AI Narrative |
| Executive | Executive Summary, KPI Dashboard, S-Curves, Risk Highlights, Financial Overview, Milestone Status, AI Executive Narrative |

---

### Canonical Section Identifiers

These string constants are the single source of truth used in `EnabledSectionsJson`, `ReportTemplate.LayoutJson`, and all UI checkbox bindings. Every section identifier is a PascalCase string with no spaces.

| Identifier | Display Name | Applies To | Description |
|-----------|-------------|------------|-------------|
| `ProjectInfo` | Project Info | All | Project name, code, period, client/contractor party logos |
| `ProgressToday` | Progress Today | Daily | Activities started/completed/in-progress today |
| `Workforce` | Workforce | Daily | Labour count by trade, total hours |
| `Equipment` | Equipment | Daily | Equipment deployed by type and count |
| `Issues` | Issues | Daily | User-entered issues and remarks |
| `ProgressByWbs` | Progress by WBS | Weekly, Monthly | WBS items with % complete and variance |
| `ResourceUsage` | Resource Usage | Weekly | Labour/equipment/material totals for the week |
| `Delays` | Delays | Weekly | Activities with negative float or behind schedule |
| `LookAhead` | Look-Ahead | Weekly | Activities scheduled for the next 2 weeks |
| `EvmSummary` | EVM Summary | Monthly | CPI, SPI, CV, SV, EAC, ETC, VAC metric cards |
| `SCurve` | S-Curve | Monthly, Executive | Cumulative planned vs actual cost chart |
| `BudgetVsActual` | Budget vs Actual | Monthly | Budget breakdown compared to actual costs |
| `ResourceProductivity` | Resource Productivity | Monthly | Resource productivity metrics |
| `KpiDashboard` | KPI Dashboard | Executive | High-level KPI cards (CPI, SPI, Budget Health) |
| `RiskHighlights` | Risk Highlights | Executive | Top delayed activities and critical path risks |
| `FinancialOverview` | Financial Overview | Executive | Total budget, spent, remaining, EAC |
| `MilestoneStatus` | Milestone Status | Executive | Key milestone dates and status |
| `SubContractors` | Sub Contractors | All | List of subcontractors defined in project parties |
| `ExecutiveSummary` | Executive Summary | Executive | One-page high-level executive summary card |
| `Photos` | Photos | Daily, Weekly | Attached images from project documents |
| `AiNarrative` | AI Narrative | All | AI-generated narrative text (type-specific length) |

These identifiers are defined as `public const string` on a static class `ReportSectionIds` in the `Planova.Reporting.Domain` layer. Settings, templates, and UI all reference these constants — never raw strings.

---

### Enums

```csharp
public enum ReportType { Daily, Weekly, Monthly, Executive }

public enum ReportStatus { Draft, Final, Archived }

public enum ReportSectionType { Text, Table, Chart, Image, AiNarrative }

public enum ScheduleFrequency { Daily, Weekly, Monthly }

public enum ExportFormat { Excel, Pdf, Word }

public enum PartyRole { Client, MainContractor, SubContractor }
```

---

## Report Engine Architecture

```
[User triggers generation] or [Scheduled timer fires]
        │
        ▼
[ReportEngine.GenerateAsync(projectId, reportType, periodStart, periodEnd)]
        │
        ├── 1. Resolve IReportDataProvider for ReportType
        │         (via DI — DailyReportDataProvider / WeeklyReportDataProvider / etc.)
        │
        ├── 2. DataProvider queries studio contracts:
        │         • IProjectService — project info, dates, location
        │         • IProjectPartyService — Client, Main Contractor, Sub Contractors
        │         • IReportSettingsService — enabled sections for this report type
        │         • IActivityService — activities, progress %, dates, delays
        │         • IResourceAssignmentService — resource usage
        │         • IWbsService — WBS breakdown
        │         • ICostService, IEvmService, ICashFlowService — cost metrics
        │         • IProjectDocumentService — attached photos/docs
        │
        ├── 3. Filter sections per ReportSettings.EnabledSectionsJson
        │         (skip disabled sections; only collect data for enabled ones)
        │
        ├── 4. Assemble ReportDataDto with only enabled sections
        │
        ├── 5. Generate AI narrative (if applicable):
        │         IReportAiService.GenerateNarrative(reportData, reportType)
        │         → Returns string narrative text
        │
        ├── 6. Create ReportInstance + ReportSections
        │         • DataSnapshotJson = serialize ReportDataDto
        │         • AiNarrative = copy of narrative text (convenience field)
        │         • Status = Draft
        │
        ├── 7. Export per schedule formats (or on-demand):
        │         IReportExportService.ExportAsync(instanceId, format)
        │         → Excel: ClosedXML workbook, sections as sheets
        │         → PDF: QuestPDF formatted document
        │         → DOCX: DocumentFormat.OpenXml document
        │
        └── 8. Return ReportInstanceDto to caller
```

### Data Provider Contracts

Data providers use a generic typed interface so each report type returns its own strongly-typed DTO, avoiding a polymorphic base with casting:

```csharp
public interface IReportDataProvider<TData> where TData : class
{
    ReportType HandledType { get; }
    Task<TData> CollectDataAsync(
        int projectId, DateTime periodStart, DateTime periodEnd,
        CancellationToken ct = default);
}
```

Concrete registrations:
```csharp
services.AddScoped<IReportDataProvider<DailyReportDataDto>, DailyReportDataProvider>();
services.AddScoped<IReportDataProvider<WeeklyReportDataDto>, WeeklyReportDataProvider>();
services.AddScoped<IReportDataProvider<MonthlyReportDataDto>, MonthlyReportDataProvider>();
services.AddScoped<IReportDataProvider<ExecutiveReportDataDto>, ExecutiveReportDataProvider>();
```

The `ReportEngine` resolves the correct provider by `ReportType` via a provider factory (which maintains a `Dictionary<ReportType, Type>` mapping). Serialization to `DataSnapshotJson` uses `System.Text.Json` with a type discriminator so deserialization knows the concrete DTO type.

---

## Report Type Data Definitions

### Daily Report Data (`DailyReportDataDto`)

| Section | Data Source | Content |
|---------|-------------|---------|
| Project Info | `IProjectService` | Name, code, date, weather (user input) |
| Progress Today | `IActivityService` | Activities started today, completed today, in-progress |
| Workforce | `IResourceAssignmentService` | Labour count by trade, total hours |
| Equipment | `IResourceAssignmentService` | Equipment deployed by type |
| Issues | User input | Issues/remarks text (saved as notes) |
| Photos | `IProjectDocumentService` | Attached images from project docs |
| AI Narrative | `IReportAiService` | 1-paragraph daily progress summary |

### Weekly Report Data (`WeeklyReportDataDto`)

| Section | Data Source | Content |
|---------|-------------|--------|
| Project Info | `IProjectService` | Name, code, week number |
| Progress by WBS | `IWbsService`, `IActivityService` | WBS items with % complete, variance |
| Resource Usage | `IResourceAssignmentService` | Labour/equipment/material totals this week |
| Delays | `IActivityService` | Activities with negative float or behind schedule |
| Look-Ahead | `IActivityService` | Activities scheduled for next 2 weeks |
| Photos | `IProjectDocumentService` | Attached images |
| AI Narrative | `IReportAiService` | 2-paragraph weekly status summary |

### Monthly Report Data (`MonthlyReportDataDto`)

| Section | Data Source | Content |
|---------|-------------|--------|
| Project Info | `IProjectService` | Name, code, month/year |
| EVM Summary | `IEvmService` | CPI, SPI, CV, SV, EAC, ETC, VAC metric cards |
| S-Curve | `ICashFlowService` | Cumulative planned vs actual cost data points |
| Budget vs Actual | `ICostService` | Budget breakdown vs actual costs |
| Progress | `IActivityService`, `IWbsService` | % complete by WBS, milestone status |
| Resource Productivity | `IResourceAssignmentService` | Productivity metrics |
| AI Narrative | `IReportAiService` | 3-4 paragraph detailed status narrative |

### Executive Report Data (`ExecutiveReportDataDto`)

| Section | Data Source | Content |
|---------|-------------|--------|
| Executive Summary | Composite | One-page high-level summary |
| KPI Dashboard | `IEvmService`, `ICostService` | CPI, SPI, Earned vs Planned, Budget Health |
| S-Curves | `ICashFlowService` | Planned vs Actual vs Earned (S-Curve) |
| Risk Highlights | `IActivityService` | Top delayed activities, critical path status |
| Financial Overview | `ICostService` | Total budget, spent, remaining, EAC |
| Milestone Status | `IActivityService` | Key milestone dates and status |
| AI Executive Narrative | `IReportAiService` | 3-5 paragraph executive-level narrative |

---

## AI Narrative Service

All four AI features are implemented in `ReportAiService` via `IAIProvider` (Semantic Kernel).

### 1. Daily Narrative
- Input: Date, activities started/completed, workforce count, issues
- Output: 1 paragraph professional daily summary
- Prompt focus: conciseness, factual reporting, issue highlighting

### 2. Weekly Narrative
- Input: Week progress by WBS, resource totals, delay items, look-ahead
- Output: 2 paragraph weekly status
- Prompt focus: trend identification, delay reasoning, look-ahead confidence

### 3. Monthly Narrative
- Input: EVM metrics, budget variance, progress, resource productivity
- Output: 3-4 paragraph detailed status
- Prompt focus: EVM analysis, budget health, performance trends, recommendations

### 4. Executive Narrative
- Input: All executive KPIs, S-curve trends, risk items, financial overview
- Output: 3-5 paragraph executive summary
- Prompt focus: strategic overview, key decisions, risk mitigation, outlook

**UI behavior:**
- AI narrative is generated on demand (button: "Generate AI Narrative")
- Narrative appears in an editable text box
- User can edit the text, regenerate it, or accept it
- AI unavailability shows a message and disables the button
- All AI output is clearly labeled "AI-Generated"

---

## Schedule Engine

**Scheduler execution model:** The scheduler runs as an in-process `IHostedService` inside the WPF desktop application. This means the application must be running for scheduled reports to generate. There is no external background worker or Windows Service. If the app is closed, missed schedules are caught up on next launch (within `MaxRetries` limit). This is acceptable for v1 — an out-of-process scheduler can be added in Phase 19 (Enterprise Edition).

### ReportGenerationHostedService

```csharp
public class ReportGenerationHostedService : IHostedService, IDisposable
{
    // On StartAsync:
    //   1. Query all schedules where IsActive == true
    //   2. For each schedule where NextRunAt <= UTC now, run generation
    //   3. Start timer with 60-second interval

    // On Timer tick:
    //   1. Query schedules where IsActive == true AND NextRunAt <= UTC now
    //   2. For each: acquire per-schedule lock; call ReportEngine.GenerateAsync
    //   3. If success: set LastRunAt=now, LastSuccessfulRunAt=now, LastStatus="Success",
    //      RetryCount=0, compute NextRunAt
    //   4. If failure: set LastStatus="Failed", LastErrorMessage=ex.Message,
    //      RetryCount++. If RetryCount >= MaxRetries: set IsActive=false, log warning
    //   5. Release lock
}
```

### NextRunAt Computation

`NextRunAt` is always stored in UTC. The `TimeOfDay` and `TimeZoneId` fields are used at computation time:

1. Determine "today" in the schedule's timezone (e.g., `TimeZoneInfo.ConvertTimeFromUtc(utcNow, tzInfo)` for Windows zones, or `TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId)`).
2. Compute the next candidate DateTime as `today.Date + TimeOfDay` (in local time).
3. If the candidate is in the past, advance to the next period (next day/week/month) and recompute.
4. Convert the candidate back to UTC for storage as `NextRunAt`.
5. If the `TimeZoneId` changes (e.g., user relocates), schedules auto-adjust on the next compute cycle.

```csharp
// Daily: next occurrence of TimeOfDay in the schedule's timezone
// Weekly: next occurrence of DayOfWeek at TimeOfDay in the schedule's timezone
// Monthly: next occurrence of DayOfMonth at TimeOfDay in the schedule's timezone
// All: if computed local time is in the past, advance to next period
```

---

## Export Service

### Export File Lifecycle & Cleanup

**Storage path:** `{AppData}/Planova/Projects/{projectId}/Reports/{instanceId}/{format}/report.{ext}`

**Creation:** Each call to `IReportExportService.ExportAsync` generates a file on disk and inserts a `ReportExport` record. Re-exporting the same instance+format overwrites the existing file and updates the `ReportExport` record (no duplicate files).

**Deletion behavior:**
- Deleting a `ReportInstance` (soft delete or hard delete) **does not** delete export files from disk — files remain for audit trail. The `ReportExport` record is soft-deleted (Status = Archived) so the file path is preserved but hidden from the active UI.
- Explicit "Delete Export" from the History tab **does** delete the file from disk and removes the `ReportExport` record. User confirms this action ("This will permanently delete the exported file").
- Deleting a `Project` cascades: all report files under the project's Reports folder are deleted from disk, and all associated `ReportInstance`, `ReportSection`, `ReportSchedule`, `ReportExport`, `ReportSettings`, and `ProjectParty` records are hard-deleted.

**Cleanup rules (background maintenance):**
- Orphaned export files (no matching `ReportExport` record) are cleaned up on application startup via a startup check.
- Report instances with Status = Archived that are older than 365 days may be purged (configurable in Reporting Settings). Purge deletes both DB records and disk files.

### Excel Export (ClosedXML)
- Reuses Phase 2 `IWorkbookWriter` infrastructure
- One workbook per report
- Each `ReportSection` becomes a worksheet
- Charts rendered as data tables (LiveCharts2 not embedded in Excel)
- Consistent styling with Planova brand colors

### PDF Export (QuestPDF)
- Full formatted document with:
  - Cover page with project name, report type, period, date
  - Table of contents
  - Sectioned content with headings, tables, text
  - Embedded S-Curve chart images (rendered from LiveCharts2)
  - AI narrative in styled text blocks
  - Footer with page numbers, generated date

### Word Export (DOCX via DocumentFormat.OpenXml)
- Uses `DocumentFormat.OpenXml` NuGet package (the official .NET SDK for Office OpenXML)
- Narrative-focused format: headings, body paragraphs, simple tables, bold/italic styling
- No embedded charts (data tables only, as plain Word tables)
- Suitable for further editing in Microsoft Word
- File extension `.docx`

---

## UI Structure — Reporting Hub

```
ReportingHubView (main container with TabControl)
  ├── Tab: Daily Report
  │     DailyReportView
  │     ├── Header: Client logo + name (left), Main Contractor logo + name (right)
  │     ├── Period: DatePicker (defaults to today)
  │     ├── Auto-populated sections (read-only preview)
  │     │     ├── Project Info card
  │     │     ├── Progress Today table (activities started/completed)
  │     │     ├── Workforce table (trade / count / hours)
  │     │     ├── Equipment table (type / count)
  │     │     ├── Issues text box (editable)
  │     │     └── Photos gallery (from project documents)
  │     ├── AI Narrative section
  │     │     ├── [Generate Narrative] button
  │     │     ├── Editable text box with narrative text
  │     │     └── [Regenerate] button
  │     ├── [Save Draft] [Generate Final] buttons
  │     └── Export: [Excel] [PDF] [Word]
  │
  ├── Tab: Weekly Report
  │     WeeklyReportView
  │     ├── Header: Client logo + name (left), Main Contractor logo + name (right)
  │     ├── Period: Week picker (week selector or date range)
  │     ├── Sections:
  │     │     ├── Project Info card
  │     │     ├── Progress by WBS tree table
  │     │     ├── Resource Usage summary table
  │     │     ├── Delay Items table (activity, delay days, reason)
  │     │     ├── Look-Ahead table (next 2 weeks)
  │     │     ├── Sub Contractors list
  │     │     └── Photos gallery
  │     ├── AI Narrative section
  │     └── Export: [Excel] [PDF] [Word]
  │
  ├── Tab: Monthly Report
  │     MonthlyReportView
  │     ├── Header: Client logo + name (left), Main Contractor logo + name (right)
  │     ├── Period: Month/Year picker
  │     ├── Sections:
  │     │     ├── Project Info card
  │     │     ├── EVM Metric Cards (CPI, SPI, CV, SV, EAC, ETC, VAC) — color-coded
  │     │     ├── S-Curve chart (LiveCharts2 — reuse from Phase 7)
  │     │     ├── Budget vs Actual table
  │     │     ├── Progress by WBS table
  │     │     ├── Resource Productivity table
  │     │     └── Sub Contractors list
  │     ├── AI Narrative section
  │     └── Export: [Excel] [PDF] [Word]
  │
  ├── Tab: Executive Report
  │     ExecutiveReportView
  │     ├── Header: Client logo + name (left), Main Contractor logo + name (right)
  │     ├── Period: Date range picker
  │     ├── Sections:
  │     │     ├── Executive Summary card (preview text)
  │     │     ├── KPI Dashboard cards (CPI, SPI, Budget Health, EAC)
  │     │     ├── S-Curve chart (LiveCharts2)
  │     │     ├── Risk Highlights table
  │     │     ├── Financial Overview table
  │     │     ├── Milestone Status table
  │     │     └── Sub Contractors list
  │     ├── AI Narrative section (executive style)
  │     └── Export: [Excel] [PDF] [Word]
  │
  ├── Tab: Schedule
  │     ReportScheduleView
  │     ├── Schedule grid: Type | Frequency | Time | Last Run | Next Run | Active
  │     ├── [Add Schedule] button → dialog
  │     │     ├── Report Type (Daily/Weekly/Monthly)
  │     │     ├── Frequency details (day of week/month, time)
  │     │     ├── Export formats (checkboxes: Excel, PDF, Word)
  │     │     └── Is Active toggle
  │     ├── Inline toggle: Active/Inactive per schedule
  │     └── Status indicators (green = on track, yellow = missed, red = failed)
  │
  ├── Tab: History
  │     ReportHistoryView
  │     ├── Filter bar: Report Type dropdown | Date range | Status dropdown
  │     ├── Grid: Title | Type | Period | Status | Generated | Actions
  │     │     ├── [Open] → preview the report
  │     │     ├── [Export] → dropdown (Excel/PDF/Word)
  │     │     ├── [Archive] / [Delete]
  │     │     └── Status badge (Draft/Final/Archived)
  │     └── [Export All] button
  │
  ├── Tab: Templates
  │     ReportTemplateEditorView
  │     ├── Report Type selector (Daily/Weekly/Monthly/Executive)
  │     ├── Section list with drag-reorder handle
  │     │     ├── Section name
  │     │     ├── Visibility toggle (eye icon)
  │     │     └── Up/Down reorder buttons (or drag)
  │     └── [Save as Default] [Reset to Default] buttons
  │
  └── Tab: Settings
        ReportSettingsView
        ├── Section: Report Content Configuration
        │     ├── Report Type tabs (Daily / Weekly / Monthly / Executive)
        │     ├── For each report type: checkbox list of all available sections
        │     │     ├── ☑ Project Info
        │     │     ├── ☑ Progress Today (Daily only)
        │     │     ├── ☑ Workforce (Daily only)
        │     │     ├── ☑ Equipment
        │     │     ├── ☑ Issues (Daily only)
        │     │     ├── ☑ Photos
        │     │     ├── ☑ AI Narrative
        │     │     ├── ☑ Progress by WBS (Weekly/Monthly)
        │     │     ├── ☑ Resource Usage (Weekly only)
        │     │     ├── ☑ Delays (Weekly only)
        │     │     ├── ☑ Look-Ahead (Weekly only)
        │     │     ├── ☑ EVM Summary (Monthly only)
        │     │     ├── ☑ S-Curve (Monthly/Executive)
        │     │     ├── ☑ Budget vs Actual (Monthly only)
        │     │     ├── ☑ Resource Productivity (Monthly only)
        │     │     ├── ☑ KPI Dashboard (Executive only)
        │     │     ├── ☑ Risk Highlights (Executive only)
        │     │     ├── ☑ Financial Overview (Executive only)
        │     │     ├── ☑ Milestone Status (Executive only)
        │     │     └── ☑ Sub Contractors
        │     ├── [Restore Defaults] button per report type
        │     └── [Save] button
        │
        └── Section: Project Parties
              ├── Client card
              │     ├── Logo preview (thumbnail) + [Upload Logo] button
              │     ├── Name: TextBox
              │     ├── Address: TextBox
              │     ├── Contact Person: TextBox
              │     └── Contact Email / Phone: TextBox
              ├── Main Contractor card (same fields as Client)
              ├── Sub Contractors list
              │     ├── [Add Sub Contractor] button
              │     ├── Each Sub Contractor card:
              │     │     ├── Logo preview + [Upload Logo]
              │     │     ├── Name: TextBox
              │     │     ├── Address: TextBox
              │     │     ├── Contact fields
              │     │     ├── Up/Down reorder buttons
              │     │     └── [Remove] button
              │     └── Drag-reorder for Sub Contractor cards
              └── [Save Parties] button
```

---

## Navigation Shell Integration

### Replace existing "Reports" nav target

**Current** (line 314 of `ShellViewModel.cs`):
```csharp
nav.RegisterTarget("reports", "Reports", "DocumentText24", false, false,
    () => _serviceProvider.GetRequiredService<ReportView>());
```

**After Phase 8:**
```csharp
nav.RegisterTarget("reports", "Reports", "DocumentText24", true, false,
    () => _serviceProvider.GetRequiredService<ReportingHubView>());
```

Key changes:
- `isStudio`: `false` → `true` (project-gated)
- View factory: `ReportView` → `ReportingHubView`

### Add "reports" to `_studioTargetIds`

```csharp
private readonly List<string> _studioTargetIds = new()
{ "reports", "boq", "wbs", "activity", "resource", "cost", "excel-studio",
  "primavera", "schedule-compare", "delay-analysis", "chronology",
  "knowledge-base", "integration-hub" };
```

This ensures the Reports tab is disabled when no project is active.

### DI Registration

**New — in `Planova.Reporting.Extensions.ServiceCollectionExtensions`:**
```csharp
public static IServiceCollection AddPlanovaReporting(this IServiceCollection services)
{
    // Services
    services.AddScoped<IReportEngine, ReportEngine>();
    services.AddScoped<IReportSchedulerService, ReportSchedulerService>();
    services.AddScoped<IReportExportService, ReportExportService>();
    services.AddScoped<IReportAiService, ReportAiService>();
    services.AddScoped<IProjectPartyService, ProjectPartyService>();
    services.AddScoped<IReportSettingsService, ReportSettingsService>();

    // Data Providers
    services.AddScoped<DailyReportDataProvider>();
    services.AddScoped<WeeklyReportDataProvider>();
    services.AddScoped<MonthlyReportDataProvider>();
    services.AddScoped<ExecutiveReportDataProvider>();

    // Repositories
    services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>();
    services.AddScoped<IReportInstanceRepository, ReportInstanceRepository>();
    services.AddScoped<IReportScheduleRepository, ReportScheduleRepository>();
    services.AddScoped<IProjectPartyRepository, ProjectPartyRepository>();

    // Background
    services.AddHostedService<ReportGenerationHostedService>();

    return services;
}
```

**In `App.xaml.cs`:**
```csharp
// Add with other module registrations
services.AddPlanovaReporting();

// Register Reporting UI ViewModels and Views (replace old)
services.AddTransient<ReportingHubViewModel>();
services.AddTransient<ReportingHubView>();
services.AddTransient<DailyReportViewModel>();
services.AddTransient<DailyReportView>();
services.AddTransient<WeeklyReportViewModel>();
services.AddTransient<WeeklyReportView>();
services.AddTransient<MonthlyReportViewModel>();
services.AddTransient<MonthlyReportView>();
services.AddTransient<ExecutiveReportViewModel>();
services.AddTransient<ExecutiveReportView>();
services.AddTransient<ReportScheduleViewModel>();
services.AddTransient<ReportScheduleView>();
services.AddTransient<ReportHistoryViewModel>();
services.AddTransient<ReportHistoryView>();
services.AddTransient<ReportTemplateEditorViewModel>();
services.AddTransient<ReportTemplateEditorView>();
services.AddTransient<ReportSettingsViewModel>();
services.AddTransient<ReportSettingsView>();
services.AddTransient<ProjectPartyViewModel>();

// Retain old IReportService for backward compat (entity lists)
// Old ReportView/ViewModel are REMOVED — no longer registered
```

---

## Solution Structure Changes

### New project: `Planova.Reporting`
- **Direct references:** `Planova.Shared` (common DTOs, base types), `Planova.Domain` (Project entity reference)
- **Contract-only references (no broad project deps):** References only the *interface/contract projects* or *specific abstraction namespaces* from:
  - `Planova.Activity` → `IActivityService` (via a contracts abstraction — or references the Application layer's `Services` namespace only, not the full Activity domain)
  - `Planova.Resource` → `IResourceAssignmentService` (same pattern)
  - `Planova.Cost` → `ICostService`, `IEvmService`, `ICashFlowService` (same pattern)
  - Shared abstractions: `IAIProvider` (from `Planova.AI` or the `Planova.Shared.Abstractions` if extracted)
- **Infrastructure references:** `Planova.Excel` (for `IWorkbookWriter`), `DocumentFormat.OpenXml` (for DOCX export), `QuestPDF` (for PDF export)
- **No direct references to:** `Planova.Persistence`, `Planova.UI`, or any EF Core NuGet packages
- **No UI references** — UI only references service interfaces from `Planova.Reporting.Application`

### `Planova.Persistence` changes
- New entity configurations: `ReportTemplateConfiguration`, `ReportInstanceConfiguration`, `ReportSectionConfiguration`, `ReportScheduleConfiguration`, `ReportExportConfiguration`, `ProjectPartyConfiguration`
- New repositories: `ReportTemplateRepository`, `ReportInstanceRepository`, `ReportScheduleRepository`, `ProjectPartyRepository`
- New EF Core migration: `AddReportingCenterEntities`
- Register new DbSets in `PlanovaDbContext` — ReportTemplates, ReportInstances, ReportSections, ReportSchedules, ReportExports, ProjectParties
- Logo storage: `{AppData}/Planova/Projects/{projectId}/Parties/` — copied on upload

### `Planova.UI` changes
- New folder: `Views/Reporting/` — all Reporting Hub XAML views
- New folder: `ViewModels/Reporting/` — all Reporting ViewModels
- `ShellViewModel.cs` — replace `ReportView` with `ReportingHubView`, change `isStudio` to `true`, add `"reports"` to `_studioTargetIds`
- `App.xaml.cs` — register all Reporting ViewModels/Views, call `services.AddPlanovaReporting()`, remove old `ReportView`/`ReportViewModel` registrations

---

## Workstream Breakdown

### Workstream A: Domain Module Setup
1. Create `Planova.Reporting` project with folder structure
2. Define all enums: `ReportType`, `ReportStatus`, `ReportSectionType`, `ScheduleFrequency`, `ExportFormat`
3. Define domain entities: `ReportTemplate`, `ReportInstance`, `ReportSection`, `ReportSchedule`, `ReportExport`
4. Define all repository interfaces
5. Define all service interfaces: `IReportEngine`, `IReportDataProvider`, `IReportSchedulerService`, `IReportExportService`, `IReportAiService`
6. Add project references to solution

### Workstream B: EF Core Persistence
1. Add DbSets for all Reporting entities to `PlanovaDbContext`
2. Implement entity configurations (all 5 entities)
3. Implement repositories
4. Generate EF Core migration `AddReportingCenterEntities`
5. Verify migration runs clean against existing database

### Workstream C: Data Providers
1. Define `IReportDataProvider` contract interface
2. Implement `DailyReportDataProvider` — queries activities, resources, project info
3. Implement `WeeklyReportDataProvider` — queries progress by WBS, resource totals, delays, look-ahead
4. Implement `MonthlyReportDataProvider` — queries EVM, cash flow, cost, progress
5. Implement `ExecutiveReportDataProvider` — composites all data for executive view
6. Unit test each provider with mock studio services

### Workstream D: Report Engine
1. Implement `ReportEngine.GenerateAsync` — resolve provider, collect data, generate AI narrative, create instance, export
2. Implement data snapshot serialization (freeze report data at generation time)
3. Implement section assembly from template layout
4. Implement draft → final → archived status workflow

### Workstream E: Export Service
1. Implement `IReportExportService.ExportToExcelAsync` — ClosedXML workbook, sections as sheets
2. Implement `IReportExportService.ExportToPdfAsync` — QuestPDF formatted document with:
   - Cover page: Client logo + name (left), Main Contractor logo + name (right), project title
   - Sub Contractors listed on cover page
   - Sectioned content with headings, tables, chart images
   - Footer with page numbers
3. Implement `IReportExportService.ExportToWordAsync` — QuestPDF or OpenXML document
4. File storage to `{AppData}/Planova/Projects/{projectId}/Reports/{instanceId}/`
5. Record `ReportExport` entry in database

### Workstream F: AI Narrative Service
1. Implement `IReportAiService.GenerateNarrativeAsync` with `IAIProvider`
2. Design and implement 4 prompt templates (Daily, Weekly, Monthly, Executive)
3. Implement structured narrative output and editable display
4. Implement regenerate/edit/accept workflow
5. Handle AI unavailability gracefully (disable button, show message)

### Workstream G: Schedule Engine
1. Implement `IReportSchedulerService` — CRUD for schedules, compute NextRunAt
2. Implement `ReportGenerationHostedService` — IHostedService with 60-second timer
3. On startup: run any missed schedules
4. Log generation results, handle failures per schedule

### Workstream H: UI — Reporting Hub Shell
1. Create `ReportingHubView.xaml` + `ReportingHubViewModel.cs` with 7-tab initialization
2. Register in `ShellViewModel.RegisterNavigationTargets()` — replace old `reports` target
3. Add `"reports"` to `_studioTargetIds`
4. Register all Reporting views/viewmodels in `App.xaml.cs`
5. Wire `services.AddPlanovaReporting()` extension method
6. Remove old `ReportView`/`ReportViewModel` registrations

### Workstream I: UI — Daily Report View
1. Date picker bound to `SelectedDate` property
2. Auto-populated read-only sections (Project Info, Progress, Workforce, Equipment, Issues)
3. Photos gallery (thumbnails from project documents)
4. AI Narrative section with Generate/Regenerate/Edit
5. Export buttons (Excel, PDF, Word)

### Workstream J: UI — Weekly Report View
1. Week picker (week number + year)
2. Progress by WBS tree table
3. Resource Usage summary table
4. Delay Items table
5. Look-Ahead table (next 2 weeks)
6. Photos gallery
7. AI Narrative section
8. Export buttons

### Workstream K: UI — Monthly Report View
1. Month picker (month + year)
2. EVM metric cards (color-coded: green/amber/red)
3. S-Curve chart (LiveCharts2, reuse from Phase 7 CashFlowView)
4. Budget vs Actual table
5. Progress by WBS table
6. AI Narrative section
7. Export buttons

### Workstream L: UI — Executive Report View
1. Date range picker
2. KPI Dashboard section (cards)
3. S-Curve chart (LiveCharts2)
4. Risk Highlights table
5. Financial Overview table
6. Milestone Status table
7. AI Executive Narrative section
8. Export buttons

### Workstream M: UI — Schedule Management
1. Schedule grid: Type | Frequency | Time | Last/Next Run | Active toggle
2. Add/Edit/Delete schedule dialogs
3. Active/Inactive inline toggle
4. Status indicators

### Workstream N: UI — Report History
1. Filter bar (Report Type, Date Range, Status)
2. Report list grid with Actions column
3. Open (preview), Export (dropdown), Archive, Delete actions
4. Export All button

### Workstream O: UI — Template Editor
1. Report Type selector
2. Section list with drag-reorder (or up/down buttons)
3. Visibility toggle per section
4. Save as Default / Reset to Default

### Workstream P: UI — Report Settings
1. Create `ReportSettingsView.xaml` + `ReportSettingsViewModel.cs`
2. Report Content Configuration section — report type tabs with per-section checkbox list
3. Load/save enabled sections via `IReportSettingsService`
4. Restore defaults button per report type
5. Preview summary showing which sections are enabled for each type

### Workstream Q: UI — Project Parties Manager
1. Create `ProjectPartyViewModel.cs` (shared component or inline in Settings)
2. Client card with logo upload (OpenFileDialog, filter images), name, address, contact fields
3. Main Contractor card (same layout)
4. Sub Contractors list with add/remove/reorder, each with logo upload and fields
5. Logo file handling — copy to `{AppData}/Planova/Projects/{projectId}/Parties/` on upload
6. Save all parties via `IProjectPartyService`
7. Report header/footer auto-renders parties from the data provider

### Workstream R: Domain — Project Parties & Report Settings
1. Define `ProjectParty` entity and `PartyRole` enum
2. Define `IProjectPartyRepository` and `IProjectPartyService` interfaces
3. Implement `ProjectPartyService` — CRUD per project, logo upload handling
4. Define `IReportSettingsService` — get/set enabled sections per report type per project
5. Implement `ReportSettingsService` — load/save JSON config
6. Implement `ProjectPartyConfiguration` and add `ProjectParties` DbSet
7. Implement `ProjectPartyRepository`
8. Generate EF Core migration update

### Workstream S: Localization
1. Add English resource strings for all Reporting Center screens (including Settings and Parties)
2. Add Arabic resource strings for all Reporting Center screens
3. Verify RTL layout on all Reporting views (especially Settings checkboxes and party cards)
4. Verify theme consistency (dark/light/high contrast)

### Workstream T: Testing
1. Unit test `ReportEngine` orchestration logic (mocked providers)
2. Unit test each `ReportDataProvider` data aggregation with mock services
3. Unit test `ReportScheduleService` next-run computation
4. Unit test `ReportExportService` core logic
5. Unit test `ReportAiService` with mock `IAIProvider`
6. Unit test domain entity validation
7. Unit test `ProjectPartyService` CRUD and logo handling
8. Unit test `ReportSettingsService` section enable/disable logic
9. Integration test persistence round-trip for all Reporting entities
10. Integration test full generation pipeline (mock data → instance → export)
11. UI smoke test all Reporting Hub tabs

---

## Database Index Strategy

| Table | Index | Type | Purpose |
|-------|-------|------|---------|
| ReportTemplates | `(ProjectId, ReportType)` | Non-clustered | Template lookup per project |
| ReportInstances | `(ProjectId, ReportType, PeriodStart)` | Non-clustered | Period-based queries |
| ReportInstances | `(ProjectId, Status)` | Non-clustered | History filtering |
| ReportSchedules | `(IsActive, NextRunAt)` | Non-clustered | Scheduler tick queries |
| ReportSchedules | `(ProjectId, ReportType)` | Unique | One schedule per type per project |
| ReportExports | `(ReportInstanceId, Format)` | Non-clustered | Export lookup |
| ProjectParties | `(ProjectId, Role)` WHERE Role IN (0,1) | Filtered unique | One Client + one MainContractor per project |
| ProjectParties | `(ProjectId, DisplayOrder)` | Non-clustered | Sub Contractor ordering |

---

## Existing Module Integration Points

| Existing Module | How Reporting Center Consumes |
|----------------|------------------------------|
| `Planova.Activity` | `IActivityService` — activities, progress %, dates, delays |
| `Planova.Resource` | `IResourceAssignmentService` — resource usage, labour/equipment counts |
| `Planova.Cost` | `ICostService` — budget, direct costs |
| `Planova.Cost` | `IEvmService` — EVM metrics (CPI, SPI, CV, SV, EAC, ETC, VAC) |
| `Planova.Cost` | `ICashFlowService` — S-curve data points |
| `Planova.Wbs` | `IWbsService` — WBS breakdown for progress reporting |
| `Planova.Domain` | `IProjectService` — project info, client, dates, status |
| `Planova.Excel` | `IWorkbookWriter` — Excel export (Phase 2 reuse) |
| `Planova.AI` | `IAIProvider` — AI narrative generation (Semantic Kernel) |
| `Planova.Localization` | `ReportResources.en.resx` / `.ar.resx` — localized strings |
| Phase 7' (Project Docs) | `IProjectDocumentService` — photos/documents for reports |
| Existing nav shell | Replace `ReportView` with `ReportingHubView` in nav registration |
| Per-studio report tabs | Unchanged — remain inside each studio workspace |
| **New: Planova.Reporting** | `IProjectPartyService` — Client, Main Contractor, Sub Contractors for report headers |
| **New: Planova.Reporting** | `IReportSettingsService` — per-report-type section visibility configuration |

---

## Risks and Mitigations

### Risk: Report data aggregation is slow for large projects (1000+ activities)
**Mitigation:** Data snapshot is collected once at generation time and stored as JSON. All provider queries run asynchronously in parallel. Show progress indicator during generation.

### Risk: AI narrative quality varies with provider/model
**Mitigation:** All AI output is clearly labeled "AI-Generated". User can edit before saving or regenerate. AI unavailability is handled gracefully (button disabled, message shown).

### Risk: S-Curve chart export to Excel requires image embedding
**Mitigation:** Excel export includes chart data as a table (not image). For PDF, render the LiveCharts2 chart to a bitmap and embed in the PDF.

### Risk: Scheduled generation fails (e.g., AI unavailable, DB locked)
**Mitigation:** Each schedule is independent with its own lock. Failures are logged; the schedule retries on the next tick. A "Failed" status shows in the Schedule view.

### Risk: Existing ReportView functionality (entity lists) will be removed
**Mitigation:** The entity-list functionality (Projects/Clients/Contracts summary) can be preserved as a "Project Directory" quick-export action or a supplementary tab. The old `IReportService` is retained for backward compatibility.

### Risk: Phase 7 (Cost Studio) may not be fully complete
**Mitigation:** Reporting Center depends on `IEvmService`, `ICashFlowService`, and `ICostService` interfaces. If implementations are incomplete, stub implementations (returning zero/sample data) are used for development; full integration when Phase 7 is stable.

### Risk: Party logo files may be missing, deleted, or very large
**Mitigation:** Logo files are copied to app-managed storage on upload. Missing logos are handled gracefully (show placeholder icon, skip from export). File size limited to 2MB; dimensions resized to 200x200 max on upload.

### Risk: User enables conflicting or incomplete section sets
**Mitigation:** Section toggles are independent and per-type; no hard dependencies between sections. A preview panel shows the resulting report layout with enabled sections highlighted and disabled sections greyed out.

---

## Acceptance Criteria

Phase 8 is complete when all of the following are true:

### Functional — Report Generation
- All 4 report types (Daily, Weekly, Monthly, Executive) generate with live project data from all relevant studios.
- Each report type displays correct sections with data aggregated from the appropriate studio services.
- Daily Report shows activities started/completed, workforce, equipment, issues, photos.
- Weekly Report shows progress by WBS, resource usage, delays, look-ahead.
- Monthly Report shows EVM metric cards, S-Curve chart, budget vs actual, AI narrative.
- Executive Report shows KPI dashboard, S-Curves, risk highlights, financial overview, executive narrative.

### Functional — AI & Export
- AI narrative generates for all 4 report types with regenerate and edit capability.
- Reports export to Excel (.xlsx), PDF (.pdf), and Word (.docx) with correct formatting.
- AI unavailability (provider down or timeout) does not crash the app — button is disabled with a message, generation proceeds without narrative.

### Functional — Schedule & Scheduler
- Report schedules can be created, edited, toggled active/inactive.
- Background service (`ReportGenerationHostedService`) auto-generates reports at configured schedule times while the app is running.
- Schedules respect their `TimeZoneId` — a schedule set to 08:00 Cairo time generates at the correct UTC offset.
- When the app starts after being closed during a scheduled window, the scheduler catches up overdue generation jobs (up to `MaxRetries` limit).
- After `MaxRetries` consecutive failures, the schedule auto-deactivates (`IsActive = false`), `LastStatus = "Failed"`, and a warning is logged.

### Functional — Settings & Parties
- Settings tab allows enabling/disabling individual sections per report type independently.
- Settings tab allows defining Client, Main Contractor, and Sub Contractors with names and logos.
- Party logos upload correctly (max 2MB, auto-resized to 200x200) and are stored in app-managed storage.
- Report headers display Client and Main Contractor logos and names correctly.
- Reports include Sub Contractors section when parties are configured.
- Section visibility settings persist between sessions and affect report generation (disabled sections are not collected or rendered).

### Functional — History & Templates
- Report history lists all generated reports with filter (type, date, status).
- Existing reports can be re-exported to any format at any time.
- Template editor allows reordering and toggling visibility of sections.

### Edge Cases — Missing / Empty Data
- A project with no activities still generates all 4 report types with empty sections and a note "No data available for this period."
- A project with no parties defined generates reports with a generic header (project name only) — no crash, no missing-logo errors.
- A project with no EVM data (cost studio not yet configured) shows EVM sections with zeros and a "Cost data not available" indicator.
- Report generation with all sections disabled in Settings produces an empty report with a message "All sections are disabled for this report type."
- Missing or deleted party logo files show a placeholder icon in the header and skip the logo from exports (no broken image links).

### Edge Cases — Concurrency & Idempotency
- Rapid double-click of "Generate Report" produces only one `ReportInstance` (debounced or locked).
- Scheduled generation of the same project+type+period within the same minute is idempotent — no duplicate instances.
- Concurrent generation requests for different projects/report types do not block each other.
- A schedule tick that fires while a previous generation for the same schedule is still running is skipped (per-schedule lock with timeout).

### Edge Cases — Settings / Template Drift
- If a `ReportTemplate` is deleted after a `ReportInstance` references it, the instance remains viewable and exportable (it uses its own `DataSnapshotJson`).
- If `EnabledSectionsJson` in `ReportSettings` references a section ID that no longer exists in the canonical list, it is silently ignored during generation (graceful degradation).
- Changing section visibility in Settings does not affect already-generated reports — it only applies to new generations.

### Edge Cases — Scheduler Failure Recovery
- If the database is unavailable during a scheduled tick, the failure is logged, `RetryCount` is incremented, and the schedule retries on the next tick.
- If a generation succeeds but export fails (e.g., disk full), the `ReportInstance` is saved (Status = Draft) and the failure is recorded in `LastStatus`/`LastErrorMessage`. The user can manually re-export from History.

### UI & Platform
- All Reporting Center screens render correctly in English and Arabic with RTL support.
- Navigation rail "Reports" item is project-gated (disabled when no project is active).
- Existing per-studio report tabs (BOQ, WBS, Resource, Cost) remain fully functional.
- All prior phases (0 through 7) remain fully functional.
- The implementation is Clean Architecture and MVVM compliant.

---

## Definition of Done

- `Planova.Reporting` module exists with all domain entities, service interfaces, and implementations.
- All repositories implemented and registered in `Planova.Persistence`.
- EF Core migration `AddReportingCenterEntities` runs successfully.
- All 4 data providers collect correct data from existing studio service contracts.
- AI narrative generation works for all 4 report types via `IAIProvider` with graceful fallback.
- Report scheduling engine auto-generates reports on configured schedules.
- Reports export correctly to Excel, PDF, and Word.
- Reporting Hub replaces the old "Reports" nav target in `ShellViewModel`.
- "Reports" nav item is project-gated (`isStudio: true` + in `_studioTargetIds`).
- All 7 Reporting Hub tabs are functional and navigable.
- Report Settings tab allows per-report-type section enable/disable with persistence.
- Project Parties (Client, Main Contractor, Sub Contractors) can be managed with name and logo per project.
- Report headers auto-render Client and Main Contractor logos and names from saved parties.
- Old `IReportService` (entity lists) is retained for backward compatibility.
- Localization complete for all new Reporting Center scope including Settings and Parties (EN + AR).
- All tests pass.
