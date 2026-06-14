using Planova.Primavera.Application.Models;
using Planova.Primavera.Application.Writers;
using Planova.Primavera.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Primavera.Application.Services;

public class PrimaveraExportService : IPrimaveraExportService
{
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private readonly IPrimaveraExportRepository _exportRepository;
    private readonly XerWriter _writer;
    private readonly ILoggingService _logger;

    public PrimaveraExportService(
        IPrimaveraWorkspaceService workspaceService,
        IPrimaveraExportRepository exportRepository,
        XerWriter writer,
        ILoggingService logger)
    {
        _workspaceService = workspaceService;
        _exportRepository = exportRepository;
        _writer = writer;
        _logger = logger;
    }

    public async Task<string> ExportAsync(int projectId, string outputPath, CancellationToken ct = default)
    {
        var profile = new PrimaveraExportProfile
        {
            ProjectId = projectId,
            OutputPath = outputPath
        };
        return await ExportWithProfileAsync(profile, ct);
    }

    public async Task<string> ExportWithProfileAsync(PrimaveraExportProfile profile, CancellationToken ct = default)
    {
        var snapshot = await _workspaceService.GetSnapshotAsync(profile.ProjectId, ct);

        var calendars = profile.IncludeCalendars
            ? snapshot.Calendars.Select(c => new Primavera.Domain.Entities.PrimaveraCalendar
            {
                CalendarId = c.CalendarId, Name = c.Name,
                IsBaseCalendar = c.IsBaseCalendar, BaseCalendarId = c.BaseCalendarId
            }).ToList()
            : new();

        var activities = profile.IncludeActivities
            ? snapshot.Activities.Select(a => new Primavera.Domain.Entities.PrimaveraActivity
            {
                TaskId = a.TaskId, Name = a.Name, Duration = a.Duration,
                StartDate = a.StartDate, EndDate = a.EndDate, PercentComplete = a.PercentComplete,
                CalendarId = a.CalendarId, Status = a.Status
            }).ToList()
            : new();

        var relationships = profile.IncludeRelationships
            ? snapshot.Relationships.Select(r => new Primavera.Domain.Entities.PrimaveraRelationship
            {
                PredTaskId = r.PredTaskId, SuccTaskId = r.SuccTaskId, Type = r.Type, LagDuration = r.LagDuration
            }).ToList()
            : new();

        var assignments = profile.IncludeResourceAssignments
            ? snapshot.ResourceAssignments.Select(r => new Primavera.Domain.Entities.PrimaveraResourceAssignment
            {
                TaskId = r.TaskId, ResourceId = r.ResourceId, Units = r.Units,
                PlannedUnits = r.PlannedUnits, ActualUnits = r.ActualUnits, CostPerUnit = r.CostPerUnit
            }).ToList()
            : new();

        var codes = profile.IncludeCodes
            ? snapshot.Codes.Select(c => new Primavera.Domain.Entities.PrimaveraCode
            {
                CodeTypeId = c.CodeTypeId, CodeType = c.CodeType, CodeValue = c.CodeValue, CodeName = c.CodeName
            }).ToList()
            : new();

        var baselines = profile.IncludeBaselines
            ? snapshot.Baselines.Select(b => new Primavera.Domain.Entities.PrimaveraBaseline
            {
                BaselineId = b.BaselineId, Name = b.Name, VersionNumber = b.VersionNumber, IsActive = b.IsActive
            }).ToList()
            : new();

        var udfs = profile.IncludeUdfs
            ? snapshot.Udfs.Select(u => new Primavera.Domain.Entities.PrimaveraUdf
            {
                UdfTypeId = u.UdfTypeId, TableName = u.TableName, FieldName = u.FieldName, FieldType = u.FieldType
            }).ToList()
            : new();

        var rawTables = profile.PreserveRawTables
            ? await _exportRepository.GetRawTablesAsync(profile.ProjectId, ct)
            : new();

        await _writer.WriteAsync(profile.OutputPath, calendars, null, activities, relationships,
            assignments, codes, baselines, udfs, rawTables, ct);

        var rowCounts = new Dictionary<string, int>
        {
            ["Activities"] = activities.Count,
            ["Relationships"] = relationships.Count,
            ["ResourceAssignments"] = assignments.Count,
            ["Calendars"] = calendars.Count,
            ["Codes"] = codes.Count,
            ["Baselines"] = baselines.Count,
            ["Udfs"] = udfs.Count,
            ["RawTables"] = rawTables.Count
        };

        _logger.Info("Export completed: Path={Path}, ProjectId={ProjectId}, RowCounts={Counts}",
            profile.OutputPath, profile.ProjectId,
            string.Join(", ", rowCounts.Where(kv => kv.Value > 0).Select(kv => $"{kv.Key}={kv.Value}")));

        return profile.OutputPath;
    }
}
