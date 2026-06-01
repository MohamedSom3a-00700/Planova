# Planova Phase 1 Implementation Plan

**Phase**: 1 - Project Management Foundation

**Date**: 2026-06-01

**Source of Truth**: [docs/01-PRODUCT_VISION.md](./01-PRODUCT_VISION.md),
[docs/02-MASTER_ROADMAP.md](./02-MASTER_ROADMAP.md),
[docs/04-SYSTEM_ARCHITECTURE.md](./04-SYSTEM_ARCHITECTURE.md),
[docs/11-UI_UX_DESIGN_SYSTEM.md](./11-UI_UX_DESIGN_SYSTEM.md),
[`docs/PLANOVA_CONSTITUTION_DRAFT.md`](./PLANOVA_CONSTITUTION_DRAFT.md)

## Summary

Phase 1 delivers the project management foundation that Planova needs before
specialized studios begin. This phase turns the shell from Phase 0 into a real
working workspace for managing projects, clients, contracts, and app-level user
profiles, with supporting dashboard and reporting views that give the user an
immediate command center.

This phase is still foundational, not specialized. It should establish the core
business objects, their relationships, basic workflows, list/detail experiences,
and dashboard reporting surfaces that later studios will depend on.

## Phase 1 Objectives

1. Introduce the Project Management domain model.
2. Build the Projects, Clients, Contracts, and User Profiles experience.
3. Add dashboard and reporting views for project visibility.
4. Create reusable list/detail/edit patterns for later studios.
5. Extend the persistence layer with the first meaningful business schema.
6. Preserve Clean Architecture, MVVM, localization, and theme consistency.
7. Keep User Profiles limited to application profile data only.

## Phase 1 Scope

### In Scope

- Projects
- Clients
- Contracts
- User Profiles
- Project dashboard
- Reporting views for project management
- Search, filtering, sorting, and basic navigation for the new entities
- List/detail/edit flows
- Summary cards and KPI tiles
- Initial business entities and relationships
- Validation rules for project metadata

### Out of Scope

- Authentication and authorization platform design
- Roles and permissions framework
- Multi-user collaboration workflows
- Studio-specific functionality such as BOQ, WBS, scheduling, delay analysis,
  claims, chronology, AI copilot, and integrations
- Advanced analytics platform features
- Workflow automation engine
- Enterprise deployment and cloud scaling work

## Non-Negotiable Constraints

- Clean Architecture must remain intact.
- MVVM must remain the UI pattern.
- Localization must support English and Arabic.
- RTL behavior must remain correct.
- Theme support must remain consistent with Phase 0.
- No direct vendor lock-in for AI or automation.
- User Profiles are application profile data only, not identity management.
- The project management foundation must be reusable by later studio modules.

## Phase 1 Product Shape

Phase 1 should feel like the first working business workspace in Planova.

The user should be able to:

- create and manage projects
- register clients
- define contracts linked to projects and clients
- maintain app-level profile data
- see project status and health at a glance
- open project-focused reporting views
- search and filter records efficiently

## Target Solution Impact

Phase 1 extends the foundation established in Phase 0 and should build on the
existing shell, theme, localization, logging, settings, and database setup.

Expected additions:

- new Application services for project management use cases
- new Domain entities and value objects
- new Infrastructure/Persistence mappings
- new UI modules/views for project management screens
- dashboard widgets and reporting views

## Primary Domain Model

Phase 1 should introduce the following core concepts.

### Project

Represents a managed project record in Planova.

Likely data points:

- project code
- project name
- project description
- client association
- contract association
- project status
- start date
- finish date
- baseline date fields as needed
- currency
- location
- owner
- notes

### Client

Represents the customer or owner organization.

Likely data points:

- client code
- client name
- contact details
- organization details
- notes

### Contract

Represents the commercial agreement connected to a project and client.

Likely data points:

- contract number
- contract title
- client association
- project association
- contract value
- currency
- award date
- commencement date
- completion date
- status
- notes

### User Profile

Represents app-level profile data only.

This is not authentication.
This is not identity infrastructure.

Likely data points:

- display name
- role label for the app UI
- organization name
- preferred language
- preferred theme
- default workspace behavior
- profile note fields

## Screen Plan

### 1. Projects Workspace

Deliver:

- project list view
- project detail view
- project create/edit form
- project search and filtering
- project status summary
- quick actions

### 2. Clients Workspace

Deliver:

- client list view
- client detail view
- client create/edit form
- client search and filtering
- linked projects summary

### 3. Contracts Workspace

Deliver:

- contract list view
- contract detail view
- contract create/edit form
- contract search and filtering
- linked project/client summary

### 4. User Profiles Workspace

Deliver:

- profile detail form
- preference settings
- language and theme preferences surfaced through the profile area
- workspace behavior preferences

### 5. Project Dashboard

Deliver:

- project health cards
- recent activity summary
- projects by status
- upcoming milestones placeholder if data exists
- quick actions

### 6. Reporting Views

Deliver:

- project summary report view
- client summary report view
- contract summary report view
- printable/exportable report previews where practical

## Implementation Order

### 1. Domain Model and Contracts

Start by defining the business entities and the contracts needed by the
application layer.

Deliverables:

- Project entity
- Client entity
- Contract entity
- User profile model
- value objects or shared primitives where needed
- repository/service interfaces only where they support the architecture

Rules:

- Domain stays free of UI and infrastructure concerns.
- Entities should enforce core invariants.
- Shared abstractions should remain small and explicit.

### 2. Persistence Schema and Mappings

Add the first meaningful business schema on top of the Phase 0 database
foundation.

Deliverables:

- EF Core entity mappings
- migrations for project management tables
- indexes and relationship constraints
- seed or default data only if truly needed

Requirements:

- Use code-first migrations.
- Keep relationships explicit.
- Model project/client/contract links clearly.
- Support future studio integration without schema churn.

### 3. Application Services and Use Cases

Create the use cases that power the UI.

Deliverables:

- create project
- update project
- list projects
- view project details
- create client
- update client
- list clients
- view client details
- create contract
- update contract
- list contracts
- view contract details
- load and save user profile preferences
- dashboard summary queries
- report query services

Requirements:

- Application layer owns orchestration.
- Validation should happen before persistence.
- CancellationToken should be supported in async operations.

### 4. UI Workspaces

Add the first real business modules to the shell.

Deliverables:

- Projects workspace
- Clients workspace
- Contracts workspace
- User Profiles workspace
- dashboard landing view
- reporting view surfaces

Requirements:

- Use MVVM throughout.
- Keep views declarative.
- Reuse shell navigation and tab workspace patterns.
- Use Fluent UI WPF styling and existing theme tokens.

### 5. List/Detail/Edit Patterns

Standardize the interaction model so later studio modules can reuse it.

Deliverables:

- reusable data grid layouts
- filter and sort controls
- detail pane patterns
- create/edit dialogs or inline forms
- validation message patterns

Requirements:

- Keep the experience dense and efficient.
- Do not introduce ribbon-heavy UI.
- Maintain large-screen usability.

### 6. Dashboard and Reporting Views

Create visible project management insight without turning this phase into a full
analytics platform.

Deliverables:

- KPI cards
- project status distribution
- recent record summaries
- simple report templates
- print/export friendly preview surfaces

Requirements:

- Dashboard is a command center, not a report dump.
- Reports should stay focused on project management data.
- Keep the reporting layer simple enough to evolve later.

### 7. Localization and RTL Coverage

Expand localization from shell-only coverage to business screens.

Deliverables:

- localized project labels
- localized client labels
- localized contract labels
- localized profile labels
- localized dashboard labels
- localized report labels

Requirements:

- English and Arabic must both be supported.
- No hardcoded user-facing strings in the new screens.
- RTL rendering must remain correct for the new views.

### 8. Validation

Verify the new foundation is usable and coherent.

Deliverables:

- data creation and edit smoke tests
- project/client/contract navigation checks
- dashboard render checks
- report preview checks
- localization checks
- persistence round-trip checks

## Technical Decisions

### Domain Design

Use a clean domain model with explicit entity relationships and behavior that
supports project management today and studio workflows later.

### UI Pattern

Use tab-based workspaces, lists with detail panes, and consistent command
surfaces instead of modal-heavy navigation.

### Reporting

Use lightweight report views and templates that can expand later into richer
reporting and export workflows.

### Data Access

Use the existing EF Core foundation from Phase 0, with mappings and queries
organized by application use case.

### Profile Storage

Keep User Profiles limited to app preferences and display data, not security or
identity infrastructure.

## Detailed Work Breakdown

### Workstream A: Domain and Schema

1. Define the project management entities.
2. Define relationship rules between projects, clients, and contracts.
3. Add schema mappings and migrations.
4. Add indexes for common lookup fields.

### Workstream B: Application Use Cases

1. Implement CRUD and query services.
2. Add validation rules for entity creation and updates.
3. Add dashboard summary query services.
4. Add report-oriented query services.

### Workstream C: UI Workspaces

1. Create the Projects workspace.
2. Create the Clients workspace.
3. Create the Contracts workspace.
4. Create the User Profiles workspace.
5. Wire the workspaces into shell navigation and tab hosting.

### Workstream D: Dashboard and Reports

1. Build project health cards.
2. Build list summary panels.
3. Add report preview pages.
4. Add export/print affordances where the infrastructure already supports them.

### Workstream E: Localization and Polish

1. Add localized resource coverage for all new views.
2. Verify Arabic layout and RTL behavior.
3. Ensure theme consistency with the shell.
4. Remove any hardcoded user-facing text.

### Workstream F: Validation

1. Test create/read/update flows.
2. Test dashboard rendering.
3. Test report pages.
4. Test persistence and restart behavior.

## Risks and Mitigations

### Risk: Phase 1 expands into a full ERP-style system

Mitigation:

- Keep the scope on project management foundation only.
- Avoid adding identity, collaboration, or workflow engine features.

### Risk: Dashboard becomes too ambitious too early

Mitigation:

- Keep dashboards summary-oriented.
- Limit reporting views to the project management domain.

### Risk: User Profiles drift into authentication design

Mitigation:

- Keep profile data app-level only.
- Revisit identity later as a separate decision.

### Risk: Data model becomes too rigid for later studios

Mitigation:

- Keep entities lean.
- Use shared abstractions carefully.
- Leave room for phase-specific extensions.

## Acceptance Criteria

Phase 1 is complete when all of the following are true:

- Projects can be created, edited, listed, and viewed.
- Clients can be created, edited, listed, and viewed.
- Contracts can be created, edited, listed, and viewed.
- App-level User Profiles can be updated and persisted.
- Dashboard views show project management summaries.
- Reporting views render project management summaries.
- English and Arabic screens work correctly.
- RTL behavior remains correct.
- The existing shell and theme system remain intact.
- The implementation remains Clean Architecture and MVVM compliant.

## Definition of Done

- Core project management entities are implemented.
- UI workspaces are integrated into the shell.
- Dashboard and reporting views are functional.
- Localization is complete for the new scope.
- Settings and profile preferences persist correctly.
- The data model is documented enough for Phase 2 and beyond.

## Next Step After Phase 1

Once Phase 1 is stable, the next planning step should focus on the first studio
workflow, most likely:

- BOQ Studio
- WBS Studio
- or another studio chosen by roadmap priority

That decision should be based on the state of project management data and the
user journeys you want to unlock first.
