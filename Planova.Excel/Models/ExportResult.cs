namespace Planova.Excel.Models;

public class ExportResult
{
    public int TotalRecords { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
}
