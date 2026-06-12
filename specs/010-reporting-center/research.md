# Research: Reporting Center

## Design Decisions

### 1. Report Data Provider Pattern — Generic Typed Interface

**Decision**: Use `IReportDataProvider<TData>` generic interface with four concrete providers (Daily, Weekly, Monthly, Executive), each returning a strongly-typed DTO, resolved via a provider factory

**Rationale**:
- Each report type has different data shape requirements — a single polymorphic interface with casting would lose type safety
- Provider factory maintains `Dictionary<ReportType, Type>` mapping for DI-based resolution in `ReportEngine`
- `DataSnapshotJson` uses `System.Text.Json` with type discriminator for round-trip fidelity
- This pattern is consistent with prior module approaches (no over-engineered abstraction)

**Alternatives considered**:
- *Single `IReportDataProvider` returning `object`*: Rejected — loses type safety; requires runtime casting throughout the engine
- *Fully dynamic (ExpandoObject/Dictionary)*: Rejected — no compile-time validation; IDE support lost

### 2. Report Engine — Data Snapshot Strategy

**Decision**: `DataSnapshotJson` is the single authoritative source; `ReportSections` are derived denormalized views generated at creation time

**Rationale**:
- `DataSnapshotJson` stores the complete frozen DTO as serialized JSON — this is the source of truth for viewing, re-export, and data integrity
- `ReportSection` records are derived from the snapshot at creation time for efficient UI display (avoids full JSON deserialization for section list rendering)
- If `ReportSections` become corrupted or lost, they can be regenerated from `DataSnapshotJson`
- `AiNarrative` convenience field on `ReportInstance` duplicates the narrative from `DataSnapshotJson` to avoid deserialization when only narrative text is needed

**Alternatives considered**:
- *Only snapshot JSON, no derived sections*: Rejected — each report view would require full JSON deserialization just to render section listings (History, Schedule results preview)
- *Only sections, no snapshot JSON*: Rejected — sections are display-oriented; exporting would require re-assembly that may not match original

### 3. Scheduled Generation — In-Process IHostedService

**Decision**: `ReportGenerationHostedService` runs as an in-process `IHostedService` inside the WPF application with 60-second tick interval

**Rationale**:
- No external scheduler or Windows Service needed for v1 (acceptable for desktop app)
- 60-second tick balances responsiveness with CPU overhead
- `NextRunAt` pre-computed and stored in UTC; each tick queries `WHERE IsActive AND NextRunAt <= UtcNow`
- Missed schedules caught up on next launch (within MaxRetries limit)
- Per-schedule locking to prevent duplicate concurrent generation of same schedule

**Alternatives considered**:
- *Windows Task Scheduler integration*: Rejected — adds OS-specific dependency; less testable
- *External background service*: Rejected — adds deployment complexity; single-process desktop app is simpler for v1

### 4. NextRunAt Timezone Handling

**Decision**: `TimeOfDay` stored in schedule's configured timezone; `NextRunAt` stored in UTC after conversion

**Rationale**:
- Users configure time in their local timezone (e.g., "8:00 AM Cairo time")
- `TimeZoneId` (IANA or Windows ID) stored per schedule for computation
- Candidate time computed as: `todayInZone.Date + TimeOfDay` → if past, advance to next period → convert to UTC for `NextRunAt`
- If `TimeZoneId` changes, schedules auto-adjust on next computation cycle
- No DST ambiguity issues beyond standard `TimeZoneInfo` handling

**Alternatives considered**:
- *Always UTC*: Rejected — user would have to compute UTC offset manually, causing confusion
- *Local server time only*: Rejected — users in different timezones need their own schedules

### 5. Export File Lifecycle

**Decision**: Export files stored on disk; `ReportExport` records track metadata; deletion is soft-delete for instance-level, hard-delete for explicit export-level

**Rationale**:
- FR-022 / Edge Cases define: deleting a `ReportInstance` soft-deletes export records (file preserved for audit); explicit "Delete Export" removes file + record
- Project cascade: all report files and records deleted on project deletion
- Orphan cleanup: application startup scans for orphaned export files (no matching `ReportExport` record) and removes them
- Archival purge: instances older than 365 days with Archived status may be purged (configurable)

**Alternatives considered**:
- *Database BLOB storage*: Rejected — SQLite BLOBs would bloat database; file system is simpler for export documents
- *Always hard-delete on instance delete*: Rejected — audit trail requirement demands file retention

### 6. Report Status Lifecycle

**Decision**: Forward-only state machine: Draft → Final → Archived (per Q4 clarification)

**Rationale**:
- No way to go back — prevents accidental data loss and keeps audit trail clean
- To update a Final report, user must generate a new instance
- Archived is terminal — reports in this state are hidden from default History view but can be viewed via filter

**Alternatives considered**:
- *Bidirectional*: Rejected — per user clarification; forward-only is simpler and prevents confusion

### 7. AI Narrative Integration

**Decision**: `IReportAiService` generates type-appropriate narratives via `IAIProvider` (Semantic Kernel); UI provides Generate/Regenerate/Accept/Edit workflow

**Rationale**:
- Per constitution Principle VI (AI Agnostic), `IAIProvider` abstraction prevents vendor lock-in
- Each report type receives a distinct system prompt tuned to its purpose (daily factual, weekly trend, monthly EVM analysis, executive strategic)
- AI unavailability: buttons disabled with informative message; existing narratives remain editable (FR-010)
- All AI output labeled "AI-Generated" per FR-009

**Alternatives considered**:
- *Single generic prompt for all types*: Rejected — each report type has different audience and depth requirements
- *Pre-generated narratives stored as templates*: Rejected — spec requires AI-powered generation; templates would not provide contextual analysis

### 8. Project Parties — Logo Storage

**Decision**: Logo images stored on disk under `{AppData}/Planova/Projects/{projectId}/Parties/`; file path stored in `ProjectParty.LogoPath`

**Rationale**:
- FR-027 explicitly specifies this path scheme
- File system storage avoids SQLite BLOB bloat for images
- Path is project-scoped, making project-cascade deletion straightforward
- Supported formats: common image formats (PNG, JPG, BMP)

**Alternatives considered**:
- *Database BLOB*: Rejected — same reasoning as export files; images are binary files better suited to file system
