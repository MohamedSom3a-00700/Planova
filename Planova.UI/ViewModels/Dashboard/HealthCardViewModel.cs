using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Dashboard;

public partial class HealthCardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private decimal _scheduleHealthPct;

    [ObservableProperty]
    private decimal _costHealthPct;

    [ObservableProperty]
    private string _riskLevel = "Low";

    [ObservableProperty]
    private int _criticalActivityCount;

    [ObservableProperty]
    private string _healthIndicator = "Green";

    [ObservableProperty]
    private bool _hasData;
}
