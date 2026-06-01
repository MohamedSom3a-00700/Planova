namespace Planova.Domain.ValueObjects;

public sealed class ProjectStatus
{
    public static readonly ProjectStatus Draft = new("Draft");
    public static readonly ProjectStatus UnderReview = new("Under Review");
    public static readonly ProjectStatus Approved = new("Approved");
    public static readonly ProjectStatus InProgress = new("In Progress");
    public static readonly ProjectStatus OnHold = new("On Hold");
    public static readonly ProjectStatus Completed = new("Completed");
    public static readonly ProjectStatus Cancelled = new("Cancelled");

    private static readonly Dictionary<ProjectStatus, HashSet<ProjectStatus>> Transitions = new()
    {
        [Draft] = new HashSet<ProjectStatus> { UnderReview, Cancelled },
        [UnderReview] = new HashSet<ProjectStatus> { Approved, Draft, Cancelled },
        [Approved] = new HashSet<ProjectStatus> { InProgress, Cancelled },
        [InProgress] = new HashSet<ProjectStatus> { OnHold, Completed, Cancelled },
        [OnHold] = new HashSet<ProjectStatus> { InProgress, Completed, Cancelled },
        [Completed] = new HashSet<ProjectStatus>(),
        [Cancelled] = new HashSet<ProjectStatus>(),
    };

    public string Value { get; }

    private ProjectStatus(string value)
    {
        Value = value;
    }

    public static ProjectStatus FromString(string value)
    {
        return value switch
        {
            "Draft" => Draft,
            "Under Review" => UnderReview,
            "Approved" => Approved,
            "In Progress" => InProgress,
            "On Hold" => OnHold,
            "Completed" => Completed,
            "Cancelled" => Cancelled,
            _ => throw new ArgumentException($"Invalid project status: {value}", nameof(value)),
        };
    }

    public static IReadOnlyList<ProjectStatus> All => new[]
    {
        Draft, UnderReview, Approved, InProgress, OnHold, Completed, Cancelled
    };

    public bool CanTransitionTo(ProjectStatus target)
    {
        return Transitions.TryGetValue(this, out var allowed) && allowed.Contains(target);
    }

    public IReadOnlySet<ProjectStatus> AllowedNext()
    {
        return Transitions.TryGetValue(this, out var allowed)
            ? allowed.ToHashSet()
            : new HashSet<ProjectStatus>();
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) =>
        obj is ProjectStatus other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ProjectStatus? left, ProjectStatus? right) =>
        Equals(left, right);

    public static bool operator !=(ProjectStatus? left, ProjectStatus? right) =>
        !Equals(left, right);
}
