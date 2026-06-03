namespace Planova.Excel.Models;

public class PreviewData
{
    public string WorksheetName { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public int TotalRowCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
}
