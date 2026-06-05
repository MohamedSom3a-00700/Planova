using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceLibraryViewModel : ObservableObject
{
    private readonly IResourceService _resourceService;
    private readonly IResourceImportService _importService;

    [ObservableProperty]
    private ObservableCollection<ResourceDto> _resources = new();

    [ObservableProperty]
    private ResourceFilter _filter = new();

    [ObservableProperty]
    private ResourceDto? _selectedResource;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private ResourceType _editResourceType = ResourceType.Labour;

    [ObservableProperty]
    private ResourceScope _editScope = ResourceScope.Project;

    [ObservableProperty]
    private decimal _editDefaultRate;

    [ObservableProperty]
    private string _editUnitOfMeasure = "hr";

    [ObservableProperty]
    private string _editCurrency = "USD";

    public ResourceLibraryViewModel(IResourceService resourceService, IResourceImportService importService)
    {
        _resourceService = resourceService;
        _importService = importService;
    }

    [RelayCommand]
    private async Task LoadResources()
    {
        IsLoading = true;
        try
        {
            var results = await _resourceService.SearchAsync(Filter);
            Resources = new ObservableCollection<ResourceDto>(results);
            StatusMessage = $"Loaded {results.Count} resources";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Search()
    {
        Filter = Filter with { SearchQuery = SearchQuery };
        await LoadResources();
    }

    [RelayCommand]
    private void CreateResource()
    {
        IsNew = true;
        IsEditing = true;
        EditName = string.Empty;
        EditResourceType = ResourceType.Labour;
        EditScope = ResourceScope.Project;
        EditDefaultRate = 0;
        EditUnitOfMeasure = "hr";
        EditCurrency = "USD";
    }

    [RelayCommand]
    private void EditResource(ResourceDto? resource)
    {
        if (resource is null) return;
        IsNew = false;
        IsEditing = true;
        EditName = resource.Name;
        EditResourceType = resource.ResourceType;
        EditScope = resource.Scope;
        EditDefaultRate = resource.DefaultRate;
        EditUnitOfMeasure = resource.UnitOfMeasure;
        EditCurrency = resource.Currency;
    }

    [RelayCommand]
    private async Task SaveResource()
    {
        try
        {
            if (IsNew)
            {
                var request = new CreateResourceRequest
                {
                    Name = EditName,
                    ResourceType = EditResourceType,
                    Scope = EditScope,
                    DefaultRate = EditDefaultRate,
                    UnitOfMeasure = EditUnitOfMeasure,
                    Currency = EditCurrency
                };
                await _resourceService.CreateAsync(request);
                StatusMessage = "Resource created";
            }
            else if (SelectedResource is not null)
            {
                var request = new UpdateResourceRequest
                {
                    Id = SelectedResource.Id,
                    Name = EditName,
                    DefaultRate = EditDefaultRate,
                    UnitOfMeasure = EditUnitOfMeasure,
                    Currency = EditCurrency
                };
                await _resourceService.UpdateAsync(request);
                StatusMessage = "Resource updated";
            }

            IsEditing = false;
            await LoadResources();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
    }

    [RelayCommand]
    private async Task DeactivateResource(ResourceDto? resource)
    {
        if (resource is null) return;
        await _resourceService.DeactivateAsync(resource.Id);
        StatusMessage = "Resource deactivated";
        await LoadResources();
    }

    [RelayCommand]
    private async Task ReactivateResource(ResourceDto? resource)
    {
        if (resource is null) return;
        await _resourceService.ReactivateAsync(resource.Id);
        StatusMessage = "Resource reactivated";
        await LoadResources();
    }

    [RelayCommand]
    private async Task ImportResources()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV Files|*.csv|Excel Files|*.xlsx;*.xls|All Files|*.*",
            Title = "Import Resources"
        };
        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        try
        {
            await using var stream = File.OpenRead(dialog.FileName);
            var preview = await _importService.PreviewImportAsync(stream, dialog.FileName, null);

            if (preview.ValidRows > 0)
            {
                stream.Position = 0;
                var request = new ImportRequest
                {
                    FileStream = stream,
                    FileName = dialog.FileName,
                    DuplicateHandling = ImportDuplicateHandling.Skip,
                    SelectedRowNumbers = preview.Rows.Where(r => r.IsValid).Select(r => r.RowNumber).ToList()
                };
                var result = await _importService.ExecuteImportAsync(request);
                StatusMessage = $"Imported {result.SuccessCount} resources ({result.ErrorCount} errors)";
            }
            else
            {
                StatusMessage = "No valid rows found in file";
            }
        }
        finally
        {
            IsLoading = false;
        }

        await LoadResources();
    }

    [RelayCommand]
    private async Task ExportResources()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV Files|*.csv",
            Title = "Export Resources",
            FileName = "resources.csv"
        };
        if (dialog.ShowDialog() != true) return;

        var resources = await _resourceService.SearchAsync(Filter);
        var lines = new List<string> { "Code,Name,Type,Status,Scope,DefaultRate,UnitOfMeasure" };
        foreach (var r in resources)
            lines.Add($"{r.Code},{r.Name},{r.ResourceType},{r.Status},{r.Scope},{r.DefaultRate},{r.UnitOfMeasure}");

        await File.WriteAllLinesAsync(dialog.FileName, lines);
        StatusMessage = $"Exported {resources.Count} resources";
    }
}
