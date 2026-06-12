using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Planova.UI.Converters;

public class ReportStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status switch
            {
                "Draft" => new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                "Final" => new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                "Archived" => new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                _ => new SolidColorBrush(Color.FromRgb(108, 117, 125))
            };
        }
        return new SolidColorBrush(Color.FromRgb(108, 117, 125));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
