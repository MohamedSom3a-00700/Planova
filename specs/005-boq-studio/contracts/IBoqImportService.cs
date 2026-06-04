// IBoqImportService — Orchestrates BOQ import from Excel (via Phase 2) or CSV
// Implemented by Planova.Boq.Application.Services

public interface IBoqImportService
{
    Task<BoqImportResult> ImportFromExcelAsync(Guid projectId, string filePath, Guid? mappingProfileId, IProgress<int> progress, CancellationToken ct);
    Task<BoqImportResult> ImportFromCsvAsync(Guid projectId, string filePath, CsvImportOptions options, IProgress<int> progress, CancellationToken ct);
    Task<BoqImportPreview> PreviewImportAsync(IReadOnlyList<ImportRow> rows, TreeBuildStrategy strategy, CancellationToken ct);
}

public record BoqImportResult(
    Guid BoqId,
    int ItemsImported,
    int ItemsUpdated,
    int ItemsSkipped,
    IReadOnlyList<ValidationIssue> Errors,
    TimeSpan Duration
);

public record BoqImportPreview(
    IReadOnlyList<BoqItem> Tree,
    IReadOnlyList<ValidationIssue> Warnings,
    int TotalItems
);

public record CsvImportOptions(
    bool HasHeaders,
    string CodeColumn,
    string DescriptionColumn,
    string UnitColumn,
    string QuantityColumn,
    string RateColumn,
    string? LevelColumn,
    string? ParentIdColumn,
    string Delimiter = ","
);
