using System.Globalization;
using System.Windows.Data;

namespace Planova.UI.Converters;

public class DocumentCategoryToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string category)
        {
            return category.ToLowerInvariant() switch
            {
                "boq" => "\uEA8C",
                "spec" => "\uE943",
                "contract" => "\uE8D5",
                "drawing" => "\uE7B8",
                "other" => "\uE8A5",
                _ => "\uE8A5"
            };
        }
        return "\uE8A5";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
