# Data Model: Activity Studio

## Entity Relationship Summary

```
Project (1) ──has many──> Activity (self-referencing via WbsSummary children)
                                                     │
                                                     ├──> ActivityRelationship (predecessor/successor)
                                                     ├──> Calendar (assigned via CalendarId)
                                                     └──> WbsItem (optional traceability FK)

Calendar (1) ──has many──> CalendarDay (specific date working status)

ActivityBank (1) ──has many──> ActivityBankItem (self-referencing tree)
                                                    │
                                                    └──> ActivityBankItemRelationship (default FS logic within entry)
```

## Activity

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| ProjectId | int | Associated project (FK to Project) |
| ParentActivityId | Guid? | Parent activity for WbsSummary rollup hierarchy (nullable) |
| WbsItemId | Guid? | Optional FK to WbsItem for WBS traceability (nullable) |
| CalendarId | Guid? | Assigned calendar for date calculations (nullable; inherits project default) |
| Code | string | Auto-generated sequential per project (e.g., "A-001", "A-002") |
| Name | string | Activity title |
| Description | string | Optional description |
| ActivityType | ActivityType | Task, Milestone, LevelOfEffort, WbsSummary |
| Status | ActivityStatus | NotStarted, InProgress, Completed, OnHold, Revise |
| Duration | int? | Duration in working days (not set for milestones; auto-rollup for WbsSummary) |
| PlannedStart | DateTime? | Planned start date (auto-rollup for WbsSummary) |
| PlannedFinish | DateTime? | Planned finish date (auto-rollup for WbsSummary) |
| ActualStart | DateTime? | Actual start date |
| ActualFinish | DateTime? | Actual finish date |
| PercentComplete | decimal? | 0-100 (auto-rollup for WbsSummary) |
| Weight | decimal? | Percentage contribution to total project value (0-100) |
| Notes | string | Free-text notes |
| SortOrder | int | Display order among siblings in WbsSummary hierarchy |
| IsActive | bool | Soft-delete flag (default: true) |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**Validation rules**:
- Name required (max 300 chars)
- ProjectId required
- Code auto-generated and unique within ProjectId
- PlannedStart must precede PlannedFinish (if both set)
- Duration must be > 0 for Task and LevelOfEffort; must be 0 for Milestone
- Duration, PlannedStart, PlannedFinish, and PercentComplete are NOT directly editable when ActivityType = WbsSummary (auto-rollup from child activities)
- Weight must be between 0 and 100 (if set); sum of sibling weights should not exceed 100
- ParentActivityId must reference an existing WbsSummary-type activity (WbsSummary can only have child activities of Task, Milestone, LevelOfEffort types)
- CalendarId must reference an existing Calendar (or null to use project default)
- Cannot delete activity that is a predecessor of other relationships without confirmation

## ActivityRelationship

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| ProjectId | int | Associated project (FK to Project) |
| PredecessorId | Guid | FK to Activity (predecessor) |
| SuccessorId | Guid | FK to Activity (successor) |
| Type | RelationshipType | FS, SS, FF, SF |
| LagDays | int | Lag in working days (positive = delay, negative = overlap; default: 0) |
| Description | string | Optional description |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**Validation rules**:
- PredecessorId and SuccessorId must reference existing activities in the same ProjectId
- PredecessorId must not equal SuccessorId (no self-referencing relationships — FR-011)
- No duplicate relationship (same predecessor + successor + type combination)
- Must not create circular references (detected via DFS cycle check — FR-010)
- LagDays can be negative (overlap) but should not make successor finish before predecessor starts in FS type

## Calendar

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| ProjectId | int? | Null = Global calendar; non-null = Project-specific |
| Name | string | Calendar name (e.g., "Standard 5-Day", "Sunday-Thursday") |
| Type | CalendarType | Global (shared across projects) or Project |
| HoursPerDay | decimal | Working hours per day (default: 8) |
| DaysPerWeek | int | Working days per week (default: 5) |
| Monday | bool | Is Monday a working day (default: true) |
| Tuesday | bool | Is Tuesday a working day (default: true) |
| Wednesday | bool | Is Wednesday a working day (default: true) |
| Thursday | bool | Is Thursday a working day (default: true) |
| Friday | bool | Is Friday a working day (default: false) |
| Saturday | bool | Is Saturday a working day (default: false) |
| Sunday | bool | Is Sunday a working day (default: false) |
| IsDefault | bool | Whether this is the project default calendar |
| Description | string | Optional description |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**Validation rules**:
- Name required (max 200 chars)
- At least one day per week must be marked as working day (otherwise date calculations degenerate; warn user but allow — edge case from spec)
- HoursPerDay must be > 0 and ≤ 24
- DaysPerWeek must be between 1 and 7 (should match actual working day flags)
- ProjectId null = Global; ProjectId set = Project-specific
- Only one calendar per project may have IsDefault = true
- Pre-seeded Global calendars: Standard 5-Day (Sun-Thu), Standard 6-Day (Sun-Fri), Standard 7-Day (all days)

