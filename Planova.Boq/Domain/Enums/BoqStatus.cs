namespace Planova.Boq.Domain.Enums;

public enum BoqStatus
{
    Draft,
    Final,
    Revised,
    Approved
}

public static class BoqStatusTransitions
{
    private static readonly HashSet<(BoqStatus, BoqStatus)> ValidTransitions = new()
    {
        (BoqStatus.Draft, BoqStatus.Final),
        (BoqStatus.Final, BoqStatus.Revised),
        (BoqStatus.Final, BoqStatus.Approved),
        (BoqStatus.Revised, BoqStatus.Final),
        (BoqStatus.Revised, BoqStatus.Approved),
    };

    public static bool CanTransition(BoqStatus from, BoqStatus to) =>
        ValidTransitions.Contains((from, to));
}
