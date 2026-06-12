using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Entities;

namespace Planova.Reporting.Domain.Interfaces;

public interface IReportSchedulerService
{
    Task<List<ReportScheduleDto>> GetSchedulesAsync(int projectId, CancellationToken ct = default);
    Task<ReportScheduleDto> CreateAsync(CreateScheduleRequest request, CancellationToken ct = default);
    Task<ReportScheduleDto> UpdateAsync(Guid id, UpdateScheduleRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ToggleActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<DateTime> ComputeNextRunAsync(ReportSchedule schedule, CancellationToken ct = default);
}
