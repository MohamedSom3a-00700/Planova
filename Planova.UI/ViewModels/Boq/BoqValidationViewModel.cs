using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqValidationViewModel : ObservableObject
{
    private readonly IBoqValidationService _validationService;
    private readonly IBoqService _boqService;
    private readonly IBoqSession _session;

    public BoqValidationViewModel(IBoqValidationService validationService, IBoqService boqService, IBoqSession session)
    {
        _validationService = validationService;
        _boqService = boqService;
        _session = session;
        _session.BoqChanged += OnBoqChanged;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasBoq;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isValid;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private int _warningCount;

    [ObservableProperty]
    private ValidationResult? _validationResult;

    public ObservableCollection<BoqItemValidationDto> ItemValidations { get; } = new();

    private async void OnBoqChanged(object? sender, Guid boqId)
    {
        HasBoq = true;
        ValidationResult = null;
        ItemValidations.Clear();
        StatusMessage = "Ready to validate";
    }

    [RelayCommand]
    private async Task RunValidationAsync(CancellationToken ct)
    {
        var boqId = _session.CurrentBoqId;
        if (boqId == null || boqId == Guid.Empty) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Running validation...";
            ItemValidations.Clear();

            var tree = await _boqService.GetTreeAsync(boqId.Value, ct);
            var flatItems = FlattenTree(tree).ToList();

            ValidationResult = await _validationService.ValidateStructureAsync(boqId.Value, ct);
            IsValid = ValidationResult.IsValid;
            ErrorCount = ValidationResult.Errors.Count;
            WarningCount = ValidationResult.Warnings.Count;

            var errorLookup = ValidationResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.ToList());
            var warningLookup = ValidationResult.Warnings
                .GroupBy(w => w.Code)
                .ToDictionary(g => g.Key, g => g.ToList());

            for (int i = 0; i < flatItems.Count; i++)
            {
                var item = flatItems[i];
                var issues = new List<string>();
                if (errorLookup.TryGetValue(item.Code, out var itemErrors))
                    issues.AddRange(itemErrors.Select(e => $"ERROR: {e.Message}"));
                if (warningLookup.TryGetValue(item.Code, out var itemWarnings))
                    issues.AddRange(itemWarnings.Select(w => $"WARN: {w.Message}"));

                ItemValidations.Add(new BoqItemValidationDto(
                    i + 1,
                    item.Code,
                    item.Description,
                    item.Unit,
                    item.Quantity,
                    item.Rate,
                    item.Amount,
                    item.CostCode,
                    item.ItemType.ToString(),
                    item.Level,
                    item.IsActive,
                    issues.Count == 0 ? "PASS" : "FAIL",
                    issues.Count > 0 ? string.Join("; ", issues) : null
                ));
            }

            StatusMessage = IsValid
                ? $"Validation passed — {ItemValidations.Count} items checked"
                : $"Validation failed: {ErrorCount} errors, {WarningCount} warnings — {ItemValidations.Count} items checked";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Validation error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static IEnumerable<BoqItemDto> FlattenTree(IReadOnlyList<BoqItemDto> items)
    {
        foreach (var item in items)
        {
            yield return item;
            if (item.Children != null)
            {
                foreach (var child in FlattenTree(item.Children))
                    yield return child;
            }
        }
    }
}
