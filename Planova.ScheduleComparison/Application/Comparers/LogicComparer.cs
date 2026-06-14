using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Application.Comparers;

public class LogicComparer
{
    public List<LogicDiff> Compare(ScheduleData source, ScheduleData target)
    {
        var diffs = new List<LogicDiff>();
        var sourceByKey = BuildMatchDictionary(source.Relationships);
        var targetByKey = BuildMatchDictionary(target.Relationships);

        var matchedKeys = new HashSet<string>();

        foreach (var (matchKey, sourceRel) in sourceByKey)
        {
            if (targetByKey.TryGetValue(matchKey, out var targetRel))
            {
                matchedKeys.Add(matchKey);
                var diff = CompareRelationship(sourceRel, targetRel);
                if (diff != null)
                    diffs.Add(diff);
            }
            else
            {
                diffs.Add(new LogicDiff
                {
                    PredecessorMatchKey = sourceRel.PredecessorMatchKey,
                    SuccessorMatchKey = sourceRel.SuccessorMatchKey,
                    ChangeType = ChangeType.Removed.ToString()
                });
            }
        }

        foreach (var (matchKey, targetRel) in targetByKey)
        {
            if (!matchedKeys.Contains(matchKey))
            {
                diffs.Add(new LogicDiff
                {
                    PredecessorMatchKey = targetRel.PredecessorMatchKey,
                    SuccessorMatchKey = targetRel.SuccessorMatchKey,
                    ChangeType = ChangeType.Added.ToString()
                });
            }
        }

        return diffs;
    }

    private static Dictionary<string, ScheduleRelationship> BuildMatchDictionary(List<ScheduleRelationship> relationships)
    {
        var dict = new Dictionary<string, ScheduleRelationship>();

        foreach (var rel in relationships)
        {
            var predKey = ResolveMatchKey(rel.PredecessorProvenanceId, rel.PredecessorActivityId);
            var succKey = ResolveMatchKey(rel.SuccessorProvenanceId, rel.SuccessorActivityId);
            var matchKey = $"{predKey}->{succKey}";

            rel.PredecessorMatchKey = predKey;
            rel.SuccessorMatchKey = succKey;

            if (!dict.ContainsKey(matchKey))
                dict[matchKey] = rel;
        }

        return dict;
    }

    private static string ResolveMatchKey(string provenanceId, string activityId)
    {
        if (!string.IsNullOrEmpty(provenanceId))
            return provenanceId;
        return activityId;
    }

    private static LogicDiff? CompareRelationship(ScheduleRelationship source, ScheduleRelationship target)
    {
        if (source.RelationshipType == target.RelationshipType
            && source.Lag == target.Lag)
        {
            return null;
        }

        return new LogicDiff
        {
            PredecessorMatchKey = source.PredecessorMatchKey,
            SuccessorMatchKey = source.SuccessorMatchKey,
            ChangeType = ChangeType.Modified.ToString(),
            OldRelationshipType = source.RelationshipType,
            NewRelationshipType = target.RelationshipType,
            OldLag = source.Lag,
            NewLag = target.Lag
        };
    }
}
