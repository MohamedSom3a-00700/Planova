# Planova Phase 5 Implementation Plan

**Phase**: 5 — Activity Studio

**Date**: 2026-06-04

**Source of Truth**: [docs/01-PRODUCT_VISION.md](./01-PRODUCT_VISION.md),
[docs/02-MASTER_ROADMAP.md](./02-MASTER_ROADMAP.md),
[docs/04-SYSTEM_ARCHITECTURE.md](./04-SYSTEM_ARCHITECTURE.md),
[docs/06-MODULE_CATALOG.md](./06-MODULE_CATALOG.md),
[docs/07-DATABASE_STRATEGY.md](./07-DATABASE_STRATEGY.md),
[docs/09-AI_STRATEGY.md](./09-AI_STRATEGY.md),
[docs/11-UI_UX_DESIGN_SYSTEM.md](./11-UI_UX_DESIGN_SYSTEM.md),
[docs/PLANOVA_CONSTITUTION_DRAFT.md](./PLANOVA_CONSTITUTION_DRAFT.md),
[docs/PHASE_3_IMPLEMENTATION_PLAN.md](./PHASE_3_IMPLEMENTATION_PLAN.md),
[docs/PHASE_4_IMPLEMENTATION_PLAN.md](./PHASE_4_IMPLEMENTATION_PLAN.md)

## Summary

Phase 5 delivers the **Activity Studio** — the fourth specialized studio in the
Planova project controls workflow and the natural next step after WBS Studio
(Phase 4). This phase transforms hierarchical WBS work packages into a detailed
project schedule with activities, logic relationships, milestones, calendars,
and a reusable **Activity Bank** of pre-defined construction method sequences.

The ultimate workflow progression:

```
BOQ (Phase 3) → WBS (Phase 4) → Activities (Phase 5) → Resources (Phase 6) → Cost Loading (Phase 7)
```

The **Activity Bank** is the major architectural innovation in this phase. It
serves as a reusable knowledge base of construction method sequences at the
work-package level — for example, "Column Concrete" breaks down into
Formwork → Reinforcement → Pouring → Curing → Stripping → Finishing, each with
default durations and predecessor-successor logic. The bank is seeded with 50+
entries across 13 construction categories (Concrete, Excavation, Steel, Masonry,
MEP, Finishing, Infrastructure, Landscaping, etc.) and can be extended by users.

This phase does not cover resource loading, cost loading, cash flow, or earned
value analysis. Those are separate downstream phases (6, 7, 18) that will
consume the structured activity schedule output.

## Phase 5 Objectives

1. Introduce the Activity domain model (Activity, ActivityRelationship, Calendar,
   CalendarDay, ActivityBank, ActivityBankItem).
2. Build a Gantt chart viewer and activity list with filter/search by project,
   WBS, and status.
3. Implement an activity editor with full CRUD, status management, and
   inline editing of durations, dates, and assignments.
4. Build a predecessor-successor relationship editor with visual logic display
   and circular reference detection.
5. Create a Calendar manager with CRUD for base calendars, working days,
   exceptions/holidays, and calendar assignment to activities.
6. Build the **Activity Bank** — a reusable library of pre-defined construction
   method sequences seeded with 50+ entries across 13 categories, each with
   hierarchical sub-task breakdowns and default FS logic.
7. Implement a WBS-to-Activity generation wizard that creates activities from
   WBS items with the option to apply Activity Bank templates.
8. Deliver schedule reports with Excel and PDF export.
9. Preserve Clean Architecture, MVVM, localization (English + Arabic), and
   theme consistency.
10. Ensure the activity output is structured for consumption by Phase 6
    (Resource Studio) and Phase 7 (Cost Studio).

## Phase 5 Scope

### In Scope

- Activity domain entity with project and WBS item association
- ActivityRelationship domain entity for predecessor-successor logic
- Calendar and CalendarDay domain entities for working time definition
- ActivityBank and ActivityBankItem domain entities for reusable method sequences
- Activity Bank seeding (50+ entries across 13 construction categories)
- Activity list view with filter, search, and status indicators
- Gantt chart view with horizontal bars, milestones, and relationship arrows
- Activity inline editor (name, code, dates, duration, status, type, weight)
- Activity status state machine (NotStarted → InProgress → Completed, with
  OnHold/Revise transitions)
- Predecessor-successor relationship editor (FS, SS, FF, SF with lag)
- Circular relationship detection and prevention
- Calendar manager (CRUD, working days, exceptions, hours per day)
- Calendar assignment to individual activities or groups
- WBS-to-Activity generation wizard (select WBS items → generate activities)
- Activity Bank browser with category tree, search, and entry preview
- Apply Activity Bank entry to WBS item (generates sub-tasks + relationships)
- Schedule reports with Excel and PDF export
- Navigation rail integration — Activity Studio opens in multi-tab workspace
- Database persistence — new tables: Activities, ActivityRelationships,
  Calendars, CalendarDays, ActivityBanks, ActivityBankItems
