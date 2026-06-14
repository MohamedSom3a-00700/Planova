using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Constants;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Application.Comparers;

public class FloatComparer
{
    public FloatImpactReport? Compare(ScheduleData source, ScheduleData target)
    {
        var sourceByKey = source.Activities
            .ToDictionary(a => ActivityComparer.ResolveMatchKey(a, source.Activities));

        var targetByKey = target.Activities
            .ToDictionary(a => ActivityComparer.ResolveMatchKey(a, target.Activities));

        if (sourceByKey.Count == 0 && targetByKey.Count == 0)
            return null;

        var deltas = new List<ActivityFloatDelta>();
        var negativeFloatKeys = new List<string>();
        var improvedKeys = new List<string>();
        var worsenedKeys = new List<string>();

        foreach (var (matchKey, targetAct) in targetByKey)
        {
            if (!sourceByKey.TryGetValue(matchKey, out var sourceAct))
                continue;

            var oldTotal = sourceAct.TotalFloat;
            var newTotal = targetAct.TotalFloat;
            var oldFree = sourceAct.FreeFloat;
            var newFree = targetAct.FreeFloat;

            if (oldTotal == newTotal && oldFree == newFree)
                continue;

            var delta = (newTotal ?? 0) - (oldTotal ?? 0);
            var freeDelta = (newFree ?? 0) - (oldFree ?? 0);

            deltas.Add(new ActivityFloatDelta
            {
                MatchKey = matchKey,
                OldTotalFloat = oldTotal,
                NewTotalFloat = newTotal,
                FloatDelta = delta,
                OldFreeFloat = oldFree,
                NewFreeFloat = newFree,
                FreeFloatDelta = freeDelta
            });

            if (newTotal < 0)
                negativeFloatKeys.Add(matchKey);

            if (delta > 0)
                improvedKeys.Add(matchKey);
            else if (delta < 0)
                worsenedKeys.Add(matchKey);
        }

        return new FloatImpactReport
        {
            ActivityFloatDeltas = deltas,
            ActivitiesWithNegativeFloat = negativeFloatKeys,
            ActivitiesWithImprovedFloat = improvedKeys,
            ActivitiesWithWorsenedFloat = worsenedKeys
        };
    }
}
