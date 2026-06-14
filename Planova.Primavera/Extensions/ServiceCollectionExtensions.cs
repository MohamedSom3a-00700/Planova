using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Planova.Primavera.Application.Parsers;
using Planova.Primavera.Application.Services;
using Planova.Primavera.Application.Writers;
using Planova.Primavera.Background;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.Primavera.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaPrimavera(this IServiceCollection services)
    {
        services.TryAddSingleton<XerParser>();
        services.TryAddSingleton<XerWriter>();

        services.TryAddScoped<IPrimaveraImportService, PrimaveraImportService>();
        services.TryAddScoped<IPrimaveraExportService, PrimaveraExportService>();
        services.TryAddScoped<IPrimaveraWorkspaceService, PrimaveraWorkspaceService>();
        services.TryAddScoped<IPrimaveraValidationService, PrimaveraValidationService>();
        services.TryAddScoped<IPrimaveraRepairService, PrimaveraRepairService>();

        services.AddHostedService<PrimaveraImportHostedService>();

        return services;
    }

    public static IServiceCollection AddPlanovaPrimaveraIfAvailable(this IServiceCollection services)
    {
        services.TryAddSingleton<XerParser>();
        services.TryAddSingleton<XerWriter>();

        services.TryAddScoped<IPrimaveraImportService, PrimaveraImportService>();
        services.TryAddScoped<IPrimaveraExportService, PrimaveraExportService>();
        services.TryAddScoped<IPrimaveraWorkspaceService, PrimaveraWorkspaceService>();
        services.TryAddScoped<IPrimaveraValidationService, PrimaveraValidationService>();
        services.TryAddScoped<IPrimaveraRepairService, PrimaveraRepairService>();

        return services;
    }
}
