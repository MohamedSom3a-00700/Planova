using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Planova.Primavera.Application.Dto;
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

    public PrimaveraImportService(
        IPrimaveraImportRepository repository,
        XerParser parser,
        ILogger<PrimaveraImportService> logger)
    {
        _repository = repository;
        _parser = parser;
        _logger = logger;
    }

    public async Task<XerImportPreviewDto> PreviewAsync(string filePath, CancellationToken ct = default)
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

        var fileHash = ComputeFileHash(filePath);
        if (await _repository.HasDuplicateFileAsync(fileHash, ct))
        {
            return new XerImportPreviewDto
            {
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                ValidationIssues = new List<PrimaveraValidationIssueDto>
                {
                    new() { Severity = "Warning", Description = "This file appears to be a duplicate of a previously imported file." }
                }
            };
        }

        var parseResult = await _parser.ParseAsync(filePath, ct);
        var issues = new List<PrimaveraValidationIssueDto>();

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

        var projectCode = parseResult.Project?.ProjectId;
        var projectName = parseResult.Project?.Name;
        var allTables = System.Text.Json.JsonSerializer.Serialize(parseResult.TableNames.ToList());

        var session = new XerImportSession
        {
            Id = Guid.NewGuid(),
            Status = PrimaveraImportStatus.Previewing,
            SourceFileName = fileInfo.Name,
            SourceFileHash = fileHash,
            ImportedAt = DateTime.UtcNow,
            ImportedBy = Environment.UserName,
            RowCounts = System.Text.Json.JsonSerializer.Serialize(parseResult.RowCounts),
            ProjectCode = projectCode,
            ProjectName = projectName,
            TableNames = allTables
        };

        await _repository.CreateSessionAsync(session, ct);

        return new XerImportPreviewDto
        {
            SessionId = session.Id,
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
            RowCounts = parseResult.RowCounts,
            UnsupportedTables = parseResult.RawTables.Select(r => r.TableName).Distinct().ToList(),
            ValidationIssues = issues
        };
    }

    public async Task<XerImportResultDto> CommitAsync(Guid sessionId, bool overwrite, CancellationToken ct = default)
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
            session.Status = PrimaveraImportStatus.Committed;
            await _repository.UpdateSessionAsync(session, ct);

            _logger.LogInformation(
                "Import committed: Session={SessionId}, File={File}, Overwrite={Overwrite}",
                sessionId, session.SourceFileName, overwrite);

            return new XerImportResultDto
            {
                Success = true,
                ImportSessionId = sessionId,
                RowCounts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(session.RowCounts ?? "{}") ?? new()
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
            TableNames = s.TableNames
        }).ToList();
    }

    private static string ComputeFileHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
