using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Dashboard;

public partial class DistributionSummaryItem : ObservableObject
{
    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    private string _color = "#00BFFF";
}
