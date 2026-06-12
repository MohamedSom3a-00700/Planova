using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Planova.UI.Converters;

public class FilePathToImageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path) && File.Exists(path))
        {
            var uri = new Uri(path, UriKind.Absolute);
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = uri;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            return img;
        }
        return null!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
