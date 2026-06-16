using FluentAssertions;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Models;

namespace Planova.ScheduleComparison.Tests.Application.Comparers;

public class CriticalPathComparerTests
{
    private static ScheduleActivity Activity(string activityId, bool isCritical, int startDay, int finishDay)
    {
        return new ScheduleActivity
        {
            ActivityId = activityId,
            IsCritical = isCritical,
            Start = new DateTime(2024, 1, startDay),
            Finish = new DateTime(2024, 1, finishDay)
        };
    }

    private static ScheduleData ScheduleWith(params ScheduleActivity[] activities)
    {
        return new ScheduleData { Activities = activities.ToList() };
    }

    [Fact]
    public void Compare_NoCriticalPathChanges_ReturnsDiffWithNoChanges()
    {
        var source = ScheduleWith(
            Activity("A", true, 1, 10),
            Activity("B", true, 5, 15));
        var target = ScheduleWith(
            Activity("A", true, 1, 10),
            Activity("B", true, 5, 15));

        var result = new CriticalPathComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.EnteredCriticalPath.Should().BeEmpty();
        result.ExitedCriticalPath.Should().BeEmpty();
        result.RemainedOnCriticalPath.Should().BeEquivalentTo("activityId:A", "activityId:B");
        result.DurationChange.Should().Be(0);
        result.SourceDuration.Should().Be(14);
        result.TargetDuration.Should().Be(14);
    }

    [Fact]
    public void Compare_ActivityEntersCriticalPath_ReportsEntered()
    {
        var source = ScheduleWith(
            Activity("A", true, 1, 10));
        var target = ScheduleWith(
            Activity("A", true, 1, 10),
            Activity("B", true, 5, 15));

        var result = new CriticalPathComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.EnteredCriticalPath.Should().BeEquivalentTo("activityId:B");
        result.ExitedCriticalPath.Should().BeEmpty();
        result.RemainedOnCriticalPath.Should().BeEquivalentTo("activityId:A");
    }

    [Fact]
    public void Compare_ActivityExitsCriticalPath_ReportsExited()
    {
        var source = ScheduleWith(
            Activity("A", true, 1, 10),
            Activity("B", true, 5, 15));
        var target = ScheduleWith(
            Activity("A", true, 1, 10));

        var result = new CriticalPathComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.EnteredCriticalPath.Should().BeEmpty();
        result.ExitedCriticalPath.Should().BeEquivalentTo("activityId:B");
        result.RemainedOnCriticalPath.Should().BeEquivalentTo("activityId:A");
    }

    [Fact]
    public void Compare_CriticalPathDurationChanged_ReportsDurationChange()
    {
        var source = ScheduleWith(
            Activity("A", true, 1, 10));
        var target = ScheduleWith(
            Activity("A", true, 1, 15));

        var result = new CriticalPathComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.SourceDuration.Should().Be(9);
        result.TargetDuration.Should().Be(14);
        result.DurationChange.Should().Be(5);
    }

    [Fact]
    public void Compare_MultipleCriticalPathChanges_ReportsAllChanges()
    {
        var source = ScheduleWith(
            Activity("A", true, 1, 5),
            Activity("B", false, 3, 10),
            Activity("C", true, 6, 12));
        var target = ScheduleWith(
            Activity("A", true, 1, 5),
            Activity("B", false, 3, 10),
            Activity("D", true, 4, 8));

        var result = new CriticalPathComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.EnteredCriticalPath.Should().BeEquivalentTo("activityId:D");
        result.ExitedCriticalPath.Should().BeEquivalentTo("activityId:C");
        result.RemainedOnCriticalPath.Should().BeEquivalentTo("activityId:A");
    }

    [Fact]
    public void Compare_BothSourceAndTargetHaveNoCriticalActivities_ReturnsNull()
    {
        var source = ScheduleWith(
            Activity("A", false, 1, 5));
        var target = ScheduleWith(
            Activity("A", false, 1, 5));

        var result = new CriticalPathComparer().Compare(source, target);

        result.Should().BeNull();
    }
}
