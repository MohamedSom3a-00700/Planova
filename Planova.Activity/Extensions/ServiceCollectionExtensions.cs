using Microsoft.Extensions.DependencyInjection;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaActivity(this IServiceCollection services)
    {
        services.AddScoped<IActivityService, Application.Services.ActivityService>();
        services.AddScoped<IActivityRelationshipService, Application.Services.ActivityRelationshipService>();
        services.AddScoped<ICalendarService, Application.Services.CalendarService>();
        services.AddScoped<IActivityBankService, Application.Services.ActivityBankService>();
        services.AddScoped<IWbsGenerationService, Application.Services.WbsGenerationService>();
        services.AddScoped<IActivityReportService, Application.Services.ActivityReportService>();

        return services;
    }
}
