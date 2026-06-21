using System.Globalization;
using System.Windows.Data;

namespace Planova.UI.Converters;

public class WbsStatusToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLowerInvariant() switch
            {
                "draft" => "\uE7C0",
                "underreview" => "\uE7FB",
                "approved" => "\uE73E",
                "archived" => "\uE77B",
                _ => "\uE7C0"
            };
        }
        return "\uE7C0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
