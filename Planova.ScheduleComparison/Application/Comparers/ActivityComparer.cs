using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Constants;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Application.Comparers;

public class ActivityComparer
{
    public List<ActivityDiff> Compare(ScheduleData source, ScheduleData target)
    {
        var diffs = new List<ActivityDiff>();
        var sourceByKey = BuildMatchDictionary(source.Activities);
        var targetByKey = BuildMatchDictionary(target.Activities);

        var matchedKeys = new HashSet<string>();

        foreach (var (matchKey, sourceActivity) in sourceByKey)
        {
            if (targetByKey.TryGetValue(matchKey, out var targetActivity))
            {
                matchedKeys.Add(matchKey);
                diffs.AddRange(CompareFields(sourceActivity, targetActivity, matchKey));
            }
            else
            {
                diffs.Add(new ActivityDiff
                {
                    MatchKey = matchKey,
                    FieldName = null!,
                    ChangeType = ChangeType.Removed.ToString(),
                    OldValue = sourceActivity.Name,
                    Severity = "Major"
                });
            }
        }

        foreach (var (matchKey, targetActivity) in targetByKey)
        {
            if (!matchedKeys.Contains(matchKey))
            {
                diffs.Add(new ActivityDiff
                {
                    MatchKey = matchKey,
                    FieldName = null!,
                    ChangeType = ChangeType.Added.ToString(),
                    NewValue = targetActivity.Name,
                    Severity = "Major"
                });
            }
        }

        return diffs;
    }

    private static Dictionary<string, ScheduleActivity> BuildMatchDictionary(List<ScheduleActivity> activities)
    {
        var dict = new Dictionary<string, ScheduleActivity>();

        foreach (var activity in activities)
        {
            var key = ResolveMatchKey(activity, activities);
            if (!string.IsNullOrEmpty(key) && !dict.ContainsKey(key))
            {
                dict[key] = activity;
            }
        }

        return dict;
    }

    public static string ResolveMatchKey(ScheduleActivity activity, List<ScheduleActivity> allActivities)
    {
        if (!string.IsNullOrEmpty(activity.ProvenanceId))
            return $"provenance:{activity.ProvenanceId}";

        if (!string.IsNullOrEmpty(activity.ActivityId))
            return $"activityId:{activity.ActivityId}";

        if (!string.IsNullOrEmpty(activity.WbsCode) && !string.IsNullOrEmpty(activity.ActivityCode))
            return $"wbs+code:{activity.WbsCode}:{activity.ActivityCode}";

        return string.Empty;
    }

    private static List<ActivityDiff> CompareFields(ScheduleActivity source, ScheduleActivity target, string matchKey)
    {
        var diffs = new List<ActivityDiff>();

        CompareStringField(diffs, matchKey, ComparisonFieldNames.Name, source.Name, target.Name);
        CompareStringField(diffs, matchKey, ComparisonFieldNames.Status, source.Status, target.Status);
        CompareDateTimeField(diffs, matchKey, ComparisonFieldNames.Start, source.Start, target.Start);
        CompareDateTimeField(diffs, matchKey, ComparisonFieldNames.Finish, source.Finish, target.Finish);
        CompareNumericField(diffs, matchKey, ComparisonFieldNames.Duration, source.Duration, target.Duration);
        CompareNumericField(diffs, matchKey, ComparisonFieldNames.OriginalDuration, source.OriginalDuration, target.OriginalDuration);
        CompareNumericField(diffs, matchKey, ComparisonFieldNames.RemainingDuration, source.RemainingDuration, target.RemainingDuration);
        CompareNumericField(diffs, matchKey, ComparisonFieldNames.PercentComplete, source.PercentComplete, target.PercentComplete);
        CompareDateTimeField(diffs, matchKey, ComparisonFieldNames.ActualStart, source.ActualStart, target.ActualStart);
        CompareDateTimeField(diffs, matchKey, ComparisonFieldNames.ActualFinish, source.ActualFinish, target.ActualFinish);
        CompareDateTimeField(diffs, matchKey, ComparisonFieldNames.EarlyStart, source.EarlyStart, target.EarlyStart);
        CompareDateTimeField(diffs, matchKey, ComparisonFieldNames.EarlyFinish, source.EarlyFinish, target.EarlyFinish);
        CompareDateTimeField(diffs, matchKey, ComparisonFieldNames.LateStart, source.LateStart, target.LateStart);
        CompareDateTimeField(diffs, matchKey, ComparisonFieldNames.LateFinish, source.LateFinish, target.LateFinish);
        CompareNumericField(diffs, matchKey, ComparisonFieldNames.TotalFloat, source.TotalFloat, target.TotalFloat);
        CompareNumericField(diffs, matchKey, ComparisonFieldNames.FreeFloat, source.FreeFloat, target.FreeFloat);
        CompareStringField(diffs, matchKey, ComparisonFieldNames.Calendar, source.Calendar, target.Calendar);

        if (diffs.Count == 0)
        {
            diffs.Add(new ActivityDiff
            {
                MatchKey = matchKey,
                FieldName = null!,
                ChangeType = ChangeType.Unchanged.ToString(),
                Severity = "Info"
            });
        }

        return diffs;
    }

    private static void CompareStringField(List<ActivityDiff> diffs, string matchKey, string fieldName, string? oldVal, string? newVal)
    {
        if (!string.Equals(oldVal, newVal, StringComparison.Ordinal))
        {
            diffs.Add(new ActivityDiff
            {
                MatchKey = matchKey,
                FieldName = fieldName,
                ChangeType = ChangeType.Modified.ToString(),
                OldValue = oldVal,
                NewValue = newVal,
                Severity = "Info"
            });
        }
    }

    private static void CompareDateTimeField(List<ActivityDiff> diffs, string matchKey, string fieldName, DateTime? oldVal, DateTime? newVal)
    {
        if (oldVal != newVal)
        {
            diffs.Add(new ActivityDiff
            {
                MatchKey = matchKey,
                FieldName = fieldName,
                ChangeType = ChangeType.Modified.ToString(),
                OldValue = oldVal?.ToString("O"),
                NewValue = newVal?.ToString("O"),
                Severity = "Major"
            });
        }
    }

    private static void CompareNumericField(List<ActivityDiff> diffs, string matchKey, string fieldName, double? oldVal, double? newVal)
    {
        if (oldVal != newVal)
        {
            diffs.Add(new ActivityDiff
            {
                MatchKey = matchKey,
                FieldName = fieldName,
                ChangeType = ChangeType.Modified.ToString(),
                OldValue = oldVal?.ToString(),
                NewValue = newVal?.ToString(),
                Severity = "Minor"
            });
        }
    }
}
