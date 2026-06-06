using Planova.Domain.Entities;

namespace Planova.Application.Repositories;

public interface ISubcontractorRepository
{
    Task<IEnumerable<Subcontractor>> GetAllAsync(CancellationToken ct = default);
    Task<Subcontractor?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Subcontractor> AddAsync(Subcontractor subcontractor, CancellationToken ct = default);
    Task UpdateAsync(Subcontractor subcontractor, CancellationToken ct = default);
    Task DeleteAsync(Subcontractor subcontractor, CancellationToken ct = default);
    Task<IEnumerable<Subcontractor>> SearchAsync(string query, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<bool> HasLinkedProjectsAsync(int subcontractorId, CancellationToken ct = default);
}