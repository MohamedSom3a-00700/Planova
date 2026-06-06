using Planova.Cost.Application.Dto;

namespace Planova.Cost.Domain.Interfaces;

public interface ICostAiService
{
    Task<AiSuggestionDto> EstimateCostAsync(Guid activityId, CancellationToken ct = default);
    Task<List<CostAnomalyDto>> DetectAnomaliesAsync(int projectId, CancellationToken ct = default);
    Task<AiForecastDto> ForecastEacAsync(int projectId, CancellationToken ct = default);
    Task<string> GenerateNarrativeAsync(int projectId, CancellationToken ct = default);
    bool IsAvailable { get; }
}
