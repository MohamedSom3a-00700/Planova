# Domain Interfaces — Cost Studio

## Repository Interfaces

### IBudgetRepository

```csharp
public interface IBudgetRepository
{
    Task<Budget?> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Budget budget, CancellationToken ct = default);
    Task UpdateAsync(Budget budget, CancellationToken ct = default);
}
```

### IBudgetRevisionRepository

```csharp
public interface IBudgetRevisionRepository
{
    Task<List<BudgetRevision>> GetByBudgetIdAsync(Guid budgetId, CancellationToken ct = default);
    Task<BudgetRevision?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<int> GetNextRevisionNumberAsync(Guid budgetId, CancellationToken ct = default);
    Task AddAsync(BudgetRevision revision, CancellationToken ct = default);
    Task UpdateAsync(BudgetRevision revision, CancellationToken ct = default);
}
```

### IDirectCostRepository

```csharp
public interface IDirectCostRepository
{
    Task<List<DirectCost>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<List<DirectCost>> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task<DirectCost?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DirectCost directCost, CancellationToken ct = default);
    Task UpdateAsync(DirectCost directCost, CancellationToken ct = default);
    Task DeleteAsync(DirectCost directCost, CancellationToken ct = default);
}
```

### ICostBaselineRepository

```csharp
public interface ICostBaselineRepository
{
    Task<CostBaseline?> GetActiveBaselineAsync(int projectId, CancellationToken ct = default);
    Task<List<CostBaseline>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<CostBaseline?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(CostBaseline baseline, CancellationToken ct = default);
    Task UpdateAsync(CostBaseline baseline, CancellationToken ct = default);
}
```

### IActualCostRepository

```csharp
public interface IActualCostRepository
{
    Task<ActualCost?> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task<List<ActualCost>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task AddAsync(ActualCost actualCost, CancellationToken ct = default);
    Task UpdateAsync(ActualCost actualCost, CancellationToken ct = default);
    Task<List<ImportResultDto>> ImportFromExcelAsync(
        Stream excelStream, int projectId, CancellationToken ct = default);
}
```

## Service Interfaces

### ICostService

```csharp
public interface ICostService
{
    Task<CostBreakdownDto> GetCostBreakdownAsync(int projectId, CancellationToken ct = default);
    Task<BudgetDto> GetBudgetAsync(int projectId, CancellationToken ct = default);
    Task<BudgetDto> UpdateBudgetAsync(Guid budgetId, UpdateBudgetRequest request, CancellationToken ct = default);
    Task<bool> HasResourceCostsChangedAsync(int projectId, CancellationToken ct = default);
}
```

### IDirectCostService

```csharp
public interface IDirectCostService
{
    Task<List<DirectCostDto>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<List<DirectCostDto>> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task<DirectCostDto> CreateAsync(CreateDirectCostRequest request, CancellationToken ct = default);
    Task<DirectCostDto> UpdateAsync(Guid id, UpdateDirectCostRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### IActualCostService

```csharp
public interface IActualCostService
{
    Task<ActualCostDto?> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task<ActualCostDto> SetAsync(Guid activityId, decimal amount, string currency, CancellationToken ct = default);
    Task<ImportResultDto> ImportFromExcelAsync(Stream excelStream, int projectId, CancellationToken ct = default);
    Task<List<ActivityVarianceDto>> GetVarianceByProjectAsync(int projectId, CancellationToken ct = default);
}
```

### ICashFlowService

```csharp
public interface ICashFlowService
{
    Task<List<CashFlowPeriodDto>> GetCashFlowAsync(
        int projectId, CashFlowPeriodType periodType, DateTime? dataDate, CancellationToken ct = default);
}
```

### IEvmService

```csharp
public interface IEvmService
{
    Task<EvmMetricsDto> ComputeMetricsAsync(
        int projectId, DateTime dataDate, CancellationToken ct = default);
    Task<List<ActivityEvmDto>> GetActivityDetailAsync(
        int projectId, DateTime dataDate, CancellationToken ct = default);
}
```

### ICostAiService

```csharp
public interface ICostAiService
{
    Task<AiSuggestionDto> EstimateCostAsync(Guid activityId, CancellationToken ct = default);
    Task<List<CostAnomalyDto>> DetectAnomaliesAsync(int projectId, CancellationToken ct = default);
    Task<AiForecastDto> ForecastEacAsync(int projectId, CancellationToken ct = default);
    Task<string> GenerateNarrativeAsync(int projectId, CancellationToken ct = default);
    bool IsAvailable { get; }
}
```

### ICostReportService

```csharp
public interface ICostReportService
{
    Task<ReportResultDto> GenerateReportAsync(
        CostReportType reportType, int projectId, CancellationToken ct = default);
    Task<byte[]> ExportToExcelAsync(
        CostReportType reportType, int projectId, CancellationToken ct = default);
    Task<byte[]> ExportToPdfAsync(
        CostReportType reportType, int projectId, CancellationToken ct = default);
}
```
