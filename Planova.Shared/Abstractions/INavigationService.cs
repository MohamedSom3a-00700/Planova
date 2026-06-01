namespace Planova.Shared.Abstractions;

public sealed record NavigationTargetInfo(string Id, string DisplayName);

public interface INavigationService
{
    event EventHandler<string>? ActiveTargetChanged;

    void NavigateTo(string targetId);
    void RegisterTarget(string id, string displayName, Func<object> viewFactory);
    string GetActiveTarget();
    IReadOnlyCollection<NavigationTargetInfo> GetTargets();
    bool TryCreateView(string targetId, out object? view);
}
