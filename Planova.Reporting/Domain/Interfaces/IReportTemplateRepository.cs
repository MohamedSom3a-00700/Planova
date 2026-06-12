using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Interfaces;

public interface IReportTemplateRepository
{
    Task<List<ReportTemplate>> GetByProjectAsync(int projectId, ReportType? reportType = null, CancellationToken ct = default);
    Task<List<ReportTemplate>> GetGlobalTemplatesAsync(ReportType? reportType = null, CancellationToken ct = default);
    Task<ReportTemplate?> GetDefaultForProjectAsync(int projectId, ReportType reportType, CancellationToken ct = default);
    Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ReportTemplate template, CancellationToken ct = default);
    Task UpdateAsync(ReportTemplate template, CancellationToken ct = default);
    Task DeleteAsync(ReportTemplate template, CancellationToken ct = default);
}
