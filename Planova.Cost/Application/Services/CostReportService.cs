using ClosedXML.Excel;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Planova.Cost.Application.Services;

public class CostReportService : ICostReportService
{
    private readonly ICostService _costService;
    private readonly ICashFlowService _cashFlowService;
    private readonly IEvmService _evmService;
    private readonly ICostAiService _aiService;

    public CostReportService(
        ICostService costService,
        ICashFlowService cashFlowService,
        IEvmService evmService,
        ICostAiService aiService)
    {
        _costService = costService;
        _cashFlowService = cashFlowService;
        _evmService = evmService;
        _aiService = aiService;
    }

    public async Task<ReportResultDto> GenerateReportAsync(
        CostReportType reportType, int projectId, CancellationToken ct = default)
    {
        var data = await GetReportDataAsync(reportType, projectId, ct);
        var narrative = _aiService.IsAvailable
            ? await _aiService.GenerateNarrativeAsync(projectId, ct)
            : null;

        return new ReportResultDto(reportType, projectId, DateTime.UtcNow, data, narrative);
    }

    public async Task<byte[]> ExportToExcelAsync(
        CostReportType reportType, int projectId, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(reportType.ToString());

        switch (reportType)
        {
            case CostReportType.CostBreakdown:
                await WriteCostBreakdownExcelAsync(ws, projectId, ct);
                break;
            case CostReportType.CashFlow:
                await WriteCashFlowExcelAsync(ws, projectId, ct);
                break;
            case CostReportType.Evm:
                await WriteEvmExcelAsync(ws, projectId, ct);
                break;
            case CostReportType.BudgetSummary:
                await WriteBudgetSummaryExcelAsync(ws, projectId, ct);
                break;
        }

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToPdfAsync(
        CostReportType reportType, int projectId, CancellationToken ct = default)
    {
        var narrative = _aiService.IsAvailable
            ? await _aiService.GenerateNarrativeAsync(projectId, ct)
            : null;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => c.Column(col =>
                {
                    col.Item().Text($"{reportType} Report").Bold().FontSize(16);
                    col.Item().Text($"Project: {projectId} | Generated: {DateTime.UtcNow:g}");
                }));

                page.Content().Element(c => BuildPdfContent(c, reportType, projectId, narrative, ct));

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        using var stream = new MemoryStream();
        doc.GeneratePdf(stream);
        return stream.ToArray();
    }

    private async Task<object?> GetReportDataAsync(CostReportType type, int projectId, CancellationToken ct)
    {
        return type switch
        {
            CostReportType.CostBreakdown => await _costService.GetCostBreakdownAsync(projectId, ct),
            CostReportType.CashFlow => await _cashFlowService.GetCashFlowAsync(projectId, CashFlowPeriodType.Monthly, null, ct),
            CostReportType.Evm => await _evmService.ComputeMetricsAsync(projectId, DateTime.UtcNow, ct),
            CostReportType.BudgetSummary => await _costService.GetBudgetAsync(projectId, ct),
            _ => null
        };
    }

    private async Task WriteCostBreakdownExcelAsync(IXLWorksheet ws, int projectId, CancellationToken ct)
    {
        var breakdown = await _costService.GetCostBreakdownAsync(projectId, ct);
        ws.Cell(1, 1).Value = "Node Type";
        ws.Cell(1, 2).Value = "Label";
        ws.Cell(1, 3).Value = "Planned Cost";
        ws.Cell(1, 4).Value = "Actual Cost";
        ws.Cell(1, 5).Value = "Variance";

        int row = 2;
        WriteBreakdownRow(ws, row++, breakdown);
        foreach (var child in breakdown.Children)
        {
            WriteBreakdownRow(ws, row++, child);
        }
    }

    private static void WriteBreakdownRow(IXLWorksheet ws, int row, CostBreakdownDto item)
    {
        ws.Cell(row, 1).Value = item.NodeType;
        ws.Cell(row, 2).Value = item.Label;
        ws.Cell(row, 3).Value = (double)item.PlannedCost;
        ws.Cell(row, 4).Value = (double)item.ActualCost;
        ws.Cell(row, 5).Value = (double)item.Variance;
    }

    private async Task WriteCashFlowExcelAsync(IXLWorksheet ws, int projectId, CancellationToken ct)
    {
        var periods = await _cashFlowService.GetCashFlowAsync(projectId, CashFlowPeriodType.Monthly, null, ct);
        ws.Cell(1, 1).Value = "Period Start";
        ws.Cell(1, 2).Value = "Period End";
        ws.Cell(1, 3).Value = "Planned Cost";
        ws.Cell(1, 4).Value = "Actual Cost";
        ws.Cell(1, 5).Value = "Cumulative Planned";
        ws.Cell(1, 6).Value = "Cumulative Actual";

        for (int i = 0; i < periods.Count; i++)
        {
            var p = periods[i];
            var r = i + 2;
            ws.Cell(r, 1).Value = p.PeriodStart.ToString("d");
            ws.Cell(r, 2).Value = p.PeriodEnd.ToString("d");
            ws.Cell(r, 3).Value = (double)p.PlannedCost;
            ws.Cell(r, 4).Value = (double)p.ActualCost;
            ws.Cell(r, 5).Value = (double)p.CumulativePlanned;
            ws.Cell(r, 6).Value = (double)p.CumulativeActual;
        }
    }

