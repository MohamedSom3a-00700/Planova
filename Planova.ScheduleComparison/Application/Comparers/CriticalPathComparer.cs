using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Application.Comparers;

public class CriticalPathComparer
{
    public CriticalPathDiff? Compare(ScheduleData source, ScheduleData target)
    {
        var sourceCritical = source.Activities
            .Where(a => a.IsCritical)
            .ToDictionary(a => ActivityComparer.ResolveMatchKey(a, source.Activities));

        var targetCritical = target.Activities
            .Where(a => a.IsCritical)
            .ToDictionary(a => ActivityComparer.ResolveMatchKey(a, target.Activities));

        if (sourceCritical.Count == 0 && targetCritical.Count == 0)
            return null;

        var entered = new List<string>();
        var exited = new List<string>();
        var remained = new List<string>();

        foreach (var (key, _) in targetCritical)
        {
            if (sourceCritical.ContainsKey(key))
                remained.Add(key);
            else
                entered.Add(key);
        }

        foreach (var (key, _) in sourceCritical)
        {
            if (!targetCritical.ContainsKey(key))
                exited.Add(key);
        }

        var sourceDuration = source.Activities
            .Where(a => a.IsCritical)
            .MaxBy(a => a.Finish)
            ?.Finish;

        var targetDuration = target.Activities
            .Where(a => a.IsCritical)
            .MaxBy(a => a.Finish)
            ?.Finish;

        double? durationChange = null;
        if (sourceDuration.HasValue && targetDuration.HasValue)
            durationChange = (targetDuration.Value - sourceDuration.Value).TotalDays;

        return new CriticalPathDiff
        {
            SourceDuration = sourceDuration.HasValue ? (sourceDuration.Value - (source.Activities.MinBy(a => a.Start)?.Start ?? sourceDuration.Value)).TotalDays : null,
            TargetDuration = targetDuration.HasValue ? (targetDuration.Value - (target.Activities.MinBy(a => a.Start)?.Start ?? targetDuration.Value)).TotalDays : null,
            DurationChange = durationChange,
            EnteredCriticalPath = entered,
            ExitedCriticalPath = exited,
            RemainedOnCriticalPath = remained
        };
    }
}
