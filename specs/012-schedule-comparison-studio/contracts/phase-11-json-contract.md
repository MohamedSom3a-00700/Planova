# Phase 11 JSON Contract: ScheduleComparisonResult

**Phase**: 10 → 11 boundary

**Version**: 1.0

**Status**: Draft (immutable once a session completes)

## Purpose

This JSON envelope is produced by Phase 10 (Schedule Comparison Studio) and consumed by Phase 11 (Delay Analysis). It represents the complete, unattributed diff result of a single comparison session.

## Schema

### ScheduleComparisonResult

```json
{
  "schemaVersion": "1.0",
  "sessionId": "guid",
  "projectId": 0,
  "mode": "BaselineVsUpdate | UpdateVsUpdate | XerVsXer | AsPlannedVsAsBuilt",
  "comparedAt": "2026-06-14T12:00:00Z",
  "source": { "sourceKind": "Snapshot", "projectId": 0, "snapshotId": "guid?", "primaveraProjectId": 0?, "label": "string", "capturedAt": "datetime?" },
  "target": { /* same structure as source */ },
  "includedScopes": ["Activities", "Logic", "Resources", "CriticalPath", "Float"],
  "generatedByVersion": "10.0.0.0",
  "activityDiffs": [ { /* ActivityDiff */ } ],
  "logicDiffs": [ { /* LogicDiff */ } ],
  "resourceDiffs": [ { /* ResourceDiff */ } ],
  "criticalPathDiffResult": { /* CriticalPathDiff or null */ },
  "floatReport": { /* FloatImpactReport or null */ },
  "summary": { /* ComparisonSummary */ }
}
```

### ComparisonSourceInfo

```json
{
  "sourceKind": "Snapshot | Primavera | Native",
  "projectId": 0,
  "snapshotId": "guid | null",
  "primaveraProjectId": 0 | null,
  "label": "string",
  "capturedAt": "datetime | null"
}
```

### ActivityDiff

```json
{
  "matchKey": "string",
  "fieldName": "Start | Finish | Duration | ...",
  "changeType": "Added | Removed | Modified | Unchanged",
  "oldValue": "string | null",
  "newValue": "string | null",
  "severity": "Critical | Major | Minor | Info"
}
```

### LogicDiff

```json
{
  "predecessorMatchKey": "string",
  "successorMatchKey": "string",
  "changeType": "Added | Removed | Modified | Unchanged",
  "oldRelationshipType": "FS | SS | FF | SF | null",
  "newRelationshipType": "FS | SS | FF | SF | null",
  "oldLag": "double?",
  "newLag": "double?"
}
```

### ResourceDiff

```json
{
  "activityMatchKey": "string",
  "resourceId": "string",
  "changeType": "Added | Removed | Modified | Unchanged",
  "oldUnits": "double?",
  "newUnits": "double?",
  "oldCost": "decimal?",
  "newCost": "decimal?"
}
```

### CriticalPathDiff

```json
{
  "sourceDuration": "double?",
  "targetDuration": "double?",
  "durationChange": "double?",
  "enteredCriticalPath": ["activityMatchKey"],
  "exitedCriticalPath": ["activityMatchKey"],
  "remainedOnCriticalPath": ["activityMatchKey"]
}
```

### FloatImpactReport

```json
{
  "activityFloatDeltas": [
    {
      "matchKey": "string",
      "oldTotalFloat": "double?",
      "newTotalFloat": "double?",
      "floatDelta": "double?",
      "oldFreeFloat": "double?",
      "newFreeFloat": "double?",
      "freeFloatDelta": "double?"
    }
  ],
  "activitiesWithNegativeFloat": ["matchKey"],
  "activitiesWithImprovedFloat": ["matchKey"],
  "activitiesWithWorsenedFloat": ["matchKey"]
}
```

### ComparisonSummary

```json
{
  "totalActivities": 0,
  "addedActivities": 0,
  "removedActivities": 0,
  "modifiedActivities": 0,
  "totalRelationships": 0,
  "addedRelationships": 0,
  "removedRelationships": 0,
  "modifiedRelationships": 0,
  "totalResourceAssignments": 0,
  "addedAssignments": 0,
  "removedAssignments": 0,
  "modifiedAssignments": 0,
  "criticalPathDurationDelta": "double?",
  "activitiesWithFloatLoss": 0,
  "activitiesWithFloatGain": 0
}
```

## Rules

- `schemaVersion` must be present and non-empty. Consumers should check this before parsing.
- The envelope is immutable once the session reaches Completed state. It is never overwritten or patched.
- Nullable fields (`?`) may be absent or null. Consumers must handle both.
- `sourceKind` and `targetKind` determine how `primaveraProjectId` and `snapshotId` should be interpreted.
