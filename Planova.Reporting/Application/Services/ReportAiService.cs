using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Reporting.Application.Services;

public class ReportAiService : IReportAiService
{
    private readonly ILoggingService _logger;

    public ReportAiService(ILoggingService logger)
    {
        _logger = logger;
    }

    public bool IsAvailable => false;

    public Task<string> GenerateNarrativeAsync(ReportType reportType, object reportData, CancellationToken ct = default)
    {
        _logger.Info("Generating AI narrative for {ReportType} report", reportType);

        var narrative = reportType switch
        {
            ReportType.Daily => "AI-Generated Daily Report Narrative: Project activities progressed as scheduled with key accomplishments noted.",
            ReportType.Weekly => "AI-Generated Weekly Report Narrative: This week saw steady progress across all work packages.",
            ReportType.Monthly => "AI-Generated Monthly Report Narrative: Monthly performance indicators show positive trends in cost and schedule performance.",
            ReportType.Executive => "AI-Generated Executive Report Narrative: The project remains on track with key milestones achieved this period.",
            _ => "AI-Generated Narrative: Report data summarized."
        };

        _logger.Info("AI narrative generated for {ReportType} report ({Length} chars)", reportType, narrative.Length);
        return Task.FromResult(narrative);
    }
}
