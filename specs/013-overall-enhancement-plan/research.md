# Research: Overall Enhancement Plan

**Phase**: 0 — Outline & Research

**Date**: 2026-06-17

**Prerequisite for**: Phase 1 (design & contracts)

## Decision Log

### Tech stack

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| .NET 8 / C# 12 | Platform-wide standard established in constitution | — |
| WPF + Fluent UI WPF | Platform UI standard per constitution | — |
| CommunityToolkit.Mvvm | Platform MVVM standard per constitution | — |
| EF Core 8 + SQLite | Platform persistence standard per constitution | — |
| ClosedXML (Excel import/export) | Platform Excel standard; reuses existing `IWorkbookReader`/`IWorkbookWriter` from `Planova.Excel` | EPPlus (license change risk), Interop (server-incompatible) |
| QuestPDF (PDF reports) | Platform reporting standard per constitution | — |
| Semantic Kernel + Ollama (default) | Platform AI standard per constitution; configurable endpoint for production | Direct OpenAI SDK (vendor lock-in) |
| xUnit | Platform test framework standard | — |

### Heuristic BOQ column mapping

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Built-in heuristic matching using column name patterns (Code, Description, Unit, Qty, Rate, Amount) + row population threshold | No external AI dependency; predictable deterministic results; works offline | External AI API (ongoing costs, internet required); ML model (over-engineered for column name matching) |

### Multi-sheet BOQ import strategy

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Scan all worksheets; detect BOQ sheets by column signature; skip known non-BOQ names (Cover, Summary, Index, etc.); offer Single/Multiple/Merge modes | Simple column-signature approach catches real BOQ sheets; merge mode creates hierarchical structure preserving order | Require manual selection (slow, error-prone); merge-all flat (loses section boundaries) |
| Stream large Excel files using ClosedXML's deferred loading per worksheet | Memory-efficient for files up to 50MB/100K rows | Load all worksheets into memory (crashes on large files) |

### WBS auto-code generation

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Structured dot-notation codes (1, 1.1, 1.2, 1.2.1) regenerated on every structural change | Always consistent with current tree; no stale codes | Static codes assigned once (break on reorder); manual codes (error-prone) |

### WBS AI generation

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Semantic Kernel with configurable provider (default Ollama + Llama 3.2); graceful degradation when API unreachable | AI-agnostic per constitution; offline-capable with Ollama; fallback UX prevents blocking | Direct provider SDK (vendor lock-in); built-in rules-only (limited for document understanding) |

### WBS concurrent editing

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Lock-based: acquire lock on edit open; release on save/cancel; other users see read-only with lock holder name | Prevents data loss without complex merge UI; appropriate for desktop app with infrequent concurrent edits | Last-write-wins (data loss risk); merge/diff (over-engineered for desktop app) |

### Empty and loading states

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Contextual empty states with guidance text + suggested next action; inline loading spinners for async operations | Informs user what to do next; keeps UI responsive | Skeleton screens (more dev effort for marginal UX gain); no empty state (confusing) |

### Role-based access control

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Four roles: Project Manager (full), Planner (create/edit WBS only), Quantity Surveyor (import BOQ + map to WBS), Viewer (read-only) | Covers the key personas from the spec; clear separation of duties | Open access (security risk); two-role only (too coarse) |

### WBS status transitions

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Forward progression (Draft→Under Review→Approved→Archived) with limited reversals (Draft→Archived, Under Review→Draft, Approved→Under Review) | Supports real-world workflow (revisions, cancellations) without unbounded state machine | Linear only (too rigid); free-text status (no validation) |

### Dashboard card approach

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Fluent UI styled cards with icons; light glass-effect (acrylic); separate ViewModels per card type (HealthCard, CostCard) | Consistent with constitution's Fluent UI mandate; card-per-metric enables independent updates | Single monolithic card (harder to maintain); no glass effect (misses spec requirement) |

### Project folder auto-creation

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Create 13 standard subfolders synchronously on folder path assignment; non-destructive on path change (create new, don't delete old) | Simple, predictable, non-destructive | Async creation (unnecessary complexity); destructive on path change (data loss risk) |
