using FluentAssertions;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Constants;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Tests.Application.Comparers;

public class ActivityComparerTests
{
    private readonly ActivityComparer _sut = new();

    [Fact]
    public void Compare_IdenticalSourceAndTarget_ReturnsSingleUnchangedDiff()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", Duration = 5 }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", Duration = 5 }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().HaveCount(1);
        result[0].MatchKey.Should().Be("activityId:A1");
        result[0].ChangeType.Should().Be(ChangeType.Unchanged.ToString());
    }

    [Fact]
    public void Compare_OneAddedActivityInTarget_ReturnsAddedDiff()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1" }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1" },
                new() { ActivityId = "A2", Name = "Task 2" }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(d =>
            d.MatchKey == "activityId:A2" &&
            d.ChangeType == ChangeType.Added.ToString() &&
            d.NewValue == "Task 2");
    }

    [Fact]
    public void Compare_OneRemovedActivityFromTarget_ReturnsRemovedDiff()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1" },
                new() { ActivityId = "A2", Name = "Task 2" }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1" }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(d =>
            d.MatchKey == "activityId:A2" &&
            d.ChangeType == ChangeType.Removed.ToString() &&
            d.OldValue == "Task 2");
    }

    [Fact]
    public void Compare_ModifiedDuration_ReturnsModifiedDiff()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", Duration = 5 }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", Duration = 10 }
            }
        };

        var result = _sut.Compare(source, target);

        var durationDiff = result.Should().ContainSingle(d =>
            d.MatchKey == "activityId:A1" &&
            d.FieldName == ComparisonFieldNames.Duration &&
            d.ChangeType == ChangeType.Modified.ToString()).Subject;
        durationDiff.OldValue.Should().Be("5");
        durationDiff.NewValue.Should().Be("10");
    }

    [Fact]
    public void Compare_MatchByProvenanceId_WhenAvailable()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ProvenanceId = "prov:1", ActivityId = "A1", Name = "Task 1" }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ProvenanceId = "prov:1", ActivityId = "A1", Name = "Task 1" }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().ContainSingle(d => d.MatchKey == "provenance:prov:1");
    }

    [Fact]
    public void Compare_MatchByActivityId_WhenProvenanceIdIsMissing()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ProvenanceId = "", ActivityId = "A1", Name = "Task 1" }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ProvenanceId = "", ActivityId = "A1", Name = "Task 1" }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().ContainSingle(d => d.MatchKey == "activityId:A1");
    }

    [Fact]
    public void Compare_MatchByWbsCodeAndActivityCode_WhenActivityIdIsMissing()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { WbsCode = "WBS1", ActivityCode = "C001", Name = "Task 1" }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { WbsCode = "WBS1", ActivityCode = "C001", Name = "Task 1" }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().ContainSingle(d => d.MatchKey == "wbs+code:WBS1:C001");
    }

    [Fact]
    public void Compare_LowConfidenceMatching_DifferentIdsSameName_ReturnsRemovedAndAdded()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Excavation" }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new()
                {
                    ActivityId = "B1",
                    Name = "Excavation",
                    Duration = 10
                }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(d =>
            d.ChangeType == ChangeType.Removed.ToString() &&
            d.MatchKey == "activityId:A1");
        result.Should().ContainSingle(d =>
            d.ChangeType == ChangeType.Added.ToString() &&
            d.MatchKey == "activityId:B1");
    }

    [Fact]
    public void Compare_UnmatchedEntity_FlaggedAsRemoved()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Source Only" }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "B1", Name = "Target Only" }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(d =>
            d.ChangeType == ChangeType.Removed.ToString() &&
            d.MatchKey == "activityId:A1" &&
            d.OldValue == "Source Only");
        result.Should().ContainSingle(d =>
            d.ChangeType == ChangeType.Added.ToString() &&
            d.MatchKey == "activityId:B1" &&
            d.NewValue == "Target Only");
    }

    [Fact]
    public void Compare_ModifiedStartDate_ReturnsModifiedDiffWithMajorSeverity()
    {
        var start = new DateTime(2024, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var startModified = new DateTime(2024, 1, 2, 8, 0, 0, DateTimeKind.Utc);
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", Start = start }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", Start = startModified }
            }
        };

        var result = _sut.Compare(source, target);

        var startDiff = result.Should().ContainSingle(d =>
            d.MatchKey == "activityId:A1" &&
            d.FieldName == ComparisonFieldNames.Start &&
            d.ChangeType == ChangeType.Modified.ToString()).Subject;
        startDiff.OldValue.Should().Be(start.ToString("O"));
        startDiff.NewValue.Should().Be(startModified.ToString("O"));
        startDiff.Severity.Should().Be("Major");
    }

    [Fact]
    public void Compare_ModifiedPercentComplete_ReturnsModifiedDiffWithMinorSeverity()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", PercentComplete = 50 }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", PercentComplete = 75 }
            }
        };

        var result = _sut.Compare(source, target);

        var pcDiff = result.Should().ContainSingle(d =>
            d.MatchKey == "activityId:A1" &&
            d.FieldName == ComparisonFieldNames.PercentComplete &&
            d.ChangeType == ChangeType.Modified.ToString()).Subject;
        pcDiff.OldValue.Should().Be("50");
        pcDiff.NewValue.Should().Be("75");
        pcDiff.Severity.Should().Be("Minor");
    }

    [Fact]
    public void Compare_NoMatchKey_EmptyKeysAreSkipped()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { Name = "Orphan" }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { Name = "Orphan" }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Compare_MultipleFieldsChanged_ReturnsDiffForEachChangedField()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new()
                {
                    ActivityId = "A1",
                    Name = "Task 1",
                    Duration = 5,
                    Status = "Not Started",
                    Start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new()
                {
                    ActivityId = "A1",
                    Name = "Task 1 Updated",
                    Duration = 10,
                    Status = "In Progress",
                    Start = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                }
            }
        };

        var result = _sut.Compare(source, target);

        result.Should().HaveCount(4);
        result.Should().ContainSingle(d => d.FieldName == ComparisonFieldNames.Name);
        result.Should().ContainSingle(d => d.FieldName == ComparisonFieldNames.Duration);
        result.Should().ContainSingle(d => d.FieldName == ComparisonFieldNames.Status);
        result.Should().ContainSingle(d => d.FieldName == ComparisonFieldNames.Start);
    }

    [Fact]
    public void Compare_EmptyActivities_ReturnsEmptyList()
    {
        var source = new ScheduleData { Activities = new List<ScheduleActivity>() };
        var target = new ScheduleData { Activities = new List<ScheduleActivity>() };

        var result = _sut.Compare(source, target);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Compare_ActivityStatusChanged_ReturnsModifiedDiff()
    {
        var source = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", Status = "Not Started" }
            }
        };
        var target = new ScheduleData
        {
            Activities = new List<ScheduleActivity>
            {
                new() { ActivityId = "A1", Name = "Task 1", Status = "Completed" }
            }
        };

        var result = _sut.Compare(source, target);

        var statusDiff = result.Should().ContainSingle(d =>
            d.MatchKey == "activityId:A1" &&
            d.FieldName == ComparisonFieldNames.Status &&
            d.ChangeType == ChangeType.Modified.ToString()).Subject;
        statusDiff.OldValue.Should().Be("Not Started");
        statusDiff.NewValue.Should().Be("Completed");
    }
}
