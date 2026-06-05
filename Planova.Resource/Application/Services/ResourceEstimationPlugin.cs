using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Resource.Application.Services;

public sealed class ResourceEstimationPlugin
{
    private readonly IResourceRepository _resourceRepository;
    private readonly ILogger<ResourceEstimationPlugin> _logger;

    public ResourceEstimationPlugin(
        IResourceRepository resourceRepository,
        ILogger<ResourceEstimationPlugin> logger)
    {
        _resourceRepository = resourceRepository;
        _logger = logger;
    }

    [KernelFunction("SuggestResources")]
    [Description("Suggests resources (labour, equipment, material, subcontractor) for a construction activity based on its description and WBS category")]
    public async Task<string> SuggestResourcesAsync(
        [Description("The activity name")] string activityName,
        [Description("The activity description")] string activityDescription,
        [Description("The WBS category or work breakdown structure code")] string wbsCategory,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resources = await _resourceRepository.SearchAsync(string.Empty, null, null, null, cancellationToken);
            var resourceContext = resources.Select(r => new
            {
                r.Id,
                r.Code,
                r.Name,
                Type = r.ResourceType.ToString(),
                r.UnitOfMeasure,
                r.DefaultRate,
                r.Trade,
                r.SkillLevel,
                r.EquipmentType,
                r.Capacity
            });

            var context = new
            {
                ActivityName = activityName,
                ActivityDescription = activityDescription,
                WbsCategory = wbsCategory,
                AvailableResources = resourceContext
            };

            var json = JsonSerializer.Serialize(context, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build resource context for AI estimation");
            return "[]";
        }
    }
}
