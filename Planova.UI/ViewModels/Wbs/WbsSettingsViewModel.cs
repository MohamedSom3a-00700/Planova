using CommunityToolkit.Mvvm.ComponentModel;
using Planova.Wbs.Domain.Enums;

namespace Planova.UI.ViewModels.Wbs;

public sealed partial class WbsSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _defaultWbsLevel = "ControlAccount";

    [ObservableProperty]
    private bool _autoGenerateCodes = true;

    [ObservableProperty]
    private bool _redistributeWeightsOnSave = true;

    [ObservableProperty]
    private bool _enableBulkEdit = true;

    [ObservableProperty]
    private bool _showWeightColumn = true;

    [ObservableProperty]
    private bool _usePrimaveraColorsByDefault;

    [ObservableProperty]
    private int _defaultDurationDays = 30;

    [ObservableProperty]
    private decimal _defaultWeight = 10;

    [ObservableProperty]
    private bool _enableAiAssistedEditing = true;

    public WbsSettingsViewModel()
    {
    }
}
