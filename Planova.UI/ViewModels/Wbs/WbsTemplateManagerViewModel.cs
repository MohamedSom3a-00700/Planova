using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Domain.Interfaces;
using Planova.Excel.Readers;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using Planova.Wbs.Domain.Interfaces;
using Microsoft.Win32;

namespace Planova.UI.ViewModels.Wbs;

public sealed partial class WbsTemplateManagerViewModel : ObservableObject
{
    private readonly IWbsTemplateService _templateService;
    private readonly IWbsService _wbsService;
    private readonly IBoqSession _session;
    private readonly IWorkbookReader _workbookReader;

    public WbsTemplateManagerViewModel(
        IWbsTemplateService templateService,
        IWbsService wbsService,
        IBoqSession session,
        IWorkbookReader workbookReader)
    {
        _templateService = templateService;
        _wbsService = wbsService;
        _session = session;
        _workbookReader = workbookReader;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public ObservableCollection<WbsTemplate> Templates { get; } = new();

    public ObservableCollection<string> Categories { get; } = new();

    [ObservableProperty]
    private string _selectedCategory = string.Empty;

    [ObservableProperty]
    private WbsTemplate? _selectedTemplate;

    [ObservableProperty]
    private int _wbsProjectId;

    [ObservableProperty]
    private string _saveTemplateName = string.Empty;

    [ObservableProperty]
    private string _saveTemplateCategory = string.Empty;

    public ObservableCollection<TemplateTreeNode> TemplateTree { get; } = new();

    public List<string> AllCategories { get; } = new()
    {
        "All", "Building Construction", "Infrastructure", "Industrial", "Oil & Gas"
    };

    partial void OnSelectedCategoryChanged(string value)
    {
        _ = LoadTemplatesAsync(CancellationToken.None);
    }

    partial void OnSelectedTemplateChanged(WbsTemplate? value)
    {
        BuildTree(value);
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

            Categories.Clear();
            var distinct = templates.Select(t => t.Category).Distinct().OrderBy(c => c);
            foreach (var c in distinct)
                Categories.Add(c);
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
            var projectId = _session.CurrentProjectId;
            var name = $"WBS from {SelectedTemplate.Name}";
            await _templateService.ApplyAsync(SelectedTemplate.Id, name, projectId?.GetHashCode() ?? 0, ct);
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

    [RelayCommand]
    private async Task SaveAsTemplateAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(SaveTemplateName)) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var category = string.IsNullOrWhiteSpace(SaveTemplateCategory) ? "Custom" : SaveTemplateCategory;
            await _templateService.CreateAsync(SaveTemplateName, category, null, null, ct);
            SaveTemplateName = string.Empty;
            SaveTemplateCategory = string.Empty;

            if (SelectedCategory == "All" || SelectedCategory == category)
                await LoadTemplatesAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save template: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ImportTemplateAsync(CancellationToken ct)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON Files|*.json|All Files|*.*",
            Title = "Import WBS Template"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var json = await File.ReadAllTextAsync(dialog.FileName, ct);
            await _templateService.ImportFromJsonAsync(json, ct);
            await LoadTemplatesAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to import template: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ImportFromExcelAsync(CancellationToken ct)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Files|*.xlsx;*.xls;*.xlsm|All Files|*.*",
            Title = "Import WBS Template from Excel"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var info = await _workbookReader.OpenAsync(dialog.FileName, ct);
            var sheetName = info.Worksheets.FirstOrDefault()?.Name;
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                ErrorMessage = "No worksheets found in the workbook";
                HasError = true;
                return;
            }

            var rows = await _workbookReader.ReadAllAsync(dialog.FileName, sheetName, ct);
            if (rows.Count == 0)
            {
                ErrorMessage = "No data rows found";
                HasError = true;
                return;
            }

