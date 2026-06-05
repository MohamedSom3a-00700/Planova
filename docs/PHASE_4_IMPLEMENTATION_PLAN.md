# Planova Phase 4 Implementation Plan

**Phase**: 4 — WBS Studio

**Date**: 2026-06-04

**Source of Truth**: [docs/01-PRODUCT_VISION.md](./01-PRODUCT_VISION.md),
[docs/02-MASTER_ROADMAP.md](./02-MASTER_ROADMAP.md),
[docs/04-SYSTEM_ARCHITECTURE.md](./04-SYSTEM_ARCHITECTURE.md),
[docs/06-MODULE_CATALOG.md](./06-MODULE_CATALOG.md),
[docs/09-AI_STRATEGY.md](./09-AI_STRATEGY.md),
[docs/11-UI_UX_DESIGN_SYSTEM.md](./11-UI_UX_DESIGN_SYSTEM.md),
[docs/PLANOVA_CONSTITUTION_DRAFT.md](./PLANOVA_CONSTITUTION_DRAFT.md),
[docs/PHASE_3_IMPLEMENTATION_PLAN.md](./PHASE_3_IMPLEMENTATION_PLAN.md)

## Summary

Phase 4 delivers the WBS (Work Breakdown Structure) Studio — the second specialized studio in the Planova project controls workflow. This phase transforms structured BOQ data from Phase 3 into a hierarchical work breakdown structure that becomes the backbone for scheduling, resource loading, and cost control in downstream phases.

The WBS Studio is the natural next step after BOQ Studio (Phase 3) and follows the ultimate workflow:

```
BOQ (Phase 3) → WBS (Phase 4) → Activities (Phase 5) → Resources (Phase 6) → Cost Loading (Phase 7)
```

This phase does not cover activities, resources, cost loading, or scheduling workflows. Those are separate downstream phases that will consume the structured WBS output.

## Phase 4 Objectives

1. Introduce the WBS domain model (Wbs, WbsItem, WbsTemplate, WbsTemplateItem).
2. Build a tree-structured WBS viewer and editor with hierarchy management.
3. Implement BOQ-to-WBS mapping engine that consumes Phase 3 BOQ data.
4. Create a WBS template system with reusable project structures.
5. Implement AI-assisted WBS generation using Semantic Kernel and Ollama.
6. Deliver WBS summary and dictionary reports with Excel/PDF export.
7. Provide WBS item weight allocation and progress tracking fields.
8. Preserve Clean Architecture, MVVM, localization, and theme consistency.
9. Ensure the WBS output is structured for consumption by Phase 5 (Activity Studio).

## Phase 4 Scope

### In Scope

- Wbs domain entity with project association and source tracking
- WbsItem domain entity with self-referencing tree hierarchy
- WbsTemplate and WbsTemplateItem domain entities
- WBS tree viewer with expand/collapse, level colors, and metadata columns
- WBS inline editor (add, edit, delete, reorder, restructure items)
- BOQ-to-WBS mapping wizard (auto-suggest, manual mapping, grouping strategies)
- BOQ item traceability via SourceBoqItemId on WbsItem
- WBS template manager (CRUD, apply template, seed standard templates)
- AI WBS generation from project scope description (Semantic Kernel + Ollama)
- WBS weight allocation (% distribution across work packages)
- WBS summary and dictionary reports with Excel/PDF export
- Navigation rail integration — WBS Studio opens in the multi-tab workspace
- Database persistence — new Wbs, WbsItems, WbsTemplates, WbsTemplateItems tables
- Localization (English and Arabic) for all WBS screens
- Unit tests for domain, services, validation, and mapping

### Out of Scope

- Activity generation from WBS (Phase 5)
- Logic relationships (Phase 5)
- Resource loading (Phase 6)
- Cost loading or cash flow (Phase 7)
- Primavera P6 integration (Phase 9)
- Multi-user collaboration on WBS (Phase 19)
- WBS code formatting configuration
- Advanced WBS analytics or earned value (Phase 18)
- Drag-and-drop tree restructuring (deferred to Phase 4 enhancement)

## Non-Negotiable Constraints

