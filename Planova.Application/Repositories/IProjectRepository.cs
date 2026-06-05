using Planova.Domain.Entities;

namespace Planova.Application.Repositories;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Project> AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task DeleteAsync(Project project, CancellationToken ct = default);
    Task<IEnumerable<Project>> SearchAsync(string query, CancellationToken ct = default);
    Task<IEnumerable<Project>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
}
