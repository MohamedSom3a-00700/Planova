using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceService
{
    Task<ResourceDto> CreateAsync(CreateResourceRequest request, CancellationToken ct = default);
    Task<ResourceDto> UpdateAsync(UpdateResourceRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task ReactivateAsync(Guid id, CancellationToken ct = default);
    Task<ResourceDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ResourceDto>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<List<ResourceDto>> SearchAsync(ResourceFilter filter, CancellationToken ct = default);
    Task<ResourceDuplicateCheckResult> CheckDuplicateNameAsync(string name, ResourceScope scope, int? projectId, Guid? excludeId = null, CancellationToken ct = default);
}
