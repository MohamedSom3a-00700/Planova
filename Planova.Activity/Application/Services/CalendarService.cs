using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Application.Services;

public class CalendarService : ICalendarService
{
    private readonly ICalendarRepository _calendarRepository;
    private readonly ICalendarDayRepository _calendarDayRepository;
    private readonly CalendarDateCalculator _dateCalculator;

    public CalendarService(ICalendarRepository calendarRepository, ICalendarDayRepository calendarDayRepository)
    {
        _calendarRepository = calendarRepository;
        _calendarDayRepository = calendarDayRepository;
        _dateCalculator = new CalendarDateCalculator(calendarDayRepository);
    }

    public async Task<CalendarDto> CreateAsync(CreateCalendarRequest request, CancellationToken ct = default)
    {
        var calendar = new Calendar
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Name = request.Name,
            Type = request.ProjectId is null ? CalendarType.Global : CalendarType.Project,
            HoursPerDay = request.HoursPerDay,
            DaysPerWeek = request.DaysPerWeek,
            Monday = request.Monday,
            Tuesday = request.Tuesday,
            Wednesday = request.Wednesday,
            Thursday = request.Thursday,
            Friday = request.Friday,
            Saturday = request.Saturday,
            Sunday = request.Sunday,
            IsDefault = request.IsDefault,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _calendarRepository.AddAsync(calendar, ct);
        return MapToDto(calendar);
    }

    public async Task<CalendarDto> UpdateAsync(UpdateCalendarRequest request, CancellationToken ct = default)
    {
        var calendar = await _calendarRepository.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Calendar {request.Id} not found");

        if (request.Name is not null) calendar.Name = request.Name;
        if (request.HoursPerDay.HasValue) calendar.HoursPerDay = request.HoursPerDay.Value;
        if (request.DaysPerWeek.HasValue) calendar.DaysPerWeek = request.DaysPerWeek.Value;
        if (request.Monday.HasValue) calendar.Monday = request.Monday.Value;
        if (request.Tuesday.HasValue) calendar.Tuesday = request.Tuesday.Value;
        if (request.Wednesday.HasValue) calendar.Wednesday = request.Wednesday.Value;
        if (request.Thursday.HasValue) calendar.Thursday = request.Thursday.Value;
        if (request.Friday.HasValue) calendar.Friday = request.Friday.Value;
        if (request.Saturday.HasValue) calendar.Saturday = request.Saturday.Value;
        if (request.Sunday.HasValue) calendar.Sunday = request.Sunday.Value;
        if (request.IsDefault.HasValue) calendar.IsDefault = request.IsDefault.Value;
        if (request.Description is not null) calendar.Description = request.Description;

        calendar.UpdatedAt = DateTime.UtcNow;
        await _calendarRepository.UpdateAsync(calendar, ct);
        return MapToDto(calendar);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _calendarRepository.DeleteAsync(id, ct);
    }

    public async Task<List<CalendarDto>> GetCalendarsAsync(int? projectId = null, CancellationToken ct = default)
    {
        List<Calendar> calendars;
        if (projectId.HasValue)
            calendars = await _calendarRepository.GetByProjectIdAsync(projectId.Value, ct);
        else
            calendars = await _calendarRepository.GetGlobalCalendarsAsync(ct);

        return calendars.Select(MapToDto).ToList();
    }

    public async Task<CalendarDto?> GetDefaultAsync(int projectId, CancellationToken ct = default)
    {
        var calendar = await _calendarRepository.GetDefaultForProjectAsync(projectId, ct);
        return calendar is not null ? MapToDto(calendar) : null;
    }

    public async Task<CalendarDayDto> SetDayStatusAsync(Guid calendarId, DateTime date, CalendarDayStatus status, string? label = null, CancellationToken ct = default)
    {
        await _calendarDayRepository.SetWorkingStatusAsync(calendarId, date, status, label, ct);
        var day = await _calendarDayRepository.GetAsync(calendarId, date, ct);

        return new CalendarDayDto
        {
            Id = day?.Id ?? Guid.Empty,
            CalendarId = calendarId,
            Date = date,
            Status = status.ToString(),
            Label = label
        };
    }

    public async Task BulkSetDaysAsync(Guid calendarId, DateTime from, DateTime to, CalendarDayStatus status, string? label = null, CancellationToken ct = default)
    {
        await _calendarDayRepository.BulkSetRangeAsync(calendarId, from, to, status, label, ct);
    }

    public async Task<List<CalendarDayDto>> GetDayRangeAsync(Guid calendarId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var days = await _calendarDayRepository.GetRangeAsync(calendarId, from, to, ct);
        return days.Select(d => new CalendarDayDto
        {
            Id = d.Id,
            CalendarId = d.CalendarId,
            Date = d.Date,
            Status = d.Status.ToString(),
            Label = d.Label
        }).ToList();
    }

    public async Task<DateTime> AddWorkingDaysAsync(DateTime startDate, int days, Guid? calendarId = null, int? projectId = null, CancellationToken ct = default)
    {
        Calendar? calendar = null;

        if (calendarId.HasValue)
            calendar = await _calendarRepository.GetByIdAsync(calendarId.Value, ct);

        calendar ??= Calendar.GetDefault();

        return await _dateCalculator.AddWorkingDaysAsync(startDate, days, calendar, ct);
    }

    public async Task<int> CountWorkingDaysAsync(DateTime from, DateTime to, Guid? calendarId = null, int? projectId = null, CancellationToken ct = default)
    {
        Calendar? calendar = null;

        if (calendarId.HasValue)
            calendar = await _calendarRepository.GetByIdAsync(calendarId.Value, ct);

        calendar ??= Calendar.GetDefault();
        return await _dateCalculator.CountWorkingDaysAsync(from, to, calendar, ct);
    }

    private static CalendarDto MapToDto(Calendar c) => new()
    {
        Id = c.Id,
        ProjectId = c.ProjectId,
        Name = c.Name,
        Type = c.Type.ToString(),
        HoursPerDay = c.HoursPerDay,
        DaysPerWeek = c.DaysPerWeek,
        Monday = c.Monday,
        Tuesday = c.Tuesday,
        Wednesday = c.Wednesday,
        Thursday = c.Thursday,
        Friday = c.Friday,
        Saturday = c.Saturday,
        Sunday = c.Sunday,
        IsDefault = c.IsDefault,
        Description = c.Description
    };
}
