using Planova.Reporting.Application.Dto;
using Planova.Reporting.Application.Mappings;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.Reporting.Application.Services;

public class ReportSettingsService : IReportSettingsService
{
    private readonly IReportTemplateRepository _templateRepository;

    public ReportSettingsService(IReportTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public Task<ReportSettingsDto> GetSettingsAsync(int projectId, ReportType reportType, CancellationToken ct = default)
    {
        var settings = new ReportSettingsDto(
            Guid.Empty, projectId, reportType.ToString(), "[]",
            DateTime.UtcNow, DateTime.UtcNow);

        return Task.FromResult(settings);
    }

    public Task<ReportSettingsDto> UpdateSettingsAsync(int projectId, ReportType reportType, UpdateSettingsRequest request, CancellationToken ct = default)
    {
        var settings = new ReportSettingsDto(
            Guid.NewGuid(), projectId, reportType.ToString(), request.EnabledSectionsJson,
            DateTime.UtcNow, DateTime.UtcNow);

        return Task.FromResult(settings);
    }

    public Task ResetToDefaultsAsync(int projectId, ReportType reportType, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public async Task<List<string>> GetEnabledSectionsAsync(int projectId, ReportType reportType, CancellationToken ct = default)
    {
        var template = await _templateRepository.GetDefaultForProjectAsync(projectId, reportType, ct);
        if (template == null)
            return new List<string>();

        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(template.LayoutJson) ?? new List<string>();
    }
}
