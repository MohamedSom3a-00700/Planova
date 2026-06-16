# Feature Specification: Schedule Comparison Studio

**Feature Branch**: `012-schedule-comparison-studio`

**Created**: 2026-06-14

**Status**: Draft

## Clarifications

### Session 2026-06-14

- Q: What is the lifecycle state machine for a ComparisonSession? → A: Draft → Running → Completed / Failed / Cancelled. Once terminal, it cannot be re-run; user creates a new session.
- Q: How are concurrent operations (duplicate comparison runs, snapshot deletion) handled? → A: Run button is disabled while a comparison is in progress. Deleting a snapshot referenced by any session shows a confirmation warning.
- Q: What is the maximum supported schedule size? → A: 50K activities / 150K relationships / 5K resources with graceful degradation. Beyond that, a performance warning is shown.

**Input**: User description: "Schedule Comparison Studio — a dedicated workspace for comparing two schedule snapshots side-by-side, detecting changes across activities, logic, resources, critical path, and float, and exporting results for downstream delay analysis."

## User Scenarios & Testing

### User Story 1 - Compare Two Schedule Versions and View Diffs (Priority: P1)

A project planner needs to compare two versions of a project schedule to understand what changed between a baseline and the current update. They select the source (baseline snapshot) and target (current schedule), choose which dimensions to compare (activities, logic, resources, critical path, float), and run the comparison. The system presents a summary of all changes and allows drill-down into each dimension.

**Why this priority**: This is the core feature — without the ability to compare and view diffs, there is no Schedule Comparison Studio. All other capabilities (export, history, snapshots) depend on this flow.

**Independent Test**: Can be fully tested by selecting any two schedules in the comparison workspace, running a comparison, and verifying that diff results appear in the Activities, Logic, Resources, Critical Path, and Float tabs with correct counts and field-level detail.

**Acceptance Scenarios**:

1. **Given** a project with at least two schedule snapshots, **When** the user selects a source and target in the Compare tab and clicks "Run Comparison", **Then** a summary card displays the count of added, removed, and modified items for each dimension (activities, logic, resources, critical path, float) within 15 seconds for a moderate schedule (10K activities, 30K relationships, 1K resources).

2. **Given** a completed comparison, **When** the user clicks the Activities tab, **Then** a color-coded grid shows each activity with its old and new values for Start, Finish, Duration, Percent Complete, Status, and Calendar, with rows colored Green (Added), Red (Removed), Yellow (Modified), and Gray (Unchanged).

3. **Given** a completed comparison, **When** the user clicks the Logic tab, **Then** a grid displays broken, added, and modified predecessor/successor relationships with old and new relationship types and lag values.

4. **Given** a completed comparison, **When** the user clicks the Resources tab, **Then** a grid shows resource assignment diffs with units and cost deltas, and a summary bar displays total cost change.

5. **Given** a completed comparison, **When** the user clicks the Critical Path tab, **Then** the user sees side-by-side critical path lists for source and target, CP duration change, and a list of activities that entered or exited the critical path.

6. **Given** a completed comparison, **When** the user clicks the Float tab, **Then** a grid shows total float and free float deltas per activity sorted by float loss, with a filter to show only negative float values.

---

### User Story 2 - Capture and Restore Schedule Snapshots (Priority: P1)

A project planner wants to freeze the current schedule data at a point in time so it can be used as a baseline or update for future comparisons. They capture a snapshot with a descriptive label, and later use it as a source or target in the Compare tab.

**Why this priority**: Snapshots are the foundation of the fallthrough data resolution chain. Without snapshots, users can only compare Primavera imports or live data, limiting the comparison modes.

**Independent Test**: Can be tested by capturing a snapshot of the current schedule, then selecting it as a source or target in the Compare tab and successfully running a comparison.

**Acceptance Scenarios**:

1. **Given** an active project with schedule data, **When** the user captures a snapshot and provides a label, **Then** the snapshot is stored and appears in the source/target picker in the Compare tab with its label and capture timestamp.

