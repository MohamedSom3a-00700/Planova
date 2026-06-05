using ClosedXML.Excel;
using Planova.Resource.Application.Dto;
using Planova.Resource.Application.Mappings;
using Planova.Resource.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Planova.Resource.Application.Services;

public class ResourceReportService : IResourceReportService
{
    private readonly IResourceAssignmentRepository _assignmentRepository;

    public ResourceReportService(IResourceAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<ResourceUsageReportDto> GenerateUsageSummaryAsync(int projectId, CancellationToken ct = default)
    {
        var assignments = await _assignmentRepository.GetByProjectAsync(projectId, ct);

        var activityGroups = assignments
            .GroupBy(a => a.ActivityId)
            .ToList();

        var activities = activityGroups.Select(g =>
        {
            var first = g.First();
            return new ActivityResourceSection
            {
                ActivityCode = g.Key.ToString()[..8],
                ActivityName = $"Activity {g.Key.ToString()[..8]}",
                Assignments = g.Select(a => a.ToDto()).ToList(),
                ActivityTotalCost = g.Sum(a => a.TotalCost)
            };
        }).ToList();

        return new ResourceUsageReportDto
        {
            ProjectId = projectId,
            GeneratedAt = DateTime.UtcNow,
            Activities = activities,
            TotalCost = activities.Sum(a => a.ActivityTotalCost)
        };
    }

    public async Task<ResourceCostReportDto> GenerateCostReportAsync(int projectId, CancellationToken ct = default)
    {
        var assignments = await _assignmentRepository.GetByProjectAsync(projectId, ct);

        var typeGroups = assignments
            .GroupBy(a => a.Resource?.ResourceType.ToString() ?? "Unknown")
            .ToList();

        var sections = typeGroups.Select(g => new CostSummarySection
        {
            SectionName = g.Key,
            LineItems = g.Select(a => new CostLineItem
            {
                Label = a.Resource?.Name ?? "Unknown",
                TotalCost = a.TotalCost,
                Currency = a.Currency,
                AssignmentCount = 1
            }).ToList()
        }).ToList();

        return new ResourceCostReportDto
        {
            ProjectId = projectId,
            GeneratedAt = DateTime.UtcNow,
            CostSections = sections,
            GrandTotal = assignments.Sum(a => a.TotalCost)
        };
    }

    public async Task<byte[]> ExportToExcelAsync(int projectId, ReportType type, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        IXLWorksheet ws;

        if (type == ReportType.UsageSummary)
        {
            var report = await GenerateUsageSummaryAsync(projectId, ct);
            ws = workbook.Worksheets.Add("Usage Summary");

            ws.Cell(1, 1).Value = "Activity";
            ws.Cell(1, 2).Value = "Resource";
            ws.Cell(1, 3).Value = "Quantity";
            ws.Cell(1, 4).Value = "Rate";
            ws.Cell(1, 5).Value = "Total Cost";

            int row = 2;
            foreach (var activity in report.Activities)
            {
                ws.Cell(row, 1).Value = activity.ActivityName;
                foreach (var assignment in activity.Assignments)
                {
                    ws.Cell(row, 2).Value = assignment.ResourceName;
                    ws.Cell(row, 3).Value = (double)assignment.Quantity;
                    ws.Cell(row, 4).Value = (double)assignment.Rate;
                    ws.Cell(row, 5).Value = (double)assignment.TotalCost;
                    row++;
                }
                ws.Cell(row, 1).Value = "Subtotal";
                ws.Cell(row, 5).Value = (double)activity.ActivityTotalCost;
                row++;
            }

            ws.Cell(row, 1).Value = "GRAND TOTAL";
            ws.Cell(row, 5).Value = (double)report.TotalCost;
        }
        else
        {
            var report = await GenerateCostReportAsync(projectId, ct);
            ws = workbook.Worksheets.Add("Cost Report");

            ws.Cell(1, 1).Value = "Type";
            ws.Cell(1, 2).Value = "Resource";
            ws.Cell(1, 3).Value = "Total Cost";
            ws.Cell(1, 4).Value = "Currency";

            int row = 2;
            foreach (var section in report.CostSections)
            {
                foreach (var item in section.LineItems)
                {
                    ws.Cell(row, 1).Value = section.SectionName;
                    ws.Cell(row, 2).Value = item.Label;
                    ws.Cell(row, 3).Value = (double)item.TotalCost;
                    ws.Cell(row, 4).Value = item.Currency;
                    row++;
                }
            }

            ws.Cell(row, 1).Value = "GRAND TOTAL";
            ws.Cell(row, 3).Value = (double)report.GrandTotal;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToPdfAsync(int projectId, ReportType type, CancellationToken ct = default)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => c.Column(col =>
                {
                    col.Item().Text($"Resource Report - {type}").Bold().FontSize(16);
                    col.Item().Text($"Generated: {DateTime.UtcNow:g}");
                }));

                page.Content().Element(c =>
                {
                    if (type == ReportType.UsageSummary)
                    {
                        var report = GenerateUsageSummaryAsync(projectId, ct).GetAwaiter().GetResult();
                        c.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Activity").Bold();
                                header.Cell().Text("Resource").Bold();
                                header.Cell().Text("Cost").Bold();
                            });

                            foreach (var activity in report.Activities)
                            {
                                foreach (var assignment in activity.Assignments)
                                {
                                    table.Cell().Text(activity.ActivityName);
                                    table.Cell().Text(assignment.ResourceName);
                                    table.Cell().Text($"${assignment.TotalCost:N2}");
                                }
                            }
                        });
                    }
                    else
                    {
                        var report = GenerateCostReportAsync(projectId, ct).GetAwaiter().GetResult();
                        c.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Type").Bold();
                                header.Cell().Text("Resource").Bold();
                                header.Cell().Text("Cost").Bold();
                            });

                            foreach (var section in report.CostSections)
                            {
                                foreach (var item in section.LineItems)
                                {
                                    table.Cell().Text(section.SectionName);
                                    table.Cell().Text(item.Label);
                                    table.Cell().Text($"${item.TotalCost:N2}");
                                }
                            }
                        });
                    }
                });

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
}
