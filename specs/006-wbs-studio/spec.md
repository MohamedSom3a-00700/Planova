# Feature Specification: WBS Studio

**Feature Branch**: `006-wbs-studio`

**Created**: 2026-06-04

**Status**: Draft

**Input**: User description: "Planova\docs\PHASE_4_IMPLEMENTATION_PLAN.md"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and Manage a WBS (Priority: P1)

A project planner opens the WBS Studio and creates a new Work Breakdown Structure for their project. They can start from scratch, or use an existing BOQ, a template, or an AI suggestion as the starting point. They then manage the WBS tree by adding, editing, deleting, and reordering work packages at different hierarchy levels.

**Why this priority**: Creating and managing the WBS is the core value of the feature — without it, nothing else works.

**Independent Test**: Can be fully tested by creating a new WBS via the "Manual" option, adding 5+ items at various levels, editing item properties, deleting an item with children (verifying cascade warning), and reordering items. Delivers a functional WBS tree with persisted hierarchy.

**Acceptance Scenarios**:

1. **Given** the user is on the WBS list screen, **When** they click "Create WBS" and select "Manual", **Then** a new WBS with a single root item is created and the tree viewer opens
2. **Given** the tree viewer is open, **When** the user adds a child item to a selected node, **Then** the new item appears indented under the parent with the correct level
3. **Given** the tree viewer is open, **When** the user edits an item's name, description, weight, dates, or assignee, **Then** changes are persisted and immediately visible
4. **Given** a WBS item with children, **When** the user deletes it, **Then** they see a confirmation dialog warning about cascade deletion
5. **Given** two sibling items, **When** the user moves one up or down, **Then** the sort order updates and the tree refreshes

---

### User Story 2 - View and Navigate the WBS Hierarchy (Priority: P1)

A planner views their WBS as a structured tree with expand/collapse at every level. Each item is color-coded by level type (Summary, Control Account, Work Package, Planning Package). Weight percentages are visualized alongside each item to give an immediate sense of workload distribution.

**Why this priority**: Reading and understanding the WBS structure is the most frequent daily activity for planners.

**Independent Test**: Can be fully tested by opening an existing WBS with 10+ items across 3+ levels, expanding/collapsing nodes, verifying color coding per level type, and confirming weight bars display correctly. Delivers a browsable, readable WBS tree.

**Acceptance Scenarios**:

1. **Given** a WBS with at least 3 levels of hierarchy, **When** the user opens it, **Then** all items are displayed in a tree with proper indentation and connecting lines
2. **Given** a collapsed parent node, **When** the user clicks expand, **Then** all direct children are revealed
3. **Given** an expanded node, **When** the user clicks collapse, **Then** all descendants are hidden
4. **Given** items of different WbsLevelType, **When** the tree renders, **Then** each type has a distinct color (Summary, ControlAccount, WorkPackage, PlanningPackage)
5. **Given** items with assigned weights, **When** the tree renders, **Then** a proportional weight bar is displayed alongside each item

---

### User Story 3 - Map BOQ to WBS (Priority: P2)

A project estimator selects a previously created BOQ from Phase 3 and uses the mapping wizard to auto-generate a WBS structure. They choose from three mapping strategies (one-to-one, grouped, or custom), preview the result, make adjustments, and commit the final WBS.

**Why this priority**: BOQ-to-WBS mapping is the primary integration path between Phase 3 and Phase 4, enabling a streamlined workflow.

**Independent Test**: Can be fully tested by selecting a BOQ with 5+ items, applying the "Grouped" mapping strategy, previewing the generated tree, adjusting the names of two nodes, and committing. Delivers a WBS with traceable source BOQ references on each item.

**Acceptance Scenarios**:

