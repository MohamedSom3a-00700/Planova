using Planova.Cost.Domain.Entities;

namespace Planova.Cost.Domain.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Budget budget, CancellationToken ct = default);
    Task UpdateAsync(Budget budget, CancellationToken ct = default);
}
