using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.Primavera.Background;

public class PrimaveraImportHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PrimaveraImportHostedService> _logger;

    public PrimaveraImportHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<PrimaveraImportHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Primavera Import Hosted Service started.");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IPrimaveraImportService>();
            _ = importService;
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Primavera Import Hosted Service stopped.");
        }
    }
}
