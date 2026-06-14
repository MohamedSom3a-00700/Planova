# Feature Specification: Primavera Studio

**Feature Branch**: `011-primavera-studio`

**Created**: 2026-06-12

**Status**: Draft

**Input**: User description: "Phase 9 Primavera Studio implementation plan covering XER import/export, workspace editing for activities/relationships/resources/calendars/codes/baselines/UDFs, validation and repair workflows, cross-studio data sharing via direct service injection, and project-gated studio navigation"

## Clarifications

### Session 2026-06-12

- Q: Entity Matching on Re-import (Merge Behavior) → A: Match by XER internal IDs (task_id, calendar_id, etc.). Overwrite matched rows, insert unmatched rows, flag rows in DB but not in new file for user review.
- Q: Import Crash Recovery → A: Atomic rollback — wrap each import commit in a single transaction; if anything fails mid-way, roll back entirely. No partial data persists.
- Q: Audit Logging Scope → A: Log all mutation operations — import attempts (success/failure with file name and row counts), exports (with file path and row counts), repair actions, and workspace edits (batch summary: entity type, count modified, timestamp). Validation runs are NOT logged.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Import XER File with Preview (Priority: P1)

A project planner has a Primavera P6 XER file exported from their client's system. They navigate to the Primavera Studio, click Import, select the XER file, and see a preview of the data organized by entity type (projects, activities, relationships, resources, calendars, codes, baselines, UDFs). The preview shows row counts and highlights validation issues (missing calendars, broken references). The user reviews the issues, decides whether to overwrite or merge with existing data, and commits the import. After commit, all schedule data is available in the workspace.

**Why this priority**: XER import is the foundational capability — without importing data, none of the other features can be used.

**Independent Test**: Can be fully tested by importing a known XER file, verifying the preview shows the correct row counts per entity type, confirming validation issues are reported, and confirming committed data appears in the workspace tabs.

**Acceptance Scenarios**:

1. **Given** a Primavera Studio with no XER data, **When** the user imports a valid XER file, **Then** the system shows a preview grid with row counts per entity type
2. **Given** an XER file with validation issues (e.g., missing calendars), **When** the preview is displayed, **Then** issues are listed with severity and affected entity references
3. **Given** an XER file has been imported before, **When** the user imports a new version, **Then** the user is prompted to overwrite or merge
4. **Given** an import is committed, **When** the user opens the workspace, **Then** all imported entities are available for browsing and editing

---

### User Story 2 - Browse and Edit Workspace Data (Priority: P1)

After importing XER data, the planner navigates through tabbed workspace areas: Activities, Relationships, Resources, Calendars, Codes, Baselines, and UDFs. Each tab shows a virtualized grid of its entities. The user can sort, filter, and edit cell values inline. Changes are saved individually or in batch. Baseline data is viewable in a read-only mode with clear visual distinction from the active schedule.

**Why this priority**: The workspace is the primary interaction surface for viewing and editing schedule data after import.

**Independent Test**: Can be tested by importing a moderate XER file (10K activities), opening each workspace tab, and verifying grid loading, filtering, cell editing, and baseline browsing work without performance degradation.

**Acceptance Scenarios**:

1. **Given** imported XER data, **When** the user opens the Activities tab, **Then** activities are displayed in a virtualized grid with sort, filter, and inline editing
2. **Given** the workspace has multiple baselines, **When** the user opens the Baselines tab, **Then** each baseline's data is viewable in read-only mode, visually distinct from the active schedule
3. **Given** the user edits an activity field, **When** the edit is committed, **Then** the change is persisted and visible in the grid immediately
4. **Given** a moderate schedule (10K activities, 30K relationships), **When** scrolling through any grid, **Then** the UI remains responsive (scroll, filter, cell edit under 100ms)

---

### User Story 3 - Validate Schedule Integrity (Priority: P1)

The planner runs validation on the current workspace to check for schedule integrity issues. The system detects broken references, missing calendars, invalid relationship links, duplicate codes, orphaned records, zero-duration activities, missing predecessors/successors, and circular logic. Results are grouped by severity and entity type. The user reviews each issue and can navigate directly to the offending entity.

**Why this priority**: Validation ensures data quality before using the schedule for downstream analysis or before exporting.

