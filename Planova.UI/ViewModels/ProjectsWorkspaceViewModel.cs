using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;
using Planova.UI.Services;

namespace Planova.UI.ViewModels;

public partial class ProjectsWorkspaceViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;
    private readonly IContractorService _contractorService;
    private readonly ISubcontractorService _subcontractorService;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IProjectDocumentService _projectDocumentService;
    private readonly IProjectPartyService _projectPartyService;
    private readonly QrCodeService _qrCodeService;
    private readonly MapHtmlService _mapHtmlService;
    private List<ProjectSummaryDto> _allProjects = new();

    public ProjectsWorkspaceViewModel(
        IProjectService projectService,
        IClientService clientService,
        IContractorService contractorService,
        ISubcontractorService subcontractorService,
        ICurrentProjectService currentProjectService,
        IProjectDocumentService projectDocumentService,
        IProjectPartyService projectPartyService,
        QrCodeService qrCodeService,
        MapHtmlService mapHtmlService)
    {
        _projectService = projectService;
        _clientService = clientService;
        _contractorService = contractorService;
        _subcontractorService = subcontractorService;
        _currentProjectService = currentProjectService;
        _projectDocumentService = projectDocumentService;
        _projectPartyService = projectPartyService;
        _qrCodeService = qrCodeService;
        _mapHtmlService = mapHtmlService;
    }

    public ObservableCollection<ProjectSummaryDto> Projects { get; } = new();
    public ObservableCollection<ClientSummaryDto> Clients { get; } = new();
    public ObservableCollection<ContractorSummaryDto> Contractors { get; } = new();
    public ObservableCollection<SubcontractorSummaryDto> Subcontractors { get; } = new();
    public ObservableCollection<string> Currencies { get; } = new()
    {
        "USD", "EUR", "GBP", "EGP", "SAR", "AED", "JOD", "KWD", "QAR", "OMR", "BHD", "LYD", "TND", "DZD", "MAD"
    };
    public ObservableCollection<string> StatusFilters { get; } = new()
    {
        "All", "Draft", "Under Review", "Approved", "In Progress", "On Hold", "Completed", "Cancelled"
    };
    public ObservableCollection<ProjectPartyDto> ProjectParties { get; } = new();
    public ObservableCollection<string> PartyRoles { get; } = new()
    {
        "Client", "MainContractor", "SubContractor"
    };

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedStatusFilter = "All";

    [ObservableProperty]
    private ProjectDetailDto? _selectedProject;

    [ObservableProperty]
    private ProjectSummaryDto? _selectedSummary;

    partial void OnSelectedSummaryChanged(ProjectSummaryDto? value)
    {
        if (value is not null)
            SelectProjectCommand.Execute(value);
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ApplyFilters();
    }

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isCreating;

    [ObservableProperty]
    private string _editCode = string.Empty;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string? _editDescription;

    [ObservableProperty]
    private DateTime? _editStartDate;

    [ObservableProperty]
    private DateTime? _editFinishDate;

    [ObservableProperty]
    private string? _editCurrency;

    [ObservableProperty]
    private string? _editLocation;

    [ObservableProperty]
    private int? _editClientId;

    [ObservableProperty]
    private int? _editContractorId;

    [ObservableProperty]
    private int? _editSubcontractorId;

    [ObservableProperty]
    private string? _editNotes;

    [ObservableProperty]
    private string? _editLogoSourcePath;

    [ObservableProperty]
    private string? _editDocumentsFolder;

    [ObservableProperty]
    private ProjectPartyDto? _selectedParty;

    [ObservableProperty]
    private string _editPartyName = string.Empty;

    [ObservableProperty]
    private string _editPartyRole = "Client";

    [ObservableProperty]
    private string? _editPartyAddress;

    [ObservableProperty]
    private string? _editPartyContactPerson;

    [ObservableProperty]
    private string? _editPartyContactEmail;

    [ObservableProperty]
    private string? _editPartyContactPhone;

    [ObservableProperty]
    private int _editPartyDisplayOrder;

    [ObservableProperty]
    private double? _editLatitude;

    [ObservableProperty]
    private double? _editLongitude;

    [ObservableProperty]
    private string? _logoPreviewPath;

    [ObservableProperty]
    private string? _qrCodePath;

    [ObservableProperty]
    private string? _mapHtmlPath;

    [ObservableProperty]
    private string _selectedDocumentTypeFilter = "All";

    [ObservableProperty]
    private ProjectDocumentDto? _selectedDocument;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public ObservableCollection<ProjectDocumentDto> Documents { get; } = new();
    public ObservableCollection<string> DocumentTypeFilters { get; } = new()
        { "All", "Boq", "Drawing", "Spec", "Contract", "Other" };

    private void ApplyFilters()
    {
        Projects.Clear();
        var filtered = _allProjects.AsEnumerable();

        if (SelectedStatusFilter != "All")
            filtered = filtered.Where(p => p.Status == SelectedStatusFilter);

        if (!string.IsNullOrEmpty(SearchQuery))
        {
            var query = SearchQuery.Trim().ToLowerInvariant();
            filtered = filtered.Where(p =>
                (p.Code?.ToLowerInvariant().Contains(query) == true) ||
                (p.Name?.ToLowerInvariant().Contains(query) == true) ||
                (p.ClientName?.ToLowerInvariant().Contains(query) == true) ||
                (p.ContractorName?.ToLowerInvariant().Contains(query) == true) ||
                (p.Status?.ToLowerInvariant().Contains(query) == true));
        }

        foreach (var p in filtered)
            Projects.Add(p);
    }

    partial void OnSelectedDocumentTypeFilterChanged(string value)
    {
        if (SelectedProject == null) return;
        LoadDocuments();
    }

    private void LoadDocuments()
    {
        if (SelectedProject == null) return;
        Documents.Clear();

        var docs = SelectedProject.Documents ?? new List<ProjectDocumentDto>();

        if (SelectedDocumentTypeFilter != "All")
            docs = docs.Where(d => d.DocumentType == SelectedDocumentTypeFilter).ToList();

        foreach (var d in docs)
            Documents.Add(d);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            _allProjects = (await _projectService.GetAllAsync()).ToList();

            var clients = await _clientService.GetAllAsync();
            Clients.Clear();
            foreach (var c in clients)
                Clients.Add(c);

            var contractors = await _contractorService.GetAllAsync();
            Contractors.Clear();
            foreach (var c in contractors)
                Contractors.Add(c);

            var subcontractors = await _subcontractorService.GetAllAsync();
            Subcontractors.Clear();
            foreach (var s in subcontractors)
                Subcontractors.Add(s);

            ApplyFilters();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load projects: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Search()
    {
        ApplyFilters();
    }

    [RelayCommand]
    private void FilterByStatus(string? status)
    {
        SelectedStatusFilter = status ?? "All";
    }

    [RelayCommand]
    private async Task SelectProjectAsync(ProjectSummaryDto? project)
    {
        if (project == null) return;

        IsLoading = true;
        try
        {
            SelectedProject = await _projectService.GetByIdAsync(project.Id);
            IsEditing = false;
            IsCreating = false;
            LoadDocuments();
            await LoadPartiesAsync();

        if (SelectedProject != null)
        {
            _currentProjectService.SetProject(new ProjectContext(
                SelectedProject.Id,
                SelectedProject.Code,
                SelectedProject.Name));

            EditLatitude = SelectedProject.Latitude;
            EditLongitude = SelectedProject.Longitude;

            if (SelectedProject.Latitude.HasValue && SelectedProject.Longitude.HasValue)
            {
                GenerateMapHtml(SelectedProject.Latitude.Value, SelectedProject.Longitude.Value, SelectedProject.Name);
            }
            else
            {
                MapHtmlPath = null;
            }

            QrCodePath = SelectedProject.QrCodePath;
            LogoPreviewPath = SelectedProject.LogoPath;
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

    [RelayCommand]
    private void NewProject()
    {
        IsCreating = true;
        IsEditing = true;
        SelectedProject = null;
        EditCode = string.Empty;
        EditName = string.Empty;
        EditDescription = null;
        EditStartDate = null;
        EditFinishDate = null;
        EditCurrency = null;
        EditLocation = null;
        EditClientId = null;
        EditContractorId = null;
        EditSubcontractorId = null;
        EditNotes = null;
        EditLogoSourcePath = null;
        EditDocumentsFolder = null;
        EditLatitude = null;
        EditLongitude = null;
        LogoPreviewPath = null;
        QrCodePath = null;
        MapHtmlPath = null;
        ErrorMessage = string.Empty;
        HasError = false;
    }

    [RelayCommand]
    private void EditProject()
    {
        if (SelectedProject == null) return;

        IsCreating = false;
        IsEditing = true;
        EditCode = SelectedProject.Code;
        EditName = SelectedProject.Name;
        EditDescription = SelectedProject.Description;
        EditStartDate = SelectedProject.StartDate;
        EditFinishDate = SelectedProject.FinishDate;
        EditCurrency = SelectedProject.Currency;
        EditLocation = SelectedProject.Location;
        EditClientId = SelectedProject.ClientId;
        EditContractorId = SelectedProject.ContractorId;
        EditSubcontractorId = SelectedProject.SubcontractorId;
        EditNotes = SelectedProject.Notes;
        EditLogoSourcePath = null;
        EditDocumentsFolder = SelectedProject.DocumentsFolder;
        EditLatitude = SelectedProject.Latitude;
        EditLongitude = SelectedProject.Longitude;
        LogoPreviewPath = SelectedProject.LogoPath;
        QrCodePath = SelectedProject.QrCodePath;
        ErrorMessage = string.Empty;
        HasError = false;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        IsCreating = false;
        ErrorMessage = string.Empty;
        HasError = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        HasError = false;
        IsLoading = true;

        try
        {
            if (IsCreating)
            {
                var dto = new CreateProjectDto(
                    EditCode, EditName, EditDescription,
                    EditStartDate, EditFinishDate, EditCurrency,
                    EditLocation, EditClientId, EditContractorId, EditSubcontractorId, EditNotes,
                    EditLogoSourcePath, EditDocumentsFolder, EditLatitude, EditLongitude);

                SelectedProject = await _projectService.CreateAsync(dto);
            }
            else if (SelectedProject != null)
            {
                var dto = new UpdateProjectDto(
                    EditCode, EditName, EditDescription,
                    EditStartDate, EditFinishDate, EditCurrency,
                    EditLocation, EditClientId, EditContractorId, EditSubcontractorId, EditNotes,
                    EditLogoSourcePath, EditDocumentsFolder, EditLatitude, EditLongitude, QrCodePath);

                SelectedProject = await _projectService.UpdateAsync(SelectedProject.Id, dto);
            }

            IsEditing = false;
            IsCreating = false;
            LoadDocuments();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedProject == null) return;

        IsLoading = true;
        try
        {
            await _projectService.DeleteAsync(SelectedProject.Id);
            _currentProjectService.SetProject(null);
            SelectedProject = null;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ChangeStatusAsync(string? newStatus)
    {
        if (SelectedProject == null || string.IsNullOrEmpty(newStatus)) return;

        IsLoading = true;
        try
        {
            SelectedProject = await _projectService.ChangeStatusAsync(SelectedProject.Id, newStatus);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void BrowseLogo()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
            Title = "Select Project Logo"
        };

        if (dialog.ShowDialog() == true)
        {
            EditLogoSourcePath = dialog.FileName;
            LogoPreviewPath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void BrowseDocumentsFolder()
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
            if (!string.IsNullOrEmpty(folder))
                EditDocumentsFolder = folder;
        }
    }

    [RelayCommand]
    private async Task AddDocumentsAsync()
    {
        if (SelectedProject == null) return;

        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Allowed documents (*.pdf;*.xlsx;*.xls;*.xlsm;*.dwg;*.dxf;*.doc;*.docx)|*.pdf;*.xlsx;*.xls;*.xlsm;*.dwg;*.dxf;*.doc;*.docx",
            Title = "Select documents to add"
        };

        if (dialog.ShowDialog() == true)
        {
            IsLoading = true;
            try
            {
                foreach (var filePath in dialog.FileNames)
                {
                    var docType = DetectDocumentType(filePath);
                    var dto = new AddProjectDocumentDto(SelectedProject.Id, filePath, docType, null);
                    await _projectDocumentService.AddAsync(dto);
                }

                SelectedProject = await _projectService.GetByIdAsync(SelectedProject.Id);
                LoadDocuments();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task DeleteDocumentAsync(ProjectDocumentDto? doc)
    {
        if (SelectedProject == null || doc == null) return;

        if (_projectDocumentService.IsLockedDocumentType(doc.DocumentType))
        {
            ErrorMessage = $"'{doc.DocumentType}' documents are required by studios and cannot be deleted.";
            HasError = true;
            return;
        }

        try
        {
            await _projectDocumentService.DeleteAsync(doc.Id);
            SelectedProject = await _projectService.GetByIdAsync(SelectedProject.Id);
            LoadDocuments();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task DeleteAllDocumentsAsync()
    {
        if (SelectedProject == null) return;

        try
        {
            var lockedDocs = SelectedProject.Documents?
                .Where(d => _projectDocumentService.IsLockedDocumentType(d.DocumentType))
                .Select(d => d.DocumentType)
                .Distinct()
                .ToList();

            if (lockedDocs is { Count: > 0 })
            {
                ErrorMessage = $"Cannot delete all: locked document types ({string.Join(", ", lockedDocs)}) are required by studios.";
                HasError = true;
                return;
            }

            await _projectDocumentService.DeleteByProjectAsync(SelectedProject.Id);
            SelectedProject = await _projectService.GetByIdAsync(SelectedProject.Id);
            LoadDocuments();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private void OpenDocument(ProjectDocumentDto? doc)
    {
        if (doc == null) return;
        try
        {
            Process.Start(new ProcessStartInfo(doc.AbsolutePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Cannot open file: {ex.Message}";
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task ScanFolderAsync()
    {
        if (SelectedProject == null || string.IsNullOrEmpty(EditDocumentsFolder)) return;

        IsLoading = true;
        try
        {
            var dto = new ScanFolderDto(SelectedProject.Id, EditDocumentsFolder);
            await _projectDocumentService.ScanFolderAsync(dto);
            SelectedProject = await _projectService.GetByIdAsync(SelectedProject.Id);
            LoadDocuments();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SetLocationAsync()
    {
        if (SelectedProject == null || !EditLatitude.HasValue || !EditLongitude.HasValue) return;

        IsLoading = true;
        try
        {
            var qrPath = _qrCodeService.GenerateLocationQr(SelectedProject.Id, EditLatitude.Value, EditLongitude.Value);
            QrCodePath = qrPath;

            GenerateMapHtml(EditLatitude.Value, EditLongitude.Value, SelectedProject.Name);

            var dto = new UpdateProjectDto(
                SelectedProject.Code, SelectedProject.Name, SelectedProject.Description,
                SelectedProject.StartDate, SelectedProject.FinishDate, SelectedProject.Currency,
                SelectedProject.Location, SelectedProject.ClientId, SelectedProject.ContractorId,
                SelectedProject.SubcontractorId, SelectedProject.Notes,
                null, SelectedProject.DocumentsFolder, EditLatitude, EditLongitude, QrCodePath);

            await _projectService.UpdateAsync(SelectedProject.Id, dto);
            SelectedProject = await _projectService.GetByIdAsync(SelectedProject.Id);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void GenerateMapHtml(double latitude, double longitude, string projectName)
    {
        var html = _mapHtmlService.GenerateMapHtml(latitude, longitude, projectName);
        var tempDir = Path.Combine(Path.GetTempPath(), "Planova", "Maps");
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, $"map_{Guid.NewGuid()}.html");
        File.WriteAllText(tempFile, html);
        MapHtmlPath = tempFile;
    }

    [RelayCommand]
    private void CopyQrCodePath()
    {
        if (!string.IsNullOrEmpty(QrCodePath))
        {
            try
            {
                System.Windows.Clipboard.SetText(QrCodePath);
            }
            catch { }
        }
    }

    public event Action<string?, string?>? CoordinatesFromMapClicked;

    [RelayCommand]
    private void OpenInGoogleMaps()
    {
        var lat = EditLatitude ?? SelectedProject?.Latitude;
        var lng = EditLongitude ?? SelectedProject?.Longitude;
        if (lat.HasValue && lng.HasValue)
        {
            var url = $"https://www.google.com/maps?q={lat.Value.ToString(CultureInfo.InvariantCulture)},{lng.Value.ToString(CultureInfo.InvariantCulture)}";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Cannot open browser: {ex.Message}";
                HasError = true;
            }
        }
        else
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://www.google.com/maps") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Cannot open browser: {ex.Message}";
                HasError = true;
            }
        }
    }

    [RelayCommand]
    private void GetCoordinatesFromMap()
    {
        CoordinatesFromMapClicked?.Invoke(null, null);
    }

    private static string DetectDocumentType(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var lower = fileName.ToLowerInvariant();

        if (lower.Contains("boq") || lower.Contains("bill") || lower.Contains("quantity"))
            return "Boq";
        if (lower.Contains("spec"))
            return "Spec";
        if (lower.Contains("contract") || lower.Contains("agreement"))
            return "Contract";
        if (lower.Contains("draw") || lower.Contains("dwg"))
            return "Drawing";

        return "Other";
    }

    [RelayCommand]
    private async Task LoadPartiesAsync()
    {
        if (SelectedProject == null) return;
        try
        {
            var parties = await _projectPartyService.GetPartiesAsync(SelectedProject.Id);
            ProjectParties.Clear();
            foreach (var p in parties)
                ProjectParties.Add(p);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task SyncPartiesFromProjectAsync()
    {
        if (SelectedProject == null) return;
        IsLoading = true;
        try
        {
            var ct = CancellationToken.None;
            var project = SelectedProject;

            if (!string.IsNullOrEmpty(project.ClientName))
                await _projectPartyService.SavePartyAsync(project.Id,
                    new SavePartyRequest(null, "Client", project.ClientName, null, null, null, null, 0), ct);

            if (!string.IsNullOrEmpty(project.ContractorName))
                await _projectPartyService.SavePartyAsync(project.Id,
                    new SavePartyRequest(null, "MainContractor", project.ContractorName, null, null, null, null, 1), ct);

            if (!string.IsNullOrEmpty(project.SubcontractorName))
                await _projectPartyService.SavePartyAsync(project.Id,
                    new SavePartyRequest(null, "SubContractor", project.SubcontractorName, null, null, null, null, 2), ct);

            await LoadPartiesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SavePartyAsync()
    {
        if (SelectedProject == null) return;
        if (string.IsNullOrWhiteSpace(EditPartyName)) return;

        IsLoading = true;
        try
        {
            Guid? existingId = SelectedParty?.Id;
            var request = new SavePartyRequest(
                existingId,
                EditPartyRole,
                EditPartyName.Trim(),
                EditPartyAddress,
                EditPartyContactPerson,
                EditPartyContactEmail,
                EditPartyContactPhone,
                EditPartyDisplayOrder);

            await _projectPartyService.SavePartyAsync(SelectedProject.Id, request);
            await LoadPartiesAsync();

            EditPartyName = string.Empty;
            EditPartyAddress = null;
            EditPartyContactPerson = null;
            EditPartyContactEmail = null;
            EditPartyContactPhone = null;
            EditPartyDisplayOrder = 0;
            SelectedParty = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeletePartyAsync(ProjectPartyDto? party)
    {
        if (party == null) return;

        try
        {
            await _projectPartyService.DeletePartyAsync(party.Id);
            await LoadPartiesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private void EditParty(ProjectPartyDto? party)
    {
        if (party == null) return;
        SelectedParty = party;
        EditPartyName = party.Name;
        EditPartyRole = party.Role;
        EditPartyAddress = party.Address;
        EditPartyContactPerson = party.ContactPerson;
        EditPartyContactEmail = party.ContactEmail;
        EditPartyContactPhone = party.ContactPhone;
        EditPartyDisplayOrder = party.DisplayOrder;
    }

    [RelayCommand]
    private void CancelPartyEdit()
    {
        EditPartyName = string.Empty;
        EditPartyAddress = null;
        EditPartyContactPerson = null;
        EditPartyContactEmail = null;
        EditPartyContactPhone = null;
        EditPartyDisplayOrder = 0;
        SelectedParty = null;
    }
}
