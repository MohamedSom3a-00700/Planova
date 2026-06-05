using FluentAssertions;
using Moq;
using Planova.Activity.Application.Dto;
using Planova.Activity.Application.Services;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Tests.Application;

public class CalendarServiceTests
{
    private readonly Mock<ICalendarRepository> _calendarRepo;
    private readonly Mock<ICalendarDayRepository> _dayRepo;
    private readonly CalendarService _service;

    public CalendarServiceTests()
    {
        _calendarRepo = new Mock<ICalendarRepository>();
        _dayRepo = new Mock<ICalendarDayRepository>();
        _service = new CalendarService(_calendarRepo.Object, _dayRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_GlobalCalendar_ShouldSucceed()
    {
        var request = new CreateCalendarRequest
        {
            Name = "Test Calendar",
            ProjectId = null
        };

        _calendarRepo.Setup(r => r.AddAsync(It.IsAny<Calendar>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Calendar");
    }

    [Fact]
    public async Task GetCalendarsAsync_ShouldReturnList()
    {
        _calendarRepo.Setup(r => r.GetGlobalCalendarsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GetCalendarsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
