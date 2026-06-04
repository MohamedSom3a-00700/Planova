// IImportService — Orchestrates the full import workflow: read, validate, map, commit in batches
// Implemented by Planova.Excel.Services

public interface IImportService
{
    Task<ValidationResult> ValidateAsync(ImportRequest request, CancellationToken ct);
    Task<ImportResult> ImportAsync(ImportRequest request, IProgress<int> progress, CancellationToken ct);
    Task<ImportResult> PreviewImportAsync(ImportRequest request, CancellationToken ct);
}
