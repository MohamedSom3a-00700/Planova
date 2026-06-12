using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Interfaces;

public interface IReportSettingsService
{
    Task<ReportSettingsDto> GetSettingsAsync(int projectId, ReportType reportType, CancellationToken ct = default);
    Task<ReportSettingsDto> UpdateSettingsAsync(int projectId, ReportType reportType, UpdateSettingsRequest request, CancellationToken ct = default);
    Task ResetToDefaultsAsync(int projectId, ReportType reportType, CancellationToken ct = default);
    Task<List<string>> GetEnabledSectionsAsync(int projectId, ReportType reportType, CancellationToken ct = default);
}
