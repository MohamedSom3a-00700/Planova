using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Mappings;
using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Tests.Application.Services;

public class ScheduleComparisonResultPipelineTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private static ScheduleActivity Activity(string id, string name, double? duration = null, string? provenanceId = null, bool isCritical = false) => new()
    {
        ActivityId = id,
        Name = name,
        Duration = duration,
        ProvenanceId = provenanceId ?? id,
        Status = "Active",
        Start = new DateTime(2026, 1, 1),
        Finish = new DateTime(2026, 1, 10),
        PercentComplete = 0,
        IsCritical = isCritical
    };

    [Fact]
    public void FullResultPipeline_WithAllComparers_RoundTripsAndMapsToDtos()
    {
        var source = new ScheduleData
        {
            Activities =
            [
                Activity("A1", "Foundation", 5, isCritical: true),
                Activity("A2", "Walls", 10, isCritical: true),
                Activity("A3", "Roof", 7),
            ],
            Relationships =
            [
                new() { PredecessorProvenanceId = "A1", SuccessorProvenanceId = "A2", RelationshipType = "FS", Lag = 0 },
                new() { PredecessorProvenanceId = "A2", SuccessorProvenanceId = "A3", RelationshipType = "FS", Lag = 2 },
            ],
            ResourceAssignments =
            [
                new() { ActivityMatchKey = "A1", ResourceId = "R001", Units = 100, Cost = 5000 },
            ]
        };

        var target = new ScheduleData
        {
            Activities =
            [
                Activity("A1", "Foundation", 5, isCritical: true),
                Activity("A2", "Walls", 12),
                Activity("A4", "Finishing", 6, isCritical: true),
            ],
            Relationships =
            [
                new() { PredecessorProvenanceId = "A1", SuccessorProvenanceId = "A2", RelationshipType = "FS", Lag = 0 },
                new() { PredecessorProvenanceId = "A2", SuccessorProvenanceId = "A4", RelationshipType = "SS", Lag = 1 },
            ],
            ResourceAssignments =
            [
                new() { ActivityMatchKey = "A1", ResourceId = "R001", Units = 120, Cost = 6000 },
                new() { ActivityMatchKey = "A4", ResourceId = "R002", Units = 80, Cost = 4000 },
            ]
        };

        var activityDiffs = new ActivityComparer().Compare(source, target);
        var logicDiffs = new LogicComparer().Compare(source, target);
        var resourceDiffs = new ResourceComparer().Compare(source, target);
        var criticalPathDiff = new CriticalPathComparer().Compare(source, target);
        var floatReport = new FloatComparer().Compare(source, target);

        var original = new ScheduleComparisonResult
        {
            SessionId = Guid.NewGuid(),
            ProjectId = 1,
            Mode = "UpdateVsUpdate",
            ComparedAt = DateTime.UtcNow,
            Source = new ComparisonSourceInfo { SourceKind = "Snapshot", Label = "Baseline" },
            Target = new ComparisonSourceInfo { SourceKind = "Snapshot", Label = "Update" },
            IncludedScopes = ["Activities", "Logic", "Resources", "CriticalPath", "Float"],
            GeneratedByVersion = "1.0.0.0",
            ActivityDiffs = activityDiffs,
            LogicDiffs = logicDiffs,
            ResourceDiffs = resourceDiffs,
            CriticalPathDiffResult = criticalPathDiff,
            FloatReport = floatReport,
            Summary = new ComparisonSummary()
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ScheduleComparisonResult>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.SessionId.Should().Be(original.SessionId);
        deserialized.ProjectId.Should().Be(1);
        deserialized.Mode.Should().Be("UpdateVsUpdate");
        deserialized.Source.Label.Should().Be("Baseline");
        deserialized.Target.Label.Should().Be("Update");

        deserialized.ActivityDiffs.Should().HaveCount(original.ActivityDiffs.Count);
        deserialized.LogicDiffs.Should().HaveCount(original.LogicDiffs.Count);
        deserialized.ResourceDiffs.Should().HaveCount(original.ResourceDiffs.Count);
        deserialized.CriticalPathDiffResult.Should().NotBeNull();
        deserialized.FloatReport.Should().NotBeNull();

        var activityDto = deserialized.ActivityDiffs.Select(d => d.ToDto()).ToList();
        activityDto.Should().HaveCount(original.ActivityDiffs.Count);
        activityDto[0].MatchKey.Should().NotBeNullOrEmpty();

        var logicDto = deserialized.LogicDiffs.Select(d => d.ToDto()).ToList();
        logicDto.Should().HaveCount(original.LogicDiffs.Count);

        var resourceDto = deserialized.ResourceDiffs.Select(d => d.ToDto()).ToList();
        resourceDto.Should().HaveCount(original.ResourceDiffs.Count);

        var cpDto = deserialized.CriticalPathDiffResult!.ToDto();
        cpDto.Should().NotBeNull();
        cpDto.EnteredCriticalPath.Should().NotBeNull();
        cpDto.ExitedCriticalPath.Should().NotBeNull();

        var floatDeltas = deserialized.FloatReport!.ActivityFloatDeltas.Select(d => d.ToDto()).ToList();
        floatDeltas.Should().HaveCount(original.FloatReport!.ActivityFloatDeltas.Count);
    }

    [Fact]
    public void ResultWithNullOptionalSections_RoundTrip_Succeeds()
    {
        var original = new ScheduleComparisonResult
        {
            SessionId = Guid.NewGuid(),
            ProjectId = 2,
            Mode = "BaselineVsUpdate",
            ComparedAt = DateTime.UtcNow,
            Source = new ComparisonSourceInfo { SourceKind = "Snapshot", Label = "A" },
            Target = new ComparisonSourceInfo { SourceKind = "Snapshot", Label = "B" },
            IncludedScopes = ["Activities"],
            GeneratedByVersion = "1.0.0.0",
            ActivityDiffs =
            [
                new() { MatchKey = "act:1", FieldName = "Duration", ChangeType = "Modified", OldValue = "5", NewValue = "10", Severity = "Minor" }
            ],
            LogicDiffs = [],
            ResourceDiffs = [],
            CriticalPathDiffResult = null,
            FloatReport = null,
            Summary = new ComparisonSummary()
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ScheduleComparisonResult>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.CriticalPathDiffResult.Should().BeNull();
        deserialized.FloatReport.Should().BeNull();
        deserialized.ActivityDiffs.Should().HaveCount(1);
        deserialized.LogicDiffs.Should().BeEmpty();
        deserialized.ResourceDiffs.Should().BeEmpty();

        var activityDto = deserialized.ActivityDiffs.Select(d => d.ToDto()).ToList();
        activityDto.Should().ContainSingle();
        activityDto[0].FieldName.Should().Be("Duration");
        activityDto[0].ChangeType.Should().Be("Modified");
    }

    [Fact]
    public void EmptyResult_RoundTrip_AllCollectionsEmpty()
    {
        var original = new ScheduleComparisonResult
        {
            SessionId = Guid.NewGuid(),
            ProjectId = 3,
            Mode = "UpdateVsUpdate",
            ComparedAt = DateTime.UtcNow,
            Source = new ComparisonSourceInfo { SourceKind = "Snapshot", Label = "Src" },
            Target = new ComparisonSourceInfo { SourceKind = "Snapshot", Label = "Tgt" },
            IncludedScopes = [],
            GeneratedByVersion = "1.0.0.0",
            ActivityDiffs = [],
            LogicDiffs = [],
            ResourceDiffs = [],
            CriticalPathDiffResult = null,
            FloatReport = null,
            Summary = new ComparisonSummary()
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ScheduleComparisonResult>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.ActivityDiffs.Should().BeEmpty();
        deserialized.LogicDiffs.Should().BeEmpty();
        deserialized.ResourceDiffs.Should().BeEmpty();
        deserialized.CriticalPathDiffResult.Should().BeNull();
        deserialized.FloatReport.Should().BeNull();

        deserialized.ActivityDiffs.Select(d => d.ToDto()).Should().BeEmpty();
        deserialized.LogicDiffs.Select(d => d.ToDto()).Should().BeEmpty();
        deserialized.ResourceDiffs.Select(d => d.ToDto()).Should().BeEmpty();
    }

    [Fact]
    public void FloatReportWithMixedChanges_RoundTrip_MapsCorrectly()
    {
        var original = new ScheduleComparisonResult
        {
            SessionId = Guid.NewGuid(),
            ProjectId = 4,
            Mode = "UpdateVsUpdate",
            ComparedAt = DateTime.UtcNow,
            Source = new ComparisonSourceInfo(),
            Target = new ComparisonSourceInfo(),
            IncludedScopes = ["Float"],
            ActivityDiffs = [],
            LogicDiffs = [],
            ResourceDiffs = [],
            CriticalPathDiffResult = null,
            FloatReport = new FloatImpactReport
            {
                ActivityFloatDeltas =
                [
                    new() { MatchKey = "act:1", OldTotalFloat = 10, NewTotalFloat = 20, FloatDelta = 10 },
                    new() { MatchKey = "act:2", OldTotalFloat = 15, NewTotalFloat = 3, FloatDelta = -12 },
                ],
                ActivitiesWithNegativeFloat = ["act:2"],
                ActivitiesWithImprovedFloat = ["act:1"],
                ActivitiesWithWorsenedFloat = ["act:2"]
            },
            Summary = new ComparisonSummary()
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ScheduleComparisonResult>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.FloatReport.Should().NotBeNull();
        deserialized.FloatReport!.ActivityFloatDeltas.Should().HaveCount(2);
        deserialized.FloatReport.ActivitiesWithNegativeFloat.Should().ContainSingle("act:2");
        deserialized.FloatReport.ActivitiesWithImprovedFloat.Should().ContainSingle("act:1");
        deserialized.FloatReport.ActivitiesWithWorsenedFloat.Should().ContainSingle("act:2");

        var deltas = deserialized.FloatReport.ActivityFloatDeltas.Select(d => d.ToDto()).ToList();
        deltas.Should().HaveCount(2);
        deltas[0].MatchKey.Should().Be("act:1");
        deltas[0].FloatDelta.Should().Be(10);
        deltas[1].MatchKey.Should().Be("act:2");
        deltas[1].FloatDelta.Should().Be(-12);
    }

    [Fact]
    public void CriticalPathWithChanges_RoundTrip_MapsCorrectly()
    {
        var original = new ScheduleComparisonResult
        {
            SessionId = Guid.NewGuid(),
            ProjectId = 5,
            Mode = "UpdateVsUpdate",
            ComparedAt = DateTime.UtcNow,
            Source = new ComparisonSourceInfo(),
            Target = new ComparisonSourceInfo(),
            IncludedScopes = ["CriticalPath"],
            ActivityDiffs = [],
            LogicDiffs = [],
            ResourceDiffs = [],
            CriticalPathDiffResult = new CriticalPathDiff
            {
                SourceDuration = 30,
                TargetDuration = 35,
                DurationChange = 5,
                EnteredCriticalPath = ["A3", "A4"],
                ExitedCriticalPath = ["A1"],
                RemainedOnCriticalPath = ["A2"]
            },
            FloatReport = null,
            Summary = new ComparisonSummary()
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ScheduleComparisonResult>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.CriticalPathDiffResult.Should().NotBeNull();
        deserialized.CriticalPathDiffResult!.SourceDuration.Should().Be(30);
        deserialized.CriticalPathDiffResult.TargetDuration.Should().Be(35);
        deserialized.CriticalPathDiffResult.DurationChange.Should().Be(5);
        deserialized.CriticalPathDiffResult.EnteredCriticalPath.Should().BeEquivalentTo(["A3", "A4"]);
        deserialized.CriticalPathDiffResult.ExitedCriticalPath.Should().BeEquivalentTo(["A1"]);
        deserialized.CriticalPathDiffResult.RemainedOnCriticalPath.Should().BeEquivalentTo(["A2"]);

        var dto = deserialized.CriticalPathDiffResult.ToDto();
        dto.SourceDuration.Should().Be(30);
        dto.TargetDuration.Should().Be(35);
        dto.EnteredCriticalPath.Should().BeEquivalentTo(["A3", "A4"]);
    }
}
