# Feature Specification: BOQ Studio Implementation

**Feature Branch**: `005-boq-studio`

**Created**: 2026-06-03

**Status**: Draft

**Input**: docs/PHASE_3_IMPLEMENTATION_PLAN.md — Phase 3 BOQ Studio implementation plan detailing the bill of quantities workspace for planning engineers.

## User Scenarios & Testing

### User Story 1 — Import BOQ from Excel Using Phase 2 Infrastructure (Priority: P1)

A planning engineer opens the BOQ import wizard, selects an Excel workbook, maps columns to BOQ fields using Phase 2 mapping profiles, previews the assembled tree hierarchy, validates the structure, and commits the BOQ to the database.

**Why this priority**: Import is the primary data entry path; without it users cannot bring existing BOQs into the system.

**Independent Test**: Can be tested by importing a multi-sheet Excel workbook with hierarchical BOQ rows, confirming that flat rows are correctly assembled into a tree, and that the BOQ viewer displays the imported hierarchy.

**Acceptance Scenarios**:

1. **Given** a user has a valid .xlsx file with BOQ columns (Code, Description, Unit, Quantity, Rate), **When** they select the file in the import wizard and map columns to BOQ fields, **Then** the system displays a tree preview of the assembled hierarchy
2. **Given** the source data has a Level or ParentId column, **When** the tree builder runs, **Then** the hierarchy is correctly assembled with proper parent-child relationships
3. **Given** the workbook contains invalid data (missing codes, unrecognized units), **When** validation runs, **Then** the user sees errors with item-level details before commit
4. **Given** a user has a saved mapping profile, **When** they reuse it during import, **Then** columns are pre-mapped automatically

---

### User Story 2 — View BOQ as Hierarchical Tree (Priority: P1)

A planning engineer opens an existing BOQ and navigates the hierarchical tree with expand/collapse, reads section subtotals and the grand total, and inspects individual line items.

**Why this priority**: Tree viewing is the primary consumption pattern; every other workflow depends on users seeing their BOQ data.

**Independent Test**: Can be tested by creating a multi-level BOQ and verifying the tree renders with correct hierarchy, compute subtotals, and display totals accurately.

**Acceptance Scenarios**:

1. **Given** a BOQ with sections and sub-items, **When** the user opens the BOQ viewer, **Then** the tree shows all levels with expand/collapse toggles
2. **Given** a section with child items is expanded, **When** viewed, **Then** the section subtotal equals the sum of all child item amounts
3. **Given** the tree is fully expanded, **When** viewed, **Then** the grand total footer equals the sum of all root-level section amounts
4. **Given** the BOQ contains 10,000+ items, **When** the user scrolls, **Then** the UI remains responsive with no visible lag

---

### User Story 3 — Edit BOQ Items Inline (Priority: P2)

A planning engineer adjusts quantities, rates, or descriptions after import. They edit inline, add new items or sections, delete obsolete entries, and reorder items in the tree.

**Why this priority**: Editing is essential for BOQ refinement before downstream WBS and cost workflows.

**Independent Test**: Can be tested by editing individual item fields and confirming the tree, subtotals, and totals reflect changes immediately.

**Acceptance Scenarios**:

1. **Given** a user changes a quantity value on an item, **When** they confirm the edit, **Then** the item amount and all parent section subtotals update immediately
2. **Given** a user adds a new item to a section, **When** saved, **Then** the item appears in the tree and the section subtotal is recalculated
3. **Given** a user deletes a section item that has children, **When** they confirm deletion, **Then** all children are removed and subtotals update
4. **Given** a user reorders items via move up/down, **When** the operation completes, **Then** the tree reflects the new order

---

### User Story 4 — Validate BOQ Integrity (Priority: P2)

A planning engineer runs validation on a BOQ to check for structural issues before using the data in downstream workflows.

**Why this priority**: Validation ensures data quality; a BOQ with errors propagates issues to WBS and cost phases.

**Independent Test**: Can be tested by creating a BOQ with intentional errors (duplicate codes, orphan items, invalid units) and confirming the validation engine catches all issues.

**Acceptance Scenarios**:

1. **Given** a BOQ has duplicate item codes, **When** validation runs, **Then** each duplicate is reported with the item reference and code value
2. **Given** a BOQ has an item whose ParentId references a nonexistent item, **When** validation runs, **Then** the orphan item is reported
3. **Given** a BOQ has items with unrecognized units, **When** validation runs, **Then** warnings are displayed for each unrecognized unit
4. **Given** a circular parent reference exists, **When** validation runs, **Then** the cycle is detected and reported