1. **Given** a project with at least one BOQ, **When** the user creates a WBS via "From BOQ", **Then** they see a step to select which BOQ to use with a preview of its tree
2. **Given** a selected BOQ, **When** the user chooses a mapping strategy, **Then** the system generates a suggested WBS tree based on that strategy
3. **Given** the generated preview, **When** the user adjusts item names, levels, or WbsLevelType, **Then** changes are reflected in the preview
4. **Given** a satisfactory preview, **When** the user clicks "Commit", **Then** the WBS is persisted with each item linked to its source BOQ item via traceability
5. **Given** a committed WBS from BOQ, **When** the user inspects a WBS item, **Then** they can see which BOQ item it originated from

---

### User Story 4 - Apply and Manage WBS Templates (Priority: P2)

A project planner applies a reusable WBS template to quickly generate a standard structure for a new project. They can browse templates by industry category, preview the template structure, apply it to create a new WBS, and customize the result.

**Why this priority**: Templates accelerate project setup and enforce organizational standards, delivering significant time savings.

**Independent Test**: Can be fully tested by browsing the template library (including seeded templates like "Building Construction"), previewing a template's tree, applying it to a project, and verifying the resulting WBS has the correct hierarchy. Delivers a complete WBS from a template in a single operation.

**Acceptance Scenarios**:

1. **Given** the template manager, **When** the user filters by "Building Construction" category, **Then** matching templates are shown
2. **Given** a selected template, **When** the user previews it, **Then** the full template hierarchy is shown as a tree
3. **Given** a template preview, **When** the user clicks "Apply to Project", **Then** a new WBS is created with all template items deep-copied
4. **Given** an existing WBS with a good structure, **When** the user saves it as a template, **Then** the template is available for reuse
5. **Given** a user-created template, **When** the user exports it as JSON, **Then** a valid JSON file is downloaded

---

### User Story 5 - AI-Assisted WBS Generation (Priority: P3)

A project manager with limited domain knowledge types a brief project scope description (e.g., "Build a 5-story office building with underground parking") and the system suggests a WBS structure using AI. The user reviews, modifies, accepts, or regenerates the suggestion.

**Why this priority**: AI generation is a productivity booster but not essential — users can always fall back to manual, BOQ, or template methods.

**Independent Test**: Can be fully tested by entering a project scope in the AI panel, clicking "Generate", receiving a suggested WBS tree, modifying two nodes, and accepting the result. Delivers a WBS starter that the user can further edit.

**Acceptance Scenarios**:

1. **Given** the AI generation panel, **When** the user enters a project scope of 2-3 sentences, **Then** a "Generate" button is enabled
2. **Given** a project scope entered, **When** the user clicks "Generate", **Then** a suggested WBS tree is displayed within a reasonable time with progress indication
3. **Given** a generated suggestion, **When** the user modifies item names or adds new items, **Then** the tree updates
4. **Given** a suggestion the user is not satisfied with, **When** they click "Regenerate", **Then** a new suggestion replaces the previous one
5. **Given** a satisfactory suggestion, **When** the user clicks "Accept", **Then** a new WBS is created from the suggestion

---

### User Story 6 - View and Export WBS Reports (Priority: P3)

A project planner generates a WBS Summary Report (hierarchical view with weights and item counts) or a WBS Dictionary (full item descriptions, responsibilities, and source BOQ references). They export these reports to Excel or PDF for sharing with stakeholders.

**Why this priority**: Reports are important for stakeholder communication but the core value is in creating and managing the WBS itself.

**Independent Test**: Can be fully tested by opening a WBS with 5+ items, generating a Summary Report, verifying the hierarchical layout, and exporting to Excel and PDF. Delivers a formatted, shareable document.

**Acceptance Scenarios**:

1. **Given** an open WBS, **When** the user switches to the Reports tab, **Then** both Summary Report and Dictionary options are available
2. **Given** the Summary Report, **When** the user views it, **Then** the hierarchy, levels, weights, and item counts are displayed
3. **Given** the WBS Dictionary, **When** the user views it, **Then** full item descriptions, responsibilities, deliverables, and source BOQ references are shown
4. **Given** any report view, **When** the user clicks "Export to Excel", **Then** a valid Excel file is saved
5. **Given** any report view, **When** the user clicks "Export to PDF", **Then** a valid PDF file is saved

