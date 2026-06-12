using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Reporting.Background;

public class ExportFileCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggingService _logger;

    public ExportFileCleanupService(IServiceProvider serviceProvider, ILoggingService logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Info("Export file cleanup starting...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var instanceRepo = scope.ServiceProvider.GetRequiredService<IReportInstanceRepository>();

            var instances = await instanceRepo.GetByProjectAsync(0, ct: stoppingToken);
            var cleanedCount = 0;

            foreach (var instance in instances)
            {
                if (instance.Exports is null || instance.Exports.Count == 0)
                    continue;

                var orphanExports = instance.Exports
                    .Where(e => !File.Exists(e.FilePath))
                    .ToList();

                if (orphanExports.Count == 0)
                    continue;

                foreach (var export in orphanExports)
                {
                    _logger.Warning("Removing orphan export record {ExportId} — file not found: {FilePath}", export.Id, export.FilePath);
                    instance.Exports.Remove(export);
                }

                await instanceRepo.UpdateAsync(instance, stoppingToken);
                cleanedCount += orphanExports.Count;
            }

            _logger.Info("Export file cleanup complete. Removed {Count} orphan export record(s).", cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.Error("Export file cleanup failed: {Message}", ex, ex.Message);
        }
    }
}
