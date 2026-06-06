# Cross-Module Contracts — Cost Studio

## Dependencies on Other Modules

### Planova.Activity

Cost Studio reads activities for:
- Cost breakdown tree structure (WBS → Activity hierarchy)
- Activity planned dates for cash flow spreading
- Activity percent complete for EVM computation
- Activity code matching for actual cost import

**Consumed interfaces** (from `Planova.Activity.Domain`):

```csharp
// Activity entity reference (ID + code + name + dates + percentComplete)
// Cost Studio does NOT modify activities — read-only dependency
public interface IActivityQueryService
{
    Task<List<ActivitySummary>> GetByProjectIdAsync(int projectId, CancellationToken ct);
    Task<ActivitySummary?> GetByIdAsync(Guid activityId, CancellationToken ct);
}
```

### Planova.Resource

Cost Studio reads resource assignment costs for:
- Loading resource costs into the cost breakdown tree
- Aggregating total resource cost per activity

**Consumed interfaces** (from `Planova.Resource.Domain`):

```csharp
// Resource assignment cost data — aggregate by activity
public interface IResourceCostQueryService
{
    Task<List<ResourceCostByActivityDto>> GetCostByActivityAsync(
        int projectId, CancellationToken ct);
}

public record ResourceCostByActivityDto(
    Guid ActivityId,
    decimal TotalCost,
    string Currency
);
```

### Planova.Wbs

Cost Studio reads WBS hierarchy for:
- Cost breakdown tree top-level structure
- Grouping activities under WBS nodes

**Consumed interfaces**:

```csharp
// WBS tree structure — read-only
public interface IWbsQueryService
{
    Task<List<WbsNodeDto>> GetHierarchyAsync(int projectId, CancellationToken ct);
}

public record WbsNodeDto(
    Guid Id,
    string Code,
    string Name,
    Guid? ParentId
);
```

## Contracts Exposed by Cost Studio

Cost Studio does not expose interfaces consumed by other modules in this phase. Other modules are downstream consumers of cost data only through reports and exports.

## Event-Based Integration

Cost Studio observes the following domain events for cross-module consistency:

| Event | Source | Handler | Action |
|-------|--------|---------|--------|
| ActivityDeleted | Planova.Activity | CostStudio | Mark associated DirectCosts and ActualCosts as orphaned |
| ResourceCostChanged | Planova.Resource | CostStudio | Set budget resource cost change indicator (FR-030) |
| BudgetRevisionApproved | Planova.Cost | AuditLog | Log approval event with user identity and timestamp |
| BaselineSet | Planova.Cost | AuditLog | Log baseline creation event |

## Excel Integration

Cost Studio uses `Planova.Excel` infrastructure for:

- `ActualCostImportReader`: Reads Excel files with columns `[ActivityCode, Amount, Currency, EntryDate]`
- `CostReportWriter`: Writes Excel files for all four report types
- Shared services: `IImportService`, `IExportService` from `Planova.Excel`

## AI Integration

Cost Studio uses `Planova.Shared` AI abstractions:

```csharp
// Configuration-based AI provider resolution (Ollama, OpenAI, etc.)
// Via Semantic Kernel — IChatCompletionService from Microsoft.SemanticKernel.Abstractions
```

Configuration in `appsettings.json`:

```json
{
  "CostAiOptions": {
    "Provider": "Ollama",
    "Model": "llama3.2",
    "EstimationPromptTemplate": "...",
    "AnomalyPromptTemplate": "...",
    "ForecastPromptTemplate": "...",
    "NarrativePromptTemplate": "..."
  }
}
```

## Audit Logging

Cost Studio uses the shared `AuditLog` infrastructure for sensitive operations:

| Operation | Entity | Payload |
|-----------|--------|---------|
| Budget revision created | BudgetRevision | `{ BudgetId, RevisionNumber, RevisionType, Amount, CreatedBy }` |
| Budget revision approved | BudgetRevision | `{ BudgetId, RevisionNumber, ApprovedBy, ApprovedAt }` |
| Baseline set | CostBaseline | `{ ProjectId, BaselineId, Description, CreatedBy }` |
| Baseline removed | CostBaseline | `{ ProjectId, BaselineId, CreatedBy }` |
| Manual BAC override | Budget | `{ BudgetId, PreviousTotal, NewTotal, UpdatedBy }` |
