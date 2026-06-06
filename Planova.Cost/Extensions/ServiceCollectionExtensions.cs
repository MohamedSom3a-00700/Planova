using Microsoft.Extensions.DependencyInjection;
using Planova.Cost.Domain.Interfaces;

namespace Planova.Cost.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaCost(this IServiceCollection services)
    {
        services.AddScoped<ICostService, Application.Services.CostService>();
        services.AddScoped<IDirectCostService, Application.Services.DirectCostService>();
        services.AddScoped<IActualCostService, Application.Services.ActualCostService>();
        services.AddScoped<ICashFlowService, Application.Services.CashFlowService>();
        services.AddScoped<IEvmService, Application.Services.EvmService>();
        services.AddScoped<ICostAiService, Application.Services.CostAiService>();
        services.AddScoped<ICostReportService, Application.Services.CostReportService>();
        return services;
    }
}
