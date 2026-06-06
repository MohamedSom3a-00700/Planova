using Planova.Cost.Application.Dto;

namespace Planova.Cost.Domain.Interfaces;

public interface IEvmService
{
    Task<EvmMetricsDto> ComputeMetricsAsync(
        int projectId, DateTime dataDate, CancellationToken ct = default);
    Task<List<ActivityEvmDto>> GetActivityDetailAsync(
        int projectId, DateTime dataDate, CancellationToken ct = default);
}
