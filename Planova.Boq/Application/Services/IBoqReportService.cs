using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Enums;

namespace Planova.Boq.Domain.Interfaces;

public interface IBoqReportService
{
    Task<byte[]> GenerateSummaryReportAsync(Guid boqId, ReportFormat format, CancellationToken ct);
    Task<byte[]> GenerateItemizedReportAsync(Guid boqId, ReportFormat format, CancellationToken ct);
    Task SaveReportAsync(Guid boqId, ReportType type, ReportFormat format, string outputPath, CancellationToken ct);
}
