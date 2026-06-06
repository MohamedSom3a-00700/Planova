# Planova Phase 7 Implementation Plan

**Phase**: 7 — Cost Studio

**Date**: 2026-06-06

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
[docs/PHASE_6_IMPLEMENTATION_PLAN.md](./PHASE_6_IMPLEMENTATION_PLAN.md),
[specs/008-resource-studio/data-model.md](../specs/008-resource-studio/data-model.md)

---

## Summary

Phase 7 delivers the **Cost Studio** — the sixth specialized studio in the Planova project controls workflow and the natural downstream consumer of Phase 6 (Resource Studio). This phase transforms resource-loaded activities into a complete cost management system covering cost loading, direct cost tracking, budget management with revisions and contingency, cash flow forecasting with S-Curve visualization, and full Earned Value Management (EVM).

The ultimate workflow progression:

```
BOQ (Phase 3) → WBS (Phase 4) → Activities (Phase 5) → Resources (Phase 6) → Cost Studio (Phase 7)
```

Cost Studio reads resource assignment costs from Phase 6 as the foundation of the project budget. On top of that it layers direct costs (permits, insurance, overhead, etc.) attached at project or activity level, a formal budget structure with revision history and contingency, a cash flow engine driven by activity planned dates, EVM metrics computed against a user-set Data Date, AI-powered cost estimation, anomaly detection, forecast, and narrative generation, and four full reports exportable to Excel and PDF.

---

## Phase 7 Objectives

1. Introduce the Cost domain model: Budget, BudgetRevision, DirectCost, CashFlowPeriod, CostBaseline, ActualCost.
2. Build cost loading view — aggregate resource assignment costs from Phase 6 into a WBS → Activity → Resource cost breakdown tree.
3. Implement direct cost management — custom category cost lines attached to project or activity, with quantity × unit rate, spread across project duration for cash flow.
4. Implement budget management — Original / Revised / Approved budget with revision history, contingency amount, Approved/Pending revision status.
5. Build cash flow engine — driven by activity planned dates, weekly/monthly toggle, cumulative S-Curve chart via LiveCharts2.
6. Implement EVM engine — Data Date driven, full metric set: PV, EV, AC, CPI, SPI, CV, SV, EAC, ETC, VAC.
7. Implement actual cost entry — manual entry per activity (single total) and Excel import via Phase 2 mapping profile infrastructure.
8. Build AI cost services — cost estimation (suggest budget from activity + resources), anomaly detection (flag over-cost activities), EAC/ETC forecast (CPI trend), narrative generation (cost status report text).
9. Deliver four reports — Cost Breakdown, Cash Flow, EVM, Budget Summary — with Excel and PDF export and AI narrative tab embedded in EVM report.
10. Preserve Clean Architecture, MVVM, localization (English + Arabic), RTL, and theme consistency with Phases 0–6.

---

## Phase 7 Scope

### In Scope

- **Cost domain module** (`Planova.Cost`) following the same module pattern as `Planova.Resource`, `Planova.Activity`
- Budget entity with BAC = resource costs + direct costs + contingency, plus manual override option
- BudgetRevision entity with Original → Revised → Approved history and Approved/Pending status
- DirectCost entity: category (Permits, Insurance, Overhead, Preliminaries, Mobilization, Other + user-defined), quantity × unit rate, lump sum amount, attached to Project or Activity, spread evenly across project duration for cash flow
- CostBaseline entity — single active baseline per project, set by user action ("Set Baseline"), freezes PV schedule
- ActualCost entity — one total actual cost record per activity, manual entry + Excel import
- Cash flow engine — periodic (weekly/monthly) planned vs actual cost table driven by activity PlannedStart/PlannedFinish
- S-Curve chart — cumulative planned cost vs cumulative actual cost over time, LiveCharts2
- EVM engine — Data Date configured per project; PV, EV, AC, CPI, SPI, CV, SV, EAC, ETC, VAC computed on demand
- Cost loading view — WBS tree → Activity → resource assignments aggregated from Phase 6, direct cost lines
- Budget summary view — Original / Revised / Approved / Contingency / Total BAC
- Direct cost manager — CRUD with category, quantity, unit, rate, project/activity scope
- Actual cost entry — grid per activity with manual edit + Excel import dialog
- Cash flow view — period table with weekly/monthly toggle + S-Curve chart
- EVM dashboard — metric cards (CPI, SPI, CV, SV, EAC, ETC, VAC) + EVM table + AI narrative tab
- AI cost estimation — suggest cost/budget for activity from description + assigned resources
- AI anomaly detection — flag activities where AC significantly exceeds PV
- AI EAC/ETC forecast — project final cost based on current CPI trend
- AI narrative — generate cost status narrative paragraph for EVM report tab
- Four reports with Excel + PDF export:
  - Cost Breakdown Report (WBS → Activity → Resources → Direct Costs)
  - Cash Flow Report (period table + S-Curve)
  - EVM Report (metrics table + AI narrative tab)
  - Budget Summary Report (Original / Revised / Approved / Contingency / Total)
