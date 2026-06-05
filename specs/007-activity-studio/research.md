# Research: Activity Studio (Phase 5)

## Overview

All Technical Context unknowns from plan.md have been resolved through codebase exploration. This document captures key decisions, rationale, and alternatives evaluated.

## Technology Decisions

### Gantt Chart Rendering

- **Decision**: Custom Canvas control using `DrawingVisual` for performance
- **Rationale**: No off-the-shelf WPF Gantt library meets the dual requirements of (a) rendering 1,000+ activities with relationship arrows in under 2 seconds and (b) full customization for WbsSummary rollup indicators, milestone diamonds, and RTL layout. WPF-UI provides no charting components. LiveCharts2 was evaluated but is designed for data-plotting charts, not Gantt timelines.
- **Alternatives considered**: LiveCharts2 (insufficient for Gantt), OxyPlot (no Gantt support), commercial Gantt libraries (licensing overhead, overkill for single-user desktop), DataGrid-based rendering (performance insufficient for 1k+ items with arrows)

### Circular Reference Detection

- **Decision**: DFS-based cycle detection (Tarjan's or iterative DFS) on the adjacency graph at relationship creation time
- **Rationale**: The relationship graph is directed and acyclic by definition (no scheduling loops). A DFS approach has O(V+E) complexity — for 10k activities, completion in under 1 second is achievable with an optimized adjacency list representation. Detection runs only when a relationship is added or modified, not on every read.
- **Alternatives considered**: Floyd-Warshall (O(V³) too slow), topological sort on full graph (overkill for incremental check), BFS from successor forward (DFS preferred for cycle detection)

### Calendar Date Calculations

- **Decision**: Pure in-memory date engine that respects assigned calendar's working days and exceptions
- **Rationale**: Calendar date math (adding duration to a start date, computing duration between two dates) is computationally intensive for large activity sets. Running this in-memory avoids round-trips to SQLite per date calculation. SQLite has date functions but they don't know about custom calendar exceptions.
- **Alternatives considered**: SQL date functions (no custom calendar support), EF Core computed columns (too rigid), stored procedures (anti-pattern per Clean Architecture)

### Activity Code Generation

- **Decision**: Sequential auto-number per project (A-001, A-002, A-003...) with optional WBS code prefix inheritance
- **Rationale**: Matches FR-004 requirement. Sequential numbering is simple, performant, and user-friendly. WBS prefix inheritance adds traceability when activities are generated from WBS.
- **Alternatives considered**: UUID-based codes (non-human-readable), user-assigned codes (too error-prone), hierarchical codes like "1.1.1.1" (confusing when WBS is deep)

### WbsSummary Auto-Rollup

- **Decision**: Rollup computed on read via child traversal (not stored/cached in DB)
- **Rationale**: WbsSummary dates, duration, and percent complete are derived from child activities (FR-005). Computing on read ensures correctness when child activities change. For large hierarchies, a cache-invalidation strategy can be added later if performance warrants.
- **Alternatives considered**: Stored rollup values with trigger-based updates (complex, error-prone with circular updates), computed SQL columns (limited expression support), background job (staleness window)

### Activity Bank Seed Data

- **Decision**: Seeded via EF Core migration or JSON configuration file loaded on first access
- **Rationale**: FR-016 requires 50+ entries across 13 construction categories. JSON configuration is easier to author and maintain than C# seed code. EF Core migration seed is an alternative if referential integrity requires relational seeding.
- **Alternatives considered**: Hardcoded C# seed (verbose, hard to maintain), SQL script (not version-controlled in code), external SQLite file (deployment complexity)

### Predecessor Delete Warning

- **Decision**: Soft-block: warn + require confirmation before deleting an activity that is a predecessor to others
- **Rationale**: Fr-003 US1 Scenario 4 requires warning but allows deletion (orphaned relationships are auto-deleted). This balances user autonomy with data integrity.
- **Alternatives considered**: Hard-block (prevents deletion, frustrating for users), cascade-delete with undo (overly complex), auto-reassign relationships (ambiguous semantics)

### Report Export Infrastructure

- **Decision**: Reuse existing Phase 2 `IWorkbookWriter` (ClosedXML) for Excel export and QuestPDF for PDF export
- **Rationale**: Assumption in spec states these are already available from Phase 2. Codebase confirms `Planova.Excel` project with `IWorkbookWriter`, `IWorkbookReader`, and QuestPDF usage in existing reports.
- **Alternatives considered**: EPPlus (also available but ClosedXML is the established pattern), iTextSharp (not in the stack), HTML-to-PDF (not established in codebase)

### Multilingual & RTL

- **Decision**: Follow existing localization pattern: resource files per module, `ILocalizationService` with runtime switching, `FlowDirection` for RTL
- **Rationale**: Established in Phases 0-4. The Gantt chart Canvas must respect `FlowDirection` for RTL — this means mirroring the time axis (right-to-left timeline) and relationship arrows.
- **Alternatives considered**: Separate RTL layouts (duplication), CSS-like flip transforms (not applicable to WPF Canvas)

## Integration Decisions

### WBS Integration

- **Decision**: Activity references WbsItem via `WbsItemId` (FK, nullable). Read-only access to WBS entities from Planova.Activity through Planova.Wbs public interfaces.
- **Rationale**: Activities link to WBS for traceability (FR-003). Cross-module access follows existing Wbs → Boq integration pattern (read-only FK reference). Planova.Activity references Planova.Domain (for WbsItem entity) and Planova.Shared.

### Shell Integration

- **Decision**: Replace placeholder registration in ShellViewModel with real Activity Studio view factory
- **Rationale**: Navigation target "activity" is already registered with `isPlaceholder: true`. The implementation replaces the placeholder factory with `ActivityStudioView` creation + `InitializeTabs` call, matching BOQ and WBS patterns exactly.

### Existing Project Structure Integration

- **Decision**: Activity DbSets added to existing `PlanovaDbContext`; entity configurations in `Planova.Persistence/EntityConfigurations/`
- **Rationale**: Single DbContext pattern is established. New configurations follow the same naming convention (`ActivityConfiguration.cs`, etc.) and registration via `modelBuilder.ApplyConfigurationsFromAssembly()`.

## Key Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Gantt Canvas performance with 10k+ activities | Medium | High | Virtualized rendering (visible range only), DrawingVisual batching, async scroll debouncing |
| Circular reference detection on large graphs blocking UI | Low | High | Background thread with CancellationToken, progress feedback for very large networks |
| Calendar date calculation correctness with edge cases (all non-working days, leap years) | Low | Medium | Comprehensive unit tests for CalendarDateCalculator with edge case coverage |
| Activity Bank seed data quality (50+ entries across 13 categories) | Medium | Medium | JSON-driven with review process; entries seeded as plausible defaults, not production-ready |
| RTL Gantt correctness (mirrored timeline, arrows) | Medium | Medium | Early visual verification with Arabic sample data; automated snapshot tests |
