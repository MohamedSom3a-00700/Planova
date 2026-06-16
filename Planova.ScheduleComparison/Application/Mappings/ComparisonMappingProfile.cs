using Planova.ScheduleComparison.Application.Dto;
using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Application.Mappings;

public static class ComparisonMappingProfile
{
    public static ComparisonSessionDto ToDto(this ComparisonSession session)
    {
        return new ComparisonSessionDto
        {
            Id = session.Id,
            ProjectId = session.ProjectId,
            Mode = session.Mode.ToString(),
            State = session.State.ToString(),
            SourceLabel = session.SourceLabel,
            TargetLabel = session.TargetLabel,
            Error = session.Error,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt
        };
    }

    public static List<ComparisonSessionDto> ToDtoList(this List<ComparisonSession> sessions)
    {
        return sessions.Select(s => s.ToDto()).ToList();
    }

    public static ComparisonSummaryDto ToSummaryDto(this ComparisonSummary summary)
    {
        var totalChanges = summary.AddedActivities + summary.RemovedActivities + summary.ModifiedActivities
            + summary.AddedRelationships + summary.RemovedRelationships + summary.ModifiedRelationships
            + summary.AddedAssignments + summary.RemovedAssignments + summary.ModifiedAssignments;

        return new ComparisonSummaryDto
        {
            TotalChanges = totalChanges,
            Added = summary.AddedActivities + summary.AddedRelationships + summary.AddedAssignments,
            Removed = summary.RemovedActivities + summary.RemovedRelationships + summary.RemovedAssignments,
            Modified = summary.ModifiedActivities + summary.ModifiedRelationships + summary.ModifiedAssignments,
            ActivityChanges = summary.ModifiedActivities,
            LogicChanges = summary.ModifiedRelationships,
            ResourceChanges = summary.ModifiedAssignments,
            HasCriticalPathChanges = summary.CriticalPathDurationDelta.HasValue,
            HasFloatChanges = summary.ActivitiesWithFloatLoss > 0 || summary.ActivitiesWithFloatGain > 0
        };
    }

    public static ActivityDiffDto ToDto(this ActivityDiff diff)
    {
        return new ActivityDiffDto
        {
            MatchKey = diff.MatchKey,
            FieldName = diff.FieldName,
            ChangeType = diff.ChangeType,
            OldValue = diff.OldValue,
            NewValue = diff.NewValue,
            Severity = diff.Severity
        };
    }

    public static LogicDiffDto ToDto(this LogicDiff diff)
    {
        return new LogicDiffDto
        {
            PredecessorMatchKey = diff.PredecessorMatchKey,
            SuccessorMatchKey = diff.SuccessorMatchKey,
            ChangeType = diff.ChangeType,
            OldRelationshipType = diff.OldRelationshipType,
            NewRelationshipType = diff.NewRelationshipType,
            OldLag = diff.OldLag,
            NewLag = diff.NewLag
        };
    }

    public static ResourceDiffDto ToDto(this ResourceDiff diff)
    {
        return new ResourceDiffDto
        {
            ActivityMatchKey = diff.ActivityMatchKey,
            ResourceId = diff.ResourceId,
            ChangeType = diff.ChangeType,
            OldUnits = diff.OldUnits,
            NewUnits = diff.NewUnits,
            OldCost = diff.OldCost,
            NewCost = diff.NewCost
        };
    }

    public static CriticalPathDiffDto ToDto(this CriticalPathDiff diff)
    {
        return new CriticalPathDiffDto
        {
            SourceDuration = diff.SourceDuration,
            TargetDuration = diff.TargetDuration,
            DurationChange = diff.DurationChange,
            EnteredCriticalPath = diff.EnteredCriticalPath,
            ExitedCriticalPath = diff.ExitedCriticalPath,
            RemainedOnCriticalPath = diff.RemainedOnCriticalPath
        };
    }

    public static FloatImpactDto ToDto(this ActivityFloatDelta delta)
    {
        return new FloatImpactDto
        {
            MatchKey = delta.MatchKey,
            OldTotalFloat = delta.OldTotalFloat,
            NewTotalFloat = delta.NewTotalFloat,
            FloatDelta = delta.FloatDelta,
            OldFreeFloat = delta.OldFreeFloat,
            NewFreeFloat = delta.NewFreeFloat,
            FreeFloatDelta = delta.FreeFloatDelta
        };
    }

    public static List<ComparisonResult> ToEntityRows(
        this ScheduleComparisonResult result,
        Guid sessionId,
        List<ActivityDiff> activityDiffs,
        List<LogicDiff> logicDiffs,
        List<ResourceDiff> resourceDiffs)
    {
        var rows = new List<ComparisonResult>();

        foreach (var diff in activityDiffs)
        {
            if (diff.ChangeType == ChangeType.Unchanged.ToString())
                continue;

            rows.Add(new ComparisonResult
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                EntityType = "Activity",
                MatchKey = diff.MatchKey,
                ChangeType = Enum.Parse<ChangeType>(diff.ChangeType),
                MatchConfidence = MatchConfidence.High,
                FieldName = string.IsNullOrEmpty(diff.FieldName) ? null : diff.FieldName,
                OldValue = diff.OldValue,
                NewValue = diff.NewValue,
                Severity = diff.Severity
            });
        }

        foreach (var diff in logicDiffs)
        {
            if (diff.ChangeType == ChangeType.Unchanged.ToString())
                continue;

            rows.Add(new ComparisonResult
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                EntityType = "Relationship",
                MatchKey = $"{diff.PredecessorMatchKey}->{diff.SuccessorMatchKey}",
                ChangeType = Enum.Parse<ChangeType>(diff.ChangeType),
                MatchConfidence = MatchConfidence.High,
                FieldName = "RelationshipType",
                OldValue = diff.OldRelationshipType,
                NewValue = diff.NewRelationshipType,
                Severity = "Major"
            });
        }

        foreach (var diff in resourceDiffs)
        {
            if (diff.ChangeType == ChangeType.Unchanged.ToString())
                continue;

            rows.Add(new ComparisonResult
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                EntityType = "ResourceAssignment",
                MatchKey = $"{diff.ActivityMatchKey}:{diff.ResourceId}",
                ChangeType = Enum.Parse<ChangeType>(diff.ChangeType),
                MatchConfidence = MatchConfidence.High,
                FieldName = "Units",
                OldValue = diff.OldUnits?.ToString(),
                NewValue = diff.NewUnits?.ToString(),
                Severity = "Minor"
            });
        }

        return rows;
    }
}
