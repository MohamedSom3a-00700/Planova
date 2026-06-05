namespace Planova.Activity.Domain.Enums;

public enum ActivityStatus
{
    NotStarted,
    InProgress,
    Completed,
    OnHold,
    Revise
}

public static class ActivityStatusTransitions
{
    private static readonly HashSet<(ActivityStatus, ActivityStatus)> ValidTransitions = new()
    {
        (ActivityStatus.NotStarted, ActivityStatus.InProgress),
        (ActivityStatus.InProgress, ActivityStatus.Completed),
        (ActivityStatus.InProgress, ActivityStatus.OnHold),
        (ActivityStatus.InProgress, ActivityStatus.Revise),
        (ActivityStatus.OnHold, ActivityStatus.InProgress),
        (ActivityStatus.Revise, ActivityStatus.InProgress),
    };

    public static bool IsValid(ActivityStatus from, ActivityStatus to)
    {
        return ValidTransitions.Contains((from, to));
    }
}