            var template = new WbsTemplate
            {
                Id = Guid.NewGuid(),
                Name = Path.GetFileNameWithoutExtension(dialog.FileName),
                Category = "Custom",
                IsStandard = false,
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var items = new List<WbsTemplateItem>();
            var codeToId = new Dictionary<string, Guid>();

            foreach (var row in rows)
            {
                if (!row.TryGetValue("Code", out var codeObj) || codeObj?.ToString() is not { } code)
                    continue;

                var itemId = Guid.NewGuid();
                codeToId[code] = itemId;

                row.TryGetValue("ParentCode", out var parentObj);
                var parentCode = parentObj?.ToString();

                row.TryGetValue("ShortCode", out var scObj);
                row.TryGetValue("Name", out var nameObj);
                row.TryGetValue("Description", out var descObj);
                row.TryGetValue("Level", out var levelObj);
                row.TryGetValue("SortOrder", out var sortObj);
                row.TryGetValue("DefaultDurationDays", out var durObj);
                row.TryGetValue("TypicalWeight", out var wtObj);
                row.TryGetValue("WbsLevel", out var wlObj);

                var item = new WbsTemplateItem
                {
                    Id = itemId,
                    TemplateId = template.Id,
                    ParentId = parentCode is not null && codeToId.ContainsKey(parentCode) ? codeToId[parentCode] : null,
                    Code = code,
                    ShortCode = scObj?.ToString() ?? string.Empty,
                    Name = nameObj?.ToString() ?? string.Empty,
                    Description = descObj?.ToString(),
                    Level = int.TryParse(levelObj?.ToString(), out var l) ? l : 0,
                    SortOrder = int.TryParse(sortObj?.ToString(), out var s) ? s : 0,
                    DefaultDurationDays = int.TryParse(durObj?.ToString(), out var d) ? d : null,
                    TypicalWeight = decimal.TryParse(wtObj?.ToString(), out var w) ? w : null,
                    WbsLevel = Enum.TryParse<WbsLevelType>(wlObj?.ToString(), true, out var wl) ? wl : WbsLevelType.Summary
                };

                items.Add(item);
            }

            template.Items = items;

            await _templateService.ImportFromJsonAsync(
                System.Text.Json.JsonSerializer.Serialize(template), ct);
            await LoadTemplatesAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to import from Excel: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportTemplateAsync(CancellationToken ct)
    {
        if (SelectedTemplate is null) return;

        var dialog = new SaveFileDialog
        {
            Filter = "JSON Files|*.json|All Files|*.*",
            Title = "Export WBS Template",
            FileName = $"{SelectedTemplate.Name}.json"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var json = await _templateService.ExportToJsonAsync(SelectedTemplate.Id, ct);
            await File.WriteAllTextAsync(dialog.FileName, json, ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to export template: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildTree(WbsTemplate? template)
    {
        TemplateTree.Clear();
        if (template?.Items is null) return;

        var roots = template.Items
            .Where(i => i.ParentId is null)
            .OrderBy(i => i.SortOrder);

        foreach (var root in roots)
            TemplateTree.Add(BuildNode(root, template.Items));
    }

    private static TemplateTreeNode BuildNode(WbsTemplateItem item, ICollection<WbsTemplateItem> allItems)
    {
        var node = new TemplateTreeNode
        {
            Name = item.Name,
            Code = item.Code,
            Level = item.Level,
            WbsLevel = item.WbsLevel.ToString(),
            DefaultDurationDays = item.DefaultDurationDays,
            TypicalWeight = item.TypicalWeight
        };

        var children = allItems
            .Where(i => i.ParentId == item.Id)
            .OrderBy(i => i.SortOrder);

        foreach (var child in children)
            node.Children.Add(BuildNode(child, allItems));

        return node;
    }
}

public sealed class TemplateTreeNode
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Level { get; set; }
    public string WbsLevel { get; set; } = string.Empty;
    public int? DefaultDurationDays { get; set; }
    public decimal? TypicalWeight { get; set; }
    public ObservableCollection<TemplateTreeNode> Children { get; set; } = new();
}
