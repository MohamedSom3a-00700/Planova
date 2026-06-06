# Research: Cost Studio

## Design Decisions

### 1. Cost Breakdown Tree — Hierarchical Data Model

**Decision**: In-memory tree construction from flat query results; no recursive CTEs or materialized path

**Rationale**:
- Cost breakdown is a read-only view combining WBS items, activities, resource assignment costs, and direct costs
- Query architecture: 3 parallel queries (WBS hierarchy, resource costs per activity, direct costs) → in-memory tree assembly
- WBS hierarchy already exists as a tree in `Planova.Wbs`; activity-parent relationships tie activities to WBS nodes
- Resource costs are aggregated from `ResourceAssignment` (Phase 6) — activity-level total cost is sum of assignment costs
- Direct costs are attached to either a project (root level) or a specific activity
- In-memory composition avoids over-engineered SQL trees; at 500 activities the join overhead is negligible
- TreeView in WPF uses `HierarchicalDataTemplate` bound to a flat `List<CostBreakdownItem>` with `Children` populated on load

**Alternatives considered**:
- *SQL recursive CTE with tree serialization*: Rejected — adds query complexity without benefit; data is already in-memory for display
- *Denormalized path column on CostBreakdownItem*: Rejected — no write-back to WBS needed; existing WBS structure is authoritative

### 2. Direct Cost Categories — Enum-Driven with User-Defined Extension

**Decision**: `DirectCostCategory` enum with 9 predefined values + `Custom` type with free-text category name

**Rationale**:
- Spec FR-004 defines: Permits, Insurance, Overhead, Preliminaries, Mobilization, Demobilization, Testing, Other, Custom
- `Custom` values: enum `Custom` with `CategoryName` string field (`string(200)`) for user-defined label
- UI: dropdown with predefined categories + "Custom…" option → text input for custom name
- This avoids a full dynamic category management system (over-engineering for <500 items per project)

**Alternatives considered**:
- *Separate DirectCostCategory table*: Rejected — 9 fixed values don't warrant a table; FK + lookup overhead
- *Free-text only*: Rejected — loses consistent categorization for reporting and filtering

### 3. Budget Revision State Machine

**Decision**: Budget revision workflow: `Pending → Approved` (one-way); no rejection/reopen for v1

**Rationale**:
- FR-009/FR-010: revision types are Original, Revised, Approved; status is Pending or Approved
- `Pending`: newly created, editable, awaiting approval
- `Approved`: locked, contributes to budget
- Only one Approved revision per type allowed
- Simpler than full approval workflow (no rejection, no multi-step approval chain)
- Audit trail via `ApprovedBy` and `ApprovedAt` columns on the revision record

**Alternatives considered**:
- *Full approval workflow (Pending → Rejected → Resubmitted → Approved)*: Rejected — spec only requires Pending → Approved; add later if needed
- *No status state machine (just revision history)*: Rejected — would lose the approval accountability requirement (FR-031 audit logging)

### 4. Cost Baseline — Point-in-Time Snapshot Strategy

**Decision**: Snapshot only the fields needed for EVM (planned cost per activity, planned dates, BAC) into a separate `CostBaselineRow` table

**Rationale**:
- FR-011/FR-012: only one active baseline per project; baseline snapshots current activity costs and schedule
- `CostBaseline` table: `Id`, `ProjectId`, `CreatedAt`, `CreatedBy`, `Description`, `IsActive`
- `CostBaselineRow` table: `Id`, `BaselineId`, `ActivityId`, `PlannedCost`, `PlannedStart`, `PlannedFinish`, `BudgetAtCompletion`
- If resource costs change after baseline: show indicator per FR-030 but don't invalidate baseline
- User must explicitly re-baseline to capture updated costs
- Simplifies EVM: PV = sum of `PlannedCost` for all baseline rows where `PlannedStart <= DataDate`; EV = sum of `PlannedCost × ActualPercentComplete`

**Alternatives considered**:
- *Deep copy entire activity/resource graph*: Rejected — massive duplication; only cost/schedule fields are needed for EVM
- *No snapshot (compute from current data)*: Rejected — EVM requires a frozen reference point; current data changes invalidate historical metrics

### 5. Actual Cost Import — Dual-Mode with Upsert

**Decision**: Two import modes (per-activity total, per-resource-assignment aggregated), both upserting by activity code

**Rationale**:
- FR-014/FR-015: import with activity code matching, upsert, unmatched row reporting
- Per-activity mode: single amount per activity → direct write to `ActualCost` table
- Per-resource-assignment mode: amounts per assignment → aggregated to activity total
- All matched rows are imported in a single transaction; unmatched rows returned as error report
- If >20% unmatched (or total >10000 rows), abort entire import (FR-033)
- File validation before any DB writes: verify format, count rows, check activity code matching rates

**Alternatives considered**:
- *Import only mode (no manual entry)*: Rejected — FR-013 requires manual entry as well
- *Per-row transaction with partial success*: Rejected — spec says abort if >20% unmatched; all-or-nothing is clearer

### 6. Cash Flow Period Spreading — Equal Daily Distribution

