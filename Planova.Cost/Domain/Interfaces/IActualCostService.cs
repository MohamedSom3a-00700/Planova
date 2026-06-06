using Planova.Cost.Application.Dto;

namespace Planova.Cost.Domain.Interfaces;

public interface IActualCostService
{
    Task<ActualCostDto?> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task<ActualCostDto> SetAsync(Guid activityId, decimal amount, string currency, CancellationToken ct = default);
    Task<ImportResultDto> ImportFromExcelAsync(Stream excelStream, int projectId, CancellationToken ct = default);
    Task<List<ActivityVarianceDto>> GetVarianceByProjectAsync(int projectId, CancellationToken ct = default);
    Task MarkOrphanedByActivityIdAsync(Guid activityId, CancellationToken ct = default);
}
