using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Activity;

public partial class ActivityStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public ActivityStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class ActivityStudioViewModel : ObservableObject
{
    [ObservableProperty]
    private ActivityStudioTab? _selectedTab;

    public ObservableCollection<ActivityStudioTab> Tabs { get; } = new();
}
