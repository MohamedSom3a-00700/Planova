namespace Planova.ScheduleComparison.Domain.Interfaces;

public interface IComparisonExportService
{
    Task<string> ExportToExcelAsync(Guid sessionId, string outputDir, CancellationToken ct = default);
    Task<string> ExportToPdfAsync(Guid sessionId, string outputDir, CancellationToken ct = default);
    Task<string> ExportToJsonAsync(Guid sessionId, string outputDir, CancellationToken ct = default);
}
