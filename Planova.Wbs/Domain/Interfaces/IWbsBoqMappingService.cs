using Planova.Wbs.Application.Dto;

namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsBoqMappingService
{
    Task<WbsMappingResult> MapOneToOneAsync(Guid boqId, CancellationToken ct);
    Task<WbsMappingResult> MapGroupedAsync(Guid boqId, CancellationToken ct);
    Task<WbsMappingResult> MapCustomAsync(Guid boqId, IReadOnlyList<ManualMapping> mappings, CancellationToken ct);
    Task<Entities.Wbs> CommitMappingAsync(WbsMappingResult result, string wbsName, int projectId, CancellationToken ct);
}
