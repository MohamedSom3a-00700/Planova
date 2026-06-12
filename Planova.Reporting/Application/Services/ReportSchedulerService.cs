using Planova.Reporting.Application.Dto;
using Planova.Reporting.Application.Mappings;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Reporting.Application.Services;

public class ReportSchedulerService : IReportSchedulerService
{
    private readonly IReportScheduleRepository _repository;
    private readonly ILoggingService _logger;

    public ReportSchedulerService(IReportScheduleRepository repository, ILoggingService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<ReportScheduleDto>> GetSchedulesAsync(int projectId, CancellationToken ct = default)
    {
        var schedules = await _repository.GetByProjectAsync(projectId, ct);
        return schedules.Select(s => s.ToDto()).ToList();
    }

    public async Task<ReportScheduleDto> CreateAsync(CreateScheduleRequest request, CancellationToken ct = default)
    {
        _logger.Info("Creating schedule for project {ProjectId}: {ReportType} {Frequency}", request.ProjectId, request.ReportType, request.Frequency);

        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ReportType = Enum.Parse<ReportType>(request.ReportType),
            Frequency = Enum.Parse<ScheduleFrequency>(request.Frequency),
            DayOfWeek = request.DayOfWeek,
            DayOfMonth = request.DayOfMonth,
            TimeOfDay = TimeSpan.Parse(request.TimeOfDay),
            TimeZoneId = request.TimeZoneId,
            ExportFormats = request.ExportFormats,
            MaxRetries = request.MaxRetries,
            IsActive = true,
            NextRunAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        schedule.NextRunAt = await ComputeNextRunAsync(schedule, ct);
        await _repository.AddAsync(schedule, ct);

        _logger.Info("Schedule {ScheduleId} created, next run at {NextRunAt}", schedule.Id, schedule.NextRunAt);
        return schedule.ToDto();
    }

    public async Task<ReportScheduleDto> UpdateAsync(Guid id, UpdateScheduleRequest request, CancellationToken ct = default)
    {
        _logger.Info("Updating schedule {ScheduleId}", id);

        var schedule = await _repository.GetByIdAsync(id, ct);
        if (schedule == null)
        {
            _logger.Error("Schedule {ScheduleId} not found for update", new InvalidOperationException($"Schedule {id} not found."), id);
            throw new InvalidOperationException($"Schedule {id} not found.");
        }

        if (request.Frequency != null) schedule.Frequency = Enum.Parse<ScheduleFrequency>(request.Frequency);
        if (request.DayOfWeek.HasValue) schedule.DayOfWeek = request.DayOfWeek;
        if (request.DayOfMonth.HasValue) schedule.DayOfMonth = request.DayOfMonth;
        if (request.TimeOfDay != null) schedule.TimeOfDay = TimeSpan.Parse(request.TimeOfDay);
        if (request.TimeZoneId != null) schedule.TimeZoneId = request.TimeZoneId;
        if (request.ExportFormats != null) schedule.ExportFormats = request.ExportFormats;
        if (request.MaxRetries.HasValue) schedule.MaxRetries = request.MaxRetries.Value;

        schedule.UpdatedAt = DateTime.UtcNow;
        schedule.NextRunAt = await ComputeNextRunAsync(schedule, ct);
        await _repository.UpdateAsync(schedule, ct);

        _logger.Info("Schedule {ScheduleId} updated, next run at {NextRunAt}", id, schedule.NextRunAt);
        return schedule.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _logger.Info("Deleting schedule {ScheduleId}", id);

        var schedule = await _repository.GetByIdAsync(id, ct);
        if (schedule == null)
        {
            _logger.Warning("Schedule {ScheduleId} not found for deletion", id);
            return;
        }

        await _repository.DeleteAsync(schedule, ct);
        _logger.Info("Schedule {ScheduleId} deleted", id);
    }

    public async Task ToggleActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        _logger.Info("Toggling schedule {ScheduleId} active={IsActive}", id, isActive);

        var schedule = await _repository.GetByIdAsync(id, ct);
        if (schedule == null)
        {
            _logger.Warning("Schedule {ScheduleId} not found for toggle", id);
            return;
        }

        schedule.IsActive = isActive;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(schedule, ct);

        _logger.Info("Schedule {ScheduleId} toggled to active={IsActive}", id, isActive);
    }

    public async Task<DateTime> ComputeNextRunAsync(ReportSchedule schedule, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var todayUtc = now.Date;

        var timeZone = GetTimeZoneInfo(schedule.TimeZoneId);
        var nowInZone = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone);
        var todayInZone = nowInZone.Date;

        var candidateDate = schedule.Frequency switch
        {
            ScheduleFrequency.Daily => todayInZone,
            ScheduleFrequency.Weekly when schedule.DayOfWeek.HasValue => GetNextWeekday(todayInZone, (DayOfWeek)schedule.DayOfWeek.Value),
            ScheduleFrequency.Monthly when schedule.DayOfMonth.HasValue => GetNextDayOfMonth(todayInZone, schedule.DayOfMonth.Value),
            _ => todayInZone
        };

        var candidateTime = candidateDate + schedule.TimeOfDay;
        if (candidateTime <= nowInZone)
        {
            candidateDate = schedule.Frequency switch
            {
                ScheduleFrequency.Daily => candidateDate.AddDays(1),
                ScheduleFrequency.Weekly => candidateDate.AddDays(7),
                ScheduleFrequency.Monthly => candidateDate.AddMonths(1),
                _ => candidateDate.AddDays(1)
            };
            candidateTime = candidateDate + schedule.TimeOfDay;
        }

        return TimeZoneInfo.ConvertTimeToUtc(candidateTime, timeZone);
    }

    private static TimeZoneInfo GetTimeZoneInfo(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static DateTime GetNextWeekday(DateTime from, DayOfWeek targetDay)
    {
        var daysToAdd = ((int)targetDay - (int)from.DayOfWeek + 7) % 7;
        return daysToAdd == 0 ? from : from.AddDays(daysToAdd);
    }

    private static DateTime GetNextDayOfMonth(DateTime from, int day)
    {
        var maxDay = DateTime.DaysInMonth(from.Year, from.Month);
        var targetDay = Math.Min(day, maxDay);
        var candidate = new DateTime(from.Year, from.Month, targetDay);
        return candidate < from ? candidate.AddMonths(1) : candidate;
    }
}
