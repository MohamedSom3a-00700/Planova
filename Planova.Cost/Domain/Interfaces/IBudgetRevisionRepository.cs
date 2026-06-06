using Planova.Cost.Domain.Entities;

namespace Planova.Cost.Domain.Interfaces;

public interface IBudgetRevisionRepository
{
    Task<List<BudgetRevision>> GetByBudgetIdAsync(Guid budgetId, CancellationToken ct = default);
    Task<BudgetRevision?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<int> GetNextRevisionNumberAsync(Guid budgetId, CancellationToken ct = default);
    Task AddAsync(BudgetRevision revision, CancellationToken ct = default);
    Task UpdateAsync(BudgetRevision revision, CancellationToken ct = default);
}
