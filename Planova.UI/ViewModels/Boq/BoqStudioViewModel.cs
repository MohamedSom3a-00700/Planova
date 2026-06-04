using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public BoqStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class BoqStudioViewModel : ObservableObject
{
    [ObservableProperty]
    private BoqStudioTab? _selectedTab;

    public ObservableCollection<BoqStudioTab> Tabs { get; } = new();
}
