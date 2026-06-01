using System.Windows;
using Planova.UI.ViewModels;

namespace Planova.UI.Views;

public partial class ShellView : Window
{
    public ShellView(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
