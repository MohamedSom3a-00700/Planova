# Feature Specification: Cost Studio - Cost Management System

**Feature Branch**: `009-cost-studio`

**Created**: 2026-06-06

**Status**: Draft

**Input**: User description: "Phase 7 Cost Studio implementation plan covering cost loading, direct costs, budget management, cash flow forecasting, EVM, actual cost entry, AI cost services, and reports"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Cost Breakdown and Manage Direct Costs (Priority: P1)

A project manager opens Cost Studio and immediately sees a hierarchical cost breakdown: project phases (WBS) → activities → resource costs (automatically loaded from previously assigned resources) plus any direct cost lines (permits, insurance, overhead). The user can add, edit, or delete direct cost items at either the project level or attached to a specific activity.

**Why this priority**: This is the foundational view — without seeing cost structure, no other cost management is possible.

**Independent Test**: Can be fully tested by loading a project with activities that have resources assigned, then verifying the cost breakdown tree displays the correct cost totals from resource assignments. User can also add a direct cost and see it appear in the tree.

**Acceptance Scenarios**:

1. **Given** a project with activities that have resource assignments, **When** the user opens Cost Studio, **Then** the cost breakdown tree shows WBS items, activities, and resource assignment costs with computed totals
2. **Given** the cost breakdown tree is displayed, **When** the user adds a direct cost (category, description, quantity, unit, rate), **Then** it appears in the tree at the selected level (project or activity) with the correct computed total
3. **Given** a direct cost exists in the tree, **When** the user edits its quantity or rate, **Then** the total updates automatically
4. **Given** a direct cost exists, **When** the user deletes it, **Then** it is removed from the tree and budget totals reflect the change

---

### User Story 2 - Manage Budget with Revisions and Contingency (Priority: P1)

The user views the project budget summary: resource costs + direct costs + contingency = Total Budget. The user can set a contingency amount (absolute or percentage), record budget revisions (Original → Revised → Approved), and optionally override the computed total with a manual budget figure.

**Why this priority**: The budget is the central financial control mechanism for the project. Without it, cost tracking has no reference point.

**Independent Test**: Can be fully tested by creating a project budget, adding a revision, approving it, and verifying the budget summary reflects the correct components.

**Acceptance Scenarios**:

1. **Given** a project with resource costs and direct costs, **When** the user views the budget summary, **Then** the Total Budget displays as the sum of resource costs, direct costs, and contingency
2. **Given** the budget summary, **When** the user enters a contingency amount, **Then** the Total Budget updates to include it
3. **Given** an active budget, **When** the user creates a budget revision (e.g., Revised type with an amount), **Then** it appears in the revision history with Pending status
4. **Given** a pending budget revision, **When** the user approves it, **Then** its status changes to Approved

---

### User Story 3 - Enter Actual Costs and Track Performance (Priority: P1)

The user enters actual costs incurred for each activity, either manually by typing the total amount or by importing data from an Excel spreadsheet. The system matches imported rows to activities by their codes. After entry, the user sees cost variance (planned vs actual) for each activity.

**Why this priority**: Actual cost data is required for EVM computation, cash flow accuracy, and cost control decisions.

**Independent Test**: Can be fully tested by manually entering an actual cost for an activity and seeing the variance column update. Can also import a spreadsheet and verify which rows were matched successfully.

**Acceptance Scenarios**:

1. **Given** a list of project activities, **When** the user enters an actual cost amount for an activity, **Then** it is saved and the variance (planned cost vs actual) is displayed
2. **Given** the actual cost view, **When** the user imports an Excel file with activity codes and cost amounts, **Then** matched rows are imported and unmatched rows are reported as errors
3. **Given** an activity already has an actual cost entered, **When** the user imports a new value for the same activity, **Then** the existing value is updated (not duplicated)
4. **Given** an actual cost entry with different currency from the project default, **When** viewed, **Then** the currency code is displayed alongside the amount

---

### User Story 4 - View Cash Flow with S-Curve (Priority: P2)

The user toggles between weekly and monthly cash flow periods to see planned vs actual costs over time. A chart shows the cumulative planned cost (S-Curve) alongside cumulative actual cost, helping the user quickly assess whether the project is spending ahead of or behind schedule.

**Why this priority**: Cash flow visibility is essential for financial planning and stakeholder reporting, but depends on actual cost data being entered first.

