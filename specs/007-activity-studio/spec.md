# Feature Specification: Activity Studio

**Feature Branch**: `007-activity-studio`

**Created**: 2026-06-04

**Status**: Draft

**Input**: Phase 5 implementation plan for transforming WBS work packages into a detailed project schedule with activities, logic relationships, milestones, calendars, and a reusable Activity Bank of pre-defined construction method sequences.

## Clarifications

### Session 2026-06-04

- Q: What does the "Weight" field on an activity represent and how is it used? → A: Weight represents the activity's percentage contribution to the total project value/effort, used for progress measurement and downstream Phase 7 cost distribution.
- Q: What happens when applying a bank entry to a WBS item that already has generated activities? → A: Warn the user and offer two options — replace existing activities with the new breakdown, or merge (append alongside existing). Additionally, the system should detect activities under that WBS item that are NOT part of any bank entry and ask if the user wants to save them as a new custom bank entry for reuse in other projects.
- Q: How should the "WbsSummary" activity type behave? → A: WbsSummary is an auto-rollup activity representing a parent WBS item, deriving its dates, duration, and percent complete automatically from its child activities. It is not manually editable for those rollup fields.

## User Scenarios & Testing

### User Story 1 - Create and Manage Activities (Priority: P1)

A project scheduler needs to create, edit, and delete schedule activities for a construction project. Each activity has a name, code, duration, planned dates, status, type (task, milestone, etc.), and can be linked to a WBS work package for traceability.

**Why this priority**: Activity creation is the foundational capability — without individual activities, no scheduling, relationships, or reporting can happen.

**Independent Test**: A user can create a new activity with a name and duration, see it appear in the activity list, edit its fields, and delete it. All changes persist after reloading the project.

**Acceptance Scenarios**:

1. **Given** a project with a WBS structure, **When** a user creates a new activity and assigns it to a WBS item, **Then** the activity appears in the list with an auto-generated code, the assigned WBS path, and a default status of NotStarted.
2. **Given** an existing activity, **When** a user edits its name, duration, dates, or status, **Then** the changes are saved and immediately reflected in the list and Gantt views.
3. **Given** an activity with no dependent relationships, **When** a user deletes it, **Then** it is removed from all views and the action is confirmed.
4. **Given** an activity linked as a predecessor to other activities, **When** a user attempts to delete it, **Then** the system warns about existing relationships and requires confirmation.

---

### User Story 2 - View Project Schedule on Gantt Chart (Priority: P1)

A project scheduler needs to see all activities laid out on a timeline with bars positioned by start and finish dates, milestones displayed as distinct markers, and predecessor-successor arrows showing logical dependencies between activities.

**Why this priority**: The Gantt chart is the primary visualization tool for understanding and communicating the schedule. Without it, users cannot assess sequencing, overlaps, or critical paths at a glance.

**Independent Test**: A user opens a project with activities and sees them rendered as horizontal bars on a timeline with correct positions corresponding to their dates. Zoom controls allow switching between day, week, and month views.

**Acceptance Scenarios**:

1. **Given** a project with multiple activities having planned start and finish dates, **When** a user opens the Gantt view, **Then** each activity appears as a horizontal bar spanning its duration at the correct position on the time axis.
2. **Given** an activity marked as a milestone (zero duration), **When** viewed on the Gantt chart, **Then** it appears as a diamond-shaped marker at its planned date.
3. **Given** activities connected by predecessor-successor relationships, **When** viewed on the Gantt chart, **Then** arrows connect the related bars showing the logic flow.
4. **Given** a Gantt chart with many activities, **When** a user uses the zoom controls, **Then** the timeline switches between day, week, and month granularity and bars adjust accordingly.

---

### User Story 3 - Define Activity Relationships and Validate Logic (Priority: P2)

A project scheduler needs to define logical dependencies between activities — finish-to-start, start-to-start, finish-to-finish, and start-to-finish — each with optional lag days. The system must detect and reject circular relationships that would create infinite scheduling loops.

**Why this priority**: Relationship logic is essential for accurate schedule calculation. Without it, the schedule is just a list of tasks with no sense of order. Circular reference detection prevents data corruption that would break downstream resource and cost phases.

**Independent Test**: A user creates two activities, links them with a finish-to-start relationship, and verifies the relationship appears in the relationship editor and as an arrow on the Gantt chart. Attempting to create a circular loop is rejected with a clear message.

**Acceptance Scenarios**:

1. **Given** two existing activities, **When** a user creates a finish-to-start relationship with 2 days lag, **Then** the relationship is saved and displayed in the relationship editor and as an arrow on the Gantt chart.
2. **Given** a chain of activities A → B → C, **When** a user attempts to add a relationship C → A, **Then** the system detects the circular reference, rejects the relationship, and displays a clear warning message identifying the loop.
3. **Given** an existing relationship, **When** a user changes its type from finish-to-start to start-to-start, **Then** the relationship is updated and the Gantt chart arrow reflects the new logic.
4. **Given** a relationship with a successor activity, **When** a user deletes the relationship, **Then** the link is removed and the Gantt chart arrow disappears.

