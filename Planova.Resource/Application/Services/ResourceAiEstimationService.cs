using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Planova.Resource.Application.Dto;
using Planova.Resource.Application.Mappings;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Resource.Application.Services;

public class ResourceAiEstimationService : IResourceAiEstimationService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IResourceAssignmentRepository _assignmentRepository;
    private readonly IResourceRateRepository _rateRepository;
    private readonly ILogger<ResourceAiEstimationService> _logger;
    private readonly Kernel? _kernel;

    public ResourceAiEstimationService(
        IResourceRepository resourceRepository,
        IResourceAssignmentRepository assignmentRepository,
        IResourceRateRepository rateRepository,
        ILogger<ResourceAiEstimationService> logger,
        Kernel? kernel = null)
    {
        _resourceRepository = resourceRepository;
        _assignmentRepository = assignmentRepository;
        _rateRepository = rateRepository;
        _logger = logger;
        _kernel = kernel;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        try
        {
            return _kernel is not null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<AiSuggestionDto>> EstimateResourcesAsync(Guid activityId, CancellationToken ct = default)
    {
        if (_kernel is null)
        {
            _logger.LogWarning("AI estimation unavailable: Semantic Kernel not configured");
            return [];
        }

        try
        {
            var resources = await _resourceRepository.SearchAsync(string.Empty, null, null, null, ct);
            var resourceContext = string.Join("\n",
                resources.Select(r => $"  - {r.Code}: {r.Name} ({r.ResourceType}, {r.UnitOfMeasure}, ${r.DefaultRate}/hr)"));

            var prompt = $"""
You are a construction resource estimator. Given an activity, suggest appropriate resources.

Activity ID: {activityId}

Available resources:
{resourceContext}

Suggest a JSON array of resources needed. Each item must have: resourceCode, suggestedQuantity, unitOfMeasure.
Return ONLY valid JSON. No explanation.
""";

            var function = _kernel.CreateFunctionFromPrompt(prompt);
            var response = await _kernel.InvokeAsync(function, cancellationToken: ct);

            var result = response?.GetValue<string>() ?? "[]";

            try
            {
                var suggestions = System.Text.Json.JsonSerializer.Deserialize<List<AiSuggestionDto>>(result,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return suggestions ?? [];
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse AI estimation response");
                return [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI estimation failed for activity {ActivityId}", activityId);
            return [];
        }
    }

    public async Task<List<ResourceAssignmentDto>> AcceptSuggestionsAsync(Guid activityId, List<AcceptedSuggestionDto> suggestions, CancellationToken ct = default)
    {
        var assignments = new List<ResourceAssignment>();

        foreach (var suggestion in suggestions)
        {
            var resources = await _resourceRepository.SearchAsync(suggestion.ResourceCode, null, null, null, ct);
            var resource = resources.FirstOrDefault();
            if (resource is null) continue;

            var rate = await _rateRepository.GetEffectiveRateAsync(resource.Id, DateTime.UtcNow, ct);

            var assignment = new ResourceAssignment
            {
                Id = Guid.NewGuid(),
                ActivityId = activityId,
                ResourceId = resource.Id,
                Quantity = suggestion.Quantity,
                Rate = rate?.Rate ?? resource.DefaultRate,
                Currency = resource.Currency,
                UnitOfMeasure = resource.UnitOfMeasure,
                TotalCost = suggestion.Quantity * (rate?.Rate ?? resource.DefaultRate),
                Notes = "AI estimated",
                CreatedAt = DateTime.UtcNow
            };

            assignments.Add(assignment);
        }

        await _assignmentRepository.AddRangeAsync(assignments, ct);
        return assignments.Select(a => a.ToDto()).ToList();
    }
}