- Clean Architecture must remain intact.
- MVVM must remain the UI pattern.
- Localization must support English and Arabic.
- RTL behavior must remain correct.
- Theme support must remain consistent with Phases 0/1/2/3.
- Phase 3 BOQ module must be consumed (not duplicated).
- AI provider must be abstracted via IAIProvider — no direct vendor lock-in.
- Self-referencing tree must support large WBS structures with virtualized UI.
- The WBS data model must leave room for later Activity and Cost integration.
- WbsItem.SourceBoqItemId must allow nullable traceback to BOQ items.

## Phase 4 Product Shape

Phase 4 should feel like the natural structural companion to BOQ Studio.

The user should be able to:

- create a new WBS for a project (manual, from BOQ, from template, or AI-generated)
- view the WBS as a hierarchical tree with expand/collapse
- see colored level indicators (Summary, Control Account, Work Package, Planning Package)
- edit individual WBS items (name, description, dates, weight, assigned to)
- add new items and sections to the tree
- map a BOQ to automatically generate a WBS structure
- adjust the BOQ-to-WBS mapping before committing
- apply a template to generate a standard WBS structure
- use AI to suggest a WBS from a project scope description
- manage reusable WBS templates
- generate a formatted WBS report (summary or dictionary)
- export the WBS to Excel or PDF

## Target Solution Impact

Phase 4 extends the foundation established in Phases 0, 1, 2, and 3.

Expected additions:

- new Domain entities and value objects in a new Planova.Wbs project
- new Application services for WBS use cases
- new Infrastructure/Persistence mappings and migrations
- new UI modules for WBS Studio (viewer, editor, mapping, templates, AI, reports)
- expanded localization resources for WBS terminology
- AI provider integration for WBS generation

## Primary Domain Model

### Wbs

Represents a work breakdown structure associated with a project.

Properties:

- Id (Guid)
- ProjectId (int, FK -> Projects)
- Name
- Description
- Revision (int)
- Status (WbsStatus: Draft, Final, Revised, Approved)
- Source (WbsSource: Manual, FromBOQ, FromTemplate, AIGenerated)
- SourceBoqId (Guid?, FK -> Boqs — set when generated from BOQ)
- TotalWeight (decimal, computed — sum of top-level item weights)
- CreatedAt
- UpdatedAt

### WbsItem

Represents a single node in the WBS hierarchy.

Properties:

- Id (Guid)
- WbsId (Guid, FK -> Wbs)
- ParentId (Guid?, nullable, self-referencing FK -> WbsItem)
- Code (string — e.g. "1.1.2")
- Name (string — work package title)
- Description (string?)
- Level (int — 0 = root, 1 = level 1, etc.)
- SortOrder (int)
- WbsLevel (WbsLevelType: Summary, ControlAccount, WorkPackage, PlanningPackage)
- SourceBoqItemId (Guid?, FK -> BoqItem — traceability to BOQ)
- Weight (decimal? — % of total project work)
- PlannedStart (DateTime?)
- PlannedFinish (DateTime?)
- DurationDays (int?)
- AssignedTo (string? — responsible party/role)
- Deliverable (string?)
- Notes (string?)
- IsActive (bool)
- CreatedAt
- UpdatedAt

Navigation:

- Children (ICollection<WbsItem> — self-referencing tree)
- Parent (WbsItem?)
- Wbs (Wbs)
- SourceBoqItem (BoqItem?)

### WbsTemplate

Represents a reusable WBS structure for a type of project.

Properties:

- Id (Guid)
- Name
- Description
- Category (string? — "Building Construction", "Infrastructure", "Oil & Gas", "Industrial", etc.)
- Industry (string? — construction, oil & gas, infrastructure, etc.)
- IsStandard (bool — system-seeded vs user-created)
- Version (int)
- Tags (string? — JSON array)
- CreatedAt
- UpdatedAt

Navigation:

- Items (ICollection<WbsTemplateItem>)

### WbsTemplateItem

A reusable node in a WBS template.

Properties:

- Id (Guid)
- TemplateId (Guid, FK -> WbsTemplate)
- ParentId (Guid?, self-referencing FK -> WbsTemplateItem)
- Code (string)
- Name (string)
- Description (string?)
- Level (int)
- SortOrder (int)
- WbsLevel (WbsLevelType)
- DefaultDurationDays (int?)
- TypicalWeight (decimal?)
- Children (ICollection<WbsTemplateItem>)

### Value Objects

- **WbsStatus** — Draft, Final, Revised, Approved. Includes a transition map (same pattern as BoqStatus).
- **WbsLevelType** — Summary (rollup node), ControlAccount (management control point), WorkPackage (assignable work), PlanningPackage (placeholder for future decomposition).
- **WbsSource** — Manual, FromBOQ, FromTemplate, AIGenerated.

## Screen Plan

### 1. WBS List Workspace

- WBS list view with project filter
- Search by name, filter by status and source
- Create new WBS with source selection dialog:
  - **Manual** — start with a single root item
  - **From BOQ** — select a BOQ to map from
  - **From Template** — select a template to apply
  - **AI Generated** — enter project scope for AI suggestion
- Summary cards (total WBS, by status, by source)
- Open, edit, delete, revise actions

### 2. WBS Viewer

- Tree view with hierarchical expand/collapse
- Columns: Code, Name, Level Type, Weight%, Planned Dates, Assigned To, Deliverable
- Color-coded by WbsLevelType (Summary=blue, ControlAccount=green, WorkPackage=orange, PlanningPackage=gray)
- Level indentation with connecting lines
- Weight % bar visualization alongside items
- Tab-based: Viewer, Editor, Mapping, Reports

### 3. WBS Editor

- Inline editing of name, description, weight, dates, assigned to, deliverable
- Add new item (insert at level with WbsLevelType selection)
- Delete item with child confirmation (cascade)
- Reorder (move up/down)
- Weight redistribution on add/delete (auto-adjust sibling weights)
- Bulk update (dates, assigned to across selection)
- Undo/redo support for edit operations

### 4. BOQ-to-WBS Mapping Wizard

3-step workflow:

1. **Select BOQ** — Choose a BOQ from the project's available BOQs. Preview the BOQ tree.
2. **Map Items** — Auto-suggest WBS structure with three strategies:
   - **One-to-One**: Each BOQ item becomes a WBS work package, BOQ sections become summary nodes
   - **Grouped**: BOQ sections become WBS control accounts, child BOQ items roll into work packages
   - **Custom**: User manually maps BOQ items to WBS nodes (drag-assign)
   - Allow splitting (one BOQ item → multiple WBS items) and merging (multiple BOQ items → one WBS item)
3. **Preview & Commit** — Preview the generated WBS tree, adjust names/levels, set WbsLevelType, commit to database

Traceability: WbsItem.SourceBoqItemId links back to the originating BoqItem.

### 5. WBS Template Manager

- Browse templates by category/industry
- Standard seeded templates (Building, Infrastructure, Industrial, Oil & Gas)
- Create custom template from existing WBS
- Edit template items (add/delete/reorder)
- Apply template to project → creates new Wbs with template items
- Import/export templates as JSON
- Template preview (tree view)

### 6. AI WBS Generation Panel

- Text input for project scope/description
- Optional reference BOQ selection for context
- AI generates suggested WBS structure (name, levels, work packages)
- Accept/modify/regenerate workflow
- Configurable AI provider (Ollama default, OpenAI fallback)
- Powered by Semantic Kernel with IWbsAiGenerationService abstraction
- Progress indication during generation

### 7. WBS Reports

- **WBS Summary Report** — hierarchical view with levels, weights, item counts, date ranges
- **WBS Dictionary** — full item descriptions, responsibilities, deliverables, source BOQ references
- Export to Excel (use Phase 2 WorkbookWriter)
- Export to PDF (use QuestPDF)
- Print preview

## Implementation Order

### 1. Domain Model and Contracts

- Wbs entity
- WbsItem entity (self-referencing)
- WbsTemplate entity
- WbsTemplateItem entity
- WbsStatus value object with transition rules
- WbsLevelType enum
- WbsSource enum
- Repository/service interfaces

### 2. Persistence Schema and Mappings