---

### User Story 4 - Manage Working Time Calendars (Priority: P2)

A project scheduler needs to define working time calendars that determine which days are available for work. Global calendars (e.g., Standard 5-Day Work Week) serve as defaults across all projects. Project-specific calendars add exceptions like public holidays and custom shutdowns. Each activity can be assigned a calendar.

**Why this priority**: Calendars control duration calculations and date logic. Without proper calendar support, schedule dates would be inaccurate and not reflect real-world working conditions.

**Independent Test**: A user creates a new calendar, sets its working days (Sunday-Thursday), adds a public holiday exception, and assigns it to an activity. The activity's date calculations respect the non-working days.

**Acceptance Scenarios**:

1. **Given** the calendar manager, **When** a user creates a new calendar with a name, working days configuration, and hours per day, **Then** the calendar appears in the calendar list with a type badge (Global or Project).
2. **Given** a calendar with working days configured, **When** a user adds a specific date as a non-working exception (holiday), **Then** that date is marked as non-working in the calendar day grid.
3. **Given** multiple calendars, **When** a user assigns a calendar to an activity, **Then** the activity's duration and date calculations use that calendar's working days.
4. **Given** the calendar day grid, **When** a user bulk-selects a date range and sets it as non-working, **Then** all dates in that range are updated accordingly.

---

### User Story 5 - Generate Activities from WBS Work Packages (Priority: P3)

A project scheduler needs to generate schedule activities directly from the project's WBS work packages. In simple mode, each selected WBS item becomes one activity. In Activity Bank mode, a selected bank entry's full sub-task breakdown is applied to each WBS item, generating multiple activities with default durations and relationships.

**Why this priority**: Manual activity creation for large projects is time-consuming and error-prone. WBS-to-activity generation dramatically reduces setup time and ensures traceability between the WBS and the schedule.

**Independent Test**: A user selects WBS items from the WBS tree, chooses simple 1:1 generation, reviews the preview of generated activities, and commits them to the database.

**Acceptance Scenarios**:

1. **Given** a project with a populated WBS, **When** a user opens the generation wizard, selects WBS items, and chooses 1:1 mode, **Then** the preview shows one activity per selected WBS item with auto-generated codes and names.
2. **Given** a WBS item and a selected Activity Bank entry, **When** a user runs generation in Bank mode, **Then** the preview shows multiple sub-task activities per WBS item with default durations and FS relationships matching the bank entry structure.
3. **Given** a preview of generated activities, **When** a user reviews the list before committing, **Then** they can remove individual activities and adjust default settings.
4. **Given** confirmed generation, **When** the user commits, **Then** all generated activities and relationships are persisted and visible in the activity list and Gantt chart.

---

### User Story 6 - Browse and Apply Activity Bank Templates (Priority: P3)

A project scheduler needs to browse a library of pre-defined construction method sequences organized by category. Each entry shows its full sub-task breakdown with default durations and logic. The user can apply an entry to a WBS item, which generates the complete sub-task tree in a single operation. Users can also create custom entries from their own activity groups.

**Why this priority**: The Activity Bank saves significant time by reusing standard construction methods (e.g., Column Concrete, Slab Concrete) across projects. It encodes domain expertise that would otherwise need to be recreated for each project.

**Independent Test**: A user opens the Activity Bank, browses categories, previews an entry's sub-task breakdown, applies it to a WBS item, and sees the generated activities appear.

**Acceptance Scenarios**:

1. **Given** the Activity Bank browser, **When** a user expands a construction category (e.g., Concrete Works), **Then** they see a list of entries with names, codes, and descriptions.
2. **Given** an Activity Bank entry, **When** a user clicks to preview it, **Then** they see its full hierarchical sub-task breakdown with default durations and relationship arrows between sub-tasks.
3. **Given** a WBS item selected in the project tree, **When** a user applies a bank entry to it, **Then** the system generates sub-task activities for each bank item with the default durations and FS relationships from the entry. If activities already exist under that WBS item, the system warns the user and offers replace or merge options.
4. **Given** a set of existing activities organized as a work sequence, **When** a user saves them as a new bank entry, **Then** the entry appears in the specified category with all sub-task details preserved.
5. **Given** the Activity Bank has never been accessed, **When** a user opens it for the first time, **Then** the system populates it with 50+ pre-seeded entries across 13 construction categories.

---

### User Story 7 - Generate and Export Schedule Reports (Priority: P3)

