using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Excel;

public partial class ExcelStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public ExcelStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class ExcelStudioViewModel : ObservableObject
{
    [ObservableProperty]
    private ExcelStudioTab? _selectedTab;

    public ObservableCollection<ExcelStudioTab> Tabs { get; } = new();
}
