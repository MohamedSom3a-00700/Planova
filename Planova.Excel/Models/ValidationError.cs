namespace Planova.Excel.Models;

public class ValidationError
{
    public int RowIndex { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? RawValue { get; set; }
}
