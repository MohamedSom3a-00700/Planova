using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Dashboard;

public partial class QrCodeCardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string? _qrCodePath;

    [ObservableProperty]
    private string? _googleMapsLink;

    [ObservableProperty]
    private double? _latitude;

    [ObservableProperty]
    private double? _longitude;

    [ObservableProperty]
    private bool _hasData;
}
