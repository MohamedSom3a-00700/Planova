# Research Findings: BOQ Studio

## 1. Virtualized Tree View for WPF

**Decision**: Use a custom VirtualizingTreeView built on WPF VirtualizingStackPanel, or adopt an existing OSS solution like `gong-wpf-dragdrop`-compatible tree with virtualization.

**Rationale**: WPF's built-in TreeView does not support virtualization out of the box for hierarchical data with expand/collapse. A flat VirtualizingStackPanel with indentation modeling (each item stores its level) is the most reliable approach. The tree is rendered as a flat list with level-based indentation and expand/collapse toggling visibility of children. This pattern is proven in large-data WPF applications.

**Alternatives considered**:
- Standard WPF TreeView with IsVirtualizing — poor performance beyond 500 items
- Third-party controls (Telerik, DevExpress) — cost prohibited per Build vs Buy principle
- WinUI TreeView — not compatible with WPF

## 2. Tree Hierarchy Assembly Strategy

**Decision**: Support three detection strategies in priority order: (1) explicit Level column, (2) explicit ParentId column, (3) Code prefix matching (dot-separated segments).

**Rationale**: The spec (FR-007) requires all three approaches. Each maps to real-world BOQ data formats. The tree builder applies strategies in priority order — the first one with data wins.

**Alternatives considered**:
- Single-strategy only — too restrictive for diverse user data
- AI-based detection — premature; violates AI Agnostic principle for a deterministic problem

## 3. CSV Reader Library

**Decision**: Use CsvHelper (NuGet: CsvHelper).

**Rationale**: CsvHelper is the de facto standard .NET CSV parsing library. It handles edge cases (quoted fields, escaped commas, different cultures), is well-maintained, and has zero external dependencies beyond .NET.

**Alternatives considered**:
- Manual parsing — error-prone, unnecessary complexity
- TextFieldParser (VB.NET) — works but outdated API style
- Sylvan.Data.Csv — newer but less ecosystem support

## 4. PDF Report Generation

**Decision**: Use QuestPDF (NuGet: QuestPDF).

**Rationale**: QuestPDF is already the approved reporting library per the constitution (Technology Standards > Reporting). It supports RTL text (required for Arabic), has a fluent C# API, and can generate both memory-stream and file outputs.

**Alternatives considered**:
- iTextSharp — commercial licensing complications
- PdfSharp — limited RTL support
- Microsoft Print to PDF — no programmatic control

## 5. Optimistic Locking with EF Core

**Decision**: Use EF Core concurrency tokens (`[ConcurrencyCheck]` or `IsRowVersion`) on Boq entity. Increment version on every save. On `DbUpdateConcurrencyException`, prompt user to reload and retry.

**Rationale**: Spec (edge cases) explicitly requires optimistic locking. EF Core's built-in concurrency token mechanism handles the SQLite compatibility (use `IsRowVersion` with a byte[] column or manual integer increment).

**Alternatives considered**:
- Pessimistic locking — not suitable for single-user desktop app
- Application-level lock — unnecessary complexity for single-user scenario

## 6. Subtotal Computation Strategy

**Decision**: Compute subtotals and grand total in the application layer, not the database (FR-003). Use recursive tree traversal on the loaded in-memory tree.

**Rationale**: The tree is loaded into memory for UI display. Computing totals on the loaded graph avoids N+1 queries and gives immediate recalculation on inline edits.

**Alternatives considered**:
- Database computed columns — cannot handle hierarchical aggregation without recursive CTEs (SQLite limitation)
- Database trigger-based — couples logic to persistence layer, violates Clean Architecture

## 7. Localization Strategy

**Decision**: Reuse Planova.Localization project patterns. Add BOQ-specific resource strings (.resx) in Planova.Localization. Use the existing runtime switching mechanism.

**Rationale**: FR-019 requires English + Arabic with runtime switching. Existing localization infrastructure in Planova.Localization already supports this pattern (used by other modules).

## 8. Phase 2 Reuse Points

| Phase 2 Component | BOQ Studio Usage |
|---|---|
| IWorkbookReader | Read Excel workbooks during BOQ import |
| IWorkbookPreviewService | Preview worksheets in import wizard |
| IMappingProfileService | Save/reuse column mapping profiles for BOQ imports |
| IWorkbookWriter | Export BOQ reports to Excel |
| IValidationService | Reuse for Excel-level validation; BOQ-specific validation is separate |

## Decisions Consolidated

| Topic | Decision |
|-------|----------|
| Virtualized Tree | Flat VirtualizingStackPanel with level indentation |
| Tree Assembly | Level > ParentId > Code prefix (priority order) |
| CSV Parsing | CsvHelper |
| PDF Reports | QuestPDF |
| Concurrency | EF Core concurrency token (integer version) |
| Subtotal Compute | Application-layer tree traversal |
| Localization | Planova.Localization .resx + runtime switch |
| Phase 2 Reuse | IWorkbookReader, IMappingProfileService, IWorkbookWriter |
