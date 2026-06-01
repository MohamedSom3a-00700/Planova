using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planova.Application.Services;
using Planova.Infrastructure.Logging;
using Planova.Localization.Services;
using Planova.Persistence.DbContext;
using Planova.Persistence.Services;
using Planova.Shared.Abstractions;
using Planova.UI.ViewModels;
using Planova.UI.Views;
using Serilog;

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
            if (!string.IsNullOrEmpty(theme))
            {
                var themeService = _host.Services.GetRequiredService<IThemeService>();
                themeService.SetTheme(theme);
            }

            var language = settingsService.Get<string>("LanguagePreference");
            if (!string.IsNullOrEmpty(language))
            {
                var localizationService = _host.Services.GetRequiredService<ILocalizationService>();
                localizationService.SetLanguage(language);
            }

            var shellView = _host.Services.GetRequiredService<ShellView>();

            if (settingsService.Get<int?>("WindowWidth") is int w)
                shellView.Width = w;
            if (settingsService.Get<int?>("WindowHeight") is int h)
                shellView.Height = h;
            if (settingsService.Get<int?>("WindowX") is int x)
                shellView.Left = x;
            if (settingsService.Get<int?>("WindowY") is int y)
                shellView.Top = y;
            if (settingsService.Get<bool>("WindowMaximized"))
                shellView.WindowState = WindowState.Maximized;

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
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddDbContext<PlanovaDbContext>(options =>
            options.UseSqlite($"Data Source={GetDatabasePath()}"));

        services.AddTransient<ShellViewModel>();
        services.AddTransient<ShellView>();
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
        return dir;
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