- Localization (English and Arabic) for all Activity screens
- Unit and integration tests for domain, services, validation, generation

### Out of Scope

- Resource loading or crew assignment (Phase 6)
- Cost loading or cash flow (Phase 7)
- Earned value or performance metrics (Phase 18)
- Primavera P6 XER import/export (Phase 9)
- Schedule comparison or baseline management (Phase 10)
- Delay analysis or forensic scheduling (Phase 11)
- Multi-user collaboration on schedules (Phase 19)
- AI-assisted activity generation (deferred to Phase 16 AI Copilot)
- Automatic scheduling (CPM engine — manual logic-driven only)
- Drag-and-drop Gantt bar resizing (deferred to enhancement)
- Resource leveling or smoothing
- Progress tracking against baseline

## Non-Negotiable Constraints

- Clean Architecture must remain intact.
- MVVM must remain the UI pattern.
- Localization must support English and Arabic.
- RTL behavior must remain correct.
- Theme support must remain consistent with Phases 0/1/2/3/4.
- Phase 4 WBS module must be consumed (not duplicated) — WbsItemId on Activity.
- Phase 2 Excel/PDF infrastructure must be reused for reports.
- Self-referencing tree must support large activity structures with virtualized UI.
- The Activity data model must accommodate downstream Resource and Cost
  integration.
- Calendar model must support both global (default) and project-specific calendars.
- Activity Bank must be extensible — users create custom entries beyond seeded data.
- Relationship validation must prevent circular logic at the application layer.
- All async operations must accept CancellationToken.

## Phase 5 Product Shape

Phase 5 should feel like the natural schedule companion to WBS Studio.

The user should be able to:

- browse activities for a project in a filterable list view
- view activities on a Gantt chart with timeline bars and logic arrows
- create, edit, and delete individual activities (name, code, status, dates,
  duration, calendar, type, weight)
- define predecessor-successor relationships between activities with lag
- see relationship chains visually on the Gantt chart
- manage calendars — create global and project-specific calendars, set working
  days, add holiday exceptions, define hours per day
- assign a calendar to one or more activities
- generate activities from WBS work packages (one WBS item → one activity)
- browse the Activity Bank by construction category
- preview an Activity Bank entry's full sub-task breakdown with default durations
  and relationships
- apply an Activity Bank entry to a WBS item — generating the complete sub-task
  tree with default logic in a single operation
- create custom Activity Bank entries from existing activity groups
- generate a schedule report with activity list, dates, durations, and logic
- export the schedule to Excel or PDF

## Target Solution Impact

Phase 5 extends the foundation established in Phases 0, 1, 2, 3, and 4.

Expected additions:

- new Domain entities in a new `Planova.Activity` project
- new Application services for Activity use cases
- new Infrastructure/Persistence mappings, repositories, and migrations
- new UI modules for Activity Studio (list, Gantt, editor, relationships,
  calendars, bank, generation, reports)
- expanded localization resources for Activity terminology
- seeded Activity Bank data (50+ construction method entries)

## Primary Domain Model

### Activity

Represents a single schedule activity or task.

Properties:

- Id (Guid)
- ProjectId (int, FK → Projects)
- WbsItemId (Guid?, FK → WbsItem — optional traceability to WBS)
- CalendarId (Guid?, FK → Calendar)
- Code (string — auto-generated, e.g. "A-001")
- Name (string — activity description)
- Description (string?)
- ActivityType (ActivityType: Task, Milestone, LevelOfEffort, WbsSummary)
- Status (ActivityStatus: NotStarted, InProgress, Completed, OnHold, Revise)
- DurationDays (int?)
- RemainingDays (int?)
- PercentComplete (decimal, 0–100)
- PlannedStart (DateTime?)
- PlannedFinish (DateTime?)
- ActualStart (DateTime?)
- ActualFinish (DateTime?)
- Weight (decimal?)
- IsMilestone (bool — shortcut flag, zero duration)
- Notes (string?)
- CreatedAt
- UpdatedAt

Navigation:

- PredecessorRelationships (ICollection<ActivityRelationship> — as successor)
- SuccessorRelationships (ICollection<ActivityRelationship> — as predecessor)
- Calendar (Calendar?)
- WbsItem (WbsItem?)

### ActivityRelationship

Represents a predecessor-successor logical tie between two activities.

Properties:

- Id (Guid)
- PredecessorActivityId (Guid, FK → Activity)
- SuccessorActivityId (Guid, FK → Activity)
- RelationshipType (RelationshipType: FinishToStart, StartToStart,
  FinishToFinish, StartToFinish)
