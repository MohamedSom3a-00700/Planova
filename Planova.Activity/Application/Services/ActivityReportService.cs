using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Planova.Activity.Application.Services;

public class ActivityReportService : IActivityReportService
{
    private readonly IActivityService _activityService;
    private readonly IActivityRelationshipService _relationshipService;

    public ActivityReportService(IActivityService activityService, IActivityRelationshipService relationshipService)
    {
        _activityService = activityService;
        _relationshipService = relationshipService;
    }

    public async Task<ScheduleReportDto> GenerateScheduleReportAsync(int projectId, CancellationToken ct = default)
    {
        var activities = await _activityService.GetByProjectAsync(projectId, null, ct);
        var relationships = await _relationshipService.GetByProjectAsync(projectId, ct);

        var rows = activities.Select(a =>
        {
            var preds = relationships.Where(r => r.SuccessorId == a.Id).ToList();
            var succs = relationships.Where(r => r.PredecessorId == a.Id).ToList();

            return new ScheduleReportRowDto
            {
                Code = a.Code,
                Name = a.Name,
                Type = a.ActivityType,
                Status = a.Status,
                Duration = a.Duration,
                PlannedStart = a.PlannedStart,
                PlannedFinish = a.PlannedFinish,
                PercentComplete = a.PercentComplete,
                Predecessors = string.Join(", ", preds.Select(p => p.PredecessorCode)),
                Successors = string.Join(", ", succs.Select(s => s.SuccessorCode))
            };
        }).ToList();

        return new ScheduleReportDto
        {
            ProjectName = $"Project {projectId}",
            GeneratedAt = DateTime.UtcNow,
            Rows = rows
        };
    }

    public async Task<byte[]> ExportToExcelAsync(int projectId, CancellationToken ct = default)
    {
        var report = await GenerateScheduleReportAsync(projectId, ct);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Schedule Report");

        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Name";
        ws.Cell(1, 3).Value = "Type";
        ws.Cell(1, 4).Value = "Status";
        ws.Cell(1, 5).Value = "Duration";
        ws.Cell(1, 6).Value = "Planned Start";
        ws.Cell(1, 7).Value = "Planned Finish";
        ws.Cell(1, 8).Value = "% Complete";
        ws.Cell(1, 9).Value = "Predecessors";
        ws.Cell(1, 10).Value = "Successors";

        for (int i = 0; i < report.Rows.Count; i++)
        {
            var row = report.Rows[i];
            var r = i + 2;
            ws.Cell(r, 1).Value = row.Code;
            ws.Cell(r, 2).Value = row.Name;
            ws.Cell(r, 3).Value = row.Type;
            ws.Cell(r, 4).Value = row.Status;
            ws.Cell(r, 5).Value = row.Duration;
            ws.Cell(r, 6).Value = row.PlannedStart?.ToString("d");
            ws.Cell(r, 7).Value = row.PlannedFinish?.ToString("d");
            ws.Cell(r, 8).Value = row.PercentComplete;
            ws.Cell(r, 9).Value = row.Predecessors;
            ws.Cell(r, 10).Value = row.Successors;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportToPdfAsync(int projectId, CancellationToken ct = default)
    {
        var report = await GenerateScheduleReportAsync(projectId, ct);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.Header().Text($"Schedule Report - {report.ProjectName}").SemiBold().FontSize(16);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Code");
                        header.Cell().Text("Name");
                        header.Cell().Text("Type");
                        header.Cell().Text("Status");
                        header.Cell().Text("Duration");
                        header.Cell().Text("Start");
                    });

                    foreach (var row in report.Rows)
                    {
                        table.Cell().Text(row.Code ?? "");
                        table.Cell().Text(row.Name ?? "");
                        table.Cell().Text(row.Type ?? "");
                        table.Cell().Text(row.Status ?? "");
                        table.Cell().Text(row.Duration?.ToString() ?? "");
                        table.Cell().Text(row.PlannedStart?.ToString("d") ?? "");
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated: ");
                    text.Span(report.GeneratedAt.ToString("g"));
                });
            });
        }).GeneratePdf();
    }
}