**Independent Test**: Can be tested by viewing the cash flow for a project with activities that have planned dates and resource costs. The period table populates with planned costs spread across the schedule.

**Acceptance Scenarios**:

1. **Given** a project with activities that have planned start/finish dates, **When** the user opens the cash flow view, **Then** planned costs are spread across weekly periods between the project start and end dates
2. **Given** the cash flow view, **When** the user toggles from weekly to monthly periods, **Then** the table and chart update to show monthly aggregation
3. **Given** actual costs have been entered, **When** viewing cash flow, **Then** the chart shows both cumulative planned (S-Curve) and cumulative actual cost lines

---

### User Story 5 - Monitor Earned Value Management (EVM) (Priority: P2)

The user sets a Data Date (the cutoff date for EVM calculation) and immediately sees key performance metrics: Cost Performance Index (CPI), Schedule Performance Index (SPI), Cost Variance (CV), Schedule Variance (SV), Estimate at Completion (EAC), Estimate to Complete (ETC), and Variance at Completion (VAC). Metrics are color-coded (green/amber/red) for quick health assessment.

**Why this priority**: EVM is the industry standard for project performance measurement, but requires baseline, actual costs, and progress data to function.

**Independent Test**: Can be tested by setting a baseline, entering some progress (% complete) and actual costs, then setting a Data Date and verifying the EVM metrics compute correctly.

**Acceptance Scenarios**:

1. **Given** a project without a baseline, **When** the user opens EVM view, **Then** a clear warning is shown indicating no baseline is set, with a prompt to set one
2. **Given** a baseline has been set, **When** the user selects a Data Date, **Then** all EVM metrics compute and display (CPI, SPI, CV, SV, EAC, ETC, VAC)
3. **Given** EVM metrics are displayed, **When** CPI or SPI is below 1.0, **Then** it appears in red; when at or above 1.0, it appears in green

---

### User Story 6 - Use AI-Powered Cost Services (Priority: P3)

The user can ask the system to estimate the cost of an activity based on its description and assigned resources, detect cost anomalies (activities where actual costs significantly exceed planned), forecast final project cost based on current trends, and generate a professional cost status narrative for reports.

**Why this priority**: AI features enhance decision-making but are not required for core cost management functionality.

**Independent Test**: Can be tested by triggering each AI action and verifying output is generated. AI unavailability should show a clear disabled-state message.

**Acceptance Scenarios**:

1. **Given** an activity with a description and assigned resources, **When** the user clicks "Estimate Cost", **Then** an AI-generated cost suggestion is displayed with confidence level and reasoning
2. **Given** a project with actual costs entered, **When** the user clicks "Detect Anomalies", **Then** activities where actual costs significantly exceed planned are flagged with severity levels
3. **Given** EVM metrics are available, **When** the user requests a cost forecast, **Then** an AI-adjusted EAC estimate is displayed alongside the formula-based EAC
4. **Given** EVM metrics and anomalies are available, **When** the user requests a cost narrative, **Then** a 2-4 paragraph professional status narrative is generated

---

### User Story 7 - Generate and Export Reports (Priority: P3)

The user generates any of four cost reports: Cost Breakdown (WBS → Activity → Resources → Direct Costs), Cash Flow (period table + chart), EVM (metrics + activity details), and Budget Summary (Original/Revised/Approved/Contingency). Each report can be exported to Excel or PDF. The EVM report includes an AI-generated narrative.

**Why this priority**: Reporting is essential for stakeholder communication but depends on all underlying data being in place.

**Independent Test**: Can be tested by viewing each report in the application and verifying data is correct. Export to Excel and PDF can be tested independently.

**Acceptance Scenarios**:

1. **Given** all cost data is entered, **When** the user generates the Cost Breakdown Report, **Then** it shows the complete WBS → Activity → Resource → Direct Cost hierarchy with totals
2. **Given** any report is displayed, **When** the user clicks "Export to Excel", **Then** a valid Excel file is downloaded
3. **Given** any report is displayed, **When** the user clicks "Export to PDF", **Then** a valid PDF file is downloaded
4. **Given** the EVM Report, **When** the user views it, **Then** an AI-generated narrative tab is available with a "Regenerate" button

### Edge Cases

