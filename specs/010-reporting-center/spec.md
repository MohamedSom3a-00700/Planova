# Feature Specification: Reporting Center

**Feature Branch**: `010-reporting-center`

**Created**: 2026-06-12

**Status**: Draft

**Input**: User description: "Phase 8 Reporting Center implementation plan covering cross-studio report orchestration, four consolidated report types (Daily, Weekly, Monthly, Executive), scheduled generation, AI narrative generation, unified report management, and project parties management"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Generate and View Daily Reports (Priority: P1)

A project manager opens the Reports hub, selects the Daily Report tab, picks a date, and immediately sees auto-populated sections: activities started and completed today, workforce on site by trade, equipment deployed, issues/remarks (editable), and attached site photos from project documents. The user can generate an AI narrative summary of the day's progress, edit it, regenerate it, or accept it.

**Why this priority**: Daily reporting is the most frequent reporting task on construction projects — site engineers and project managers need a quick daily snapshot.

**Independent Test**: Can be fully tested by opening the Daily Report tab for a project with activities, resources, and project documents. Verify all sections populate with correct data from the selected date.

**Acceptance Scenarios**:

1. **Given** a project with activities scheduled on a specific date, **When** the user selects that date in the Daily Report, **Then** the Progress Today section shows activities started, completed, and in-progress for that date
2. **Given** the project has resource assignments, **When** viewing the Daily Report, **Then** the Workforce section shows labour count by trade with total hours, and the Equipment section shows deployed equipment by type and count
3. **Given** the Daily Report is displayed, **When** the user clicks "Generate AI Narrative", **Then** a one-paragraph professional daily progress summary appears in an editable text box
4. **Given** an AI narrative is displayed, **When** the user edits the text and saves the report, **Then** the edited narrative is preserved
5. **Given** the Daily Report is complete, **When** the user exports it, **Then** valid Excel, PDF, and Word files are generated

---

### User Story 2 - Generate and View Weekly Reports (Priority: P1)

The user selects the Weekly Report tab, picks a week, and sees progress by WBS items with percent complete and variance, resource usage totals for the week (labour, equipment, materials), delay items (activities with negative float or behind schedule), and a look-ahead section showing activities scheduled for the next 2 weeks. The user can generate a 2-paragraph AI weekly status summary.

**Why this priority**: Weekly reports are the standard reporting cadence for project progress reviews with stakeholders and site management.

**Independent Test**: Can be tested by opening the Weekly Report for a project with activities, WBS structure, and resource assignments spanning multiple weeks. Verify the progress, resource totals, delays, and look-ahead data are accurate.

**Acceptance Scenarios**:

1. **Given** a project with activities across multiple WBS items, **When** the user selects a week in the Weekly Report, **Then** the Progress by WBS section shows each WBS item with percent complete and variance
2. **Given** the project has delayed activities, **When** viewing the Weekly Report, **Then** the Delays section lists activities with negative float or behind-schedule status, including delay days and reasons
3. **Given** the Weekly Report is displayed, **When** the user views the Look-Ahead section, **Then** it shows activities scheduled for the next 2 weeks
4. **Given** the Weekly Report is displayed, **When** the user generates an AI narrative, **Then** a 2-paragraph weekly status summary is produced

---

### User Story 3 - Generate and View Monthly Reports with EVM (Priority: P1)

The user selects the Monthly Report tab, picks a month, and sees EVM metric cards (CPI, SPI, CV, SV, EAC, ETC, VAC) color-coded for quick health assessment, an S-Curve chart showing cumulative planned vs actual cost, a budget vs actual table, progress by WBS, resource productivity metrics, and a 3-4 paragraph AI narrative.

**Why this priority**: Monthly reports provide the financial and performance health check required for steering committee reviews and investor reporting.

**Independent Test**: Can be tested by opening the Monthly Report for a project with cost data, EVM metrics, and cash flow data. Verify all metrics and charts display correctly.

**Acceptance Scenarios**:

