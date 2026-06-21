namespace Planova.Wbs.Domain.Enums;

public enum WbsStatus
{
    Draft,
    UnderReview,
    Approved,
    Archived
}

public static class WbsStatusTransitions
{
    private static readonly HashSet<(WbsStatus, WbsStatus)> ValidTransitions = new()
    {
        (WbsStatus.Draft, WbsStatus.UnderReview),
        (WbsStatus.UnderReview, WbsStatus.Approved),
        (WbsStatus.Approved, WbsStatus.Archived),
        (WbsStatus.Draft, WbsStatus.Archived),
        (WbsStatus.UnderReview, WbsStatus.Draft),
        (WbsStatus.Approved, WbsStatus.UnderReview),
    };

    public static bool CanTransition(WbsStatus from, WbsStatus to) =>
        ValidTransitions.Contains((from, to));
}
