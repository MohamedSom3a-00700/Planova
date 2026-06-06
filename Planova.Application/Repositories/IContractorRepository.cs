using Planova.Domain.Entities;

namespace Planova.Application.Repositories;

public interface IContractorRepository
{
    Task<IEnumerable<Contractor>> GetAllAsync(CancellationToken ct = default);
    Task<Contractor?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Contractor> AddAsync(Contractor contractor, CancellationToken ct = default);
    Task UpdateAsync(Contractor contractor, CancellationToken ct = default);
    Task DeleteAsync(Contractor contractor, CancellationToken ct = default);
    Task<IEnumerable<Contractor>> SearchAsync(string query, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<bool> HasLinkedProjectsAsync(int contractorId, CancellationToken ct = default);
}