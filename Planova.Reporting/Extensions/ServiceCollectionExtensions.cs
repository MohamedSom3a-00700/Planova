using Microsoft.Extensions.DependencyInjection;
using Planova.Reporting.Application.DataProviders;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Application.Services;
using Planova.Reporting.Background;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.Reporting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlanovaReporting(this IServiceCollection services)
    {
        services.AddScoped<IReportEngine, ReportEngine>();
        services.AddScoped<IReportExportService, ReportExportService>();
        services.AddScoped<IReportAiService, ReportAiService>();
        services.AddScoped<IReportSchedulerService, ReportSchedulerService>();
        services.AddScoped<IReportSettingsService, ReportSettingsService>();
        services.AddScoped<IProjectPartyService, ProjectPartyService>();

        services.AddScoped<IReportDataProvider<DailyReportDataDto>, DailyReportDataProvider>();
        services.AddScoped<IReportDataProvider<WeeklyReportDataDto>, WeeklyReportDataProvider>();
        services.AddScoped<IReportDataProvider<MonthlyReportDataDto>, MonthlyReportDataProvider>();
        services.AddScoped<IReportDataProvider<ExecutiveReportDataDto>, ExecutiveReportDataProvider>();

        services.AddHostedService<ReportGenerationHostedService>();
        services.AddHostedService<ExportFileCleanupService>();

        return services;
    }
}