**Decision**: Spread planned cost evenly across activity duration (working days); aggregate to weekly/monthly buckets

**Rationale**:
- FR-016/FR-017: weekly and monthly period toggling
- For each activity with resource costs and planned dates: `DailyCost = TotalCost / WorkingDaysInDuration`
- Multiply daily cost by number of days falling within each period bucket
- Pre-aggregated in-memory (not stored as periods) to avoid staleness and to support toggle without recomputation
- Chart (FR-018): S-Curve as cumulative sum of period costs over time
- Actual costs: spread actuals across periods using `ActualCost.Amount / RemainingPeriods` until date of entry; after entry date, actual is 0

**Alternatives considered**:
- *Stored cash flow periods with CRON refresh*: Rejected — too complex; in-memory computation for 2-year project is sub-second
- *Linear distribution across calendar days*: Rejected — working days is more realistic for construction schedules

### 7. EVM Computation — Service-Layer Calculation

**Decision**: Stateless `EvmService` computing all metrics from baseline, actual costs, and progress data on demand

**Rationale**:
- FR-019/FR-020: Data Date drives all EVM computations
- PV = sum of `CostBaselineRow.PlannedCost` where `PlannedStart <= DataDate`
- EV = sum of `CostBaselineRow.PlannedCost × Activity.PercentComplete / 100`
- AC = sum of `ActualCost.Amount` up to `DataDate`
- CPI = EV / AC (handle AC=0 → Infinity displayed as "N/A")
- SPI = EV / PV (handle PV=0 → Infinity displayed as "N/A")
- EAC = BAC / CPI; ETC = EAC - AC; VAC = BAC - EAC
- Color coding (FR-020): CPI/SPI ≥ 1.0 → green; 0.8–0.99 → amber; <0.8 → red
- Cached per Data Date selection; invalidated when new actual costs are entered

**Alternatives considered**:
- *Stored EVM metrics with real-time refresh*: Rejected — metrics always depend on Data Date; recompute is cheap (<3s per SC-005)
- *Database computed columns*: Rejected — EVM uses data from 4+ tables; too complex for DB functions

### 8. AI Cost Services — Semantic Kernel Integration

**Decision**: Single `CostAiPlugin` with four functions (Estimate, Anomalies, Forecast, Narrative); graceful degradation when unavailable

**Rationale**:
- Follows constitution mandate for Semantic Kernel (Principle VI)
- `EstimateCost`: receives activity name, description, assigned resources → returns structured `(SuggestedBudget, Confidence, Reasoning)`
- `DetectAnomalies`: receives list of `(ActivityName, PlannedCost, ActualCost)` tuples → returns flagged items with severity
- `Forecast`: receives CPI trend, EAC formula result → returns AI-adjusted EAC
- `GenerateNarrative`: receives EVM metrics, anomaly list → returns 2-4 paragraph narrative
- Graceful degradation (FR-025): if Semantic Kernel provider returns error/disconnected, show disabled buttons with informative message
- Configuration-driven: `AICostOptions` section in `appsettings.json`

**Alternatives considered**:
- *Separate plugins per function*: Rejected — all four are cost-domain; single plugin is cleaner DI registration
- *Template-based narrative without AI*: Rejected — spec (US-6, FR-024) explicitly requires AI-generated narrative; regression to template would degrade user experience

### 9. Report Generation — QuestPDF + ClosedXML Reuse

**Decision**: Four report types sharing a common `CostReportService` with strategy pattern per type

**Rationale**:
- FR-026/FR-027: Cost Breakdown, Cash Flow, EVM, Budget Summary reports with Excel + PDF export
- Each report type is a strategy implementing `ICostReport`
- PDF: QuestPDF document per report type (reuse existing QuestPDF infrastructure)
- Excel: ClosedXML workbook per report type (reuse existing `Planova.Excel.Writers`)
- EVM report includes AI narrative tab (FR-028): narrative content from AI service (or "AI unavailable" placeholder)
- `CostReportService` orchestrates: accept report type + parameters → delegate to strategy → return file bytes for export

**Alternatives considered**:
- *Single monolithic report generator*: Rejected — each report has different data sources and layout; strategy pattern keeps concerns separated
- *HTML-to-PDF conversion*: Rejected — QuestPDF is mandated; HTML-to-PDF adds a browser dependency

### 10. Audit Logging — Simple Audit Trail

**Decision**: Audit events stored in a shared `AuditLog` table (existing infrastructure); structured JSON payload per event type

**Rationale**:
- FR-031: log budget revision creation/approval, baseline set/remove, manual BAC override
- Reuse existing `Planova.Persistence.AuditLog` table (tracking user identity + timestamp)
- Each auditable operation writes: `{ EntityType, EntityId, Operation, UserId, Timestamp, Payload (JSON) }`
- Payload captures before/after values for changed fields where applicable
- Not using full change-tracking audit (too noisy); only explicit sensitive operations

**Alternatives considered**:
- *EF Core Interceptor-based automatic audit*: Rejected — would capture too many events; explicit logging is more targeted and maintainable
- *Dedicated audit table per module*: Rejected — shared AuditLog table is simpler and sufficient for this volume (~200 events/year per project)
