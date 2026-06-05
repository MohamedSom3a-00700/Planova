using FluentAssertions;
using Moq;
using Planova.Activity.Application.Dto;
using Planova.Activity.Application.Services;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Tests.Application;

public class ActivityReportServiceTests
{
    private readonly Mock<IActivityService> _activityService;
    private readonly Mock<IActivityRelationshipService> _relService;
    private readonly ActivityReportService _service;

    public ActivityReportServiceTests()
    {
        _activityService = new Mock<IActivityService>();
        _relService = new Mock<IActivityRelationshipService>();
        _service = new ActivityReportService(_activityService.Object, _relService.Object);
    }

    [Fact]
    public async Task GenerateScheduleReportAsync_NoActivities_ShouldReturnEmpty()
    {
        _activityService.Setup(s => s.GetByProjectAsync(It.IsAny<int>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _relService.Setup(s => s.GetByProjectAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GenerateScheduleReportAsync(1);

        result.Should().NotBeNull();
        result.Rows.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateScheduleReportAsync_WithActivities_ShouldReturnRows()
    {
        _activityService.Setup(s => s.GetByProjectAsync(It.IsAny<int>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ActivityDto { Id = Guid.NewGuid(), Code = "A-001", Name = "Test", ActivityType = "Task", Status = "NotStarted", Duration = 5 }
            ]);
        _relService.Setup(s => s.GetByProjectAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GenerateScheduleReportAsync(1);

        result.Rows.Should().HaveCount(1);
        result.Rows[0].Code.Should().Be("A-001");
    }
}
