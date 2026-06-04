using System.Globalization;
using System.Windows.Data;

namespace Planova.UI.Converters;

public class NavIconConverter : IValueConverter
{
    private static readonly Dictionary<string, string> IconMap = new()
    {
        ["dashboard"] = "\uE80F",
        ["projects"] = "\uE709",
        ["boq"] = "\uE943",
        ["wbs"] = "\uE8A1",
        ["activity"] = "\uE823",
        ["resource"] = "\uE716",
        ["cost"] = "\uE9D9",
        ["reports"] = "\uE9B9",
        ["primavera"] = "\uEDA2",
        ["schedule-compare"] = "\uE895",
        ["delay-analysis"] = "\uE9D4",
        ["claims"] = "\uE9C8",
        ["chronology"] = "\uE97A",
        ["correspondence"] = "\uE715",
        ["knowledge-base"] = "\uE8F2",
        ["analytics"] = "\uE9D5",
        ["integration-hub"] = "\uEB60",
        ["clients"] = "\uE716",
        ["settings"] = "\uE713"
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string id && IconMap.TryGetValue(id, out var glyph))
            return glyph;
        return "\uE10B";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}