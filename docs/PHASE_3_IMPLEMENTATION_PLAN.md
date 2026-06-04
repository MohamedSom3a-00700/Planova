# Planova Phase 3 Implementation Plan

**Phase**: 3 — BOQ Studio

**Date**: 2026-06-03

**Source of Truth**: [docs/01-PRODUCT_VISION.md](./01-PRODUCT_VISION.md),
[docs/02-MASTER_ROADMAP.md](./02-MASTER_ROADMAP.md),
[docs/04-SYSTEM_ARCHITECTURE.md](./04-SYSTEM_ARCHITECTURE.md),
[docs/06-MODULE_CATALOG.md](./06-MODULE_CATALOG.md),
[docs/11-UI_UX_DESIGN_SYSTEM.md](./11-UI_UX_DESIGN_SYSTEM.md),
[docs/PLANOVA_CONSTITUTION_DRAFT.md](./PLANOVA_CONSTITUTION_DRAFT.md),
[specs/005-boq-studio/spec.md](../specs/005-boq-studio/spec.md)

## Summary

Phase 3 delivers the BOQ (Bill of Quantities) Studio — the entry point of the Planova project controls workflow. This phase turns the placeholder BOQ navigation item into a full-featured workspace where planning engineers import, validate, classify, and manage bills of quantities.

The BOQ Studio is the first specialized studio in the product roadmap and the natural next step after Phase 1 (Project Management) and Phase 2 (Excel Engine). It directly leverages the Excel integration infrastructure from Phase 2 for BOQ import and extends it with BOQ-specific domain models, validation, classification, and library management.

This phase does not cover WBS, activities, resources, cost loading, or scheduling workflows. Those are separate downstream phases that will consume the structured BOQ output.

## Phase 3 Objectives

1. Introduce the BOQ domain model (Boq, BoqItem, BoqLibrary, BoqClassification).
2. Build a tree-structured BOQ viewer and editor with section subtotals and hierarchy.
3. Implement a BOQ import wizard that reuses Phase 2 Excel infrastructure.
4. Create a validation engine specific to BOQ structure and content rules.
5. Implement BOQ classification with user-defined cost code taxonomies.
6. Add BOQ libraries for reusable item templates and rate books.
7. Deliver BOQ summary reports with Excel/PDF export.
8. Preserve Clean Architecture, MVVM, localization, and theme consistency.
9. Ensure the BOQ output is structured for consumption by Phase 4 (WBS Studio).

## Phase 3 Scope

### In Scope

- Boq domain entity with project association
- BoqItem domain entity with self-referencing tree hierarchy
- BoqLibrary and BoqLibraryItem domain entities
- BoqClassification taxonomy entity
- BOQ tree viewer with expand/collapse, subtotals, and totals
- BOQ inline editor (add, edit, delete, reorder items)
- BOQ import wizard leveraging Phase 2 Excel engine
- BOQ validation engine (code uniqueness, parent existence, unit consistency)
- BOQ classification management (cost codes, bulk assign)
- BOQ libraries (CRUD, insert library items into BOQ)
- BOQ summary and itemized reports with Excel/PDF export
- Localization (English and Arabic) for all BOQ screens
- Unit tests for domain, services, and validation
- Integration tests for import workflow

### Out of Scope

- WBS generation or mapping (Phase 4)
- Activity generation (Phase 5)
- Resource loading (Phase 6)
- Cost loading or cash flow (Phase 7)
- AI-powered BOQ classification (deferred to AI Copilot phase)
- Primavera integration
- Multi-user collaboration on BOQs
- Workflow engine or approval routing

## Non-Negotiable Constraints

- Clean Architecture must remain intact.
- MVVM must remain the UI pattern.
- Localization must support English and Arabic.
- RTL behavior must remain correct.
- Theme support must remain consistent with Phase 0/1/2.
- Phase 2 Excel module must be reused (not duplicated or bypassed).
- No direct vendor lock-in for AI or automation.
- Self-referencing tree must support large BOQs (10,000+ items) with virtualized UI.
- The BOQ data model must leave room for later WBS and cost integration.

## Phase 3 Product Shape

Phase 3 should feel like the first specialized engineering workspace in Planova.

The user should be able to:

- create a new BOQ and associate it with a project
- import a BOQ from Excel using the Phase 2 import infrastructure
- view the BOQ as a hierarchical tree with expand/collapse
- see section subtotals and a grand total
- edit individual BOQ items (quantity, rate, description)
- add new items and sections to the tree
- validate the BOQ for structural and data integrity
- assign classification codes to items
- manage reusable BOQ item libraries
- generate a formatted BOQ report (summary or itemized)
- export the BOQ to Excel or PDF

## Target Solution Impact

Phase 3 extends the foundation established in Phase 0, 1, and 2.

Expected additions:

- new Domain entities and value objects in Planova.Domain
- new Application services for BOQ use cases
- new Infrastructure/Persistence mappings and migrations
- new UI modules for BOQ Studio (viewer, editor, import, classification, libraries, reports)
- expanded localization resources for BOQ terminology

## Primary Domain Model

### Boq

Represents a bill of quantities associated with a project.

Properties:

- Id (Guid)
- ProjectId (Guid, FK -> Projects)
- Name
- Description
- Revision (int)
- Currency (string, e.g. USD, EUR, EGP)
- TotalAmount (decimal, computed)
- Status (BoqStatus: Draft, Final, Revised, Approved)
- ImportedAt (DateTime?)
- ImportSource (string? — filename or source reference)
- Notes
- CreatedAt
- UpdatedAt

### BoqItem

Represents a single line item in the BOQ hierarchy.

Properties:

- Id (Guid)
- BoqId (Guid, FK -> Boqs)
- ParentId (Guid?, nullable, self-referencing FK -> BoqItem)
- Code (string — item code or reference number)
- Description (string)
- Unit (string — EA, LS, M, M2, M3, KG, TON, HR, DAY, WK, MONTH, etc.)
- Quantity (decimal)
- Rate (decimal)
- Amount (decimal, computed = Quantity * Rate)
- Level (int — 0 = root section, 1 = section, 2 = item, etc.)
- SortOrder (int)
- ItemType (BoqItemType: Section, Item, SubItem)
- CostCode (string?)
- ClassificationCode (string?)
- Notes (string?)
- CreatedAt
- UpdatedAt

Navigation:

- Children (ICollection<BoqItem> — self-referencing tree)
- Parent (BoqItem?)
- Boq (Boq)

### BoqLibrary

Represents a named library of reusable items/rates.

Properties:

- Id (Guid)
- Name
- Description
- Category (string?)
- IsStandard (bool — system vs user-created)
- CreatedAt
- UpdatedAt

Navigation:

- Items (ICollection<BoqLibraryItem>)

### BoqLibraryItem

A reusable item template from a library.

Properties:

- Id (Guid)
- LibraryId (Guid, FK -> BoqLibrary)
- Code (string)
- Description (string)
- Unit (string)
- DefaultRate (decimal?)
- Category (string?)
- Tags (string? — JSON array)
- CreatedAt
- UpdatedAt

### BoqClassification

A classification code within a taxonomy.

Properties:

- Id (Guid)
- ProjectId (Guid?, FK -> Projects — project-specific or global)
- ParentId (Guid?, self-referencing FK for hierarchical taxonomies)
- Code (string)
- Name (string)
- Description (string?)
- SortOrder (int)
- CreatedAt
- UpdatedAt

### Value Objects

- **BoqStatus** — Draft, Final, Revised, Approved. Includes a transition map.
- **BoqItemType** — Section, Item, SubItem.
- **Unit** — Enum with supported units (EA, LS, M, M2, M3, KG, TON, HR, DAY, WK, MONTH, LUMP_SUM, etc.)

## Screen Plan

### 1. BOQ List Workspace

- BOQ list view with project filter
- Search by name, filter by status
- Create new BOQ (with project selection)
- Open, edit, delete actions
- Summary cards (total BOQs, by status, total value)

### 2. BOQ Viewer

- Tree view with hierarchical expand/collapse
- Columns: Code, Description, Unit, Quantity, Rate, Amount
- Section subtotals at each level
- Grand total footer
- Tab-based: Viewer, Editor, Classification, Reports

### 3. BOQ Editor

- Inline editing of quantity, rate, description, unit
- Add new item / section (insert at level)
- Delete item with child confirmation
- Reorder (move up/down)
- Bulk update quantities/rates
- Undo/redo support for edit operations

### 4. BOQ Import Wizard

