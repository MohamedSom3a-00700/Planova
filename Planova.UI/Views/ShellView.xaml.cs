using System.Windows;
using Planova.UI.ViewModels;
using Wpf.Ui.Controls;

namespace Planova.UI.Views;

public partial class ShellView : FluentWindow
{
    private const double WordmarkCollapseWidth = 640;

    public ShellView(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        SizeChanged += OnShellViewSizeChanged;
        MinWidth = 800;
        MinHeight = 600;
    }

    private void OnShellViewSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (HeaderWordmark == null) return;
        HeaderWordmark.Visibility = e.NewSize.Width < WordmarkCollapseWidth
            ? Visibility.Collapsed
            : Visibility.Visible;
    }
}