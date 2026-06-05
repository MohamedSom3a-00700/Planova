using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels;

public sealed partial class WorkspaceTabViewModel : ObservableObject
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

    [ObservableProperty]
    private bool _isSelected;
}
