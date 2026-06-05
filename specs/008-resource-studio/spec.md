# Feature Specification: Resource Studio

**Feature Branch**: `008-resource-studio`

**Created**: 2026-06-05

**Status**: Draft

**Input**: User description: "Phase 6 - Resource Studio for Planova: labour, equipment, material, and subcontractor resource management with crew templates, resource histogram, and AI-powered resource estimation"

## Clarifications

### Session 2026-06-05

- Q: Can resources have the same name? → A: Duplicate names allowed but system warns on save. Uniqueness enforced via resource codes (FR-004) per type.
- Q: What happens to resource assignments when a Phase 5 activity is deleted? → A: Restrict deletion — system prevents activity deletion when resource assignments exist; user must remove assignments first.
- Q: How should currencies be handled in reports? → A: Mixed currencies allowed per rate entry; reports display amounts with their currency code; no cross-currency conversion.
- Q: What is the expected resource library scale? → A: Medium (200-1,000 resources per project). Performance targets assume this range.
- Q: How should resource deletion work when referenced by crews or assignments? → A: Allow deactivation (soft delete) for referenced resources; hard delete only permitted when no crew template or assignment references the resource.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Manage Resource Library (Priority: P1)

A planning engineer needs to create and manage a library of resources (labour trades, equipment, materials, subcontractors) that can be reused across projects. They should be able to browse resources by type, search by name or code, create new resources with type-specific fields, and edit or deactivate existing ones.

**Why this priority**: The resource library is the foundational data store that all other Resource Studio features depend on. Without resources, crews cannot be composed, activities cannot be loaded, and histograms cannot be generated.

**Independent Test**: Can be fully tested by creating, editing, searching, and deleting resources of each type (Labour, Equipment, Material, Subcontractor) and verifying persistence and retrieval.

**Acceptance Scenarios**:

1. **Given** the user opens the Resource Library workspace, **When** they filter by resource type "Labour", **Then** only labour resources are displayed with their trade, skill level, and hourly rate fields visible.
2. **Given** the user creates a new resource, **When** they select type "Equipment" and fill in Equipment-specific fields (type, capacity, operating cost), **Then** the resource is saved and retrievable with all Equipment-specific data intact.
3. **Given** a resource exists in the library, **When** the user deactivates it, **Then** it no longer appears in active resource pickers but remains in the database for historical reference.
4. **Given** the user is on the Resource Library screen, **When** they search by code "R-LBR-001", **Then** only the matching labour resource is shown.

---

### User Story 2 - Manage Resource Rates with Effective Dating (Priority: P1)

A planning engineer needs to set and manage resource rates over time. Labour rates may change at contract renewal dates, and equipment costs may fluctuate. The system should allow adding future-dated rate changes and show rate history.

**Why this priority**: Rate management directly impacts cost calculations. Without accurate rates, resource assignments will compute incorrect total costs, undermining downstream cost loading (Phase 7).

**Independent Test**: Can be fully tested by adding multiple rates with different effective dates for a resource and verifying the correct rate is returned for any given date.

**Acceptance Scenarios**:

1. **Given** a resource has rates with effective dates of Jan 1 ($50/hr) and Mar 1 ($55/hr), **When** the system resolves the rate for Feb 15, **Then** it returns $50/hr (the rate effective on or before that date).
2. **Given** the user adds a new rate for a resource, **When** the effective date is in the future, **Then** the rate is saved and becomes active automatically on that date.
3. **Given** a resource, **When** the user views the rate history, **Then** all past, current, and future rates are displayed chronologically with effective dates.

---

### User Story 3 - Compose and Apply Crew Templates (Priority: P2)

A planning engineer needs to define reusable crew templates (e.g., "Concrete Crew: 3 Carpenters + 1 Foreman + 2 Laborers") with auto-calculated blended rates, and apply these crews to one or more activities in a single operation.

**Why this priority**: Crew templates significantly speed up resource loading by allowing planners to compose standard teams once and reuse them across many activities, reducing repetitive data entry.

