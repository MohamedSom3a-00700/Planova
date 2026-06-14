using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Planova.UI.Converters;

public class ComparisonColorConverter : IValueConverter
{
    private static readonly Brush AddedBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
    private static readonly Brush RemovedBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
    private static readonly Brush ModifiedBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
    private static readonly Brush UnchangedBrush = new SolidColorBrush(Color.FromRgb(158, 158, 158));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var changeType = value as string;

        return changeType switch
        {
            "Added" => AddedBrush,
            "Removed" => RemovedBrush,
            "Modified" => ModifiedBrush,
            "Unchanged" => UnchangedBrush,
            _ => UnchangedBrush
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
