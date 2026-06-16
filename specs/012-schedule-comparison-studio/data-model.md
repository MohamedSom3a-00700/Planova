# Data Model: Schedule Comparison Studio

**Phase**: 1 — Design & Contracts

**Date**: 2026-06-14

## Entities

### ComparisonSession

| Field | Type | Description |
|-------|------|-------------|
| `Id` | Guid | Primary key |
| `ProjectId` | int | Owning project |
| `Mode` | ComparisonMode | BaselineVsUpdate, UpdateVsUpdate, XerVsXer, AsPlannedVsAsBuilt |
| `State` | SessionState | Draft, Running, Completed, Failed, Cancelled |
| `SourceKind` | string | "Snapshot", "Primavera", "Native" |
| `SourceSnapshotId` | Guid? | Snapshot ID if source is a snapshot |
| `SourcePrimaveraProjectId` | int? | Primavera project ID if source is Primavera |
| `SourceLabel` | string | User-visible source label |
| `SourceCapturedAt` | DateTime? | When source data was captured |
| `TargetKind` | string | Same as SourceKind |
| `TargetSnapshotId` | Guid? | Snapshot ID if target is a snapshot |
| `TargetPrimaveraProjectId` | int? | Primavera project ID if target is Primavera |
| `TargetLabel` | string | User-visible target label |
| `TargetCapturedAt` | DateTime? | When target data was captured |
| `IncludedScopes` | string | Comma-separated scope list: Activities,Logic,Resources,CriticalPath,Float |
| `ResultJson` | string | Serialized `ScheduleComparisonResult` envelope (immutable once Completed) |
| `Error` | string? | Error message if Failed |
| `CreatedAt` | DateTime | Session creation timestamp |
| `StartedAt` | DateTime? | When comparison execution started |
| `CompletedAt` | DateTime? | When comparison finished (success or failure) |

**State transitions**: Draft → Running → Completed / Failed / Cancelled

**Validation**:
- Must be in Running state to transition to Completed/Failed/Cancelled
- Terminal states (Completed/Failed/Cancelled) are immutable
- Cannot transition from terminal state to any other state

### ComparisonResult

| Field | Type | Description |
|-------|------|-------------|
| `Id` | Guid | Primary key |
| `SessionId` | Guid | FK → ComparisonSession |
| `EntityType` | string | "Activity", "Relationship", "ResourceAssignment" |
| `MatchKey` | string | Deterministic match key used for entity matching |
| `ChangeType` | ChangeType | Added, Removed, Modified, Unchanged |
| `MatchConfidence` | MatchConfidence | High, Low |
| `FieldName` | string | The field that changed (null for Added/Removed) |
| `OldValue` | string? | Previous value (null for Added) |
| `NewValue` | string? | New value (null for Removed) |
| `Severity` | string | Critical, Major, Minor, Info (based on configured thresholds) |

**Validation**:
- `OldValue` must be null when ChangeType is Added
- `NewValue` must be null when ChangeType is Removed
- `FieldName` must be null when ChangeType is Added or Removed (the whole entity changed)
- MatchKey must be unique per SessionId + EntityType

### ScheduleSnapshot

| Field | Type | Description |
|-------|------|-------------|
| `Id` | Guid | Primary key |
| `ProjectId` | int | Owning project |
| `Label` | string | User-defined name |
| `BaselineId` | Guid? | Baseline GUID for traceability |
| `SnapshotData` | string | Serialized schedule data (activities, relationships, resource assignments) |
| `CapturedAt` | DateTime | When the snapshot was captured |
| `ActivityCount` | int | Count of activities in snapshot (denormalized) |
| `RelationshipCount` | int | Count of relationships in snapshot (denormalized) |

**Validation**:
- Label must be non-empty and ≤ 200 characters
- Cannot delete a snapshot referenced by any existing ComparisonSession

### ComparisonRule

| Field | Type | Description |
|-------|------|-------------|
| `Id` | Guid | Primary key |
| `ProjectId` | int | Owning project |
| `Name` | string | Rule name |
| `SeverityThresholdCritical` | double | Days change threshold for Critical severity |
| `SeverityThresholdMajor` | double | Days change threshold for Major severity |
| `SeverityThresholdMinor` | double | Days change threshold for Minor severity |
| `EnableFuzzyMatching` | bool | Whether fuzzy name matching is enabled |
| `MatchingStrategyPreference` | string | Ordered priority list for matching strategies |

## Enums

### ComparisonMode

| Value | Description |
|-------|-------------|
| `BaselineVsUpdate` | Compare a stored baseline against current schedule |
| `UpdateVsUpdate` | Compare two update snapshots |
| `XerVsXer` | Compare two Primavera imports |
| `AsPlannedVsAsBuilt` | Compare planned vs as-built snapshots |

### SessionState

| Value | Description |
|-------|-------------|
| `Draft` | Created but not yet executing |
| `Running` | Comparison execution in progress |
| `Completed` | Comparison finished successfully |
| `Failed` | Comparison finished with error |
| `Cancelled` | Comparison was cancelled by user |

### ChangeType

| Value | Description |
|-------|-------------|
| `Added` | Entity exists only in target |
| `Removed` | Entity exists only in source |
| `Modified` | Entity exists in both but field differs |
| `Unchanged` | Entity exists in both with no changes |

### MatchConfidence

| Value | Description |
|-------|-------------|
| `High` | Matched via deterministic strategy (priority 1-4) |
| `Low` | Matched via fuzzy name or ambiguous match |

### ComparisonScope

| Value | Description |
|-------|-------------|
| `Activities` | Compare activity fields |
| `Logic` | Compare relationships |
| `Resources` | Compare resource assignments |
| `CriticalPath` | Compare CP membership |
| `Float` | Compare float values |

## Relationships

```
ComparisonSession 1 ── * ComparisonResult
    (SessionId FK)

Project 1 ── * ComparisonSession
    (ProjectId FK)

Project 1 ── * ScheduleSnapshot
    (ProjectId FK)

Project 1 ── * ComparisonRule
    (ProjectId FK)
```

## Cross-Phase Contract (Phase 11)

The `ScheduleComparisonResult` model is not a persisted entity — it is the versioned JSON envelope stored on `ComparisonSession.ResultJson`. Key fields:

| Field | Type | Description |
|-------|------|-------------|
| `SchemaVersion` | string | "1.0" — contract version for Phase 11 consumption |
| `SessionId` | Guid | Matches ComparisonSession.Id |
| `ProjectId` | int | Matches ComparisonSession.ProjectId |
| `Mode` | string | Comparison mode as string |
| `ComparedAt` | DateTime | When the comparison completed |
| `Source` | ComparisonSourceInfo | Source metadata (kind, project ID, snapshot ID, label, captured at) |
| `Target` | ComparisonSourceInfo | Target metadata (same structure) |
| `IncludedScopes` | List\<string\> | Which dimensions were included |
| `GeneratedByVersion` | string | Application version that generated this result |
| `ActivityDiffs` | List\<ActivityDiff\> | Per-activity diff records |
| `LogicDiffs` | List\<LogicDiff\> | Per-relationship diff records |
| `ResourceDiffs` | List\<ResourceDiff\> | Per-resource assignment diff records |
| `CriticalPathDiffResult` | CriticalPathDiff? | CP membership and drift comparison |
| `FloatReport` | FloatImpactReport? | Float delta report |
| `Summary` | ComparisonSummary | Aggregate counts and statistics |
