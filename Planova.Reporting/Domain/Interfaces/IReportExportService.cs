using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Interfaces;

public interface IReportExportService
{
    Task<byte[]> ExportToExcelAsync(Guid instanceId, object reportData, CancellationToken ct = default);
    Task<byte[]> ExportToPdfAsync(Guid instanceId, object reportData, CancellationToken ct = default);
    Task<byte[]> ExportToWordAsync(Guid instanceId, object reportData, CancellationToken ct = default);
    Task<ReportExportDto> SaveExportAsync(Guid instanceId, ExportFormat format, byte[] fileContent, string exportedBy, CancellationToken ct = default);
    Task DeleteExportFileAsync(Guid exportId, CancellationToken ct = default);
}
