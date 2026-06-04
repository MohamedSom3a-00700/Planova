using Planova.Boq.Application.Dto;

namespace Planova.Boq.Application.Services;

public interface IBoqImportService
{
    Task<BoqImportResult> ImportFromExcelAsync(Guid projectId, string filePath, Guid? mappingProfileId, IProgress<int> progress, CancellationToken ct);
    Task<BoqImportResult> ImportFromExcelAsync(Guid projectId, string filePath, string? sheetName, Guid? mappingProfileId, IProgress<int> progress, CancellationToken ct);
    Task<BoqImportResult> ImportFromCsvAsync(Guid projectId, string filePath, CsvImportOptions options, IProgress<int> progress, CancellationToken ct);
    Task<BoqImportPreview> PreviewImportAsync(IReadOnlyList<ImportRow> rows, TreeBuildStrategy strategy, CancellationToken ct);
}
