using System.Text.Json;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Application.Mappings;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Reporting.Application.Services;

public class ReportEngine : IReportEngine
{
    private readonly IReportInstanceRepository _instanceRepository;
    private readonly IReportTemplateRepository _templateRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggingService _logger;

    public ReportEngine(
        IReportInstanceRepository instanceRepository,
        IReportTemplateRepository templateRepository,
        IServiceProvider serviceProvider,
        ILoggingService logger)
    {
        _instanceRepository = instanceRepository;
        _templateRepository = templateRepository;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ReportInstanceDto> GenerateAsync(int projectId, ReportType reportType, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        _logger.Info("Generating {ReportType} report for project {ProjectId} [{PeriodStart:d} — {PeriodEnd:d}]", reportType, projectId, periodStart, periodEnd);

        var instance = new ReportInstance
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ReportType = reportType,
            Title = $"{reportType} Report — {periodStart:d} to {periodEnd:d}",
            Status = ReportStatus.Draft,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GeneratedAt = DateTime.UtcNow,
            DataSnapshotJson = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var template = await _templateRepository.GetDefaultForProjectAsync(projectId, reportType, ct);
        if (template != null)
            instance.TemplateId = template.Id;

        await _instanceRepository.AddAsync(instance, ct);

        _logger.Info("Report instance {InstanceId} created for project {ProjectId}", instance.Id, projectId);
        return instance.ToDto();
    }

    public async Task<ReportInstanceDto> RegenerateNarrativeAsync(Guid instanceId, CancellationToken ct = default)
    {
        _logger.Info("Regenerating narrative for report instance {InstanceId}", instanceId);

        var instance = await _instanceRepository.GetByIdAsync(instanceId, ct);
        if (instance == null)
        {
            _logger.Error("Report instance {InstanceId} not found for narrative regeneration", new InvalidOperationException($"Report instance {instanceId} not found."), instanceId);
            throw new InvalidOperationException($"Report instance {instanceId} not found.");
        }

        instance.UpdatedAt = DateTime.UtcNow;
        await _instanceRepository.UpdateAsync(instance, ct);

        _logger.Info("Narrative regenerated for report instance {InstanceId}", instanceId);
        return instance.ToDto();
    }
}