**Independent Test**: Can be tested by importing an XER file with known issues, running validation, and verifying all expected issue types are detected and reported with correct severity.

**Acceptance Scenarios**:

1. **Given** workspace data with integrity issues, **When** the user runs validation, **Then** all issues are detected and listed with severity (Error, Warning, Info) and entity references
2. **Given** validation results are displayed, **When** the user clicks an issue, **Then** they are navigated to the offending entity in the relevant workspace tab
3. **Given** validation completes, **When** the user views the summary, **Then** issue counts are shown grouped by entity type and severity

---

### User Story 4 - Repair Schedule Issues (Priority: P2)

After validation, the user opens the Repair tab and sees suggested fixes for detected issues. Each suggested fix shows the problem, the proposed resolution, and affected entities. The user approves individual fixes or selects multiple fixes for batch application. Applied repairs are recorded with timestamp and user identity for auditability.

**Why this priority**: Repair capabilities turn validation findings into actionable improvements, but validation must work first.

**Independent Test**: Can be tested by running validation on a schedule with known fixable issues, reviewing suggestions, applying fixes, and re-running validation to confirm issues are resolved.

**Acceptance Scenarios**:

1. **Given** validation issues exist in the workspace, **When** the user opens the Repair tab, **Then** suggested fixes are listed with proposed resolution and affected entities
2. **Given** a suggested fix, **When** the user approves and applies it, **Then** the fix is applied and logged with timestamp and user identity
3. **Given** multiple issues share a common fix pattern, **When** the user selects and applies them in batch, **Then** all selected fixes are applied and logged

---

### User Story 5 - Export Workspace to XER (Priority: P2)

The planner needs to send the current schedule state back to the client. They open the Export dialog, confirm the export scope (all entities or selected), and generate an XER file. The system produces a valid XER that includes all supported entity types plus any raw tables that were preserved from the original import (round-trip fidelity). The user is shown the export location and can open the file.

**Why this priority**: Export completes the import-edit-export workflow, but editing and validation are higher priority for initial use.

**Independent Test**: Can be tested by importing an XER file, making edits, exporting to a new XER, and verifying the exported file contains all entities and any preserved raw tables.

**Acceptance Scenarios**:

1. **Given** workspace data, **When** the user exports to XER, **Then** a valid XER file is generated containing all supported entity types and preserved raw tables
2. **Given** raw tables were preserved from import, **When** the user exports, **Then** those tables are re-emitted verbatim in the exported XER
3. **Given** an export completes, **When** the user views the export confirmation, **Then** the file path and row counts per entity type are displayed

---

### User Story 6 - Cross-Studio Data Consumption (Priority: P2)

A downstream studio (e.g., Schedule Comparison, Delay Analysis) needs to read Primavera schedule data when available. The studio injects the Primavera workspace service (as nullable) and checks availability. When Primavera data exists, the consumer uses it; when absent, it falls back to native data. The consumer never accesses Primavera database tables directly.

**Why this priority**: Cross-studio sharing makes Primavera data valuable across the platform, but depends on import and workspace features being operational first.

**Independent Test**: Can be tested by creating a consumer studio that injects the Primavera service, verifying data is returned when Primavera data exists, and verifying the consumer works with native data when Primavera is absent.

**Acceptance Scenarios**:

1. **Given** a consumer studio with nullable injection of the Primavera workspace service, **When** Primavera data exists for the current project, **Then** the consumer receives schedule data through the service interface
2. **Given** the same consumer studio, **When** Primavera data does not exist, **Then** the consumer falls back to native data without errors
3. **Given** any consumer studio, **When** reading Primavera data, **Then** it accesses data through domain service interfaces only — no direct database access

---

### User Story 7 - Project-Gated Navigation (Priority: P3)

The Primavera Studio appears as a navigation item in the shell only when a project is active. When no project is open, the item is disabled or hidden — consistent with other project-gated studios. Opening the studio navigates to the dedicated workspace with its own tabbed surface and command area.

**Why this priority**: Navigation gating is important for UX consistency but is a shell integration detail rather than core functionality.

**Independent Test**: Can be tested by opening the application with no project (item disabled), creating/opening a project (item enabled), and opening the studio to verify the workspace loads.

