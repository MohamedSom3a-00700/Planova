using System.Globalization;
using System.Windows.Data;

namespace Planova.UI.Converters;

public class IndexToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index && parameter is string target && int.TryParse(target, out var targetIndex))
            return index == targetIndex;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is string target && int.TryParse(target, out var targetIndex))
            return targetIndex;
        return Binding.DoNothing;
    }
}
