# Research: Schedule Comparison Studio

**Phase**: 0 — Outline & Research

**Date**: 2026-06-14

**Prerequisite for**: Phase 1 (design & contracts)

## Decision Log

### Tech stack

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| .NET 8 / C# 12 | Platform-wide standard established in Phase 0 | — |
| WPF + Fluent UI WPF | Platform UI standard per constitution | — |
| CommunityToolkit.Mvvm | Platform MVVM standard per constitution | — |
| EF Core 8 + SQLite | Platform persistence standard per constitution | — |
| ClosedXML (Excel export) | Platform Excel standard; reuses `IWorkbookWriter` abstraction from Planova.Excel | EPPlus (license change risk), Interop (server-incompatible) |
| QuestPDF (PDF export) | Platform reporting standard per constitution | — |
| xUnit | Platform test framework standard | — |

### Comparison engine approach

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Dedicated comparers per dimension (Activity, Logic, Resource, CP, Float) | Single responsibility, independently testable, independently selectable | Monolithic comparer (harder to test/maintain) |
| Neutral `ScheduleData` model | Decouples comparers from any source schema; enables uniform matching pipeline | Compare source-native models directly (tight coupling, no reuse) |
| Deterministic matching priority (provenance ID → activity ID → WBS+code → fuzzy) | Predictable results; low-confidence flagged for review | Single strategy (misses matches); ML-based matching (over-engineered, non-deterministic) |
| CP membership + float comparison from existing fields | Phase 10 scope boundary — no CPM scheduler | Full forward/backward pass recalculation (out of scope, Phase 11 enhancement) |

### Optional Primavera resolution

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| `IServiceProvider.GetService<IPrimaveraWorkspaceService>()` | Clean null return when not registered; no DI wiring failures | Nullable constructor injection (fails when service not registered) |

### Persistence strategy

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Two-tier storage: `ComparisonResult` rows (queryable) + `ResultJson` envelope (Phase 11) | Single JSON blob alone not queryable for grids/paging; separate rows alone loses full envelope semantics | Single JSON blob only (no paging/filtering); full normalization only (no Phase 11 envelope) |
| Repository registered in `Planova.Persistence` | Clean Architecture — module must not reference EF Core or persistence layer | Register in module (violates dependency direction) |

### Session lifecycle

| Decision | Rationale |
|----------|----------|
| Draft → Running → Completed / Failed / Cancelled (terminal states final) | Clear state machine prevents inconsistent records; no in-place re-runs |

### Concurrent operation handling

| Decision | Rationale |
|----------|----------|
| Disable Run button during execution; confirm snapshot deletion when referenced | Prevents data races without a queue system; appropriate for single-user desktop app |

### Maximum supported size

| Decision | Rationale |
|----------|----------|
| 50K activities / 150K relationships / 5K resources | 5x moderate benchmark provides headroom; beyond that, performance warning shown |

### Excel export

| Decision | Rationale |
|----------|-----------|
| Reuse `Planova.Excel.Writers.IWorkbookWriter` where data model fits; custom ClosedXML multi-sheet workbooks as a `ComparisonExportService` detail | Avoids new cross-module abstraction; keeps custom behavior contained |

### Export failure handling

| Decision | Rationale |
|----------|-----------|
| Temp-file-then-move pattern; failed export does not invalidate session | Prevents partial/corrupted export files; user can retry from History tab |
