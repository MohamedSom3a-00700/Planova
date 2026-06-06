using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Cost;

public partial class CostStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public CostStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class CostStudioViewModel : ObservableObject
{
    [ObservableProperty]
    private CostStudioTab? _selectedTab;

    public ObservableCollection<CostStudioTab> Tabs { get; } = new();
}
