using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planova.Application.Repositories;
using Planova.Application.Services;
using Planova.Infrastructure.Logging;
using Planova.Localization.Services;
using Planova.Persistence.DbContext;
using Planova.Persistence.Repositories;
using Planova.Persistence.Services;
using Planova.Shared.Abstractions;
using Planova.UI.Services;
using Planova.Excel.Extensions;
using Planova.Excel.Services;
using Planova.UI.ViewModels;
using Planova.UI.ViewModels.Excel;
using Planova.UI.Views;
using Planova.UI.Views.AI;
using Planova.UI.Views.Clients;
using Planova.UI.Views.Projects;
using Planova.UI.Views.Contracts;
using Planova.UI.Views.Dashboard;
using Planova.UI.Views.Excel;
using Planova.UI.Views.Profile;
using Planova.UI.Views.Reports;
using QuestPDF.Infrastructure;
using Serilog;
using Wpf.Ui.Appearance;
using WpfUiControls = Wpf.Ui.Controls;

namespace Planova.UI;

public partial class App : System.Windows.Application
{
    private IHost _host = null!;

    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(GetLogDirectory(), "planova-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 100_000_000)
            .CreateBootstrapLogger();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Log.Information("Planova starting up");

            _host = Host.CreateDefaultBuilder(e.Args)
                .UseSerilog(dispose: true)
                .ConfigureServices(ConfigureServices)
                .Build();

            var databaseService = _host.Services.GetRequiredService<IDatabaseService>();
            databaseService.InitializeAsync().GetAwaiter().GetResult();

            var settingsService = _host.Services.GetRequiredService<ISettingsService>();
            settingsService.Load().GetAwaiter().GetResult();

            var theme = settingsService.Get<string>("ThemePreference");
            var resolvedTheme = AppTheme.Dark;
            if (!string.IsNullOrEmpty(theme))
            {
                var themeService = _host.Services.GetRequiredService<IThemeService>();
                themeService.SetTheme(theme);
                resolvedTheme = themeService.CurrentTheme;
            }

            ApplyWpfUiTheme(resolvedTheme);

            var language = settingsService.Get<string>("LanguagePreference");
            if (!string.IsNullOrEmpty(language))
            {
                var localizationService = _host.Services.GetRequiredService<ILocalizationService>();
                localizationService.SetLanguage(language);
            }

            var shellView = _host.Services.GetRequiredService<ShellView>();

            ApplyWindowBounds(shellView, settingsService);

            shellView.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            MessageBox.Show(
                "An error occurred while starting the application. Please check the logs for details.",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<ILoggingService, SerilogLoggingService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IHighContrastDetector, HighContrastDetector>();
        services.AddSingleton<IThemeService, Services.ThemeService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddDbContext<PlanovaDbContext>(options =>
            options.UseSqlite($"Data Source={GetDatabasePath()}"));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddTransient<ShellViewModel>();
        services.AddTransient<ShellView>();
        services.AddTransient<ProjectsWorkspaceViewModel>();
        services.AddTransient<ProjectsWorkspaceView>();
        services.AddTransient<ClientsWorkspaceViewModel>();
        services.AddTransient<ClientsWorkspaceView>();
        services.AddTransient<ContractsWorkspaceViewModel>();
        services.AddTransient<ContractsWorkspaceView>();
        services.AddTransient<UserProfileViewModel>();
        services.AddTransient<UserProfileView>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardView>();
        services.AddTransient<AssistantPanelViewModel>();
        services.AddTransient<AssistantPanelView>();

        services.AddPlanovaExcel();
        services.AddScoped<IMappingProfileService, MappingProfileService>();
        services.AddTransient<WorkbookBrowserViewModel>();
        services.AddTransient<WorkbookBrowserView>();
        services.AddTransient<ImportViewModel>();
        services.AddTransient<ImportWizardView>();
        services.AddTransient<ExportViewModel>();
        services.AddTransient<ExportWizardView>();
        services.AddTransient<MappingProfilesViewModel>();
        services.AddTransient<MappingProfilesView>();
        services.AddTransient<ExcelStudioViewModel>();
        services.AddTransient<ExcelStudioView>();
    }

    private static string GetDatabasePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "Planova");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "planova.db");
    }

    private static string GetLogDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "Planova", "logs");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "planova-.log");
    }

    private static void ApplyWpfUiTheme(AppTheme theme)
    {
        ApplicationTheme wpfUiTheme = theme switch
        {
            AppTheme.Light => ApplicationTheme.Light,
            AppTheme.HighContrast => ApplicationTheme.HighContrast,
            _ => ApplicationTheme.Dark
        };

        var backdrop = WpfUiControls.WindowBackdrop.IsSupported(WpfUiControls.WindowBackdropType.Mica)
            ? WpfUiControls.WindowBackdropType.Mica
            : WpfUiControls.WindowBackdropType.None;

        ApplicationThemeManager.Apply(wpfUiTheme, backdrop);
    }

    private static void ApplyWindowBounds(Window window, ISettingsService settingsService)
    {
        const double defaultWidth = 1280;
        const double defaultHeight = 720;
        const double minWidth = 900;
        const double minHeight = 600;

        var effectiveMaxWidth = Math.Max(minWidth, SystemParameters.VirtualScreenWidth);
        var effectiveMaxHeight = Math.Max(minHeight, SystemParameters.VirtualScreenHeight);

        window.Width = Math.Clamp(settingsService.Get<int?>("WindowWidth") ?? defaultWidth, minWidth, effectiveMaxWidth);
        window.Height = Math.Clamp(settingsService.Get<int?>("WindowHeight") ?? defaultHeight, minHeight, effectiveMaxHeight);

        var x = settingsService.Get<int?>("WindowX");
        var y = settingsService.Get<int?>("WindowY");

        if (x is int savedX && y is int savedY && IsWithinVirtualScreen(savedX, savedY, window.Width, window.Height))
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = savedX;
            window.Top = savedY;
        }
        else
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        if (settingsService.Get<bool>("WindowMaximized"))
            window.WindowState = WindowState.Maximized;
    }

    private static bool IsWithinVirtualScreen(double left, double top, double width, double height)
    {
        var screenLeft = SystemParameters.VirtualScreenLeft;
        var screenTop = SystemParameters.VirtualScreenTop;
        var screenRight = screenLeft + SystemParameters.VirtualScreenWidth;
        var screenBottom = screenTop + SystemParameters.VirtualScreenHeight;

        return left >= screenLeft - 100
               && top >= screenTop - 100
               && left + width <= screenRight + 100
               && top + height <= screenBottom + 100;
    }

    private void OnDispatcherUnhandledException(object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled dispatcher exception");
        MessageBox.Show(
            $"An unexpected error occurred: {e.Exception.Message}",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnCurrentDomainUnhandledException(object sender,
        UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Log.Fatal(ex, "Unhandled AppDomain exception");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            var shellView = _host?.Services.GetService<ShellView>();
            if (shellView != null)
            {
                var settingsService = _host?.Services.GetService<ISettingsService>();
                if (settingsService != null)
                {
                    settingsService.Set("WindowX", (int)shellView.Left);
                    settingsService.Set("WindowY", (int)shellView.Top);
                    settingsService.Set("WindowWidth", (int)shellView.Width);
                    settingsService.Set("WindowHeight", (int)shellView.Height);
                    settingsService.Set("WindowMaximized", shellView.WindowState == WindowState.Maximized);
                    settingsService.Save().GetAwaiter().GetResult();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving settings on exit");
        }

        _host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