4-step workflow:

1. **Select File** — Choose Excel file, preview worksheets (reuse Phase 2 WorkbookBrowser)
2. **Map Columns** — Map Excel columns to BOQ fields (reuse Phase 2 mapping profiles)
3. **Preview Tree** — Preview the generated BOQ tree with validation results
4. **Validate & Commit** — Run validation, show errors/warnings, commit to database

Supports CSV import with a dedicated CSV reader path.

### 5. BOQ Validation Panel

- Run validation on demand
- Error list with item reference and message
- Warning list for non-blocking issues
- Fix suggestions inline
- Validation rules:
  - Code uniqueness within BOQ
  - Parent item exists
  - Quantity > 0 (warning for zero)
  - Rate > 0 (warning for zero)
  - Unit is recognized
  - No circular parent references
  - Amount matches Qty * Rate

### 6. BOQ Classification Workspace

- Manage classification taxonomies (add/edit/delete codes)
- Tree view of classification codes
- Assign classification to BOQ items (single or bulk)
- Filter BOQ items by classification code
- Import/export classification schemes

### 7. BOQ Libraries Workspace

- List/CRUD libraries
- Library item management (add/edit/delete items)
- Insert library items into active BOQ
- Rate book view (standard rates by unit)
- Import library from Excel

### 8. BOQ Reports

- BOQ Summary Report — totals by section, grand total, item count
- Itemized BOQ Report — full item list with all columns
- Export to Excel (use Phase 2 WorkbookWriter)
- Export to PDF (use QuestPDF)
- Print preview

## Implementation Order

### 1. Domain Model and Contracts

- Boq entity
- BoqItem entity (self-referencing)
- BoqLibrary entity
- BoqLibraryItem entity
- BoqClassification entity
- BoqStatus value object with transition rules
- BoqItemType enum
- Unit enum
- Repository/service interfaces

### 2. Persistence Schema and Mappings

- EF Core entity configurations for all BOQ entities
- Relationships: Boq -> Project, BoqItem -> Boq, BoqItem self-reference
- Indexes on Code, BoqId, ParentId, CostCode
- Migration for BOQ tables
- Seed data for standard units

### 3. Application Services

- IBoqService — CRUD, tree queries, totals computation
- IBoqItemService — CRUD for items, reorder, bulk update
- IBoqImportService — Excel/CSV import, flat-row-to-tree builder
- IBoqValidationService — structural and data validation
- IBoqClassificationService — taxonomy management, code assignment
- IBoqLibraryService — library CRUD, insert items into BOQ
- IBoqReportService — summary and itemized report queries

### 4. BOQ Import Service

- Adapter over Phase 2 IWorkbookReader
- Flat-row-to-tree builder (using Code/Level/ParentId columns)
- Mapping profile integration (reuse Phase 2 IMappingProfileService)
- CSV reader for non-Excel imports
- Batch commit with progress reporting

### 5. BOQ Validation Engine

- Required fields (Code, Description, Unit)
- Code uniqueness within a BOQ
- Parent existence check
- Circular reference detection
- Quantity and rate range validation
- Unit recognition validation
- Item type consistency (Section cannot have rate)
- Configurable validation rules

### 6. BOQ Classification Engine

- Taxonomy tree management
- Bulk assign/remove classification codes
- Filter by classification
- Import/export classification schemes

### 7. BOQ Libraries

- Standard libraries (seeded)
- User-defined libraries
- Library-to-BOQ item insertion
- Rate book management

### 8. UI — BOQ List and Viewer

- BOQ list view with project context
- Tree view with virtualization (VirtualizingTreeView or equivalent)
- Expand/collapse at all levels
- Section subtotals, grand total
- Right-click context menu

### 9. UI — BOQ Import Wizard

- Reuse Excel Studio components
- Add BOQ-specific preview step (tree hierarchy)
- Validation results display
- Import progress and completion

### 10. UI — BOQ Editor

- Inline editing via DataGrid or TreeList
- Add/insert item at level
- Delete with confirmation
- Reorder via drag or move buttons
- Bulk edit dialog for quantities/rates

### 11. UI — Classification and Libraries

- Classification taxonomy tree management
- Item classification assignment
- Library CRUD views
- Insert library items into BOQ

### 12. UI — BOQ Reports

