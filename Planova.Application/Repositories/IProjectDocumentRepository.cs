using Planova.Domain.Entities;

namespace Planova.Application.Repositories;

public interface IProjectDocumentRepository
{
    Task<IEnumerable<ProjectDocument>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<IEnumerable<ProjectDocument>> GetByProjectIdAndTypeAsync(int projectId, string documentType, CancellationToken ct = default);
    Task<ProjectDocument?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProjectDocument> AddAsync(ProjectDocument document, CancellationToken ct = default);
    Task DeleteAsync(ProjectDocument document, CancellationToken ct = default);
    Task<bool> PathExistsAsync(int projectId, string relativePath, CancellationToken ct = default);
    Task DeleteByProjectIdAsync(int projectId, CancellationToken ct = default);
}
