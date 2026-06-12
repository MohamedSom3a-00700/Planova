using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Interfaces;

public interface IReportDataProvider<TData> where TData : class
{
    ReportType HandledType { get; }
    Task<TData> CollectDataAsync(int projectId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
}
