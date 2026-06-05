using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public ResourceStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class ResourceStudioViewModel : ObservableObject
{
    [ObservableProperty]
    private ResourceStudioTab? _selectedTab;

    public ObservableCollection<ResourceStudioTab> Tabs { get; } = new();
}
