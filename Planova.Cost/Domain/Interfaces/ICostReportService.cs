using Planova.Cost.Application.Dto;

namespace Planova.Cost.Domain.Interfaces;

public interface ICostReportService
{
    Task<ReportResultDto> GenerateReportAsync(
        CostReportType reportType, int projectId, CancellationToken ct = default);
    Task<byte[]> ExportToExcelAsync(
        CostReportType reportType, int projectId, CancellationToken ct = default);
    Task<byte[]> ExportToPdfAsync(
        CostReportType reportType, int projectId, CancellationToken ct = default);
}
