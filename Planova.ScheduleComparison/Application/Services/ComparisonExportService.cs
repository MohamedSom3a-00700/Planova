using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Text.Json;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.ScheduleComparison.Application.Services;

public class ComparisonExportService : IComparisonExportService
{
    private readonly IComparisonRepository _repository;

    public ComparisonExportService(IComparisonRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> ExportToExcelAsync(Guid sessionId, string outputDir, CancellationToken ct = default)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId, ct);
        if (session == null)
            throw new InvalidOperationException($"Session {sessionId} not found.");

        var results = await _repository.GetResultsBySessionIdAsync(sessionId, ct);
        var filePath = Path.Combine(outputDir, $"comparison_{sessionId:N}.xlsx");

        Directory.CreateDirectory(outputDir);
        var tempPath = Path.Combine(outputDir, $"_{Guid.NewGuid():N}.xlsx");

        try
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();

            var activitySheet = workbook.Worksheets.Add("Activities");
            BuildActivitySheet(activitySheet, results);

            var logicSheet = workbook.Worksheets.Add("Logic");
            BuildLogicSheet(logicSheet, results);

            var resourceSheet = workbook.Worksheets.Add("Resources");
            BuildResourceSheet(resourceSheet, results);

            workbook.SaveAs(tempPath);
            File.Move(tempPath, filePath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }

        return filePath;
    }

    public async Task<string> ExportToPdfAsync(Guid sessionId, string outputDir, CancellationToken ct = default)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId, ct);
        if (session == null)
            throw new InvalidOperationException($"Session {sessionId} not found.");

        var results = await _repository.GetResultsBySessionIdAsync(sessionId, ct);
        var filePath = Path.Combine(outputDir, $"comparison_{sessionId:N}.pdf");

        Directory.CreateDirectory(outputDir);
        var tempPath = filePath + ".tmp";

        try
        {
            QuestPDF.Fluent.Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Margin(50);
                    page.Header().AlignCenter().Text("Schedule Comparison Report").SemiBold().FontSize(20);
                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Session: {sessionId}");
                        col.Item().Text($"Source: {session.SourceLabel}");
                        col.Item().Text($"Target: {session.TargetLabel}");
                        col.Item().Text($"Compared: {session.CompletedAt:g}");
                        col.Item().Text($"Total changes: {results.Count}");

                        var added = results.Count(r => r.ChangeType == Domain.Enums.ChangeType.Added);
                        var removed = results.Count(r => r.ChangeType == Domain.Enums.ChangeType.Removed);
                        var modified = results.Count(r => r.ChangeType == Domain.Enums.ChangeType.Modified);

                        col.Item().Text($"Added: {added}");
                        col.Item().Text($"Removed: {removed}");
                        col.Item().Text($"Modified: {modified}");
                    });
                });
            }).GeneratePdf(tempPath);

            File.Move(tempPath, filePath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }

        return filePath;
    }

    public async Task<string> ExportToJsonAsync(Guid sessionId, string outputDir, CancellationToken ct = default)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId, ct);
        if (session == null)
            throw new InvalidOperationException($"Session {sessionId} not found.");

        if (string.IsNullOrEmpty(session.ResultJson))
            throw new InvalidOperationException($"Session {sessionId} has no result data.");

        var filePath = Path.Combine(outputDir, $"comparison_{sessionId:N}.json");
        Directory.CreateDirectory(outputDir);
        var tempPath = filePath + ".tmp";

        try
        {
            var formatted = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<JsonElement>(session.ResultJson),
                new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(tempPath, formatted, ct);
            File.Move(tempPath, filePath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }

        return filePath;
    }

    private static void BuildActivitySheet(ClosedXML.Excel.IXLWorksheet sheet, List<ComparisonResult> results)
    {
        var activityResults = results.Where(r => r.EntityType == "Activity").ToList();

        sheet.Cell(1, 1).Value = "MatchKey";
        sheet.Cell(1, 2).Value = "Field";
        sheet.Cell(1, 3).Value = "Change";
        sheet.Cell(1, 4).Value = "Old Value";
        sheet.Cell(1, 5).Value = "New Value";
        sheet.Cell(1, 6).Value = "Severity";

        for (int i = 0; i < activityResults.Count; i++)
        {
            var row = i + 2;
            sheet.Cell(row, 1).Value = activityResults[i].MatchKey;
            sheet.Cell(row, 2).Value = activityResults[i].FieldName ?? "(entity)";
            sheet.Cell(row, 3).Value = activityResults[i].ChangeType.ToString();
            sheet.Cell(row, 4).Value = activityResults[i].OldValue;
            sheet.Cell(row, 5).Value = activityResults[i].NewValue;
            sheet.Cell(row, 6).Value = activityResults[i].Severity;
        }

        sheet.Columns().AdjustToContents();
    }

    private static void BuildLogicSheet(ClosedXML.Excel.IXLWorksheet sheet, List<ComparisonResult> results)
    {
        var logicResults = results.Where(r => r.EntityType == "Relationship").ToList();

        sheet.Cell(1, 1).Value = "MatchKey";
        sheet.Cell(1, 2).Value = "Change";
        sheet.Cell(1, 3).Value = "Old Type";
        sheet.Cell(1, 4).Value = "New Type";

        for (int i = 0; i < logicResults.Count; i++)
        {
            var row = i + 2;
            sheet.Cell(row, 1).Value = logicResults[i].MatchKey;
            sheet.Cell(row, 2).Value = logicResults[i].ChangeType.ToString();
            sheet.Cell(row, 3).Value = logicResults[i].OldValue;
            sheet.Cell(row, 4).Value = logicResults[i].NewValue;
        }

        sheet.Columns().AdjustToContents();
    }

    private static void BuildResourceSheet(ClosedXML.Excel.IXLWorksheet sheet, List<ComparisonResult> results)
    {
        var resourceResults = results.Where(r => r.EntityType == "ResourceAssignment").ToList();

        sheet.Cell(1, 1).Value = "MatchKey";
        sheet.Cell(1, 2).Value = "Change";
        sheet.Cell(1, 3).Value = "Old Units";
        sheet.Cell(1, 4).Value = "New Units";

        for (int i = 0; i < resourceResults.Count; i++)
        {
            var row = i + 2;
            sheet.Cell(row, 1).Value = resourceResults[i].MatchKey;
            sheet.Cell(row, 2).Value = resourceResults[i].ChangeType.ToString();
            sheet.Cell(row, 3).Value = resourceResults[i].OldValue;
            sheet.Cell(row, 4).Value = resourceResults[i].NewValue;
        }

        sheet.Columns().AdjustToContents();
    }
}