1. **Given** a project with a cost baseline and actual costs entered, **When** the user opens the Monthly Report, **Then** EVM metric cards display (CPI, SPI, CV, SV, EAC, ETC, VAC) with color coding
2. **Given** the project has cash flow data, **When** viewing the Monthly Report, **Then** the S-Curve chart shows cumulative planned vs actual cost over time
3. **Given** the Monthly Report is displayed, **When** the user views the Budget vs Actual section, **Then** it shows a detailed comparison of budget breakdown vs incurred costs
4. **Given** the Monthly Report is displayed, **When** the user generates an AI narrative, **Then** a 3-4 paragraph detailed status narrative with EVM analysis is produced

---

### User Story 4 - Generate and View Executive Reports (Priority: P2)

The user selects the Executive Report tab, picks a date range, and sees high-level KPI cards (CPI, SPI, Budget Health), S-Curves, risk highlights (top delayed activities and critical path risks), financial overview (total budget, spent, remaining, EAC), milestone status, and a 3-5 paragraph AI executive narrative. This tab also provides access to the legacy Project Directory view (entity list with PDF export) as a sub-tab or quick-export button.

**Why this priority**: Executive reports serve senior management and external stakeholders — important but depend on all underlying data being in place.

**Independent Test**: Can be tested by opening the Executive Report for a fully populated project. Verify KPI cards, charts, financial summary, and milestone data are correct.

**Acceptance Scenarios**:

1. **Given** a project with complete cost and progress data, **When** the user opens the Executive Report, **Then** the KPI Dashboard shows high-level cards (CPI, SPI, Budget Health, EAC)
2. **Given** the Executive Report is displayed, **When** viewing the Risk Highlights section, **Then** top delayed activities and critical path risks are listed
3. **Given** the Executive Report is displayed, **When** the user generates an AI narrative, **Then** a 3-5 paragraph executive-level strategic narrative is produced
4. **Given** the user needs access to entity lists (projects, clients, contracts), **When** viewing the Executive Report tab, **Then** a Project Directory sub-tab or quick-export button provides access to the legacy entity list with PDF export

---

### User Story 5 - Schedule Automatic Report Generation (Priority: P2)

The user navigates to the Schedule tab and configures automatic report generation. For each report type (Daily, Weekly, Monthly), the user sets the frequency, time of day, timezone, export formats (Excel, PDF, Word), and activates the schedule. The system auto-generates reports at the configured times. The user can view the schedule grid with next-run time, last-run status, and active/inactive toggle.

**Why this priority**: Scheduled generation reduces manual effort and ensures reports are consistently available on time, but is not essential for the initial report viewing workflow.

**Independent Test**: Can be tested by creating schedules at various frequencies and verifying they generate reports at the expected times.

**Acceptance Scenarios**:

1. **Given** the Schedule tab, **When** the user adds a new schedule (Daily, Weekly, or Monthly with time and export format), **Then** it appears in the schedule grid with the computed Next Run time
2. **Given** an active schedule, **When** the scheduled time arrives and the application is running, **Then** the report is auto-generated and appears in the History tab
3. **Given** a schedule fails repeatedly, **When** the retry count exceeds the maximum, **Then** the schedule is automatically deactivated with a warning
4. **Given** a schedule exists, **When** the user toggles it inactive, **Then** no auto-generation occurs for that schedule

---

### User Story 6 - View Report History and Re-export (Priority: P2)

The user opens the History tab and sees all previously generated reports in a filterable grid. The user can filter by report type, date range, and status (Draft, Final, Archived). From each row, the user can open and preview the report, re-export it to any format, archive it, or delete it.

**Why this priority**: History and re-export are essential for audit and stakeholder follow-up, but require reports to exist first.

**Independent Test**: Can be tested by generating multiple reports and verifying they appear in the history with correct metadata. Export and archive actions can be tested independently.

**Acceptance Scenarios**:

1. **Given** multiple reports have been generated, **When** the user opens the History tab, **Then** the grid displays all reports with title, type, period, status, and generated date
2. **Given** the History grid is displayed, **When** the user filters by report type and date range, **Then** only matching reports are shown
3. **Given** a report in the history, **When** the user clicks "Open", **Then** the full report preview is displayed
4. **Given** a report in the history, **When** the user re-exports it, **Then** a valid file in the chosen format is generated
5. **Given** a report in the history, **When** the user archives it, **Then** its status changes to Archived and it is hidden from the default view