- Actual cost Excel import — Phase 2 mapping profile system reused; supports per-activity and per-resource-assignment rows; import instructions document included in plan
- Navigation rail integration — Cost Studio replaces the current placeholder "Cost Studio" empty state
- Database: new tables in `PlanovaDbContext` — Budgets, BudgetRevisions, DirectCosts, CostBaselines, ActualCosts, CashFlowPeriods
- EF Core migration for all new Cost entities
- Localization — English and Arabic for all Cost Studio screens
- Unit tests — domain logic, EVM calculations, cash flow engine, budget revision rules
- Integration tests — persistence round-trip, import pipeline

### Out of Scope

- Multi-currency conversion (amounts stored and displayed with their own currency code, no conversion)
- Resource leveling or cost optimization (deferred)
- Primavera P6 cost import/export (Phase 9)
- Purchase orders or procurement workflows (deferred)
- Multi-user approval workflows for budget revisions (Phase 19)
- Multiple active baselines (Phase 9/10)
- Cost forecasting beyond EAC/ETC (deferred to Analytics Center Phase 18)
- Invoice or payment tracking (deferred)

---

## Non-Negotiable Constraints

- Clean Architecture must remain intact — Cost domain logic in `Planova.Cost`, persistence in `Planova.Persistence`, UI in `Planova.UI`.
- MVVM must remain the UI pattern throughout.
- Localization must support English and Arabic; RTL layout must remain correct.
- Theme support must remain consistent with Phases 0–6.
- Phase 6 Resource module must be consumed (not duplicated) — cost loading reads `ResourceAssignment.TotalCost` via `IResourceAssignmentService`.
- Phase 5 Activity module must be consumed — EVM reads `Activity.PercentComplete`, `PlannedStart`, `PlannedFinish`.
- Phase 2 Excel infrastructure must be reused for actual cost import and all report exports.
- AI provider must be abstracted via `IAIProvider` — no direct vendor lock-in.
- All async operations must accept `CancellationToken`.
- BAC is computed as: sum of resource assignment costs + sum of direct costs + contingency, with optional manual override stored as a separate field.
- EV formula: `sum(BudgetedCost × PercentComplete / 100)` for all activities where `PlannedStart ≤ DataDate`.
- PV formula: `sum(BudgetedCost × PlannedPercentComplete)` where PlannedPercentComplete is derived from the baseline schedule at the Data Date.
- AC is the manually entered (or imported) actual cost per activity, summed at project level.

---

## Phase 7 Product Shape

Phase 7 should feel like the natural cost companion to Resource Studio.

The user should be able to:

- Open Cost Studio and immediately see the cost breakdown tree: WBS items → activities → resource assignments (from Phase 6) with computed costs
- Add direct cost lines (permits, insurance, overhead, etc.) at project or activity level, each with category, quantity, unit, and rate
- Review the budget summary: resource costs + direct costs + contingency = Total BAC
- Record budget revisions with Original/Revised/Approved tracking and pending approval status
- Set the Data Date and immediately see EVM metric cards update (CPI, SPI, CV, SV, EAC, ETC, VAC)
- Enter actual costs per activity manually, or import them from Excel using the mapping profile system
- Toggle the cash flow view between weekly and monthly periods, seeing a planned vs actual cost table
- View the S-Curve chart showing cumulative planned cost vs cumulative actual cost
- Click "Set Baseline" to freeze the current planned cost schedule as the EVM baseline
- Ask the AI to estimate cost for an activity, detect cost anomalies, forecast final cost, or generate a narrative
- Generate any of the four reports and export to Excel or PDF

---

## Data Model

### New Module: `Planova.Cost`

Following the same structure as `Planova.Resource`:

