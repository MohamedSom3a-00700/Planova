using System.Globalization;
using System.Windows.Data;

namespace Planova.UI.Converters;

public class PartyRoleToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var role = value as string;
        return role switch
        {
            "Client" => "Contact24",
            "MainContractor" => "Contractor24",
            "SubContractor" => "People24",
            "Consultant" => "Assistant24",
            _ => "People24"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