---

### User Story 7 - Customize Report Templates and Settings (Priority: P3)

The user navigates to the Templates tab and reorders sections or toggles section visibility per report type. In the Settings tab, the user configures which content sections appear in each report type independently (e.g., hide Equipment from Daily reports, or reorder sections in the Executive report). The Settings tab also allows the user to define project parties: Client name with logo, Main Contractor name with logo, and one or more Sub Contractors each with name and logo.

**Why this priority**: Customization enhances user satisfaction but is not required for core reporting functionality.

**Independent Test**: Can be tested by modifying section visibility and order for each report type, then generating a report and verifying the changes are reflected. Project parties settings can be tested independently.

**Acceptance Scenarios**:

1. **Given** the Settings tab, **When** the user unchecks a section for a report type, **Then** that section no longer appears when generating that report type
2. **Given** the Templates tab, **When** the user reorders sections, **Then** the generated report displays sections in the new order
3. **Given** the Project Parties section in Settings, **When** the user adds a Client with name and logo, **Then** the client details appear in all report headers
4. **Given** project parties are defined, **When** the user generates any report, **Then** the header shows Client logo + name (left) and Main Contractor logo + name (right), with Sub Contractors listed in a dedicated section
5. **Given** the Settings tab, **When** the user clicks "Restore Defaults" for a report type, **Then** all sections are re-enabled for that type

### Edge Cases