```
Planova.Cost/
  Domain/
    Entities/
      Budget.cs
      BudgetRevision.cs
      DirectCost.cs
      CostBaseline.cs
      ActualCost.cs
      CashFlowPeriod.cs
    Enums/
      BudgetRevisionStatus.cs
      DirectCostCategory.cs
      CashFlowPeriodType.cs
      CostAnomalySeverity.cs
    Interfaces/
      IBudgetService.cs
      IDirectCostService.cs
      ICashFlowService.cs
      IEvmService.cs
      IActualCostService.cs
      ICostAiService.cs
      ICostReportService.cs
      ICostImportService.cs
      IBudgetRepository.cs
      IBudgetRevisionRepository.cs
      IDirectCostRepository.cs
      ICostBaselineRepository.cs
      IActualCostRepository.cs
      ICashFlowPeriodRepository.cs
  Application/
    Dto/
      BudgetDto.cs
      BudgetRevisionDto.cs
      DirectCostDto.cs
      CostBreakdownDto.cs
      CashFlowDto.cs
      EvmDto.cs
      ActualCostDto.cs
      CostReportDto.cs
      AiCostSuggestionDto.cs
    Services/
      BudgetService.cs
      DirectCostService.cs
      CashFlowService.cs
      EvmService.cs
      ActualCostService.cs
      CostAiService.cs
      CostReportService.cs
      CostImportService.cs
    Mappings/
      CostMappingProfile.cs
  Extensions/
    ServiceCollectionExtensions.cs
```

---

### Entity: Budget

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ResourceCostTotal` | `decimal(18,2)` | Yes | Computed from Phase 6 assignments on demand |
| `DirectCostTotal` | `decimal(18,2)` | Yes | Computed from DirectCost records |
| `Contingency` | `decimal(18,2)` | Yes | User-entered contingency amount, default 0 |
| `ContingencyPercent` | `decimal(5,2)?` | No | Optional % for display; not used in calculation |
| `ManualBacOverride` | `decimal(18,2)?` | No | If set, overrides computed BAC as approved total |
| `Currency` | `string(3)` | Yes | ISO 4217, default from Project |
| `Notes` | `string(500)?` | No | |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Computed (not stored):**
- `BAC = ResourceCostTotal + DirectCostTotal + Contingency` (or `ManualBacOverride` if set)

**Unique constraint:** `(ProjectId)` — one Budget per project.

---

### Entity: BudgetRevision

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `BudgetId` | `Guid` | Yes | FK → Budget |
| `RevisionNumber` | `int` | Yes | Auto-incremented per budget |
| `RevisionType` | `string(20)` | Yes | Original, Revised, Approved |
| `Amount` | `decimal(18,2)` | Yes | Budget amount at this revision |
| `Status` | `BudgetRevisionStatus` | Yes | Pending, Approved |
| `Reason` | `string(500)?` | No | Reason for revision |
| `ApprovedBy` | `string(100)?` | No | |
| `ApprovedAt` | `DateTime?` | No | |
| `CreatedAt` | `DateTime` | Yes | |
| `CreatedBy` | `string(100)?` | No | |

---

### Entity: DirectCost

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project (always set) |
| `ActivityId` | `Guid?` | No | FK → Activity (Phase 5); null = project-level cost |
| `Category` | `DirectCostCategory` | Yes | Permits, Insurance, Overhead, Preliminaries, Mobilization, Other, Custom |
| `CustomCategory` | `string(100)?` | No | Used when Category == Custom |
| `Description` | `string(200)` | Yes | |
| `Quantity` | `decimal(18,4)` | Yes | |
| `UnitOfMeasure` | `string(50)` | Yes | |
| `UnitRate` | `decimal(18,4)` | Yes | Rate per unit |
| `TotalAmount` | `decimal(18,2)` | Yes | Computed: `Quantity × UnitRate`, stored |
| `Currency` | `string(3)` | Yes | ISO 4217 |
| `SpreadMethod` | `string(20)` | Yes | EvenSpread (only method for now) |
| `Notes` | `string(500)?` | No | |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

---

### Entity: CostBaseline

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `Name` | `string(100)` | Yes | e.g., "Baseline 1" |
| `BaselineDate` | `DateTime` | Yes | Date baseline was set |
| `TotalPV` | `decimal(18,2)` | Yes | Total Planned Value at baseline |
| `IsActive` | `bool` | Yes | Only one active baseline per project |
| `Notes` | `string(500)?` | No | |
| `CreatedAt` | `DateTime` | Yes | |
| `CreatedBy` | `string(100)?` | No | |

**Navigation:**
- `BaselineActivities: ICollection<CostBaselineActivity>`

**Unique constraint:** Only one `IsActive == true` per ProjectId.

---

### Entity: CostBaselineActivity

Snapshot of each activity's planned cost and schedule at baseline time.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `BaselineId` | `Guid` | Yes | FK → CostBaseline |
| `ActivityId` | `Guid` | Yes | FK → Activity (Phase 5) |
| `BudgetedCost` | `decimal(18,2)` | Yes | Resource costs + direct costs at snapshot time |
| `PlannedStart` | `DateTime?` | No | Snapshot of Activity.PlannedStart |
| `PlannedFinish` | `DateTime?` | No | Snapshot of Activity.PlannedFinish |
| `PlannedPercentComplete` | `decimal(5,2)` | Yes | PV-based % at Data Date (computed during EVM) |

---

### Entity: ActualCost

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ActivityId` | `Guid` | Yes | FK → Activity (Phase 5) |
| `ActualAmount` | `decimal(18,2)` | Yes | Total actual cost for this activity |
| `Currency` | `string(3)` | Yes | ISO 4217 |
| `DataDate` | `DateTime` | Yes | The Data Date this actual applies to |
| `Source` | `string(20)` | Yes | Manual, Import |
| `Notes` | `string(500)?` | No | |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique constraint:** `(ProjectId, ActivityId)` — one ActualCost record per activity.

