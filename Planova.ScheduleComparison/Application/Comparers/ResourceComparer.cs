using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Application.Comparers;

public class ResourceComparer
{
    public List<ResourceDiff> Compare(ScheduleData source, ScheduleData target)
    {
        var diffs = new List<ResourceDiff>();
        var sourceByKey = BuildMatchDictionary(source.ResourceAssignments);
        var targetByKey = BuildMatchDictionary(target.ResourceAssignments);

        var matchedKeys = new HashSet<string>();

        foreach (var (matchKey, sourceAssignment) in sourceByKey)
        {
            if (targetByKey.TryGetValue(matchKey, out var targetAssignment))
            {
                matchedKeys.Add(matchKey);
                var diff = CompareAssignment(sourceAssignment, targetAssignment, matchKey);
                if (diff != null)
                    diffs.Add(diff);
            }
            else
            {
                diffs.Add(new ResourceDiff
                {
                    ActivityMatchKey = sourceAssignment.ActivityMatchKey,
                    ResourceId = sourceAssignment.ResourceId,
                    ChangeType = ChangeType.Removed.ToString(),
                    OldUnits = sourceAssignment.Units,
                    OldCost = sourceAssignment.Cost
                });
            }
        }

        foreach (var (matchKey, targetAssignment) in targetByKey)
        {
            if (!matchedKeys.Contains(matchKey))
            {
                diffs.Add(new ResourceDiff
                {
                    ActivityMatchKey = targetAssignment.ActivityMatchKey,
                    ResourceId = targetAssignment.ResourceId,
                    ChangeType = ChangeType.Added.ToString(),
                    NewUnits = targetAssignment.Units,
                    NewCost = targetAssignment.Cost
                });
            }
        }

        return diffs;
    }

    private static Dictionary<string, ScheduleResourceAssignment> BuildMatchDictionary(List<ScheduleResourceAssignment> assignments)
    {
        var dict = new Dictionary<string, ScheduleResourceAssignment>();

        foreach (var assignment in assignments)
        {
            var matchKey = $"{assignment.ActivityMatchKey}:{assignment.ResourceId}";
            if (!dict.ContainsKey(matchKey))
                dict[matchKey] = assignment;
        }

        return dict;
    }

    private static ResourceDiff? CompareAssignment(
        ScheduleResourceAssignment source,
        ScheduleResourceAssignment target,
        string matchKey)
    {
        if (source.Units == target.Units && source.Cost == target.Cost)
            return null;

        return new ResourceDiff
        {
            ActivityMatchKey = source.ActivityMatchKey,
            ResourceId = source.ResourceId,
            ChangeType = ChangeType.Modified.ToString(),
            OldUnits = source.Units,
            NewUnits = target.Units,
            OldCost = source.Cost,
            NewCost = target.Cost
        };
    }
}
