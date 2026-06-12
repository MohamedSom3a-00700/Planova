using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Interfaces;

public interface IReportScheduleRepository
{
    Task<List<ReportSchedule>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<ReportSchedule?> GetByProjectAndTypeAsync(int projectId, ReportType reportType, CancellationToken ct = default);
    Task<ReportSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ReportSchedule>> GetDueSchedulesAsync(DateTime utcNow, CancellationToken ct = default);
    Task AddAsync(ReportSchedule schedule, CancellationToken ct = default);
    Task UpdateAsync(ReportSchedule schedule, CancellationToken ct = default);
    Task DeleteAsync(ReportSchedule schedule, CancellationToken ct = default);
}
