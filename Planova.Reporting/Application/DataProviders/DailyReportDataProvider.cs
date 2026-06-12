using Planova.Activity.Domain.Interfaces;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using Planova.Resource.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Reporting.Application.DataProviders;

public class DailyReportDataProvider : IReportDataProvider<DailyReportDataDto>
{
    private readonly IActivityService _activityService;
    private readonly IResourceAssignmentService _assignmentService;
    private readonly ILoggingService _logger;

    public DailyReportDataProvider(
        IActivityService activityService,
        IResourceAssignmentService assignmentService,
        ILoggingService logger)
    {
        _activityService = activityService;
        _assignmentService = assignmentService;
        _logger = logger;
    }

    public ReportType HandledType => ReportType.Daily;

    public async Task<DailyReportDataDto> CollectDataAsync(int projectId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        _logger.Info("Collecting daily report data for project {ProjectId} on {Date}", projectId, periodStart.Date);

        var activities = await _activityService.GetByProjectAsync(projectId, null, ct);
        var assignments = await _assignmentService.GetByProjectAsync(projectId, ct);

        var reportDate = periodStart.Date;

        var totalActivities = activities.Count(a => a.ActivityType != "WbsSummary");
        var completed = activities.Count(a => a.Status == "Completed" && a.ActivityType != "WbsSummary");
        var inProgress = activities.Count(a => a.Status == "InProgress" && a.ActivityType != "WbsSummary");
        var notStarted = activities.Count(a => a.Status == "NotStarted" && a.ActivityType != "WbsSummary");
        var overallPct = totalActivities > 0
            ? activities.Where(a => a.ActivityType != "WbsSummary").Average(a => a.PercentComplete ?? 0m)
            : 0m;

        var progressToday = activities
            .Where(a => a.ActivityType != "WbsSummary")
            .Select(a => new DailyProgressItem(
                a.Name,
                a.Code,
                a.PercentComplete,
                a.Status,
                a.PlannedStart,
                a.PlannedFinish,
                a.ActualStart,
                a.ActualFinish))
            .ToList();

        var workforce = assignments
            .Where(a => a.ResourceType == Planova.Resource.Domain.Enums.ResourceType.Labour)
            .Select(a => new DailyWorkforceItem(
                a.ResourceName,
                a.ResourceCode,
                a.ActivityName,
                a.Quantity,
                a.UnitOfMeasure))
            .ToList();

        var equipment = assignments
            .Where(a => a.ResourceType == Planova.Resource.Domain.Enums.ResourceType.Equipment)
            .Select(a => new DailyEquipmentItem(
                a.ResourceName,
                a.ResourceCode,
                a.ActivityName,
                a.Quantity,
                a.UnitOfMeasure))
            .ToList();

        var dto = new DailyReportDataDto(
            reportDate,
            totalActivities,
            completed,
            inProgress,
            notStarted,
            overallPct,
            progressToday,
            workforce,
            equipment,
            new List<DailyIssueItem>(),
            new List<DailyPhotoItem>()
        );

        _logger.Info("Daily report data collected: {Total} activities, {Completed} completed, {Workforce} workforce, {Equipment} equipment",
            totalActivities, completed, workforce.Count, equipment.Count);
        return dto;
    }
}