**Independent Test**: Can be fully tested by creating a crew template, verifying the blended rate calculation, applying it to an activity, and confirming that individual resource assignments are created for each crew member.

**Acceptance Scenarios**:

1. **Given** a crew with 2 Carpenters ($40/hr each) and 1 Foreman ($55/hr), **When** the blended rate is computed, **Then** it displays $135/hr (2 x $40 + 1 x $55).
2. **Given** a crew template, **When** the user applies it to a selected activity, **Then** individual resource assignments are created for each crew member with the correct quantity and rate.
3. **Given** a crew template, **When** the user applies it to multiple selected activities, **Then** each activity receives the full set of resource assignments.
4. **Given** the user clones a crew template, **When** the clone is created, **Then** all resources and quantities are duplicated with a new name.

---

### User Story 4 - Load Resources onto Activities (Priority: P1)

A planning engineer needs to assign resources (or crew templates) to specific activities from Phase 5, specifying quantities, rates, and assignment dates. The total cost per activity should be computed automatically.

**Why this priority**: Resource loading is the primary operational feature — it connects the resource library to the project schedule and produces the cost data that Phase 7 will consume.

**Independent Test**: Can be fully tested by selecting an activity, assigning resources, editing quantities and rates, and verifying the computed total cost updates in real time.

**Acceptance Scenarios**:

1. **Given** the user selects an activity, **When** they assign a resource with quantity 10 and rate $50/hr, **Then** the total cost displays $500 and the assignment is persisted.
2. **Given** an activity with existing resource assignments, **When** the user edits the quantity from 10 to 15, **Then** the total cost recalculates and updates automatically.
3. **Given** an activity assignment, **When** the user removes it with confirmation, **Then** the assignment is deleted and the total cost updates accordingly.

---

### User Story 5 - View Resource Histogram (Priority: P2)

A planning engineer needs to visualize daily resource usage across the project timeline to identify peak demand periods and potential overallocation conflicts.

**Why this priority**: The histogram provides critical visual insight into resource demand patterns, helping planners avoid scheduling conflicts and optimize resource utilization.

**Independent Test**: Can be fully tested by creating assignments with date ranges and verifying that the histogram bars correctly reflect daily resource quantities with peak markers.

**Acceptance Scenarios**:

1. **Given** resource assignments exist with start and end dates, **When** the user opens the histogram view, **Then** daily usage is plotted as a bar chart across the project timeline.
2. **Given** a resource is overallocated (assigned quantity exceeds available quantity on a given day), **When** the histogram renders, **Then** the affected bars are highlighted with warning indicators.
3. **Given** the histogram is displayed, **When** the user filters by resource type "Labour", **Then** only labour resource usage is shown.
4. **Given** the histogram is displayed, **When** the user exports the data, **Then** the daily usage data is available in a downloadable format.

---

### User Story 6 - Use AI to Estimate Resources (Priority: P3)

A planning engineer wants to quickly populate resource assignments for an activity using AI suggestions based on the activity name, description, and WBS category.

**Why this priority**: AI estimation accelerates the resource loading process for users who are unsure which resources to assign, but manual assignment remains the primary workflow.

**Independent Test**: Can be fully tested by selecting an activity, triggering AI estimation, reviewing suggestions, and accepting or adjusting before committing.

**Acceptance Scenarios**:

1. **Given** the user selects an activity and clicks "Estimate Resources", **When** AI estimation completes, **Then** suggested resources (labour, equipment, materials) are displayed with quantities and confidence indicators.
2. **Given** AI suggestions are displayed, **When** the user clicks "Accept", **Then** resource assignments are created for all suggested items.
3. **Given** AI suggestions are displayed, **When** the user adjusts quantities before accepting, **Then** the modified values are used for the created assignments.
4. **Given** AI estimation is unavailable (no provider configured), **When** the user triggers estimation, **Then** a clear message indicates the service is unavailable and manual assignment is suggested.

---

### User Story 7 - Generate Resource Reports (Priority: P3)

A planning engineer needs to generate resource usage and cost summary reports for review meetings and export them to standard document formats.

**Why this priority**: Reports are important for stakeholder communication but are downstream of the core resource loading functionality.

