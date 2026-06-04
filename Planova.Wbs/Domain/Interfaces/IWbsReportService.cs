using Planova.Wbs.Application.Dto;

namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsReportService
{
    Task<WbsSummaryReport> GenerateSummaryAsync(Guid wbsId, CancellationToken ct);
    Task<WbsDictionaryReport> GenerateDictionaryAsync(Guid wbsId, CancellationToken ct);
    Task<byte[]> ExportToExcelAsync(Guid wbsId, ReportType type, CancellationToken ct);
    Task<byte[]> ExportToPdfAsync(Guid wbsId, ReportType type, CancellationToken ct);
}
