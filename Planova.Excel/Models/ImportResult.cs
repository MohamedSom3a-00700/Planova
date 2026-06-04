namespace Planova.Excel.Models;

public class ImportResult
{
    public int TotalRecords { get; set; }
    public int ImportedRecords { get; set; }
    public int UpdatedRecords { get; set; }
    public int SkippedRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public int CompletedBatches { get; set; }
    public int TotalBatches { get; set; }
    public TimeSpan Duration { get; set; }
}