## CalendarDay

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| CalendarId | Guid | FK to Calendar |
| Date | DateTime | The specific date (date-only, no time component) |
| Status | CalendarDayStatus | Working, NonWorking, Exception |
| Label | string | Optional label for exception days (e.g., "Public Holiday", "Shutdown") |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**Validation rules**:
- CalendarId required
- Date is unique per CalendarId (only one entry per date per calendar)
- Status = Exception requires a Label (max 200 chars)
- Bulk operations on date ranges must be efficient (single SQL UPDATE WHERE CalendarId AND Date BETWEEN)

## ActivityBank

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| Category | string | Construction category (e.g., "Preliminary", "Earthworks", "Concrete") |
| Subcategory | string | Optional subcategory (e.g., "Columns", "Slabs") |
| Code | string | Bank entry code (e.g., "CONC-COL-001") |
| Name | string | Entry name (e.g., "Column Concrete - Typical") |
| Description | string | Description of the construction method |
| IsStandard | bool | System-seeded (true) vs user-created (false) |
| Version | int | Entry version (for future updates) |
| Tags | string | JSON array of search tags |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**Validation rules**:
- Name required (max 200 chars)
- Category required (must be one of 13 recognized categories: Preliminary, Earthworks, Concrete, Formwork, Reinforcement, Steel, Masonry, Waterproofing, MEP, Finishing, Infrastructure, Landscaping, Testing & Handover)
- Code unique per category (e.g., "CONC-COL-001")
- IsStandard entries cannot be deleted (can be hidden); only custom entries are user-deletable

## ActivityBankItem

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| BankId | Guid | FK to ActivityBank |
| ParentId | Guid? | Parent bank item for hierarchical task breakdown (nullable) |
| Code | string | Sequential item code within bank entry (e.g., "1", "1.1") |
| Name | string | Sub-task name |
| Description | string | Optional description |
| Level | int | Depth in breakdown tree |
| SortOrder | int | Display order among siblings |
| DefaultDuration | int | Default duration in working days |
| DefaultActivityType | ActivityType | Task (default), Milestone, LevelOfEffort (WbsSummary not valid in bank items) |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**Validation rules**:
- BankId required
- Name required (max 300 chars)
- DefaultDuration must be > 0 (except for Milestone which is 0)
- DefaultActivityType cannot be WbsSummary (bank items cannot be rollup summaries)
- Self-referencing tree structure: ParentId must reference existing item in same BankId

## ActivityBankItemRelationship

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| BankId | Guid | FK to ActivityBank |
| PredecessorItemId | Guid | FK to ActivityBankItem (predecessor) |
| SuccessorItemId | Guid | FK to ActivityBankItem (successor) |
| Type | RelationshipType | Default logic: FS, SS, FF, SF |
| DefaultLagDays | int | Default lag in working days (default: 0) |

**Validation rules**:
- PredecessorItemId and SuccessorItemId must reference existing items in the same BankId
- PredecessorItemId must not equal SuccessorItemId
- No duplicate relationship in same bank entry
- Relationships within a bank entry must form a DAG (validated on save)

## Enums

### ActivityType

| Value | Description |
|-------|-------------|
| Task | Standard duration-based activity |
| Milestone | Zero-duration marker activity |
| LevelOfEffort | Support activity spanning its children's duration |
| WbsSummary | Auto-rollup activity representing a parent WBS item (derived dates/duration/percent complete) |

### ActivityStatus

| Value | Description |
|-------|-------------|
| NotStarted | Initial state |
| InProgress | Work has begun |
| Completed | Work is finished |
| OnHold | Temporarily paused (reversible from InProgress) |
| Revise | Returned for revision (reversible from InProgress) |

### RelationshipType

| Value | Description |
|-------|-------------|
| FS | Finish-to-Start: successor starts after predecessor finishes |
| SS | Start-to-Start: successor starts when predecessor starts |
| FF | Finish-to-Finish: successor finishes when predecessor finishes |
| SF | Start-to-Finish: successor finishes when predecessor starts |

### CalendarType

| Value | Description |
|-------|-------------|
| Global | Shared across all projects (e.g., Standard 5-Day) |
| Project | Specific to a single project (exceptions, holidays, custom shutdowns) |

### CalendarDayStatus

