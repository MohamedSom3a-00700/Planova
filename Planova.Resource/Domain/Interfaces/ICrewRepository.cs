using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Domain.Interfaces;

public interface ICrewRepository
{
    Task<Crew?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Crew>> GetAllAsync(int? projectId = null, CrewStatus? status = null, CancellationToken ct = default);
    Task<List<Crew>> SearchAsync(string query, int? projectId = null, CancellationToken ct = default);
    Task AddAsync(Crew crew, CancellationToken ct = default);
    Task UpdateAsync(Crew crew, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
