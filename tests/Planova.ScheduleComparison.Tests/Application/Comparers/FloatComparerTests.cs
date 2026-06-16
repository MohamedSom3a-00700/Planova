using FluentAssertions;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Models;

namespace Planova.ScheduleComparison.Tests.Application.Comparers;

public class FloatComparerTests
{
    private static ScheduleActivity Activity(string provenanceId, double? totalFloat, double? freeFloat)
    {
        return new ScheduleActivity
        {
            ProvenanceId = provenanceId,
            ActivityId = $"A{provenanceId}",
            TotalFloat = totalFloat,
            FreeFloat = freeFloat
        };
    }

    private static ScheduleData Data(params ScheduleActivity[] activities)
    {
        var data = new ScheduleData();
        foreach (var a in activities)
            data.Activities.Add(a);
        return data;
    }

    [Fact]
    public void Compare_IdenticalActivities_ReturnsEmptyReport()
    {
        var source = Data(Activity("1", 10, 5));
        var target = Data(Activity("1", 10, 5));

        var result = new FloatComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.ActivityFloatDeltas.Should().BeEmpty();
        result.ActivitiesWithImprovedFloat.Should().BeEmpty();
        result.ActivitiesWithWorsenedFloat.Should().BeEmpty();
    }

    [Fact]
    public void Compare_TotalFloatImproved_ReportsDelta()
    {
        var source = Data(Activity("1", 5, 3));
        var target = Data(Activity("1", 15, 3));

        var result = new FloatComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.ActivityFloatDeltas.Should().ContainSingle();
        var delta = result.ActivityFloatDeltas[0];
        delta.MatchKey.Should().Be("provenance:1");
        delta.OldTotalFloat.Should().Be(5);
        delta.NewTotalFloat.Should().Be(15);
        delta.FloatDelta.Should().Be(10);
        delta.FreeFloatDelta.Should().Be(0);
        result.ActivitiesWithImprovedFloat.Should().Contain("provenance:1");
        result.ActivitiesWithWorsenedFloat.Should().BeEmpty();
    }

    [Fact]
    public void Compare_TotalFloatWorsened_ReportsDelta()
    {
        var source = Data(Activity("1", 20, 5));
        var target = Data(Activity("1", 8, 5));

        var result = new FloatComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.ActivityFloatDeltas.Should().ContainSingle();
        var delta = result.ActivityFloatDeltas[0];
        delta.FloatDelta.Should().Be(-12);
        result.ActivitiesWithWorsenedFloat.Should().Contain("provenance:1");
        result.ActivitiesWithImprovedFloat.Should().BeEmpty();
    }

    [Fact]
    public void Compare_FreeFloatChanged_ReportsFreeFloatDelta()
    {
        var source = Data(Activity("1", 10, 5));
        var target = Data(Activity("1", 10, 12));

        var result = new FloatComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.ActivityFloatDeltas.Should().ContainSingle();
        var delta = result.ActivityFloatDeltas[0];
        delta.FloatDelta.Should().Be(0);
        delta.OldFreeFloat.Should().Be(5);
        delta.NewFreeFloat.Should().Be(12);
        delta.FreeFloatDelta.Should().Be(7);
        result.ActivitiesWithImprovedFloat.Should().BeEmpty();
        result.ActivitiesWithWorsenedFloat.Should().BeEmpty();
    }

    [Fact]
    public void Compare_BothTotalAndFreeFloatChanged_ReportsBoth()
    {
        var source = Data(Activity("1", 10, 5));
        var target = Data(Activity("1", 20, 8));

        var result = new FloatComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.ActivityFloatDeltas.Should().ContainSingle();
        var delta = result.ActivityFloatDeltas[0];
        delta.FloatDelta.Should().Be(10);
        delta.FreeFloatDelta.Should().Be(3);
    }

    [Fact]
    public void Compare_MultipleActivitiesWithMixedFloatChanges_ReportsAll()
    {
        var source = Data(
            Activity("1", 10, 5),
            Activity("2", 15, 10),
            Activity("3", 20, 15));
        var target = Data(
            Activity("1", 5, 5),
            Activity("2", 30, 10),
            Activity("3", 20, 15));

        var result = new FloatComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.ActivityFloatDeltas.Should().HaveCount(2);
        result.ActivitiesWithImprovedFloat.Should().Contain("provenance:2");
        result.ActivitiesWithWorsenedFloat.Should().Contain("provenance:1");
    }

    [Fact]
    public void Compare_ActivityInSourceOnly_IsIgnored()
    {
        var source = Data(Activity("1", 10, 5));
        var target = Data(Activity("2", 15, 8));

        var result = new FloatComparer().Compare(source, target);

        result.Should().NotBeNull();
        result!.ActivityFloatDeltas.Should().BeEmpty();
    }
}