    private async Task WriteEvmExcelAsync(IXLWorksheet ws, int projectId, CancellationToken ct)
    {
        var metrics = await _evmService.ComputeMetricsAsync(projectId, DateTime.UtcNow, ct);
        ws.Cell(1, 1).Value = "Metric";
        ws.Cell(1, 2).Value = "Value";

        var fields = new Dictionary<string, object>
        {
            ["Data Date"] = metrics.DataDate.ToString("d"),
            ["Planned Value (PV)"] = (double)metrics.PlannedValue,
            ["Earned Value (EV)"] = (double)metrics.EarnedValue,
            ["Actual Cost (AC)"] = (double)metrics.ActualCost,
            ["Cost Variance (CV)"] = (double)metrics.CostVariance,
            ["Schedule Variance (SV)"] = (double)metrics.ScheduleVariance,
            ["CPI"] = (double)(metrics.CostPerformanceIndex ?? 0),
            ["SPI"] = (double)(metrics.SchedulePerformanceIndex ?? 0),
            ["BAC"] = (double)metrics.BudgetAtCompletion,
            ["EAC"] = (double)metrics.EstimateAtCompletion,
            ["ETC"] = (double)metrics.EstimateToComplete,
            ["VAC"] = (double)metrics.VarianceAtCompletion,
            ["Status"] = metrics.StatusColor
        };

        int row = 2;
        foreach (var kv in fields)
        {
            ws.Cell(row, 1).Value = kv.Key;
            ws.Cell(row, 2).Value = ToCellValue(kv.Value);
            row++;
        }
    }

    private async Task WriteBudgetSummaryExcelAsync(IXLWorksheet ws, int projectId, CancellationToken ct)
    {
        var budget = await _costService.GetBudgetAsync(projectId, ct);
        ws.Cell(1, 1).Value = "Field";
        ws.Cell(1, 2).Value = "Value";

        var fields = new Dictionary<string, object>
        {
            ["Resource Cost Total"] = (double)budget.ResourceCostTotal,
            ["Direct Cost Total"] = (double)budget.DirectCostTotal,
            ["Contingency Amount"] = (double)(budget.ContingencyAmount ?? 0),
            ["Contingency %"] = (double)(budget.ContingencyPercent ?? 0),
            ["Total Budget"] = (double)budget.TotalBudget,
            ["Currency"] = budget.Currency,
            ["Status"] = budget.Status,
            ["Manual Override"] = budget.IsManualOverride
        };

        int row = 2;
        foreach (var kv in fields)
        {
            ws.Cell(row, 1).Value = kv.Key;
            ws.Cell(row, 2).Value = ToCellValue(kv.Value);
            row++;
        }
    }

    private static XLCellValue ToCellValue(object value)
    {
        return value switch
        {
            string s => s,
            int i => i,
            long l => l,
            double d => d,
            decimal m => (double)m,
            bool b => b,
            DateTime dt => dt,
            _ => value.ToString() ?? string.Empty
        };
    }

    private void BuildPdfContent(IContainer c, CostReportType reportType, int projectId, string? narrative, CancellationToken ct)
    {
        c.Column(col =>
        {
            if (!string.IsNullOrEmpty(narrative))
            {
                col.Item().Background(Colors.Blue.Lighten5).Padding(8).Text(narrative).FontSize(9).Italic();
                col.Item().PaddingVertical(4);
            }

            col.Item().Element(content =>
            {
                if (reportType == CostReportType.Evm)
                {
                    var metrics = _evmService.ComputeMetricsAsync(projectId, DateTime.UtcNow, ct).GetAwaiter().GetResult();
                    content.Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Metric").Bold();
                            header.Cell().Text("Value").Bold();
                        });

                        table.Cell().Text("Planned Value (PV)");
                        table.Cell().Text($"{metrics.PlannedValue:N2}");
                        table.Cell().Text("Earned Value (EV)");
                        table.Cell().Text($"{metrics.EarnedValue:N2}");
                        table.Cell().Text("Actual Cost (AC)");
                        table.Cell().Text($"{metrics.ActualCost:N2}");
                        table.Cell().Text("CPI");
                        table.Cell().Text($"{metrics.CostPerformanceIndex:N2}");
                        table.Cell().Text("SPI");
                        table.Cell().Text($"{metrics.SchedulePerformanceIndex:N2}");
                        table.Cell().Text("EAC");
                        table.Cell().Text($"{metrics.EstimateAtCompletion:N2}");
                        table.Cell().Text("VAC");
                        table.Cell().Text($"{metrics.VarianceAtCompletion:N2}");
                    });
                }
                else if (reportType == CostReportType.CostBreakdown)
                {
                    var breakdown = _costService.GetCostBreakdownAsync(projectId, ct).GetAwaiter().GetResult();
                    content.Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Category").Bold();
                            header.Cell().Text("Planned").Bold();
                            header.Cell().Text("Actual").Bold();
                        });

                        foreach (var child in breakdown.Children)
                        {
                            table.Cell().Text(child.Label);
                            table.Cell().Text($"{child.PlannedCost:N2}");
                            table.Cell().Text($"{child.ActualCost:N2}");
                        }
                    });
                }
                else if (reportType == CostReportType.BudgetSummary)
                {
                    var budget = _costService.GetBudgetAsync(projectId, ct).GetAwaiter().GetResult();
                    content.Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Item").Bold();
                            header.Cell().Text("Amount").Bold();
                        });

                        table.Cell().Text("Resource Cost");
                        table.Cell().Text($"{budget.ResourceCostTotal:N2}");
                        table.Cell().Text("Direct Cost");
                        table.Cell().Text($"{budget.DirectCostTotal:N2}");
                        table.Cell().Text("Contingency");
                        table.Cell().Text($"{budget.ContingencyAmount:N2}");
                        table.Cell().Text("Total Budget");
                        table.Cell().Text($"{budget.TotalBudget:N2}");
                    });
                }
            });
        });
    }
}
