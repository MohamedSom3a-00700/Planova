using Planova.Excel.Models;

namespace Planova.Excel.Services;

/// <summary>Orchestrates the full import workflow: read, validate, map, commit in batches.</summary>
public interface IImportService
{
    /// <summary>Validates import records against entity rules without committing.</summary>
    Task<ValidationResult> ValidateAsync(ImportRequest request, CancellationToken ct);

    /// <summary>Executes the full import with batch-atomic commit and progress reporting.</summary>
    Task<ImportResult> ImportAsync(ImportRequest request, IProgress<int> progress, CancellationToken ct);

    /// <summary>Previews the import result (validation only, no commit).</summary>
    Task<ImportResult> PreviewImportAsync(ImportRequest request, CancellationToken ct);
}
