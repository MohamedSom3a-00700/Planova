using Planova.Cost.Domain.Entities;

namespace Planova.Cost.Domain.Interfaces;

public interface ICostBaselineRepository
{
    Task<CostBaseline?> GetActiveBaselineAsync(int projectId, CancellationToken ct = default);
    Task<List<CostBaseline>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<CostBaseline?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(CostBaseline baseline, CancellationToken ct = default);
    Task UpdateAsync(CostBaseline baseline, CancellationToken ct = default);
}