---

### User Story 5 — Classify BOQ Items (Priority: P3)

A cost engineer manages classification taxonomies and assigns codes to BOQ items for structured cost reporting and later WBS mapping.

**Why this priority**: Classification enables cost analysis but is not required for basic BOQ import, viewing, and editing.

**Independent Test**: Can be tested by creating a classification taxonomy, assigning codes to items, and filtering items by classification code.

**Acceptance Scenarios**:

1. **Given** a user has defined classification codes in a taxonomy, **When** they select multiple items and assign a code, **Then** all selected items are updated with that classification code
2. **Given** a user filters items by classification code, **When** the filter is applied, **Then** only items with that classification are displayed
3. **Given** a user exports a classification scheme, **When** they import it into another project, **Then** the codes are available in the target project

---

### User Story 6 — Manage and Use BOQ Libraries (Priority: P3)

A planning engineer creates a library of standard BOQ items with default rates and inserts them into active BOQs.

**Why this priority**: Libraries improve efficiency for recurring items but are not required for basic BOQ management.

**Independent Test**: Can be tested by creating a library with standard items, inserting them into a BOQ, and verifying the inserted items appear with correct defaults.

**Acceptance Scenarios**:

1. **Given** a user has a library with standard rate-book items, **When** they select items and insert into the BOQ at a chosen position, **Then** the items are added with their default rates and descriptions
2. **Given** a user creates a new library item, **When** saved, **Then** the item is available for insertion into any BOQ
3. **Given** a user imports a library from Excel, **When** the import completes, **Then** the library items are available immediately

---

### User Story 7 — Generate and Export BOQ Reports (Priority: P3)

A planning engineer generates a formatted BOQ report and exports it to Excel or PDF for client submission.

**Why this priority**: Reporting is important for stakeholder communication but follows initial BOQ setup.

**Independent Test**: Can be tested by generating both summary and itemized reports and confirming the exported Excel and PDF files contain correct data.

**Acceptance Scenarios**:

1. **Given** a BOQ with items and sections, **When** the user generates a summary report, **Then** the report shows section totals, item counts, and the grand total
2. **Given** a user exports to Excel using Phase 2 WorkbookWriter, **When** the export completes, **Then** the Excel file contains the full BOQ with correct structure and formatting
3. **Given** a user exports to PDF, **When** the export completes, **Then** the PDF contains a formatted BOQ suitable for printing
4. **Given** the application is in Arabic, **When** a report is generated, **Then** the report displays Arabic text with correct RTL layout

### Edge Cases

- What happens when a BOQ has zero items after import?
- How does the system handle a flat (non-hierarchical) import with no Level or ParentId column?
- What happens when an imported code exceeds the maximum string length?
- How does the system handle negative quantities or rates during import or manual entry?
- Cascade delete: deleting a parent section item automatically removes all its descendants
- How does the system detect and handle circular parent references during tree editing?
- What happens when a library item with an unrecognized unit is inserted into a BOQ?
- How does the system handle extremely deep hierarchies (10+ levels) in the virtualized tree view?
- Concurrent edits on the same BOQ are prevented via optimistic locking — the save operation rejects stale data and prompts the user to reload and retry
- Phase 2 service failures (Excel reader, mapping profiles) are handled with graceful degradation — user-friendly error message, full details logged, retry or CSV fallback offered

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow users to create BOQs associated with a project, with name, description, currency, and initial Draft status
- **FR-002**: System MUST support a self-referencing tree hierarchy for BOQ items with unlimited nesting levels
- **FR-003**: System MUST compute section subtotals and a grand total automatically in the application layer, not the database
- **FR-004**: System MUST display the BOQ tree in a virtualized view supporting expand/collapse at all levels
- **FR-005**: System MUST support importing BOQ data from .xlsx and .xlsm files via Phase 2 Excel services, and from .csv files via a dedicated standalone CSV reader path
- **FR-006**: System MUST reuse Phase 2 IWorkbookReader and IMappingProfileService for workbook reading and column mapping during BOQ import
- **FR-007**: System MUST assemble flat imported rows into a hierarchical tree using Code prefix matching, Level column, or explicit ParentId column
- **FR-008**: System MUST validate BOQ data for code uniqueness, parent existence, circular references, unit recognition, and quantity/rate range
- **FR-009**: System MUST display validation errors and warnings with item-level details and inline fix suggestions
- **FR-010**: System MUST support inline editing of quantity, rate, description, and unit on individual items
- **FR-011**: System MUST support adding new items/sections, deleting items (with child confirmation), and reordering items in the tree
- **FR-012**: System MUST support user-defined classification taxonomies as hierarchical code structures
- **FR-013**: System MUST support assigning classification codes to items individually and in bulk
- **FR-014**: System MUST support filtering BOQ items by classification code
- **FR-015**: System MUST support BOQ libraries with CRUD operations for libraries and their items
- **FR-016**: System MUST support inserting library items into an active BOQ at a selected position
- **FR-017**: System MUST generate summary reports (section totals, grand total, item counts) and itemized reports (full item list)
- **FR-018**: System MUST export BOQ reports to Excel using Phase 2 WorkbookWriter and to PDF
- **FR-019**: System MUST support both English and Arabic UI with runtime switching and correct RTL behavior for all BOQ screens
- **FR-020**: System MUST handle BOQs with 10,000+ items without UI lag or degraded performance

