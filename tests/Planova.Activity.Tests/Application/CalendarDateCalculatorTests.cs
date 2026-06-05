using FluentAssertions;
using Moq;
using Planova.Activity.Application.Services;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Tests.Application;

public class CalendarDateCalculatorTests
{
    private readonly Mock<ICalendarDayRepository> _dayRepo;
    private readonly CalendarDateCalculator _calculator;

    public CalendarDateCalculatorTests()
    {
        _dayRepo = new Mock<ICalendarDayRepository>();
        _calculator = new CalendarDateCalculator(_dayRepo.Object);
    }

    [Fact]
    public async Task AddWorkingDays_Standard5Day_ShouldSkipWeekends()
    {
        _dayRepo.Setup(r => r.GetRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var calendar = Calendar.GetDefault();
        var start = new DateTime(2026, 6, 8); // Monday

        var result = await _calculator.AddWorkingDaysAsync(start, 5, calendar);

        result.Should().BeAfter(start);
    }

    [Fact]
    public async Task CountWorkingDays_SameDay_ShouldReturnOne()
    {
        _dayRepo.Setup(r => r.GetRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var calendar = Calendar.GetDefault();
        var start = new DateTime(2026, 6, 8); // Monday

        var result = await _calculator.CountWorkingDaysAsync(start, start, calendar);

        result.Should().Be(1);
    }
}