2. **Given** a previously captured snapshot, **When** the user restores it as a comparison source, **Then** all activities, relationships, and resource assignments from the capture time are available for comparison.

3. **Given** multiple snapshots for a project, **When** the user views the snapshot list, **Then** each snapshot shows its label, capture timestamp, and allows deletion.

4. **Given** a snapshot linked to a baseline, **When** the snapshot is listed, **Then** the baseline GUID or reference is preserved for traceability.

---

### User Story 3 - Export Comparison Results (Priority: P2)

After running a comparison, a project planner needs to share the results with stakeholders. They export the diff data to Excel for detailed review, to PDF for a formatted report, and to JSON for downstream delay analysis in a future phase.

**Why this priority**: Export completes the comparison workflow by making results portable and consumable by stakeholders and other tools. It is lower priority than the core comparison itself.

**Independent Test**: Can be tested by running a comparison, then exporting to each format and verifying the output file contains the expected data (worksheet per dimension for Excel, formatted tables for PDF, structured JSON for the Phase 11 contract).

**Acceptance Scenarios**:

1. **Given** a completed comparison, **When** the user exports to Excel, **Then** a workbook is generated with one worksheet per comparison dimension (Activities, Logic, Resources, Critical Path, Float) plus a summary sheet, and the file is saved to the project's export folder.

2. **Given** a completed comparison, **When** the user exports to PDF, **Then** a formatted report is generated with a summary section, color-coded tables per dimension, and a file saved to the project's export folder.

3. **Given** a completed comparison, **When** the user exports the comparison result as JSON, **Then** a structured JSON file is generated containing SchemaVersion, Source metadata, Target metadata, IncludedScopes, GeneratedByVersion, and all diff data (ActivityDiffs, LogicDiffs, ResourceDiffs, CriticalPathDiffResult, FloatReport, Summary).

4. **Given** an export that fails due to disk or permission issues, **When** the user checks the session, **Then** the comparison session remains intact and the export can be retried from the History tab.

---

### User Story 4 - Review Past Comparison Sessions and Re-open (Priority: P2)

A project planner needs to review or re-export results from a comparison they ran days or weeks ago. They browse the history of past sessions, re-open a session to view its results, re-export it, or delete old sessions.

**Why this priority**: History and session management turns comparisons from ephemeral operations into permanent project records. It is critical for auditability but not required for the initial comparison to function.

**Independent Test**: Can be tested by running a comparison, navigating to the History tab, finding the session in the list, re-opening it to verify diffs are still viewable, re-exporting it, and deleting it.

**Acceptance Scenarios**:

1. **Given** past comparison sessions for a project, **When** the user opens the History tab, **Then** a grid lists all sessions with date, comparison mode, source/target labels, scope, and summary counts.

2. **Given** a past session selected in the History tab, **When** the user clicks "Re-open", **Then** the Compare tab loads with the session's source, target, and scope pre-selected and the diff tabs populated with the saved results.

3. **Given** a past session, **When** the user clicks "Delete", **Then** the session is soft-deleted and no longer appears in the active history list.

4. **Given** a past session with previously exported files, **When** the user clicks "Re-export", **Then** the export runs again using the stored comparison results and produces new output files.

---

### User Story 5 - Compare Primavera Imports (Priority: P3)

A project planner has imported two Primavera XER files into the project. They want to compare the schedules directly from the imported Primavera data without creating native snapshots first.

**Why this priority**: XER-vs-XER comparison is important for users who work with Primavera-sourced data, but it depends on the Primavera import module (Phase 9) being available, making it a lower priority than snapshot-to-snapshot comparison.

**Independent Test**: Can be tested by importing two Primavera XER files, selecting them as source and target in the Compare tab with XER-vs-XER mode, and verifying that comparison results are correct with Primavera provenance preserved.

**Acceptance Scenarios**:

1. **Given** a project with two imported Primavera XER files, **When** the user selects XER-vs-XER mode in the Compare tab, **Then** both Primavera projects are available in the source and target pickers.

2. **Given** a completed XER-vs-XER comparison, **When** the user views any diff tab, **Then** the source and target metadata on the session indicate Primavera as the source kind with the original Primavera project ID.

