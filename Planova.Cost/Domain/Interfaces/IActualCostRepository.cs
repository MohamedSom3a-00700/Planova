using Planova.Cost.Domain.Entities;

namespace Planova.Cost.Domain.Interfaces;

public interface IActualCostRepository
{
    Task<ActualCost?> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task<List<ActualCost>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task AddAsync(ActualCost actualCost, CancellationToken ct = default);
    Task UpdateAsync(ActualCost actualCost, CancellationToken ct = default);
    Task MarkAsOrphanedByActivityIdAsync(Guid activityId, CancellationToken ct = default);
}
