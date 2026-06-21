using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;
using Planova.UI.Services;
using Planova.UI.ViewModels.Projects;

namespace Planova.UI.ViewModels;

public partial class ProjectsWorkspaceViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;
    private readonly IContractorService _contractorService;
    private readonly ISubcontractorService _subcontractorService;
    private readonly IConsultantService _consultantService;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IProjectDocumentService _projectDocumentService;
    private readonly IProjectPartyService _projectPartyService;
    private readonly QrCodeService _qrCodeService;
    private readonly MapHtmlService _mapHtmlService;
    private readonly IServiceProvider _serviceProvider;
    private List<ProjectSummaryDto> _allProjects = new();

    public event Action<string>? StatusMessage;

    public ProjectListViewModel ListVM { get; }
    public ProjectDetailViewModel DetailVM { get; }

    public ProjectsWorkspaceViewModel(
        IProjectService projectService,
        IClientService clientService,
        IContractorService contractorService,
        ISubcontractorService subcontractorService,
        IConsultantService consultantService,
        ICurrentProjectService currentProjectService,
        IProjectDocumentService projectDocumentService,
        IProjectPartyService projectPartyService,
        QrCodeService qrCodeService,
        MapHtmlService mapHtmlService,
        IServiceProvider serviceProvider)
    {
        _projectService = projectService;
        _clientService = clientService;
        _contractorService = contractorService;
        _subcontractorService = subcontractorService;
        _consultantService = consultantService;
        _currentProjectService = currentProjectService;
        _projectDocumentService = projectDocumentService;
        _projectPartyService = projectPartyService;
        _qrCodeService = qrCodeService;
        _mapHtmlService = mapHtmlService;
        _serviceProvider = serviceProvider;

        ListVM = _serviceProvider.GetRequiredService<ProjectListViewModel>();
        DetailVM = _serviceProvider.GetRequiredService<ProjectDetailViewModel>();

        ListVM.NewProjectCommand = NewProjectCommand;
        ListVM.EditProjectCommand = EditProjectCommand;
        ListVM.DeleteProjectCommand = DeleteCommand;
        DetailVM.EditProjectCommand = EditProjectCommand;
        DetailVM.DeleteProjectCommand = DeleteCommand;
        DetailVM.StatusMessage += msg => StatusMessage?.Invoke(msg);

        ListVM.PropertyChanged += OnListVmPropertyChanged;
        DetailVM.PropertyChanged += OnDetailVmPropertyChanged;
    }

    public ObservableCollection<ProjectSummaryDto> Projects { get; } = new();
    public ObservableCollection<PartyComboItem> Clients { get; } = new();
    public ObservableCollection<PartyComboItem> Contractors { get; } = new();
    public ObservableCollection<PartyComboItem> Subcontractors { get; } = new();
    public ObservableCollection<PartyComboItem> Consultants { get; } = new();
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
        "Client", "MainContractor", "SubContractor", "Consultant"
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

    private async void OnListVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectListViewModel.SelectedProject))
        {
            var selected = ListVM.SelectedProject;
            if (selected is not null)
            {
                DetailVM.LoadCommand.Execute(selected.Id);
            }
        }
    }

    private void OnDetailVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectDetailViewModel.Project) && DetailVM.Project is not null)
        {
            SelectedProject = DetailVM.Project;
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        ListVM.SearchQuery = value;
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ListVM.SelectedStatusFilter = value;
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
    private PartyComboItem? _editSelectedClient;

    [ObservableProperty]
    private PartyComboItem? _editSelectedContractor;

    [ObservableProperty]
    private PartyComboItem? _editSelectedSubcontractor;

    [ObservableProperty]
    private PartyComboItem? _editSelectedConsultant;

    [ObservableProperty]
    private string? _editNotes;

    [ObservableProperty]
    private string? _editLogoSourcePath;

    [ObservableProperty]
    private string? _editCoverSourcePath;

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
    private string? _editGoogleMapsLink;

    [ObservableProperty]
    private double? _editLatitude;

    [ObservableProperty]
    private double? _editLongitude;

    [ObservableProperty]
    private string? _logoPreviewPath;

    [ObservableProperty]
    private string? _coverPreviewPath;

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
                Clients.Add(new PartyComboItem { SourceId = c.Id, Name = c.Name });

            var contractors = await _contractorService.GetAllAsync();
            Contractors.Clear();
            foreach (var c in contractors)
                Contractors.Add(new PartyComboItem { SourceId = c.Id, Name = c.Name });

            var subcontractors = await _subcontractorService.GetAllAsync();
            Subcontractors.Clear();
            foreach (var s in subcontractors)
                Subcontractors.Add(new PartyComboItem { SourceId = s.Id, Name = s.Name });

            var consultants = await _consultantService.GetAllAsync();
            Consultants.Clear();
            foreach (var c in consultants)
                Consultants.Add(new PartyComboItem { SourceId = c.Id, Name = c.Name });

            var allParties = await _projectPartyService.GetAllPartiesAsync();
            foreach (var p in allParties)
            {
                if (p.Role == "Client")
                    Clients.Add(new PartyComboItem { Name = p.Name, IsFromPartySystem = true });
                else if (p.Role == "MainContractor")
                    Contractors.Add(new PartyComboItem { Name = p.Name, IsFromPartySystem = true });
                else if (p.Role == "SubContractor")
                    Subcontractors.Add(new PartyComboItem { Name = p.Name, IsFromPartySystem = true });
                else if (p.Role == "Consultant")
                    Consultants.Add(new PartyComboItem { Name = p.Name, IsFromPartySystem = true });
            }

            ApplyFilters();

            await ListVM.LoadCommand.ExecuteAsync(null);

            if (System.Windows.Application.Current.Properties["StartupAction"] is string action)
            {
                System.Windows.Application.Current.Properties.Remove("StartupAction");

                switch (action)
                {
                    case "new":
                        NewProject();
                        break;

                    case "edit":
                    case "delete":
                        var first = ListVM.Projects.FirstOrDefault();
                        if (first is not null)
                        {
                            SelectedProject = await _projectService.GetByIdAsync(first.Id);
                            if (SelectedProject is not null)
                            {
                                _currentProjectService.SetProject(new ProjectContext(
                                    SelectedProject.Id, SelectedProject.Code, SelectedProject.Name));

                                if (action == "edit")
                                    EditProject();
                                else
                                    await DeleteAsync();
                            }
                        }
                        break;
                }
            }
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
        ListVM.SearchCommand.Execute(null);
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
            await SyncPartiesInternalAsync();
            await LoadPartiesAsync();

            if (SelectedProject is not null)
            {
                DetailVM.LoadCommand.Execute(SelectedProject.Id);
            }

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
                CoverPreviewPath = SelectedProject.CoverImagePath;
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
    public void NewProject()
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
        EditSelectedClient = null;
        EditSelectedContractor = null;
        EditSelectedSubcontractor = null;
        EditSelectedConsultant = null;
        EditNotes = null;
        EditLogoSourcePath = null;
        EditCoverSourcePath = null;
        EditDocumentsFolder = null;
        EditGoogleMapsLink = null;
        EditLatitude = null;
        EditLongitude = null;
        LogoPreviewPath = null;
        CoverPreviewPath = null;
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
        EditSelectedClient = SelectedProject.ClientId.HasValue
            ? Clients.FirstOrDefault(c => c.SourceId == SelectedProject.ClientId) : null;
        EditSelectedContractor = SelectedProject.ContractorId.HasValue
            ? Contractors.FirstOrDefault(c => c.SourceId == SelectedProject.ContractorId) : null;
        EditSelectedSubcontractor = SelectedProject.SubcontractorId.HasValue
            ? Subcontractors.FirstOrDefault(c => c.SourceId == SelectedProject.SubcontractorId) : null;
        EditSelectedConsultant = SelectedProject.ConsultantId.HasValue
            ? Consultants.FirstOrDefault(c => c.SourceId == SelectedProject.ConsultantId) : null;
        EditNotes = SelectedProject.Notes;
        EditLogoSourcePath = null;
        EditCoverSourcePath = null;
        EditDocumentsFolder = SelectedProject.DocumentsFolder;
        EditLatitude = SelectedProject.Latitude;
        EditLongitude = SelectedProject.Longitude;
        EditGoogleMapsLink = SelectedProject.GoogleMapsLink;
        LogoPreviewPath = SelectedProject.LogoPath;
        CoverPreviewPath = SelectedProject.CoverImagePath;
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
            var clientId = EditSelectedClient?.IsFromPartySystem == false ? EditSelectedClient.SourceId : null;
            var contractorId = EditSelectedContractor?.IsFromPartySystem == false ? EditSelectedContractor.SourceId : null;
            var subcontractorId = EditSelectedSubcontractor?.IsFromPartySystem == false ? EditSelectedSubcontractor.SourceId : null;
            var consultantId = EditSelectedConsultant?.IsFromPartySystem == false ? EditSelectedConsultant.SourceId : null;

            if (IsCreating)
            {
                if (!string.IsNullOrEmpty(EditGoogleMapsLink) && string.IsNullOrEmpty(QrCodePath))
                {
                    QrCodePath = _qrCodeService.GenerateQrFromUrl(0, EditGoogleMapsLink);
                    if (QrCodeService.TryParseGoogleMapsLink(EditGoogleMapsLink, out var lat, out var lng))
                    {
                        EditLatitude = lat;
                        EditLongitude = lng;
                    }
                }
                else if (EditLatitude.HasValue && EditLongitude.HasValue && string.IsNullOrEmpty(QrCodePath))
                {
                    QrCodePath = _qrCodeService.GenerateLocationQr(0, EditLatitude.Value, EditLongitude.Value);
                    var latStr = EditLatitude.Value.ToString(CultureInfo.InvariantCulture);
                    var lngStr = EditLongitude.Value.ToString(CultureInfo.InvariantCulture);
                    EditGoogleMapsLink = $"https://www.google.com/maps?q={latStr},{lngStr}";
                }

                var dto = new CreateProjectDto(
                    EditCode, EditName, EditDescription,
                    EditStartDate, EditFinishDate, EditCurrency,
                    EditLocation, clientId, contractorId, subcontractorId, consultantId, EditNotes,
                    EditLogoSourcePath, EditCoverSourcePath, EditDocumentsFolder, EditLatitude, EditLongitude, EditGoogleMapsLink);

                SelectedProject = await _projectService.CreateAsync(dto);

                if (SelectedProject is not null && !string.IsNullOrEmpty(QrCodePath))
                {
                    QrCodePath = _qrCodeService.GenerateQrFromUrl(SelectedProject.Id, EditGoogleMapsLink ?? $"https://www.google.com/maps?q={EditLatitude?.ToString(CultureInfo.InvariantCulture)},{EditLongitude?.ToString(CultureInfo.InvariantCulture)}");
                    var updateDto = new UpdateProjectDto(
                        SelectedProject.Code, SelectedProject.Name, SelectedProject.Description,
                        SelectedProject.StartDate, SelectedProject.FinishDate, SelectedProject.Currency,
                        SelectedProject.Location, clientId, contractorId, subcontractorId, consultantId, SelectedProject.Notes,
                        null, null, SelectedProject.DocumentsFolder, EditLatitude, EditLongitude, EditGoogleMapsLink, QrCodePath);
                    SelectedProject = await _projectService.UpdateAsync(SelectedProject.Id, updateDto);
                }
            }
            else if (SelectedProject != null)
            {
                if (!string.IsNullOrEmpty(EditGoogleMapsLink) && string.IsNullOrEmpty(QrCodePath))
                {
                    QrCodePath = _qrCodeService.GenerateQrFromUrl(SelectedProject.Id, EditGoogleMapsLink);
                    if (QrCodeService.TryParseGoogleMapsLink(EditGoogleMapsLink, out var lat, out var lng))
                    {
                        EditLatitude = lat;
                        EditLongitude = lng;
                    }
                }
                else if (EditLatitude.HasValue && EditLongitude.HasValue && string.IsNullOrEmpty(QrCodePath))
                {
                    QrCodePath = _qrCodeService.GenerateLocationQr(SelectedProject.Id, EditLatitude.Value, EditLongitude.Value);
                    var latStr = EditLatitude.Value.ToString(CultureInfo.InvariantCulture);
                    var lngStr = EditLongitude.Value.ToString(CultureInfo.InvariantCulture);
                    EditGoogleMapsLink = $"https://www.google.com/maps?q={latStr},{lngStr}";
                }

                var dto = new UpdateProjectDto(
                    EditCode, EditName, EditDescription,
                    EditStartDate, EditFinishDate, EditCurrency,
                    EditLocation, clientId, contractorId, subcontractorId, consultantId, EditNotes,
                    EditLogoSourcePath, EditCoverSourcePath, EditDocumentsFolder, EditLatitude, EditLongitude, EditGoogleMapsLink, QrCodePath);

                SelectedProject = await _projectService.UpdateAsync(SelectedProject.Id, dto);
            }

            var wasCreating = IsCreating;
            IsEditing = false;
            IsCreating = false;
            LoadDocuments();
            await LoadAsync();
            await SyncPartiesInternalAsync();
            StatusMessage?.Invoke(wasCreating ? "Project created successfully" : "Project saved successfully");
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
            var projectName = SelectedProject.Name;
            await _projectService.DeleteAsync(SelectedProject.Id);
            _currentProjectService.SetProject(null);
            DetailVM.Project = null;
            SelectedProject = null;
            await LoadAsync();
            StatusMessage?.Invoke($"Project '{projectName}' deleted");
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
            StatusMessage?.Invoke($"Status changed to {newStatus}");
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
    private void BrowseCover()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
            Title = "Select Project Cover Image"
        };

        if (dialog.ShowDialog() == true)
        {
            EditCoverSourcePath = dialog.FileName;
            CoverPreviewPath = dialog.FileName;
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
            StatusMessage?.Invoke($"Document '{doc.FileName}' deleted");
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
            StatusMessage?.Invoke("All documents deleted");
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
            StatusMessage?.Invoke("Folder scanned successfully");
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

            var latStr = EditLatitude.Value.ToString(CultureInfo.InvariantCulture);
            var lngStr = EditLongitude.Value.ToString(CultureInfo.InvariantCulture);
            var googleMapsUrl = $"https://www.google.com/maps?q={latStr},{lngStr}";

            var dto = new UpdateProjectDto(
                SelectedProject.Code, SelectedProject.Name, SelectedProject.Description,
                SelectedProject.StartDate, SelectedProject.FinishDate, SelectedProject.Currency,
                SelectedProject.Location, SelectedProject.ClientId, SelectedProject.ContractorId,
                SelectedProject.SubcontractorId, null, SelectedProject.Notes,
                null, null, SelectedProject.DocumentsFolder, EditLatitude, EditLongitude, googleMapsUrl, QrCodePath);

            await _projectService.UpdateAsync(SelectedProject.Id, dto);
            SelectedProject = await _projectService.GetByIdAsync(SelectedProject.Id);

            if (SelectedProject is not null)
                DetailVM.LoadCommand.Execute(SelectedProject.Id);

            StatusMessage?.Invoke("Location QR code generated");
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

    private async Task SyncPartiesInternalAsync()
    {
        if (SelectedProject == null) return;
        var project = SelectedProject;
        var ct = CancellationToken.None;

        if (!string.IsNullOrEmpty(project.ClientName))
            await _projectPartyService.SavePartyAsync(project.Id,
                new SavePartyRequest(null, "Client", project.ClientName, null, null, null, null, 0), ct);

        if (!string.IsNullOrEmpty(project.ContractorName))
            await _projectPartyService.SavePartyAsync(project.Id,
                new SavePartyRequest(null, "MainContractor", project.ContractorName, null, null, null, null, 1), ct);

        if (!string.IsNullOrEmpty(project.SubcontractorName))
            await _projectPartyService.SavePartyAsync(project.Id,
                new SavePartyRequest(null, "SubContractor", project.SubcontractorName, null, null, null, null, 2), ct);

        if (!string.IsNullOrEmpty(project.ConsultantName))
            await _projectPartyService.SavePartyAsync(project.Id,
                new SavePartyRequest(null, "Consultant", project.ConsultantName, null, null, null, null, 3), ct);
    }

    [RelayCommand]
    private async Task SyncPartiesFromProjectAsync()
    {
        if (SelectedProject == null) return;
        IsLoading = true;
        try
        {
            await SyncPartiesInternalAsync();
            await LoadPartiesAsync();
            StatusMessage?.Invoke("Parties synced from project");
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
            StatusMessage?.Invoke($"Party saved");
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
            StatusMessage?.Invoke($"Party '{party.Name}' removed");
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

public class PartyComboItem
{
    public int? SourceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsFromPartySystem { get; init; }
}
