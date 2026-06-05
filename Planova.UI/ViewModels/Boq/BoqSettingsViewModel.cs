using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Domain.Enums;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqSettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settings;

    public BoqSettingsViewModel(ISettingsService settings)
    {
        _settings = settings;
        LoadSettings();
    }

    [ObservableProperty]
    private string _defaultCurrency = "USD";

    [ObservableProperty]
    private int _quantityDecimals = 2;

    [ObservableProperty]
    private int _rateDecimals = 2;

    [ObservableProperty]
    private string _defaultDelimiter = ",";

    [ObservableProperty]
    private string _defaultCodeColumn = "Code";

    [ObservableProperty]
    private string _defaultDescriptionColumn = "Description";

    [ObservableProperty]
    private string _defaultUnitColumn = "Unit";

    [ObservableProperty]
    private string _defaultQuantityColumn = "Quantity";

    [ObservableProperty]
    private string _defaultRateColumn = "Rate";

    [ObservableProperty]
    private bool _autoValidateOnLoad;

    [ObservableProperty]
    private bool _expandTreeOnLoad = true;

    [ObservableProperty]
    private ReportType _defaultReportType = ReportType.Summary;

    [ObservableProperty]
    private ReportFormat _defaultReportFormat = ReportFormat.Pdf;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public List<ReportType> ReportTypeOptions { get; } = [ReportType.Summary, ReportType.Itemized];
    public List<ReportFormat> ReportFormatOptions { get; } = [ReportFormat.Pdf, ReportFormat.Excel];
    public List<string> DecimalOptions { get; } = ["0", "1", "2", "3", "4", "5", "6"];
    public List<string> DelimiterOptions { get; } = [",", ";", "\t", "|"];

    private const string Prefix = "Boq_";

    private void LoadSettings()
    {
        DefaultCurrency = _settings.Get<string>(Prefix + nameof(DefaultCurrency)) ?? "USD";
        QuantityDecimals = _settings.Get<int?>(Prefix + nameof(QuantityDecimals)) ?? 2;
        RateDecimals = _settings.Get<int?>(Prefix + nameof(RateDecimals)) ?? 2;
        DefaultDelimiter = _settings.Get<string>(Prefix + nameof(DefaultDelimiter)) ?? ",";
        DefaultCodeColumn = _settings.Get<string>(Prefix + nameof(DefaultCodeColumn)) ?? "Code";
        DefaultDescriptionColumn = _settings.Get<string>(Prefix + nameof(DefaultDescriptionColumn)) ?? "Description";
        DefaultUnitColumn = _settings.Get<string>(Prefix + nameof(DefaultUnitColumn)) ?? "Unit";
        DefaultQuantityColumn = _settings.Get<string>(Prefix + nameof(DefaultQuantityColumn)) ?? "Quantity";
        DefaultRateColumn = _settings.Get<string>(Prefix + nameof(DefaultRateColumn)) ?? "Rate";
        AutoValidateOnLoad = _settings.Get<bool?>(Prefix + nameof(AutoValidateOnLoad)) ?? false;
        ExpandTreeOnLoad = _settings.Get<bool?>(Prefix + nameof(ExpandTreeOnLoad)) ?? true;
        DefaultReportType = _settings.Get<string>(Prefix + nameof(DefaultReportType)) == "Itemized" ? ReportType.Itemized : ReportType.Summary;
        DefaultReportFormat = _settings.Get<string>(Prefix + nameof(DefaultReportFormat)) == "Excel" ? ReportFormat.Excel : ReportFormat.Pdf;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            _settings.Set(Prefix + nameof(DefaultCurrency), DefaultCurrency);
            _settings.Set(Prefix + nameof(QuantityDecimals), QuantityDecimals);
            _settings.Set(Prefix + nameof(RateDecimals), RateDecimals);
            _settings.Set(Prefix + nameof(DefaultDelimiter), DefaultDelimiter);
            _settings.Set(Prefix + nameof(DefaultCodeColumn), DefaultCodeColumn);
            _settings.Set(Prefix + nameof(DefaultDescriptionColumn), DefaultDescriptionColumn);
            _settings.Set(Prefix + nameof(DefaultUnitColumn), DefaultUnitColumn);
            _settings.Set(Prefix + nameof(DefaultQuantityColumn), DefaultQuantityColumn);
            _settings.Set(Prefix + nameof(DefaultRateColumn), DefaultRateColumn);
            _settings.Set(Prefix + nameof(AutoValidateOnLoad), AutoValidateOnLoad);
            _settings.Set(Prefix + nameof(ExpandTreeOnLoad), ExpandTreeOnLoad);
            _settings.Set(Prefix + nameof(DefaultReportType), DefaultReportType.ToString());
            _settings.Set(Prefix + nameof(DefaultReportFormat), DefaultReportFormat.ToString());

            await _settings.Save();
            StatusMessage = "Settings saved";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        DefaultCurrency = "USD";
        QuantityDecimals = 2;
        RateDecimals = 2;
        DefaultDelimiter = ",";
        DefaultCodeColumn = "Code";
        DefaultDescriptionColumn = "Description";
        DefaultUnitColumn = "Unit";
        DefaultQuantityColumn = "Quantity";
        DefaultRateColumn = "Rate";
        AutoValidateOnLoad = false;
        ExpandTreeOnLoad = true;
        DefaultReportType = ReportType.Summary;
        DefaultReportFormat = ReportFormat.Pdf;
        StatusMessage = "Defaults restored (save to persist)";
    }
}
