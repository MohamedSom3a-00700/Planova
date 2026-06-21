using Microsoft.Extensions.DependencyInjection;
using Planova.Boq.CsvReader;
using Planova.Boq.Domain.Interfaces;
using AppServices = Planova.Boq.Application.Services;

namespace Planova.Boq.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaBoq(this IServiceCollection services)
    {
        services.AddScoped<IBoqService, AppServices.BoqService>();
        services.AddScoped<AppServices.IBoqImportService, AppServices.BoqImportService>();
        services.AddScoped<IBoqExportService, AppServices.BoqExportService>();
        services.AddScoped<IBoqValidationService, AppServices.BoqValidationService>();
        services.AddScoped<IBoqReportService, AppServices.BoqReportService>();
        services.AddScoped<ITreeBuilder, AppServices.TreeBuilderService>();
        services.AddScoped<IBoqCsvReader, BoqCsvReader>();
        services.AddScoped<AppServices.ClassificationService>();
        services.AddScoped<AppServices.LibraryService>();
        services.AddSingleton<IBoqSession, AppServices.BoqSession>();
        services.AddScoped<IMultiSheetBoqImportService, AppServices.MultiSheetBoqImportService>();
        services.AddScoped<IBoqColumnMappingService, AppServices.BoqColumnMappingService>();
        services.AddScoped<AppServices.IBoqDescriptionParser, AppServices.BoqDescriptionParser>();

        return services;
    }
}
