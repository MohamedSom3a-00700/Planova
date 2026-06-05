using Planova.Resource.Domain.Enums;
using Res = Planova.Resource.Domain.Entities.Resource;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceRepository
{
    Task<Res?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Res>> GetByProjectAsync(int projectId, ResourceScope? scope = null, ResourceType? type = null, CancellationToken ct = default);
    Task<List<Res>> SearchAsync(string query, ResourceType? type = null, ResourceScope? scope = null, int? projectId = null, CancellationToken ct = default);
    Task<List<Res>> GetByTypeAsync(ResourceType type, ResourceScope? scope = null, int? projectId = null, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, ResourceScope scope, int? projectId = null, CancellationToken ct = default);
    Task<string> GenerateNextCodeAsync(ResourceType type, CancellationToken ct = default);
    Task<bool> HasDuplicateNameAsync(string name, ResourceScope scope, int? projectId = null, Guid? excludeId = null, CancellationToken ct = default);
    Task AddAsync(Res resource, CancellationToken ct = default);
    Task UpdateAsync(Res resource, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetCountAsync(ResourceType? type = null, ResourceScope? scope = null, CancellationToken ct = default);
}
