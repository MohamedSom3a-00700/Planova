using Microsoft.Extensions.DependencyInjection;
using Planova.Excel.Readers;
using Planova.Excel.Services;
using Planova.Excel.Validation;
using Planova.Excel.Writers;

namespace Planova.Excel.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaExcel(this IServiceCollection services)
    {
        services.AddSingleton<IWorkbookReader, WorkbookReader>();
        services.AddSingleton<IWorkbookWriter, WorkbookWriter>();
        services.AddSingleton<IWorkbookPreviewService, WorkbookPreviewService>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<IImportService, ImportService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<IMappingProfileService, MappingProfileService>();

        return services;
    }
}