- LagDays (int — default 0)
- CreatedAt

Navigation:

- PredecessorActivity (Activity)
- SuccessorActivity (Activity)

### Calendar

Represents a working time calendar (global, project-specific, or resource).

Properties:

- Id (Guid)
- Name (string — e.g. "Standard 5-Day", "6-Day Work Week")
- Description (string?)
- IsBaseCalendar (bool — system default vs user-created)
- CalendarType (CalendarType: Global, Project, Resource)
- HoursPerDay (decimal — default 8)
- DaysPerWeek (int — default 5)
- CreatedAt
- UpdatedAt

Navigation:

- Days (ICollection<CalendarDay>)

### CalendarDay

Represents a specific date's working status within a calendar.

Properties:

- Id (Guid)
- CalendarId (Guid, FK → Calendar)
- Date (DateTime — the specific date)
- IsWorkingDay (bool — working or non-working)
- StartTime (TimeSpan? — custom day start)
- EndTime (TimeSpan? — custom day end)
- IsException (bool — marks holiday or variance)

### ActivityBank

Represents a reusable construction method template at the work-package level.
Each entry contains a full hierarchy of sub-tasks with default durations and
logic relationships.

Properties:

- Id (Guid)
- Category (string — e.g. "Concrete Works", "Excavation", "Landscaping")
- Subcategory (string? — e.g. "Columns", "Slabs", "Beams")
- Code (string — standard code, e.g. "CONC-COL-001")
- Name (string — e.g. "Column Concrete")
- Description (string? — method statement summary)
- UnitOfMeasure (string? — m³, m², each, ton)
- DefaultDurationDays (int? — typical total duration)
- IsStandard (bool — system-seeded vs user-created)
- Tags (string? — JSON array for search/filter)
- CreatedAt
- UpdatedAt

Navigation:

- Items (ICollection<ActivityBankItem>)
- Relationships (ICollection<ActivityBankItemRelationship>)

### ActivityBankItem

Represents a single sub-task within an Activity Bank entry.

Properties:

- Id (Guid)
- BankId (Guid, FK → ActivityBank)
- ParentId (Guid?, self-referencing FK → ActivityBankItem)
- Code (string — e.g. "CONC-COL-001-FORM")
- Name (string — e.g. "Formwork Installation")
- Description (string?)
- Level (int — 0 = root, 1, 2)
- SortOrder (int)
- ActivityType (ActivityType: Task, Milestone, LevelOfEffort)
- DefaultDurationDays (int?)
- IsCritical (bool — suggested as critical-path activity)
- CreatedAt
- UpdatedAt

Navigation:

- Parent (ActivityBankItem?)
- Children (ICollection<ActivityBankItem>)
- Bank (ActivityBank)

### ActivityBankItemRelationship

Represents a default predecessor-successor relationship between two items
within the same Activity Bank entry.

Properties:

- Id (Guid)
- BankItemId (Guid, FK → ActivityBankItem — the successor)
- PredecessorItemId (Guid, FK → ActivityBankItem — the predecessor)
- RelationshipType (RelationshipType: FinishToStart, StartToStart,
  FinishToFinish, StartToFinish)
- LagDays (int — default 0)

### Value Objects

- **ActivityType** — Task, Milestone, LevelOfEffort, WbsSummary
- **ActivityStatus** — NotStarted, InProgress, Completed, OnHold, Revise
  (transition map: NotStarted → InProgress → Completed; OnHold ↔ InProgress;
  Revise ↔ InProgress)
- **RelationshipType** — FinishToStart (FS), StartToStart (SS),
  FinishToFinish (FF), StartToFinish (SF)
- **CalendarType** — Global (shared across all projects), Project (project-specific),
  Resource (resource-specific — reserved for Phase 6)

## Activity Bank Seed Data

The Activity Bank ships with 50+ pre-defined entries organized under 13
construction categories. Each entry contains 3–10 sub-task items with default
FS relationships and typical durations.

### Category 1: Preliminary / General

| Entry | Sub-tasks |
|-------|-----------|
| Site Mobilization | Site Setup, Office Establishment, Utilities Connection, Security Installation |
| Site Survey | Control Points, Topographic Survey, Setting Out |
| Temporary Facilities | Access Roads, Storage Areas, Crew Accommodation |
| Demobilization | Site Clearance, Facility Removal, Restoration, Final Clean |

### Category 2: Earthworks & Excavation

| Entry | Sub-tasks |
|-------|-----------|
| Bulk Excavation | Topsoil Stripping, Bulk Dig, Haulage, Stockpiling |
| Trench Excavation | Alignment Marking, Trench Dig, Shoring, Dewatering |
| Backfill & Compaction | Material Import, Layering, Compaction Testing |
| Soil Improvement | Soil Testing, Stabilization, Compaction, Verification |