- Summary and itemized report views
- Excel export via Phase 2 WorkbookWriter
- PDF export via QuestPDF
- Print preview

### 13. Localization and RTL Coverage

- English and Arabic resources for all BOQ screens
- Terms: BOQ, item, section, subtotal, unit, quantity, rate, amount, classification, library
- RTL verification for tree views and data grids

### 14. Validation

- Create/edit BOQ smoke tests
- Import workflow end-to-end tests
- Validation engine tests
- Tree rendering tests
- Report generation tests
- Localization correctness tests
- Persistence round-trip tests

## Technical Decisions

### Tree Data Structure

Use a self-referencing table (ParentId) in SQLite with a recursive CTE for tree queries. In-memory tree building from flat lists for UI consumption. No closure table or nested sets — keep the model simple for the initial phase.

### Tree UI Control

Use a custom VirtualizingTreeView or Fluent UI TreeView. If no suitable Fluent UI control exists, implement a lightweight tree using nested ItemsControls with virtualization at the outer level.

### BOQ Tree Computation

Total amounts are computed recursively: leaf items have Amount = Qty * Rate, parent items aggregate children amounts. Computation happens in the Application layer, not in the database.

### Import Architecture

The BOQ import service wraps Phase 2 IWorkbookReader. A BOQ-specific row mapper converts flat Excel rows into a flat list of pending BoqItem records. A tree builder then assembles the hierarchy using Code/Level/ParentId heuristics. Validation runs on the assembled tree before commit.

### Classification

Classification is a many-to-one relationship (many items can share one classification code). Codes are stored as strings on BoqItem for simplicity, with the taxonomy definition in a separate BoqClassification table.

### Large BOQ Support

- Virtualized tree view for 10,000+ items
- Lazy loading of child items where practical
- Background validation with progress reporting
- Batch import processing (reuse Phase 2 pattern)

## Detailed Work Breakdown

### Workstream A: Domain and Schema

1. Define Boq, BoqItem, BoqLibrary, BoqLibraryItem, BoqClassification entities
2. Define value objects (BoqStatus, BoqItemType, Unit)
3. Add entity configurations and relationships
4. Create migration and apply
5. Add indexes for common query paths

### Workstream B: Application Use Cases

1. Implement IBoqService (CRUD, tree queries, totals)
2. Implement IBoqItemService (CRUD, reorder, bulk update)
3. Implement IBoqImportService (Excel/CSV, tree builder)
4. Implement IBoqValidationService (structural and data rules)
5. Implement IBoqClassificationService (taxonomy, assignment)
6. Implement IBoqLibraryService (libraries, item insertion)
7. Implement IBoqReportService (summary, itemized)

### Workstream C: BOQ Import Engine

1. Create BoqImportService adapter over Phase 2 IWorkbookReader
2. Implement FlatRowToBoqItem mapper
3. Implement TreeBuilder (flat rows → hierarchy using Code/Level/ParentId)
4. Integrate with IMappingProfileService for column mapping
5. Add CSV import support
6. Implement batch commit with progress reporting

### Workstream D: Validation Engine

1. Create BOQ-specific validation rules
2. Implement required field validator
3. Implement code uniqueness validator
4. Implement parent existence validator
5. Implement circular reference detector
6. Implement unit recognition validator
7. Implement quantity/rate range validator
8. Aggregate validation results for UI display

### Workstream E: Classification Engine

1. Create BoqClassificationService
2. Implement taxonomy tree management (CRUD)
3. Implement bulk assign/remove codes to items
4. Implement filter items by classification
5. Implement import/export classification schemes

### Workstream F: BOQ Libraries

1. Create BoqLibraryService
2. Implement library CRUD
3. Implement library item management
4. Implement insert library items into BOQ
5. Seed standard libraries

### Workstream G: UI — BOQ List and Viewer

1. Create BOQ list view with project filter
2. Create tree viewer with expand/collapse
3. Create section subtotal and grand total display
4. Create context menu for item actions
5. Wire navigation from BOQ list to BOQ viewer tab

### Workstream H: UI — BOQ Import Wizard

1. Reuse Excel Studio Select File step (add BOQ option)
2. Reuse Excel Studio Map Columns step (add BOQ fields)
3. Create BOQ-specific tree preview step
4. Create validation results display step
5. Wire import completion to BOQ list navigation