---

### Entity: CashFlowPeriod

Pre-computed cash flow periods, regenerated on demand.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `PeriodType` | `CashFlowPeriodType` | Yes | Weekly, Monthly |
| `PeriodStart` | `DateTime` | Yes | Start of this period |
| `PeriodEnd` | `DateTime` | Yes | End of this period |
| `PlannedCost` | `decimal(18,2)` | Yes | Sum of planned cost in this period |
| `ActualCost` | `decimal(18,2)` | Yes | Sum of actual cost in this period |
| `CumulativePlanned` | `decimal(18,2)` | Yes | Running cumulative planned |
| `CumulativeActual` | `decimal(18,2)` | Yes | Running cumulative actual |
| `ComputedAt` | `DateTime` | Yes | When this was last computed |

**Index:** `(ProjectId, PeriodType, PeriodStart)`.

---

### Enums

```csharp
public enum BudgetRevisionStatus { Pending, Approved }

public enum DirectCostCategory
{
    Permits, Insurance, Overhead, Preliminaries,
    Mobilization, Demobilization, Testing, Other, Custom
}

public enum CashFlowPeriodType { Weekly, Monthly }

public enum CostAnomalySeverity { Low, Medium, High, Critical }
```

---

## EVM Calculation Rules

All EVM computation lives in `EvmService` in the Application layer.

| Symbol | Formula |
|--------|---------|
| **BAC** | `ResourceCostTotal + DirectCostTotal + Contingency` (or ManualBacOverride) |
| **PV** | `sum(BudgetedCost × PlannedPercentComplete)` for baseline activities where `PlannedStart ≤ DataDate` |
| **EV** | `sum(BudgetedCost × PercentComplete / 100)` for activities where `PlannedStart ≤ DataDate` |
| **AC** | `sum(ActualCost.ActualAmount)` for activities where `PlannedStart ≤ DataDate` |
| **CV** | `EV - AC` |
| **SV** | `EV - PV` |
| **CPI** | `EV / AC` (0 if AC = 0) |
| **SPI** | `EV / PV` (0 if PV = 0) |
| **EAC** | `BAC / CPI` (or `BAC` if CPI = 0) |
| **ETC** | `EAC - AC` |
| **VAC** | `BAC - EAC` |

PlannedPercentComplete for each baseline activity at a given Data Date:
- If `DataDate < PlannedStart` → 0%
- If `DataDate ≥ PlannedFinish` → 100%
- Otherwise → `(DataDate - PlannedStart).TotalDays / (PlannedFinish - PlannedStart).TotalDays × 100`

---

## Cash Flow Calculation Rules

All cash flow logic lives in `CashFlowService`.

