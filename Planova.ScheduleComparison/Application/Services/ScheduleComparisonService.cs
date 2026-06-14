using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Mappings;
using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Enums;
using Planova.Primavera.Domain.Interfaces;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.ScheduleComparison.Application.Services;

public class ScheduleComparisonService : IScheduleComparisonService
{
    private readonly IComparisonRepository _repository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ScheduleSnapshotService _snapshotService;

    public ScheduleComparisonService(
        IComparisonRepository repository,
        IServiceProvider serviceProvider,
        ScheduleSnapshotService snapshotService)
    {
        _repository = repository;
        _serviceProvider = serviceProvider;
        _snapshotService = snapshotService;
    }

    public async Task<ComparisonSession> CompareAsync(
        int projectId,
        Guid? sourceSnapshotId,
        Guid? targetSnapshotId,
        string sourceKind,
        string targetKind,
        string sourceLabel,
        string targetLabel,
        List<ComparisonScope> scopes,
        Guid? ruleId = null,
        CancellationToken ct = default)
    {
        var session = new ComparisonSession
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Mode = DetermineMode(sourceKind, targetKind),
            State = SessionState.Draft,
            SourceKind = sourceKind,
            SourceSnapshotId = sourceSnapshotId,
            SourceLabel = sourceLabel,
            TargetKind = targetKind,
            TargetSnapshotId = targetSnapshotId,
            TargetLabel = targetLabel,
            IncludedScopes = string.Join(",", scopes.Select(s => s.ToString())),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddSessionAsync(session, ct);

        session.State = SessionState.Running;
        session.StartedAt = DateTime.UtcNow;
        await _repository.UpdateSessionAsync(session, ct);

        try
        {
            var sourceData = await ResolveScheduleDataAsync(projectId, sourceKind, sourceSnapshotId, ct);
            var targetData = await ResolveScheduleDataAsync(projectId, targetKind, targetSnapshotId, ct);

            if (sourceData == null)
                throw new InvalidOperationException("Source schedule data could not be resolved.");
            if (targetData == null)
                throw new InvalidOperationException("Target schedule data could not be resolved.");

            var activityDiffs = new List<ActivityDiff>();
            var logicDiffs = new List<LogicDiff>();
            var resourceDiffs = new List<ResourceDiff>();
            CriticalPathDiff? criticalPathDiff = null;
            FloatImpactReport? floatReport = null;

            foreach (var scope in scopes)
            {
                switch (scope)
                {
                    case ComparisonScope.Activities:
                        activityDiffs = new ActivityComparer().Compare(sourceData, targetData);
                        break;
                    case ComparisonScope.Logic:
                        logicDiffs = new LogicComparer().Compare(sourceData, targetData);
                        break;
                    case ComparisonScope.Resources:
                        resourceDiffs = new ResourceComparer().Compare(sourceData, targetData);
                        break;
                    case ComparisonScope.CriticalPath:
                        criticalPathDiff = new CriticalPathComparer().Compare(sourceData, targetData);
                        break;
                    case ComparisonScope.Float:
                        floatReport = new FloatComparer().Compare(sourceData, targetData);
                        break;
                }
            }

            var summary = BuildSummary(activityDiffs, logicDiffs, resourceDiffs, criticalPathDiff, floatReport);

            var result = new ScheduleComparisonResult
            {
                SchemaVersion = "1.0",
                SessionId = session.Id,
                ProjectId = projectId,
                Mode = session.Mode.ToString(),
                ComparedAt = DateTime.UtcNow,
                Source = new ComparisonSourceInfo
                {
                    SourceKind = sourceKind,
                    ProjectId = projectId,
                    SnapshotId = sourceSnapshotId,
                    Label = sourceLabel
                },
                Target = new ComparisonSourceInfo
                {
                    SourceKind = targetKind,
                    ProjectId = projectId,
                    SnapshotId = targetSnapshotId,
                    Label = targetLabel
                },
                IncludedScopes = scopes.Select(s => s.ToString()).ToList(),
                GeneratedByVersion = "10.0.0.0",
                ActivityDiffs = activityDiffs,
                LogicDiffs = logicDiffs,
                ResourceDiffs = resourceDiffs,
                CriticalPathDiffResult = criticalPathDiff,
                FloatReport = floatReport,
                Summary = summary
            };

            var resultRows = result.ToEntityRows(session.Id, activityDiffs, logicDiffs, resourceDiffs);
            if (resultRows.Count > 0)
            {
                await _repository.AddResultsAsync(resultRows, ct);
            }

            session.ResultJson = JsonSerializer.Serialize(result);
            session.State = SessionState.Completed;
            session.CompletedAt = DateTime.UtcNow;
            await _repository.UpdateSessionAsync(session, ct);

            return session;
        }
        catch (Exception ex)
        {
            session.State = SessionState.Failed;
            session.Error = ex.Message;
            session.CompletedAt = DateTime.UtcNow;
            await _repository.UpdateSessionAsync(session, ct);
            throw;
        }
    }

