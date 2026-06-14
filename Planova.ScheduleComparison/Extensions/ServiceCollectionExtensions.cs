using Microsoft.Extensions.DependencyInjection;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.ScheduleComparison.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaScheduleComparison(this IServiceCollection services)
    {
        services.AddScoped<IScheduleComparisonService, Application.Services.ScheduleComparisonService>();
        services.AddScoped<IScheduleSnapshotService, Application.Services.ScheduleSnapshotService>();
        services.AddScoped<IComparisonExportService, Application.Services.ComparisonExportService>();

        return services;
    }
}
