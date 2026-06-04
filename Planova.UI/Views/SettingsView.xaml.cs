using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