| Value | Description |
|-------|-------------|
| Working | Normal working day |
| NonWorking | Scheduled non-working day |
| Exception | Exception (holiday or shutdown), requires a label |

## State Machine: ActivityStatus

```
                        ┌────────────┐
                        │ NotStarted │
                        └──────┬─────┘
                               │
                               v
                        ┌────────────┐
                   ┌───│ InProgress │────┐
                   │   └──────┬─────┘    │
                   │          │          │
                   v          v          v
              ┌────────┐ ┌────────┐ ┌──────────┐
              │OnHold  │ │Revise  │ │Completed │
              └────────┘ └────────┘ └──────────┘
                   │          │
                   └────┬─────┘
                        │
                        v
                   ┌────────────┐
                   │ InProgress │  (resume from OnHold/Revise)
                   └────────────┘

Transitions:
- NotStarted → InProgress (start work)
- InProgress → Completed (finish work)
- InProgress → OnHold (pause, reason required)
- InProgress → Revise (return for revision, reason required)
- OnHold → InProgress (resume)
- Revise → InProgress (resume after revision)

No transitions from Completed (terminal state for status purposes).
No direct NotStarted → Completed (cannot skip working state).
No direct OnHold/Revise → Completed (must resume to InProgress first).
```

## State Machine: ActivityType (Bank Item Generation)

```
Bank Item Apply triggers Activity creation:
                    ┌──────────────────────┐
                    │ ActivityBankItem     │
                    │ (DefaultActivityType)│
                    └──────────┬───────────┘
                               │
                               v
                    ┌──────────────────────┐
                    │       Activity       │
                    │ (ActivityType = copy │
                    │  from bank item)     │
                    └──────────────────────┘

WbsSummary auto-rollup (not a state machine — computed on read):
                    ┌──────────────────────┐
                    │  Activity (WbsSummary)│
                    │  reads child activity │
                    │  dates/duration/pct   │
                    └──────────────────────┘
```

## Indexes (Persistence)

| Table | Column(s) | Purpose |
|-------|-----------|---------|
| Activity | ProjectId | Filter activities by project |
| Activity | WbsItemId | Traceability lookup from WBS |
| Activity | CalendarId | Calendar assignment lookup |
| Activity | Status | Filter by status |
| Activity | Code + ProjectId | Unique code per project |
| Activity | ParentActivityId | WbsSummary child traversal |
| ActivityRelationship | ProjectId | Filter by project |
| ActivityRelationship | PredecessorId | Predecessor lookup |
| ActivityRelationship | SuccessorId | Successor lookup |
| Calendar | ProjectId | Filter by project (null = Global) |
| CalendarDay | CalendarId | Calendar day lookup |
| CalendarDay | CalendarId + Date | Unique date per calendar |
| ActivityBank | Category | Category browsing |
| ActivityBank | Code + Category | Unique code |
| ActivityBankItem | BankId | Items within bank entry |
| ActivityBankItem | ParentId | Hierarchical traversal |
| ActivityBankItemRelationship | BankId | Relationships within entry |

## Seed Data

### Calendar Seeds

| Name | Type | Days | Hours/Day |
|------|------|------|-----------|
| Standard 5-Day Work Week | Global | Sun-Thu | 8 |
| Standard 6-Day Work Week | Global | Sun-Fri | 8 |
| Standard 7-Day Work Week | Global | All days | 8 |

### Activity Bank Seeds (50+ entries across 13 categories)

Seeded via JSON configuration file loaded on first access. Structure per entry:

```json
{
  "category": "Concrete",
  "subcategory": "Columns",
  "code": "CONC-COL-001",
  "name": "Column Concrete - Typical",
  "description": "Standard reinforced concrete column construction sequence",
  "isStandard": true,
  "items": [
    { "code": "1", "name": "Column Rebar Fixing", "duration": 3, "type": "Task" },
    { "code": "2", "name": "Column Formwork Installation", "duration": 2, "type": "Task" },
    { "code": "3", "name": "Column Concrete Pouring", "duration": 1, "type": "Task" },
    { "code": "4", "name": "Column Formwork Stripping", "duration": 1, "type": "Task" },
    { "code": "5", "name": "Column Curing", "duration": 7, "type": "Task" }
  ],
  "relationships": [
    { "predecessor": "1", "successor": "2", "type": "FS", "lag": 0 },
    { "predecessor": "2", "successor": "3", "type": "FS", "lag": 0 },
    { "predecessor": "3", "successor": "4", "type": "FS", "lag": 1 },
    { "predecessor": "4", "successor": "5", "type": "FS", "lag": 0 }
  ]
}
```
