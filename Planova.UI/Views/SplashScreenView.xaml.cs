using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Planova.UI.Views;

public partial class SplashScreenView : Window
{
    public SplashScreenView()
    {
        InitializeComponent();
        SetThemeImage();
        ContentGrid.SizeChanged += (_, _) => UpdateClip();
    }

    private void UpdateClip()
    {
        ContentGrid.Clip = new RectangleGeometry(
            new Rect(0, 0, ContentGrid.ActualWidth, ContentGrid.ActualHeight), 16, 16);
    }

    private void SetThemeImage()
    {
        try
        {
            var isLight = false;
            using (var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (key?.GetValue("AppsUseLightTheme") is int value && value == 1)
                    isLight = true;
            }

            var uri = isLight
                ? "pack://application:,,,/Resources/Branding/Splashscreen-light.png"
                : "pack://application:,,,/Resources/Branding/Splashscreen.png";

            var image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(uri);
            image.EndInit();
            image.Freeze();

            SplashImage.Source = image;
        }
        catch
        {
            // Default to dark image on error
        }
    }

    public void ReportProgress(int progress, string status)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => ReportProgress(progress, status));
            return;
        }

        ProgressBar.Value = progress;
        StatusText.Text = status;
    }
}
