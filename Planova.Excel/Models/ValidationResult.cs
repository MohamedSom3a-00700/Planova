namespace Planova.Excel.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public int TotalErrors { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