- EF Core entity configurations for all WBS entities
- Relationships: Wbs -> Project, WbsItem -> Wbs, WbsItem self-reference, WbsItem -> BoqItem (optional)
- Indexes on Code, WbsId, ParentId, SourceBoqItemId
- Migration for WBS tables
- Seed data for standard WBS templates

### 3. Application Services

- IWbsService — CRUD, tree queries, weight distribution
- IWbsItemService — CRUD for items, reorder, bulk update
- IWbsBoqMappingService — BOQ-to-WBS mapping strategies, tree generation
- IWbsTemplateService — template CRUD, apply template, seed
- IWbsValidationService — structural and data validation
- IWbsAiGenerationService — AI-powered WBS suggestion
- IWbsReportService — summary and dictionary report queries

### 4. BOQ-to-WBS Mapping Engine

- One-to-One mapping strategy (BOQ item -> WBS item)
- Grouped mapping strategy (BOQ sections -> WBS summary nodes)
- Custom mapping with manual item assignment
- SourceBoqItemId traceability
- Preview before commit with diff view

### 5. WBS Validation Engine

- Required fields (Code, Name)
- Code uniqueness within a WBS
- Parent existence check
- Circular reference detection
- Weight validation (total must not exceed 100%)
- Date range validation (start before finish)
- WbsLevelType consistency (WorkPackage cannot have children)
- Configurable validation rules

### 6. WBS Templates

- Standard templates (seeded per industry)
- User-defined templates
- Apply template to project (deep copy with new IDs)
- Import/export templates as JSON

### 7. AI WBS Generation

- Semantic Kernel integration with IAIProvider abstraction
- Prompt template for WBS generation from project scope
- Optional BOQ context injection in prompt
- Structured output parsing (AI returns JSON WBS structure)
- Fallback to manual editing if AI unavailable

### 8. UI — WBS Navigation and Registration

- Register WBS Studio nav target in ShellViewModel
- Wire nav rail item to WbsStudioView
- Ensure WBS Studio opens in multi-tab workspace
- Remove placeholder if exists

### 9. UI — WBS List and Viewer

- WBS list view with create dialog (source selection)
- Tree view with virtualization (reuse BOQ tree control pattern)
- Expand/collapse at all levels
- Color-coded by WbsLevelType
- Weight % bar visualization
- Right-click context menu

### 10. UI — WBS Editor

- Inline editing via DataGrid or TreeList
- Add/insert item at level with WbsLevelType selection
- Delete with confirmation (cascade)
- Reorder via move buttons
- Weight auto-redistribution
- Bulk edit dialog

### 11. UI — BOQ-to-WBS Mapping Wizard

- Select BOQ step (project BOQ list)
- Mapping strategy selection (one-to-one, grouped, custom)
- Tree preview of generated WBS
- Manual adjustment controls
- Commit with progress

### 12. UI — Template Manager

- Template list with category/industry filters
- Apply template dialog
- Template item tree view
- Create/edit/delete templates
- Import/export JSON

### 13. UI — AI Generation Panel

- Project scope text input
- Optional BOQ reference selection
- Generate button with progress
- Preview AI-suggested WBS tree
- Accept/modify/regenerate workflow

### 14. UI — WBS Reports

- Summary and dictionary report views
- Excel export via Phase 2 WorkbookWriter
- PDF export via QuestPDF
- Print preview

### 15. Localization and RTL Coverage

- English and Arabic resources for all WBS screens
- Terms: WBS, work package, control account, summary, weight, deliverable, dictionary
- RTL verification for tree views and data grids

### 16. Validation

- Create/edit WBS smoke tests
- BOQ-to-WBS mapping end-to-end tests
- Validation engine tests
- Template apply tests
- AI generation tests (mock provider)
- Report generation tests
- Localization correctness tests
- Persistence round-trip tests

## Technical Decisions

### Tree Data Structure

Use a self-referencing table (ParentId) in SQLite with a recursive CTE for tree queries. Same pattern as Phase 3 BOQ. In-memory tree building from flat lists for UI consumption.

### Tree UI Control

Reuse the same VirtualizingTreeView approach from Phase 3 BOQ. If a shared tree control component exists, use it. Otherwise, implement parallel tree rendering.

