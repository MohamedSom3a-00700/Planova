using Planova.Excel.Models;
using Planova.Excel.Writers;
using Serilog;

namespace Planova.Excel.Services;

public class ExportService : IExportService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ExportService>();
    private readonly IWorkbookWriter _writer;

    public ExportService(IWorkbookWriter writer)
    {
        _writer = writer;
    }

    public Task<ExportRequest> BuildRequestAsync(string entityType, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var request = new ExportRequest
        {
            EntityType = entityType,
            SheetName = entityType,
            SelectedColumns = GetDefaultColumns(entityType).ToList(),
            OutputPath = string.Empty,
            IncludeHeaders = true
        };

        return Task.FromResult(request);
    }

    public async Task<ExportResult> ExportAsync(ExportRequest request, IProgress<int> progress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Log.Information("Starting export: EntityType={EntityType}, Columns={Columns}, OutputPath={OutputPath}",
            request.EntityType, request.SelectedColumns, request.OutputPath);

        var data = await FetchDataAsync(request.EntityType, request.SelectedColumns, ct);
        var result = await _writer.WriteAsync(request, data, ct);

        progress?.Report(100);
        Log.Information("Export completed: Records={Records}, OutputPath={OutputPath}, Size={Size}",
            result.TotalRecords, result.OutputPath, result.FileSize);

        return result;
    }

    public IReadOnlyList<string> GetExportableEntityTypes()
    {
        return new List<string> { "Project", "Activity", "Resource", "Cost", "Risk" };
    }

    private static Task<IReadOnlyList<Dictionary<string, object>>> FetchDataAsync(
        string entityType, List<string> selectedColumns, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // In a full implementation, this would query Planova services/database
        // For now, return sample data structure
        var data = new List<Dictionary<string, object>>();
        return Task.FromResult<IReadOnlyList<Dictionary<string, object>>>(data);
    }

    private static IEnumerable<string> GetDefaultColumns(string entityType)
    {
        return entityType switch
        {
            "Project" => new[] { "Id", "Code", "Name", "Status", "Client", "StartDate", "FinishDate" },
            "Activity" => new[] { "Id", "Code", "Name", "ProjectId", "StartDate", "FinishDate", "Duration" },
            "Resource" => new[] { "Id", "Name", "Type", "Rate", "Currency" },
            "Cost" => new[] { "Id", "Description", "Amount", "Category", "ProjectId" },
            "Risk" => new[] { "Id", "Title", "Category", "Probability", "Impact", "Status" },
            _ => Array.Empty<string>()
        };
    }
}
