using Planova.Boq.Domain.Enums;
using Planova.Boq.Application.Models;

namespace Planova.Boq.Domain.Interfaces;

public interface IMultiSheetBoqImportService
{
    Task<IReadOnlyList<WorksheetInfo>> ScanAsync(string filePath, CancellationToken ct);
    Task<BoqScanPreview> PreviewAsync(
        string filePath,
        Guid projectId,
        ImportMode mode,
        IReadOnlyList<WorksheetSelection> selections,
        CancellationToken ct);
    Task<BoqImportSummary> ImportAsync(
        string filePath,
        Guid projectId,
        ImportMode mode,
        IReadOnlyList<WorksheetSelection> selections,
        int userId,
        CancellationToken ct);
}

public record WorksheetInfo(string Name, int Index, int RowCount, bool IsBoqSheet, decimal MatchConfidence);
public record WorksheetSelection(int WorksheetIndex, ImportAction Action, ColumnMapping? OverrideMapping);
public record ColumnMapping(int? CodeColumn, int? DescriptionColumn, int? UnitColumn, int? QuantityColumn, int? RateColumn, int? AmountColumn, decimal Confidence);

public enum ImportAction { Skip, Import, Merge }
