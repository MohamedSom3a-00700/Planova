using System.Windows.Controls;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsSettingsView : UserControl
{
    public WbsSettingsView(WbsSettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