1. For each activity with `PlannedStart` and `PlannedFinish`, compute its budgeted cost (resource assignments + direct costs attached to it).
2. Spread cost evenly across working days between `PlannedStart` and `PlannedFinish` (daily cost = TotalCost / Duration).
3. For each direct cost at project level, spread evenly across project `StartDate` to `FinishDate`.
4. Aggregate daily costs into weekly or monthly buckets based on the selected `CashFlowPeriodType`.
5. Compute cumulative planned and actual columns.
6. Store result in `CashFlowPeriod` table; invalidate and regenerate when assignments, direct costs, or activity dates change.

---

## Actual Cost Import — Mapping Instructions

The import reuses the Phase 2 `IWorkbookReader` + mapping profile system.

**Supported import modes:**

### Mode A — Per Activity
Each row represents one activity's total actual cost.

| Column | Field | Notes |
|--------|-------|-------|
| Activity Code | `ActivityCode` | Used to match Activity.Code |
| Activity Name | `ActivityName` | Optional — for reference |
| Actual Cost | `ActualAmount` | Numeric |
| Currency | `Currency` | ISO 3-letter code, default USD |
| Notes | `Notes` | Optional |

### Mode B — Per Resource Assignment
Each row represents one resource assignment's actual cost contribution.

| Column | Field | Notes |
|--------|-------|-------|
| Activity Code | `ActivityCode` | Used to match Activity |
| Resource Code | `ResourceCode` | Used to match Resource |
| Actual Cost | `ActualAmount` | Numeric — aggregated into activity total |
| Currency | `Currency` | ISO 3-letter code |
| Notes | `Notes` | Optional |

**Import behavior:**
- Mode is selected by the user in the import dialog (toggle).
- Activity matching is by `ActivityCode` (case-insensitive).
- If an `ActualCost` record already exists for an activity, it is updated (upsert).
- Rows with unmatched activity codes are flagged as errors; import continues for valid rows.
- After import, cash flow periods are regenerated.
- The mapping profile created during import is saved for future reuse (Phase 2 pattern).

---

## AI Cost Services

All four AI features are implemented in `CostAiService` via `IAIProvider` (Semantic Kernel).

### 1. Cost Estimation
- Input: Activity name, description, WBS category, list of assigned resources with quantities and rates
- Output: `AiCostSuggestionDto` — suggested total budget, confidence level, reasoning
- UI: "Estimate Cost" button on Cost Breakdown view per activity; results shown in preview/accept/reject panel

### 2. Anomaly Detection
- Input: All activities in project with PV, EV, AC values
- Output: List of `CostAnomalyDto` — activity name, severity, description of anomaly, suggested action
- Trigger: On-demand button "Detect Anomalies" on EVM dashboard
- UI: Anomaly list panel with severity badges; click to navigate to activity in cost breakdown

### 3. EAC/ETC Forecast
- Input: CPI trend over cash flow periods, BAC, current EAC
- Output: Forecast narrative + revised EAC estimate with confidence range
- UI: "AI Forecast" card on EVM dashboard showing AI-adjusted EAC vs formula EAC

### 4. Cost Narrative (EVM Report)
- Input: Full EVM metrics, project name, Data Date, top anomalies
- Output: 2–4 paragraph professional cost status narrative
- UI: "AI Narrative" tab inside EVM Report view; "Regenerate" button

---

## UI Structure — Cost Studio

Following the same multi-tab pattern as Resource/Activity/WBS studios.

```
CostStudioView (main container with TabControl)
  ├── Tab: Cost Breakdown
  │     CostBreakdownView — WBS tree → Activity → Resource lines → Direct cost lines
  │     + AI Estimate button per activity
  ├── Tab: Budget
  │     BudgetSummaryView — BAC components, contingency, revision history grid
  ├── Tab: Direct Costs
  │     DirectCostManagerView — CRUD grid, category filter, project/activity scope toggle
  ├── Tab: Actual Costs
  │     ActualCostEntryView — per-activity grid, manual edit, import button
  ├── Tab: Cash Flow
  │     CashFlowView — period toggle (weekly/monthly), table + S-Curve LiveCharts2 chart
  ├── Tab: EVM
  │     EvmDashboardView — Data Date picker, metric cards, EVM table, anomaly panel, AI forecast card
  ├── Tab: Reports
  │     CostReportView — four report tabs (Cost Breakdown / Cash Flow / EVM / Budget Summary)
  │     Each with Excel + PDF export; EVM tab has AI Narrative sub-tab
  └── Tab: Settings
        CostSettingsView — currency, default cost categories, import templates
```

