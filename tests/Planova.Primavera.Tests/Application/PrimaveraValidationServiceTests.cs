using FluentAssertions;
using Moq;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Application.Services;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.Primavera.Tests.Application;

public class PrimaveraValidationServiceTests
{
    private readonly Mock<IPrimaveraWorkspaceService> _workspaceMock = new();
    private readonly Mock<IPrimaveraValidationRepository> _repoMock = new();
    private readonly PrimaveraValidationService _service;

    public PrimaveraValidationServiceTests()
    {
        _service = new PrimaveraValidationService(_workspaceMock.Object, _repoMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_WithNoIssues_ReturnsEmpty()
    {
        _workspaceMock.Setup(w => w.GetActivitiesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraActivityDto>());
        _workspaceMock.Setup(w => w.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraRelationshipDto>());
        _workspaceMock.Setup(w => w.GetCalendarsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraCalendarDto>());

        var result = await _service.ValidateAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithMissingCalendar_ReportsIssue()
    {
        var activities = new List<PrimaveraActivityDto>
        {
            new() { TaskId = "T1", Name = "Test", CalendarId = "CAL-MISSING", Duration = 5 }
        };

        _workspaceMock.Setup(w => w.GetActivitiesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);
        _workspaceMock.Setup(w => w.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraRelationshipDto>());
        _workspaceMock.Setup(w => w.GetCalendarsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraCalendarDto>());

        var result = await _service.ValidateAsync(1);

        result.Should().Contain(i => i.Severity == "Error" && i.Description.Contains("missing calendar"));
    }

    [Fact]
    public async Task ValidateAsync_WithZeroDurationActivity_ReportsWarning()
    {
        var activities = new List<PrimaveraActivityDto>
        {
            new() { TaskId = "T1", Name = "Zero Dur", Duration = 0, Status = "Status_NotStart" }
        };

        _workspaceMock.Setup(w => w.GetActivitiesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);
        _workspaceMock.Setup(w => w.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraRelationshipDto>());
        _workspaceMock.Setup(w => w.GetCalendarsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraCalendarDto>());

        var result = await _service.ValidateAsync(1);

        result.Should().Contain(i => i.Severity == "Warning" && i.Description.Contains("zero duration"));
    }
}
