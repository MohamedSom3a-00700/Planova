namespace Planova.Shared.Abstractions;

public interface INavigationService
{
    void NavigateTo(string targetId);
    void RegisterTarget(string id, string displayName, Func<object> viewFactory);
    string GetActiveTarget();
}
