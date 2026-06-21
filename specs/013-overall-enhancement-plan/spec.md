# Feature Specification: Overall Enhancement Plan

**Feature Branch**: `013-overall-enhancement-plan`

**Created**: 2026-06-17

**Status**: Draft

**Input**: User description: "docs\PLANOVA_OVERALL_ENHANCEMENT_PLAN.md"

## User Scenarios & Testing

### User Story 1 - Dashboard Overview with Project Health (Priority: P1)

A project manager opens Planova and lands on the dashboard. They see summary cards showing project health (schedule health percentage, cost health percentage, risk level, critical activities count) alongside budget metrics (original budget, current budget, actual cost, earned value, CPI, SPI). Cards use Fluent UI styling with icons for quick scanability and a light glass-effect presentation.

**Why this priority**: The dashboard is the first screen users see; delivering actionable project health at a glance is the highest-value improvement.

**Independent Test**: Can be fully tested by opening the dashboard after configuring a project with known values and verifying all health and cost cards display correctly.

**Acceptance Scenarios**:

1. **Given** a project with completed activities and costs entered, **When** the user opens the dashboard, **Then** schedule health percentage, cost health percentage, risk level, and critical activities count are displayed.
2. **Given** a project with budget and actual cost data, **When** the dashboard loads, **Then** original budget, current budget, actual cost, earned value, CPI, and SPI cards are shown.
3. **Given** the dashboard is loaded, **When** the user scans the page, **Then** all cards display relevant icons and are styled consistently with Fluent UI and glass-effect visuals.

---

### User Story 2 - Multi-Sheet BOQ Import with Auto-Detection (Priority: P1)

A quantity surveyor imports a multi-worksheet Excel BOQ file. The system scans all worksheets, auto-detects BOQ sheets by recognizing columns (Code, Description, Unit, Qty, Rate, Amount), skips non-BOQ sheets (Cover, Summary, Index), and offers import modes: selected sheet only, multiple sheets separately, or merge all into one. After import, a summary shows total sheets, sections, items, and total amount, with the original worksheet name saved for traceability.

**Why this priority**: BOQ import is a core daily workflow; improving it reduces errors and saves significant manual effort.

**Independent Test**: Can be fully tested by importing an Excel file with 3 BOQ worksheets and 2 non-BOQ worksheets, selecting the merge mode, and verifying one consolidated BOQ with 3 section nodes.

**Acceptance Scenarios**:

1. **Given** an Excel file with multiple worksheets, **When** the user selects it for import, **Then** all worksheets are scanned and BOQ sheets are auto-detected.
2. **Given** non-BOQ worksheets (Cover, Summary, Index, Instructions, Notes, Calculation), **When** scanning completes, **Then** those sheets are ignored.
3. **Given** detected BOQ worksheets, **When** the user chooses "Merge all BOQ sheets into one BOQ", **Then** a root BOQ is created with one section node per worksheet preserving original sheet order.
4. **Given** the import completes, **When** results are shown, **Then** total sheets imported, total sections created, total BOQ items, and total amount are displayed.
5. **Given** any worksheet is imported, **When** the import completes, **Then** the original worksheet name is saved as SourceSheet for traceability.

---

### User Story 3 - WBS Creation from BOQ Mapping (Priority: P1)

A planner creates a WBS structure by selecting an existing BOQ and choosing a mapping method (By Section, By CSI, By Cost Code, By Trade, By Discipline). A preview is generated, and upon confirmation, a new WBS is created in the List tab with items, weight, and hierarchical codes auto-generated.

**Why this priority**: WBS is the central structural backbone that connects all downstream modules; enabling creation from existing BOQ data is the fastest path to value.

**Independent Test**: Can be fully tested by selecting a BOQ with sections and CSI codes, mapping by Section, previewing the result, and confirming creation — verifying the WBS appears in the List tab with correct hierarchy and auto-generated codes.

**Acceptance Scenarios**:

