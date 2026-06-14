using FluentAssertions;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Tests.Application.Comparers;

public class LogicComparerTests
{
    private static readonly ScheduleRelationship FsRelationship = new()
    {
        PredecessorActivityId = "A",
        SuccessorActivityId = "B",
        RelationshipType = "FS",
        Lag = 0
    };

    private static readonly ScheduleRelationship FsRelationshipAlt = new()
    {
        PredecessorActivityId = "X",
        SuccessorActivityId = "Y",
        RelationshipType = "FS",
        Lag = 0
    };

    [Fact]
    public void Compare_IdenticalRelationships_ReturnsNoDiffs()
    {
        var source = new ScheduleData { Relationships = [FsRelationship] };
        var target = new ScheduleData { Relationships = [FsRelationship] };

        var result = new LogicComparer().Compare(source, target);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Compare_AddedRelationshipInTarget_ReturnsSingleAddedDiff()
    {
        var source = new ScheduleData { Relationships = [] };
        var target = new ScheduleData { Relationships = [FsRelationship] };

        var result = new LogicComparer().Compare(source, target);

        result.Should().ContainSingle();
        result[0].ChangeType.Should().Be(ChangeType.Added.ToString());
        result[0].PredecessorMatchKey.Should().Be("A");
        result[0].SuccessorMatchKey.Should().Be("B");
    }

    [Fact]
    public void Compare_RemovedRelationshipFromTarget_ReturnsSingleRemovedDiff()
    {
        var source = new ScheduleData { Relationships = [FsRelationship] };
        var target = new ScheduleData { Relationships = [] };

        var result = new LogicComparer().Compare(source, target);

        result.Should().ContainSingle();
        result[0].ChangeType.Should().Be(ChangeType.Removed.ToString());
        result[0].PredecessorMatchKey.Should().Be("A");
        result[0].SuccessorMatchKey.Should().Be("B");
    }

    [Fact]
    public void Compare_RelationshipTypeChangedFsToSs_ReturnsModifiedDiffWithOldAndNewTypes()
    {
        var sourceRel = new ScheduleRelationship
        {
            PredecessorActivityId = "A",
            SuccessorActivityId = "B",
            RelationshipType = "FS",
            Lag = 0
        };
        var targetRel = new ScheduleRelationship
        {
            PredecessorActivityId = "A",
            SuccessorActivityId = "B",
            RelationshipType = "SS",
            Lag = 0
        };

        var source = new ScheduleData { Relationships = [sourceRel] };
        var target = new ScheduleData { Relationships = [targetRel] };

        var result = new LogicComparer().Compare(source, target);

        result.Should().ContainSingle();
        result[0].ChangeType.Should().Be(ChangeType.Modified.ToString());
        result[0].OldRelationshipType.Should().Be("FS");
        result[0].NewRelationshipType.Should().Be("SS");
    }

    [Fact]
    public void Compare_LagDurationChanged_ReturnsModifiedDiffWithOldAndNewLags()
    {
        var sourceRel = new ScheduleRelationship
        {
            PredecessorActivityId = "A",
            SuccessorActivityId = "B",
            RelationshipType = "FS",
            Lag = 3
        };
        var targetRel = new ScheduleRelationship
        {
            PredecessorActivityId = "A",
            SuccessorActivityId = "B",
            RelationshipType = "FS",
            Lag = 7
        };

        var source = new ScheduleData { Relationships = [sourceRel] };
        var target = new ScheduleData { Relationships = [targetRel] };

        var result = new LogicComparer().Compare(source, target);

        result.Should().ContainSingle();
        result[0].ChangeType.Should().Be(ChangeType.Modified.ToString());
        result[0].OldLag.Should().Be(3);
        result[0].NewLag.Should().Be(7);
    }

    [Fact]
    public void Compare_BothPredecessorAndSuccessorChanged_ProducesSeparateDiffs()
    {
        var sourceRels = new List<ScheduleRelationship>
        {
            new()
            {
                PredecessorActivityId = "A",
                SuccessorActivityId = "B",
                RelationshipType = "FS",
                Lag = 0
            },
            new()
            {
                PredecessorActivityId = "C",
                SuccessorActivityId = "D",
                RelationshipType = "FS",
                Lag = 2
            }
        };
        var targetRels = new List<ScheduleRelationship>
        {
            new()
            {
                PredecessorActivityId = "A",
                SuccessorActivityId = "B",
                RelationshipType = "SS",
                Lag = 1
            },
            new()
            {
                PredecessorActivityId = "E",
                SuccessorActivityId = "F",
                RelationshipType = "FS",
                Lag = 0
            }
        };

        var source = new ScheduleData { Relationships = sourceRels };
        var target = new ScheduleData { Relationships = targetRels };

        var result = new LogicComparer().Compare(source, target);

        result.Should().HaveCount(3);

        result.Should().Contain(d => d.ChangeType == ChangeType.Modified.ToString()
                                     && d.PredecessorMatchKey == "A"
                                     && d.SuccessorMatchKey == "B"
                                     && d.OldRelationshipType == "FS"
                                     && d.NewRelationshipType == "SS"
                                     && d.OldLag == 0
                                     && d.NewLag == 1);

        result.Should().Contain(d => d.ChangeType == ChangeType.Removed.ToString()
                                     && d.PredecessorMatchKey == "C"
                                     && d.SuccessorMatchKey == "D");

        result.Should().Contain(d => d.ChangeType == ChangeType.Added.ToString()
                                     && d.PredecessorMatchKey == "E"
                                     && d.SuccessorMatchKey == "F");
    }

    [Fact]
    public void Compare_EmptySourceAndTarget_ReturnsNoDiffs()
    {
        var source = new ScheduleData { Relationships = [] };
        var target = new ScheduleData { Relationships = [] };

        var result = new LogicComparer().Compare(source, target);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Compare_EmptySourceWithRelationshipsInTarget_AllMarkedAdded()
    {
        var source = new ScheduleData { Relationships = [] };
        var target = new ScheduleData
        {
            Relationships =
            [
                FsRelationship,
                FsRelationshipAlt
            ]
        };

        var result = new LogicComparer().Compare(source, target);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(d => d.ChangeType.Should().Be(ChangeType.Added.ToString()));
    }

    [Fact]
    public void Compare_EmptyTargetWithRelationshipsInSource_AllMarkedRemoved()
    {
        var source = new ScheduleData
        {
            Relationships =
            [
                FsRelationship,
                FsRelationshipAlt
            ]
        };
        var target = new ScheduleData { Relationships = [] };

        var result = new LogicComparer().Compare(source, target);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(d => d.ChangeType.Should().Be(ChangeType.Removed.ToString()));
    }

    [Fact]
    public void Compare_RelationshipMatchedByProvenanceId_ResolvesMatchKeyFromProvenance()
    {
        var sourceRel = new ScheduleRelationship
        {
            PredecessorProvenanceId = "prov1",
            PredecessorActivityId = "act1",
            SuccessorProvenanceId = "prov2",
            SuccessorActivityId = "act2",
            RelationshipType = "FS",
            Lag = 0
        };
        var targetRel = new ScheduleRelationship
        {
            PredecessorProvenanceId = "prov1",
            PredecessorActivityId = "act1",
            SuccessorProvenanceId = "prov2",
            SuccessorActivityId = "act2",
            RelationshipType = "SS",
            Lag = 5
        };

        var source = new ScheduleData { Relationships = [sourceRel] };
        var target = new ScheduleData { Relationships = [targetRel] };

        var result = new LogicComparer().Compare(source, target);

        result.Should().ContainSingle();
        result[0].PredecessorMatchKey.Should().Be("prov1");
        result[0].SuccessorMatchKey.Should().Be("prov2");
        result[0].ChangeType.Should().Be(ChangeType.Modified.ToString());
    }
}
