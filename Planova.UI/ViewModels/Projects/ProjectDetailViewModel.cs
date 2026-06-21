using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Planova.Application.Dto;
using Planova.Application.Interfaces;
using Planova.Application.Services;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Interfaces;
using Planova.UI.ViewModels.Dashboard;

namespace Planova.UI.ViewModels.Projects;

using Planova.UI.ViewModels.Dashboard;
using Planova.UI.Services;

public sealed class DocumentCategoryGroup
{
    public string Category { get; init; } = string.Empty;
    public ObservableCollection<ProjectDocumentDto> Documents { get; init; } = new();
    public int Count => Documents.Count;
}

public sealed partial class ProjectDetailViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IProjectFolderService _folderService;
    private readonly IProjectDocumentService _documentService;
    private readonly IProjectPartyService _partyService;
    private readonly QrCodeService _qrCodeService;

    private int? _currentProjectId;

    public event Action<string>? StatusMessage;

    public ProjectDetailViewModel(
        IProjectService projectService,
        IProjectFolderService folderService,
        IProjectDocumentService documentService,
        IProjectPartyService partyService,
        QrCodeService qrCodeService)
    {
        _projectService = projectService;
        _folderService = folderService;
        _documentService = documentService;
        _partyService = partyService;
        _qrCodeService = qrCodeService;
    }

    public System.Windows.Input.ICommand? EditProjectCommand { get; set; }
    public System.Windows.Input.ICommand? DeleteProjectCommand { get; set; }

    public CostCardViewModel CostCard { get; } = new();
    public HealthCardViewModel HealthCard { get; } = new();
    public QrCodeCardViewModel QrCodeCard { get; } = new();
    public ObservableCollection<ProjectPartyDto> ProjectParties { get; } = new();
    public ObservableCollection<ProjectDocumentDto> Documents { get; } = new();
    public ObservableCollection<DocumentCategoryGroup> DocumentGroups { get; } = new();
    public ObservableCollection<ProjectPartyDto> AllParties { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ProjectDetailDto? _project;

    public bool HasLayoutFolder => Project?.DocumentsFolder is not null;

    partial void OnProjectChanged(ProjectDetailDto? value)
    {
        OnPropertyChanged(nameof(HasLayoutFolder));
    }

    [ObservableProperty]
    private int _selectedLayoutIndex;

    public string LayoutPreviewTitle => SelectedLayoutIndex switch
    {
        0 => "Default Layout",
        1 => "Images Layout",
        2 => "DWG Layout",
        3 => "PDF Layout",
        _ => "Default Layout"
    };

    public string LayoutPreviewDescription => SelectedLayoutIndex switch
    {
        0 => "Standard document list with thumbnails",
        1 => "Image gallery view for photos and renders",
        2 => "CAD drawing viewer for .dwg files",
        3 => "PDF document viewer for reports and plans",
        _ => "Standard document list with thumbnails"
    };

    partial void OnSelectedLayoutIndexChanged(int value)
    {
        OnPropertyChanged(nameof(LayoutPreviewTitle));
        OnPropertyChanged(nameof(LayoutPreviewDescription));
    }

    [ObservableProperty]
    private ProjectPartyDto? _selectedAssignParty;

    [RelayCommand]
    private async Task LoadAsync(int projectId, CancellationToken ct)
    {
        _currentProjectId = projectId;
        IsLoading = true;
        HasError = false;

        try
        {
            Project = await _projectService.GetByIdAsync(projectId, ct);

            if (Project is not null)
            {
                CostCard.ProjectName = Project.Name;
                CostCard.OriginalBudget = null;
                CostCard.CurrentBudget = null;
                CostCard.ActualCost = null;
                CostCard.EarnedValue = null;
                CostCard.Cpi = null;
                CostCard.Spi = null;
                CostCard.HasData = false;

                HealthCard.ProjectName = Project.Name;
                HealthCard.ScheduleHealthPct = 0;
                HealthCard.CostHealthPct = 0;
                HealthCard.RiskLevel = "Unknown";
                HealthCard.CriticalActivityCount = 0;
                HealthCard.HealthIndicator = "Green";
                HealthCard.HasData = false;

                QrCodeCard.ProjectName = Project.Name;
                QrCodeCard.QrCodePath = Project.QrCodePath;
                QrCodeCard.GoogleMapsLink = Project.GoogleMapsLink;
                QrCodeCard.Latitude = Project.Latitude;
                QrCodeCard.Longitude = Project.Longitude;

                if (!string.IsNullOrEmpty(Project.GoogleMapsLink)
                    && (string.IsNullOrEmpty(Project.QrCodePath) || !File.Exists(Project.QrCodePath)))
                {
                    var qrPath = _qrCodeService.GenerateQrFromUrl(Project.Id, Project.GoogleMapsLink);
                    QrCodeCard.QrCodePath = qrPath;
                    QrCodeCard.HasData = true;

                    double? lat = null;
                    double? lng = null;
                    if (QrCodeService.TryParseGoogleMapsLink(Project.GoogleMapsLink, out var parsedLat, out var parsedLng))
                    {
                        lat = parsedLat;
                        lng = parsedLng;
                        QrCodeCard.Latitude = lat;
                        QrCodeCard.Longitude = lng;
                    }

                    var updateDto = new UpdateProjectDto(
                        Project.Code, Project.Name, Project.Description,
                        Project.StartDate, Project.FinishDate, Project.Currency,
                        Project.Location, Project.ClientId, Project.ContractorId,
                        Project.SubcontractorId, Project.ConsultantId, Project.Notes,
                        null, null, Project.DocumentsFolder, lat ?? Project.Latitude, lng ?? Project.Longitude,
                        Project.GoogleMapsLink, qrPath);
                    await _projectService.UpdateAsync(Project.Id, updateDto);
                }
                else if (Project.Latitude.HasValue && Project.Longitude.HasValue
                    && string.IsNullOrEmpty(Project.GoogleMapsLink))
                {
                    var latStr = Project.Latitude.Value.ToString(CultureInfo.InvariantCulture);
                    var lngStr = Project.Longitude.Value.ToString(CultureInfo.InvariantCulture);
                    var mapsUrl = $"https://www.google.com/maps?q={latStr},{lngStr}";
                    var qrPath = _qrCodeService.GenerateQrFromUrl(Project.Id, mapsUrl);
                    QrCodeCard.QrCodePath = qrPath;
                    QrCodeCard.GoogleMapsLink = mapsUrl;
                    QrCodeCard.HasData = true;

                    var updateDto = new UpdateProjectDto(
                        Project.Code, Project.Name, Project.Description,
                        Project.StartDate, Project.FinishDate, Project.Currency,
                        Project.Location, Project.ClientId, Project.ContractorId,
                        Project.SubcontractorId, Project.ConsultantId, Project.Notes,
                        null, null, Project.DocumentsFolder, Project.Latitude, Project.Longitude,
                        mapsUrl, qrPath);
                    await _projectService.UpdateAsync(Project.Id, updateDto);
                }
                else
                {
                    QrCodeCard.HasData = !string.IsNullOrEmpty(Project.QrCodePath) && File.Exists(Project.QrCodePath);
                }

                await LoadPartiesAsync(projectId, ct);
                await LoadDocumentsAsync(projectId, ct);
                await LoadAllPartiesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load project details: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAllPartiesAsync(CancellationToken ct)
    {
        try
        {
            var all = await _partyService.GetAllPartiesAsync(ct);
            var assigned = _currentProjectId.HasValue
                ? await _partyService.GetPartiesAsync(_currentProjectId.Value, ct)
                : new List<ProjectPartyDto>();
            var assignedIds = assigned.Select(p => p.Id).ToHashSet();
            AllParties.Clear();
            foreach (var p in all.OrderBy(p => p.Role))
            {
                if (!assignedIds.Contains(p.Id))
                    AllParties.Add(p);
            }
        }
        catch
        {
        }
    }

    private async Task LoadPartiesAsync(int projectId, CancellationToken ct)
    {
        try
        {
            var parties = await _partyService.GetPartiesAsync(projectId, ct);
            ProjectParties.Clear();
            foreach (var p in parties)
            {
                ProjectParties.Add(p);
            }
        }
        catch
        {
        }
    }

    private async Task LoadDocumentsAsync(int projectId, CancellationToken ct)
    {
        try
        {
            var docs = await _documentService.GetByProjectAsync(projectId, ct);
            Documents.Clear();
            DocumentGroups.Clear();
            foreach (var d in docs)
            {
                Documents.Add(d);
            }
            foreach (var group in docs
                         .GroupBy(d => d.DocumentType)
                         .OrderBy(g => g.Key))
            {
                var categoryGroup = new DocumentCategoryGroup
                {
                    Category = group.Key,
                };
                foreach (var doc in group.OrderBy(d => d.FileName))
                {
                    categoryGroup.Documents.Add(doc);
                }
                DocumentGroups.Add(categoryGroup);
            }
        }
        catch
        {
        }
    }

    [RelayCommand]
    private void OpenDocument(ProjectDocumentDto document)
    {
        if (document is null) return;
        if (!string.IsNullOrWhiteSpace(document.AbsolutePath) && File.Exists(document.AbsolutePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = document.AbsolutePath,
                UseShellExecute = true
            });
        }
    }

    [RelayCommand]
    private void OpenProjectFolder()
    {
        var folder = Project?.DocumentsFolder ?? Project?.Location;
        if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
        }
    }

    [RelayCommand]
    private void OpenInGoogleMaps()
    {
        if (Project is null) return;
        if (Project.Latitude is double lat && Project.Longitude is double lng)
        {
            var url = $"https://www.google.com/maps?q={lat},{lng}";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }

    [RelayCommand]
    private async Task CreateFolderStructure(CancellationToken ct)
    {
        if (Project?.DocumentsFolder is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            await _folderService.CreateFolderStructureAsync(Project.DocumentsFolder, ct);
            StatusMessage?.Invoke("Folder structure created successfully");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create folder structure: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ScanFolderAsync()
    {
        if (_currentProjectId is null || Project?.DocumentsFolder is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var dto = new ScanFolderDto(_currentProjectId.Value, Project.DocumentsFolder);
            await _documentService.ScanFolderAsync(dto);
            StatusMessage?.Invoke("Folder scanned successfully");
            await LoadDocumentsAsync(_currentProjectId.Value, CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to scan folder: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AssignPartyAsync()
    {
        if (_currentProjectId is null || SelectedAssignParty is null) return;

        var partyName = SelectedAssignParty.Name;

        IsLoading = true;
        HasError = false;

        try
        {
            var existing = await _partyService.GetPartiesAsync(_currentProjectId.Value);
            var hasRole = existing.Any(p => p.Role == SelectedAssignParty.Role);
            if (hasRole)
            {
                ErrorMessage = $"This project already has a {SelectedAssignParty.Role}. Only one {SelectedAssignParty.Role} is allowed.";
                HasError = true;
                return;
            }

            var request = new SavePartyRequest(
                null,
                SelectedAssignParty.Role,
                SelectedAssignParty.Name,
                SelectedAssignParty.Address,
                SelectedAssignParty.ContactPerson,
                SelectedAssignParty.ContactEmail,
                SelectedAssignParty.ContactPhone,
                SelectedAssignParty.DisplayOrder);

            await _partyService.SavePartyAsync(_currentProjectId.Value, request);
            await LoadPartiesAsync(_currentProjectId.Value, CancellationToken.None);
            await LoadAllPartiesAsync(CancellationToken.None);
            SelectedAssignParty = null;
            StatusMessage?.Invoke($"Party '{partyName}' assigned to project");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to assign party: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BrowseLayoutFolderAsync()
    {
        var dialog = new OpenFileDialog
        {
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Select Folder"
        };

        if (dialog.ShowDialog() == true)
        {
            var folder = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(folder) && Project is not null)
            {
                try
                {
                    var dto = new UpdateProjectDto(
                        Project.Code, Project.Name, Project.Description,
                        Project.StartDate, Project.FinishDate,
                        Project.Currency, Project.Location,
                        Project.ClientId, Project.ContractorId, Project.SubcontractorId, Project.ConsultantId,
                        Project.Notes,
                        DocumentsFolder: folder);

                    await _projectService.UpdateAsync(Project.Id, dto);
                    Project = Project with { DocumentsFolder = folder };
                    OnPropertyChanged(nameof(HasLayoutFolder));
                    StatusMessage?.Invoke($"Documents folder set to: {folder}");
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Failed to set documents folder: {ex.Message}";
                    HasError = true;
                }
            }
        }
    }

    [RelayCommand]
    private async Task RemovePartyAsync(ProjectPartyDto? party)
    {
        if (_currentProjectId is null || party is null) return;

        try
        {
            await _partyService.DeletePartyAsync(party.Id);
            ProjectParties.Remove(party);
            AllParties.Add(party);
            StatusMessage?.Invoke($"Party '{party.Name}' removed from project");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to remove party: {ex.Message}";
            HasError = true;
        }
    }
}
