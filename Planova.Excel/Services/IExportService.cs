using Planova.Excel.Models;

namespace Planova.Excel.Services;

/// <summary>Orchestrates the full export workflow: query, format, write workbook.</summary>
public interface IExportService
{
    /// <summary>Builds a default export request for the given entity type.</summary>
    Task<ExportRequest> BuildRequestAsync(string entityType, CancellationToken ct);

    /// <summary>Exports data to an Excel workbook with progress reporting.</summary>
    Task<ExportResult> ExportAsync(ExportRequest request, IProgress<int> progress, CancellationToken ct);

    /// <summary>Returns the list of entity types available for export.</summary>
    IReadOnlyList<string> GetExportableEntityTypes();
}
