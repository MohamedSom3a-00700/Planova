using Microsoft.Extensions.DependencyInjection;
using Planova.Activity.Domain.Interfaces;
using Planova.Application.Repositories;
using Planova.Application.Services;
using Planova.Boq.Domain.Interfaces;
using Planova.Cost.Domain.Interfaces;
using Planova.Persistence.Repositories;
using Planova.Persistence.Services;
using Planova.Resource.Domain.Interfaces;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;
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

        // Cost repositories
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IBudgetRevisionRepository, BudgetRevisionRepository>();
        services.AddScoped<IDirectCostRepository, DirectCostRepository>();
        services.AddScoped<ICostBaselineRepository, CostBaselineRepository>();
        services.AddScoped<IActualCostRepository, ActualCostRepository>();

        // Audit service
        services.AddScoped<IAuditService, AuditService>();

        // Resource repositories
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IResourceRateRepository, ResourceRateRepository>();
        services.AddScoped<ICrewRepository, CrewRepository>();
        services.AddScoped<ICrewResourceRepository, CrewResourceRepository>();
        services.AddScoped<IResourceAssignmentRepository, ResourceAssignmentRepository>();
        services.AddScoped<IResourceUsageRepository, ResourceUsageRepository>();

        // Project document services
        services.AddScoped<IProjectDocumentRepository, ProjectDocumentRepository>();
        services.AddScoped<IProjectDocumentService, ProjectDocumentService>();

        // Reporting repositories
        services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>();
        services.AddScoped<IReportInstanceRepository, ReportInstanceRepository>();
        services.AddScoped<IReportScheduleRepository, ReportScheduleRepository>();
        services.AddScoped<IProjectPartyRepository, ProjectPartyRepository>();

        return services;
    }
}
