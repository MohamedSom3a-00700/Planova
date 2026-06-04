using Planova.Boq.Application.Dto;

namespace Planova.Boq.Domain.Interfaces;

public interface IBoqExportService
{
    Task<ExportResult> ExportToExcelAsync(Guid boqId, ExportOptions options, CancellationToken ct);
    Task<ExportResult> ExportToCsvAsync(Guid boqId, ExportOptions options, CancellationToken ct);
}
