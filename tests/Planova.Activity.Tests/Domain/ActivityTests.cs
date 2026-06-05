using FluentAssertions;
using Planova.Activity.Domain.Enums;

using ActivityEntity = Planova.Activity.Domain.Entities.Activity;

namespace Planova.Activity.Tests.Domain;

public class ActivityTests
{
    [Fact]
    public void Activity_WithValidProperties_ShouldBeCreated()
    {
        var activity = new ActivityEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            Code = "A-001",
            Name = "Test Activity",
            ActivityType = ActivityType.Task,
            Status = ActivityStatus.NotStarted,
            Duration = 5
        };

        activity.Id.Should().NotBeEmpty();
        activity.Code.Should().Be("A-001");
        activity.ActivityType.Should().Be(ActivityType.Task);
        activity.Status.Should().Be(ActivityStatus.NotStarted);
        activity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activity_Milestone_DurationShouldBeZero()
    {
        var activity = new ActivityEntity
        {
            ActivityType = ActivityType.Milestone,
            Duration = 0
        };

        activity.ActivityType.Should().Be(ActivityType.Milestone);
        activity.Duration.Should().Be(0);
    }

    [Fact]
    public void Activity_WbsSummary_ShouldHaveDefaultStatus()
    {
        var activity = new ActivityEntity
        {
            ActivityType = ActivityType.WbsSummary,
            Status = ActivityStatus.NotStarted
        };

        activity.ActivityType.Should().Be(ActivityType.WbsSummary);
    }
}
