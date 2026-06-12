using Planova.Reporting.Application.Dto;
using Planova.Reporting.Application.Mappings;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Planova.Reporting.Application.Services;

public class ReportExportService : IReportExportService
{
    private readonly IReportInstanceRepository _instanceRepository;
    private readonly ILoggingService _logger;

    public ReportExportService(IReportInstanceRepository instanceRepository, ILoggingService logger)
    {
        _instanceRepository = instanceRepository;
        _logger = logger;
    }

    public async Task<byte[]> ExportToExcelAsync(Guid instanceId, object reportData, CancellationToken ct = default)
    {
        _logger.Info("Exporting report instance {InstanceId} to Excel", instanceId);

        var instance = await _instanceRepository.GetByIdAsync(instanceId, ct);
        if (instance == null)
        {
            _logger.Error("Report instance {InstanceId} not found for Excel export", new InvalidOperationException($"Report instance {instanceId} not found."), instanceId);
            throw new InvalidOperationException($"Report instance {instanceId} not found.");
        }

        var result = GenerateExcelExport(instance, reportData);
        _logger.Info("Excel export completed for instance {InstanceId} ({Size} bytes)", instanceId, result.Length);
        return result;
    }

    public async Task<byte[]> ExportToPdfAsync(Guid instanceId, object reportData, CancellationToken ct = default)
    {
        _logger.Info("Exporting report instance {InstanceId} to PDF", instanceId);

        var instance = await _instanceRepository.GetByIdAsync(instanceId, ct);
        if (instance == null)
        {
            _logger.Error("Report instance {InstanceId} not found for PDF export", new InvalidOperationException($"Report instance {instanceId} not found."), instanceId);
            throw new InvalidOperationException($"Report instance {instanceId} not found.");
        }

        var result = GeneratePdfExport(instance, reportData);
        _logger.Info("PDF export completed for instance {InstanceId} ({Size} bytes)", instanceId, result.Length);
        return result;
    }

    public async Task<byte[]> ExportToWordAsync(Guid instanceId, object reportData, CancellationToken ct = default)
    {
        _logger.Info("Exporting report instance {InstanceId} to Word", instanceId);

        var instance = await _instanceRepository.GetByIdAsync(instanceId, ct);
        if (instance == null)
        {
            _logger.Error("Report instance {instanceId} not found for Word export", new InvalidOperationException($"Report instance {instanceId} not found."), instanceId);
            throw new InvalidOperationException($"Report instance {instanceId} not found.");
        }

        var result = GenerateWordExport(instance, reportData);
        _logger.Info("Word export completed for instance {InstanceId} ({Size} bytes)", instanceId, result.Length);
        return result;
    }

    public async Task<ReportExportDto> SaveExportAsync(Guid instanceId, ExportFormat format, byte[] fileContent, string exportedBy, CancellationToken ct = default)
    {
        _logger.Info("Saving {Format} export for instance {InstanceId} (by {ExportedBy})", format, instanceId, exportedBy);

        var instance = await _instanceRepository.GetByIdAsync(instanceId, ct);
        if (instance == null)
        {
            _logger.Error("Report instance {InstanceId} not found for saving export", new InvalidOperationException($"Report instance {instanceId} not found."), instanceId);
            throw new InvalidOperationException($"Report instance {instanceId} not found.");
        }

        var exportDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Planova", "Projects", instance.ProjectId.ToString(), "Reports", "Exports");

        Directory.CreateDirectory(exportDir);

        var extension = format switch
        {
            ExportFormat.Excel => ".xlsx",
            ExportFormat.Pdf => ".pdf",
            ExportFormat.Word => ".docx",
            _ => ".bin"
        };

        var fileName = $"{instance.Title}_{instance.Id:N}{extension}";
        var filePath = Path.Combine(exportDir, fileName);
        await File.WriteAllBytesAsync(filePath, fileContent, ct);

        var export = new ReportExport
        {
            Id = Guid.NewGuid(),
            ReportInstanceId = instanceId,
            Format = format,
            FilePath = filePath,
            FileSizeBytes = fileContent.Length,
            ExportedAt = DateTime.UtcNow,
            ExportedBy = exportedBy
        };

        if (instance.Exports == null)
            instance.Exports = new List<ReportExport>();
        instance.Exports.Add(export);
        await _instanceRepository.UpdateAsync(instance, ct);

        _logger.Info("Export saved: {FilePath} ({Size} bytes)", filePath, fileContent.Length);
        return export.ToDto();
    }

