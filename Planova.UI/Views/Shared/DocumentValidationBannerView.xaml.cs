using System.Windows.Controls;
using Planova.UI.ViewModels.Shared;

namespace Planova.UI.Views.Shared;

public partial class DocumentValidationBannerView : UserControl
{
    public DocumentValidationBannerView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DocumentValidationBannerViewModel vm)
            await vm.LoadCommand.ExecuteAsync(null);
    }
}