### Category 3: Concrete Works

Each concrete entry follows the same method sequence:

| Entry | Sub-tasks |
|-------|-----------|
| Column Concrete | Formwork Install, Reinforcement, Pouring, Curing, Strip Formwork, Finish |
| Slab Concrete | Soffit Formwork, Slab Reinforcement, Pouring, Curing, Strip Formwork |
| Beam Concrete | Beam Formwork, Beam Reinforcement, Pouring, Curing, Strip Formwork |
| Wall Concrete | Wall Formwork, Wall Reinforcement, Pouring, Curing, Strip Formwork |
| Foundation Concrete | Excavation, Blinding, Foundation Formwork, Reinforcement, Pouring, Cure |
| Stair Concrete | Stair Formwork, Stair Reinforcement, Pouring, Curing, Strip Formwork |
| Retaining Wall Conc. | Wall Formwork, Reinforcement, Pouring, Curing, Waterproofing, Backfill |

### Category 4: Formwork

| Entry | Sub-tasks |
|-------|-----------|
| Soffit Formwork | Layout Marking, Props Installation, Plywood Fixing, Alignment Check |
| Edge Formwork | Edge Marking, Shuttering Fixing, Bracing, Alignment Check |
| Table Formwork | Table Assembly, Crane Lifting, Positioning, Adjustment |

### Category 5: Reinforcement

| Entry | Sub-tasks |
|-------|-----------|
| Column Rebar | Rebar Cutting, Rebar Bending, Cage Assembly, Tying, Spacers |
| Slab Rebar | Bottom Mesh, Top Mesh, Chair Support, Tying, Cover Blocks |
| Beam Rebar | Main Bars, Stirrups, Tying, Spacers, Cover |
| Wall Rebar | Vertical Bars, Horizontal Bars, Tying, Spacers |
| Post-Tensioning | Duct Installation, Strand Threading, Stressing, Grouting |

### Category 6: Steel / Metal Works

| Entry | Sub-tasks |
|-------|-----------|
| Structural Steel Erection | Column Erection, Beam Installation, Bolting, Welding, Paint Touch-up |
| Metal Decking | Deck Sheet Laying, Stud Welding, Edge Trim, Diaphragm Check |
| Handrails | Bracket Fixing, Rail Installation, Welding, Grinding, Painting |

### Category 7: Masonry & Blockwork

| Entry | Sub-tasks |
|-------|-----------|
| Block Laying | Layout, First Course, Vertical Reinforcement, Laying, Curing |
| Brickwork | Soaking, Laying, Jointing, Cleaning, Curing |
| Plastering | Surface Prep, Mixing, First Coat, Second Coat, Finishing, Curing |
| Screeding | Surface Prep, Mixing, Laying, Leveling, Curing |

### Category 8: Waterproofing

| Entry | Sub-tasks |
|-------|-----------|
| Basement Waterproofing | Surface Prep, Primer, Membrane Application, Protection Board, Testing |
| Roof Waterproofing | Slope Screed, Primer, Membrane, Insulation, Protection Layer |
| Wet Area Waterproofing | Surface Prep, Corner Sealing, Membrane, Curing, Slab Test |

### Category 9: MEP

| Entry | Sub-tasks |
|-------|-----------|
| HVAC Ductwork | Duct Fabrication, Hanger Install, Duct Lifting, Insulation, Leak Test |
| Plumbing Rough-in | Pipe Laying, Jointing, Pressure Test, Insulation |
| Electrical Conduit | Conduit Laying, Box Fixing, Pull Wire, Continuity Test |
| Fire Fighting | Pipe Install, Sprinkler Install, Pump Install, Hydro Test, Commission |
| ELV Systems | Cable Tray, Cable Laying, Termination, Testing |

### Category 10: Finishing

| Entry | Sub-tasks |
|-------|-----------|
| Ceramic Tiling | Surface Prep, Waterproofing, Adhesive, Tile Laying, Grouting, Cleaning |
| Painting | Surface Prep, Primer, First Coat, Second Coat, Touch-up |
| False Ceiling | Grid Install, Board Fixing, Jointing, Skimming, Painting |
| Flooring | Subfloor Prep, Underlay, Flooring Install, Trim, Finishing |
| Joinery | Measurements, Workshop Fabrication, Site Install, Adjustment, Finishing |
| Glazing | Frame Install, Glass Lifting, Sealing, Curing |

### Category 11: Infrastructure

| Entry | Sub-tasks |
|-------|-----------|
| Road Construction | Subgrade Prep, Sub-base, Base Course, Binder Course, Wearing Course, Markings |
| Drainage | Trench Excavation, Pipe Laying, Manhole Build, Backfill, Testing |
| Sewerage | Trench Excavation, Pipe Laying, Manhole Build, Connection, Testing |
| Water Supply | Trench Excavation, Pipe Laying, Jointing, Pressure Test, Disinfection |
| Street Lighting | Foundation, Pole Erection, Cable Laying, Luminaire Fixing, Testing |

