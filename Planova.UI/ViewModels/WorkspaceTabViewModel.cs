namespace Planova.UI.ViewModels;

public sealed class WorkspaceTabViewModel
{
    public WorkspaceTabViewModel(string id, string displayName, object content)
    {
        Id = id;
        DisplayName = displayName;
        Content = content;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public object Content { get; }
}
