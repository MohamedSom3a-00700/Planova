using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Application.Models;
using Planova.Primavera.Application.Parsers;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.Primavera.Application.Services;

public class PrimaveraImportService : IPrimaveraImportService
{
    private readonly IPrimaveraImportRepository _repository;
    private readonly XerParser _parser;
    private readonly ILogger<PrimaveraImportService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public PrimaveraImportService(
        IPrimaveraImportRepository repository,
        XerParser parser,
        ILogger<PrimaveraImportService> logger)
    {
        _repository = repository;
        _parser = parser;
        _logger = logger;
    }

    public async Task<XerImportPreviewDto> PreviewAsync(string filePath, int projectId = 0, CancellationToken ct = default)
    {
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            return new XerImportPreviewDto
            {
                FileName = filePath,
                ValidationIssues = new List<PrimaveraValidationIssueDto>
                {
                    new() { Severity = "Error", Description = "File not found." }
                }
            };
        }

        var parseResult = await _parser.ParseAsync(filePath, ct);
        var issues = new List<PrimaveraValidationIssueDto>();

        var xerProjectId = parseResult.Project?.ProjectId;
        var existingProjectId = xerProjectId != null
            && await _repository.HasExistingProjectByXerIdAsync(xerProjectId, ct)
            ? xerProjectId
            : null;

        if (parseResult.Errors.Count > 0)
        {
            issues.AddRange(parseResult.Errors.Select(e => new PrimaveraValidationIssueDto
            {
                Severity = "Error",
                Description = e
            }));
        }

        if (parseResult.Warnings.Count > 0)
        {
            issues.AddRange(parseResult.Warnings.Select(w => new PrimaveraValidationIssueDto
            {
                Severity = "Warning",
                Description = w
            }));
        }

        if (parseResult.Calendars.Count == 0)
        {
            issues.Add(new PrimaveraValidationIssueDto
            {
                Severity = "Warning",
                Description = "No calendars found in the XER file."
            });
        }

        if (parseResult.Calendars.All(c => !c.IsBaseCalendar))
        {
            issues.Add(new PrimaveraValidationIssueDto
            {
                Severity = "Warning",
                Description = "No base calendar marked as default."
            });
        }

        var projectName = parseResult.Project?.Name;
        var allTables = JsonSerializer.Serialize(parseResult.TableNames.ToList());

        var xerContent = await File.ReadAllTextAsync(filePath, ct);

        var parsedData = new XerStoredData
        {
            RawXerContent = xerContent,
            Activities = parseResult.Activities.Select(a => new XerStoredActivity
            {
                TaskId = a.TaskId,
                TaskCode = a.ActivityCode,
                WbsId = a.WbsId,
                Name = a.Name,
                Status = a.Status,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Duration = a.Duration,
                OriginalDuration = a.OriginalDuration,
                RemainingDuration = a.RemainingDuration,
                PercentComplete = a.PercentComplete,
                ActualStartDate = a.ActualStartDate,
                ActualEndDate = a.ActualEndDate,
                EarlyStartDate = a.EarlyStartDate,
                EarlyEndDate = a.EarlyEndDate,
                LateStartDate = a.LateStartDate,
                LateEndDate = a.LateEndDate,
                TotalFloat = a.TotalFloat,
                FreeFloat = a.FreeFloat,
                CalendarId = a.CalendarId
            }).ToList(),
            Relationships = parseResult.Relationships.Select(r => new XerStoredRelationship
            {
                PredTaskId = r.PredTaskId,
                SuccTaskId = r.SuccTaskId,
                Type = r.Type,
                LagDuration = r.LagDuration
            }).ToList(),
            ResourceAssignments = parseResult.ResourceAssignments.Select(ra => new XerStoredResourceAssignment
            {
                TaskId = ra.TaskId,
                ResourceId = ra.ResourceId,
                Units = ra.Units,
                CostPerUnit = ra.CostPerUnit
            }).ToList(),
            Calendars = parseResult.Calendars.Select(c => new XerStoredCalendar
            {
                CalendarId = c.CalendarId,
                Name = c.Name,
                IsBaseCalendar = c.IsBaseCalendar,
                BaseCalendarId = c.BaseCalendarId
            }).ToList(),
            Codes = parseResult.Codes.Select(c => new XerStoredCode
            {
                CodeTypeId = c.CodeTypeId,
                CodeType = c.CodeType,
                CodeValue = c.CodeValue,
                CodeName = c.CodeName
            }).ToList(),
            Baselines = parseResult.Baselines.Select(b => new XerStoredBaseline
            {
                BaselineId = b.BaselineId,
                Name = b.Name,
                VersionNumber = b.VersionNumber,
                IsActive = b.IsActive
            }).ToList(),
            Udfs = parseResult.Udfs.Select(u => new XerStoredUdf
            {
                UdfTypeId = u.UdfTypeId,
                TableName = u.TableName,
                FieldName = u.FieldName,
                FieldType = u.FieldType
            }).ToList(),
            RawTables = parseResult.RawTables.GroupBy(r => r.TableName).Select(g =>
            {
                var first = g.First();
                var headers = string.IsNullOrEmpty(first.ColumnHeaders)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(first.ColumnHeaders) ?? new();
                var rows = g.SelectMany(r =>
                {
                    if (string.IsNullOrEmpty(r.Rows))
                        return new List<Dictionary<string, string>>();
                    return JsonSerializer.Deserialize<List<Dictionary<string, string>>>(r.Rows) ?? new();
                }).ToList();
                return new XerStoredRawTable
                {
                    TableName = g.Key,
                    ColumnHeaders = headers,
                    Rows = rows
                };
            }).ToList(),
            ProjectId = parseResult.Project?.ProjectId,
            ProjectName = parseResult.Project?.Name,
            LastRecalcDate = parseResult.Project?.LastRecalcDate,
            PlanStartDate = parseResult.Project?.PlanStartDate,
            PlanEndDate = parseResult.Project?.PlanEndDate,
            SchedEndDate = parseResult.Project?.SchedEndDate,
            AddDate = parseResult.Project?.AddDate,
            LastTasksumDate = parseResult.Project?.LastTasksumDate,
            LastScheduleDate = parseResult.Project?.LastScheduleDate
        };

