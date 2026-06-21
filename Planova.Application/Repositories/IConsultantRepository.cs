using Planova.Domain.Entities;

namespace Planova.Application.Repositories;

public interface IConsultantRepository
{
    Task<IEnumerable<Consultant>> GetAllAsync(CancellationToken ct = default);
    Task<Consultant?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Consultant> AddAsync(Consultant consultant, CancellationToken ct = default);
    Task UpdateAsync(Consultant consultant, CancellationToken ct = default);
    Task DeleteAsync(Consultant consultant, CancellationToken ct = default);
    Task<IEnumerable<Consultant>> SearchAsync(string query, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<bool> HasLinkedProjectsAsync(int consultantId, CancellationToken ct = default);
}
