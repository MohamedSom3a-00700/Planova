using FluentAssertions;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Tests.Domain;

public class PrimaveraEntityTests
{
    [Fact]
    public void PrimaveraActivity_ShouldHaveDefaultValues()
    {
        var activity = new PrimaveraActivity();
        activity.TaskId.Should().BeEmpty();
        activity.SourceType.Should().Be(PrimaveraSourceType.Imported);
    }

    [Fact]
    public void PrimaveraBaseline_ShouldHaveRequiredProperties()
    {
        var baseline = new PrimaveraBaseline
        {
            BaselineId = "BL-001",
            Name = "Baseline 1",
            VersionNumber = 1,
            IsActive = true,
            SourceType = PrimaveraSourceType.Imported
        };

        baseline.BaselineId.Should().Be("BL-001");
        baseline.Name.Should().Be("Baseline 1");
        baseline.IsActive.Should().BeTrue();
        baseline.SourceType.Should().Be(PrimaveraSourceType.Imported);
    }

    [Fact]
    public void PrimaveraRelationship_ShouldStorePredSuccIds()
    {
        var rel = new PrimaveraRelationship
        {
            PredTaskId = "TASK-A",
            SuccTaskId = "TASK-B",
            Type = "FS",
            LagDuration = 2.0,
            SourceType = PrimaveraSourceType.Imported
        };

        rel.PredTaskId.Should().Be("TASK-A");
        rel.SuccTaskId.Should().Be("TASK-B");
        rel.LagDuration.Should().Be(2.0);
    }

    [Fact]
    public void PrimaveraResourceAssignment_ShouldStoreUnits()
    {
        var ra = new PrimaveraResourceAssignment
        {
            TaskId = "TASK-1",
            ResourceId = "R-001",
            Units = 40.0,
            CostPerUnit = 100m,
            SourceType = PrimaveraSourceType.Imported
        };

        ra.Units.Should().Be(40.0);
        ra.CostPerUnit.Should().Be(100m);
    }

    [Fact]
    public void PrimaveraValidationIssue_ShouldHaveDefaultSeverity()
    {
        var issue = new PrimaveraValidationIssue
        {
            Severity = PrimaveraValidationSeverity.Warning,
            EntityType = PrimaveraEntityType.Activity,
            Description = "Test issue"
        };

        issue.Severity.Should().Be(PrimaveraValidationSeverity.Warning);
        issue.IsResolved.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraRepairAction_ShouldTrackApplication()
    {
        var action = new PrimaveraRepairAction
        {
            Description = "Fix missing calendar",
            TargetEntityType = PrimaveraEntityType.Activity,
            AppliedBy = "TestUser",
            AppliedAt = DateTime.UtcNow,
            UndoAvailable = true,
            SourceType = PrimaveraSourceType.Repair
        };

        action.Description.Should().Be("Fix missing calendar");
        action.UndoAvailable.Should().BeTrue();
    }
}