    public async Task<ComparisonSession?> GetSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await _repository.GetSessionByIdAsync(sessionId, ct);
    }

    public async Task<List<ComparisonSession>> ListSessionsAsync(int projectId, CancellationToken ct = default)
    {
        return await _repository.GetSessionsByProjectAsync(projectId, ct);
    }

    public async Task ReOpenSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId, ct);
        if (session == null)
            throw new InvalidOperationException($"Session {sessionId} not found.");

        if (session.State != SessionState.Completed)
            throw new InvalidOperationException($"Session {sessionId} is not in a completed state.");

        // Session data is loaded from ResultJson on the existing entity.
        // No state change needed for re-open — the UI reads the stored data.
    }

    public async Task SoftDeleteSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId, ct);
        if (session == null)
            throw new InvalidOperationException($"Session {sessionId} not found.");

        session.State = SessionState.Cancelled;
        await _repository.UpdateSessionAsync(session, ct);
    }

    private async Task<ScheduleData?> ResolveScheduleDataAsync(
        int projectId, string kind, Guid? snapshotId, CancellationToken ct)
    {
        if (snapshotId.HasValue)
        {
            var snapshot = await _repository.GetSnapshotByIdAsync(snapshotId.Value, ct);
            if (snapshot != null)
            {
                return await _snapshotService.DeserializeSnapshotDataAsync(snapshot, ct);
            }
        }

        if (kind == "Primavera")
        {
            var primaveraService = ResolvePrimavera();
            if (primaveraService != null)
            {
                return await MapPrimaveraSnapshotToScheduleDataAsync(projectId, ct);
            }
        }

        return await BuildNativeScheduleDataAsync(projectId, ct);
    }

    private IPrimaveraWorkspaceService? ResolvePrimavera()
    {
        return _serviceProvider.GetService<IPrimaveraWorkspaceService>();
    }

    private async Task<ScheduleData> BuildNativeScheduleDataAsync(int projectId, CancellationToken ct)
    {
        var snapshot = await _snapshotService.CaptureSnapshotAsync(projectId, "__internal_comparison__", ct);
        return await _snapshotService.DeserializeSnapshotDataAsync(snapshot, ct)
               ?? new ScheduleData();
    }

    private async Task<ScheduleData> MapPrimaveraSnapshotToScheduleDataAsync(int projectId, CancellationToken ct)
    {
        var primaveraService = ResolvePrimavera();
        if (primaveraService == null)
            throw new InvalidOperationException("Primavera workspace service is not available.");

        var snapshot = await primaveraService.GetSnapshotAsync(projectId, ct);

        var data = new ScheduleData();

        foreach (var pa in snapshot.Activities)
        {
            data.Activities.Add(new ScheduleActivity
            {
                ProvenanceId = pa.Id.ToString(),
                ActivityId = pa.TaskId,
                WbsCode = pa.WbsId,
                Name = pa.Name,
                Status = pa.Status,
                Start = pa.StartDate,
                Finish = pa.EndDate,
                Duration = pa.Duration,
                RemainingDuration = pa.RemainingDuration,
                PercentComplete = pa.PercentComplete,
                CalendarId = pa.CalendarId
            });
        }

        foreach (var pr in snapshot.Relationships)
        {
            data.Relationships.Add(new ScheduleRelationship
            {
                PredecessorActivityId = pr.PredTaskId,
                SuccessorActivityId = pr.SuccTaskId,
                RelationshipType = pr.Type,
                Lag = pr.LagDuration
            });
        }

        foreach (var ra in snapshot.ResourceAssignments)
        {
            data.ResourceAssignments.Add(new ScheduleResourceAssignment
            {
                ActivityProvenanceId = ra.TaskId,
                ResourceId = ra.ResourceId,
                Units = ra.Units,
                Cost = ra.CostPerUnit
            });
        }

        return data;
    }

    private static ComparisonMode DetermineMode(string sourceKind, string targetKind)
    {
        if (sourceKind == "Primavera" && targetKind == "Primavera")
            return ComparisonMode.XerVsXer;

        if (sourceKind == "Snapshot" && targetKind == "Snapshot")
            return ComparisonMode.UpdateVsUpdate;

        if (sourceKind == "Baseline" || targetKind == "Baseline")
            return ComparisonMode.BaselineVsUpdate;

        return ComparisonMode.AsPlannedVsAsBuilt;
    }

    private static ComparisonSummary BuildSummary(
        List<ActivityDiff> activityDiffs,
        List<LogicDiff> logicDiffs,
        List<ResourceDiff> resourceDiffs,
        CriticalPathDiff? criticalPathDiff,
        FloatImpactReport? floatReport)
    {
        var summary = new ComparisonSummary
        {
            TotalActivities = activityDiffs.Count,
            AddedActivities = activityDiffs.Count(d => d.ChangeType == ChangeType.Added.ToString()),
            RemovedActivities = activityDiffs.Count(d => d.ChangeType == ChangeType.Removed.ToString()),
            ModifiedActivities = activityDiffs.Count(d => d.ChangeType == ChangeType.Modified.ToString()),
            TotalRelationships = logicDiffs.Count,
            AddedRelationships = logicDiffs.Count(d => d.ChangeType == ChangeType.Added.ToString()),
            RemovedRelationships = logicDiffs.Count(d => d.ChangeType == ChangeType.Removed.ToString()),
            ModifiedRelationships = logicDiffs.Count(d => d.ChangeType == ChangeType.Modified.ToString()),
            TotalResourceAssignments = resourceDiffs.Count,
            AddedAssignments = resourceDiffs.Count(d => d.ChangeType == ChangeType.Added.ToString()),
            RemovedAssignments = resourceDiffs.Count(d => d.ChangeType == ChangeType.Removed.ToString()),
            ModifiedAssignments = resourceDiffs.Count(d => d.ChangeType == ChangeType.Modified.ToString()),
            CriticalPathDurationDelta = criticalPathDiff?.DurationChange,
            ActivitiesWithFloatLoss = floatReport?.ActivitiesWithWorsenedFloat.Count ?? 0,
            ActivitiesWithFloatGain = floatReport?.ActivitiesWithImprovedFloat.Count ?? 0
        };

        return summary;
    }
}
