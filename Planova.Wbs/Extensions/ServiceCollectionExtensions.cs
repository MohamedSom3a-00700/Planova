using Microsoft.Extensions.DependencyInjection;
using Planova.Wbs.Application.Services;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaWbs(this IServiceCollection services)
    {
        services.AddScoped<IWbsService, WbsService>();
        services.AddScoped<IWbsValidationService, WbsValidationService>();
        services.AddScoped<IWbsBoqMappingService, WbsBoqMappingService>();
        services.AddScoped<IWbsTemplateService, WbsTemplateService>();
        services.AddScoped<IWbsAiGenerationService, WbsAiGenerationService>();
        services.AddScoped<IWbsReportService, WbsReportService>();

        return services;
    }
}