### Key Entities

- **Boq**: Bill of quantities header entity. Associated with a project (via ProjectId). Has a status lifecycle (Draft → Final ⇄ Revised → Approved), currency, revision number, and computed total amount. Tracks import source and timestamp. Approved is terminal — no further edits allowed.
- **BoqItem**: Self-referencing tree node within a BOQ. Contains code, description, unit, quantity, rate, and computed amount. Has a level, sort order, and item type (Section, Item, SubItem). Optionally references a cost code and classification code. Links to its parent item (nullable) and children collection.
- **BoqLibrary**: Named collection of reusable item templates. Can be system-standard or user-defined. Contains a collection of BoqLibraryItems.
- **BoqLibraryItem**: A reusable item template within a library. Has code, description, unit, default rate, category, and tags for discoverability.
- **BoqClassification**: A code within a user-defined classification taxonomy. Supports hierarchical parent-child relationships for tree taxonomies. Can be project-specific or global.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can import a 500-row BOQ from Excel, including column mapping and tree preview, in under 2 minutes
- **SC-002**: The virtualized tree view renders 10,000 items in under 2 seconds and scrolling remains at 60 fps
- **SC-003**: Section subtotals and grand total are mathematically accurate to within 0.01 monetary units for all BOQ sizes
- **SC-004**: Validation detects 100% of duplicate codes, missing parents, circular references, and unrecognized units in a BOQ
- **SC-005**: Users can complete inline editing of 100 items (mixture of quantity, rate, description changes) in under 5 minutes
- **SC-006**: Summary and itemized reports export to Excel and PDF with correct formatting, column alignment, and numerical accuracy
- **SC-007**: All BOQ screens function identically in English and Arabic, with correct RTL layout and no text truncation or overlap
- **SC-008**: Library items inserted into a BOQ preserve their default rate and description at the point of insertion

## Clarifications

### Session 2026-06-03

- Q: When a parent section item with children is deleted, should children be cascade-deleted, or should deletion be blocked? → A: Cascade delete — removing a parent automatically deletes all descendants
- Q: How should concurrent edits on the same BOQ be handled? → A: Single-user with optimistic locking; version check on save rejects stale edits
- Q: What are the allowed BoqStatus transitions? → A: Linear forward with a Revised rollback — Draft → Final ⇄ Revised → Approved (Approved is terminal)
- Q: How should Phase 2 Excel service failures be handled during import? → A: Graceful degradation — user-friendly error, full logging, retry or CSV fallback
- Q: Should CSV import be a standalone path or only an Excel-failure fallback? → A: Dual path — primary Excel via Phase 2; dedicated standalone CSV reader for direct CSV imports

## Assumptions

- Planning engineers are domain experts familiar with BOQ terminology and structure
- Most BOQs are fewer than 5,000 items; 10,000+ items is the upper bound for performance targets
- The hierarchical structure can be inferred from Code patterns (dot-separated segments) or explicit Level/ParentId columns
- BOQ amounts are in a single currency per BOQ with no multi-currency items
- Classification taxonomies are user-defined rather than sourced from an external standard initially
- Phase 2 Excel Engine (IWorkbookReader, IMappingProfileService, WorkbookWriter) is stable and available for reuse
- Phase 1 Project entities already exist for BOQ-to-project association
- The user works in a single-user context with optimistic locking — one active editing session per BOQ; a version check on save rejects stale edits if another session modified the same BOQ
- Clean Architecture and MVVM patterns are already established from Phases 0/1/2 and must be maintained