### Category 12: Landscaping

| Entry | Sub-tasks |
|-------|-----------|
| Softscape | Soil Prep, Grading, Turf Laying, Planting, Mulching, Irrigation |
| Hardscape | Sub-base Prep, Pavers Laying, Joint Filling, Edging, Cleaning |
| Irrigation System | Trenching, Pipe Laying, Valve Install, Sprinkler Install, Testing |
| Fencing | Post Installation, Rail Fixing, Mesh Fixing, Gate Installation |
| Tree Planting | Pit Excavation, Soil Mix, Tree Placement, Staking, Watering |

### Category 13: Testing & Handover

| Entry | Sub-tasks |
|-------|-----------|
| System Testing | Equipment Check, Function Test, Integration Test, Performance Test |
| Commissioning | Pre-commissioning, Commissioning, Demonstration, Sign-off |
| Snagging | Walkthrough, Snag List, Rectification, Re-inspection |
| As-Built Documentation | Drawing Collection, Mark-up, Submission, Approval |

### Seeding Implementation

Follow the same lazy-seeding pattern as `WbsTemplateRepository`:

- `ActivityBankRepository.SeedIfEmptyAsync()` checks on first query
- Each category entry created by a private static factory method
- Deterministic GUIDs for repeatable seeding
- `IsStandard = true` for all seeded entries
- Entries allow user modification and duplication after seeding

## Screen Plan

### 1. Activity Studio Shell (Multi-tab Workspace)

- Main container with 8 subtabs (same pattern as WbsStudioView)
- Tab navigation: List, Gantt, Editor, Relationships, Calendars, Activity Bank,
  Generation, Reports

### 2. Activity List

- Filterable list by project, WBS, status, activity type
- Search by name or code
- Status indicator badges (color-coded dots: gray=NotStarted, blue=InProgress,
  green=Completed, orange=OnHold)
- WBS path column for traceability
- Sort by planned start, code, status
- Create new activity (with WBS item selector)
- Right-click context menu: Edit, Delete, Change Status, Copy

### 3. Gantt Chart

- Horizontal bar chart with time axis (day/week/month zoom)
- Bars positioned by PlannedStart → PlannedFinish
- Color-coded by ActivityStatus (same as list badges)
- Milestones displayed as diamond shapes
- Predecessor-successor arrows between related bars
- Left panel: activity list synchronized with bar positions
- Zoom controls (Day/Week/Month/Full Schedule)
- Scrollable timeline header
- Custom WPF Canvas control (lightweight, no heavy third-party dependency)

### 4. Activity Editor

- Inline editing: name, code, description, dates, duration, status, type
- Calendar selector dropdown
- WBS item picker (tree dialog from current project)
- Milestone toggle (auto-sets duration to 0)
- Percent complete slider
- Weight input
- Save/Cancel with validation feedback
- Notes field

### 5. Relationship Editor

- Tabular view: Predecessor, Successor, Type, Lag
- Add relationship (select predecessor and successor activities)
- Edit relationship type (FS/SS/FF/SF dropdown)
- Edit lag days
- Delete relationship with confirmation
- Circular reference check on save
- Visual relationship preview on Gantt chart

### 6. Calendar Manager

- Calendar list with type indicator (Global/Project/Resource)
- Create new calendar (select type, set working hours)
- Edit calendar properties (name, hours/day, days/week)
- Calendar day grid: monthly view with day-by-day working status
- Bulk set working/non-working for date ranges
- Add holiday exceptions (public holidays, shutdowns)
- Assign calendar to project (default) or to individual activities
- Clone calendar from existing

### 7. Activity Bank

- Category tree browser (left panel): 13 categories with expand/collapse
- Entry list (right panel): filterable by category, subcategory, search
- Entry preview: Full hierarchical breakdown with durations and relationship
  arrows in a readable tree view
- Apply to WBS item: Select target WBS item → click Apply → system generates
  sub-task activities with default logic
- Create custom entry: Select existing activities → Save as Bank Entry
  (name, category, tags)
- Edit custom entry: rename, reorder sub-tasks, adjust durations
- Delete custom entry (system-seeded entries are read-only but can be copied)

### 8. WBS-to-Activity Generation Wizard

3-step workflow:

1. **Select WBS Items** — Tree browser of project WBS. Checkbox selection of
   target work packages. Select all / deselect all.
2. **Generation Options** — Choose generation mode:
   - **Simple (1:1)**: One activity per selected WBS item
   - **From Activity Bank**: Select a Bank entry to apply to each WBS item
   - **Bulk Defaults**: Set default calendar, default duration formula,
     default status