### Weight Distribution

Weight allocation uses percentage distribution across siblings. Total weight at each level must sum to 100%. Application layer handles auto-redistribution when items are added or removed.

### BOQ-to-WBS Mapping

The mapping engine reads BoqItems from Planova.Boq's repositories and generates WbsItems with SourceBoqItemId links. Three strategies with user preview before commit. The mapping is a one-time generation step, not a live sync.

### AI Integration

Use Semantic Kernel with IAIProvider abstraction (from Planova.AI or a shared AI project). The WBS generation prompt requests a JSON response with a hierarchical structure. Parsed and presented as a suggested WBS tree. Ollama with Llama 3.2 is the default provider.

### Templates

WbsTemplates are stored in SQLite with a JSON serialization path for import/export. Applying a template performs a deep copy of all template items into a new Wbs with new GUIDs.

### WBS Performance Targets

- Virtualized tree view for 5,000+ WBS items
- Lazy loading of child items where practical
- Background validation with progress reporting
- BOQ-to-WBS mapping for 1,000 BOQ items in under 5 seconds

## Detailed Work Breakdown

### Workstream A: Domain and Schema

1. Define Wbs, WbsItem, WbsTemplate, WbsTemplateItem entities
2. Define value objects (WbsStatus, WbsLevelType, WbsSource)
3. Add entity configurations and relationships
4. Create migration and apply
5. Add indexes for common query paths (WbsId, ParentId, SourceBoqItemId)
6. Seed standard WBS templates

### Workstream B: Application Use Cases

1. Implement IWbsService (CRUD, tree queries, weight distribution)
2. Implement IWbsItemService (CRUD, reorder, bulk update)
3. Implement IWbsValidationService (structural and data rules)
4. Implement IWbsReportService (summary, dictionary)

### Workstream C: BOQ-to-WBS Mapping Engine

1. Create WbsBoqMappingService
2. Implement One-to-One mapping strategy
3. Implement Grouped mapping strategy
4. Implement Custom mapping with manual assignment
5. Implement SourceBoqItemId traceability
6. Implement preview with diff view

### Workstream D: WBS Templates

1. Create WbsTemplateService
2. Implement template CRUD
3. Implement apply template (deep copy)
4. Implement import/export as JSON
5. Seed standard templates (Building, Infrastructure, Industrial, Oil & Gas)

### Workstream E: AI WBS Generation

1. Create WbsAiGenerationService
2. Design WBS generation prompt template
3. Implement Semantic Kernel integration
4. Implement structured JSON output parsing
5. Handle AI unavailability gracefully (fallback to manual)

### Workstream F: UI — WBS Registration and Navigation

1. Register WBS Studio nav target in ShellViewModel
2. Create WbsStudioView as the main workspace container
3. Create WbsStudioViewModel with tab initialization
4. Wire nav rail item with icon and label

### Workstream G: UI — WBS List and Viewer

1. Create WBS list view with project filter and source indicator
2. Create tree viewer with expand/collapse and level colors
3. Create weight % bar visualization
4. Create context menu for item actions
5. Wire navigation from WBS list to WBS tree viewer

### Workstream H: UI — WBS Editor

1. Create inline editing for name, description, dates, weight
2. Create add/insert item dialog with WbsLevelType selection
3. Create delete with child confirmation
4. Create reorder controls (move up/down)
5. Create bulk edit dialog
6. Implement undo/redo stack

### Workstream I: UI — BOQ-to-WBS Mapping Wizard

1. Create select BOQ step (list project BOQs with preview)
2. Create mapping strategy selection step
3. Create tree preview with source/target side-by-side
4. Create manual adjustment controls
5. Wire commit with progress

### Workstream J: UI — Template Manager

1. Create template list/filter view
2. Create apply template dialog
3. Create template item tree viewer
4. Create template editor (add/edit/delete items)
5. Create import/export JSON

### Workstream K: UI — AI Generation Panel

1. Create project scope text input
2. Create generate button with progress indicator
3. Create AI-suggested tree preview
4. Create accept/modify/regenerate workflow

### Workstream L: UI — WBS Reports

