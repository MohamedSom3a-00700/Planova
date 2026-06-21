using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Dashboard;

public partial class CostCardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private decimal? _originalBudget;

    [ObservableProperty]
    private decimal? _currentBudget;

    [ObservableProperty]
    private decimal? _actualCost;

    [ObservableProperty]
    private decimal? _earnedValue;

    [ObservableProperty]
    private decimal? _cpi;

    [ObservableProperty]
    private decimal? _spi;

    [ObservableProperty]
    private bool _hasData;
}
