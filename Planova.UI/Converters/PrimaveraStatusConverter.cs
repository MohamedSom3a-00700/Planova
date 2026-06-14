using System.Globalization;
using System.Windows.Data;

namespace Planova.UI.Converters;

public class PrimaveraStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status switch
            {
                "Committed" => "Committed",
                "Previewing" => "Previewing",
                "Failed" => "Failed",
                "RolledBack" => "Rolled Back",
                _ => status
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