**Independent Test**: Can be fully tested by loading resources onto activities, generating a resource usage report, and exporting it to a viewable file format.

**Acceptance Scenarios**:

1. **Given** resources are assigned to activities, **When** the user generates a resource usage summary report, **Then** it displays resources grouped by activity with quantities and costs.
2. **Given** a resource cost report is generated, **When** the user exports it, **Then** the file is saved in a standard document format and can be opened externally.
3. **Given** a report is displayed, **When** the user requests a print preview, **Then** a printer-friendly view is shown.

---

### Edge Cases

- What happens when a resource is deactivated but is currently assigned to an activity? The assignment remains valid, but the resource is marked as inactive in pickers and cannot be assigned to new activities.
- What happens when a user tries to delete a resource that is referenced by a crew template or resource assignment? Hard delete is blocked with a message listing the references. The user can deactivate the resource instead to preserve historical data. Unreferenced resources can be hard-deleted.
- How does the system handle a crew template with zero resources? The crew should not be usable (apply should be disabled) until at least one resource is added.
- What happens when a rate effective date overlaps with an existing rate for the same resource? The system should prevent duplicate effective dates and notify the user.
- How does histogram handle assignments without start/end dates? Those assignments should be excluded from the histogram with a clear indication.
- What happens when AI estimation times out or returns incomplete data? The system should show partial results if available, or a clear error message, and suggest manual assignment.
- How does importing resources from Excel handle duplicate codes? The system should detect duplicates and either skip, overwrite, or rename based on user choice during import.
- What happens when a resource rate is changed after a crew template has been applied? Existing assignments retain the rate at time of application; the crew template's blended rate is recalculated for future applications.
- What happens when the user saves a resource with a name that duplicates an existing resource in the same scope? A warning is displayed with the matching resource name and type, but the save is allowed to proceed. Uniqueness is enforced via the auto-generated resource code (FR-004).
- What happens when a user attempts to delete a Phase 5 activity that has resource assignments? The system prevents deletion and shows a message listing the assignments that must be removed first.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Users MUST be able to create, view, edit, and deactivate resources of four types: Labour, Equipment, Material, and Subcontractor.
- **FR-002**: Each resource type MUST display type-specific fields appropriate to its category (e.g., trade and skill level for Labour; capacity and operating cost for Equipment; unit price and wastage for Materials; company and contract value for Subcontractors).
- **FR-003**: Resources MUST support both Global scope (shared across all projects) and Project-specific scope.
- **FR-004**: Resource codes MUST be auto-generated sequentially per type with a prefix (e.g., Labour: R-LBR-001, Equipment: R-EQP-001).
- **FR-005**: Users MUST be able to search resources by name, code, and category, and filter by resource type and project scope.
- **FR-005b**: When saving a resource with a name that matches an existing resource within the same scope (Global or Project), the system MUST display a warning but allow the save to proceed.
- **FR-006**: Users MUST be able to define multiple rates per resource with effective date ranges.
- **FR-007**: The system MUST resolve the applicable rate for a resource on any given date by selecting the rate with the latest effective date on or before that date, falling back to the resource's default rate if none exists.
- **FR-008**: Users MUST be able to view rate history (past, present, and future-dated rates) for any resource.
- **FR-009**: Users MUST be able to perform bulk rate updates across multiple resources filtered by type or category.
- **FR-010**: Users MUST be able to create, edit, clone, and delete crew templates.
- **FR-011**: A crew template MUST allow composing multiple resources with quantities and an optional lead indicator per resource.
- **FR-012**: The system MUST compute and display the blended rate for a crew template based on the sum of (resource quantity × resource rate) for all crew members.
- **FR-013**: Users MUST be able to apply a crew template to one or more selected activities, generating individual resource assignments for each crew member.
- **FR-014**: Users MUST be able to assign resources directly to Phase 5 activities with quantity, rate, unit of measure, and optional start/end dates.
- **FR-015**: The system MUST compute the total cost of each resource assignment (quantity × rate, considering duration for hourly resources).
- **FR-016**: Users MUST be able to view a resource histogram showing daily resource quantity usage across the project timeline.
- **FR-017**: The histogram MUST display peak usage markers and highlight overallocation when assigned quantity exceeds available quantity on any day.
- **FR-018**: Users MUST be able to filter the histogram by resource type, specific resource, time range, and aggregation (sum, average, peak).
- **FR-019**: Users MUST be able to trigger AI-based resource estimation for an activity, receiving suggested labour, equipment, and material requirements.
- **FR-020**: AI suggestions MUST be presented for user preview — users can accept all, adjust quantities before accepting, or reject suggestions entirely.
- **FR-021**: The AI estimation feature MUST gracefully handle provider unavailability and suggest manual assignment as a fallback.
- **FR-022**: Users MUST be able to generate a resource usage summary report (grouped by activity) and a resource cost report (grouped by type, crew, activity).
- **FR-023**: Reports MUST be exportable to a standard spreadsheet format and a standard document format.
- **FR-024**: Users MUST be able to import a resource library from a standard spreadsheet file.
- **FR-025**: All user-facing text in the Resource Studio MUST be available in both English and Arabic, with correct RTL layout support.

