using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Planova.Shared.Abstractions;
using Serilog;

namespace Planova.UI.Converters;

public class ThemeAwareLogoConverter : IValueConverter
{
    private const string LogoPathFormat = "pack://application:,,,/Resources/Branding/{0}";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var isDarkTheme = IsDarkOrHighContrast();
            var isWordmark = parameter is string param && param == "Wordmark";

            string fileName;
            if (IsHighContrastActive())
                fileName = "LogoMonochrome.png";
            else if (isWordmark)
                fileName = isDarkTheme ? "WordmarkDark.png" : "WordmarkLight.png";
            else
                fileName = isDarkTheme ? "LogoDark.png" : "LogoLight.png";

            return new BitmapImage(new Uri(string.Format(LogoPathFormat, fileName), UriKind.Absolute));
        }
        catch (FileNotFoundException ex)
        {
            Log.Warning(ex, "Logo asset not found, falling back to monochrome");
            return new BitmapImage(new Uri(string.Format(LogoPathFormat, "LogoMonochrome.png"), UriKind.Absolute));
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static bool IsDarkOrHighContrast()
    {
        var app = System.Windows.Application.Current;
        if (app?.Resources.MergedDictionaries == null)
            return true;

        foreach (var dict in app.Resources.MergedDictionaries)
        {
            if (dict.Source?.OriginalString?.Contains("HighContrastFallback") == true)
                return true;
            if (dict.Source?.OriginalString?.Contains("DarkTheme") == true)
                return true;
            if (dict.Source?.OriginalString?.Contains("LightTheme") == true)
                return false;
        }

        return true;
    }

    private static bool IsHighContrastActive()
    {
        return System.Windows.SystemParameters.HighContrast;
    }
}