using System.Globalization;
using System.Windows.Data;

namespace Planova.UI.Converters;

public class CollapseIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "PanelLeftExpand24" : "PanelLeftContract24";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