3. **Preview & Commit** — Preview generated activity list with codes, names,
   dates, durations. Adjust as needed. Commit to database.

### 9. Activity Reports

- **Schedule Summary** — Sortable table: Code, Name, Dates, Duration, Status,
  Percent Complete, Predecessors/Successors
- **Activity List Export** — Full activity register with all fields
- Export to Excel (reuse Phase 2 WorkbookWriter)
- Export to PDF (QuestPDF)

## Implementation Order

### 1. Domain Model and Contracts

- Activity entity
- ActivityRelationship entity
- Calendar entity
- CalendarDay entity
- ActivityBank entity
- ActivityBankItem entity
- ActivityBankItemRelationship entity
- ActivityType, ActivityStatus, RelationshipType, CalendarType enums
- ActivityStatus transitions (NotStarted → InProgress → Completed; OnHold ↔
  InProgress; Revise ↔ InProgress)
- Repository and service interfaces

### 2. Persistence Schema and Mappings

- EF Core entity configurations for all 7 new entities
- Relationships: Activity → Project, Activity → WbsItem, Activity → Calendar;
  ActivityRelationship → Activity (x2); Calendar → CalendarDay;
  ActivityBank → ActivityBankItem (self-referencing)
- Indexes on Activity: ProjectId, WbsItemId, Status, Code
- Indexes on ActivityRelationship: PredecessorActivityId, SuccessorActivityId
- Indexes on CalendarDay: CalendarId, Date
- Indexes on ActivityBankItem: BankId, ParentId
- Migration for Activity tables
- Seed data for Activity Bank (50+ entries)

### 3. Application Services

- IActivityService — CRUD, status transitions, filter/search, bulk operations
- IActivityRelationshipService — CRUD, validation, circular reference detection
- ICalendarService — CRUD, day management, clone, default calendar
- IActivityBankService — CRUD, apply bank entry to WBS item, create custom entry
- IActivityGenerationService — WBS-to-activity generation, bank application
- IActivityValidationService — schedule validation, logic consistency
- IActivityReportService — schedule summary, export

### 4. Activity Validation Engine

- Required fields (Name, Code)
- Date validation (start before finish)
- Status transition validation
- Circular relationship detection (graph traversal from each activity)
- Self-referencing relationship prevention
- Cross-project relationship prevention
- Duration consistency (milestone = 0 days)
- Calendar assignment validation

### 5. Activity Bank Seeding

- 50+ bank entries across 13 categories (see seed data section above)
- Each entry: 3–10 sub-task items with FS relationships
- Lazy seeding on first repository access
- Entries organized by Category → Subcategory → Entry → Items

### 6. WBS-to-Activity Generation

- 1:1 generation (one WBS item = one activity)
- Activity Bank application (one bank entry expands into N sub-tasks)
- Default code generation from WBS item codes
- Calendar and duration defaults
- Preview before commit

### 7. UI — Activity Navigation and Registration

- Register Activity Studio nav target in ShellViewModel
- Replace existing placeholder with live ActivityStudioView
- Wire nav rail item with CalendarDay24 icon
- Ensure Activity Studio opens in multi-tab workspace

### 8. UI — Activity List and Gantt

- Activity list view with filter/search/status badges
- Gantt chart with bars, milestones, relationship arrows
- Zoom controls (Day/Week/Month)
- Color coding by status
- Left-right split: list panel + timeline panel

### 9. UI — Activity Editor and Relationships

- Activity editor form with all fields
- Relationship table editor with type/lag dropdowns
- Circular reference warning dialog
- Status transition confirmation

### 10. UI — Calendar Manager

- Calendar list with type badges
- Monthly day grid with working/non-working colors
- Exception picker (holiday calendar)
- Hours/day configuration
- Calendar assignment picker for activities

### 11. UI — Activity Bank Browser

- Category tree (left panel)
- Entry list with search (right panel)
- Entry preview with tree breakdown and logic display
- Apply to WBS item dialog
- Create custom entry dialog

### 12. UI — WBS-to-Activity Generation Wizard

- WBS item tree selector
- Generation mode selection
- Bank entry picker (for bank mode)
- Preview grid
- Commit with progress

### 13. UI — Reports

- Schedule summary table view
- Excel export (reuse Phase 2 WorkbookWriter)
- PDF export (QuestPDF)

### 14. Localization and RTL Coverage

- English and Arabic resources for all Activity screens
- Terms: activity, predecessor, successor, lag, calendar, milestone, duration,
  logic, bank, category
- RTL verification for Gantt charts, tables, tree views

### 15. Validation

- Activity CRUD smoke tests
- Relationship CRUD tests with circular reference detection
- Calendar day grid tests (exception handling, date math)
- Activity Bank application tests (entry → activity generation)
- WBS-to-activity generation tests (1:1 and bank mode)
- Status transition tests
- Schedule export tests