A project scheduler needs to generate a schedule summary report showing activity codes, names, dates, durations, status, percent complete, and predecessor-successor relationships. The report must be exportable to Excel for further analysis and to PDF for distribution to stakeholders.

**Why this priority**: Schedule reports are essential for project communication, progress meetings, and documentation. Exports enable downstream workflows in spreadsheets and formal distribution to non-system users.

**Independent Test**: A user opens the reports view, generates a schedule summary, and exports it to both Excel and PDF formats with all activity data included.

**Acceptance Scenarios**:

1. **Given** a project with activities and relationships, **When** a user opens the schedule report view, **Then** they see a sortable table with columns for code, name, dates, duration, status, percent complete, predecessors, and successors.
2. **Given** the schedule summary table, **When** a user clicks Export to Excel, **Then** the full data is written to an Excel file with proper formatting.
3. **Given** the schedule summary table, **When** a user clicks Export to PDF, **Then** a formatted PDF document is generated with the schedule data suitable for distribution.

### Edge Cases

- What happens when an activity's planned start date is after its planned finish date? The system should reject with a validation error.
- What happens when an activity's calendar is changed after dates have been set? The system should recalculate dates based on the new calendar's working days, or warn the user if dates would become invalid.
- What happens when a WBS item is deleted after activities have been linked to it? Activities should remain but lose their WBS traceability link (orphaned activities).
- What happens when a project has thousands of activities? The Gantt chart should remain usable with virtualized rendering — only the visible time range and visible activities need to be rendered.
- What happens when a user applies a bank entry with 10 sub-tasks to 20 WBS items? The system should generate 200 activities in a single operation with progress feedback.
- What happens when a calendar has all days set as non-working? Duration calculations should handle this gracefully, perhaps by defaulting to a 7-day continuous calendar or warning the user.
- What happens when the relationship graph is very large (10,000+ activities)? The circular reference check should complete within a reasonable time and not block the user interface.

## Requirements

### Functional Requirements

- **FR-001**: Users MUST be able to create, edit, view, and delete schedule activities with fields including name, code, description, duration, planned start/finish dates, actual start/finish dates, status, activity type, percent complete, weight (percentage contribution to total project value/effort, used for progress measurement and cost distribution), and notes.
- **FR-002**: Activities MUST support a status state machine with transitions: NotStarted → InProgress → Completed, with OnHold and Revise as reversible states from InProgress.
- **FR-003**: Activities MUST be linkable to a WBS work package for traceability, and to a calendar for working day calculations.
- **FR-004**: Activities MUST support auto-generated sequential codes per project (e.g., A-001, A-002), with optional WBS code prefix inheritance when generated from WBS items.
- **FR-005**: Activities of type WbsSummary MUST auto-rollup their dates, duration, and percent complete from their child activities. Such fields on WbsSummary activities MUST be read-only and updated automatically when child activities change.
- **FR-006**: Users MUST be able to filter and search activities by project, WBS item, status, activity type, name, and code.
- **FR-007**: Users MUST be able to view activities on a Gantt chart showing horizontal bars positioned by planned start and finish dates, milestones as diamond markers, and relationship arrows between related activities.
- **FR-008**: The Gantt chart MUST support zoom controls to switch between day, week, and month time granularity.
- **FR-009**: Users MUST be able to create, edit, and delete predecessor-successor relationships between activities with types: Finish-to-Start (FS), Start-to-Start (SS), Finish-to-Finish (FF), Start-to-Finish (SF), each with configurable lag days.
- **FR-010**: The system MUST detect and reject circular relationships that would create infinite scheduling loops, displaying a clear message identifying the loop.
- **FR-011**: The system MUST prevent self-referencing relationships (an activity cannot be its own predecessor or successor).
- **FR-012**: Users MUST be able to create, edit, clone, and delete calendars with properties including name, type (Global or Project), hours per day, and days per week.
- **FR-013**: Users MUST be able to configure individual calendar days as working or non-working, add exception dates (holidays, shutdowns), and bulk-set date ranges.
- **FR-014**: Users MUST be able to assign a calendar to individual activities or set a project default calendar.
- **FR-015**: Activity duration and date calculations MUST respect the assigned calendar's working days and exceptions.
- **FR-016**: The system MUST ship with a pre-seeded Activity Bank containing at least 50 entries across 13 construction categories (Preliminary, Earthworks, Concrete, Formwork, Reinforcement, Steel, Masonry, Waterproofing, MEP, Finishing, Infrastructure, Landscaping, Testing & Handover).
- **FR-017**: Users MUST be able to browse the Activity Bank by category tree, search entries by name or keyword, and preview an entry's full hierarchical sub-task breakdown with default durations and relationships.
- **FR-018**: Users MUST be able to apply an Activity Bank entry to a WBS item, generating the complete sub-task tree with default durations and FS relationships in a single operation. If the WBS item already has activities, the system MUST warn the user and offer a choice: replace existing activities with the new breakdown, or merge (append alongside existing). Additionally, the system MUST detect activities under that WBS item not associated with any bank entry and prompt to save them as a new custom bank entry.
- **FR-019**: Users MUST be able to create custom Activity Bank entries from existing activity groups, edit custom entries, and delete custom entries.
- **FR-020**: Users MUST be able to generate activities from WBS work packages in two modes: 1:1 (one activity per WBS item) and Activity Bank (bank entry applied to each selected WBS item).
- **FR-021**: The WBS-to-activity generation wizard MUST provide a preview step showing generated activities before allowing commit to the database.
- **FR-022**: Users MUST be able to generate a schedule report showing activity code, name, dates, duration, status, percent complete, predecessors, and successors.
- **FR-023**: Users MUST be able to export the schedule report to Excel and PDF formats.
- **FR-024**: All Activity Studio screens MUST support English and Arabic localization, including RTL layout correctness.
- **FR-025**: The Activity Studio MUST integrate into the existing multi-tab workspace alongside other studios, with navigation rail registration.
- **FR-026**: The activity data model MUST be structured for downstream consumption by Resource Studio (Phase 6) and Cost Studio (Phase 7).

