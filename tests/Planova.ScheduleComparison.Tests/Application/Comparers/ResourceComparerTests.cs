using FluentAssertions;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Models;

namespace Planova.ScheduleComparison.Tests.Application.Comparers;

public class ResourceComparerTests
{
    private readonly ResourceComparer _sut = new();

    [Fact]
    public void Compare_IdenticalAssignments_ReturnsNoDiffs()
    {
        var source = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m },
                new ScheduleResourceAssignment { ActivityMatchKey = "A2", ResourceId = "R2", Units = 2.5, Cost = 250m }
            ]
        };
        var target = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m },
                new ScheduleResourceAssignment { ActivityMatchKey = "A2", ResourceId = "R2", Units = 2.5, Cost = 250m }
            ]
        };

        var result = _sut.Compare(source, target);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Compare_AddedAssignmentInTarget_ReturnsAddedDiff()
    {
        var source = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m }
            ]
        };
        var target = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m },
                new ScheduleResourceAssignment { ActivityMatchKey = "A2", ResourceId = "R2", Units = 2.0, Cost = 200m }
            ]
        };

        var result = _sut.Compare(source, target);

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(new ResourceDiff
        {
            ActivityMatchKey = "A2",
            ResourceId = "R2",
            ChangeType = "Added",
            NewUnits = 2.0,
            NewCost = 200m
        });
    }

    [Fact]
    public void Compare_RemovedAssignmentFromTarget_ReturnsRemovedDiff()
    {
        var source = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m },
                new ScheduleResourceAssignment { ActivityMatchKey = "A2", ResourceId = "R2", Units = 2.0, Cost = 200m }
            ]
        };
        var target = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m }
            ]
        };

        var result = _sut.Compare(source, target);

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(new ResourceDiff
        {
            ActivityMatchKey = "A2",
            ResourceId = "R2",
            ChangeType = "Removed",
            OldUnits = 2.0,
            OldCost = 200m
        });
    }

    [Fact]
    public void Compare_UnitsChanged_ReturnsModifiedDiff()
    {
        var source = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m }
            ]
        };
        var target = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 2.0, Cost = 100m }
            ]
        };

        var result = _sut.Compare(source, target);

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(new ResourceDiff
        {
            ActivityMatchKey = "A1",
            ResourceId = "R1",
            ChangeType = "Modified",
            OldUnits = 1.0,
            NewUnits = 2.0,
            OldCost = 100m,
            NewCost = 100m
        });
    }

    [Fact]
    public void Compare_CostChanged_ReturnsModifiedDiff()
    {
        var source = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m }
            ]
        };
        var target = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 200m }
            ]
        };

        var result = _sut.Compare(source, target);

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(new ResourceDiff
        {
            ActivityMatchKey = "A1",
            ResourceId = "R1",
            ChangeType = "Modified",
            OldUnits = 1.0,
            NewUnits = 1.0,
            OldCost = 100m,
            NewCost = 200m
        });
    }

    [Fact]
    public void Compare_BothUnitsAndCostChanged_ReturnsModifiedDiff()
    {
        var source = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m }
            ]
        };
        var target = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 2.5, Cost = 250m }
            ]
        };

        var result = _sut.Compare(source, target);

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(new ResourceDiff
        {
            ActivityMatchKey = "A1",
            ResourceId = "R1",
            ChangeType = "Modified",
            OldUnits = 1.0,
            NewUnits = 2.5,
            OldCost = 100m,
            NewCost = 250m
        });
    }

    [Fact]
    public void Compare_ActivityMatchKeyChanged_ReturnsRemovedAndAddedDiffs()
    {
        var source = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m }
            ]
        };
        var target = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A2", ResourceId = "R1", Units = 1.0, Cost = 100m }
            ]
        };

        var result = _sut.Compare(source, target);

        result.Should().HaveCount(2);
        result.Should().Contain(d => d.ChangeType == "Removed").Which.Should().BeEquivalentTo(new ResourceDiff
        {
            ActivityMatchKey = "A1",
            ResourceId = "R1",
            ChangeType = "Removed",
            OldUnits = 1.0,
            OldCost = 100m
        });
        result.Should().Contain(d => d.ChangeType == "Added").Which.Should().BeEquivalentTo(new ResourceDiff
        {
            ActivityMatchKey = "A2",
            ResourceId = "R1",
            ChangeType = "Added",
            NewUnits = 1.0,
            NewCost = 100m
        });
    }

    [Fact]
    public void Compare_SameKeyMultipleAssignments_OnlyFirstIsMatched()
    {
        var source = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 1.0, Cost = 100m }
            ]
        };
        var target = new ScheduleData
        {
            ResourceAssignments =
            [
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 2.0, Cost = 200m },
                new ScheduleResourceAssignment { ActivityMatchKey = "A1", ResourceId = "R1", Units = 3.0, Cost = 300m }
            ]
        };

        var result = _sut.Compare(source, target);

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(new ResourceDiff
        {
            ActivityMatchKey = "A1",
            ResourceId = "R1",
            ChangeType = "Modified",
            OldUnits = 1.0,
            NewUnits = 2.0,
            OldCost = 100m,
            NewCost = 200m
        });
    }
}