- What happens when no project is active? The Reports nav item is disabled (gated like other studios).
- How does the system handle AI service unavailability? AI narrative buttons show a disabled state with a message indicating the service is temporarily unavailable. Existing narratives remain editable.
- What happens when schedule timezone changes? Schedules auto-adjust on the next computation cycle — no user action required.
- How are report data snapshots handled? Data is frozen at generation time so that reports always reflect the state when generated, even if source data changes later.
- What happens to export files when a report instance is deleted? Export files remain on disk for audit trail; the ReportExport record is soft-deleted. Only explicit "Delete Export" permanently removes files.
- How does the system handle scheduled reports when the application is closed? Missed schedules are caught up on next application launch (within MaxRetries limit).
- What happens if the same report instance is re-exported? The export file is overwritten and the ReportExport record is updated — no duplicate files.
- How does the system handle scheduling for Executive reports? The Schedule tab supports Daily, Weekly, and Monthly only. Executive reports are generated on-demand or can be scheduled through monthly frequency.
- What does the Reporting Hub show when a project has no data yet? Each tab shows a contextual empty state with guidance linking to the relevant studio — e.g., "No activities found for this date. Add activities in Activity Studio."
- What happens when two users generate a report for the same project+period+type simultaneously? Both generations succeed as separate ReportInstance records. Users manage duplicates from History.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a 7-tab Reporting Hub workspace (Daily Report, Weekly Report, Monthly Report, Executive Report, Schedule, History, Templates/Settings) that requires an active project to access
- **FR-002**: Daily Report tab MUST auto-populate sections based on a selected date: project info, progress today (activities started/completed/in-progress), workforce by trade, equipment deployed, issues (editable), and photos from project documents
- **FR-003**: Weekly Report tab MUST auto-populate sections based on a selected week: project info, progress by WBS with percent complete and variance, resource usage totals, delay items, and look-ahead for the next 2 weeks
- **FR-004**: Monthly Report tab MUST auto-populate sections based on a selected month: project info, EVM metric cards (CPI, SPI, CV, SV, EAC, ETC, VAC), S-Curve chart, budget vs actual table, progress by WBS, and resource productivity metrics
- **FR-005**: Executive Report tab MUST auto-populate sections based on a selected period: executive summary, KPI dashboard cards, S-Curves, risk highlights, financial overview, and milestone status
- **FR-006**: Executive Report tab MUST provide access to the legacy Project Directory (entity list with PDF export) as a sub-tab or quick-export button in the hub header
- **FR-007**: All four report types MUST support on-demand AI narrative generation that produces type-appropriate narrative lengths (1 paragraph daily, 2 paragraphs weekly, 3-4 paragraphs monthly, 3-5 paragraphs executive)
- **FR-008**: AI narrative MUST be displayed in an editable text box with Generate, Regenerate, and Accept workflow; edits MUST be preserved on save
- **FR-009**: AI-generated narratives MUST be clearly labeled as "AI-Generated"
- **FR-010**: System MUST degrade gracefully when AI services are unavailable — disable AI buttons, show informative message, allow manual narrative entry
- **FR-011**: All report types MUST support export to Excel, PDF, and Word formats
- **FR-012**: Excel export MUST create one workbook per report with each section as a separate worksheet
- **FR-013**: PDF export MUST produce a formatted document with cover page (client/contractor logos and names), table of contents, sectioned content, embedded chart images, and page-numbered footer
- **FR-014**: Word export MUST produce a narrative-focused .docx document with headings, body paragraphs, tables, and basic styling
- **FR-015**: Report data MUST be snapshotted (frozen) at generation time so reports reflect the state when generated, regardless of subsequent source data changes
- **FR-016**: Report lifecycle MUST be forward-only: Draft → Final → Archived. Once a report is Final, it cannot return to Draft. Once Archived, it cannot be unarchived. Users must generate a new report instance to produce updated content
- **FR-017**: System MUST support scheduled auto-generation via configurable schedules (Daily, Weekly, Monthly) with time-of-day, timezone, and export format preferences
- **FR-018**: Scheduled generation MUST run as an in-process timer while the application is running; missed schedules MUST be caught up on next launch (within configurable MaxRetries limit)
- **FR-019**: Schedules MUST auto-deactivate after consecutive failures exceeding the MaxRetries threshold (default 3)
- **FR-020**: System MUST display a Schedule grid showing report type, frequency, time, last run status, next run time, and active/inactive toggle per schedule
- **FR-021**: System MUST provide a Report History view with filterable grid (by type, date range, status) showing title, type, period, status, and generated date
- **FR-022**: Users MUST be able to open, re-export, archive, and delete reports from the History view
- **FR-023**: Users MUST be able to configure section visibility per report type in Settings — independently choose which content sections appear in each of the four report types
- **FR-024**: Users MUST be able to reorder and toggle section visibility in report templates per report type
- **FR-025**: Users MUST be able to manage Project Parties per project: define Client (one per project), Main Contractor (one per project), and one or more Sub Contractors, each with name, logo, address, and contact information
- **FR-026**: All report headers MUST auto-populate with Client and Main Contractor logos and names (left/right); Sub Contractors MUST be listed in a dedicated section
- **FR-027**: System MUST support logo image upload for project parties with storage under `{AppData}/Planova/Projects/{projectId}/Parties/`
- **FR-028**: The existing "Reports" nav item MUST remain project-gated (disabled when no project is active) and point to the new Reporting Hub
- **FR-029**: The existing `IReportService` (Projects/Clients/Contracts entity lists with PDF export) MUST be retained for backward compatibility and accessible from the Executive Report tab
- **FR-030**: All Reporting Hub screens MUST support English and Arabic (RTL) localization without layout issues
- **FR-031**: All async operations MUST accept and respect CancellationToken for cancellation
- **FR-032**: System MUST log critical report operations — report generation (manual and scheduled), exports, report/schedule deletions, and schedule activate/deactivate — with timestamp and user identity. Routine views and settings lookups are NOT logged

### Key Entities

