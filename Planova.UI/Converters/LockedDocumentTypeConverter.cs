using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Planova.Application.Services;

namespace Planova.UI.Converters;

public class LockedDocumentTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var docType = value as string;
        var isLocked = !string.IsNullOrEmpty(docType) && DocumentTypeRegistry.IsLockedType(docType);

        var invert = parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase);
        if (invert) isLocked = !isLocked;

        if (targetType == typeof(bool))
            return isLocked;

        return isLocked ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}