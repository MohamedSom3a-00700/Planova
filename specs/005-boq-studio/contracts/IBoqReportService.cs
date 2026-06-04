// IBoqReportService — Generates BOQ reports (summary, itemized) in PDF format
// Implemented by Planova.Boq.Application.Services

public interface IBoqReportService
{
    Task<byte[]> GenerateSummaryReportAsync(Guid boqId, ReportFormat format, CancellationToken ct);
    Task<byte[]> GenerateItemizedReportAsync(Guid boqId, ReportFormat format, CancellationToken ct);
    Task SaveReportAsync(Guid boqId, ReportType type, ReportFormat format, string outputPath, CancellationToken ct);
}

public enum ReportType { Summary, Itemized }
public enum ReportFormat { Pdf, Excel }
