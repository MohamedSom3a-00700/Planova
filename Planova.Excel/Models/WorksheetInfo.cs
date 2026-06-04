namespace Planova.Excel.Models;

public class WorksheetInfo
{
    public string Name { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public List<string> Columns { get; set; } = new();
}