### Workstream I: UI — BOQ Editor

1. Create inline editing for quantity, rate, description
2. Create add/insert item dialog
3. Create delete with child confirmation
4. Create reorder controls (move up/down)
5. Create bulk edit dialog
6. Implement undo/redo stack for edit operations

### Workstream J: UI — Classification and Libraries

1. Create classification taxonomy tree view
2. Create classification assignment panel
3. Create library list view
4. Create library item management view
5. Create insert library items dialog

### Workstream K: UI — BOQ Reports

1. Create BOQ summary report view
2. Create itemized BOQ report view
3. Implement Excel export (reuse Phase 2 WorkbookWriter)
4. Implement PDF export (QuestPDF)
5. Implement print preview

### Workstream L: Localization and Polish

1. Add English resource strings for all BOQ screens
2. Add Arabic resource strings for all BOQ screens
3. Verify Arabic layout and RTL behavior
4. Ensure theme consistency with the existing shell
5. Remove any hardcoded user-facing strings
6. Verify navigation rail BOQ icon and highlighting

### Workstream M: Testing

1. Unit test domain entities and value objects
2. Unit test validation engine
3. Unit test application services
4. Integration test import workflow
5. Integration test persistence round-trip
6. UI smoke test all BOQ workspaces

## Risks and Mitigations

### Risk: Self-referencing tree performance degrades with large BOQs

Mitigation:

- Use level-based queries with indexes on ParentId and BoqId
- Cache computed totals at the section level
- Virtualize the tree UI to handle 10,000+ items
- Consider closure table or materialized path if recursive CTEs become a bottleneck

### Risk: Flat-row-to-tree heuristics produce incorrect hierarchies

Mitigation:

- Support multiple import strategies: Code prefix matching, Level column, explicit ParentId column
- Allow user to preview and adjust the tree before commit
- Validate that the tree is complete (no orphans, no cycles)

### Risk: Phase 2 Excel services need modification for BOQ-specific needs

Mitigation:

- Keep BOQ-specific logic in the BOQ import adapter
- Extend Phase 2 contracts with backward-compatible additions if needed
- Avoid modifying existing Excel service behavior

### Risk: BOQ data model becomes too rigid for later WBS and cost integration

Mitigation:

- Keep BoqItem extensible with CostCode and ClassificationCode string fields
- Avoid deep coupling between BOQ and future WBS entities
- Use loose coupling through ProjectId and shared code references

### Risk: Tree editing (add/delete/reorder) is complex in WPF

Mitigation:

- Use a flat ObservableCollection backed by the tree for the DataGrid
- Rebuild the tree on save (flat → tree → persist)
- Keep the editor simple for Phase 3, enhance later

## Acceptance Criteria

Phase 3 is complete when all of the following are true:

- BOQs can be created, associated with projects, and persisted.
- BOQ items form a hierarchical tree with proper parent-child relationships.
- BOQs can be imported from Excel using the Phase 2 import infrastructure.
- Imported flat rows are correctly assembled into a tree hierarchy.
- Validation detects missing codes, duplicate codes, missing parents, and invalid units.
- Classification codes can be created, managed, and assigned to BOQ items.
- Libraries support reusable items and rate books.
- Items from libraries can be inserted into a BOQ.
- The BOQ tree viewer displays expand/collapse, section subtotals, and grand total.
- BOQ items can be edited inline with add, delete, and reorder operations.
- BOQ reports (summary and itemized) can be generated and exported to Excel/PDF.
- English and Arabic screens work correctly.
- RTL behavior remains correct.
- The existing shell, theme, and Phase 2 infrastructure remain intact.
- The implementation remains Clean Architecture and MVVM compliant.

## Definition of Done

- Core BOQ domain entities are implemented.
- BOQ import, validation, classification, and library services are functional.
- BOQ tree viewer and editor are integrated into the shell.
- BOQ reports are functional with Excel and PDF export.
- Localization is complete for all new BOQ scope.
- All tests pass.
- The data model is documented for Phase 4 (WBS Studio) consumption.

## Next Step After Phase 3

When BOQ Studio is stable, the next implementation plan should target Phase 4 — WBS Studio:

- Manual WBS creation from structured BOQ
- AI WBS generation
- WBS templates
- BOQ-to-WBS mapping integration
