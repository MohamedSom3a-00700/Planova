using Planova.Resource.Application.Dto;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceAssignmentService
{
    Task<ResourceAssignmentDto> CreateAsync(CreateAssignmentRequest request, CancellationToken ct = default);
    Task<ResourceAssignmentDto> UpdateAsync(UpdateAssignmentRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ResourceAssignmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ResourceAssignmentDto>> GetByActivityAsync(Guid activityId, CancellationToken ct = default);
    Task<List<ResourceAssignmentDto>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<decimal> GetActivityTotalCostAsync(Guid activityId, CancellationToken ct = default);
    Task<bool> ActivityHasAssignmentsAsync(Guid activityId, CancellationToken ct = default);
    Task CheckActivityDeletionAsync(Guid activityId, CancellationToken ct = default);
}