### Key Entities

- **Activity**: A single schedule task or milestone within a project. Key attributes: name, code, duration, dates, status, type (Task, Milestone, LevelOfEffort, or WbsSummary), calendar assignment, WBS item link, weight (percentage contribution to total project value/effort for progress measurement and cost distribution), percent complete. The WbsSummary type is an auto-rollup activity representing a parent WBS item — its dates, duration, and percent complete are derived automatically from child activities and are not directly editable.
- **ActivityRelationship**: A logical dependency between two activities defining sequencing (FS, SS, FF, SF) and lag. Links a predecessor activity to a successor activity.
- **Calendar**: Defines working time rules including working days, hours per day, and exceptions. Can be Global (shared across projects) or Project-specific.
- **CalendarDay**: A specific date's working status within a calendar — working, non-working, or exception (holiday).
- **ActivityBank**: A reusable library entry representing a construction method template at the work-package level. Organized by category and subcategory.
- **ActivityBankItem**: A single sub-task within an Activity Bank entry, with default duration, activity type, and hierarchical parent-child relationships forming a task breakdown.
- **ActivityBankItemRelationship**: Default predecessor-successor logic between sub-tasks within a bank entry, defining the expected construction sequence.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can create a new activity and see it in the list and Gantt views within 5 seconds of saving.
- **SC-002**: A project with 1,000 activities renders in the Gantt chart within 2 seconds with correct bar positions and relationship arrows.
- **SC-003**: Circular reference detection on a network of 10,000 activities completes within 1 second and clearly identifies the offending relationship.
- **SC-004**: The Activity Bank ships with 50+ entries across 13 categories on first access, with each entry containing 3-10 sub-task items.
- **SC-005**: Users can apply a bank entry to 20 WBS items and generate 200+ activities in a single operation with progress feedback, completing in under 10 seconds.
- **SC-006**: A schedule report covering 500+ activities can be generated and exported to Excel and PDF in under 15 seconds.
- **SC-007**: All Activity Studio screens display correctly in both English and Arabic, with RTL layout properly applied.
- **SC-008**: Users can complete the full workflow — create activities, define relationships, assign calendars, generate from WBS, and export a report — without encountering errors or requiring developer assistance.
- **SC-009**: The existing Phase 0-4 shell, theme, and infrastructure remain fully functional with no regressions after introducing Activity Studio.

## Assumptions

- Users have access to projects with existing WBS structures (Phase 4 must be complete before this feature is usable).
- The target users are project schedulers and construction managers familiar with scheduling concepts (activities, relationships, calendars, Gantt charts).
- Activity durations are manually entered by users; automatic CPM scheduling calculation is out of scope — the schedule is logic-driven manually.
- The Activity Bank seed data represents common construction methods for the building and infrastructure sectors. Users in specialized domains (industrial, marine, etc.) will create their own custom entries.
- Global calendars (Standard 5-Day, 6-Day, 7-Day) are provided as defaults; project-specific calendars handle exceptions and holidays per project.
- Resource loading and cost loading are separate downstream phases that will consume the structured activity schedule output.
- Performance targets assume typical desktop hardware; very large projects (100,000+ activities) may require additional optimization in a future phase.
- The WBS item selector in the generation wizard uses the existing Phase 4 WBS tree interface without modification.
- Excel and PDF export reuse existing Phase 2 export infrastructure (WorkbookWriter and QuestPDF).
- Localization follows the same pattern established in Phases 0-4 (resource files, RTL-aware layouts).
