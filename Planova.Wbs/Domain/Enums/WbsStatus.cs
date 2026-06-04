namespace Planova.Wbs.Domain.Enums;

public enum WbsStatus
{
    Draft,
    Final,
    Revised,
    Approved
}

public static class WbsStatusTransitions
{
    private static readonly HashSet<(WbsStatus, WbsStatus)> ValidTransitions = new()
    {
        (WbsStatus.Draft, WbsStatus.Final),
        (WbsStatus.Final, WbsStatus.Approved),
        (WbsStatus.Approved, WbsStatus.Revised),
        (WbsStatus.Revised, WbsStatus.Draft),
    };

    public static bool CanTransition(WbsStatus from, WbsStatus to) =>
        ValidTransitions.Contains((from, to));
}
