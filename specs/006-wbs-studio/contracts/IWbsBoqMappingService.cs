// IWbsBoqMappingService — BOQ-to-WBS mapping engine
// Implemented by Planova.Wbs.Application.Services

public interface IWbsBoqMappingService
{
    Task<WbsMappingResult> MapOneToOneAsync(Guid boqId, CancellationToken ct);
    Task<WbsMappingResult> MapGroupedAsync(Guid boqId, CancellationToken ct);
    Task<WbsMappingResult> MapCustomAsync(Guid boqId, IReadOnlyList<ManualMapping> mappings, CancellationToken ct);
    Task<Wbs> CommitMappingAsync(WbsMappingResult result, CancellationToken ct);
}
