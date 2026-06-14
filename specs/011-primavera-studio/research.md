# Research: Primavera Studio

**Branch**: `011-primavera-studio` | **Date**: 2026-06-12 | **Plan**: [plan.md](plan.md)

## Overview

All technical context is known from the Phase 9 implementation plan (`docs/PHASE_9_IMPLEMENTATION_PLAN.md`). The spec was validated through `/speckit.clarify` with 3 clarifications resolved (entity matching on re-import, import crash recovery, audit logging scope). No unresolved NEEDS CLARIFICATION markers remain.

## XER Parser Design

### Decision
Build a custom focused parser/writer for supported XER table types. Unsupported tables preserved as raw staging data for round-trip fidelity.

### Rationale
No mature off-the-shelf .NET XER parser library exists that meets the project's needs for selective table parsing, raw table preservation, and baseline storage. Building in-house gives full control over supported table versions and round-trip behavior.

### Supported XER Tables
- CALENDAR, PROJECT, TASK (activities), TASKPRED (relationships), TASKRSRC (resource assignments), RSPROJECT (resource codes), RSOURCE (resources), RCATTYPE, RCATVAL (resource code types/values), PROJECTCODE, PROJCODECAT, PROJCODEVAL (project code types/values), UDFTYPE, UDFVALUE (UDFs), PROJECTBASELINE, TASKUDF (task-level UDFs)

### Unsupported Tables
RISK, DOCUMENT, NOTE, etc. — read as raw column+row data, stored per import session, re-emitted verbatim on export.

### Version Tolerance
Parser tolerant of optional fields and version-specific sections across Primavera P6 versions 6–22. Unknown constructs preserved via raw table mechanism.

## Raw Table Round-Trip Strategy

### Decision
Each unsupported XER table is stored as an `XerRawTable` entity with table name, column headers, and serialized row data during import. During export, raw tables are re-emitted verbatim in the same order they appeared in the original XER.

### Rationale
Preserves data integrity for unsupported constructs. Users cannot edit raw table data through the workspace — they pass through transparently.

## Performance Baseline

### Decision
Use the "moderate schedule" definition as the benchmark: ~10,000 activities, 30,000 relationships, 1,000 resources, 10 calendars, 5 baselines.

### Target Metrics
| Operation | Target |
|-----------|--------|
| XER import preview | <10s |
| Validation | <5s |
| XER export | <10s |
| Workspace open | <5s |
| Cross-studio read resolution | <2s |
| UI responsiveness (grid ops) | <100ms |

## Cross-Studio Contract Pattern

### Decision
Follow the same `IReportDataProvider<T>` pattern established in Phase 8 (Reporting Center): nullable injection of domain service interfaces, with fallback logic in each consumer.

### Rationale
- Eliminates a dedicated integration service and source resolver
- Keeps fallback decisions local to each consumer
- No direct DB access — all data flows through service interfaces
- Nullable injection naturally handles "Primavera unavailable" without a resolver abstraction

## Import Recovery Strategy

### Decision
Each import commit is wrapped in an atomic database transaction. On any failure mid-way, the entire transaction is rolled back — no partial data persists.

### Rationale
Maintains data integrity. Corrupted files are detected early by pre-validation before the transaction begins.

## Entity Matching on Re-import

### Decision
Merge behavior matches entities by XER internal IDs (task_id, calendar_id, etc.). Overwrite matched rows, insert unmatched rows, flag rows present in DB but absent from new file for user review.

### Rationale
XER IDs are stable identifiers within the Primavera ecosystem. Name-based matching would be unreliable due to rename scenarios.

## Audit Logging Scope

### Decision
Log all mutation operations: import attempts (success/failure with file name and row counts), exports (file path and row counts), repair actions, and workspace edits (batch summary). Validation runs are NOT logged.

### Rationale
Provides traceability for all data-changing operations while avoiding noise from read-only operations like validation.
