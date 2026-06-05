using Planova.Activity.Application.Dto;

namespace Planova.Activity.Domain.Interfaces;

public interface IActivityRelationshipService
{
    Task<ActivityRelationshipDto> CreateAsync(CreateRelationshipRequest request, CancellationToken ct = default);
    Task<ActivityRelationshipDto> UpdateAsync(UpdateRelationshipRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityRelationshipDto>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<List<ActivityRelationshipDto>> GetByActivityAsync(Guid activityId, CancellationToken ct = default);
    Task<CircularReferenceCheckResult> ValidateNewRelationshipAsync(Guid predecessorId, Guid successorId, CancellationToken ct = default);
}