1. Create WBS summary report view
2. Create WBS dictionary report view
3. Implement Excel export (reuse Phase 2 WorkbookWriter)
4. Implement PDF export (QuestPDF)
5. Implement print preview

### Workstream M: Localization and Polish

1. Add English resource strings for all WBS screens
2. Add Arabic resource strings for all WBS screens
3. Verify Arabic layout and RTL behavior
4. Ensure theme consistency with the existing shell
5. Remove any hardcoded user-facing strings
6. Verify navigation rail WBS icon and highlighting

### Workstream N: Testing

1. Unit test domain entities and value objects
2. Unit test validation engine
3. Unit test application services
4. Unit test BOQ-to-WBS mapping strategies
5. Unit test AI generation service (with mock provider)
6. Integration test persistence round-trip
7. Integration test template apply
8. UI smoke test all WBS workspaces

## Risks and Mitigations

### Risk: Self-referencing tree performance degrades with large WBS structures

Mitigation:
- Use level-based queries with indexes on ParentId and WbsId
- Cache computed weight totals at the item level
- Virtualize the tree UI for 5,000+ items
- Same pattern proven in Phase 3 BOQ

### Risk: BOQ-to-WBS mapping heuristics produce incorrect hierarchies

Mitigation:
- Support multiple mapping strategies
- Allow user preview and manual adjustment before commit
- Validate that the resulting tree is complete (no orphans, no cycles)
- Show traceability (SourceBoqItemId) for verification

### Risk: AI-generated WBS quality is inconsistent

Mitigation:
- Provider abstraction allows swapping models
- AI output is always suggested — never committed without user review
- User can edit, restructure, or regenerate
- Prompt engineering with examples improves consistency

### Risk: WBS data model becomes too rigid for Activity Studio

Mitigation:
- Keep WbsItem extensible with Notes, Deliverable, and custom fields
- Avoid deep coupling between WBS and future Activity entities
- Use loose coupling through WbsId and shared code references

### Risk: Phase 3 BOQ module may need minor modification for WBS integration

Mitigation:
- Access BoqItems through existing Phase 3 repository interfaces
- Keep WBS mapping logic in Planova.Wbs, not in Planova.Boq
- Extend Phase 3 contracts with backward-compatible additions if needed

## Acceptance Criteria

Phase 4 is complete when all of the following are true:

- WBS structures can be created, associated with projects, and persisted.
- WBS items form a hierarchical tree with proper parent-child relationships.
- WBS can be auto-generated from a BOQ with user-configurable mapping strategies.
- Mapping traceability (SourceBoqItemId) is preserved on WbsItem.
- WBS templates can be applied to create a new WBS structure.
- Standard WBS templates are seeded for common project types.
- AI can suggest a WBS from a project scope description (with manual acceptance).
- The WBS tree viewer displays expand/collapse, level colors, and weight bars.
- WBS items can be added, deleted, reordered, and restructured inline.
- WBS weight distribution auto-adjusts on add/delete.
- WBS reports (summary and dictionary) can be generated and exported to Excel/PDF.
- English and Arabic screens work correctly.
- RTL behavior remains correct.
- The existing shell, theme, and Phase 0/1/2/3 infrastructure remain intact.
- The implementation remains Clean Architecture and MVVM compliant.

## Definition of Done

- Core WBS domain entities are implemented.
- WBS CRUD, BOQ-to-WBS mapping, templates, and AI generation services are functional.
- WBS tree viewer and editor are integrated into the shell with navigation registration.
- BOQ-to-WBS mapping wizard is functional with all three strategies.
- WBS template manager is functional with standard seeded templates.
- AI WBS generation is functional with provider abstraction.
- WBS reports are functional with Excel and PDF export.
- Localization is complete for all new WBS scope.
- All tests pass.
- The data model is documented for Phase 5 (Activity Studio) consumption.

## Next Step After Phase 4

When WBS Studio is stable, the next implementation plan should target Phase 5 — Activity Studio:

- Activity generation from WBS work packages
- Logic relationships (FS, SS, FF, SF)
- Milestones
- Calendars
- WBS-to-Activity mapping integration
