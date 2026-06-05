using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;

using ActivityEntity = Planova.Activity.Domain.Entities.Activity;

namespace Planova.Activity.Application.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepository;
    private readonly IActivityRelationshipRepository _relationshipRepository;

    public ActivityService(IActivityRepository activityRepository, IActivityRelationshipRepository relationshipRepository)
    {
        _activityRepository = activityRepository;
        _relationshipRepository = relationshipRepository;
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityRequest request, CancellationToken ct = default)
    {
        var code = await _activityRepository.GetNextCodeAsync(request.ProjectId, ct);

        var activity = new ActivityEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ParentActivityId = request.ParentActivityId,
            WbsItemId = request.WbsItemId,
            CalendarId = request.CalendarId,
            Code = code,
            Name = request.Name,
            Description = request.Description,
            ActivityType = Enum.Parse<ActivityType>(request.ActivityType),
            Status = ActivityStatus.NotStarted,
            Duration = request.Duration,
            PlannedStart = request.PlannedStart,
            PlannedFinish = request.PlannedFinish,
            Weight = request.Weight,
            Notes = request.Notes,
            SortOrder = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _activityRepository.AddAsync(activity, ct);
        return MapToDto(activity);
    }

    public async Task<ActivityDto> UpdateAsync(UpdateActivityRequest request, CancellationToken ct = default)
    {
        var activity = await _activityRepository.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Activity {request.Id} not found");

        var plannedStart = request.PlannedStart ?? activity.PlannedStart;
        var plannedFinish = request.PlannedFinish ?? activity.PlannedFinish;

        if (plannedStart.HasValue && plannedFinish.HasValue && plannedStart > plannedFinish)
            throw new InvalidOperationException("PlannedStart must be before PlannedFinish");

        if (request.Duration.HasValue && request.Duration <= 0 &&
            activity.ActivityType != ActivityType.Milestone &&
            activity.ActivityType != ActivityType.WbsSummary)
            throw new InvalidOperationException($"Duration must be > 0 for {activity.ActivityType} activities");

        if (request.Name is not null) activity.Name = request.Name;
        if (request.Description is not null) activity.Description = request.Description;
        if (request.Duration.HasValue) activity.Duration = request.Duration;
        if (request.PlannedStart.HasValue) activity.PlannedStart = request.PlannedStart;
        if (request.PlannedFinish.HasValue) activity.PlannedFinish = request.PlannedFinish;
        if (request.ActualStart.HasValue) activity.ActualStart = request.ActualStart;
        if (request.ActualFinish.HasValue) activity.ActualFinish = request.ActualFinish;
        if (request.PercentComplete.HasValue) activity.PercentComplete = request.PercentComplete;
        if (request.Weight.HasValue) activity.Weight = request.Weight;
        if (request.Notes is not null) activity.Notes = request.Notes;
        if (request.ParentActivityId is not null) activity.ParentActivityId = request.ParentActivityId;
        if (request.WbsItemId is not null) activity.WbsItemId = request.WbsItemId;
        if (request.CalendarId is not null) activity.CalendarId = request.CalendarId;

        activity.UpdatedAt = DateTime.UtcNow;
        await _activityRepository.UpdateAsync(activity, ct);
        return MapToDto(activity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var relationships = await _relationshipRepository.GetByActivityIdAsync(id, ct);
        foreach (var rel in relationships)
        {
            await _relationshipRepository.DeleteAsync(rel.Id, ct);
        }
        await _activityRepository.DeleteAsync(id, ct);
    }

    public async Task<ActivityDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, ct);
        return activity is not null ? MapToDto(activity) : null;
    }

    public async Task<List<ActivityDto>> GetByProjectAsync(int projectId, ActivityFilter? filter = null, CancellationToken ct = default)
    {
        List<ActivityEntity> activities;

        if (filter?.Status is not null)
        {
            var status = Enum.Parse<ActivityStatus>(filter.Status);
            activities = await _activityRepository.GetByStatusAsync(projectId, status, ct);
        }
        else
        {
            activities = await _activityRepository.GetByProjectIdAsync(projectId, ct);
        }

        if (filter?.WbsItemId is not null)
        {
            activities = activities.Where(a => a.WbsItemId == filter.WbsItemId).ToList();
        }

        if (!string.IsNullOrWhiteSpace(filter?.SearchText))
        {
            var search = filter.SearchText.ToLowerInvariant();
            activities = activities.Where(a =>
                a.Name.ToLowerInvariant().Contains(search) ||
                a.Code.ToLowerInvariant().Contains(search)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(filter?.ActivityType))
        {
            activities = activities.Where(a => a.ActivityType.ToString() == filter.ActivityType).ToList();
        }

        return activities.Select(MapToDto).ToList();
    }

    public async Task<List<ActivityDto>> GetByWbsItemAsync(Guid wbsItemId, CancellationToken ct = default)
    {
        var activities = await _activityRepository.GetByWbsItemIdAsync(wbsItemId, ct);
        return activities.Select(MapToDto).ToList();
    }

    public async Task<ActivityDto> ChangeStatusAsync(Guid id, ActivityStatus newStatus, string? reason = null, CancellationToken ct = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Activity {id} not found");

        if (!ActivityStatusTransitions.IsValid(activity.Status, newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {activity.Status} to {newStatus}");
        }

        activity.Status = newStatus;
        activity.UpdatedAt = DateTime.UtcNow;
        await _activityRepository.UpdateAsync(activity, ct);
        return MapToDto(activity);
    }

    public async Task<List<ActivityDto>> GetWbsSummaryChildrenAsync(Guid parentActivityId, CancellationToken ct = default)
    {
        var children = await _activityRepository.GetChildrenAsync(parentActivityId, ct);
        return children.Select(MapToDto).ToList();
    }

    public async Task RecalculateWbsSummaryAsync(Guid activityId, CancellationToken ct = default)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId, ct);
        if (activity is null || activity.ActivityType != ActivityType.WbsSummary) return;

        var children = await _activityRepository.GetChildrenAsync(activityId, ct);
        if (children.Count == 0) return;

        activity.PlannedStart = children.Min(c => c.PlannedStart);
        activity.PlannedFinish = children.Max(c => c.PlannedFinish);
        activity.Duration = children.Sum(c => c.Duration);
        activity.PercentComplete = children.Average(c => c.PercentComplete);
        activity.UpdatedAt = DateTime.UtcNow;

        await _activityRepository.UpdateAsync(activity, ct);
    }

    public async Task BulkStatusChangeAsync(List<Guid> ids, ActivityStatus newStatus, CancellationToken ct = default)
    {
        foreach (var id in ids)
        {
            await ChangeStatusAsync(id, newStatus, ct: ct);
        }
    }

    public async Task BulkDeleteAsync(List<Guid> ids, CancellationToken ct = default)
    {
        foreach (var id in ids)
        {
            await DeleteAsync(id, ct);
        }
    }

    public async Task BulkCalendarAssignAsync(List<Guid> ids, Guid calendarId, CancellationToken ct = default)
    {
        foreach (var id in ids)
        {
            var activity = await _activityRepository.GetByIdAsync(id, ct);
            if (activity is null) continue;
            activity.CalendarId = calendarId;
            activity.UpdatedAt = DateTime.UtcNow;
            await _activityRepository.UpdateAsync(activity, ct);
        }
    }

    private static ActivityDto MapToDto(ActivityEntity activity) => new()
    {
        Id = activity.Id,
        ProjectId = activity.ProjectId,
        ParentActivityId = activity.ParentActivityId,
        WbsItemId = activity.WbsItemId,
        CalendarId = activity.CalendarId,
        Code = activity.Code,
        Name = activity.Name,
        Description = activity.Description,
        ActivityType = activity.ActivityType.ToString(),
        Status = activity.Status.ToString(),
        Duration = activity.Duration,
        PlannedStart = activity.PlannedStart,
        PlannedFinish = activity.PlannedFinish,
        ActualStart = activity.ActualStart,
        ActualFinish = activity.ActualFinish,
        PercentComplete = activity.PercentComplete,
        Weight = activity.Weight,
        Notes = activity.Notes,
        SortOrder = activity.SortOrder,
        IsActive = activity.IsActive,
        CreatedAt = activity.CreatedAt,
        UpdatedAt = activity.UpdatedAt
    };
}
