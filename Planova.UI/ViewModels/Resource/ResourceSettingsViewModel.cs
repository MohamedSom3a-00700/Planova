using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Resource;

public sealed partial class ResourceSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _useOrganizationStandardRates = true;

    [ObservableProperty]
    private bool _autoApplyEscalation;

    [ObservableProperty]
    private decimal _escalationRate;

    [ObservableProperty]
    private string _defaultCurrency = "USD";

    [ObservableProperty]
    private string _defaultUnitOfMeasure = "hr";

    [ObservableProperty]
    private bool _showInactiveResources;

    [ObservableProperty]
    private bool _enableAiEstimation = true;

    [ObservableProperty]
    private string _aiProviderEndpoint = string.Empty;

    [ObservableProperty]
    private bool _enableBulkRateEditing = true;

    [ObservableProperty]
    private bool _enableCrewBlendedRate = true;
}