    public async Task DeleteExportFileAsync(Guid exportId, CancellationToken ct = default)
    {
        _logger.Info("Deleting export {ExportId}", exportId);
        var reports = await _instanceRepository.GetByProjectAsync(0, ct: ct);
        foreach (var report in reports)
        {
            var export = report.Exports?.FirstOrDefault(e => e.Id == exportId);
            if (export == null) continue;

            if (File.Exists(export.FilePath))
                File.Delete(export.FilePath);

            if (report.Exports != null)
                report.Exports.Remove(export);
            await _instanceRepository.UpdateAsync(report, ct);
            return;
        }
    }

    private static byte[] GenerateExcelExport(ReportInstance instance, object reportData)
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Report");

        ws.Cell(1, 1).Value = instance.Title;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "Status:";
        ws.Cell(3, 2).Value = instance.Status.ToString();
        ws.Cell(4, 1).Value = "Generated:";
        ws.Cell(4, 2).Value = instance.GeneratedAt.ToString("g");
        ws.Cell(5, 1).Value = "Period:";
        ws.Cell(5, 2).Value = $"{instance.PeriodStart:d} — {instance.PeriodEnd:d}";

        int row = 7;

        switch (reportData)
        {
            case DailyReportDataDto daily:
                row = WriteExcelSection(ws, row, "Summary", ["Metric", "Value"]);
                ws.Cell(row, 1).Value = "Total Activities";
                ws.Cell(row++, 2).Value = daily.TotalActivities;
                ws.Cell(row, 1).Value = "Completed";
                ws.Cell(row++, 2).Value = daily.CompletedActivities;
                ws.Cell(row, 1).Value = "In Progress";
                ws.Cell(row++, 2).Value = daily.InProgressActivities;
                ws.Cell(row, 1).Value = "Not Started";
                ws.Cell(row++, 2).Value = daily.NotStartedActivities;
                ws.Cell(row, 1).Value = "Overall Progress";
                ws.Cell(row++, 2).Value = $"{daily.OverallPercentComplete:P1}";

                if (daily.ProgressToday.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Progress Today",
                        ["Code", "Activity", "% Complete", "Status"],
                        daily.ProgressToday.Select(p => new object[] { p.ActivityCode, p.ActivityName, p.PercentComplete, p.Status }));

                if (daily.Workforce.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Workforce",
                        ["Resource", "Activity", "Qty", "UOM"],
                        daily.Workforce.Select(w => new object[] { w.ResourceName, w.ActivityName, w.Quantity, w.UnitOfMeasure }));

                if (daily.Equipment.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Equipment",
                        ["Equipment", "Activity", "Qty", "UOM"],
                        daily.Equipment.Select(e => new object[] { e.EquipmentName, e.ActivityName, e.Quantity, e.UnitOfMeasure }));
                break;

            case WeeklyReportDataDto weekly:
                if (weekly.ProgressByWbs.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Progress by WBS",
                        ["Code", "Name", "Total", "Completed", "% Complete", "Weight"],
                        weekly.ProgressByWbs.Select(p => new object[] { p.WbsCode, p.WbsName, p.TotalActivities, p.CompletedActivities, p.PercentComplete, p.Weight }));

                if (weekly.ResourceUsage.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Resource Usage",
                        ["Resource", "Type", "Total Qty", "UOM"],
                        weekly.ResourceUsage.Select(r => new object[] { r.ResourceName, r.ResourceType, r.TotalQuantity, r.UnitOfMeasure }));

                if (weekly.Delays.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Delays",
                        ["Activity", "Code", "Planned Finish", "Actual Finish", "Delay (days)"],
                        weekly.Delays.Select(d => new object[] { d.ActivityName, d.ActivityCode, d.PlannedFinish?.ToString("d"), d.ActualFinish?.ToString("d"), d.DelayDays }));

                if (weekly.LookAhead.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Look-Ahead (Next 7 Days)",
                        ["Activity", "Code", "Planned Start", "Planned Finish", "Status"],
                        weekly.LookAhead.Select(l => new object[] { l.ActivityName, l.ActivityCode, l.PlannedStart?.ToString("d"), l.PlannedFinish?.ToString("d"), l.Status }));
                break;

            case MonthlyReportDataDto monthly:
                if (monthly.EvmSummary != null)
                {
                    row = WriteExcelSection(ws, row, "EVM Summary", ["Metric", "Value"]);
                    ws.Cell(row, 1).Value = "Planned Value";
                    ws.Cell(row++, 2).Value = monthly.EvmSummary.PlannedValue;
                    ws.Cell(row, 1).Value = "Earned Value";
                    ws.Cell(row++, 2).Value = monthly.EvmSummary.EarnedValue;
                    ws.Cell(row, 1).Value = "Actual Cost";
                    ws.Cell(row++, 2).Value = monthly.EvmSummary.ActualCost;
                    ws.Cell(row, 1).Value = "CPI";
                    ws.Cell(row++, 2).Value = monthly.EvmSummary.CostPerformanceIndex;
                    ws.Cell(row, 1).Value = "SPI";
                    ws.Cell(row++, 2).Value = monthly.EvmSummary.SchedulePerformanceIndex;
                    ws.Cell(row, 1).Value = "EAC";
                    ws.Cell(row++, 2).Value = monthly.EvmSummary.EstimateAtCompletion;
                }

                if (monthly.BudgetVsActual.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Budget vs Actual",
                        ["Category", "Budget", "Actual", "Variance"],
                        monthly.BudgetVsActual.Select(b => new object[] { b.Category, b.Budget, b.Actual, b.Variance }));

                if (monthly.ProgressByWbs.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Progress by WBS",
                        ["Code", "Name", "% Complete", "Weight"],
                        monthly.ProgressByWbs.Select(p => new object[] { p.WbsCode, p.WbsName, p.PercentComplete, p.Weight }));

                if (monthly.ResourceProductivity.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Resource Productivity",
                        ["Resource", "Type", "Total Qty", "Total Cost", "UOM"],
                        monthly.ResourceProductivity.Select(r => new object[] { r.ResourceName, r.ResourceType, r.TotalQuantity, r.TotalCost, r.UnitOfMeasure }));
                break;

            case ExecutiveReportDataDto exec:
                if (exec.KpiCards.Count > 0)
                    row = WriteExcelDataTable(ws, row, "KPI Dashboard",
                        ["Label", "Value", "Unit", "Trend", "Status"],
                        exec.KpiCards.Select(k => new object[] { k.Label, k.Value, k.Unit, k.Trend, k.StatusColor }));

                if (exec.SCurve.Count > 0)
                    row = WriteExcelDataTable(ws, row, "S-Curve",
                        ["Date", "Planned Value", "Earned Value", "Actual Cost"],
                        exec.SCurve.Select(s => new object[] { s.Date.ToString("yyyy-MM"), s.PlannedValue, s.EarnedValue, s.ActualCost }));

                if (exec.FinancialOverview != null)
                {
                    row = WriteExcelSection(ws, row, "Financial Overview", ["Metric", "Value"]);
                    ws.Cell(row, 1).Value = "Total Budget";
                    ws.Cell(row++, 2).Value = exec.FinancialOverview.TotalBudget;
                    ws.Cell(row, 1).Value = "Total Actual Cost";
                    ws.Cell(row++, 2).Value = exec.FinancialOverview.TotalActualCost;
                    ws.Cell(row, 1).Value = "Variance";
                    ws.Cell(row++, 2).Value = exec.FinancialOverview.TotalVariance;
                    ws.Cell(row, 1).Value = "EAC";
                    ws.Cell(row++, 2).Value = exec.FinancialOverview.EstimateAtCompletion;
                    ws.Cell(row, 1).Value = "VAC";
                    ws.Cell(row++, 2).Value = exec.FinancialOverview.VarianceAtCompletion;
                    ws.Cell(row, 1).Value = "CPI";
                    ws.Cell(row++, 2).Value = exec.FinancialOverview.CostPerformanceIndex;
                }

                if (exec.Milestones.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Milestone Status",
                        ["Activity", "Code", "Status", "Planned Start", "Planned Finish", "% Complete"],
                        exec.Milestones.Select(m => new object[] { m.ActivityName, m.ActivityCode, m.Status, m.PlannedStart?.ToString("yyyy-MM-dd"), m.PlannedFinish?.ToString("yyyy-MM-dd"), m.PercentComplete }));

                if (exec.ProjectParties.Count > 0)
                    row = WriteExcelDataTable(ws, row, "Project Directory",
                        ["Name", "Role"],
                        exec.ProjectParties.Select(p => new object[] { p.Name, p.Role }));
                break;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static int WriteExcelSection(ClosedXML.Excel.IXLWorksheet ws, int startRow, string title, string[] headers)
    {
        ws.Cell(startRow, 1).Value = title;
        ws.Cell(startRow, 1).Style.Font.Bold = true;
        ws.Cell(startRow, 1).Style.Font.FontSize = 12;
        startRow++;
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(startRow, i + 1).Value = headers[i];
            ws.Cell(startRow, i + 1).Style.Font.Bold = true;
            ws.Cell(startRow, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(0x2A, 0x34, 0x41);
            ws.Cell(startRow, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
        }
        return startRow + 1;
    }

    private static int WriteExcelDataTable(ClosedXML.Excel.IXLWorksheet ws, int startRow, string title, string[] headers, IEnumerable<object[]> rows)
    {
        startRow = WriteExcelSection(ws, startRow, title, headers);
        int r = startRow;
        foreach (var row in rows)
        {
                for (int i = 0; i < row.Length; i++)
                SetExcelCell(ws.Cell(r, i + 1), row[i]);
            r++;
        }
        return r + 1;
    }

    private static void SetExcelCell(ClosedXML.Excel.IXLCell cell, object? val)
    {
        if (val is null) { cell.Value = string.Empty; return; }
        if (val is decimal d) { cell.Value = (double)d; return; }
        if (val is int i) { cell.Value = i; return; }
        if (val is long l) { cell.Value = l; return; }
        if (val is float f) { cell.Value = (double)f; return; }
        if (val is bool b) { cell.Value = b; return; }
        cell.Value = val.ToString() ?? string.Empty;
    }

    private static byte[] GeneratePdfExport(ReportInstance instance, object reportData)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.Header().Text(instance.Title).FontSize(18).Bold();
                page.Content().Column(col =>
                {
                    col.Item().Text($"Status: {instance.Status}  |  Generated: {instance.GeneratedAt:g}  |  Period: {instance.PeriodStart:d} — {instance.PeriodEnd:d}")
                        .FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);

                    col.Item().PaddingVertical(8);

                    switch (reportData)
                    {
                        case DailyReportDataDto daily:
                            AddPdfSummaryCards(col,
                                ("Total Activities", daily.TotalActivities.ToString()),
                                ("Completed", daily.CompletedActivities.ToString()),
                                ("In Progress", daily.InProgressActivities.ToString()),
                                ("Not Started", daily.NotStartedActivities.ToString()));
                            col.Item().Text($"Overall Progress: {daily.OverallPercentComplete:P1}").FontSize(14).Bold();
                            AddPdfTable(col, "Progress Today", ["Code", "Activity", "% Complete", "Status"],
                                daily.ProgressToday.Select(p => new[] { p.ActivityCode, p.ActivityName, $"{p.PercentComplete:P0}", p.Status }));
                            AddPdfTable(col, "Workforce", ["Resource", "Activity", "Qty", "UOM"],
                                daily.Workforce.Select(w => new[] { w.ResourceName, w.ActivityName, w.Quantity.ToString(), w.UnitOfMeasure }));
                            AddPdfTable(col, "Equipment", ["Equipment", "Activity", "Qty", "UOM"],
                                daily.Equipment.Select(e => new[] { e.EquipmentName, e.ActivityName, e.Quantity.ToString(), e.UnitOfMeasure }));
                            break;

                        case WeeklyReportDataDto weekly:
                            AddPdfTable(col, "Progress by WBS", ["Code", "Name", "Total", "Completed", "% Complete", "Weight"],
                                weekly.ProgressByWbs.Select(p => new[] { p.WbsCode, p.WbsName, p.TotalActivities.ToString(), p.CompletedActivities.ToString(), $"{p.PercentComplete:P1}", $"{p.Weight:P1}" }));
                            AddPdfTable(col, "Resource Usage", ["Resource", "Type", "Total Qty", "UOM"],
                                weekly.ResourceUsage.Select(r => new[] { r.ResourceName, r.ResourceType, r.TotalQuantity.ToString(), r.UnitOfMeasure }));
                            AddPdfTable(col, "Delays", ["Activity", "Code", "Planned Finish", "Actual Finish", "Delay (days)"],
                                weekly.Delays.Select(d => new[] { d.ActivityName, d.ActivityCode, d.PlannedFinish?.ToString("d") ?? "-", d.ActualFinish?.ToString("d") ?? "-", d.DelayDays.ToString() }));
                            AddPdfTable(col, "Look-Ahead (Next 7 Days)", ["Activity", "Code", "Planned Start", "Planned Finish", "Status"],
                                weekly.LookAhead.Select(l => new[] { l.ActivityName, l.ActivityCode, l.PlannedStart?.ToString("d") ?? "-", l.PlannedFinish?.ToString("d") ?? "-", l.Status }));
                            break;

                        case MonthlyReportDataDto monthly:
                            if (monthly.EvmSummary != null)
                            {
                                col.Item().Text("EVM Summary").FontSize(14).Bold();
                                col.Item().Text($"PV: {monthly.EvmSummary.PlannedValue:N2}  |  EV: {monthly.EvmSummary.EarnedValue:N2}  |  AC: {monthly.EvmSummary.ActualCost:N2}  |  CPI: {monthly.EvmSummary.CostPerformanceIndex:N2}  |  SPI: {monthly.EvmSummary.SchedulePerformanceIndex:N2}  |  EAC: {monthly.EvmSummary.EstimateAtCompletion:N2}");
                                col.Item().PaddingVertical(4);
                            }
                            AddPdfTable(col, "Budget vs Actual", ["Category", "Budget", "Actual", "Variance"],
                                monthly.BudgetVsActual.Select(b => new[] { b.Category, b.Budget.ToString("N2"), b.Actual.ToString("N2"), b.Variance.ToString("N2") }));
                            AddPdfTable(col, "Progress by WBS", ["Code", "Name", "% Complete", "Weight"],
                                monthly.ProgressByWbs.Select(p => new[] { p.WbsCode, p.WbsName, $"{p.PercentComplete:P1}", $"{p.Weight:P1}" }));
                            AddPdfTable(col, "Resource Productivity", ["Resource", "Type", "Total Qty", "Total Cost", "UOM"],
                                monthly.ResourceProductivity.Select(r => new[] { r.ResourceName, r.ResourceType, r.TotalQuantity.ToString(), r.TotalCost.ToString("N2"), r.UnitOfMeasure }));
                            break;

                        case ExecutiveReportDataDto exec:
                            if (exec.KpiCards.Count > 0)
                            {
                                col.Item().Text("KPI Dashboard").FontSize(14).Bold();
                                foreach (var k in exec.KpiCards)
                                    col.Item().Text($"{k.Label}: {k.Value:N2} {k.Unit}  ({k.Trend ?? "-"})");
                                col.Item().PaddingVertical(4);
                            }
                            AddPdfTable(col, "S-Curve", ["Date", "Planned Value", "Earned Value", "Actual Cost"],
                                exec.SCurve.Select(s => new[] { s.Date.ToString("yyyy-MM"), s.PlannedValue.ToString("N2"), s.EarnedValue.ToString("N2"), s.ActualCost.ToString("N2") }));
                            if (exec.FinancialOverview != null)
                            {
                                col.Item().Text("Financial Overview").FontSize(14).Bold();
                                col.Item().Text($"Budget: {exec.FinancialOverview.TotalBudget:N2}  |  Actual: {exec.FinancialOverview.TotalActualCost:N2}  |  Variance: {exec.FinancialOverview.TotalVariance:N2}  |  EAC: {exec.FinancialOverview.EstimateAtCompletion:N2}  |  CPI: {exec.FinancialOverview.CostPerformanceIndex:N2}");
                                col.Item().PaddingVertical(4);
                            }
                            AddPdfTable(col, "Milestone Status", ["Activity", "Code", "Status", "Planned Start", "Planned Finish", "% Complete"],
                                exec.Milestones.Select(m => new[] { m.ActivityName, m.ActivityCode, m.Status, m.PlannedStart?.ToString("yyyy-MM-dd") ?? "-", m.PlannedFinish?.ToString("yyyy-MM-dd") ?? "-", $"{m.PercentComplete:P0}" }));
                            AddPdfTable(col, "Project Directory", ["Name", "Role"],
                                exec.ProjectParties.Select(p => new[] { p.Name, p.Role }));
                            break;
                    }
                });
                page.Footer().AlignCenter().Text(t =>
                    t.Span("Generated by Planova").FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Medium));
            });
        }).GeneratePdf();
    }

    private static void AddPdfSummaryCards(ColumnDescriptor col, params (string Label, string Value)[] cards)
    {
        col.Item().Row(r =>
        {
            foreach (var (label, value) in cards)
            {
                r.AutoItem().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(6).Column(c =>
                {
                    c.Item().Text(value).FontSize(18).Bold();
                    c.Item().Text(label).FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                });
            }
        });
    }

    private static void AddPdfTable(ColumnDescriptor col, string title, string[] headers, IEnumerable<string[]> rows)
    {
        col.Item().PaddingVertical(4).Text(title).FontSize(12).Bold();
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                foreach (var _ in headers)
                    c.RelativeColumn();
            });
            table.Header(header =>
            {
                foreach (var h in headers)
                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Darken2).Padding(3).Text(h).FontSize(8).FontColor(QuestPDF.Helpers.Colors.White).Bold();
            });
            foreach (var row in rows)
            {
                foreach (var cell in row)
                    table.Cell().BorderBottom(0.5f).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(2).Text(cell).FontSize(7);
            }
        });
    }

    private static byte[] GenerateWordExport(ReportInstance instance, object reportData)
    {
        using var ms = new MemoryStream();
        using var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        var body = mainPart.Document.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Body());

        void AddTitle(string text)
        {
            body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text(text))
                { RunProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties(new DocumentFormat.OpenXml.Wordprocessing.Bold(), new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = "28" }) }));
        }

        void AddSubTitle(string text)
        {
            body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text(text))
                { RunProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = "18" }) }));
        }

        void AddSection(string title)
        {
            body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text(title))
                { RunProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties(new DocumentFormat.OpenXml.Wordprocessing.Bold(), new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = "24" }) }));
        }

        void AddText(string text)
        {
            body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text(text))));
        }

        void AddTable(string[] headers, IEnumerable<string[]> rows)
        {
            var table = new DocumentFormat.OpenXml.Wordprocessing.Table();
            var headerRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
            foreach (var h in headers)
                headerRow.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.TableCell(
                    new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                        new DocumentFormat.OpenXml.Wordprocessing.Run(
                            new DocumentFormat.OpenXml.Wordprocessing.Text(h)))));
            table.AppendChild(headerRow);
            foreach (var row in rows)
            {
                var dataRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                foreach (var cell in row)
                    dataRow.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.TableCell(
                        new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                            new DocumentFormat.OpenXml.Wordprocessing.Run(
                                new DocumentFormat.OpenXml.Wordprocessing.Text(cell)))));
                table.AppendChild(dataRow);
            }
            body.AppendChild(table);
            body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(""))));
        }

        AddTitle(instance.Title);
        AddSubTitle($"Status: {instance.Status}  |  Generated: {instance.GeneratedAt:g}  |  Period: {instance.PeriodStart:d} — {instance.PeriodEnd:d}");

        switch (reportData)
        {
            case DailyReportDataDto daily:
                AddSection("Summary");
                AddText($"Total Activities: {daily.TotalActivities}  |  Completed: {daily.CompletedActivities}  |  In Progress: {daily.InProgressActivities}  |  Not Started: {daily.NotStartedActivities}");
                AddText($"Overall Progress: {daily.OverallPercentComplete:P1}");
                if (daily.ProgressToday.Count > 0)
                {
                    AddSection("Progress Today");
                    AddTable(["Code", "Activity", "% Complete", "Status"], daily.ProgressToday.Select(p => new[] { p.ActivityCode, p.ActivityName, $"{p.PercentComplete:P0}", p.Status }));
                }
                if (daily.Workforce.Count > 0)
                {
                    AddSection("Workforce");
                    AddTable(["Resource", "Activity", "Qty", "UOM"], daily.Workforce.Select(w => new[] { w.ResourceName, w.ActivityName, w.Quantity.ToString(), w.UnitOfMeasure }));
                }
                if (daily.Equipment.Count > 0)
                {
                    AddSection("Equipment");
                    AddTable(["Equipment", "Activity", "Qty", "UOM"], daily.Equipment.Select(e => new[] { e.EquipmentName, e.ActivityName, e.Quantity.ToString(), e.UnitOfMeasure }));
                }
                break;

            case WeeklyReportDataDto weekly:
                if (weekly.ProgressByWbs.Count > 0)
                {
                    AddSection("Progress by WBS");
                    AddTable(["Code", "Name", "Total", "Completed", "% Complete", "Weight"], weekly.ProgressByWbs.Select(p => new[] { p.WbsCode, p.WbsName, p.TotalActivities.ToString(), p.CompletedActivities.ToString(), $"{p.PercentComplete:P1}", $"{p.Weight:P1}" }));
                }
                if (weekly.ResourceUsage.Count > 0)
                {
                    AddSection("Resource Usage");
                    AddTable(["Resource", "Type", "Total Qty", "UOM"], weekly.ResourceUsage.Select(r => new[] { r.ResourceName, r.ResourceType, r.TotalQuantity.ToString(), r.UnitOfMeasure }));
                }
                if (weekly.Delays.Count > 0)
                {
                    AddSection("Delays");
                    AddTable(["Activity", "Code", "Planned Finish", "Actual Finish", "Delay (days)"], weekly.Delays.Select(d => new[] { d.ActivityName, d.ActivityCode, d.PlannedFinish?.ToString("d") ?? "-", d.ActualFinish?.ToString("d") ?? "-", d.DelayDays.ToString() }));
                }
                if (weekly.LookAhead.Count > 0)
                {
                    AddSection("Look-Ahead (Next 7 Days)");
                    AddTable(["Activity", "Code", "Planned Start", "Planned Finish", "Status"], weekly.LookAhead.Select(l => new[] { l.ActivityName, l.ActivityCode, l.PlannedStart?.ToString("d") ?? "-", l.PlannedFinish?.ToString("d") ?? "-", l.Status }));
                }
                break;

            case MonthlyReportDataDto monthly:
                if (monthly.EvmSummary != null)
                {
                    AddSection("EVM Summary");
                    AddText($"PV: {monthly.EvmSummary.PlannedValue:N2}  |  EV: {monthly.EvmSummary.EarnedValue:N2}  |  AC: {monthly.EvmSummary.ActualCost:N2}");
                    AddText($"CPI: {monthly.EvmSummary.CostPerformanceIndex:N2}  |  SPI: {monthly.EvmSummary.SchedulePerformanceIndex:N2}  |  EAC: {monthly.EvmSummary.EstimateAtCompletion:N2}");
                }
                if (monthly.BudgetVsActual.Count > 0)
                {
                    AddSection("Budget vs Actual");
                    AddTable(["Category", "Budget", "Actual", "Variance"], monthly.BudgetVsActual.Select(b => new[] { b.Category, b.Budget.ToString("N2"), b.Actual.ToString("N2"), b.Variance.ToString("N2") }));
                }
                if (monthly.ProgressByWbs.Count > 0)
                {
                    AddSection("Progress by WBS");
                    AddTable(["Code", "Name", "% Complete", "Weight"], monthly.ProgressByWbs.Select(p => new[] { p.WbsCode, p.WbsName, $"{p.PercentComplete:P1}", $"{p.Weight:P1}" }));
                }
                if (monthly.ResourceProductivity.Count > 0)
                {
                    AddSection("Resource Productivity");
                    AddTable(["Resource", "Type", "Total Qty", "Total Cost", "UOM"], monthly.ResourceProductivity.Select(r => new[] { r.ResourceName, r.ResourceType, r.TotalQuantity.ToString(), r.TotalCost.ToString("N2"), r.UnitOfMeasure }));
                }
                break;

            case ExecutiveReportDataDto exec:
                if (exec.KpiCards.Count > 0)
                {
                    AddSection("KPI Dashboard");
                    foreach (var k in exec.KpiCards)
                        AddText($"{k.Label}: {k.Value:N2} {k.Unit}  ({k.Trend ?? "-"})");
                }
                if (exec.SCurve.Count > 0)
                {
                    AddSection("S-Curve");
                    AddTable(["Date", "Planned Value", "Earned Value", "Actual Cost"], exec.SCurve.Select(s => new[] { s.Date.ToString("yyyy-MM"), s.PlannedValue.ToString("N2"), s.EarnedValue.ToString("N2"), s.ActualCost.ToString("N2") }));
                }
                if (exec.FinancialOverview != null)
                {
                    AddSection("Financial Overview");
                    AddText($"Budget: {exec.FinancialOverview.TotalBudget:N2}  |  Actual: {exec.FinancialOverview.TotalActualCost:N2}  |  Variance: {exec.FinancialOverview.TotalVariance:N2}");
                }
                if (exec.Milestones.Count > 0)
                {
                    AddSection("Milestone Status");
                    AddTable(["Activity", "Code", "Status", "Planned Start", "Planned Finish", "% Complete"], exec.Milestones.Select(m => new[] { m.ActivityName, m.ActivityCode, m.Status, m.PlannedStart?.ToString("yyyy-MM-dd") ?? "-", m.PlannedFinish?.ToString("yyyy-MM-dd") ?? "-", $"{m.PercentComplete:P0}" }));
                }
                if (exec.ProjectParties.Count > 0)
                {
                    AddSection("Project Directory");
                    AddTable(["Name", "Role"], exec.ProjectParties.Select(p => new[] { p.Name, p.Role }));
                }
                break;
        }

        mainPart.Document.Save();
        return ms.ToArray();
    }
}
