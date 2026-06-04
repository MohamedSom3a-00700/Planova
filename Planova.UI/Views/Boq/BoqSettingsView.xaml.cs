using System.Windows.Controls;
using Planova.UI.ViewModels.Boq;

namespace Planova.UI.Views.Boq;

public partial class BoqSettingsView : UserControl
{
    public BoqSettingsView(BoqSettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
