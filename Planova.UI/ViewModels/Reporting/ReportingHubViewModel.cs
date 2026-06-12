using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Reporting;

public partial class ReportingHubTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public ReportingHubTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class ReportingHubViewModel : ObservableObject
{
    public Action<string>? OnHubStatusMessage { get; set; }

    [ObservableProperty]
    private ReportingHubTab? _selectedTab;

    public ObservableCollection<ReportingHubTab> Tabs { get; } = new();
}
