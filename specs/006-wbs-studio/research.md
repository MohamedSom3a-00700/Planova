# Research: WBS Studio

All technical context items were resolvable from the Planova Constitution and existing patterns (Phase 3 BOQ Studio). No [NEEDS CLARIFICATION] markers were present.

## Technology Decisions

| Decision | Choice | Rationale | Alternatives Considered |
|----------|--------|-----------|------------------------|
| Tree UI pattern | Self-referencing ParentId with recursive CTE | Same proven pattern as Phase 3 BOQ; supports arbitrary depth | Adjacency list, Nested Sets, Materialized Path — ParentId adequate for CRUD-heavy trees |
| Virtualized tree control | Reuse VirtualizingTreeView from Phase 3 | Avoid reinvention; consistent UX | Custom implementations — higher risk, no benefit |
| AI framework | Semantic Kernel + IAIProvider abstraction | Per constitution (VI. AI Provider Agnostic) | Direct Ollama API, LangChain — SK provides provider abstraction |
| Default AI provider | Ollama (Llama 3.2) | Per constitution; local, no API key needed | OpenAI — requires API key and internet |
| Report PDF | QuestPDF | Per constitution; used in Phase 2 | iTextSharp, PDFSharp — QuestPDF already approved |
| Report Excel | ClosedXML / EPPlus | Per constitution; used in Phase 2 (WorkbookWriter) | NPOI, Office Interop — existing infrastructure reused |
| Weight model | Percentage-based siblings sum to 100% | Industry standard for WBS weight allocation | Absolute hours, relative ranking, equal distribution |
| Mapping strategies | One-to-One, Grouped, Custom | Covers the 3 common WBS generation patterns from BOQs | Single automatic strategy — insufficient flexibility |
| Template storage | SQLite with JSON import/export | Templates stored in DB for queryability; JSON for portability | Separate JSON files only — loses DB query benefits |
| Status lifecycle | Draft → Final → Approved → Revised → Draft | Forward-only with revision cycle (clarified with user) | Free-form — risk of invalid states |

## Key Architecture Decisions

- **New Planova.Wbs project**: Follows same pattern as Planova.Boq — dedicated project with Domain + Application layers
- **BOQ consumed as read-only dependency**: Planova.Wbs references Planova.Boq repositories for mapping, does not modify BOQ data
- **AI generation as suggestion only**: Never committed without user review; output is always suggested WBS tree JSON parsed by structured output
- **Weight auto-redistribution**: Application layer handles recalculation on add/delete; siblings redistribute proportionally
- **Two code systems per WBS item**: Numeric (tree position based) and alpha short code (from item name, min 3 letters) — both auto-generated, read-only in UI
