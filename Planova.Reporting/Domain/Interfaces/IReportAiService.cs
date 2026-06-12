using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Interfaces;

public interface IReportAiService
{
    Task<string> GenerateNarrativeAsync(ReportType reportType, object reportData, CancellationToken ct = default);
    bool IsAvailable { get; }
}
