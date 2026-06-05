using Planova.Resource.Domain.Entities;

namespace Planova.Resource.Domain.Interfaces;

public interface ICrewResourceRepository
{
    Task<List<CrewResource>> GetByCrewAsync(Guid crewId, CancellationToken ct = default);
    Task<List<CrewResource>> GetByResourceAsync(Guid resourceId, CancellationToken ct = default);
    Task<CrewResource?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(CrewResource crewResource, CancellationToken ct = default);
    Task UpdateAsync(CrewResource crewResource, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteByCrewAsync(Guid crewId, CancellationToken ct = default);
}