1. **Given** a project with a BOQ containing sections, **When** the user selects "Map by Section" in the WBS Mapping tab, **Then** a preview of the resulting WBS hierarchy is displayed.
2. **Given** the preview is shown, **When** the user confirms creation, **Then** a new WBS appears in the List tab with items, weight, and auto-generated structured codes (1, 1.1, 1.2, etc.).
3. **Given** a WBS is created from BOQ mapping, **When** the user views it in the Tree tab, **Then** the hierarchy is visually displayed with expand/collapse and search controls.

---

### User Story 4 - Project Folder Auto-Creation (Priority: P2)

A user selects or creates a project and assigns a project folder path. The system automatically creates the standard subfolder structure: Contract, BOQ, Drawing, Specification, Schedule, Claim, Reports, Correspondence, Photo, Meeting & Presentations, Archive, Lookahead, Logos.

**Why this priority**: Automated folder setup saves administrative time and enforces a consistent project organization standard.

**Independent Test**: Can be fully tested by creating a new project, specifying a folder path, and verifying all 13 subfolders are created.

**Acceptance Scenarios**:

1. **Given** a new project with a user-specified folder path, **When** the project is saved, **Then** all 13 standard subfolders are created automatically.
2. **Given** an existing project with an already-populated folder, **When** the user changes the folder path, **Then** the new folder structure is created at the new path.

---

### User Story 5 - WBS Editor with Drag-and-Drop (Priority: P2)

A planner works in the WBS Editor tab, building and refining the WBS tree. They can add root, child, and sibling nodes, reorder via drag-and-drop or button controls (Move Up/Down, Indent/Outdent), edit properties (Code, Name, Description, Level, Weight, Owner, Discipline, Deliverable, Start/Finish Date, Notes) in a side panel, and see codes regenerate automatically after structural changes.

**Why this priority**: The editor is the primary WBS manipulation tool; drag-and-drop and auto-code generation significantly improve editing efficiency.

**Independent Test**: Can be fully tested by creating nodes, reordering them via drag-and-drop, editing properties, and verifying auto-generated codes reflect the new structure.

**Acceptance Scenarios**:

1. **Given** the WBS Editor tab is open with a selected WBS, **When** the user clicks "Add Child" on a selected node, **Then** a new child node is added and codes regenerate.
2. **Given** a WBS with multiple nodes, **When** the user drags a node to a new parent, **Then** the node moves, and all codes update automatically.
3. **Given** a node is selected, **When** the user edits properties in the side panel, **Then** changes are reflected immediately in the tree.

---

### User Story 6 - Project Screen with Full Metadata (Priority: P2)

A project manager views the Projects screen and sees rich metadata for each project: progress percentage, health indicator, current data date, budget, number of activities, last updated date, project logo, project cover image, connected XER and database details, last import/export dates. From here they can open the project folder, preview documents, and see cost/performance cards.

**Why this priority**: Enriched project metadata reduces the need to navigate into other modules for basic status information.

**Independent Test**: Can be fully tested by configuring a project with all metadata fields and verifying they display correctly on the Projects screen with icons.

**Acceptance Scenarios**:

1. **Given** a project with all metadata populated, **When** the Projects screen loads, **Then** progress percentage, health indicator, budget, activities count, and last updated date are visible.
2. **Given** a project with a connected XER file, **When** viewing the project details, **Then** connected XER and last import/export dates are shown.
3. **Given** a project with logo and cover image uploaded, **When** viewing the project, **Then** both images are displayed.

---

### Edge Cases

- What happens when an Excel BOQ file has no recognizable BOQ columns? The system should show a clear error message and allow manual column mapping.
- What happens when a user tries to create a WBS from a BOQ that has no sections or cost codes? The system should warn the user and suggest alternative mapping methods.
- What happens when a user changes the project folder path after files have been created in the old path? The system should create the structure at the new path without deleting the old one.
- What happens when BOQ import encounters a very large Excel file (100K+ rows)? The system should process without loading unnecessary worksheets into memory and show progress.
- What happens when WBS auto-code generation encounters a cycle in the tree structure? The system should detect cycles and prevent code generation with an error message.

## Clarifications

### Session 2026-06-17

