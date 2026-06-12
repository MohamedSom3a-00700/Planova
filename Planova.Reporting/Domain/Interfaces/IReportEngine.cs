using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Interfaces;

public interface IReportEngine
{
    Task<ReportInstanceDto> GenerateAsync(int projectId, ReportType reportType, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
    Task<ReportInstanceDto> RegenerateNarrativeAsync(Guid instanceId, CancellationToken ct = default);
}
