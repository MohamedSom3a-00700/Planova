using Planova.Cost.Application.Dto;

namespace Planova.Cost.Domain.Interfaces;

public interface IDirectCostService
{
    Task<List<DirectCostDto>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<List<DirectCostDto>> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task<DirectCostDto> CreateAsync(CreateDirectCostRequest request, CancellationToken ct = default);
    Task<DirectCostDto> UpdateAsync(Guid id, UpdateDirectCostRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task MarkOrphanedByActivityIdAsync(Guid activityId, CancellationToken ct = default);
}
