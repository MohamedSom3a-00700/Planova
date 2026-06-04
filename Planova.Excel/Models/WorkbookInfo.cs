namespace Planova.Excel.Models;

public class WorkbookInfo
{
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public List<WorksheetInfo> Worksheets { get; set; } = new();
    public bool IsValid { get; set; }
    public string Format { get; set; } = string.Empty;
}
