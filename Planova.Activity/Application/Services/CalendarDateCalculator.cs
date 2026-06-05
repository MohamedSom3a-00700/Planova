using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Application.Services;

public class CalendarDateCalculator
{
    private readonly ICalendarDayRepository _calendarDayRepository;

    public CalendarDateCalculator(ICalendarDayRepository calendarDayRepository)
    {
        _calendarDayRepository = calendarDayRepository;
    }

    public async Task<DateTime> AddWorkingDaysAsync(DateTime startDate, int days, Calendar calendar, CancellationToken ct = default)
    {
        if (days <= 0) return startDate;
        if (!HasAnyWorkingDay(calendar)) return startDate.AddDays(days);

        var current = startDate;
        var workingDaysAdded = 0;

        var calendarDays = await GetCalendarDayExceptions(calendar.Id, startDate, startDate.AddDays(days * 2), ct);

        while (workingDaysAdded < days)
        {
            current = current.AddDays(1);

            if (IsWorkingDay(calendar, current, calendarDays))
            {
                workingDaysAdded++;
            }
        }

        return current;
    }

    public async Task<int> CountWorkingDaysAsync(DateTime from, DateTime to, Calendar calendar, CancellationToken ct = default)
    {
        if (!HasAnyWorkingDay(calendar)) return (to - from).Days;

        var calendarDays = await GetCalendarDayExceptions(calendar.Id, from, to, ct);
        var count = 0;
        var current = from;

        while (current <= to)
        {
            if (IsWorkingDay(calendar, current, calendarDays))
                count++;
            current = current.AddDays(1);
        }

        return count;
    }

    private async Task<List<CalendarDay>> GetCalendarDayExceptions(Guid calendarId, DateTime from, DateTime to, CancellationToken ct)
    {
        return await _calendarDayRepository.GetRangeAsync(calendarId, from, to, ct);
    }

    private static bool IsWorkingDay(Calendar calendar, DateTime date, List<CalendarDay> exceptions)
    {
        var exception = exceptions.FirstOrDefault(e => e.Date == date.Date);
        if (exception is not null)
            return exception.Status == CalendarDayStatus.Working;

        return date.DayOfWeek switch
        {
            DayOfWeek.Monday => calendar.Monday,
            DayOfWeek.Tuesday => calendar.Tuesday,
            DayOfWeek.Wednesday => calendar.Wednesday,
            DayOfWeek.Thursday => calendar.Thursday,
            DayOfWeek.Friday => calendar.Friday,
            DayOfWeek.Saturday => calendar.Saturday,
            DayOfWeek.Sunday => calendar.Sunday,
            _ => false
        };
    }

    private static bool HasAnyWorkingDay(Calendar calendar)
    {
        return calendar.Monday || calendar.Tuesday || calendar.Wednesday
            || calendar.Thursday || calendar.Friday || calendar.Saturday || calendar.Sunday;
    }
}
