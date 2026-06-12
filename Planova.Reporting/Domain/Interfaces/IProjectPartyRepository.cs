using Planova.Reporting.Domain.Entities;

namespace Planova.Reporting.Domain.Interfaces;

public interface IProjectPartyRepository
{
    Task<List<ProjectParty>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<ProjectParty?> GetClientAsync(int projectId, CancellationToken ct = default);
    Task<ProjectParty?> GetMainContractorAsync(int projectId, CancellationToken ct = default);
    Task<List<ProjectParty>> GetSubContractorsAsync(int projectId, CancellationToken ct = default);
    Task<ProjectParty?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ProjectParty party, CancellationToken ct = default);
    Task UpdateAsync(ProjectParty party, CancellationToken ct = default);
    Task DeleteAsync(ProjectParty party, CancellationToken ct = default);
}