3. **Given** a project where Primavera is not installed or registered, **When** the user opens the Schedule Comparison Studio, **Then** XER-vs-XER mode is not available but all other comparison modes function normally.

---

### Edge Cases

- What happens when the user cancels a running comparison? The system should leave no partial session or mark it as Cancelled without storing partial results.
- How does the system handle a comparison where no differences are found between two identical schedules? The summary should show zero changes across all dimensions and the diff grids should be empty.
- What happens when a schedule snapshot is corrupted or its data cannot be deserialized? A clear error message is shown and the user is prompted to re-capture the snapshot or use live data.
- How does the system behave when the user tries to compare a source and target from incompatible data sources (e.g., trying XER-vs-XER when only one Primavera import exists)? The picker should disable invalid combinations or show a clear explanation.
- What happens when the user's project has over 50,000 activities (or 150K relationships, or 5K resources)? The system guarantees operation up to these limits with graceful performance degradation. Beyond these thresholds, a performance warning is shown and processing may be significantly slower.
- How does the export handle empty diff dimensions (e.g., no resource changes)? The corresponding worksheet/section should still appear but show a "No changes detected" message rather than being omitted.
- What happens when the user tries to start a second comparison while one is already running? The Run button is disabled while a comparison is in progress, preventing concurrent execution.
- What happens when the user tries to delete a snapshot that is referenced by an existing comparison session? A confirmation warning is shown indicating which sessions reference the snapshot before allowing deletion.

## Requirements

### Functional Requirements

- **FR-001**: The system MUST allow users to select a source and target schedule from available snapshots, Primavera imports, or live schedule data, and run a comparison across one or more dimensions (activities, logic, resources, critical path, float).
- **FR-002**: The system MUST support four comparison modes: Baseline vs Update, Update vs Update, XER vs XER, and As-Planned vs As-Built.
- **FR-003**: The system MUST display activity diffs in a color-coded grid showing Added (Green), Removed (Red), Modified (Yellow), and Unchanged (Gray) rows with field-level old/new values.
- **FR-004**: The system MUST display logic diffs showing added, removed, and modified relationships with predecessor/successor identifiers, relationship type changes, and lag changes.
- **FR-005**: The system MUST display resource assignment diffs showing added, removed, and modified assignments with units and cost deltas, plus a total cost change summary.
- **FR-006**: The system MUST display critical path diffs comparing CP membership between source and target, showing activities that entered or exited the path and CP duration change.
- **FR-007**: The system MUST display float impact diffs showing total float and free float deltas per activity, ranked by float loss, with a filter for negative float only.
- **FR-008**: The system MUST allow users to capture a schedule snapshot with a user-defined label and restore it later for comparison.
- **FR-009**: The system MUST allow users to list, view details, and delete snapshots per project.
- **FR-010**: The system MUST persist comparison results in two forms: (a) queryable per-diff rows for grid viewing, filtering, and paging; (b) a complete versioned JSON envelope on the session for downstream consumption.
- **FR-011**: The system MUST support exporting comparison results to Excel with one worksheet per dimension.
- **FR-012**: The system MUST support exporting comparison results to PDF as a formatted report with summary and color-coded tables.
- **FR-013**: The system MUST support exporting the complete comparison result as JSON conforming to a versioned schema (including SchemaVersion, Source/Target metadata, IncludedScopes, GeneratedByVersion, and all diff data).
- **FR-014**: The system MUST record a history of all comparison sessions per project, allowing users to re-open past sessions to view results and re-export. Re-opening loads saved results for review; it does not re-run the comparison.
- **FR-021**: The system MUST enforce a session lifecycle with distinct states: Draft (before execution starts), Running (execution in progress), and terminal states Completed, Failed, or Cancelled. Once a session reaches a terminal state, it cannot be re-run — the user must create a new session for a fresh comparison.
- **FR-022**: The system MUST disable the Run Comparison button while a comparison is already executing. Deleting a snapshot that is referenced by any existing session MUST show a confirmation warning before proceeding.
- **FR-023**: The system MUST support schedule sizes up to 50K activities / 150K relationships / 5K resources with predictable performance. Beyond these limits, a performance warning MUST be displayed and the system may degrade gracefully rather than fail.
- **FR-015**: The system MUST handle cancellation of a running comparison without leaving partial session data.
- **FR-016**: The system MUST handle comparison failures by recording the failure status and error message on the session without corrupting other sessions.
- **FR-017**: The system MUST handle export failures gracefully without invalidating the comparison session, and allow the user to retry the export.
- **FR-018**: The system MUST match activities between source and target schedules using a deterministic priority: provenance ID, activity ID, WBS path + activity code, and optionally fuzzy name matching (flagged as low-confidence).
- **FR-019**: The system MUST surface unmatched source activities as Added, unmatched target activities as Removed, and flag low-confidence matches for user review.
- **FR-020**: The system MUST be project-gated — the Schedule Comparison Studio is only accessible when a project is active.