- Q: What user roles and permissions should govern BOQ/WBS operations? → A: Role-based — Project Manager (full access + approve/archive), Planner (create/edit WBS, no approve), Quantity Surveyor (import BOQ + map to WBS), Viewer (view only)
- Q: What are the valid WBS state transitions? → A: Standard workflow with limited reversals: Draft → Under Review → Approved → Archived; plus Draft → Archived (cancelled), Under Review → Draft (revisions requested), Approved → Under Review (reopen)
- Q: Which AI service should power WBS generation from documents? → A: External AI API (e.g., OpenAI, Azure AI) with configurable endpoint; required for unstructured document analysis
- Q: How should concurrent WBS editing conflicts be handled? → A: Lock-based — one user edits at a time; others see read-only notice until lock is released
- Q: How should empty and loading states be presented across screens? → A: Contextual empty states with guidance text and suggested next actions; inline loading indicators for async operations

## Requirements

### Functional Requirements

- **FR-001**: Dashboard MUST display health indicators (schedule health %, cost health %, risk level, critical activities count) for each active project.
- **FR-002**: Dashboard MUST display cost and performance cards (original budget, current budget, actual cost, earned value, CPI, SPI).
- **FR-003**: Dashboard cards MUST use icons and a consistent visual style (Fluent UI with light glass-effect).
- **FR-004**: Projects screen MUST display progress percentage, health indicator, current data date, budget, number of activities, last updated date, project logo, and project cover image.
- **FR-005**: Projects screen MUST show connected XER, connected database, last import date, and last export date.
- **FR-006**: Projects screen MUST provide a button to open the project folder in the file system.
- **FR-007**: Projects screen MUST support previewing project documents (Excel, PDF) in a preview panel.
- **FR-008**: Projects screen MUST support a project layout selector for images, DWG, or PDF.
- **FR-009**: Projects screen MUST support Google Maps link with location preview after saving.
- **FR-010**: When a project folder is assigned, the system MUST auto-create the following subfolders: Contract, BOQ, Drawing, Specification, Schedule, Claim, Reports, Correspondence, Photo, Meeting & Presentations, Archive, Lookahead, Logos.
- **FR-011**: Parties area MUST support managing Clients, Main Contractor, Subcontractors, and Consultant with full details and logo per party.
- **FR-012**: Parties MUST be explicitly linked to projects and reusable across the project workflow.
- **FR-013**: BOQ Studio MUST auto-detect worksheets with BOQ columns (Code, Description, Unit, Qty/Quantity, Rate, Amount, minimum populated rows) when importing from Excel.
- **FR-014**: BOQ Studio MUST ignore non-BOQ worksheets: Cover, Summary, Index, Instructions, Notes, Calculation.
- **FR-015**: BOQ Studio MUST offer import mode options: import selected sheet only, import multiple sheets separately, merge all BOQ sheets into one.
- **FR-016**: When merging all sheets, BOQ Studio MUST create a root BOQ with one section node per worksheet, preserving original sheet order.
- **FR-017**: BOQ import MUST display a summary: total sheets imported, total sections created, total BOQ items imported, total amount.
- **FR-018**: BOQ import MUST save the original worksheet name as SourceSheet for traceability.
- **FR-019**: BOQ Studio MUST handle different column layouts per worksheet using a column mapping engine.
- **FR-020**: BOQ Studio MUST support large Excel files without loading unnecessary worksheets into memory.
- **FR-021**: BOQ Studio MUST use a built-in heuristic/matching algorithm to identify correct column mapping during import, with no external AI service dependency
- **FR-022**: BOQ Studio MUST remove the tab editor, validate BOQ tab (merge into import flow), and libraries tab.
- **FR-023**: BOQ Studio MUST integrate with ProjectId so BOQ data is project-specific.
- **FR-024**: BOQ Studio MUST add split-view in the tree tab.
- **FR-025**: BOQ Studio MUST provide a preview grid before import in a list view with split view.
- **FR-026**: BOQ exports MUST include: Tender BOQ, Client BOQ, Excel, PDF.
- **FR-027**: BOQ reports MUST include: BOQ summary, cost summary, trade summary, CSI summary.
- **FR-028**: BOQ MUST support integration with Schedule (WBS).
- **FR-029**: WBS Studio MUST include tabs: List, Tree, Editor, Mapping, Templates, AI Generation, Reports, Settings.
- **FR-030**: WBS List tab MUST show all WBS definitions with columns: Name, Description, Status, Source, Revision, Items Count, Weight, Created By, Created Date, Modified Date.
- **FR-031**: WBS List tab MUST support actions: Create, Duplicate, Rename, Delete, Archive, Export.
- **FR-032**: WBS sources MUST include: BOQ, Template, AI, Manual, Imported, Primavera.
- **FR-033**: WBS statuses MUST include: Draft, Under Review, Approved, Archived.
- **FR-034**: WBS Tree tab MUST be a read-only visual viewer with WBS selection, Expand All, Collapse All, and Search Node controls.
- **FR-035**: WBS Tree tab MUST support view modes: Standard, Primavera Colors, Weight View, Responsibility View.
- **FR-036**: WBS Editor tab MUST include a WBS Tree and a Properties Panel side-by-side.
- **FR-037**: WBS Editor MUST support adding root, child, and sibling nodes; delete, duplicate, rename.
- **FR-038**: WBS Editor MUST support reordering via buttons (Move Up, Move Down, Indent, Outdent) and drag-and-drop (change order, parent, level).
- **FR-039**: WBS Editor MUST include a context menu with: Add Child, Add Sibling, Duplicate, Rename, Delete, Move Up, Move Down, Indent, Outdent.
- **FR-040**: WBS Properties Panel MUST include: Code, Name, Description, Level, Weight, Owner, Discipline, Deliverable, Start Date, Finish Date, Notes.
- **FR-041**: WBS MUST auto-generate structured codes (1, 1.1, 1.2, 1.2.1, etc.) that regenerate after structural changes.
- **FR-042**: WBS Mapping tab MUST allow selecting an existing BOQ, choosing a mapping method (By Section, By CSI, By Cost Code, By Trade, By Discipline), generating a preview, and creating a WBS.
- **FR-043**: WBS Templates tab MUST support templates for: Building, Infrastructure, Landscape, Roads, Water Network, Sewer Network, Industrial, Power Plant.
- **FR-044**: WBS Templates MUST support actions: Apply Template, Export Template, Import Template, Save As Template.
- **FR-045**: WBS AI Generation tab MUST support three AI sources: BOQ Studio data, Project Documents, or both BOQ + Documents (recommended mode).
- **FR-046**: WBS AI Generation workflow MUST: select project, select BOQ, select documents, generate WBS hierarchy, accept result, create new WBS in List tab.
- **FR-047**: WBS MUST belong to a ProjectId; relationship: One Project -> Many BOQs -> Many WBS Structures.
- **FR-048**: WBS Reports MUST include: WBS Dictionary, WBS Summary, WBS Weight Report, Responsibility Matrix, Hierarchy Report.
- **FR-049**: WBS export formats MUST include: Excel, PDF, CSV, JSON.
- **FR-050**: WBS Settings MUST include default values (Default WBS Level, Default Duration, Default Weight) and behavior toggles (Auto Generate Codes, Redistribute Weights, Enable Drag Drop, Enable AI Suggestions, Use Primavera Colors).
- **FR-051**: WBS future phase MUST support versioning (revisions) and comparison between revisions showing Added, Deleted, and Modified nodes.
- **FR-052**: Projects screen MUST keep parties visible in a separate rail under the project area, including clients, main contractor, subcontractors, and consultant.
- **FR-053**: Dashboard MUST show all studio details and summary charts.
- **FR-054**: System MUST support four roles: Project Manager, Planner, Quantity Surveyor, Viewer.
- **FR-055**: Project Manager MUST be able to create, edit, approve, and archive WBS structures; import BOQ; manage all project data.
- **FR-056**: Planner MUST be able to create and edit WBS structures but NOT approve or archive them.
- **FR-057**: Quantity Surveyor MUST be able to import BOQ data and map BOQ to WBS, but NOT approve or archive WBS.
- **FR-058**: Viewer MUST be able to view dashboard, projects, BOQ, WBS, and parties but NOT create, edit, delete, approve, or archive any data.
- **FR-059**: WBS status transitions MUST follow: Draft → Under Review → Approved → Archived; plus Draft → Archived, Under Review → Draft, and Approved → Under Review as valid transitions.
- **FR-060**: System MUST enforce status transition rules and reject invalid transitions with a clear error message.
- **FR-061**: WBS AI Generation MUST use an external AI API with a configurable endpoint (e.g., OpenAI, Azure AI).
- **FR-062**: WBS AI Generation MUST degrade gracefully when the AI API is unreachable — show a clear error message and allow the user to retry or cancel.
- **FR-063**: When a user opens a WBS for editing in the Editor tab, the system MUST acquire an edit lock and prevent other users from saving changes to the same WBS.
- **FR-064**: Other users attempting to edit a locked WBS MUST see a read-only notice identifying who holds the lock.
- **FR-065**: Dashboard, Projects, BOQ, WBS, and Parties screens MUST display contextual empty-state messages with suggested next actions when no data exists.
- **FR-066**: All async operations (imports, AI generation, exports) MUST display inline loading indicators to the user.

