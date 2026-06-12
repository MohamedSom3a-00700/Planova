using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Services;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.UI.ViewModels.Reporting;

public partial class ReportTemplateEditorViewModel : ObservableObject
{
    private readonly IReportSettingsService _settingsService;
    private readonly IProjectPartyService _partyService;
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;
    private readonly IContractorService _contractorService;
    private readonly ISubcontractorService _subcontractorService;

    public Action<string>? OnStatusMessage { get; set; }

    public ReportTemplateEditorViewModel(
        IReportSettingsService settingsService,
        IProjectPartyService partyService,
        IProjectService projectService,
        IClientService clientService,
        IContractorService contractorService,
        ISubcontractorService subcontractorService)
    {
        _settingsService = settingsService;
        _partyService = partyService;
        _projectService = projectService;
        _clientService = clientService;
        _contractorService = contractorService;
        _subcontractorService = subcontractorService;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _selectedReportType = "Daily";

    [ObservableProperty]
    private ObservableCollection<SectionItem> _sections = [];

    [ObservableProperty]
    private ObservableCollection<ProjectPartyDto> _parties = [];

    [ObservableProperty]
    private string _newPartyName = string.Empty;

    [ObservableProperty]
    private string _newPartyRole = "Client";

    [ObservableProperty]
    private string _selectedTemplateCategory = "All";

    public static string[] ReportTypes => ["Daily", "Weekly", "Monthly", "Executive"];

    public static string[] PartyRoles => ["Client", "MainContractor", "SubContractor"];

    public static string[] TemplateCategories => ["All", "Construction", "Engineering", "Infrastructure", "General", "Custom"];

    [RelayCommand]
    private async Task LoadSettingsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var settings = await _settingsService.GetSettingsAsync(1, Enum.Parse<ReportType>(SelectedReportType), ct);
            var enabledList = string.IsNullOrEmpty(settings.EnabledSectionsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(settings.EnabledSectionsJson) ?? new List<string>();
            Sections = new ObservableCollection<SectionItem>(
                enabledList.Select(s => new SectionItem(s, true)));
            OnStatusMessage?.Invoke("Settings loaded");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load settings: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadPartiesAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var list = new List<ProjectPartyDto>();

            // Try to load parties from project data first
            try
            {
                var project = await _projectService.GetByIdAsync(1, ct);
                if (project != null)
                {
                    if (!string.IsNullOrEmpty(project.ClientName))
                        list.Add(new ProjectPartyDto(Guid.NewGuid(), 1, "Client", project.ClientName, null, null, null, null, null, 0, DateTime.UtcNow, DateTime.UtcNow));

                    if (!string.IsNullOrEmpty(project.ContractorName))
                        list.Add(new ProjectPartyDto(Guid.NewGuid(), 1, "MainContractor", project.ContractorName, null, null, null, null, null, 1, DateTime.UtcNow, DateTime.UtcNow));

                    if (!string.IsNullOrEmpty(project.SubcontractorName))
                        list.Add(new ProjectPartyDto(Guid.NewGuid(), 1, "SubContractor", project.SubcontractorName, null, null, null, null, null, 2, DateTime.UtcNow, DateTime.UtcNow));
                }
            }
            catch
            {
                // Project data unavailable, fall through to party service
            }

            // Also load stored parties from reporting module
            var stored = await _partyService.GetPartiesAsync(1, ct);
            foreach (var party in stored)
            {
                if (!list.Any(p => p.Role == party.Role && p.Name == party.Name))
                    list.Add(party);
            }

            Parties = new ObservableCollection<ProjectPartyDto>(list);
            OnStatusMessage?.Invoke($"Loaded {list.Count} party/parties");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load parties: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadFromProjectAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var project = await _projectService.GetByIdAsync(1, ct);
            if (project == null)
            {
                OnStatusMessage?.Invoke("No project found");
                return;
            }

            var added = 0;

            if (!string.IsNullOrEmpty(project.ClientName) &&
                !Parties.Any(p => p.Name == project.ClientName && p.Role == "Client"))
            {
                await _partyService.SavePartyAsync(1, new SavePartyRequest(null, "Client", project.ClientName, null, null, null, null, 0), ct);
                added++;
            }

            if (!string.IsNullOrEmpty(project.ContractorName) &&
                !Parties.Any(p => p.Name == project.ContractorName && p.Role == "MainContractor"))
            {
                await _partyService.SavePartyAsync(1, new SavePartyRequest(null, "MainContractor", project.ContractorName, null, null, null, null, 0), ct);
                added++;
            }

            if (!string.IsNullOrEmpty(project.SubcontractorName) &&
                !Parties.Any(p => p.Name == project.SubcontractorName && p.Role == "SubContractor"))
            {
                await _partyService.SavePartyAsync(1, new SavePartyRequest(null, "SubContractor", project.SubcontractorName, null, null, null, null, 0), ct);
                added++;
            }

            if (added > 0)
                OnStatusMessage?.Invoke($"Added {added} party/parties from project");
            else
                OnStatusMessage?.Invoke("All project parties already exist");

            await LoadPartiesAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load from project: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveSectionVisibilityAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var enabled = Sections.Where(s => s.IsEnabled).Select(s => s.Name).ToList();
            var json = JsonSerializer.Serialize(enabled);
            var request = new UpdateSettingsRequest(json);
            await _settingsService.UpdateSettingsAsync(1, Enum.Parse<ReportType>(SelectedReportType), request, ct);
            OnStatusMessage?.Invoke("Section visibility updated");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save settings: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddPartyAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(NewPartyName)) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var request = new SavePartyRequest(null, NewPartyRole, NewPartyName.Trim(), null, null, null, null, 0);
            await _partyService.SavePartyAsync(1, request, ct);
            NewPartyName = string.Empty;
            OnStatusMessage?.Invoke("Party added");
            await LoadPartiesAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to add party: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeletePartyAsync(ProjectPartyDto? party, CancellationToken ct)
    {
        if (party is null) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await _partyService.DeletePartyAsync(party.Id, ct);
            OnStatusMessage?.Invoke("Party deleted");
            await LoadPartiesAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete party: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class SectionItem : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private bool _isEnabled;

    public SectionItem(string name, bool isEnabled)
    {
        _name = name;
        _isEnabled = isEnabled;
    }
}
