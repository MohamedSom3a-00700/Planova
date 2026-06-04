# Data Model: BOQ Studio

## Entity Relationship Summary

```
Project (1) ──has many──> Boq (1) ──has many──> BoqItem (self-referencing tree)
BoqClassification (self-referencing) ──assigned to──> BoqItem
BoqLibrary (1) ──has many──> BoqLibraryItem
```

## Boq

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| ProjectId | Guid | Associated project (FK to Project) |
| Name | string | BOQ name |
| Description | string | Optional description |
| Currency | string | ISO 4217 currency code (e.g., "USD", "SAR") |
| Status | BoqStatus | Current lifecycle status (Draft, Final, Revised, Approved) |
| RevisionNumber | int | Incremented on every status change |
| TotalAmount | decimal | Computed grand total (sum of root-level section amounts) |
| ImportSource | string | Source of import (e.g., "Excel", "CSV", "Manual") |
| ImportedAt | DateTime? | Timestamp of last import |
| Version | int | Optimistic locking concurrency token |
| CreatedAt | DateTime | Creation timestamp |
| ModifiedAt | DateTime | Last modification timestamp |
| CreatedBy | string | User who created the BOQ |
| ModifiedBy | string | User who last modified the BOQ |

**Validation rules**: Name required (max 200 chars); Currency must be a valid ISO 4217 code; ProjectId must reference an existing Project; Status transitions must follow the defined state machine; TotalAmount is computed (not directly settable).

## BoqItem

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| BoqId | Guid | Parent BOQ (FK to Boq) |
| ParentId | Guid? | Parent item for hierarchy (nullable; null = root-level section) |
| Code | string | Item code (e.g., "1", "1.1", "1.1.1") |
| Description | string | Item description |
| Unit | string | Unit of measure (e.g., "EA", "LS", "M2", "HR") |
| Quantity | decimal | Item quantity |
| Rate | decimal | Unit rate |
| Amount | decimal | Computed amount (Quantity * Rate) |
| ItemType | ItemType | Section, Item, or SubItem |
| Level | int | Depth level in the tree (0-based) |
| SortOrder | int | Display order among siblings |
| ClassificationId | Guid? | Optional assigned classification code (FK to BoqClassification) |
| CostCode | string | Optional external cost code reference |
| IsActive | bool | Soft-delete flag (default: true) |

**Validation rules**: BoqId required; Code required (max 50 chars); Code must be unique within the same BOQ; Quantity must be >= 0; Rate must be >= 0; Amount is computed as Quantity * Rate; Cannot create circular parent references; ParentId must reference an existing item in the same BOQ; Deleting a parent cascades to all descendants.

## BoqLibrary

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Library name |
| Description | string | Optional description |
| LibraryType | LibraryType | System or UserDefined |
| CreatedAt | DateTime | Creation timestamp |
| ModifiedAt | DateTime | Last modification timestamp |
| CreatedBy | string | User who created the library |

**Validation rules**: Name required (max 200 chars); LibraryType defaults to UserDefined.

## BoqLibraryItem

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| LibraryId | Guid | Parent library (FK to BoqLibrary) |
| Code | string | Item code |
| Description | string | Item description |
| Unit | string | Default unit of measure |
| DefaultRate | decimal | Default unit rate |
| Category | string | Optional category for discoverability |
| Tags | string | Comma-separated tags for search |

**Validation rules**: LibraryId required; Code required; Description required; DefaultRate >= 0.

## BoqClassification

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| ParentId | Guid? | Parent classification (self-referencing, nullable) |
| Code | string | Classification code |
| Name | string | Classification name |
| Description | string | Optional description |
| Scope | ClassificationScope | Project or Global |
| ProjectId | Guid? | Associated project if Project-scoped |

**Validation rules**: Code required (max 50 chars); Code must be unique within scope; ParentId must reference an existing classification; Circular references prohibited.

## Enums

### BoqStatus

| Value | Description | Transitions To |
|-------|-------------|----------------|
| Draft | Initial state upon creation | Final |
| Final | Ready for use/approval | Revised, Approved |
| Revised | Rolled back from Final for edits | Final, Approved |
| Approved | Terminal state — no further edits | (none) |

**Transitions**: Draft → Final → (Revised ⇄ Final) → Approved. Approved is terminal.

### ItemType

| Value | Description |
|-------|-------------|
| Section | A grouping item with children (subtotal calculated) |
| Item | A leaf item with quantity and rate |
| SubItem | A child of an Item (for detailed breakdown) |

### LibraryType

| Value | Description |
|-------|-------------|
| System | Standard library shipped with Planova |
| UserDefined | User-created library |

### ClassificationScope

| Value | Description |
|-------|-------------|
| Project | Classification is scoped to a single project |
| Global | Classification is available across all projects |

## State Diagrams

### BOQ Status Lifecycle

```
         ┌─────────────────────────────┐
         │                             │
         v                             │
    ┌────────┐     ┌────────┐     ┌──────────┐
    │ Draft  │────>│ Final  │────>│ Approved │
    └────────┘     └────────┘     └──────────┘
                        │  ^
                        │  │
                        v  │
                    ┌─────────┐
                    │ Revised │
                    └─────────┘
```

### Import Request Flow

```
User selects file
       │
       v
Column Mapping (auto-detect headers, user maps columns)
       │
       v
Tree Preview (flat rows assembled into hierarchy)
       │
       v
Validation (code uniqueness, parent existence, circular refs, units, ranges)
       │
       v
Commit (batch-wise atomic commits, all-or-nothing per batch)
       │
       v
Result (success count, error details, duplicate handling)
```

### Edit Operation Flow

```
User edits inline (quantity, rate, description)
       │
       v
Value validation (range, type)
       │
       v
Amount recomputed locally
       │
       v
Parent subtotals recalculated
       │
       v
Save (optimistic lock check — version increment)
       │
       v
On conflict → prompt user to reload and retry
```
