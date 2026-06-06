using System.Globalization;
using System.Windows.Data;

namespace Planova.UI.Converters;

public class CostValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            if (parameter is string format)
                return decimalValue.ToString(format, culture);
            return decimalValue.ToString("N2", culture);
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && decimal.TryParse(str, NumberStyles.Any, culture, out var result))
            return result;
        return 0m;
    }
}
