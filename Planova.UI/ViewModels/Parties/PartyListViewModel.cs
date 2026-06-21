using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using Microsoft.Win32;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Parties;

public sealed partial class PartyListViewModel : ObservableObject
{
    private readonly IProjectPartyService _projectPartyService;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IProjectService _projectService;

    public PartyListViewModel(
        IProjectPartyService projectPartyService,
        ICurrentProjectService currentProjectService,
        IProjectService projectService)
    {
        _projectPartyService = projectPartyService;
        _currentProjectService = currentProjectService;
        _projectService = projectService;
        _currentProjectService.CurrentProjectChanged += OnCurrentProjectChanged;
    }

    private async void OnCurrentProjectChanged(object? sender, ProjectContext? context)
    {
        await RefreshAsync();
    }

    public ObservableCollection<ProjectPartyDto> Parties { get; } = new();

    public ObservableCollection<ProjectSummaryDto> ProjectOptions { get; } = new();

    public List<string> AvailableRoles { get; } = new()
    {
        "Client",
        "MainContractor",
        "SubContractor",
        "Consultant"
    };

    private static readonly Dictionary<string, int> RolePriority = new()
    {
        ["Client"] = 0,
        ["MainContractor"] = 1,
        ["SubContractor"] = 2,
        ["Consultant"] = 3
    };

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty = true;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private bool _hasProject;

    [ObservableProperty]
    private string _editTitle = "New Party";

    [ObservableProperty]
    private Guid? _editId;

    [ObservableProperty]
    private int? _editProjectId;

    [ObservableProperty]
    private ProjectSummaryDto? _selectedEditProject;

    partial void OnSelectedEditProjectChanged(ProjectSummaryDto? value)
    {
        EditProjectId = value?.Id;
    }

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editRole = "Client";

    [ObservableProperty]
    private string _editContactPerson = string.Empty;

    [ObservableProperty]
    private string _editContactEmail = string.Empty;

    [ObservableProperty]
    private string _editContactPhone = string.Empty;

    [ObservableProperty]
    private string _editAddress = string.Empty;

    [ObservableProperty]
    private string? _editLogoPath;

    [ObservableProperty]
    private string? _editLogoFileName;

    private byte[]? _editLogoBytes;

    public async Task RefreshAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var ctx = _currentProjectService.CurrentProject;
            HasProject = ctx is not null;
            ProjectName = ctx?.Name ?? string.Empty;

            var projects = await _projectService.GetAllAsync();
            ProjectOptions.Clear();
            foreach (var p in projects)
                ProjectOptions.Add(p);

            var parties = await _projectPartyService.GetAllPartiesAsync();
            Parties.Clear();
            var sorted = parties
                .OrderBy(p => RolePriority.TryGetValue(p.Role, out var pri) ? pri : 99)
                .ThenBy(p => p.ProjectName)
                .ThenBy(p => p.Name)
                .ToList();
            foreach (var party in sorted)
                Parties.Add(party);
            IsEmpty = Parties.Count == 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load parties: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private void AddNew()
    {
        EditId = null;
        var ctx = _currentProjectService.CurrentProject;
        SelectedEditProject = ctx is not null
            ? ProjectOptions.FirstOrDefault(p => p.Id == ctx.Id)
            : null;
        EditProjectId = SelectedEditProject?.Id;
        EditName = string.Empty;
        EditRole = "Client";
        EditContactPerson = string.Empty;
        EditContactEmail = string.Empty;
        EditContactPhone = string.Empty;
        EditAddress = string.Empty;
        EditLogoPath = null;
        EditLogoFileName = null;
        _editLogoBytes = null;
        EditTitle = "New Party";
        IsEditMode = true;
        HasError = false;
    }

    [RelayCommand]
    private void Edit(ProjectPartyDto party)
    {
        if (party is null) return;

        EditId = party.Id;
        EditProjectId = party.ProjectId;
        EditName = party.Name;
        EditRole = party.Role;
        EditContactPerson = party.ContactPerson ?? string.Empty;
        EditContactEmail = party.ContactEmail ?? string.Empty;
        EditContactPhone = party.ContactPhone ?? string.Empty;
        EditAddress = party.Address ?? string.Empty;
        EditLogoPath = party.LogoPath;
        EditLogoFileName = null;
        _editLogoBytes = null;
        EditTitle = $"Edit {party.Name}";
        IsEditMode = true;
        HasError = false;
    }

    [RelayCommand]
    private void BrowseLogo()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Party Logo",
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp|All Files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            EditLogoPath = dialog.FileName;
            EditLogoFileName = System.IO.Path.GetFileName(dialog.FileName);
            _editLogoBytes = System.IO.File.ReadAllBytes(dialog.FileName);
        }
    }

    [RelayCommand]
    private void ClearLogo()
    {
        EditLogoPath = null;
        EditLogoFileName = null;
        _editLogoBytes = null;
    }

    [RelayCommand]
    private async Task DeleteAsync(ProjectPartyDto party)
    {
        if (party is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            await _projectPartyService.DeletePartyAsync(party.Id);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete party: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var projectId = EditProjectId ?? SelectedEditProject?.Id;
        if (projectId is null)
        {
            ErrorMessage = "Please select a project for this party.";
            HasError = true;
            return;
        }
        EditProjectId = projectId;

        if (string.IsNullOrWhiteSpace(EditName))
        {
            ErrorMessage = "Party name is required.";
            HasError = true;
            return;
        }

        IsLoading = true;
        HasError = false;

        try
        {
            var request = new SavePartyRequest(
                EditId,
                EditRole,
                EditName.Trim(),
                EditAddress?.Trim(),
                EditContactPerson?.Trim(),
                EditContactEmail?.Trim(),
                EditContactPhone?.Trim(),
                DisplayOrder: 0);

            var result = await _projectPartyService.SavePartyAsync(projectId.Value, request);

            if (_editLogoBytes is not null && !string.IsNullOrEmpty(EditLogoFileName))
            {
                using var stream = new MemoryStream(_editLogoBytes);
                await _projectPartyService.UploadLogoAsync(result.Id, EditLogoFileName, stream);
            }

            IsEditMode = false;
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save party: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        HasError = false;
    }
}
