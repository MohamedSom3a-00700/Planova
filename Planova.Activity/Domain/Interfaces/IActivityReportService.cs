using Planova.Activity.Application.Dto;

namespace Planova.Activity.Domain.Interfaces;

public interface IActivityReportService
{
    Task<ScheduleReportDto> GenerateScheduleReportAsync(int projectId, CancellationToken ct = default);
    Task<byte[]> ExportToExcelAsync(int projectId, CancellationToken ct = default);
    Task<byte[]> ExportToPdfAsync(int projectId, CancellationToken ct = default);
}
