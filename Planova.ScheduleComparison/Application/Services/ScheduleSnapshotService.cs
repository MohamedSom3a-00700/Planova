using System.Text.Json;
using Planova.Activity.Domain.Interfaces;
using Planova.Resource.Domain.Interfaces;
using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.ScheduleComparison.Application.Services;

public class ScheduleSnapshotService : IScheduleSnapshotService
{
    private readonly IComparisonRepository _repository;
    private readonly IActivityService _activityService;
    private readonly IActivityRelationshipService _relationshipService;
    private readonly IResourceAssignmentService _resourceAssignmentService;
    private readonly ICurrentProjectService _currentProjectService;

    public ScheduleSnapshotService(
        IComparisonRepository repository,
        IActivityService activityService,
        IActivityRelationshipService relationshipService,
        IResourceAssignmentService resourceAssignmentService,
        ICurrentProjectService currentProjectService)
    {
        _repository = repository;
        _activityService = activityService;
        _relationshipService = relationshipService;
        _resourceAssignmentService = resourceAssignmentService;
        _currentProjectService = currentProjectService;
    }

    public async Task<ScheduleSnapshot> CaptureSnapshotAsync(int projectId, string label, CancellationToken ct = default)
    {
        var scheduleData = await BuildScheduleDataAsync(projectId, ct);

        var snapshot = new ScheduleSnapshot
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Label = label,
            SnapshotData = JsonSerializer.Serialize(scheduleData),
            CapturedAt = DateTime.UtcNow,
            ActivityCount = scheduleData.Activities.Count,
            RelationshipCount = scheduleData.Relationships.Count
        };

        await _repository.AddSnapshotAsync(snapshot, ct);
        return snapshot;
    }

    public async Task<ScheduleSnapshot?> RestoreSnapshotAsync(Guid snapshotId, CancellationToken ct = default)
    {
        return await _repository.GetSnapshotByIdAsync(snapshotId, ct);
    }

    public async Task<List<ScheduleSnapshot>> ListSnapshotsAsync(int projectId, CancellationToken ct = default)
    {
        return await _repository.GetSnapshotsByProjectAsync(projectId, ct);
    }

    public async Task DeleteSnapshotAsync(Guid snapshotId, CancellationToken ct = default)
    {
        var snapshot = await _repository.GetSnapshotByIdAsync(snapshotId, ct);
        if (snapshot != null)
        {
            await _repository.DeleteSnapshotAsync(snapshot, ct);
        }
    }

    public async Task<ScheduleData?> DeserializeSnapshotDataAsync(ScheduleSnapshot snapshot, CancellationToken ct = default)
    {
        return JsonSerializer.Deserialize<ScheduleData>(snapshot.SnapshotData);
    }

    private async Task<ScheduleData> BuildScheduleDataAsync(int projectId, CancellationToken ct)
    {
        var activities = await _activityService.GetByProjectAsync(projectId, null, ct);
        var relationships = await _relationshipService.GetByProjectAsync(projectId, ct);
        var assignments = await _resourceAssignmentService.GetByProjectAsync(projectId, ct);

        var scheduleData = new ScheduleData();
        var activityLookup = new Dictionary<Guid, string>();

        foreach (var act in activities)
        {
            var activityModel = new ScheduleActivity
            {
                ProvenanceId = act.Id.ToString(),
                ActivityId = act.Code,
                WbsCode = act.WbsItemCode,
                Name = act.Name,
                Status = act.Status,
                Start = act.PlannedStart,
                Finish = act.PlannedFinish,
                Duration = act.Duration.HasValue ? (double)act.Duration.Value : null,
                PercentComplete = act.PercentComplete.HasValue ? (double)act.PercentComplete.Value : null,
                ActualStart = act.ActualStart,
                ActualFinish = act.ActualFinish,
                Calendar = act.CalendarName,
                CalendarId = act.CalendarId?.ToString()
            };

            activityLookup[act.Id] = activityModel.ProvenanceId;
            scheduleData.Activities.Add(activityModel);
        }

        foreach (var rel in relationships)
        {
            var relationshipModel = new ScheduleRelationship
            {
                PredecessorProvenanceId = rel.PredecessorId.ToString(),
                PredecessorActivityId = rel.PredecessorCode,
                SuccessorProvenanceId = rel.SuccessorId.ToString(),
                SuccessorActivityId = rel.SuccessorCode,
                RelationshipType = rel.Type,
                Lag = (double)rel.LagDays
            };

            scheduleData.Relationships.Add(relationshipModel);
        }

        foreach (var assignment in assignments)
        {
            var assignmentModel = new ScheduleResourceAssignment
            {
                ActivityMatchKey = assignment.ActivityId.ToString(),
                ResourceId = assignment.ResourceCode,
                ResourceName = assignment.ResourceName,
                Role = assignment.ResourceType.ToString(),
                Units = (double)assignment.Quantity,
                Cost = assignment.TotalCost
            };

            scheduleData.ResourceAssignments.Add(assignmentModel);
        }

        return scheduleData;
    }
}
