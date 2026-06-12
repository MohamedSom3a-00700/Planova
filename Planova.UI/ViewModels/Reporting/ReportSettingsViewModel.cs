using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Services;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Reporting;

public partial class ReportSettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;
    private readonly IContractorService _contractorService;
    private readonly ISubcontractorService _subcontractorService;

    public Action<string>? OnStatusMessage { get; set; }

    // Preview items reflecting current header order
    public ObservableCollection<HeaderPreviewItem> HeaderPreviewItems { get; } = [];

    // Lookup: party name → logo file path (populated from service data)
    private readonly Dictionary<string, string> _clientLogoLookup = new();
    private readonly Dictionary<string, string> _contractorLogoLookup = new();
    private readonly Dictionary<string, string> _subcontractorLogoLookup = new();

    public ReportSettingsViewModel(
        ISettingsService settings,
        ICurrentProjectService currentProjectService,
        IProjectService projectService,
        IClientService clientService,
        IContractorService contractorService,
        ISubcontractorService subcontractorService)
    {
        _settings = settings;
        _currentProjectService = currentProjectService;
        _projectService = projectService;
        _clientService = clientService;
        _contractorService = contractorService;
        _subcontractorService = subcontractorService;
        PropertyChanged += (_, e) => RebuildPreview(e.PropertyName);
        LoadSettings();
        LoadProjectData();
    }

    private const string Prefix = "Reporting_";

    // ── General Settings ──────────────────────────────────────────────

    [ObservableProperty]
    private string _defaultExportFormat = "Excel";

    [ObservableProperty]
    private string _defaultReportStatus = "Draft";

    [ObservableProperty]
    private string _reportsFolderPath = string.Empty;

    [ObservableProperty]
    private bool _includeAiNarrative = true;

    [ObservableProperty]
    private bool _autoSaveReports = true;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private bool _enableWatermark;

    [ObservableProperty]
    private string _watermarkPosition = "BottomRight";

    // ── Header: visibility toggles ─────────────────────────────────────

    [ObservableProperty]
    private bool _showClientInReport = true;

    [ObservableProperty]
    private bool _showContractorInReport = true;

    [ObservableProperty]
    private bool _showSubcontractorInReport;

    [ObservableProperty]
    private bool _showConsultantInReport;

    // ── Header: Client ────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderClientLogoSource))]
    private string _headerClientName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderClientLogoSource))]
    private string _headerClientLogoPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _headerClientOptions = [];

    // ── Header: Main Contractor ────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderContractorLogoSource))]
    private string _headerContractorName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderContractorLogoSource))]
    private string _headerContractorLogoPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _headerContractorOptions = [];

    // ── Header: Sub Contractor ────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderSubcontractorLogoSource))]
    private string _headerSubcontractorName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderSubcontractorLogoSource))]
    private string _headerSubcontractorLogoPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _headerSubcontractorOptions = [];

    // ── Header: Consultant ─────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderConsultantLogoSource))]
    private string _headerConsultantName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderConsultantLogoSource))]
    private string _headerConsultantLogoPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _headerConsultantOptions = [];

    // ── Header column order (saved as comma-separated party types) ───

    [ObservableProperty]
    private string _headerOrder = "Client,Contractor,SubContractor,Consultant";

    // ── Footer Settings ────────────────────────────────────────────────

    [ObservableProperty]
    private bool _showFooter = true;

    [ObservableProperty]
    private string _footerText = "Confidential";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FooterLogoSource))]
    private string _footerLogoPath = string.Empty;

    // ── Status ─────────────────────────────────────────────────────────

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // ── Static options ─────────────────────────────────────────────────

    public List<string> ExportFormatOptions { get; } = ["Excel", "Pdf", "Word"];

    public List<string> ReportStatusOptions { get; } = ["Draft", "Final"];

    public List<string> WatermarkPositionOptions { get; } = ["TopLeft", "TopRight", "Center", "BottomLeft", "BottomRight"];

    // ── Logo image sources (consumed by XAML Image) ────────────────────

    public System.Windows.Media.ImageSource? HeaderClientLogoSource =>
        LogoPathToSource(HeaderClientLogoPath);

    public System.Windows.Media.ImageSource? HeaderContractorLogoSource =>
        LogoPathToSource(HeaderContractorLogoPath);

    public System.Windows.Media.ImageSource? HeaderSubcontractorLogoSource =>
        LogoPathToSource(HeaderSubcontractorLogoPath);

    public System.Windows.Media.ImageSource? HeaderConsultantLogoSource =>
        LogoPathToSource(HeaderConsultantLogoPath);

    public System.Windows.Media.ImageSource? FooterLogoSource =>
        LogoPathToSource(FooterLogoPath);

    // ── Preview update ─────────────────────────────────────────────────

    private static readonly HashSet<string> _previewAffectingProps =
    [
        nameof(ShowClientInReport), nameof(ShowContractorInReport),
        nameof(ShowSubcontractorInReport), nameof(ShowConsultantInReport),
        nameof(HeaderClientName), nameof(HeaderContractorName),
        nameof(HeaderSubcontractorName), nameof(HeaderConsultantName),
        nameof(HeaderClientLogoPath), nameof(HeaderContractorLogoPath),
        nameof(HeaderSubcontractorLogoPath), nameof(HeaderConsultantLogoPath),
        nameof(HeaderOrder)
    ];

    private void RebuildPreview(string? propertyName)
    {
        if (propertyName != null && !_previewAffectingProps.Contains(propertyName))
            return;

        var order = HeaderOrder.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        HeaderPreviewItems.Clear();
        foreach (var party in order)
        {
            var (name, logoPath, show) = party switch
            {
                "Client" => (HeaderClientName, HeaderClientLogoPath, ShowClientInReport),
                "Contractor" => (HeaderContractorName, HeaderContractorLogoPath, ShowContractorInReport),
                "SubContractor" => (HeaderSubcontractorName, HeaderSubcontractorLogoPath, ShowSubcontractorInReport),
                "Consultant" => (HeaderConsultantName, HeaderConsultantLogoPath, ShowConsultantInReport),
                _ => (string.Empty, string.Empty, false)
            };
            HeaderPreviewItems.Add(new HeaderPreviewItem
            {
                Name = name,
                LogoPath = logoPath,
                ShowInReport = show
            });
        }
    }

    // ── Auto-load logo when party name is selected ─────────────────────

    partial void OnHeaderClientNameChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && _clientLogoLookup.TryGetValue(value, out var logo))
            HeaderClientLogoPath = logo;
    }

    partial void OnHeaderContractorNameChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && _contractorLogoLookup.TryGetValue(value, out var logo))
            HeaderContractorLogoPath = logo;
    }

    partial void OnHeaderSubcontractorNameChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && _subcontractorLogoLookup.TryGetValue(value, out var logo))
            HeaderSubcontractorLogoPath = logo;
    }

    partial void OnHeaderConsultantNameChanged(string value)
    {
        // Consultant lookup not implemented yet — rely on manual path
    }

    private static System.Windows.Media.ImageSource? LogoPathToSource(string? path)
    {
        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            return null;
        var img = new System.Windows.Media.Imaging.BitmapImage();
        img.BeginInit();
        img.UriSource = new Uri(path, UriKind.Absolute);
        img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
        img.EndInit();
        return img;
    }

    // ── Persistence ────────────────────────────────────────────────────

    private void LoadSettings()
    {
        DefaultExportFormat = _settings.Get<string>(Prefix + nameof(DefaultExportFormat)) ?? "Excel";
        ReportsFolderPath = _settings.Get<string>(Prefix + nameof(ReportsFolderPath)) ?? string.Empty;
        IncludeAiNarrative = _settings.Get<bool?>(Prefix + nameof(IncludeAiNarrative)) ?? true;
        AutoSaveReports = _settings.Get<bool?>(Prefix + nameof(AutoSaveReports)) ?? true;
        DefaultReportStatus = _settings.Get<string>(Prefix + nameof(DefaultReportStatus)) ?? "Draft";
        CompanyName = _settings.Get<string>(Prefix + nameof(CompanyName)) ?? string.Empty;

        ShowClientInReport = _settings.Get<bool?>(Prefix + nameof(ShowClientInReport)) ?? true;
        ShowContractorInReport = _settings.Get<bool?>(Prefix + nameof(ShowContractorInReport)) ?? true;
        ShowSubcontractorInReport = _settings.Get<bool?>(Prefix + nameof(ShowSubcontractorInReport)) ?? false;
        ShowConsultantInReport = _settings.Get<bool?>(Prefix + nameof(ShowConsultantInReport)) ?? false;

        HeaderClientName = _settings.Get<string>(Prefix + nameof(HeaderClientName)) ?? string.Empty;
        HeaderClientLogoPath = _settings.Get<string>(Prefix + nameof(HeaderClientLogoPath)) ?? string.Empty;
        HeaderContractorName = _settings.Get<string>(Prefix + nameof(HeaderContractorName)) ?? string.Empty;
        HeaderContractorLogoPath = _settings.Get<string>(Prefix + nameof(HeaderContractorLogoPath)) ?? string.Empty;
        HeaderSubcontractorName = _settings.Get<string>(Prefix + nameof(HeaderSubcontractorName)) ?? string.Empty;
        HeaderSubcontractorLogoPath = _settings.Get<string>(Prefix + nameof(HeaderSubcontractorLogoPath)) ?? string.Empty;
        HeaderConsultantName = _settings.Get<string>(Prefix + nameof(HeaderConsultantName)) ?? string.Empty;
        HeaderConsultantLogoPath = _settings.Get<string>(Prefix + nameof(HeaderConsultantLogoPath)) ?? string.Empty;

        HeaderOrder = _settings.Get<string>(Prefix + nameof(HeaderOrder)) ?? "Client,Contractor,SubContractor,Consultant";

        EnableWatermark = _settings.Get<bool?>(Prefix + nameof(EnableWatermark)) ?? false;
        WatermarkPosition = _settings.Get<string>(Prefix + nameof(WatermarkPosition)) ?? "BottomRight";

        ShowFooter = _settings.Get<bool?>(Prefix + nameof(ShowFooter)) ?? true;
        FooterText = _settings.Get<string>(Prefix + nameof(FooterText)) ?? "Confidential";
        FooterLogoPath = _settings.Get<string>(Prefix + nameof(FooterLogoPath)) ?? string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            _settings.Set(Prefix + nameof(DefaultExportFormat), DefaultExportFormat);
            _settings.Set(Prefix + nameof(ReportsFolderPath), ReportsFolderPath);
            _settings.Set(Prefix + nameof(IncludeAiNarrative), IncludeAiNarrative);
            _settings.Set(Prefix + nameof(AutoSaveReports), AutoSaveReports);
            _settings.Set(Prefix + nameof(DefaultReportStatus), DefaultReportStatus);
            _settings.Set(Prefix + nameof(CompanyName), CompanyName);

            _settings.Set(Prefix + nameof(ShowClientInReport), ShowClientInReport);
            _settings.Set(Prefix + nameof(ShowContractorInReport), ShowContractorInReport);
            _settings.Set(Prefix + nameof(ShowSubcontractorInReport), ShowSubcontractorInReport);
            _settings.Set(Prefix + nameof(ShowConsultantInReport), ShowConsultantInReport);

            _settings.Set(Prefix + nameof(HeaderClientName), HeaderClientName);
            _settings.Set(Prefix + nameof(HeaderClientLogoPath), HeaderClientLogoPath);
            _settings.Set(Prefix + nameof(HeaderContractorName), HeaderContractorName);
            _settings.Set(Prefix + nameof(HeaderContractorLogoPath), HeaderContractorLogoPath);
            _settings.Set(Prefix + nameof(HeaderSubcontractorName), HeaderSubcontractorName);
            _settings.Set(Prefix + nameof(HeaderSubcontractorLogoPath), HeaderSubcontractorLogoPath);
            _settings.Set(Prefix + nameof(HeaderConsultantName), HeaderConsultantName);
            _settings.Set(Prefix + nameof(HeaderConsultantLogoPath), HeaderConsultantLogoPath);

            _settings.Set(Prefix + nameof(HeaderOrder), HeaderOrder);

            _settings.Set(Prefix + nameof(EnableWatermark), EnableWatermark);
            _settings.Set(Prefix + nameof(WatermarkPosition), WatermarkPosition);

            _settings.Set(Prefix + nameof(ShowFooter), ShowFooter);
            _settings.Set(Prefix + nameof(FooterText), FooterText);
            _settings.Set(Prefix + nameof(FooterLogoPath), FooterLogoPath);

            await _settings.Save();
            OnStatusMessage?.Invoke("Settings saved");
        }
        catch (Exception ex)
        {
            OnStatusMessage?.Invoke($"Save error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        DefaultExportFormat = "Excel";
        ReportsFolderPath = string.Empty;
        IncludeAiNarrative = true;
        AutoSaveReports = true;
        DefaultReportStatus = "Draft";
        CompanyName = string.Empty;

        ShowClientInReport = true;
        ShowContractorInReport = true;
        ShowSubcontractorInReport = false;
        ShowConsultantInReport = false;

        HeaderClientName = string.Empty;
        HeaderClientLogoPath = string.Empty;
        HeaderContractorName = string.Empty;
        HeaderContractorLogoPath = string.Empty;
        HeaderSubcontractorName = string.Empty;
        HeaderSubcontractorLogoPath = string.Empty;
        HeaderConsultantName = string.Empty;
        HeaderConsultantLogoPath = string.Empty;

        ShowFooter = true;
        FooterText = "Confidential";
        FooterLogoPath = string.Empty;

        EnableWatermark = false;
        WatermarkPosition = "BottomRight";
        OnStatusMessage?.Invoke("Defaults restored (save to persist)");
    }

    // ── Project data loading ───────────────────────────────────────────

    [RelayCommand]
    private void ReloadFromProject()
    {
        LoadProjectData();
        OnStatusMessage?.Invoke("Loaded project parties");
    }

    private async void LoadProjectData()
    {
        try
        {
            var project = _currentProjectService.CurrentProject;
            if (project == null) return;

            var projectDetail = await _projectService.GetByIdAsync(project.Id);
            if (projectDetail == null) return;

            if (string.IsNullOrEmpty(CompanyName))
                CompanyName = projectDetail.Name;

            var clientOptions = new List<string> { string.Empty };
            var contractorOptions = new List<string> { string.Empty };
            var subcontractorOptions = new List<string> { string.Empty };
            var consultantOptions = new List<string> { string.Empty };

            if (!string.IsNullOrEmpty(projectDetail.ClientName))
                clientOptions.Add(projectDetail.ClientName);
            if (!string.IsNullOrEmpty(projectDetail.ContractorName))
                contractorOptions.Add(projectDetail.ContractorName);
            if (!string.IsNullOrEmpty(projectDetail.SubcontractorName))
                subcontractorOptions.Add(projectDetail.SubcontractorName);

            _clientLogoLookup.Clear();
            _contractorLogoLookup.Clear();
            _subcontractorLogoLookup.Clear();

            try
            {
                var clients = await _clientService.GetAllAsync();
                foreach (var c in clients)
                {
                    if (!clientOptions.Contains(c.Name))
                        clientOptions.Add(c.Name);
                    // Fetch detail to get logo path
                    try
                    {
                        var detail = await _clientService.GetByIdAsync(c.Id);
                        if (detail != null && !string.IsNullOrEmpty(detail.Logo))
                            _clientLogoLookup[detail.Name] = detail.Logo;
                    }
                    catch { }
                }

                var contractors = await _contractorService.GetAllAsync();
                foreach (var c in contractors)
                {
                    if (!contractorOptions.Contains(c.Name))
                        contractorOptions.Add(c.Name);
                    try
                    {
                        var detail = await _contractorService.GetByIdAsync(c.Id);
                        if (detail != null && !string.IsNullOrEmpty(detail.Logo))
                            _contractorLogoLookup[detail.Name] = detail.Logo;
                    }
                    catch { }
                }

                var subcontractors = await _subcontractorService.GetAllAsync();
                foreach (var s in subcontractors)
                {
                    if (!subcontractorOptions.Contains(s.Name))
                        subcontractorOptions.Add(s.Name);
                    try
                    {
                        var detail = await _subcontractorService.GetByIdAsync(s.Id);
                        if (detail != null && !string.IsNullOrEmpty(detail.Logo))
                            _subcontractorLogoLookup[detail.Name] = detail.Logo;
                    }
                    catch { }
                }
            }
            catch
            {
            }

            HeaderClientOptions = new ObservableCollection<string>(clientOptions);
            HeaderContractorOptions = new ObservableCollection<string>(contractorOptions);
            HeaderSubcontractorOptions = new ObservableCollection<string>(subcontractorOptions);
            HeaderConsultantOptions = new ObservableCollection<string>(consultantOptions);

            // Auto-populate logos if a name was already set
            if (!string.IsNullOrEmpty(HeaderClientName) && _clientLogoLookup.TryGetValue(HeaderClientName, out var cl))
                HeaderClientLogoPath = cl;
            if (!string.IsNullOrEmpty(HeaderContractorName) && _contractorLogoLookup.TryGetValue(HeaderContractorName, out var co))
                HeaderContractorLogoPath = co;
            if (!string.IsNullOrEmpty(HeaderSubcontractorName) && _subcontractorLogoLookup.TryGetValue(HeaderSubcontractorName, out var sc))
                HeaderSubcontractorLogoPath = sc;
        }
        catch
        {
        }
    }

    // ── Folder browsing ────────────────────────────────────────────────

    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Reports Folder",
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Select"
        };
        if (dialog.ShowDialog() == true)
        {
            var folder = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(folder))
                ReportsFolderPath = folder;
            OnStatusMessage?.Invoke("Reports folder selected");
        }
    }
}

public class HeaderPreviewItem
{
    public string Name { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;
    public bool ShowInReport { get; set; }
    public System.Windows.Media.ImageSource? LogoSource => LogoPathToSource(LogoPath);

    private static System.Windows.Media.ImageSource? LogoPathToSource(string? path)
    {
        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            return null;
        var img = new System.Windows.Media.Imaging.BitmapImage();
        img.BeginInit();
        img.UriSource = new Uri(path, UriKind.Absolute);
        img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
        img.EndInit();
        return img;
    }
}