## Technical Decisions

### Gantt Chart Implementation

Build a custom WPF user control using Canvas with DrawingVisual for
performance. The left panel shows the activity list (reuse VirtualizingTreeView
from Phase 3/4). The right panel renders bars as rectangles, milestones as
diamonds, and relationships as arrow lines. Zoom is handled by transforming the
time-to-pixel scale factor.

Performance target: 1,000+ activities rendered in under 2 seconds.

### Activity Code Generation

Auto-generated sequential codes (e.g. "A-001", "A-002") based on creation
order within a project. When generated from WBS, inherit the WBS item code as
a prefix (e.g. "WBS01.01-A001").

### Calendar Date Math

Duration calculation uses calendar-aware working days (excludes non-working
days from the assigned calendar). When no calendar is assigned, defaults to a
7-day continuous calendar.

### Relationship Validation

Circular relationship detection uses a depth-first traversal from each
activity. If traversing successors leads back to the starting activity, the
relationship is rejected. Performance target: 10,000 activity network checked
in under 1 second.

### Activity Bank Application Flow

1. User selects target WBS item in WBS tree
2. Opens Activity Bank tab, browses to entry
3. Clicks "Apply"
4. System creates an Activity for each ActivityBankItem under the selected
   WBS item
5. System creates ActivityRelationship for each
   ActivityBankItemRelationship (or infers FS between sequential siblings)
6. New activities inherit the project ID and calendar from the WBS item's
   project
7. User sees the generated activities in the List and Gantt tabs

### Template vs Bank Distinction

WBS Templates (Phase 4) = project-level work breakdown structure patterns.
Activity Bank (Phase 5) = work-package-level construction method sequences.
They are complementary: a WBS template provides the skeleton, the Activity Bank
fills in the detailed construction methods for each work package.

### Calendar Model

Two-tier: Global calendars (shared by all projects — "Standard 5-Day Work
Week", "6-Day Work Week", "7-Day Continuous") and Project calendars (project-
specific exceptions, holidays, custom shifts). Resource calendars are reserved
for Phase 6.

## Detailed Work Breakdown

### Workstream A: Domain and Schema

1. Define Activity, ActivityRelationship, Calendar, CalendarDay,
   ActivityBank, ActivityBankItem, ActivityBankItemRelationship entities
2. Define enums (ActivityType, ActivityStatus with transitions,
   RelationshipType, CalendarType)
3. Add entity configurations and relationships
4. Create migration and apply
5. Add indexes for common query paths

### Workstream B: Activity Bank Seeding

1. Implement lazy-seeding pattern in ActivityBankRepository
2. Implement factory methods for all 13 categories (50+ entries)
3. Each factory method creates hierarchical ActivityBankItems with
   default ActivityBankItemRelationships
4. Seed verification test

### Workstream C: Application Services

1. Implement IActivityService
2. Implement IActivityRelationshipService
3. Implement ICalendarService
4. Implement IActivityBankService
5. Implement IActivityGenerationService
6. Implement IActivityValidationService
7. Implement IActivityReportService

### Workstream D: Activity Validation Engine

1. Implement validation rules (dates, status transitions, required fields)
2. Implement circular relationship detection
3. Implement relationship type validation
4. Implement calendar consistency checks

### Workstream E: WBS-to-Activity Generation

1. Implement 1:1 generation mode
2. Implement Activity Bank application mode
3. Implement bulk defaults (calendar, duration formula)
4. Implement preview data structure
5. Implement commit with transaction

### Workstream F: UI — Registration and Navigation

1. Register Activity Studio nav target in ShellViewModel
2. Create ActivityStudioView as the main workspace container
3. Create ActivityStudioViewModel with tab initialization
4. Wire nav rail item with icon (CalendarDay24)

### Workstream G: UI — Activity List and Gantt

1. Create activity list view with filter/search/status badges
2. Create custom Gantt chart control (Canvas-based)
3. Implement zoom controls (Day/Week/Month)
4. Implement milestone diamonds and relationship arrows
5. Implement left-right split layout

### Workstream H: UI — Activity Editor and Relationships

1. Create activity editor form with all fields
2. Create relationship table editor
3. Implement circular reference warning
4. Implement status transition dialog

### Workstream I: UI — Calendar Manager

1. Create calendar list view
2. Create monthly day grid with working/non-working colors
3. Create exception/holiday picker
4. Create calendar assignment dialog

### Workstream J: UI — Activity Bank Browser

1. Create category tree (left panel)
2. Create entry list with search (right panel)
3. Create entry preview tree with logic display
4. Create "Apply to WBS Item" dialog
5. Create "Save as Custom Entry" dialog