- What happens when no baseline has been set but EVM view is opened? Show a prominent warning and guide the user to set a baseline.
- How does the system handle duplicate activity codes during actual cost import? Flag duplicates as errors and skip those rows, allowing the import to continue for valid rows.
- What happens when resource costs change after the budget has been set? The budget shows an indicator that resource costs have been updated since last save.
- How does the system handle mixed currencies? Each amount displays its currency code. No automatic conversion is performed.
- What happens if AI services are unavailable? AI features show disabled buttons with a message indicating the service is temporarily unavailable.
- How does the system handle a project with no activities or resource assignments? Cost breakdown tree shows an empty state with guidance to first set up activities and resources.
- When an activity is deleted from Activity Studio, what happens to its associated direct costs and actual costs? They are preserved as orphaned records, marked as referencing a deleted activity, with a UI indicator warning the user.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a hierarchical cost breakdown tree showing WBS items, activities under each WBS item, resource assignment costs under each activity (loaded from resource assignments), and direct cost lines under activities or at project level
- **FR-002**: Users MUST be able to add direct cost items with category, description, quantity, unit of measure, unit rate, and scope (project or specific activity)
- **FR-003**: Users MUST be able to edit and delete direct cost items
- **FR-004**: System MUST provide standard direct cost categories: Permits, Insurance, Overhead, Preliminaries, Mobilization, Demobilization, Testing, Other, and Custom (user-defined)
- **FR-005**: System MUST compute total direct cost amount as quantity × unit rate automatically
- **FR-006**: System MUST display a budget summary showing: resource cost total, direct cost total, contingency amount, and computed Total Budget (BAC)
- **FR-007**: Users MUST be able to set a contingency amount (as an absolute value or percentage)
- **FR-008**: System MUST allow the user to manually override the computed Total Budget with a manual value
- **FR-009**: System MUST support budget revision tracking with revision types (Original, Revised, Approved), revision number (auto-incremented), amount, status (Pending, Approved), reason, approval tracking, and history
- **FR-010**: Users MUST be able to approve a pending budget revision, changing its status to Approved
- **FR-011**: System MUST allow users to set a cost baseline, which snapshots the current activity costs and schedule for EVM reference
- **FR-012**: System MUST support only one active baseline per project at a time
- **FR-013**: Users MUST be able to enter actual costs per activity (single total amount) manually
- **FR-014**: Users MUST be able to import actual costs from an Excel file with two modes: per-activity total or per-resource-assignment (aggregated to activity total)
- **FR-015**: System MUST match imported rows to activities by activity code (case-insensitive), report unmatched rows as errors, and update existing actual cost records (upsert)
- **FR-016**: Users MUST be able to toggle cash flow view between weekly and monthly periods
- **FR-017**: System MUST display a cash flow table with period, planned cost, actual cost, cumulative planned, and cumulative actual columns
- **FR-018**: System MUST display an S-Curve chart showing cumulative planned cost vs cumulative actual cost over time
- **FR-019**: Users MUST be able to configure a Data Date that drives EVM computation
- **FR-020**: System MUST compute and display EVM metrics: Planned Value (PV), Earned Value (EV), Actual Cost (AC), Cost Variance (CV), Schedule Variance (SV), Cost Performance Index (CPI), Schedule Performance Index (SPI), Estimate at Completion (EAC), Estimate to Complete (ETC), and Variance at Completion (VAC)
- **FR-021**: System MUST provide AI-powered cost estimation: given activity description and assigned resources, suggest a budget with confidence level and reasoning
- **FR-022**: System MUST provide AI-powered anomaly detection: flag activities where actual costs significantly exceed planned, with severity levels
- **FR-023**: System MUST provide AI-powered EAC/ETC forecast based on current CPI trends
- **FR-024**: System MUST provide AI-powered cost narrative generation (2-4 paragraph professional status summary)
- **FR-025**: System MUST degrade gracefully when AI services are unavailable (disable AI buttons, show informative message)
- **FR-026**: System MUST generate four cost reports: Cost Breakdown, Cash Flow, EVM, and Budget Summary
- **FR-027**: Users MUST be able to export each report to both Excel and PDF
- **FR-028**: EVM report MUST include an AI narrative tab with regenerate capability
- **FR-029**: System MUST display currency codes alongside amounts; no automatic currency conversion is performed
- **FR-030**: System MUST show a warning indicator when resource assignment costs have changed since the last budget save
- **FR-031**: System MUST log budget revision creation/approval, baseline setting/removal, and manual BAC override with user identity and timestamp for audit purposes
- **FR-032**: When an activity is deleted, its associated direct costs and actual cost records MUST be preserved as orphaned records, clearly marked as referencing a deleted activity, with a visible UI indicator
- **FR-033**: Actual cost import MUST accept files up to 5000 rows with a performance warning displayed at 5000+ rows; files exceeding 10000 rows MUST be rejected. Import MUST abort if more than 20% of rows contain unmatched activity codes

