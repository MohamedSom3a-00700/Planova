# Feature Specification: Project Management Foundation

**Feature Branch**: `002-pm-foundation`

**Created**: 2026-06-01

**Status**: Draft

**Input**: User description: "Phase 1 implementation plan for Planova - project management foundation including projects, clients, contracts, user profiles, dashboard, and reporting views"

## Clarifications

### Session 2026-06-01

- Q: What project statuses and lifecycle transitions should the system support? → A: Full lifecycle with review gates: Draft → Under Review → Approved → In Progress → On Hold → Completed → Cancelled, with conditional transitions per role.
- Q: How should entity identity and uniqueness be enforced? → A: Project code and contract number globally unique; client code unique; client name unique within organization.
- Q: What data volume should the system handle in Phase 1? → A: Up to 1,000 records per entity.
- Q: What level of change tracking should the system provide? → A: Created and last-modified timestamps on every entity.

## User Scenarios & Testing

### User Story 1 - Manage Projects (Priority: P1)

A user can create, view, edit, search, and track projects with key metadata including status, dates, client association, and contract linkage.

**Why this priority**: Projects are the central entity in the project management domain — all other entities (clients, contracts, dashboard, reports) depend on projects existing first.

**Independent Test**: Can be fully tested by creating a new project with required fields, verifying it appears in the project list, editing its details, and confirming changes persist after refresh.

**Acceptance Scenarios**:

1. **Given** the user is on the Projects workspace, **When** they create a new project with name, code, status, and dates, **Then** the project appears in the project list with the correct details.
2. **Given** a project exists, **When** the user edits any of its fields, **Then** the changes are saved and reflected in the project detail view.
3. **Given** multiple projects exist, **When** the user searches by project name or code, **Then** only matching projects are shown.
4. **Given** the user views a project detail, **When** they navigate to its linked client or contract, **Then** the corresponding detail view opens.

---

### User Story 2 - Manage Clients (Priority: P1)

A user can register and maintain client organizations with contact details, and view which projects are linked to each client.

**Why this priority**: Clients are the second core entity — projects and contracts require client associations, making client management equally foundational.

**Independent Test**: Can be fully tested by creating a client record, verifying it appears in the client list, linking it to a project, and confirming the relationship is visible from both the client detail view and the project view.

**Acceptance Scenarios**:

1. **Given** the user is on the Clients workspace, **When** they add a new client with name, code, and contact details, **Then** the client is saved and displayed in the client list.
2. **Given** a client has linked projects, **When** the user views the client detail, **Then** the linked projects summary shows all associated projects.
3. **Given** an existing client record, **When** the user updates contact information, **Then** the changes persist and are shown on subsequent views.

---

### User Story 3 - Manage Contracts (Priority: P2)

A user can create and manage contracts tied to specific projects and clients, including contract value, currency, and date tracking.

**Why this priority**: Contracts depend on both projects and clients existing, and provide commercial context. They are essential but can follow project and client creation.

**Independent Test**: Can be fully tested by creating a project and client, then creating a contract linked to both, verifying the contract appears under the project and client detail views.

**Acceptance Scenarios**:

1. **Given** a project and client exist, **When** the user creates a contract with number, title, value, and linked project/client, **Then** the contract is saved and visible in the contracts list.
2. **Given** a contract exists, **When** the user views the linked project detail, **Then** the associated contract is shown in the project summary.
3. **Given** a contract with a monetary value and currency, **When** the user edits the contract value, **Then** the update is reflected in dashboard and report totals.

---

### User Story 4 - Configure User Profile (Priority: P2)

A user can view and update their application profile including display name, language preference, theme, and default workspace behavior.

**Why this priority**: Profile management enhances user experience by allowing personalization, and the language/theme preferences directly impact usability for Arabic-speaking users.

**Independent Test**: Can be fully tested by changing the display name and language preference, confirming the UI switches to Arabic, and verifying the preference persists after restart.

**Acceptance Scenarios**:

1. **Given** the user opens their profile, **When** they update their display name and preferred language, **Then** the changes are saved and the UI language updates accordingly.
2. **Given** the user selects a preferred theme, **When** they save the profile, **Then** the application theme matches the selection on subsequent sessions.
3. **Given** the user sets a default workspace behavior, **When** they restart the application, **Then** the default workspace is applied.

---

### User Story 5 - View Project Dashboard (Priority: P2)

A user can see a dashboard with project health cards, status distribution, recent activity, and quick actions for an at-a-glance command center.

**Why this priority**: The dashboard provides immediate visibility into project status and health, making it the primary landing experience for project managers.

**Independent Test**: Can be fully tested by creating projects in different statuses, opening the dashboard, and verifying that health cards and status distribution reflect the current data.

**Acceptance Scenarios**:

1. **Given** projects exist in various statuses, **When** the user opens the dashboard, **Then** health cards show correct project counts and status distribution.
2. **Given** recent project activity, **When** the dashboard loads, **Then** the recent activity summary lists the most recent changes.
3. **Given** the user clicks a quick action on the dashboard, **When** they select create new project, **Then** the create project form opens.

---

### User Story 6 - View Reports (Priority: P3)

A user can open summary report views for projects, clients, and contracts with printable or export-friendly formatting.

**Why this priority**: Reports are valuable for external communication but depend on all entities being fully functional. They can be delivered after core workflows are stable.

**Independent Test**: Can be fully tested by populating projects, clients, and contracts, opening each report view, and verifying the data renders correctly in a formatted preview.

**Acceptance Scenarios**:

1. **Given** projects, clients, and contracts exist, **When** the user opens the project summary report, **Then** all projects are listed with key metrics in a printable format.
2. **Given** a report view is open, **When** the user triggers export, **Then** the report content is exported in the supported format.
3. **Given** the user switches to Arabic locale, **When** they open any report view, **Then** the report content and layout respect RTL orientation.

