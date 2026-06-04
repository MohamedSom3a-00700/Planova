using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Wbs;

public partial class WbsStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public WbsStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class WbsStudioViewModel : ObservableObject
{
    [ObservableProperty]
    private WbsStudioTab? _selectedTab;

    public ObservableCollection<WbsStudioTab> Tabs { get; } = new();
}
