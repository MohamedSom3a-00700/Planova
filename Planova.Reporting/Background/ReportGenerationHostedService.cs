using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.Reporting.Background;

public class ReportGenerationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReportGenerationHostedService> _logger;

    public ReportGenerationHostedService(
        IServiceProvider serviceProvider,
        ILogger<ReportGenerationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReportGenerationHostedService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var scheduleRepo = scope.ServiceProvider.GetRequiredService<IReportScheduleRepository>();
                var engine = scope.ServiceProvider.GetRequiredService<IReportEngine>();

                var dueSchedules = await scheduleRepo.GetDueSchedulesAsync(DateTime.UtcNow, stoppingToken);

                foreach (var schedule in dueSchedules)
                {
                    try
                    {
                        _logger.LogInformation("Generating scheduled report for schedule {ScheduleId}", schedule.Id);

                        var periodEnd = DateTime.UtcNow;
                        var periodStart = schedule.Frequency switch
                        {
                            Reporting.Domain.Enums.ScheduleFrequency.Daily => periodEnd.AddDays(-1),
                            Reporting.Domain.Enums.ScheduleFrequency.Weekly => periodEnd.AddDays(-7),
                            Reporting.Domain.Enums.ScheduleFrequency.Monthly => periodEnd.AddMonths(-1),
                            _ => periodEnd.AddDays(-1)
                        };

                        await engine.GenerateAsync(
                            schedule.ProjectId,
                            schedule.ReportType,
                            periodStart,
                            periodEnd,
                            stoppingToken);

                        schedule.LastRunAt = DateTime.UtcNow;
                        schedule.LastStatus = "Success";
                        schedule.LastSuccessfulRunAt = DateTime.UtcNow;
                        schedule.RetryCount = 0;

                        var scheduler = scope.ServiceProvider.GetRequiredService<IReportSchedulerService>();
                        schedule.NextRunAt = await scheduler.ComputeNextRunAsync(schedule, stoppingToken);

                        await scheduleRepo.UpdateAsync(schedule, stoppingToken);

                        _logger.LogInformation("Scheduled report generation completed for schedule {ScheduleId}", schedule.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate scheduled report for schedule {ScheduleId}", schedule.Id);
                        schedule.LastStatus = "Failed";
                        schedule.LastErrorMessage = ex.Message;
                        schedule.RetryCount++;

                        if (schedule.RetryCount >= schedule.MaxRetries)
                        {
                            schedule.IsActive = false;
                            _logger.LogWarning("Schedule {ScheduleId} deactivated after {RetryCount} failures", schedule.Id, schedule.RetryCount);
                        }

                        await scheduleRepo.UpdateAsync(schedule, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReportGenerationHostedService tick");
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }

        _logger.LogInformation("ReportGenerationHostedService stopped.");
    }
}
