using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Mappings;
using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Enums;
using Planova.Primavera.Application.Models;
using Planova.Primavera.Application.Services;
using Planova.Primavera.Domain.Interfaces;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.ScheduleComparison.Application.Services;

internal static class CompareTrace
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Planova");
    private static readonly string LogPath = Path.Combine(LogDir, "compare-trace.log");
    private static readonly object Lock = new();
    private static bool _dirCreated;
    [System.Diagnostics.Conditional("DEBUG")]
    internal static void Write(string msg)
    {
        lock (Lock)
        {
            try
            {
                if (!_dirCreated) { Directory.CreateDirectory(LogDir); _dirCreated = true; }
                System.IO.File.AppendAllText(LogPath, $"[{DateTime.UtcNow:HH:mm:ss.fff}] {msg}{Environment.NewLine}");
            }
            catch { }
        }
    }
}

public class ScheduleComparisonService : IScheduleComparisonService
{
    private readonly IComparisonRepository _repository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ScheduleSnapshotService _snapshotService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

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
        Guid? sourceImportSessionId = null,
        Guid? targetImportSessionId = null,
        CancellationToken ct = default)
    {
        CompareTrace.Write($"=== CompareAsync called ===");
        CompareTrace.Write($"projectId={projectId} sourceKind={sourceKind} targetKind={targetKind}");
        CompareTrace.Write($"sourceImportSessionId={sourceImportSessionId} targetImportSessionId={targetImportSessionId}");
        CompareTrace.Write($"sourceSnapshotId={sourceSnapshotId} targetSnapshotId={targetSnapshotId}");

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
            CompareTrace.Write("Resolving source data...");
            var sourceData = await ResolveScheduleDataAsync(projectId, sourceKind, sourceSnapshotId, sourceImportSessionId, ct);
            CompareTrace.Write($"Source data resolved: Activities={sourceData?.Activities.Count ?? -1} Relationships={sourceData?.Relationships.Count ?? -1}");

            CompareTrace.Write("Resolving target data...");
            var targetData = await ResolveScheduleDataAsync(projectId, targetKind, targetSnapshotId, targetImportSessionId, ct);
            CompareTrace.Write($"Target data resolved: Activities={targetData?.Activities.Count ?? -1} Relationships={targetData?.Relationships.Count ?? -1}");

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
                        CompareTrace.Write($"ActivityComparer returned {activityDiffs.Count} diffs");
                        break;
                    case ComparisonScope.Logic:
                        logicDiffs = new LogicComparer().Compare(sourceData, targetData);
                        CompareTrace.Write($"LogicComparer returned {logicDiffs.Count} diffs");
                        break;
                    case ComparisonScope.Resources:
                        resourceDiffs = new ResourceComparer().Compare(sourceData, targetData);
                        CompareTrace.Write($"ResourceComparer returned {resourceDiffs.Count} diffs");
                        break;
                    case ComparisonScope.CriticalPath:
                        criticalPathDiff = new CriticalPathComparer().Compare(sourceData, targetData);
                        CompareTrace.Write($"CriticalPathComparer returned {(criticalPathDiff != null ? $"durationChange={criticalPathDiff.DurationChange}" : "null")}");
                        break;
                    case ComparisonScope.Float:
                        floatReport = new FloatComparer().Compare(sourceData, targetData);
                        CompareTrace.Write($"FloatComparer returned {(floatReport != null ? $"deltas={floatReport.ActivityFloatDeltas.Count}" : "null")}");
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
            CompareTrace.Write($"Result rows to persist: {resultRows.Count}");
            if (resultRows.Count > 0)
            {
                await _repository.AddResultsAsync(resultRows, ct);
            }

            session.ResultJson = JsonSerializer.Serialize(result, JsonOptions);
            CompareTrace.Write($"ResultJson length: {session.ResultJson.Length}");
            session.State = SessionState.Completed;
            session.CompletedAt = DateTime.UtcNow;
            await _repository.UpdateSessionAsync(session, ct);

            CompareTrace.Write("=== CompareAsync completed successfully ===");
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

    public async Task ValidateCanReOpenAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId, ct);
        if (session == null)
            throw new InvalidOperationException($"Session {sessionId} not found.");

        if (session.State != SessionState.Completed)
            throw new InvalidOperationException($"Session {sessionId} is not in a completed state.");
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
        int projectId, string kind, Guid? snapshotId, Guid? importSessionId, CancellationToken ct)
    {
        CompareTrace.Write($"ResolveScheduleDataAsync: kind={kind} snapshotId={snapshotId} importSessionId={importSessionId}");

        if (snapshotId.HasValue && kind == "Snapshot")
        {
            CompareTrace.Write($"  -> PATH 1: Snapshot");
            var snapshot = await _repository.GetSnapshotByIdAsync(snapshotId.Value, ct);
            if (snapshot != null)
            {
                var data = await _snapshotService.DeserializeSnapshotDataAsync(snapshot, ct);
                CompareTrace.Write($"     Snapshot resolved: Activities={data?.Activities.Count ?? -1}");
                return data;
            }
            CompareTrace.Write($"     Snapshot not found by ID {snapshotId}");
        }

        if (kind == "XerImport" && importSessionId.HasValue)
        {
            CompareTrace.Write($"  -> PATH 2: XER Import (sessionId={importSessionId})");
            return await ResolveXerImportDataAsync(importSessionId.Value, ct);
        }

        if (kind == "Primavera")
        {
            CompareTrace.Write($"  -> PATH 3: Primavera");
            var primaveraService = ResolvePrimavera();
            if (primaveraService != null)
            {
                return await MapPrimaveraSnapshotToScheduleDataAsync(projectId, ct);
            }
            CompareTrace.Write($"     Primavera service not available");
        }

        CompareTrace.Write($"  -> PATH 4 (fallback): BuildNativeScheduleDataAsync");
        return await BuildNativeScheduleDataAsync(projectId, ct);
    }

    private async Task<ScheduleData> ResolveXerImportDataAsync(Guid importSessionId, CancellationToken ct)
    {
        CompareTrace.Write($"ResolveXerImportDataAsync: looking for session {importSessionId}");

        var importService = _serviceProvider.GetService<IPrimaveraImportService>();
        if (importService == null)
            throw new InvalidOperationException("XER import service is not available.");

        var session = await importService.GetSessionByIdAsync(importSessionId, ct);
        if (session == null)
        {
            CompareTrace.Write($"  Session {importSessionId} NOT FOUND in imported sessions!");
            throw new InvalidOperationException($"XER import session {importSessionId} not found.");
        }
        if (session.ParsedDataJson == null)
        {
            CompareTrace.Write($"  Session {importSessionId} found but ParsedDataJson is null!");
            throw new InvalidOperationException($"XER import session {importSessionId} has no parsed data.");
        }

        CompareTrace.Write($"  Session found, ParsedDataJson length: {session.ParsedDataJson.Length}");

        var storedData = JsonSerializer.Deserialize<XerStoredData>(session.ParsedDataJson, JsonOptions);
        if (storedData == null)
            throw new InvalidOperationException("Failed to deserialize XER import data.");

        CompareTrace.Write($"  Deserialized: Activities={storedData.Activities?.Count ?? -1} Relationships={storedData.Relationships?.Count ?? -1} Assignments={storedData.ResourceAssignments?.Count ?? -1}");

        var data = new ScheduleData();

        var taskIdToCode = storedData.Activities?
            .Where(a => !string.IsNullOrEmpty(a.TaskCode))
            .ToDictionary(a => a.TaskId, a => a.TaskCode!, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (storedData.Activities != null)
        {
            foreach (var a in storedData.Activities)
            {
                var stableId = taskIdToCode.GetValueOrDefault(a.TaskId, a.TaskId);
                data.Activities.Add(new ScheduleActivity
                {
                    ActivityId = stableId,
                    ActivityCode = a.TaskCode,
                    WbsCode = a.WbsId,
                    Name = a.Name,
                    Status = a.Status,
                    Start = a.StartDate,
                    Finish = a.EndDate,
                    Duration = a.Duration,
                    OriginalDuration = a.OriginalDuration,
                    RemainingDuration = a.RemainingDuration,
                    PercentComplete = a.PercentComplete,
                    ActualStart = a.ActualStartDate,
                    ActualFinish = a.ActualEndDate,
                    EarlyStart = a.EarlyStartDate,
                    EarlyFinish = a.EarlyEndDate,
                    LateStart = a.LateStartDate,
                    LateFinish = a.LateEndDate,
                    TotalFloat = a.TotalFloat,
                    FreeFloat = a.FreeFloat,
                    CalendarId = a.CalendarId
                });
            }
        }

        if (storedData.Relationships != null)
        {
            foreach (var r in storedData.Relationships)
            {
                data.Relationships.Add(new ScheduleRelationship
                {
                    PredecessorActivityId = taskIdToCode.GetValueOrDefault(r.PredTaskId, r.PredTaskId),
                    SuccessorActivityId = taskIdToCode.GetValueOrDefault(r.SuccTaskId, r.SuccTaskId),
                    RelationshipType = r.Type,
                    Lag = r.LagDuration
                });
            }
        }

        if (storedData.ResourceAssignments != null)
        {
            foreach (var ra in storedData.ResourceAssignments)
            {
                var stableActivityId = taskIdToCode.GetValueOrDefault(ra.TaskId, ra.TaskId);
                data.ResourceAssignments.Add(new ScheduleResourceAssignment
                {
                    ActivityProvenanceId = ra.TaskId,
                    ActivityMatchKey = stableActivityId,
                    ResourceId = ra.ResourceId,
                    Units = ra.Units,
                    Cost = ra.CostPerUnit
                });
            }
        }

        return data;
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
