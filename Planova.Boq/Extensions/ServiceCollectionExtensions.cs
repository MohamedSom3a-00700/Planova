using Planova.Boq.Application.Services;
using Planova.Boq.CsvReader;
using Planova.Boq.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Planova.Boq.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaBoq(this IServiceCollection services)
    {
        services.AddScoped<IBoqService, BoqService>();
        services.AddScoped<IBoqImportService, BoqImportService>();
        services.AddScoped<IBoqExportService, BoqExportService>();
        services.AddScoped<IBoqValidationService, BoqValidationService>();
        services.AddScoped<IBoqReportService, BoqReportService>();
        services.AddScoped<ITreeBuilder, TreeBuilderService>();
        services.AddScoped<IBoqCsvReader, BoqCsvReader>();
        services.AddScoped<ClassificationService>();
        services.AddScoped<LibraryService>();
        services.AddSingleton<IBoqSession, BoqSession>();

        return services;
    }
}
