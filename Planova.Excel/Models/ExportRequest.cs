namespace Planova.Excel.Models;

public class ExportRequest
{
    public string EntityType { get; set; } = string.Empty;
    public List<string> SelectedColumns { get; set; } = new();
    public string OutputPath { get; set; } = string.Empty;
    public string SheetName { get; set; } = string.Empty;
    public bool IncludeHeaders { get; set; } = true;
}
