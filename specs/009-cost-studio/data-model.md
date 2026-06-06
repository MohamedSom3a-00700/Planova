# Data Model: Cost Studio

## Entity Relationship Diagram

```
Budget (1 per project)
  └── BudgetRevision (1:N) — revision history

DirectCost (N per project)
  └── attached to Project (scope=Project) or Activity (scope=Activity)

CostBaseline (1 active per project)
  └── CostBaselineRow (1:N) — snapshot of cost/schedule per activity

ActualCost (1 per activity) — single total amount

CashFlowPeriod — computed (not stored), derived from activity costs + dates
```

---

## Entities

### Budget

The project's financial plan — the aggregate of resource costs, direct costs, and contingency.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project; one Budget per project |
| `ResourceCostTotal` | `decimal(18,2)` | Yes | Computed sum of all resource assignment costs (read-only) |
| `DirectCostTotal` | `decimal(18,2)` | Yes | Computed sum of all direct cost line totals (read-only) |
| `ContingencyAmount` | `decimal(18,2)?` | No | Absolute value or computed from percentage |
| `ContingencyPercent` | `decimal(5,2)?` | No | Percentage (alternative to absolute) |
| `TotalBudget` | `decimal(18,2)` | Yes | Computed: ResourceCostTotal + DirectCostTotal + ContingencyAmount |
| `IsManualOverride` | `bool` | Yes | Whether the user manually overrode the computed TotalBudget |
| `ManualTotalBudget` | `decimal(18,2)?` | No | User-specified total when IsManualOverride is true |
| `Currency` | `string(3)` | Yes | ISO 4217, from project default |
| `Status` | `BudgetStatus` enum | Yes | Draft, Active, Closed |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |
| `UpdatedBy` | `string(100)?` | No | |

**Unique Constraint:** `(ProjectId)` — one Budget per project.

**Navigation Properties:**
- `Revisions: ICollection<BudgetRevision>`

---

### BudgetRevision

A recorded change to the budget with approval tracking.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `BudgetId` | `Guid` | Yes | FK → Budget |
| `RevisionNumber` | `int` | Yes | Auto-incremented per budget |
| `RevisionType` | `BudgetRevisionType` enum | Yes | Original, Revised, Approved |
| `Amount` | `decimal(18,2)` | Yes | The budget amount for this revision |
| `Status` | `BudgetRevisionStatus` enum | Yes | Pending, Approved |
| `Reason` | `string(500)?` | No | Reason for the revision |
| `ApprovedBy` | `string(100)?` | No | User who approved (set when status → Approved) |
| `ApprovedAt` | `DateTime?` | No | Timestamp of approval |
| `CreatedAt` | `DateTime` | Yes | |
| `CreatedBy` | `string(100)?` | Yes | |

**Unique Constraint:** `(BudgetId, RevisionNumber)` — no duplicate revision numbers.

**Constraints:**
- Only one Approved revision per `RevisionType` allowed at a time
- `Status` transitions: `Pending → Approved` (one-way)

---

### DirectCost

