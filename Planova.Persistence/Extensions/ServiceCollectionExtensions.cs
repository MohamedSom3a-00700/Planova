using Microsoft.Extensions.DependencyInjection;
using Planova.Activity.Domain.Interfaces;
using Planova.Boq.Domain.Interfaces;
using Planova.Persistence.Repositories;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaPersistence(this IServiceCollection services)
    {
        // BOQ repositories
        services.AddScoped<IBoqRepository, BoqRepository>();
        services.AddScoped<IBoqItemRepository, BoqItemRepository>();
        services.AddScoped<IBoqClassificationRepository, BoqClassificationRepository>();
        services.AddScoped<IBoqLibraryRepository, BoqLibraryRepository>();

        // WBS repositories
        services.AddScoped<IWbsRepository, WbsRepository>();
        services.AddScoped<IWbsItemRepository, WbsItemRepository>();
        services.AddScoped<IWbsTemplateRepository, WbsTemplateRepository>();

        // Activity repositories
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IActivityRelationshipRepository, ActivityRelationshipRepository>();
        services.AddScoped<ICalendarRepository, CalendarRepository>();
        services.AddScoped<ICalendarDayRepository, CalendarDayRepository>();
        services.AddScoped<IActivityBankRepository, ActivityBankRepository>();
        services.AddScoped<IActivityBankItemRepository, ActivityBankItemRepository>();
        services.AddScoped<IActivityBankItemRelationshipRepository, ActivityBankItemRelationshipRepository>();

        return services;
    }
}