### Key Entities

- **ComparisonSession**: Represents a single comparison run — stores session configuration (mode, source/target metadata, included scopes), lifecycle state (Draft → Running → Completed/Failed/Cancelled), timestamps, and the complete serialized diff envelope for downstream consumption. Terminal states are final; sessions cannot be re-run in-place.
- **ComparisonResult**: A per-diff row record representing one changed entity field — includes match key, change type (Added/Removed/Modified), old value, new value, match confidence, severity, and entity type for queryable grid display, filtering, and paging.
- **ScheduleSnapshot**: A frozen copy of native schedule data (activities, relationships, resource assignments) captured at a point in time with a user-defined label, used as a comparison source or target.
- **ComparisonRule**: Configurable thresholds and settings that control comparison behavior (severity thresholds, matching strategy preferences, dimension toggles).

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can complete a full comparison (all five dimensions) of a moderate schedule (10K activities, 30K relationships, 1K resources) in under 15 seconds.
- **SC-002**: Users see diff grid results within 100ms of switching between comparison dimension tabs for a moderate schedule.
- **SC-003**: Activity matching correctly identifies corresponding activities across 99% of source-target pairs when using deterministic matching strategies (provenance ID, activity ID, or WBS + code).
- **SC-004**: Unmatched and low-confidence matches are surfaced in the UI and accounted for in less than 1% of cases with ambiguous matches.
- **SC-005**: Comparison sessions are queryable and pageable from the History tab within 2 seconds of loading for up to 100 sessions per project.
- **SC-006**: Excel export completes within 30 seconds for a moderate schedule, producing a single workbook with all dimension worksheets present.
- **SC-007**: JSON export conforms to the schema contract (including SchemaVersion, Source, Target, IncludedScopes, and GeneratedByVersion) and is consumable by Phase 11 without transformation.
- **SC-008**: Cancelling a running comparison leaves no partial session — the project comparison history shows either a completed session or no session for that attempted run.
- **SC-009**: A failed export does not delete or invalidate the comparison session — the user can retry export from the History tab without re-running the comparison.
- **SC-010**: Users can capture, list, restore, and delete snapshots without data loss or corruption across all project lifecycle events (re-imports, data migrations).

## Assumptions

- The active project context and basic project infrastructure from earlier phases (Phase 0, 1) is already in place and stable.
- Activity service, resource assignment service, and current project service interfaces from earlier phases (Phase 5, 6) are available and provide the data needed for snapshot capture.
- Primavera import module (Phase 9) exists and provides the workspace service interface, but the Schedule Comparison Studio also works without it.
- The Phase 11 Delay Analysis module does not yet exist; the JSON output contract is defined in this phase for future consumption.
- Snapshot data can be serialized and deserialized within reasonable size limits for moderate schedules.
- Users work with the English or Arabic interface; all labels, summaries, and export content are available in both languages.
- The comparison results are unattributed — no delay cause analysis or EOT entitlement computation is performed in this phase.
- Export files are stored on the local file system under the app-managed project folder; cloud storage is out of scope.
- All long-running operations accept user cancellation and the system remains responsive during processing.