A non-resource cost item (permits, insurance, overhead, etc.).

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ActivityId` | `Guid?` | No | FK → Activity if scope is Activity; null for project-level |
| `Category` | `DirectCostCategory` enum | Yes | Permits, Insurance, Overhead, Preliminaries, Mobilization, Demobilization, Testing, Other, Custom |
| `CustomCategoryName` | `string(200)?` | No | Free-text name when Category == Custom |
| `Description` | `string(500)` | Yes | Description of the cost item |
| `Quantity` | `decimal(18,4)` | Yes | Number of units |
| `UnitOfMeasure` | `string(50)` | Yes | e.g., "ls", "m²", "month" |
| `UnitRate` | `decimal(18,4)` | Yes | Rate per unit |
| `Currency` | `string(3)` | Yes | ISO 4217 |
| `TotalAmount` | `decimal(18,2)` | Yes | Computed: Quantity × UnitRate |
| `Scope` | `DirectCostScope` enum | Yes | Project or Activity |
| `IsOrphaned` | `bool` | Yes | True if the referenced Activity has been deleted (FR-032) |
| `DeletedActivityId` | `Guid?` | No | Original ActivityId before deletion (for audit reference) |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Navigation Properties:**
- `Activity: Activity?` (nullable; null if orphaned)

**Index:** `(ProjectId, Scope)` for breakdown tree queries.
**Index:** `(ActivityId)` for activity-scoped queries (nullable).

---

### CostBaseline

A point-in-time snapshot of activity costs and schedule, used as the EVM reference.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `Description` | `string(500)?` | No | User-provided description of this baseline |
| `IsActive` | `bool` | Yes | Only one active baseline per project |
| `CreatedAt` | `DateTime` | Yes | |
| `CreatedBy` | `string(100)` | Yes | User who set the baseline |

**Unique Constraint (filtered):** `(ProjectId)` where `IsActive = true` — only one active baseline per project.

**Navigation Properties:**
- `Rows: ICollection<CostBaselineRow>`

---

### CostBaselineRow

An individual row in the baseline snapshot, representing one activity's planned cost and schedule.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `BaselineId` | `Guid` | Yes | FK → CostBaseline |
| `ActivityId` | `Guid` | Yes | FK → Activity |
| `PlannedCost` | `decimal(18,2)` | Yes | Snapshot of activity's total planned cost |
| `PlannedStart` | `DateTime` | Yes | Snapshot of activity's planned start date |
| `PlannedFinish` | `DateTime` | Yes | Snapshot of activity's planned finish date |
| `BudgetAtCompletion` | `decimal(18,2)` | Yes | BAC from the Budget at time of baseline |

**Navigation Properties:**
- `Baseline: CostBaseline`

**Index:** `(BaselineId, ActivityId)` for EVM lookup.

---

### ActualCost

The real cost incurred for an activity, entered manually or imported.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ActivityId` | `Guid` | Yes | FK → Activity (unique per activity) |
| `Amount` | `decimal(18,2)` | Yes | Total actual cost amount |
| `Currency` | `string(3)` | Yes | ISO 4217 |
| `Source` | `ActualCostSource` enum | Yes | Manual or Imported |
| `ImportBatchId` | `string(50)?` | No | Identifies the import batch (for audit) |
| `EntryDate` | `DateTime` | Yes | Date the actual cost was recorded |
| `IsOrphaned` | `bool` | Yes | True if the referenced Activity has been deleted (FR-032) |
| `DeletedActivityId` | `Guid?` | No | Original ActivityId before deletion |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique Constraint:** `(ActivityId)` — one actual cost record per activity (upsert on import).

**Index:** `(ProjectId)` for project-level queries.

---

### CashFlowPeriod

Computed (not stored). Derived from activity costs, dates, and actual costs.

| Field | Type | Notes |
|-------|------|-------|
| `PeriodStart` | `DateTime` | Start of period (weekly or monthly) |
| `PeriodEnd` | `DateTime` | End of period |
| `PlannedCost` | `decimal(18,2)` | Sum of planned costs spread across this period |
| `ActualCost` | `decimal(18,2)` | Sum of actual costs for this period |
| `CumulativePlanned` | `decimal(18,2)` | Running total of planned costs |
| `CumulativeActual` | `decimal(18,2)` | Running total of actual costs |

---

## Enums

### BudgetRevisionType

```csharp
public enum BudgetRevisionType
{
    Original,
    Revised,
    Approved
}
```

### BudgetRevisionStatus

```csharp
public enum BudgetRevisionStatus
{
    Pending,
    Approved
}
```

### DirectCostCategory

```csharp
public enum DirectCostCategory
{
    Permits,
    Insurance,
    Overhead,
    Preliminaries,
    Mobilization,
    Demobilization,
    Testing,
    Other,
    Custom
}
```

### DirectCostScope

```csharp
public enum DirectCostScope
{
    Project,
    Activity
}
```

### ActualCostSource

```csharp
public enum ActualCostSource
{
    Manual,
    Imported
}
```

### BudgetStatus

```csharp
public enum BudgetStatus
{
    Draft,
    Active,
    Closed
}
```

---

## Validation Rules

### Budget

