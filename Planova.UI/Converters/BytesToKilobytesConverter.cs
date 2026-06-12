using System.Globalization;
using System.Windows.Data;

namespace Planova.UI.Converters;

public class BytesToKilobytesConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long bytes)
            return Math.Round(bytes / 1024.0);
        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