---

### Edge Cases

- What happens when the user tries to add a WorkPackage as a child of another WorkPackage? WorkPackages should not have children — the system should either prevent this or auto-upgrade the parent to a ControlAccount
- How does the system handle deleting a parent WBS item that has children with assigned weights? All child weights should be reclaimed and redistributed among remaining siblings
- What happens when a circular reference is detected (item A is parent of B, B is parent of A)? The system should reject the operation with a clear error message
- How does the BOQ-to-WBS mapping handle BOQ items that were already mapped in a previous WBS? Mappings should be allowed from a single BOQ to multiple WBS versions without conflict
- What happens when the AI provider is unavailable? The generation panel should show a clear error message and suggest manual alternatives
- How does weight redistribution work when an item with 30% weight is deleted leaving a sibling with 70%? The sibling's weight should remain at 100% (or redistribute proportionally across all remaining siblings)
- What happens when a template with 200+ items is applied? The system should show progress indication during the deep copy operation

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users to create a new WBS for a project with source selection (Manual, From BOQ, From Template, AI Generated)
- **FR-002**: System MUST display the WBS as a hierarchical tree with expand/collapse at all levels
- **FR-003**: System MUST color-code each WBS item by its level type (Summary, Control Account, Work Package, Planning Package)
- **FR-004**: System MUST display weight percentage bars alongside WBS items proportional to their assigned weight
- **FR-005**: System MUST allow users to add new WBS items at any level with level type selection
- **FR-006**: System MUST allow users to edit existing WBS item properties (name, description, weight, dates, assigned to, deliverable, notes)
- **FR-007**: System MUST allow users to delete WBS items with child cascade confirmation
- **FR-008**: System MUST allow users to reorder WBS items (move up/down) within their sibling group
- **FR-009**: System MUST auto-redistribute weights among siblings when an item is added or deleted
- **FR-010**: System MUST provide a BOQ-to-WBS mapping wizard with at least three strategies: One-to-One, Grouped, and Custom
- **FR-011**: System MUST persist SourceBoqItemId on each mapped WBS item for traceability to the originating BOQ item
- **FR-012**: System MUST allow users to preview the generated WBS tree before committing a BOQ mapping
- **FR-013**: System MUST include a WBS template manager with CRUD operations and category/industry filtering
- **FR-014**: System MUST seed at least four standard WBS templates (Building Construction, Infrastructure, Industrial, Oil & Gas)
- **FR-015**: System MUST allow users to apply a template to create a new WBS with deep-copied template items
- **FR-016**: System MUST allow users to create custom templates from existing WBS structures
- **FR-017**: System MUST allow users to import and export templates as JSON
- **FR-018**: System MUST provide an AI WBS generation panel where users enter a project scope description and receive a suggested WBS tree
- **FR-019**: System MUST let users accept, modify, or regenerate AI-suggested WBS structures
- **FR-020**: System MUST show progress indication during AI generation
- **FR-021**: System MUST gracefully handle AI provider unavailability with a clear error message
- **FR-022**: System MUST generate a WBS Summary Report showing hierarchy, levels, weights, and item counts
- **FR-023**: System MUST generate a WBS Dictionary showing full item descriptions, responsibilities, deliverables, and source BOQ references
- **FR-024**: System MUST export both WBS reports to Excel format
- **FR-025**: System MUST export both WBS reports to PDF format
- **FR-026**: System MUST support right-to-left (RTL) layout for Arabic language users in all WBS screens
- **FR-027**: System MUST auto-generate two codes for each WBS item: a numeric code derived from tree position (e.g., 1, 1.1, 1.1.1) and an alpha short code derived from the item name (minimum 3 letters, auto-updated on rename)
- **FR-028**: System MUST validate WBS structural rules: unique numeric and alpha codes within a WBS, parent existence, no circular references, valid date ranges
- **FR-029**: System MUST ensure total weight at each level does not exceed 100%
- **FR-030**: System MUST allow users to perform bulk updates (dates, assignee) across a selection of WBS items
- **FR-031**: System MUST allow users to search WBS by name and filter by status and source on the WBS list screen

