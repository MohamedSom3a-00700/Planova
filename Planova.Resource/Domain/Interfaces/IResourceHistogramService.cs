using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceHistogramService
{
    Task<ResourceHistogramDto> GetHistogramAsync(int projectId, HistogramFilter filter, CancellationToken ct = default);
    Task<List<ResourceHistogramDto>> GetByResourceTypeAsync(int projectId, ResourceType type, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<List<ResourceHistogramDto>> GetByResourceAsync(int projectId, Guid resourceId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<byte[]> ExportHistogramDataAsync(int projectId, HistogramFilter filter, CancellationToken ct = default);
}
