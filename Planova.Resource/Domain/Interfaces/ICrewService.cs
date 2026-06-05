using Planova.Resource.Application.Dto;

namespace Planova.Resource.Domain.Interfaces;

public interface ICrewService
{
    Task<CrewDto> CreateAsync(CreateCrewRequest request, CancellationToken ct = default);
    Task<CrewDto> UpdateAsync(UpdateCrewRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<CrewDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<CrewDto>> GetAllAsync(int? projectId = null, CancellationToken ct = default);
    Task<CrewDto> CloneAsync(Guid id, string newName, CancellationToken ct = default);
    Task<decimal> ComputeBlendedRateAsync(Guid crewId, DateTime? rateDate = null, CancellationToken ct = default);
    Task<List<ResourceAssignmentDto>> ApplyToActivitiesAsync(Guid crewId, List<Guid> activityIds, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
    Task AddResourceToCrewAsync(Guid crewId, Guid resourceId, decimal quantity, bool isLead, CancellationToken ct = default);
    Task RemoveResourceFromCrewAsync(Guid crewResourceId, CancellationToken ct = default);
    Task UpdateCrewResourceQuantityAsync(Guid crewResourceId, decimal quantity, CancellationToken ct = default);
}
