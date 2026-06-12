using FluentAssertions;
using NSubstitute;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;
using Planova.Reporting.Application.DataProviders;
using Planova.Reporting.Application.Dto;
using ReportingReportType = Planova.Reporting.Domain.Enums.ReportType;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Reporting.Tests.Application.DataProviders;

public class DailyReportDataProviderTests
{
    private readonly IActivityService _activityService = Substitute.For<IActivityService>();
    private readonly IResourceAssignmentService _assignmentService = Substitute.For<IResourceAssignmentService>();
    private readonly ILoggingService _logger = Substitute.For<ILoggingService>();
    private readonly DailyReportDataProvider _sut;

    public DailyReportDataProviderTests()
    {
        _sut = new DailyReportDataProvider(_activityService, _assignmentService, _logger);
    }

    [Fact]
    public async Task HandledType_ShouldBeDaily()
    {
        _sut.HandledType.Should().Be(ReportingReportType.Daily);
    }

    [Fact]
    public async Task CollectDataAsync_ShouldReturnDtoWithCorrectDate()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityDto>());
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<ResourceAssignmentDto>());

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.ReportDate.Should().Be(date);
    }

    [Fact]
    public async Task CollectDataAsync_WhenNoActivities_ShouldReturnEmptyCounts()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityDto>());
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<ResourceAssignmentDto>());

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.TotalActivities.Should().Be(0);
        result.CompletedActivities.Should().Be(0);
        result.InProgressActivities.Should().Be(0);
        result.NotStartedActivities.Should().Be(0);
        result.OverallPercentComplete.Should().Be(0);
    }

    [Fact]
    public async Task CollectDataAsync_ShouldExcludeWbsSummaryActivities()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);
        var activities = new List<ActivityDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Task 1", Status = "Completed", PercentComplete = 100, ActivityType = "Task", Code = "A1" },
            new() { Id = Guid.NewGuid(), Name = "Summary 1", Status = "InProgress", PercentComplete = 50, ActivityType = "WbsSummary", Code = "S1" }
        };

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(activities);
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<ResourceAssignmentDto>());

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.TotalActivities.Should().Be(1);
        result.CompletedActivities.Should().Be(1);
    }

    [Fact]
    public async Task CollectDataAsync_ShouldCountStatusesCorrectly()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);
        var activities = new List<ActivityDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Task 1", Status = "Completed", PercentComplete = 100, ActivityType = "Task", Code = "A1" },
            new() { Id = Guid.NewGuid(), Name = "Task 2", Status = "InProgress", PercentComplete = 50, ActivityType = "Task", Code = "A2" },
            new() { Id = Guid.NewGuid(), Name = "Task 3", Status = "NotStarted", PercentComplete = 0, ActivityType = "Task", Code = "A3" },
            new() { Id = Guid.NewGuid(), Name = "Task 4", Status = "NotStarted", PercentComplete = 0, ActivityType = "Task", Code = "A4" }
        };

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(activities);
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<ResourceAssignmentDto>());

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.TotalActivities.Should().Be(4);
        result.CompletedActivities.Should().Be(1);
        result.InProgressActivities.Should().Be(1);
        result.NotStartedActivities.Should().Be(2);
    }

    [Fact]
    public async Task CollectDataAsync_ShouldCalculateOverallPercentCorrectly()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);
        var activities = new List<ActivityDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Task 1", Status = "Completed", PercentComplete = 100, ActivityType = "Task", Code = "A1" },
            new() { Id = Guid.NewGuid(), Name = "Task 2", Status = "InProgress", PercentComplete = 50, ActivityType = "Task", Code = "A2" },
            new() { Id = Guid.NewGuid(), Name = "Task 3", Status = "NotStarted", PercentComplete = 0, ActivityType = "Task", Code = "A3" }
        };

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(activities);
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<ResourceAssignmentDto>());

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.OverallPercentComplete.Should().Be(50m);
    }

    [Fact]
    public async Task CollectDataAsync_ShouldIncludeProgressItems()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);
        var activities = new List<ActivityDto>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Task 1", Code = "A1", Status = "Completed",
                PercentComplete = 100, ActivityType = "Task",
                PlannedStart = date, PlannedFinish = date.AddDays(5),
                ActualStart = date, ActualFinish = date.AddDays(4)
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Task 2", Code = "A2", Status = "InProgress",
                PercentComplete = 60, ActivityType = "Task",
                PlannedStart = date, PlannedFinish = date.AddDays(10),
                ActualStart = date
            }
        };

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(activities);
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<ResourceAssignmentDto>());

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.ProgressToday.Should().HaveCount(2);
        result.ProgressToday[0].ActivityName.Should().Be("Task 1");
        result.ProgressToday[0].PercentComplete.Should().Be(100);
        result.ProgressToday[1].ActivityName.Should().Be("Task 2");
        result.ProgressToday[1].PercentComplete.Should().Be(60);
    }

    [Fact]
    public async Task CollectDataAsync_ShouldIncludeWorkforceItems()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);
        var assignments = new List<ResourceAssignmentDto>
        {
            new()
            {
                Id = Guid.NewGuid(), ProjectId = projectId, ResourceName = "John", ResourceCode = "R1",
                ActivityName = "Task 1", ResourceType = ResourceType.Labour,
                Quantity = 8, UnitOfMeasure = "hr"
            },
            new()
            {
                Id = Guid.NewGuid(), ProjectId = projectId, ResourceName = "Jane", ResourceCode = "R2",
                ActivityName = "Task 2", ResourceType = ResourceType.Labour,
                Quantity = 6, UnitOfMeasure = "hr"
            }
        };

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityDto>());
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(assignments);

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.Workforce.Should().HaveCount(2);
        result.Workforce[0].ResourceName.Should().Be("John");
        result.Workforce[1].ResourceName.Should().Be("Jane");
    }

    [Fact]
    public async Task CollectDataAsync_ShouldIncludeEquipmentItems()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);
        var assignments = new List<ResourceAssignmentDto>
        {
            new()
            {
                Id = Guid.NewGuid(), ProjectId = projectId, ResourceName = "Excavator", ResourceCode = "E1",
                ActivityName = "Excavation", ResourceType = ResourceType.Equipment,
                Quantity = 1, UnitOfMeasure = "day"
            }
        };

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityDto>());
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(assignments);

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.Equipment.Should().HaveCount(1);
        result.Equipment[0].EquipmentName.Should().Be("Excavator");
    }

    [Fact]
    public async Task CollectDataAsync_ShouldReturnEmptyIssuesAndPhotos()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityDto>());
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<ResourceAssignmentDto>());

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.Issues.Should().BeEmpty();
        result.Photos.Should().BeEmpty();
    }

    [Fact]
    public async Task CollectDataAsync_ShouldIgnoreMaterialAndSubcontractorAssignments()
    {
        var projectId = 1;
        var date = new DateTime(2026, 6, 1);
        var assignments = new List<ResourceAssignmentDto>
        {
            new()
            {
                Id = Guid.NewGuid(), ResourceName = "Labourer", ResourceCode = "L1",
                ActivityName = "Task 1", ResourceType = ResourceType.Labour,
                Quantity = 8, UnitOfMeasure = "hr"
            },
            new()
            {
                Id = Guid.NewGuid(), ResourceName = "Cement", ResourceCode = "M1",
                ActivityName = "Task 2", ResourceType = ResourceType.Material,
                Quantity = 50, UnitOfMeasure = "bag"
            },
            new()
            {
                Id = Guid.NewGuid(), ResourceName = "Sub Contractor", ResourceCode = "S1",
                ActivityName = "Task 3", ResourceType = ResourceType.Subcontractor,
                Quantity = 1, UnitOfMeasure = "lump"
            }
        };

        _activityService.GetByProjectAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityDto>());
        _assignmentService.GetByProjectAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(assignments);

        var result = await _sut.CollectDataAsync(projectId, date, date.AddDays(1));

        result.Workforce.Should().HaveCount(1);
        result.Equipment.Should().BeEmpty();
    }
}