- **ReportInstance**: A generated report with frozen data snapshot, status (Draft, Final, Archived), AI narrative text, and period start/end dates. Each instance belongs to a project and a specific report type.
- **ReportTemplate**: Defines the structure, section ordering, and section visibility for a report type. Can be project-specific or global (default).
- **ReportSection**: A section within a report instance (Text, Table, Chart, Image, or AiNarrative) containing section-specific data as JSON. Derived from the data snapshot at generation time.
- **ReportSchedule**: A scheduled generation configuration per project and report type, with frequency (Daily, Weekly, Monthly), time-of-day, timezone, export formats, retry tracking, and next-run computation.
- **ReportExport**: A tracking record for each export operation — records format (Excel, PDF, Word), file path, file size, and export timestamp per report instance.
- **ProjectParty**: A party involved in a project (Client, Main Contractor, or Sub Contractor) with name, logo, address, and contact details. Used in report headers and footers.
- **ReportSettings**: Per-project, per-report-type configuration defining which content sections are enabled for each of the four report types.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can open any of the four report type tabs and see auto-populated data in under 5 seconds for a project with up to 500 activities
- **SC-002**: AI narratives generate and display in under 15 seconds for all four report types
- **SC-003**: Export to Excel, PDF, and Word completes in under 15 seconds for a standard project report
- **SC-004**: Users can create and activate a report schedule in under 1 minute
- **SC-005**: Scheduled reports generate within 2 minutes of the configured time when the application is running
- **SC-006**: The History grid loads and displays results in under 3 seconds with up to 200 generated reports
- **SC-007**: Users can configure section visibility for a report type in under 30 seconds
- **SC-008**: Users can add or edit project parties (Client, Contractor, Sub Contractors) in under 1 minute
- **SC-009**: All Reporting Center features function correctly in both English and Arabic (RTL) without layout issues
- **SC-010**: All existing project functionality from prior phases remains fully operational after Reporting Center is deployed
- **SC-011**: The legacy Project Directory (entity list with PDF export) remains accessible from the Executive Report tab
- **SC-012**: The old Reports nav target (entity list) no longer appears; the new Reporting Hub is the sole Reports destination

## Assumptions

- Activity, resource, cost, WBS, and BOQ data from prior studios (Phases 2-7) are available and accurate at the time of report generation
- Project parties (Client, Main Contractor, Sub Contractors) are defined per project; logo images are uploaded as image files
- AI services will have a reasonable internet connection and API availability; graceful degradation covers unavailability
- Existing per-studio report tabs (inside BOQ, WBS, Resource, Cost studios) remain unchanged — Reporting Center is additive, not a replacement
- The application must be running for scheduled reports to generate; missed schedules are caught up on next launch
- Users have basic familiarity with construction reporting concepts (daily reports, EVM, S-Curves)
- Reports are single-user (no multi-user collaboration or approval workflows in this phase); any user with project access has full view/generate/export/delete permissions on all reports for that project
- All studio data is consumed through existing service interfaces — no direct database queries from the Reporting module
- Expected data volumes: up to 500 report instances, 100 schedules, 200 export records, and 20 project parties per project
- Currency formatting follows project-level settings from prior phases
- The existing IReportService (entity lists with PDF export) remains operational for backward compatibility

## Clarifications

### Session 2026-06-12

- Q: Should Executive reports be schedule-able or on-demand only? → A: The Schedule tab supports Daily, Weekly, and Monthly frequencies only. Executive reports are generated on-demand. If users need automatic Executive reports, they can use Monthly frequency.
- Q: What happens to export files when a project is deleted? → A: All report files under the project's Reports folder are deleted from disk, and all associated records are hard-deleted (cascade).
- Q: How are report data snapshots stored and used? → A: `DataSnapshotJson` is the single authoritative source — a serialized JSON of the typed DTO frozen at generation time. `ReportSections` are derived views for efficient UI display. Both are generated together.
- Q: Who can access the Reporting Center? → A: Same access as the project — any user who can open the project studio has full access to its reports. No additional role-based access control for reports in this phase.
- Q: What level of activity logging is required? → A: Log critical operations only — report generation (manual + scheduled), exports, deletions, and schedule activate/deactivate. Routine views and settings lookups are not logged.
- Q: What should empty states look like for projects with no data? → A: Show contextual empty state per tab with guidance linking to the relevant studio — e.g., "No activities found. Add activities in Activity Studio."
- Q: What are the report status lifecycle transition rules? → A: Forward-only — Draft → Final (one-way), Final → Archived (one-way). No way to go back. Users must generate a new report instance to update a Final report.
- Q: How should concurrent report generation for the same project+period+type be handled? → A: Allow duplicates — both generations succeed and create separate ReportInstance records. Users manage duplicates from History (archive/delete).
