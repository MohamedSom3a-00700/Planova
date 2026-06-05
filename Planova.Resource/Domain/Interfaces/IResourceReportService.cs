using Planova.Resource.Application.Dto;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceReportService
{
    Task<ResourceUsageReportDto> GenerateUsageSummaryAsync(int projectId, CancellationToken ct = default);
    Task<ResourceCostReportDto> GenerateCostReportAsync(int projectId, CancellationToken ct = default);
    Task<byte[]> ExportToExcelAsync(int projectId, ReportType type, CancellationToken ct = default);
    Task<byte[]> ExportToPdfAsync(int projectId, ReportType type, CancellationToken ct = default);
}
