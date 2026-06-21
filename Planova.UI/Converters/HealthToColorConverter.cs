using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Planova.UI.Converters;

public class HealthToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string health)
        {
            return health.ToLowerInvariant() switch
            {
                "green" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                "yellow" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                "red" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))
            };
        }
        return new SolidColorBrush(Color.FromRgb(158, 158, 158));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
