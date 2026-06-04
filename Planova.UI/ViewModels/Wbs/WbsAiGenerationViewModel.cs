using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Application.Services;
using Planova.Boq.Domain.Interfaces;
using Planova.Wbs.Application.Dto;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.ViewModels.Wbs;

public sealed partial class WbsAiGenerationViewModel : ObservableObject
{
    private readonly IWbsAiGenerationService _aiService;
    private readonly IWbsService _wbsService;
    private readonly IBoqService _boqService;
    private readonly IBoqImportService _importService;
    private readonly IWbsItemRepository _itemRepository;
    private readonly IBoqSession _session;

    public WbsAiGenerationViewModel(
        IWbsAiGenerationService aiService,
        IWbsService wbsService,
        IBoqService boqService,
        IBoqImportService importService,
        IWbsItemRepository itemRepository,
        IBoqSession session)
    {
        _aiService = aiService;
        _wbsService = wbsService;
        _boqService = boqService;
        _importService = importService;
        _itemRepository = itemRepository;
        _session = session;
    }

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _isAiAvailable;
    [ObservableProperty] private string _projectScope = string.Empty;
    [ObservableProperty] private int _selectedProjectId;
    [ObservableProperty] private string _wbsName = string.Empty;
    [ObservableProperty] private string _boqContext = string.Empty;
    [ObservableProperty] private bool _hasBoqContext;

    public ObservableCollection<BoqSummaryDto> AvailableBoqs { get; } = new();
    [ObservableProperty] private BoqSummaryDto? _selectedBoq;

    public ObservableCollection<SuggestedItemNode> SuggestedTree { get; } = new();

    [RelayCommand]
    private async Task CheckAvailabilityAsync(CancellationToken ct)
    {
        IsAiAvailable = await _aiService.IsAiAvailableAsync(ct);
    }

    [RelayCommand]
    private async Task ImportBoqFromExcelAsync(CancellationToken ct)
    {
        var projectId = _session.CurrentProjectId;
        if (projectId is null) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Excel Files|*.xlsx;*.xls;*.xlsm|All Files|*.*",
            Title = "Import BOQ from Excel"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            await _importService.ImportFromExcelAsync(projectId.Value, dialog.FileName, null, new Progress<int>(), ct);
            await LoadBoqsAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to import BOQ from Excel: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadBoqsAsync(CancellationToken ct)
    {
        var projectId = _session.CurrentProjectId;
        if (projectId is null) return;
        try
        {
            var boqs = await _boqService.GetByProjectIdAsync(projectId.Value, ct);
            AvailableBoqs.Clear();
            foreach (var boq in boqs)
                AvailableBoqs.Add(boq);
        }
        catch { /* BOQ loading failure is non-critical */ }
    }

    [RelayCommand]
    private async Task LoadBoqItemsAsync(CancellationToken ct)
    {
        if (SelectedBoq is null) return;
        IsLoading = true;
        HasError = false;
        try
        {
            var items = await _boqService.GetTreeAsync(SelectedBoq.Id, ct);
            var sb = new StringBuilder();
            sb.AppendLine("Reference BOQ Items:");
            foreach (var item in items.OrderBy(i => i.Level).ThenBy(i => i.SortOrder))
            {
                var indent = new string(' ', item.Level * 2);
                sb.AppendLine($"{indent}{item.Code} - {item.Description} ({item.Quantity} {item.Unit}, ${item.Rate}/unit)");
            }
            BoqContext = sb.ToString();
            HasBoqContext = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load BOQ items: {ex.Message}";
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task GenerateAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ProjectScope)) return;
        IsLoading = true;
        HasError = false;
        try
        {
            var scope = HasBoqContext
                ? $"{BoqContext}\n\nProject Scope:\n{ProjectScope}"
                : ProjectScope;

            var result = await _aiService.GenerateAsync(scope, SelectedBoq?.Id, ct);
            if (!result.IsAvailable)
            {
                ErrorMessage = "AI service is not available. Please check your connection and settings.";
                HasError = true;
                return;
            }
            SuggestedTree.Clear();
            foreach (var item in result.Items.OrderBy(i => i.Level).ThenBy(i => i.SortOrder))
                SuggestedTree.Add(BuildNode(item, null));

            if (string.IsNullOrWhiteSpace(WbsName))
                WbsName = "AI Generated WBS";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Generation failed: {ex.Message}";
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task AcceptAsync(CancellationToken ct)
    {
        if (SuggestedTree.Count == 0) return;
        IsLoading = true;
        HasError = false;
        try
        {
            var pId = _session.CurrentProjectId?.GetHashCode() ?? 0;
            var name = string.IsNullOrWhiteSpace(WbsName) ? "AI Generated WBS" : WbsName;
            var wbs = await _wbsService.CreateAsync(name, pId, WbsSource.AIGenerated, null, ct);
            foreach (var node in SuggestedTree.Where(n => n.IsSelected))
                await AddItemRecursive(node, wbs.Id, null, ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to accept suggestion: {ex.Message}";
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task RegenerateAsync(CancellationToken ct)
    {
        await GenerateAsync(ct);
    }

    private async Task AddItemRecursive(SuggestedItemNode node, Guid wbsId, Guid? parentId, CancellationToken ct)
    {
        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            WbsId = wbsId,
            ParentId = parentId,
            Code = string.Empty,
            ShortCode = string.Empty,
            Name = node.Name,
            Description = node.Description,
            Level = node.Level,
            SortOrder = node.SortOrder,
            WbsLevel = Enum.TryParse<WbsLevelType>(node.WbsLevel, out var wl) ? wl : WbsLevelType.WorkPackage,
            Weight = node.Weight,
            IsActive = true
        };
        await _itemRepository.AddAsync(item, ct);

        foreach (var child in node.Children.Where(c => c.IsSelected))
            await AddItemRecursive(child, wbsId, item.Id, ct);
    }

    private static SuggestedItemNode BuildNode(SuggestedItem item, Guid? parentId)
    {
        var node = new SuggestedItemNode
        {
            Id = item.Id,
            ParentId = item.ParentId,
            Name = item.Name,
            Description = item.Description,
            Level = item.Level,
            SortOrder = item.SortOrder,
            WbsLevel = item.WbsLevel,
            Weight = item.Weight,
            IsSelected = true
        };
        if (item.Children is not null)
        {
            foreach (var child in item.Children.OrderBy(c => c.Level).ThenBy(c => c.SortOrder))
                node.Children.Add(BuildNode(child, item.Id));
        }
        return node;
    }
}

public sealed partial class SuggestedItemNode : ObservableObject
{
    public Guid? Id { get; set; }
    public Guid? ParentId { get; set; }
    [ObservableProperty] private string _name = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
    [ObservableProperty] private string _wbsLevel = string.Empty;
    public decimal? Weight { get; set; }
    [ObservableProperty] private bool _isSelected = true;
    public ObservableCollection<SuggestedItemNode> Children { get; set; } = new();
}