---

## Solution Structure Changes

### New project: `Planova.Cost`
- References: `Planova.Shared`, `Planova.Activity` (for Activity DTO), `Planova.Resource` (for ResourceAssignment DTO)
- No UI references — UI only references service interfaces

### `Planova.Persistence` changes
- New entity configurations: BudgetConfiguration, BudgetRevisionConfiguration, DirectCostConfiguration, CostBaselineConfiguration, CostBaselineActivityConfiguration, ActualCostConfiguration, CashFlowPeriodConfiguration
- New repositories: BudgetRepository, BudgetRevisionRepository, DirectCostRepository, CostBaselineRepository, ActualCostRepository, CashFlowPeriodRepository
- New EF Core migration: `AddCostStudioEntities`
- Register new DbSets in `PlanovaDbContext`

### `Planova.UI` changes
- New folder: `Views/Cost/` — all Cost Studio XAML views
- New folder: `ViewModels/Cost/` — all Cost Studio ViewModels
- `ShellViewModel.cs` — replace "cost" placeholder target with real `CostStudioView`
- `App.xaml.cs` — register all Cost ViewModels/Views + `services.AddPlanovaCost()`

### `DashboardSummaryDto` + `DashboardService`
- Add `TotalBudget`, `OverallCpi`, `OverallSpi` to dashboard summary

---

## Workstream Breakdown

### Workstream A: Cost Domain Module Setup
1. Create `Planova.Cost` project with folder structure
2. Define all domain entities and enums
3. Define all repository interfaces
4. Define all service interfaces
5. Add project references to solution

### Workstream B: EF Core Persistence
1. Add DbSets for all Cost entities to `PlanovaDbContext`
2. Implement entity configurations (Budget, BudgetRevision, DirectCost, CostBaseline, CostBaselineActivity, ActualCost, CashFlowPeriod)
3. Implement all Cost repositories
4. Generate EF Core migration `AddCostStudioEntities`
5. Verify migration runs clean against existing database

### Workstream C: Budget Service
1. Implement `BudgetService` — create/update budget per project, compute BAC from resource costs + direct costs + contingency
2. Implement budget revision CRUD with auto-increment revision number
3. Implement revision approval (Pending → Approved status transition)
4. Implement `GetBudgetSummaryAsync` — returns full budget breakdown DTO

### Workstream D: Direct Cost Service
1. Implement `DirectCostService` — full CRUD, project or activity scope
2. Implement TotalAmount computation (Quantity × UnitRate)
3. Implement direct cost aggregation by project and by activity
4. Implement category filter and search

### Workstream E: Cash Flow Engine
1. Implement `CashFlowService.ComputePeriodsAsync` — spread activity costs across date ranges
2. Implement weekly aggregation
3. Implement monthly aggregation
4. Implement cumulative columns
5. Implement `RegenerateAsync` — full invalidation and recompute
6. Implement S-Curve data point generation (cumulative planned vs actual by period)

### Workstream F: EVM Engine
1. Implement `EvmService.ComputeAsync(projectId, dataDate)` — returns full `EvmDto`
2. Implement PV computation from baseline activity snapshots
3. Implement EV computation from Activity.PercentComplete
4. Implement AC aggregation from ActualCost records
5. Implement all derived metrics (CPI, SPI, CV, SV, EAC, ETC, VAC)
6. Implement `SetBaselineAsync` — snapshot activity costs and schedule

### Workstream G: Actual Cost Service
1. Implement `ActualCostService` — upsert per activity, manual entry
2. Implement `ActualCostImportService` — Mode A (per activity) and Mode B (per resource assignment, aggregated)
3. Implement import validation (unmatched codes, negative values)
4. Implement import result DTO with error rows flagged

### Workstream H: AI Cost Services
1. Implement `CostAiService` with `IAIProvider` abstraction
2. Implement cost estimation prompt and JSON output parsing
3. Implement anomaly detection prompt and result parsing
4. Implement EAC/ETC forecast prompt
5. Implement narrative generation prompt
6. Handle AI unavailability gracefully (disable AI buttons, show message)

### Workstream I: Report Service
1. Implement `CostReportService.GenerateCostBreakdownAsync` — WBS → Activity → Resource → Direct Cost tree
2. Implement `GenerateCashFlowReportAsync` — period table data
3. Implement `GenerateEvmReportAsync` — metrics table data + activity-level EVM rows
4. Implement `GenerateBudgetSummaryAsync` — revision history + BAC components
5. Implement Excel export for all four reports (reuse Phase 2 `WorkbookWriter`)
6. Implement PDF export for all four reports (QuestPDF)

