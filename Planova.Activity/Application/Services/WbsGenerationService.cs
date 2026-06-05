using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Application.Services;

public class WbsGenerationService : IWbsGenerationService
{
    private readonly IActivityService _activityService;
    private readonly IActivityBankService _bankService;
    private readonly IActivityRepository _activityRepository;

    public WbsGenerationService(
        IActivityService activityService,
        IActivityBankService bankService,
        IActivityRepository activityRepository)
    {
        _activityService = activityService;
        _bankService = bankService;
        _activityRepository = activityRepository;
    }

    public async Task<WbsGenerationPreviewDto> PreviewSimpleGenerationAsync(List<Guid> wbsItemIds, CancellationToken ct = default)
    {
        var previews = wbsItemIds.Select((id, index) => new ActivityPreviewItem
        {
            WbsItemId = id,
            WbsItemName = $"WBS Item {index + 1}",
            Code = $"A-{index + 1:D3}",
            Name = $"Activity for WBS {index + 1}",
            Type = "Task",
            Duration = 5,
            IsNew = true
        }).ToList();

        return new WbsGenerationPreviewDto
        {
            WbsItemIds = wbsItemIds,
            TotalActivities = previews.Count,
            Previews = previews,
            HasExistingActivities = false,
            ExistingActivityCount = 0
        };
    }

    public async Task<WbsGenerationPreviewDto> PreviewBankGenerationAsync(List<Guid> wbsItemIds, Guid bankId, CancellationToken ct = default)
    {
        var bank = await _bankService.GetByIdAsync(bankId, ct);
        var previews = new List<ActivityPreviewItem>();

        foreach (var wbsItemId in wbsItemIds)
        {
            foreach (var item in bank.Items)
            {
                previews.Add(new ActivityPreviewItem
                {
                    WbsItemId = wbsItemId,
                    WbsItemName = $"WBS {wbsItemId}",
                    Code = item.Code,
                    Name = item.Name,
                    Type = item.DefaultActivityType,
                    Duration = item.DefaultDuration,
                    IsNew = true
                });
            }
        }

        return new WbsGenerationPreviewDto
        {
            WbsItemIds = wbsItemIds,
            TotalActivities = previews.Count,
            Previews = previews,
            HasExistingActivities = false,
            ExistingActivityCount = 0
        };
    }

    public async Task<List<ActivityDto>> CommitGenerationAsync(WbsGenerationRequest request, CancellationToken ct = default)
    {
        var createdActivities = new List<ActivityDto>();

        foreach (var wbsItemId in request.WbsItemIds)
        {
            var existingForWbs = await _activityRepository.GetByWbsItemIdAsync(wbsItemId, ct);

            if (existingForWbs.Count > 0 && request.ReplaceExisting)
            {
                foreach (var existing in existingForWbs)
                {
                    await _activityRepository.DeleteAsync(existing.Id, ct);
                }
            }

            if (existingForWbs.Count > 0 && !request.ReplaceExisting && !request.MergeExisting)
                continue;

            var dto = await _activityService.CreateAsync(new CreateActivityRequest
            {
                ProjectId = request.ProjectId,
                WbsItemId = wbsItemId,
                Name = $"Activity for WBS {wbsItemId}",
                ActivityType = "Task",
                Duration = 5
            }, ct);
            createdActivities.Add(dto);
        }

        return createdActivities;
    }

    public async Task<WbsGenerationPreviewDto> ApplyBankToWbsAsync(Guid bankId, List<Guid> wbsItemIds, int projectId, CancellationToken ct = default)
    {
        var bankDto = await _bankService.GetByIdAsync(bankId, ct);
        var previews = new List<ActivityPreviewItem>();

        foreach (var wbsItemId in wbsItemIds)
        {
            foreach (var item in bankDto.Items)
            {
                var existingActivities = await _activityRepository.GetByWbsItemIdAsync(wbsItemId, ct);

                previews.Add(new ActivityPreviewItem
                {
                    WbsItemId = wbsItemId,
                    WbsItemName = $"WBS Item {wbsItemId}",
                    Code = $"{wbsItemId.ToString("N").Substring(0, 4)}-{item.Code}",
                    Name = item.Name,
                    Type = item.DefaultActivityType,
                    Duration = item.DefaultDuration,
                    IsNew = !existingActivities.Any(e => e.Name == item.Name)
                });
            }
        }

        var allExisting = await Task.WhenAll(wbsItemIds.Select(id => _activityRepository.GetByWbsItemIdAsync(id, ct)));

        return new WbsGenerationPreviewDto
        {
            WbsItemIds = wbsItemIds,
            TotalActivities = previews.Count,
            Previews = previews,
            HasExistingActivities = allExisting.Any(e => e.Count > 0),
            ExistingActivityCount = allExisting.Sum(e => e.Count)
        };
    }
}
