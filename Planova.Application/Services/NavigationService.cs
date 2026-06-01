using Planova.Shared.Abstractions;

namespace Planova.Application.Services;

public class NavigationService : INavigationService
{
    private readonly Dictionary<string, (string DisplayName, Func<object> ViewFactory)> _targets = new();
    private string _activeTarget = string.Empty;

    public void RegisterTarget(string id, string displayName, Func<object> viewFactory)
    {
        _targets[id] = (displayName, viewFactory);
    }

    public void NavigateTo(string targetId)
    {
        if (!_targets.ContainsKey(targetId))
            return;

        _activeTarget = targetId;
    }

    public string GetActiveTarget()
    {
        return _activeTarget;
    }
}
