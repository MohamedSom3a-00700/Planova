using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.ViewModels.Wbs;

public sealed partial class WbsTemplatesViewModel : ObservableObject
{
    private readonly IWbsTemplateService _templateService;

    public WbsTemplatesViewModel(IWbsTemplateService templateService)
    {
        _templateService = templateService;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public ObservableCollection<WbsTemplate> Templates { get; } = new();

    [ObservableProperty]
    private WbsTemplate? _selectedTemplate;

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private string _newTemplateName = string.Empty;

    [ObservableProperty]
    private string _newTemplateCategory = string.Empty;

    public List<string> Categories { get; } =
    [
        "All",
        "Building",
        "Infrastructure",
        "Landscape",
        "Roads",
        "Water Network",
        "Sewer Network",
        "Industrial",
        "Power Plant"
    ];

    partial void OnSelectedCategoryChanged(string value)
    {
        _ = LoadTemplatesAsync(CancellationToken.None);
    }

    [RelayCommand]
    private async Task LoadTemplatesAsync(CancellationToken ct)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            IReadOnlyList<WbsTemplate> templates;
            if (string.IsNullOrWhiteSpace(SelectedCategory) || SelectedCategory == "All")
                templates = await _templateService.GetAllAsync(ct);
            else
                templates = await _templateService.GetByCategoryAsync(SelectedCategory, ct);

            Templates.Clear();
            foreach (var t in templates)
                Templates.Add(t);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load templates: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApplyTemplateAsync(CancellationToken ct)
    {
        if (SelectedTemplate is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            await _templateService.ApplyAsync(
                SelectedTemplate.Id, SelectedTemplate.Name, 0, ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to apply template: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
