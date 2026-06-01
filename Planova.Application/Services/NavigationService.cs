using Planova.Shared.Abstractions;

namespace Planova.Application.Services;

public class NavigationService : INavigationService
{
    private readonly Dictionary<string, (string DisplayName, Func<object> ViewFactory)> _targets = new();
    private string _activeTarget = string.Empty;

    public event EventHandler<string>? ActiveTargetChanged;

    public void RegisterTarget(string id, string displayName, Func<object> viewFactory)
    {
        _targets[id] = (displayName, viewFactory);
    }

    public void NavigateTo(string targetId)
    {
        if (!_targets.ContainsKey(targetId))
            return;

        _activeTarget = targetId;
        ActiveTargetChanged?.Invoke(this, targetId);
    }

    public string GetActiveTarget()
    {
        return _activeTarget;
    }

    public IReadOnlyCollection<NavigationTargetInfo> GetTargets()
    {
        return _targets.Select(target => new NavigationTargetInfo(target.Key, target.Value.DisplayName)).ToArray();
    }

    public bool TryCreateView(string targetId, out object? view)
    {
        if (_targets.TryGetValue(targetId, out var target))
        {
            view = target.ViewFactory();
            return true;
        }

        view = null;
        return false;
    }
}
