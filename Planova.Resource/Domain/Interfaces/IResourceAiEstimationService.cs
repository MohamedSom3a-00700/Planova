using Planova.Resource.Application.Dto;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceAiEstimationService
{
    Task<List<AiSuggestionDto>> EstimateResourcesAsync(Guid activityId, CancellationToken ct = default);
    Task<List<ResourceAssignmentDto>> AcceptSuggestionsAsync(Guid activityId, List<AcceptedSuggestionDto> suggestions, CancellationToken ct = default);
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}
