namespace Planova.Resource.Domain.Enums;

public enum CrewStatus
{
    Draft,
    Active,
    Inactive
}

public static class CrewStatusTransitions
{
    private static readonly HashSet<(CrewStatus From, CrewStatus To)> ValidTransitions = new()
    {
        (CrewStatus.Draft, CrewStatus.Active),
        (CrewStatus.Draft, CrewStatus.Inactive),
        (CrewStatus.Active, CrewStatus.Inactive),
        (CrewStatus.Inactive, CrewStatus.Active)
    };

    public static bool IsValidTransition(CrewStatus from, CrewStatus to)
    {
        return ValidTransitions.Contains((from, to));
    }
}