---

### Edge Cases

- What happens when a user creates a project without linking a client or contract? The system should allow projects to exist independently, with optional client/contract links added later.
- What happens when a user deletes a linked entity (e.g., deleting a client that has associated projects)? The system should prevent deletion of entities that have active dependencies, or warn the user about cascading effects.
- What happens when required fields are missing during project/client/contract creation? The system should display validation messages for each missing required field.
- How does the system handle concurrent access or stale data? The system should detect when data has changed since the user loaded it and notify them before overwriting.
- What happens when a user attempts an invalid status transition (e.g., Draft → Completed directly)? The system should block the transition and display a message listing the allowed next statuses.
- What happens when a user tries to create a project with a project code that already exists? The system should reject the duplicate and prompt the user to use a unique code.

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow users to create, read, update, and delete project records with fields including project code (globally unique), name, description, status (Draft, Under Review, Approved, In Progress, On Hold, Completed, Cancelled), start date, finish date, currency, location, and notes.
- **FR-002**: System MUST allow users to create, read, update, and delete client records with fields including client code (unique), name (unique within organization), contact details, organization details, and notes.
- **FR-003**: System MUST allow users to create, read, update, and delete contract records with fields including contract number (globally unique), title, value, currency, award date, commencement date, completion date, status, and notes, linked to one project and one client.
- **FR-004**: System MUST allow users to view and edit their application profile including display name, role label, organization name, preferred language, preferred theme, and default workspace behavior.
- **FR-005**: System MUST provide a project list view with search, filtering by the seven defined statuses (Draft, Under Review, Approved, In Progress, On Hold, Completed, Cancelled), and sorting by key fields.
- **FR-006**: System MUST provide a client list view with search and filtering capabilities.
- **FR-007**: System MUST provide a contract list view with search and filtering capabilities linked to project and client context.
- **FR-008**: System MUST display a dashboard with project health cards showing project counts by status, recent activity summary, and quick action buttons.
- **FR-009**: System MUST provide summary report views for projects, clients, and contracts with formatted previews suitable for printing or export.
- **FR-010**: System MUST validate required fields on entity creation and display user-friendly error messages for missing or invalid data.
- **FR-011**: System MUST enforce relationship integrity by preventing deletion of clients or projects that have active linked contracts, displaying a clear warning.
- **FR-017**: System MUST enforce valid status transitions based on the project lifecycle: Draft → Under Review → Approved → In Progress; In Progress ↔ On Hold; any status → Completed or Cancelled. Invalid transitions MUST be blocked with an explanation.
- **FR-018**: System MUST prevent creation or update of entities that would produce duplicate project codes, client codes, client names, or contract numbers, displaying a clear message identifying the conflict.
- **FR-012**: System MUST persist all entity data across application restarts and automatically record created-at and last-modified-at timestamps for every entity.
- **FR-013**: System MUST reflect language and theme preference changes immediately upon saving the user profile.
- **FR-014**: System MUST display all user-facing text in English or Arabic based on the user's language preference, with no hardcoded strings.
- **FR-015**: System MUST render all views correctly in right-to-left layout when Arabic language is selected.
- **FR-016**: System MUST display linked entity summaries on detail views (e.g., projects linked to a client, contracts linked to a project/client).

### Key Entities

- **Project**: A managed work effort with code (globally unique), name, description, status (Draft → Under Review → Approved → In Progress ↔ On Hold, any → Completed or Cancelled), dates, currency, location, client association, contract association, and notes. The central entity that other domain objects relate to.
- **Client**: A customer or owner organization with code (unique), name (unique within organization), contact details, organization info, and notes. Can be linked to multiple projects and contracts.
- **Contract**: A commercial agreement with number (globally unique), title, value, currency, key dates, status, and notes. Links to exactly one project and one client.
- **User Profile**: Application-level user settings including display name, role label, organization, preferred language, preferred theme, and default workspace behavior. Separate from authentication and identity management.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A user can create a project, client, and contract with all required fields filled in under 2 minutes per entity.
- **SC-002**: A user can locate any project, client, or contract record within 3 interactions (searches, filters, or navigation steps).
- **SC-003**: The dashboard loads and displays accurate project health data within 2 seconds of opening.
- **SC-004**: All user-facing screens display correctly in both English and Arabic without layout issues or untranslated text.
- **SC-005**: A user can switch their language preference from English to Arabic and see the UI update within 1 interaction cycle (no restart required).
- **SC-006**: Users can complete all primary workflows (create, view, edit, list, search, and navigate between entities) without encountering errors or unhandled states.
- **SC-007**: Report previews render all data correctly within 3 seconds for up to 1,000 records and are suitable for export or printing.
- **SC-008**: List views load and display search results within 2 seconds when filtering across 1,000 records.

## Assumptions

- The existing Phase 0 shell (navigation, theming, localization framework, database setup) is available and stable.
- The application targets a single concurrent user for this phase — multi-user collaboration and role-based access are out of scope.
- Projects, Clients, and Contracts use optional relationships: a project can exist without a client or contract, but a contract requires both a project and a client.
- The user is assumed to be a project manager or team member familiar with standard project management terminology.
- All entity data is stored persistently using the existing Phase 0 database infrastructure.
- Report export formats are limited to what the existing infrastructure supports (e.g., print-to-PDF via the OS).
- Mobile or responsive layout support is out of scope for this phase — the UI targets desktop resolutions.
- Validation rules use reasonable defaults: required fields include project name/code, client name/code, contract number/title; dates are validated for order (start before finish); monetary values are positive numbers.
- The system is designed for up to 1,000 records per entity (Project, Client, Contract) in Phase 1. Pagination and search performance targets assume this volume.
