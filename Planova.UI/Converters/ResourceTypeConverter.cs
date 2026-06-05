using System.Globalization;
using System.Windows.Data;
using Planova.Resource.Domain.Enums;

namespace Planova.UI.Converters;

public class ResourceTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ResourceType type)
        {
            return type switch
            {
                ResourceType.Labour => "Labour",
                ResourceType.Equipment => "Equipment",
                ResourceType.Material => "Material",
                ResourceType.Subcontractor => "Subcontractor",
                _ => type.ToString()
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
