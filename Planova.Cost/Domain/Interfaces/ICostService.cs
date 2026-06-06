using Planova.Cost.Application.Dto;

namespace Planova.Cost.Domain.Interfaces;

public interface ICostService
{
    Task<CostBreakdownDto> GetCostBreakdownAsync(int projectId, CancellationToken ct = default);
    Task<BudgetDto> GetBudgetAsync(int projectId, CancellationToken ct = default);
    Task<BudgetDto> UpdateBudgetAsync(Guid budgetId, UpdateBudgetRequest request, CancellationToken ct = default);
    Task<bool> HasResourceCostsChangedAsync(int projectId, CancellationToken ct = default);
    Task<CostBaselineDto> SetBaselineAsync(int projectId, CreateBaselineRequest request, CancellationToken ct = default);
}
