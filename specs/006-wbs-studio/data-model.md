# Data Model: WBS Studio

## Entity Relationship Summary

```
Project (1) ──has many──> Wbs (1) ──has many──> WbsItem (self-referencing tree)
WbsTemplate (1) ──has many──> WbsTemplateItem (self-referencing tree)
WbsItem ──optional FK──> BoqItem (SourceBoqItemId traceability)
Wbs ──optional FK──> Boq (SourceBoqId when generated from BOQ)
```

## Wbs

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| ProjectId | int | Associated project (FK to Project) |
| Name | string | WBS name |
| Description | string | Optional description |
| Revision | int | Incremented on status changes |
| Status | WbsStatus | Draft, Final, Revised, Approved (forward-only transitions) |
| Source | WbsSource | Manual, FromBOQ, FromTemplate, AIGenerated |
| SourceBoqId | Guid? | Source BOQ when generated from BOQ mapping |
| TotalWeight | decimal | Computed sum of top-level item weights |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**Validation rules**: Name required (max 200 chars); ProjectId required; Status transitions must follow Draft → Final → Approved → Revised → Draft; SourceBoqId optional; TotalWeight is computed (not directly settable).

## WbsItem

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| WbsId | Guid | Parent WBS (FK to Wbs) |
| ParentId | Guid? | Parent item for hierarchy (nullable; null = root-level) |
| Code | string | Auto-generated numeric code from tree position (e.g., "1", "1.1", "1.1.1") |
| ShortCode | string | Auto-generated alpha code from item name (min 3 letters) |
| Name | string | Work package title |
| Description | string | Optional description |
| Level | int | Depth in tree (0 = root, 1 = level 1, etc.) |
| SortOrder | int | Display order among siblings |
| WbsLevel | WbsLevelType | Summary, ControlAccount, WorkPackage, PlanningPackage |
| SourceBoqItemId | Guid? | Optional FK to BoqItem for BOQ traceability |
| Weight | decimal? | Percentage of total project work |
| PlannedStart | DateTime? | Planned start date |
| PlannedFinish | DateTime? | Planned finish date |
| DurationDays | int? | Duration in days |
| AssignedTo | string | Responsible party or role |
| Deliverable | string | Deliverable description |
| Notes | string | Optional notes |
| IsActive | bool | Soft-delete flag (default: true) |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**Validation rules**: WbsId required; Code and ShortCode auto-generated and unique within Wbs; Name required (max 200 chars); ParentId must reference existing item in same Wbs; Cannot create circular references; Weight must be ≤ 100% at sibling level; PlannedStart must precede PlannedFinish; WorkPackage cannot have children.

## WbsTemplate

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Template name |
| Description | string | Optional description |
| Category | string | e.g., "Building Construction", "Infrastructure" |
| Industry | string | e.g., "construction", "oil & gas" |
| IsStandard | bool | System-seeded (true) vs user-created (false) |
| Version | int | Template version |
| Tags | string | JSON array of tags |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

## WbsTemplateItem

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| TemplateId | Guid | Parent template (FK to WbsTemplate) |
| ParentId | Guid? | Parent template item (nullable; self-referencing) |
| Code | string | Numeric code from position |
| ShortCode | string | Alpha code from name |
| Name | string | Item name |
| Description | string | Optional description |
| Level | int | Depth in template tree |
| SortOrder | int | Display order among siblings |
| WbsLevel | WbsLevelType | Summary, ControlAccount, WorkPackage, PlanningPackage |
| DefaultDurationDays | int? | Default planning duration |
| TypicalWeight | decimal? | Typical weight percentage |
| Children | ICollection | Self-referencing children collection |

## Enums

### WbsStatus

| Value | Description |
|-------|-------------|
| Draft | Initial creation state |
| Final | Ready for use |
| Revised | Under revision (returned from Approved) |
| Approved | Final and approved |

**Transitions**: Draft → Final → Approved → Revised → Draft (forward-only cycle).

### WbsLevelType

| Value | Description |
|-------|-------------|
| Summary | Rollup node with children |
| ControlAccount | Management control point |
| WorkPackage | Assignable work item (leaf node) |
| PlanningPackage | Placeholder for future decomposition |

### WbsSource

| Value | Description |
|-------|-------------|
| Manual | Created manually from scratch |
| FromBOQ | Generated from BOQ mapping |
| FromTemplate | Applied from a template |
| AIGenerated | Suggested by AI and accepted |

## State Machine: WbsStatus

```
                    ┌─────────┐
                    │  Draft  │
                    └────┬────┘
                         │
                         v
                    ┌─────────┐
                    │  Final  │
                    └────┬────┘
                         │
                         v
                    ┌───────────┐
               ┌───│ Approved  │
               │   └─────┬─────┘
               │         │
               │         v
          ┌────────┐  (Revised reverts to Draft for re-approval cycle)
          │Revised │
          └────┬───┘
               │
               v
            (Draft)

No bypassing steps allowed. Each transition is an explicit user action.
```
