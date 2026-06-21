using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Domain.Interfaces;

namespace Planova.UI.ViewModels.Boq;

public sealed partial class BoqColumnMappingViewModel : ObservableObject
{
    private readonly IBoqColumnMappingService _mappingService;

    public BoqColumnMappingViewModel(IBoqColumnMappingService mappingService)
    {
        _mappingService = mappingService;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private int _worksheetIndex;

    [ObservableProperty]
    private ColumnMapping? _detectedMapping;

    [ObservableProperty]
    private int _selectedCodeCol = -1;

    [ObservableProperty]
    private int _selectedDescCol = -1;

    [ObservableProperty]
    private int _selectedUnitCol = -1;

    [ObservableProperty]
    private int _selectedQtyCol = -1;

    [ObservableProperty]
    private int _selectedRateCol = -1;

    [ObservableProperty]
    private int _selectedAmountCol = -1;

    public ObservableCollection<string> Warnings { get; } = new();

    [RelayCommand]
    private async Task DetectAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(FilePath)) return;

        IsLoading = true;
        HasError = false;
        Warnings.Clear();

        try
        {
            DetectedMapping = await _mappingService.DetectMappingAsync(FilePath, WorksheetIndex, ct);
            SelectedCodeCol = DetectedMapping.CodeColumn ?? -1;
            SelectedDescCol = DetectedMapping.DescriptionColumn ?? -1;
            SelectedUnitCol = DetectedMapping.UnitColumn ?? -1;
            SelectedQtyCol = DetectedMapping.QuantityColumn ?? -1;
            SelectedRateCol = DetectedMapping.RateColumn ?? -1;
            SelectedAmountCol = DetectedMapping.AmountColumn ?? -1;

            var validation = await _mappingService.ValidateMappingAsync(DetectedMapping, ct);
            foreach (var w in validation.Warnings)
                Warnings.Add(w);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Detection failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public ColumnMapping GetCurrentMapping()
    {
        return new ColumnMapping(
            SelectedCodeCol >= 0 ? SelectedCodeCol : null,
            SelectedDescCol >= 0 ? SelectedDescCol : null,
            SelectedUnitCol >= 0 ? SelectedUnitCol : null,
            SelectedQtyCol >= 0 ? SelectedQtyCol : null,
            SelectedRateCol >= 0 ? SelectedRateCol : null,
            SelectedAmountCol >= 0 ? SelectedAmountCol : null,
            DetectedMapping?.Confidence ?? 0);
    }
}