| Rule | Condition | Error |
|------|-----------|-------|
| Contingency consistency | Both `ContingencyAmount` and `ContingencyPercent` set | "Set contingency as either absolute amount or percentage, not both" |
| Manual override | If `IsManualOverride`, `ManualTotalBudget` must be set | "Manual total budget is required when override is enabled" |
| Single active | Only one Budget per ProjectId | "A budget already exists for this project" |
| Resource cost warning | `ResourceCostTotal` changed since last save | "Resource costs have changed since the budget was last saved. Consider updating the budget." (FR-030) |

### BudgetRevision

| Rule | Condition | Error |
|------|-----------|-------|
| Status transition | Cannot approve an already-approved revision | "This revision has already been approved" |
| Type uniqueness | Only one Approved revision per `RevisionType` | "An approved revision of type '{type}' already exists" |
| Amount | `Amount > 0` | "Revision amount must be greater than zero" |

### DirectCost

| Rule | Condition | Error |
|------|-----------|-------|
| Quantity | `Quantity > 0` | "Quantity must be greater than zero" |
| UnitRate | `UnitRate > 0` | "Unit rate must be greater than zero" |
| Category consistency | If `Category == Custom`, `CustomCategoryName` must be set | "Custom category name is required" |
| Scope consistency | If `Scope == Activity`, `ActivityId` must be set | "Activity-scoped direct costs must reference an activity" |
| Activity exists | `ActivityId` references an existing activity (at creation) | "Referenced activity does not exist" |

### CostBaseline

| Rule | Condition | Error |
|------|-----------|-------|
| Active uniqueness | Only one active baseline per project | "An active baseline already exists for this project. Deactivate it before creating a new one." |
| Has rows | Baseline must capture at least one activity | "Cannot create an empty baseline" |

### ActualCost

| Rule | Condition | Error |
|------|-----------|-------|
| Amount | `Amount >= 0` | "Actual cost cannot be negative" |
| Activity exists | `ActivityId` references an existing activity (at creation) | "Referenced activity does not exist" |
| Import file size | Rows > 10000 | "Import file exceeds maximum of 10,000 rows" (FR-033) |
| Import error rate | Unmatched rows > 20% of total | "Import aborted: {X}% of rows contain unmatched activity codes (limit: 20%)" (FR-033) |
| Import performance | Rows >= 5000 | "Large import file ({N} rows). Performance may be impacted." (FR-033, warning) |

---

## State Transitions

### Budget Status

```
Draft ──activate──→ Active
Active ──close──→ Closed
Closed ──reopen──→ Active
```

- **Draft**: Initial state; budget can be freely edited
- **Active**: Budget is the current working budget; revisions can be created/approved
- **Closed**: Budget is finalized; no further edits allowed (archival)

### BudgetRevision Status

```
Pending ──approve──→ Approved
```

- **Pending**: New revision, editable, awaiting approval
- **Approved**: Locked; contributes to the effective budget

### DirectCost — Orphaned State

```
Active ──activity-deleted──→ Orphaned (IsOrphaned = true)
```

- **Orphaned**: The referenced activity was deleted; record preserved with `IsOrphaned = true` and `DeletedActivityId` set
- No direct user action transitions; triggered by activity deletion event

### ActualCost — Orphaned State

```
Active ──activity-deleted──→ Orphaned (IsOrphaned = true)
```

- Same orphaned semantics as DirectCost

### CostBaseline

```
Active ──deactivate──→ Inactive
Inactive ──set-as-active──→ Active
```

- Only one baseline can be active at a time
- Deactivating a baseline does not delete its rows (historical reference)

---

## Index Strategy

| Table | Index | Type | Purpose |
|-------|-------|------|---------|
| Budget | `(ProjectId)` | Unique | One budget per project |
| BudgetRevision | `(BudgetId, RevisionNumber)` | Unique | Revision ordering |
| BudgetRevision | `(BudgetId, Status)` | Non-clustered | Active revision filtering |
| DirectCost | `(ProjectId, Scope)` | Non-clustered | Breakdown tree query |
| DirectCost | `(ActivityId)` | Non-clustered | Activity-scoped cost lookup |
| CostBaseline | `(ProjectId)` | Filtered unique (IsActive=true) | Active baseline enforcement |
| CostBaselineRow | `(BaselineId, ActivityId)` | Non-clustered | EVM computation lookup |
| ActualCost | `(ActivityId)` | Unique | One record per activity |
| ActualCost | `(ProjectId)` | Non-clustered | Project-level queries |