**Acceptance Scenarios**:

1. **Given** no project is active, **When** the user views the navigation rail, **Then** the Primavera nav item is disabled
2. **Given** a project is active, **When** the user views the navigation rail, **Then** the Primavera nav item is enabled and clickable
3. **Given** the user clicks the Primavera nav item with an active project, **When** the studio opens, **Then** the dedicated workspace is displayed with its tabbed surface

### Edge Cases

- What happens when a user imports a corrupted XER file? The system detects the file early and shows a descriptive error message; no partial data is committed.
- How does the system handle XER files from different Primavera versions? The parser tolerates optional fields and version-specific sections; unknown constructs are preserved as raw table data.
- What happens to workspace data when a project is deleted? All Primavera data (import sessions, workspace entities, baselines, validation/repair records) are cascade-deleted with the project.
- How does the system behave when two imports happen simultaneously? Imports are sequential — the second import shows a prompt asking whether to replace or merge with the existing data.
- What happens when a user exports without having made any changes? The export produces an XER equivalent to the last imported state (including preserved raw tables).
- How are unsupported XER tables handled during editing? They are read-only — users cannot edit raw table data through the workspace. They are re-emitted verbatim on export.
- What happens to baseline data when the active schedule changes? Baselines are snapshots and remain unchanged; only the active schedule is mutable.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a Primavera Studio workspace accessible from the navigation rail when a project is active
- **FR-002**: System MUST support importing Primavera P6 XER files by parsing supported table types (projects, activities, relationships, resource assignments, calendars, codes, baselines, UDFs)
- **FR-003**: Import MUST show a preview grid before commit, displaying row counts per entity type and validation warnings
- **FR-004**: Import MUST preserve unsupported XER table types as raw data for round-trip fidelity on export
- **FR-005**: Import MUST detect malformed or incomplete XER files early and prevent partial commits. Each import MUST be wrapped in a single atomic transaction — any failure mid-way MUST roll back entirely with no partial data persisted.
- **FR-006**: Import MUST support overwrite and merge behaviors when data already exists, with user choice at commit time. Merge MUST match entities by XER internal IDs (task_id, calendar_id, etc.): overwrite matched rows, insert unmatched rows, flag rows present in DB but absent from new file for user review
- **FR-007**: Import sessions MUST be persisted with metadata (source filename, import timestamp, user identity, commit status) for audit trail
- **FR-008**: System MUST provide a tabbed workspace with areas for Activities, Relationships, Resource Assignments, Calendars, Codes, Baselines, and UDFs
- **FR-009**: All workspace grids MUST support virtualized scrolling, sorting, filtering, and inline cell editing
- **FR-010**: Baseline data MUST be viewable in read-only mode with visual distinction from the active schedule
- **FR-011**: System MUST provide on-demand schedule validation that detects: broken references, missing calendars, invalid relationships, duplicate codes, orphaned records, zero-duration activities, missing predecessors/successors, and circular logic
- **FR-012**: Validation results MUST be grouped by severity (Error, Warning, Info) and entity type, with navigation to the offending entity
- **FR-013**: System MUST provide repair suggestions for detected issues, with user approval required before applying any fix
- **FR-014**: Applied repair actions MUST be logged with timestamp, user identity, problem description, and resolution applied
- **FR-015**: System MUST support exporting the current workspace state to a valid XER file, including all supported entity types and preserved raw tables
- **FR-016**: Export MUST provide confirmation with file path and row counts per entity type
- **FR-017**: System MUST publish public domain service interfaces (`IPrimaveraWorkspaceService`, `IPrimaveraImportService`, `IPrimaveraExportService`, `IPrimaveraValidationService`, `IPrimaveraRepairService`) for cross-studio consumption
- **FR-018**: Other studios MUST consume Primavera data through nullable injection of these service interfaces (no direct database access)
- **FR-019**: Other studios MUST degrade gracefully when Primavera data is unavailable — no crashes or missing references
- **FR-020**: Data provenance MUST be preserved on all records — each row tracks whether it came from import, manual edit, repair, or export
- **FR-021**: All long-running operations (import, export, validation) MUST be cancellable by the user and MUST not block the application UI
- **FR-022**: All workspace screens MUST support English and Arabic (RTL) localization without layout issues
- **FR-023**: System MUST log all mutation operations: import attempts (success/failure with file name and row counts), exports (file path and row counts), repair actions, and workspace edits (batch summary of entity type, count modified, timestamp). Validation runs MUST NOT be logged