### Key Entities *(include if feature involves data)*

- **Wbs**: A named work breakdown structure associated with a project, tracking its status (Draft/Final/Revised/Approved), source (Manual/FromBOQ/FromTemplate/AIGenerated), and revision number. Status follows forward-only transitions: Draft → Final → Approved; Approved can return to Revised (cycles back to Draft). Links to the source BOQ when generated from one.
- **WbsItem**: A single node in the WBS hierarchy with two auto-generated codes (numeric from tree position, alpha short code from item name), level type, weight allocation, planned dates, responsible party, and deliverable description. Self-references to form a tree via ParentId. Optionally links back to a source BOQ item for traceability.
- **WbsTemplate**: A reusable WBS structure blueprint categorized by industry and project type. May be system-seeded or user-created. Contains a version number and tags.
- **WbsTemplateItem**: A reusable node within a WBS template, mirroring WbsItem structure with optional default duration and typical weight values. Self-references to form a template tree.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create a 50+ item WBS from a template in under 10 seconds
- **SC-002**: Users can map a 200-item BOQ to a WBS structure and commit in under 5 minutes (including preview and adjustments)
- **SC-003**: The WBS tree viewer loads and renders 1,000+ items without noticeable lag (under 2 seconds initial render)
- **SC-004**: Users can complete the full workflow (create WBS → add items → map BOQ → apply template → generate report → export) without requiring technical support
- **SC-005**: Expand/collapse operations on a WBS with 1,000+ items respond in under 200ms
- **SC-006**: AI-generated WBS suggestions are returned within 30 seconds for typical project scope descriptions
- **SC-007**: WBS reports (summary and dictionary) for a 500-item WBS are generated in under 5 seconds
- **SC-008**: Weight auto-redistribution recalculates all sibling weights in under 500ms when an item is added or deleted
- **SC-009**: Users can find and open any WBS from the list view in under 3 clicks
- **SC-010**: All WBS screens function correctly in both English and Arabic without layout issues

## Clarifications

### Session 2026-06-04

- Q: What access control model applies to WBS data beyond project-level visibility? → A: Same as project-level access — no additional WBS-specific permission layer; project access determines WBS access.
- Q: How are WBS item codes assigned — auto-generated or manual? → A: Two code systems — (1) numeric code auto-generated based on tree position/level (e.g., 1, 1.1, 1.1.1), (2) alpha short code auto-generated from the WBS item name (min 3 letters). Both read-only in UI, updated on rename/reorder.
- Q: What WBS status transitions are allowed? → A: Forward-only with Revised — Draft → Final → Approved; Approved can revert to Revised (which cycles back to Draft for re-approval); no bypassing steps.

## Assumptions

- Users have completed Phase 3 (BOQ Studio) and have at least one BOQ available for mapping scenarios
- The application already has a project management module; projects exist before WBS creation
- The AI generation feature requires an available AI provider; if unavailable, manual/BOQ/template methods serve as fallback
- WBS templates are seeded as part of database initialization, not requiring manual setup
- Weight allocation uses percentage-based distribution across siblings; total weight at each level sums to 100%
- Users access the WBS Studio through the existing navigation rail and multi-tab workspace
- The WBS data model is designed for downstream consumption by future phases (Activity, Resource, Cost)
- Excel export uses the existing workbook generation infrastructure from Phase 2
- PDF export uses the existing PDF generation infrastructure
- Standard reporting periods and fiscal calendar conventions (Gregorian calendar, weekly/monthly periods) are assumed
- WBS access control mirrors project-level access — no additional WBS-specific permission layer is required
