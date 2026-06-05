using Microsoft.Extensions.DependencyInjection;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Resource.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaResource(this IServiceCollection services)
    {
        services.AddScoped<IResourceService, Application.Services.ResourceService>();
        services.AddScoped<IResourceRateService, Application.Services.ResourceRateService>();
        services.AddScoped<ICrewService, Application.Services.CrewService>();
        services.AddScoped<IResourceAssignmentService, Application.Services.ResourceAssignmentService>();
        services.AddScoped<IResourceHistogramService, Application.Services.ResourceHistogramService>();
        services.AddScoped<IResourceAiEstimationService, Application.Services.ResourceAiEstimationService>();
        services.AddScoped<IResourceReportService, Application.Services.ResourceReportService>();
        services.AddScoped<IResourceImportService, Application.Services.ResourceImportService>();
        return services;
    }
}