### Key Entities

- **PrimaveraProject**: Metadata for an imported Primavera project — source file name, import timestamp, project name from XER, and active/inactive status
- **XerImportSession**: A staged import transaction with status (Previewing, Committed, Failed), source file metadata, validation summary, and row counts per entity type
- **XerExportProfile**: Export preferences and round-trip settings — entity selection, raw table preservation flag, output path template
- **XerRawTable**: Raw staging data for unsupported XER table types — table name, column headers, and serialized row data, preserved for round-trip export
- **PrimaveraActivity**: A schedule activity with fields from XER TASK table — ID, WBS, dates, durations, status, and custom UDF values
- **PrimaveraRelationship**: A logical link between two activities (FS, SS, FF, SF) with lag duration
- **PrimaveraResourceAssignment**: Resource allocation to an activity — resource ID, units, rates, and cost accounts
- **PrimaveraCalendar**: A work/non-work calendar defining working days, shifts, and exceptions for activities and resources
- **PrimaveraCode**: A code type with values used for activity categorization, filtering, and rollup (WBS, activity codes, project codes)
- **PrimaveraBaseline**: A frozen schedule snapshot with duplicated activities, relationships, resource assignments, and calendar overrides keyed by baseline ID and version
- **PrimaveraUdf**: User-defined field definitions and values attached to activities or other entities
- **PrimaveraValidationRule**: A registered validation rule with name, description, severity, and enabled/disabled status
- **PrimaveraValidationIssue**: An issue detected during validation — severity, entity type and ID, rule violated, description, and suggested fix
- **PrimaveraRepairAction**: A repair operation applied to fix a validation issue — problem, resolution, target entity, user who applied it, and timestamp

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can import a moderate XER file (10K activities, 30K relationships, 1K resources) and see the preview in under 10 seconds
- **SC-002**: On-demand validation completes in under 5 seconds for a moderate schedule
- **SC-003**: Export of a moderate schedule to XER completes in under 10 seconds
- **SC-004**: The Primavera Studio workspace opens in under 5 seconds with moderate data loaded
- **SC-005**: Grid scrolling, filtering, and cell editing respond in under 100ms for moderate schedule data
- **SC-006**: Cross-studio data resolution via nullable service injection returns results in under 2 seconds
- **SC-007**: Users can complete the import-validate-repair-export workflow without application crashes or data loss
- **SC-008**: All validation rule types detect and report issues correctly against a schedule with known integrity problems
- **SC-009**: Round-trip import-then-export preserves all supported entities and raw unsupported tables without data loss
- **SC-010**: The Primavera Studio nav item is correctly gated — disabled without a project, enabled with one
- **SC-011**: All workspace features function correctly in both English and Arabic (RTL) without layout issues
- **SC-012**: Existing native studio data entry in Activity Studio and other studios remains fully operational after Primavera Studio is deployed — no regressions

## Assumptions

- The user has access to valid Primavera P6 XER files for import; the system does not connect to Primavera databases directly
- Primavera Studio and Activity Studio are independent choices — users choose which to use; no automatic bridging or UI changes in Activity Studio
- Baseline storage uses a full-copy model (duplicate entries per baseline) for query simplicity at the cost of storage — acceptable for desktop application
- Unsupported XER table types are preserved as-is without editing capability; they pass through the workspace transparently
- Cross-studio consumers implement their own fallback logic (use native data, show empty state) when Primavera data is unavailable
- Single-user desktop application — no multi-user collaboration or concurrent editing on the same workspace
- Imported data retains source provenance (source file name, import timestamp) so users can distinguish imported from edited data
- Performance targets assume typical desktop hardware; the moderate benchmark (10K activities, 30K relationships, 1K resources, 10 calendars, 5 baselines) defines the performance baseline
- Expected data volumes: up to 10K activities, 30K relationships, 1K resources, 10 calendars, and 5 baselines per project
