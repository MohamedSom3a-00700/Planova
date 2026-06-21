# Quickstart: Overall Enhancement Plan

## Overview

This feature delivers a visual and functional refresh across five Planova areas. The work is split into independent workstreams that can be developed in parallel.

## Workstreams

| # | Workstream | Module(s) | Priority | Dependencies |
|---|-----------|-----------|----------|--------------|
| 1 | Dashboard redesign | `Planova.UI` | P1 | Existing project data model |
| 2 | Projects screen enrichment | `Planova.UI`, `Planova.Persistence` | P1 | New Project metadata fields |
| 3 | Party management | `Planova.UI`, `Planova.Persistence` | P1 | New Party entities |
| 4 | Multi-sheet BOQ import | `Planova.Boq`, `Planova.Excel`, `Planova.UI` | P1 | Existing BOQ entities |
| 5 | WBS Editor + drag-drop | `Planova.Wbs`, `Planova.UI` | P1 | Existing WBS entities |
| 6 | BOQ → WBS mapping | `Planova.Wbs`, `Planova.Boq` | P1 | Workstream 4, 5 |
| 7 | WBS AI generation | `Planova.Wbs`, `Planova.UI` | P2 | Semantic Kernel setup |
| 8 | WBS reports & export | `Planova.Wbs`, `Planova.Excel` | P2 | Workstream 5 |
| 9 | Project folder auto-creation | `Planova.Application` | P2 | — |
| 10 | WBS templates | `Planova.Wbs` | P2 | Workstream 5 |

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022+
- SQLite (included via EF Core)
- Ollama (optional, for local AI development)

### Key files

| File | Purpose |
|------|---------|
| [spec.md](./spec.md) | Feature specification |
| [plan.md](./plan.md) | Implementation plan with project structure |
| [data-model.md](./data-model.md) | Entity definitions and relationships |
| [contracts/cross-module-interfaces.md](./contracts/cross-module-interfaces.md) | Interface contracts between modules |
| [research.md](./research.md) | Technical decisions and rationale |

### Implementation order

1. **Sprint 1**: Workstreams 1, 2, 3 (UI + persistence for Dashboard, Projects, Parties)
2. **Sprint 2**: Workstreams 4, 9 (BOQ import + folder creation)
3. **Sprint 3**: Workstreams 5, 6 (WBS editor + BOQ mapping)
4. **Sprint 4**: Workstreams 7, 8, 10 (AI, reports, templates)

### Key contracts to implement first

- `IBoqImportService` — drives the multi-sheet import flow
- `IWbsLockService` — enables concurrent edit safety
- `IWbsCodeGenerationService` — needed before WBS editor drag-drop
- `IPartyService` — needed for parties rail on Projects screen

### Testing approach

- Unit test all comparers, mapping engines, and AI orchestration logic
- Integration test import/export against sample Excel files
- UI test ViewModel state transitions for import modes, WBS edit operations
- Test empty states (no projects, no BOQ, no WBS) for each screen
