using CommunityToolkit.Mvvm.ComponentModel;

namespace Planova.UI.ViewModels.Excel;

public partial class ColumnMappingEntry : ObservableObject
{
    [ObservableProperty]
    private string _excelColumn = string.Empty;

    [ObservableProperty]
    private string? _mappedField;

    public ColumnMappingEntry(string excelColumn, string? mappedField = null)
    {
        _excelColumn = excelColumn;
        _mappedField = mappedField;
    }
}