### Key Entities *(include if feature involves data)*

- **Budget**: The project's financial plan — aggregates resource costs, direct costs, and contingency into a total budget; supports manual override.
- **Budget Revision**: A recorded change to the budget — tracks Original, Revised, and Approved versions with status, reason, and approval information.
- **Direct Cost**: A non-resource cost item (permits, insurance, overhead, etc.) attached to the project or a specific activity, computed as quantity × unit rate.
- **Cost Baseline**: A point-in-time snapshot of activity costs and schedule used as the reference for EVM; only one active baseline per project.
- **Actual Cost**: The real cost incurred for an activity, entered manually or imported; one record per activity.
- **Cash Flow Period**: A time bucket (weekly or monthly) showing planned cost, actual cost, and cumulative values for S-Curve visualization.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view a complete cost breakdown tree (WBS → Activity → Resource → Direct Costs) within 3 seconds of opening Cost Studio for a project with up to 500 activities
- **SC-002**: Users can add a direct cost item in under 30 seconds using no more than 3 clicks after opening the direct cost manager
- **SC-003**: Users can import actual costs from a spreadsheet of 1000 rows in under 10 seconds, with all unmatched rows clearly reported
- **SC-004**: Cash flow view toggles between weekly and monthly periods in under 2 seconds for a 2-year project
- **SC-005**: EVM metrics compute and display in under 3 seconds after setting a Data Date
- **SC-006**: AI cost estimation, anomaly detection, and forecast results appear within 15 seconds of user request
- **SC-007**: All four reports generate and become available for export in under 10 seconds
- **SC-008**: Excel and PDF exports complete in under 15 seconds for a standard project
- **SC-009**: Users can complete the budget revision workflow (create, approve) in under 1 minute
- **SC-010**: All cost features function correctly in both English and Arabic (RTL) without layout issues
- **SC-011**: All existing project functionality from prior phases remains fully operational after Cost Studio is deployed

## Assumptions

- Resource assignment costs from the Resource Studio (Phase 6) are available and accurate at the time of cost loading
- Activity planned dates and percent complete from the Activity Studio (Phase 5) are available for EVM computation
- Users have basic familiarity with project cost management concepts (budget, EVM, cash flow)
- Users have Excel installed or available for spreadsheet import/export workflows
- AI services will have a reasonable internet connection and API availability; graceful degradation covers unavailability
- Existing project structure (WBS, activities, resources) has been established before using Cost Studio
- Currency is set at the project level; amounts stored without automatic conversion
- The system is used by a single user at a time for cost entry (no multi-user conflict resolution required)
- Standard direct cost categories (Permits, Insurance, Overhead, Preliminaries, Mobilization, Demobilization, Testing, Other, Custom) are sufficient for initial release
- Users will set a baseline before relying on EVM metrics for decision-making
- Expected data volumes per project: up to 100 budget revisions, 500 direct cost items, and 500 actual cost records
- Any user who can access Cost Studio has full read/write/edit/delete/approve permissions on all cost data (no role-based access control for v1)

## Clarifications

### Session 2026-06-06

- Q: Are there distinct user permission levels for Cost Studio? → A: Single role with full access for all Cost Studio users. Any user who can open Cost Studio has full read, write, edit, delete, and approve access to all cost data.
- Q: Should cost modifications be logged for audit purposes? → A: Yes, log sensitive operations only — budget revision creation/approval, baseline setting/removal, and manual BAC override with user identity and timestamp.
- Q: When an activity is deleted, what happens to its associated direct costs and actual costs? → A: Preserve as orphaned records, marked as referencing a deleted activity, with a UI indicator.
- Q: What are the import file constraints for actual costs? → A: Soft limit 5000 rows (warning displayed), hard cap 10000 rows (rejected). Abort if >20% unmatched activity codes.
- Q: What are the expected data volumes per project? → A: Enterprise-scale — up to 100 budget revisions, 500 direct cost items, 500 actual cost records per project.