        var session = new XerImportSession
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Status = PrimaveraImportStatus.Previewing,
            SourceFileName = fileInfo.Name,
            SourceFileHash = ComputeFileHash(filePath),
            ImportedAt = DateTime.UtcNow,
            ImportedBy = Environment.UserName,
            RowCounts = JsonSerializer.Serialize(parseResult.RowCounts),
            ProjectCode = xerProjectId,
            ProjectName = projectName,
            TableNames = allTables,
            ParsedDataJson = JsonSerializer.Serialize(parsedData, JsonOptions)
        };

        await _repository.CreateSessionAsync(session, ct);

        return new XerImportPreviewDto
        {
            SessionId = session.Id,
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
            RowCounts = parseResult.RowCounts,
            UnsupportedTables = parseResult.RawTables.Select(r => r.TableName).Distinct().ToList(),
            ValidationIssues = issues,
            ExistingProjectId = existingProjectId
        };
    }

    public async Task<XerImportResultDto> CommitAsync(Guid sessionId, XerImportType importType, CancellationToken ct = default)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId, ct);
        if (session == null)
        {
            return new XerImportResultDto
            {
                Success = false,
                ErrorMessage = "Import session not found."
            };
        }

        try
        {
            if (session.ParsedDataJson != null)
            {
                var parsedData = JsonSerializer.Deserialize<XerStoredData>(session.ParsedDataJson, JsonOptions);
                if (parsedData != null)
                {
                    var now = DateTime.UtcNow;
                    var activities = parsedData.Activities.Select(a => new PrimaveraActivity
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = session.ProjectId,
                        TaskId = a.TaskId,
                        ActivityCode = a.TaskCode,
                        WbsId = a.WbsId,
                        Name = a.Name ?? "",
                        Status = a.Status ?? "",
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        Duration = a.Duration,
                        OriginalDuration = a.OriginalDuration,
                        RemainingDuration = a.RemainingDuration,
                        PercentComplete = a.PercentComplete,
                        ActualStartDate = a.ActualStartDate,
                        ActualEndDate = a.ActualEndDate,
                        EarlyStartDate = a.EarlyStartDate,
                        EarlyEndDate = a.EarlyEndDate,
                        LateStartDate = a.LateStartDate,
                        LateEndDate = a.LateEndDate,
                        TotalFloat = a.TotalFloat,
                        FreeFloat = a.FreeFloat,
                        CalendarId = a.CalendarId,
                        ImportSessionId = session.Id,
                        SourceType = PrimaveraSourceType.Imported,
                        CreatedAt = now
                    }).ToList();

                    var relationships = parsedData.Relationships.Select(r => new PrimaveraRelationship
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = session.ProjectId,
                        PredTaskId = r.PredTaskId,
                        SuccTaskId = r.SuccTaskId,
                        Type = r.Type ?? "",
                        LagDuration = r.LagDuration,
                        ImportSessionId = session.Id,
                        SourceType = PrimaveraSourceType.Imported
                    }).ToList();

                    var resourceAssignments = parsedData.ResourceAssignments.Select(ra => new PrimaveraResourceAssignment
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = session.ProjectId,
                        TaskId = ra.TaskId,
                        ResourceId = ra.ResourceId,
                        Units = ra.Units,
                        CostPerUnit = ra.CostPerUnit,
                        ImportSessionId = session.Id,
                        SourceType = PrimaveraSourceType.Imported
                    }).ToList();

                    await _repository.PersistImportDataAsync(
                        session.ProjectId, session.Id,
                        activities, relationships, resourceAssignments,
                        new List<PrimaveraCalendar>(),
                        new List<PrimaveraCode>(),
                        new List<PrimaveraBaseline>(),
                        new List<PrimaveraUdf>(),
                        ct);
                }
            }

            session.Status = PrimaveraImportStatus.Committed;
            session.ImportType = importType;
            await _repository.UpdateSessionAsync(session, ct);

            _logger.LogInformation(
                "Import committed: Session={SessionId}, File={File}, Type={ImportType}",
                sessionId, session.SourceFileName, importType);

            return new XerImportResultDto
            {
                Success = true,
                ImportSessionId = sessionId,
                RowCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(session.RowCounts ?? "{}") ?? new()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import commit failed: Session={SessionId}", sessionId);

            session.Status = PrimaveraImportStatus.Failed;
            session.ErrorMessage = ex.Message;
            await _repository.UpdateSessionAsync(session, ct);

            return new XerImportResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<XerImportResultDto> CancelImportAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId, ct);
        if (session == null)
        {
            return new XerImportResultDto
            {
                Success = false,
                ErrorMessage = "Import session not found."
            };
        }

        try
        {
            session.Status = PrimaveraImportStatus.RolledBack;
            session.ParsedDataJson = null;
            await _repository.UpdateSessionAsync(session, ct);

            _logger.LogInformation(
                "Import cancelled: Session={SessionId}, File={File}",
                sessionId, session.SourceFileName);

            return new XerImportResultDto
            {
                Success = true,
                ImportSessionId = sessionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import cancel failed: Session={SessionId}", sessionId);
            return new XerImportResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<XerImportSessionDto>> GetImportedSessionsAsync(CancellationToken ct = default)
    {
        var sessions = await _repository.GetSessionsAsync(ct);
        return sessions.Select(s => new XerImportSessionDto
        {
            Id = s.Id,
            SourceFileName = s.SourceFileName,
            ImportedAt = s.ImportedAt,
            ImportedBy = s.ImportedBy,
            Status = s.Status.ToString(),
            RowCounts = s.RowCounts,
            ValidationSummary = s.ValidationSummary,
            ErrorMessage = s.ErrorMessage,
            ProjectCode = s.ProjectCode,
            ProjectName = s.ProjectName,
            TableNames = s.TableNames,
            ImportType = s.ImportType?.ToString(),
            ParsedDataJson = s.ParsedDataJson
        }).ToList();
    }

    public async Task<List<XerImportSessionDto>> GetImportedSessionsByProjectAsync(int projectId, CancellationToken ct = default)
    {
        var sessions = await _repository.GetSessionsByProjectAsync(projectId, ct);
        return sessions.Select(s => new XerImportSessionDto
        {
            Id = s.Id,
            SourceFileName = s.SourceFileName,
            ImportedAt = s.ImportedAt,
            ImportedBy = s.ImportedBy,
            Status = s.Status.ToString(),
            RowCounts = s.RowCounts,
            ValidationSummary = s.ValidationSummary,
            ErrorMessage = s.ErrorMessage,
            ProjectCode = s.ProjectCode,
            ProjectName = s.ProjectName,
            TableNames = s.TableNames,
            ImportType = s.ImportType?.ToString(),
            ParsedDataJson = s.ParsedDataJson
        }).ToList();
    }

    public async Task<XerImportSessionDto?> GetSessionByIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId, ct);
        if (session == null) return null;

        return new XerImportSessionDto
        {
            Id = session.Id,
            SourceFileName = session.SourceFileName,
            ImportedAt = session.ImportedAt,
            ImportedBy = session.ImportedBy,
            Status = session.Status.ToString(),
            RowCounts = session.RowCounts,
            ValidationSummary = session.ValidationSummary,
            ErrorMessage = session.ErrorMessage,
            ProjectCode = session.ProjectCode,
            ProjectName = session.ProjectName,
            TableNames = session.TableNames,
            ImportType = session.ImportType?.ToString(),
            ParsedDataJson = session.ParsedDataJson
        };
    }

    public async Task DeleteAllXerDataAsync(CancellationToken ct = default)
    {
        await _repository.DeleteAllXerDataAsync(ct);
        _logger.LogInformation("All XER import data has been deleted.");
    }

    private static string ComputeFileHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