### Workstream J: UI — Cost Studio Shell
1. Create `CostStudioView.xaml` + `CostStudioViewModel.cs` with tab initialization
2. Register Cost Studio in `ShellViewModel.RegisterNavigationTargets()` — replace placeholder
3. Register all Cost views/viewmodels in `App.xaml.cs`
4. Wire `services.AddPlanovaCost()` extension method

### Workstream K: UI — Cost Breakdown View
1. Create WBS tree control showing WBS items with expandable activity rows
2. Show resource assignment cost lines under each activity (from Phase 6)
3. Show direct cost lines under each activity and at project level
4. Show activity total cost, WBS subtotal, project total
5. AI Estimate Cost button per activity with preview/accept/reject panel

### Workstream L: UI — Budget View
1. Budget summary card: Resource Costs / Direct Costs / Contingency / BAC
2. Budget revision history grid with RevisionType, Amount, Status columns
3. Add/Edit/Approve revision dialogs
4. Contingency field with optional % display

### Workstream M: UI — Direct Cost Manager
1. Direct cost grid with category, description, quantity, unit, rate, total columns
2. Category filter dropdown
3. Project/Activity scope toggle (project-level vs activity-attached)
4. Add/Edit/Delete direct cost dialogs

### Workstream N: UI — Actual Cost Entry
1. Per-activity grid: Activity Code, Name, Planned Cost, Actual Cost, Variance column
2. Inline edit for ActualAmount
3. Import button opening Excel import dialog (Mode A / Mode B toggle)
4. Import results dialog showing matched/unmatched rows

### Workstream O: UI — Cash Flow View
1. Weekly/Monthly period toggle
2. Period table: Period, Planned Cost, Actual Cost, Cumulative Planned, Cumulative Actual
3. S-Curve LiveCharts2 line chart: Cumulative Planned (solid) vs Cumulative Actual (dashed)
4. Zoom controls and date range filter
5. Export to Excel button

### Workstream P: UI — EVM Dashboard
1. Data Date picker (date input, triggers EVM recompute)
2. KPI metric cards: CPI, SPI, CV, SV, EAC, ETC, VAC (color-coded: green/amber/red)
3. EVM table: per-activity rows with PV, EV, AC, CV, SV
4. Anomaly panel: AI anomaly detection results with severity badges
5. AI Forecast card: formula EAC vs AI-adjusted EAC
6. "Set Baseline" button with confirmation dialog
7. "Detect Anomalies" button with progress indicator

### Workstream Q: UI — Reports
1. Report hub view with four tabs: Cost Breakdown / Cash Flow / EVM / Budget Summary
2. Each tab: filter panel + preview area + Export Excel button + Export PDF button
3. EVM report tab: sub-tab for AI Narrative with "Regenerate" button
4. Print preview support

### Workstream R: UI — Cost Settings
1. Default currency setting for new cost entries
2. Custom direct cost categories management
3. Import template download links (Mode A and Mode B Excel templates)
4. Baseline management (view/delete current baseline)

### Workstream S: Localization
1. Add English resource strings for all Cost Studio screens
2. Add Arabic resource strings for all Cost Studio screens
3. Verify RTL layout on all Cost views
4. Verify theme consistency (dark/light/high contrast)

### Workstream T: Testing
1. Unit test EVM calculations (PV, EV, AC, CPI, SPI, CV, SV, EAC, ETC, VAC)
2. Unit test cash flow period aggregation (weekly and monthly)
3. Unit test budget BAC computation (resource + direct + contingency)
4. Unit test budget revision state transitions
5. Unit test DirectCost TotalAmount computation
6. Unit test ActualCost import validation (Mode A and Mode B)
7. Unit test baseline snapshot logic
8. Unit test AI service with mock IAIProvider
9. Integration test persistence round-trip for all Cost entities
10. Integration test cash flow regeneration on assignment change
11. UI smoke test all Cost Studio tabs

---

## Database Index Strategy

