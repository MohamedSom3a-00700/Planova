using Planova.Cost.Domain.Entities;

namespace Planova.Cost.Domain.Interfaces;

public interface IDirectCostRepository
{
    Task<List<DirectCost>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<List<DirectCost>> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task<DirectCost?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DirectCost directCost, CancellationToken ct = default);
    Task UpdateAsync(DirectCost directCost, CancellationToken ct = default);
    Task DeleteAsync(DirectCost directCost, CancellationToken ct = default);
    Task MarkAsOrphanedByActivityIdAsync(Guid activityId, CancellationToken ct = default);
}
