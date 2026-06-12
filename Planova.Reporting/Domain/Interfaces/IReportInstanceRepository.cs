using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Interfaces;

public interface IReportInstanceRepository
{
    Task<ReportInstance?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ReportInstance>> GetByProjectAsync(int projectId, ReportType? type = null, ReportStatus? status = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task AddAsync(ReportInstance instance, CancellationToken ct = default);
    Task UpdateAsync(ReportInstance instance, CancellationToken ct = default);
    Task DeleteAsync(ReportInstance instance, CancellationToken ct = default);
}