### Key Entities

- **Resource**: A labour trade, piece of equipment, material, or subcontractor that can be assigned to project activities. Supports both Global (shared) and Project-specific scope. Type-specific attributes vary by Resource type.
- **ResourceRate**: A specific rate value for a resource that becomes effective on a given date. Supports future-dated rate changes and rate history tracking.
- **Crew**: A reusable template grouping multiple resources with quantities to form a standard work team. Has auto-calculated blended rate.
- **CrewResource**: Links a specific resource (with quantity and lead flag) to a crew template.
- **ResourceAssignment**: Links a resource or crew to a Phase 5 activity with quantity, rate, unit, and optional date range. Stores the total cost computed at assignment time.
- **ResourceUsage**: Tracks planned and actual daily usage quantities for histogram computation, linked to a specific resource assignment.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create a new resource of any type (Labour, Equipment, Material, Subcontractor) with all type-specific fields in under 2 minutes.
- **SC-002**: Users can browse the resource library, filter by type, and find a specific resource by name or code in under 10 seconds.
- **SC-003**: Resource rate resolution for any given date returns the correct rate with 100% accuracy based on effective date rules.
- **SC-004**: Users can compose a crew template with 6+ resources and see the blended rate calculated in under 3 seconds.
- **SC-005**: Users can assign resources to an activity and see the total cost computed in under 2 seconds.
- **SC-006**: The resource histogram renders daily usage for a project with 500+ resource assignments in under 5 seconds.
- **SC-007**: AI resource estimation suggests appropriate resources for a given activity description in under 30 seconds (with typical provider response time).
- **SC-008**: Users can generate and export a resource report in under 10 seconds for projects with up to 500 resource assignments.
- **SC-009**: All Resource Studio screens render correctly in both English and Arabic without layout breakage.
- **SC-010**: Resource Studio integrates into the existing multi-tab workspace without breaking existing Phase 0-5 functionality.

## Assumptions

- Users are planning engineers familiar with construction resource categories (labour trades, equipment types, materials, subcontractors).
- The existing Phase 5 Activity module is available and provides the activity data that Resource Studio consumes.
- Expected resource library scale: 200-1,000 resources per project. Performance targets in Success Criteria assume this range.
- Resource assignments feed into Phase 7 (Cost Studio) for cost loading, cash flow, and earned value — this spec covers only creation and storage of resource data.
- Resource leveling and smoothing are out of scope and will be addressed in a later phase.
- The AI estimation feature uses an abstracted provider interface — exact AI model choice is an implementation decision.
- Resource rate currency defaults to USD with the ability to change per rate entry. Reports display amounts with their original currency code; no cross-currency conversion is performed.
- Existing Phase 2 Excel/PDF export infrastructure is reused for reports and import.
- Users have a stable network connection when using the AI estimation feature.
- Resource library import assumes a standard spreadsheet format consistent with Phase 2's import infrastructure.