### Key Entities

- **Project**: Central container that owns folders, parties, BOQs, WBS structures, activities, resources, and costs. Tracks metadata: progress, health, budget, dates, connected systems.
- **Party**: Organizations involved in a project (Client, Main Contractor, Subcontractor, Consultant). Includes full details and logo. Linked to one or more projects.
- **BOQ (Bill of Quantities)**: Project-specific BOQ data imported from Excel. Contains sections, items, cost codes, trade codes, CSI codes. Source for WBS generation and cost tracking.
- **WBS (Work Breakdown Structure)**: Hierarchical project structure auto-generated from BOQ, templates, AI, or manual creation. Central backbone connecting activities, resources, costs, and documents. Supports versioning and comparison.
- **WBS Node**: Individual node in a WBS hierarchy with code, name, description, level, weight, owner, discipline, deliverable, dates.
- **Activity**: Future entity linked to WBS nodes for scheduling (excavation, subbase, asphalt under Roads, etc.).
- **Resource**: Future entity linked to WBS nodes for resource allocation.
- **Cost Account**: Future entity linked to WBS nodes for cost tracking.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can complete a multi-sheet BOQ import (including auto-detection, column mapping, and merge) in under 2 minutes for a standard 5-worksheet file.
- **SC-002**: Users can create a WBS from a BOQ (mapping + preview + confirmation) in under 1 minute.
- **SC-003**: The dashboard displays all health and cost indicators for up to 20 projects within 3 seconds of page load.
- **SC-004**: Project folder auto-creation completes within 5 seconds of folder path assignment.
- **SC-005**: Auto-generated WBS codes are 100% accurate and consistent after any structural tree modification.
- **SC-006**: Users can navigate between dashboard, projects, BOQ, WBS, and parties without page reloads or data loss.
- **SC-007**: All screens (dashboard, projects, parties, BOQ, WBS) maintain visual consistency with icon usage and Fluent UI styling.
- **SC-008**: Users report at least 30% reduction in time spent on BOQ import and WBS creation tasks in a post-release survey.

## Assumptions

- Existing authentication and authorization systems will be reused without changes.
- The application is a desktop application (WPF/XAML) — all UI redesigns are XAML-only as specified.
- Mobile support is out of scope for this enhancement pass.
- Fluent UI styling refers to WPF Fluent Design System principles (icons, acrylic/glass effects, rounded corners, consistent spacing).
- Excel import supports .xlsx and .xls formats using existing Excel integration infrastructure.
- Google Maps links are stored as URL strings and opened in the default browser; no embedded map control is required.
- Large Excel files are defined as those exceeding 50MB or 100K rows.
- WBS AI generation uses an external AI API (configurable endpoint such as OpenAI or Azure AI); BOQ column mapping uses built-in heuristics (no external AI dependency).
- Primavera import is a future capability — WBS Studio supports the "Primavera" source type for future integration.
- WBS versioning and comparison are future-phase items (not in v1 scope).
- The existing project file/folder management system provides the foundation for auto-creation.