### Workstream K: UI — WBS-to-Activity Generation Wizard

1. Create WBS item tree selector step
2. Create generation mode selection step
3. Create bank entry picker step
4. Create preview grid step
5. Wire commit with progress

### Workstream L: UI — Reports

1. Create schedule summary view
2. Implement Excel export (reuse Phase 2 WorkbookWriter)
3. Implement PDF export (QuestPDF)

### Workstream M: Localization and Polish

1. Add English resource strings for all Activity screens
2. Add Arabic resource strings for all Activity screens
3. Verify Arabic layout and RTL behavior
4. Ensure theme consistency with existing shell
5. Remove any hardcoded user-facing strings

### Workstream N: Testing

1. Unit test domain entities and value objects
2. Unit test validation engine (including circular reference detection)
3. Unit test application services
4. Unit test Activity Bank seed data
5. Unit test WBS-to-activity generation (1:1 and bank mode)
6. Integration test persistence round-trip
7. Integration test calendar day calculations
8. UI smoke test all Activity workspaces

## Risks and Mitigations

### Risk: Gantt chart performance degrades with large schedules

Mitigation:
- Virtualized rendering — only render visible time range
- Batch relationship arrow rendering
- Lazy load activity list for left panel
- Throttle zoom/scroll repaints

### Risk: Circular relationship detection is slow on large networks

Mitigation:
- Depth-first search with visited set
- Cache predecessor/successor closure tables for fast lookups
- Limit traversal depth to 10,000
- Validate at relationship creation time only (not on every read)

### Risk: Calendar day calculations are complex and error-prone

Mitigation:
- Comprehensive unit tests for date math (working day counting, lag
  calculation)
- Test with multiple calendar configurations (5-day, 6-day, 7-day,
  with exceptions)
- Visual calendar grid preview in UI to verify correctness

### Risk: Activity Bank seed data is too generic for real projects

Mitigation:
- Structure entries as templates, not prescriptive — user adjusts
  durations and sequences after applying
- User can create custom entries from their own activity patterns
- Categories cover the most common construction methods

### Risk: Phase 4 WBS module needs minor modification for Activity integration

Mitigation:
- Access WbsItems through existing Phase 4 repository interfaces
- Keep Activity generation logic in Planova.Activity, not in Planova.Wbs
- Extend Phase 4 contracts with backward-compatible additions if needed

### Risk: WBS-to-Activity 1:1 generation produces too many activities

Mitigation:
- Allow user to select specific WBS items (not forced to generate all)
- Bank mode groups sub-tasks under a single parent WBS item
- Preview before commit allows user to remove unwanted activities

## Acceptance Criteria

Phase 5 is complete when all of the following are true:

- Activities can be created, edited, deleted, and persisted for a project.
- Activities can be linked to WBS items for traceability.
- Activities are displayed in a filterable list view and a Gantt chart.
- The Gantt chart shows bars positioned by dates, milestones as diamonds, and
  relationship arrows.
- Predecessor-successor relationships (FS, SS, FF, SF) can be created between
  activities with lag.
- Circular relationships are detected and rejected with a clear message.
- Calendars can be created with working days, exceptions, and configurable hours.
- Calendar days can be bulk-set as working or non-working for date ranges.
- Activities can be assigned to a calendar.
- The Activity Bank contains 50+ seeded entries across 13 categories.
- Activity Bank entries display their full sub-task hierarchy with durations and
  logic in a preview.
- A Bank entry can be applied to a WBS item, generating sub-task activities with
  default relationships.
- WBS-to-activity generation works in 1:1 mode and Bank application mode.
- Schedule reports can be generated and exported to Excel and PDF.
- English and Arabic screens work correctly.
- RTL behavior remains correct.
- The existing shell, theme, and infrastructure from Phases 0–4 remain intact.
- The implementation remains Clean Architecture and MVVM compliant.

## Definition of Done

- Core Activity domain entities are implemented.
- Activity CRUD, relationships, calendars, and Activity Bank services are
  functional.
- Activity Bank is seeded with 50+ entries with full item breakdowns.
- Gantt chart and activity list are integrated into the shell with navigation
  registration.
- Calendar manager is functional with working day configuration and exceptions.
- WBS-to-activity generation wizard is functional (1:1 and Bank modes).
- Relationship editor is functional with circular reference detection.
- Activity reports are functional with Excel and PDF export.
- Localization is complete for all new Activity scope.
- All tests pass.
- The data model is documented for Phase 6 (Resource Studio) and Phase 7
  (Cost Studio) consumption.

## Next Step After Phase 5

When Activity Studio is stable, the next implementation plan should target
Phase 6 — Resource Studio:

- Labour resources and roles
- Equipment and material resources
- Subcontractor management
- Crew templates
- Resource calendars
- Resource loading onto activities