| Table | Index | Type | Purpose |
|-------|-------|------|---------|
| Budgets | `(ProjectId)` | Unique | One budget per project |
| BudgetRevisions | `(BudgetId, RevisionNumber)` | Unique | Revision lookup |
| DirectCosts | `(ProjectId, ActivityId)` | Non-clustered | Activity cost lookup |
| DirectCosts | `(ProjectId, Category)` | Non-clustered | Category filtering |
| CostBaselines | `(ProjectId, IsActive)` | Non-clustered | Active baseline lookup |
| CostBaselineActivities | `(BaselineId, ActivityId)` | Unique | Baseline snapshot lookup |
| ActualCosts | `(ProjectId, ActivityId)` | Unique | One actual per activity |
| CashFlowPeriods | `(ProjectId, PeriodType, PeriodStart)` | Non-clustered | Period range queries |

---

## Risks and Mitigations

### Risk: EVM requires baseline — user may not have set one
Mitigation: EVM view shows a prominent "No baseline set" warning with "Set Baseline" call-to-action. PV displays as 0 until baseline is set; EV and AC still compute.

### Risk: Cash flow computation is slow for large projects
Mitigation: Pre-compute and store in `CashFlowPeriod` table. Invalidate only when relevant data changes. Show stale indicator when regeneration is needed.

### Risk: Actual cost import with unmatched activity codes
Mitigation: Import shows a detailed validation result dialog listing matched and unmatched rows. Partial import is allowed — valid rows are committed, invalid rows are skipped with the user's confirmation.

### Risk: Resource cost total from Phase 6 changes after budget is set
Mitigation: Budget displays a "Resource Cost Updated" indicator when `ResourceAssignment` totals have changed since the last budget save. User must refresh to re-sync.

### Risk: AI narrative quality varies with provider
Mitigation: All AI output is clearly labeled as AI-generated. User can regenerate or edit the narrative before exporting. AI unavailability degrades gracefully (button disabled, message shown).

### Risk: Multiple currency codes in cost breakdown
Mitigation: Display currency code next to each amount. No conversion. Report totals show a warning note when multiple currencies are present.

---

## Acceptance Criteria

Phase 7 is complete when all of the following are true:

- Cost breakdown tree shows WBS → Activities → Resource costs (from Phase 6) → Direct cost lines.
- Direct costs can be created, edited, and deleted with all standard categories plus custom.
- Direct costs can be attached to project level or a specific activity.
- Budget shows BAC = resource costs + direct costs + contingency, with optional manual override.
- Budget revision history is tracked with Original/Revised/Approved types and Pending/Approved status.
- A baseline can be set, snapshotting activity costs and planned schedule.
- Data Date can be configured per project to drive EVM computation.
- All EVM metrics (PV, EV, AC, CPI, SPI, CV, SV, EAC, ETC, VAC) compute correctly.
- Actual costs can be entered manually per activity or imported from Excel (Mode A and Mode B).
- Cash flow view shows planned vs actual cost in weekly and monthly periods with toggle.
- S-Curve chart renders cumulative planned vs cumulative actual cost.
- AI cost estimation, anomaly detection, EAC forecast, and narrative all function with IAIProvider abstraction.
- All four reports generate correctly and export to Excel and PDF.
- EVM report includes an AI narrative tab with Regenerate capability.
- All Cost Studio screens render correctly in English and Arabic with RTL support.
- Phase 6 Resource Studio, Phase 5 Activity Studio, and all prior phases remain fully functional.
- The implementation is Clean Architecture and MVVM compliant.

---

## Definition of Done

- `Planova.Cost` module exists with all domain entities, service interfaces, and implementations.
- All repositories implemented and registered in `Planova.Persistence`.
- EF Core migration `AddCostStudioEntities` runs successfully.
- Cost Studio replaces the placeholder navigation target in `ShellViewModel`.
- All Cost Studio tabs are functional and navigable.
- Baseline, EVM, and cash flow engines pass unit tests with 100% accuracy on known test cases.
- Actual cost import supports both modes with mapping profile reuse.
- AI services integrated with `IAIProvider` abstraction and graceful fallback.
- All four reports export to Excel and PDF.
- Localization complete for all new Cost Studio scope.
- Dashboard updated with cost KPIs (TotalBudget, CPI, SPI).
- All tests pass.

---

## Next Step After Phase 7

When Cost Studio is stable, the next implementation plan should target **Phase 8 — Reporting Center**:

- Daily, Weekly, Monthly, Executive report templates
- Unified report hub across all studios
- Scheduled report generation
- AI-assisted narrative generation across all report types
